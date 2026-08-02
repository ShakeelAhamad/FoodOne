using FoodOne.Data;
using FoodOne.Helpers;
using FoodOne.Models;
using FoodOne.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authorization;

namespace FoodOne.Controllers.Admin
{
    [Authorize]
    [Route("admin/item")]
    public class ItemController : BaseAdminController
    {
        private readonly FoodAppDbContext _dbContext;
        private readonly IWebHostEnvironment _env;
        public ItemController(FoodAppDbContext dbContext, IWebHostEnvironment env)
        {
            _dbContext = dbContext;
            _env = env;
        }

        [Route("")]
        public IActionResult Index()
        {
            ViewBag.Title = "Item";
            return AdminView("Index");
        }
        [HttpPost]
        [Route("list")]
        public async Task<JsonResult> List()
        {
            var form           = Request.Form;
            var draw           = form["draw"].FirstOrDefault();
            var start          = Convert.ToInt32(form["start"].FirstOrDefault() ?? "0");
            var length         = Convert.ToInt32(form["length"].FirstOrDefault() ?? "10");
            var search         = form["search[value]"].FirstOrDefault()?.Trim();
            var orderColIndex  = form["order[0][column]"].FirstOrDefault();
            var orderDir       = form["order[0][dir]"].FirstOrDefault() ?? "asc";
            var orderColumn    = orderColIndex != null ? form[$"columns[{orderColIndex}][data]"].FirstOrDefault() : "ItemName";

            var query = _dbContext.Items.Include(i => i.Category).AsNoTracking();
            // Global search: name, detail, category name and numeric price (case-insensitive)
            if (!string.IsNullOrEmpty(search))
            {
                search = search.Trim();
                var searchLower = search.ToLower();
                // try parse numeric search for price comparisons
                bool isDecimal = decimal.TryParse(search, out var priceVal);

                query = query.Where(i =>
                    (i.ItemName != null && EF.Functions.Like(i.ItemName.ToLower(), $"%{searchLower}%"))
                    || (i.ItemDetail != null && EF.Functions.Like(i.ItemDetail.ToLower(), $"%{searchLower}%"))
                    || (i.Category != null && i.Category.CategoryName != null && EF.Functions.Like(i.Category.CategoryName.ToLower(), $"%{searchLower}%"))
                    || (isDecimal && i.Price == priceVal)
                );
            }

            var recordsTotal = await _dbContext.Items.CountAsync();
            var recordsFiltered = await query.CountAsync();

            var allowedColumns = new[] { "Id", "itemName", "price", "status", "categoryName" };
            if (!allowedColumns.Contains(orderColumn)) orderColumn = "ItemName";

            // Explicit ordering to avoid EF.Property issues and support navigation property
            var dirDesc = string.Equals(orderDir, "desc", StringComparison.OrdinalIgnoreCase);
            switch (orderColumn)
            {
                case "ItemName":
                    query = dirDesc ? query.OrderByDescending(i => i.ItemName) : query.OrderBy(i => i.ItemName);
                    break;
                case "Price":
                    query = dirDesc ? query.OrderByDescending(i => i.Price) : query.OrderBy(i => i.Price);
                    break;
                case "CategoryName":
                    query = dirDesc
                        ? query.OrderByDescending(i => i.Category != null ? i.Category.CategoryName : string.Empty)
                        : query.OrderBy(i => i.Category != null ? i.Category.CategoryName : string.Empty);
                    break;
                case "Status":
                    query = dirDesc ? query.OrderByDescending(i => i.Status) : query.OrderBy(i => i.Status);
                    break;
                default:
                    query = dirDesc ? query.OrderByDescending(i => i.Id) : query.OrderBy(i => i.Id);
                    break;
            }

            var data = await query.Skip(start).Take(length).Select(i => new
            {
                i.Id,
                i.ItemName,
                i.CategoryId,
                i.ItemDetail,
                CategoryName = i.Category != null ? i.Category.CategoryName : string.Empty,
                Price = i.Price,
                ImageUrl = string.IsNullOrEmpty(i.Image) ? string.Empty : ("/uploads/items/" + i.Image),
                statusStr = i.Status == 1 ? "Active" : "Inactive",
                i.Status
            }).ToListAsync();

            return Json(new
            {
                draw = draw,
                recordsTotal = recordsTotal,
                recordsFiltered = recordsFiltered,
                data = data
            });
        }

        [HttpPost]
        [Route("delete")]
        public async Task<JsonResult> Delete(int id)
        {
            var item = await _dbContext.Items.FindAsync(id);
            if (item == null) return Json(new { success = false, message = "Item not found." });

            if (!string.IsNullOrEmpty(item.Image))
            {
                FileUploadHelper.DeleteFile(item.Image, "items");
            }
            _dbContext.Items.Remove(item);
            await _dbContext.SaveChangesAsync();
            return Json(new { success = true, message = "Item deleted successfully." });
        }
        [HttpPost]
        [Route("add")]
        public async Task<JsonResult> Add(ItemViewModel model) {
            // Server-side validation
            if (!ModelState.IsValid)
            {
                foreach (var items in ModelState)
                {
                    foreach (var error in items.Value.Errors)
                    {
                        Console.WriteLine($"{items.Key} : {error.ErrorMessage}");
                    }
                }
                var errors = ModelState
                    .Where(x => x.Value.Errors.Any())
                    .ToDictionary(
                        x => x.Key,
                        x => x.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                    );


                return Json(new
                {
                    success = false,
                    errors
                });
            }

            // Save the item to the database

            Item item;
            if(model.Id > 0)
            {
                item = await _dbContext.Items.FindAsync(model.Id);
                if (item == null)
                {
                    return Json(new
                    {
                        success = false,
                        message = "Item not found."
                    });
                }
            }
            else
            {
                item = new Item()
                {
                    CreatedAt = DateTime.Now
                };
                _dbContext.Items.Add(item);
            }


            // Upload image if selected
            if (model.Image != null)
            {
                // Delete old image while updating
                if (model.Id > 0 && !string.IsNullOrEmpty(item.Image))
                {
                    FileUploadHelper.DeleteFile(item.Image,"items");
                }
                item.Image = await FileUploadHelper.UploadFile(model.Image, "items");
            }

            // Common fields
            item.ItemName   = model.ItemName;
            item.ItemDetail = model.ItemDetail;
            item.CategoryId = model.CategoryId ?? 0;
            item.Price      = model.Price ?? 0;
            item.Status     = model.Status;
            item.UpdatedAt  = DateTime.Now;
            await _dbContext.SaveChangesAsync();
            return Json(new
            {
                success = true,
                message = model.Id > 0
                    ? "Item updated successfully."
                    : "Item added successfully."
            });
        }


    }
}
