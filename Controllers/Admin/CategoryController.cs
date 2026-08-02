using FoodOne.Data;
using FoodOne.Models;
using FoodOne.Helpers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FoodOne.ViewModels;
using Microsoft.AspNetCore.Authorization;

namespace FoodOne.Controllers.Admin
{
    [Authorize]
    [Route("admin/category")]
    public class CategoryController : BaseAdminController
    {
        private readonly FoodAppDbContext _dbContext;
        private readonly IWebHostEnvironment _env;

        public CategoryController(FoodAppDbContext dbContext, IWebHostEnvironment env)
        {
            _dbContext = dbContext;
            _env = env;
        }


        [Route("")]
        public IActionResult Index()
        {
            ViewBag.Title = "Category";
            return AdminView("Index");
        }

        [HttpPost]
        [Route("list")]
        public async Task<JsonResult> List()
        {
            var form   = Request.Form;
            var draw   = form["draw"].FirstOrDefault();
            var start  = Convert.ToInt32(form["start"].FirstOrDefault() ?? "0");
            var length = Convert.ToInt32(form["length"].FirstOrDefault() ?? "10");
            var search = form["search[value]"].FirstOrDefault()?.Trim();

            // Determine ordering
            var orderColIndex  = form["order[0][column]"].FirstOrDefault();
            var orderDir       = form["order[0][dir]"].FirstOrDefault() ?? "asc";
            var orderColumn    = orderColIndex != null ? form[$"columns[{orderColIndex}][data]"].FirstOrDefault() : "CategoryName";

            // Base query
            var query = _dbContext.Categories.AsNoTracking();

            // Global search
            if (!string.IsNullOrEmpty(search))
            {
                search = search.Trim();
                query = query.Where(c =>
                    c.CategoryName != null &&
                    c.CategoryName.Contains(search)
                );
            }

            var recordsTotal = await _dbContext.Categories.CountAsync();
            var recordsFiltered = await query.CountAsync();

            // Safe ordering: allow only known columns to avoid SQL injection
            var allowedColumns = new[] { "Id", "CategoryName", "Status" };
            if (!allowedColumns.Contains(orderColumn)) orderColumn = "CategoryName";

            // Apply ordering dynamically using EF.Property
            if (orderDir.ToLower() == "desc")
                query = query.OrderByDescending(e => EF.Property<object>(e, orderColumn));
            else
                query = query.OrderBy(e => EF.Property<object>(e, orderColumn));

            var data = await query
                .Skip(start)
                .Take(length)
                .Select(c => new {
                    c.Id,
                    c.CategoryName,
                    CategoryImageUrl = string.IsNullOrEmpty(c.CategoryImage) ? string.Empty : ("/uploads/category/" + c.CategoryImage),
                    statusStr = c.Status == 1 ? "Active" : "Inactive",
                    c.Status
                })
                .ToListAsync();

            return Json(new {
                draw = draw,
                recordsTotal = recordsTotal,
                recordsFiltered = recordsFiltered,
                data = data
            });
        }

        [HttpPost]
        [Route("add")]
        public async Task<JsonResult> Add(CategoryViewModel model)
        {
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

            Category category;

            // UPDATE
            if (model.Id > 0)
            {
                category = await _dbContext.Categories.FindAsync(model.Id);

                if (category == null)
                {
                    return Json(new
                    {
                        success = false,
                        message = "Category not found."
                    });
                }
            }
            // ADD
            else
            {
                category = new Category()
                {
                    CreatedAt = DateTime.Now
                };

                _dbContext.Categories.Add(category);
            }

            // Upload image if selected
            if (model.CategoryImage != null)
            {
                // Delete old image while updating
                if (model.Id > 0 &&
                    !string.IsNullOrEmpty(category.CategoryImage))
                {
                    FileUploadHelper.DeleteFile(
                        category.CategoryImage,
                        "category");
                }

                category.CategoryImage = await FileUploadHelper.UploadFile(model.CategoryImage, "category");
            }

            // Common fields
            category.CategoryName = model.CategoryName;
            category.Status = model.Status;
            category.UpdatedAt = DateTime.Now;

            await _dbContext.SaveChangesAsync();

            return Json(new
            {
                success = true,
                message = model.Id > 0
                    ? "Category updated successfully."
                    : "Category added successfully."
            });
        }

        [HttpPost]
        [Route("delete")]
        public async Task<JsonResult> Delete(int id)
        {
            var category = await _dbContext.Categories.FindAsync(id);

            if (category == null)
            {
                return Json(new
                {
                    success = false,
                    message = "Category not found."
                });
            }

            // Delete image if exists
            if (!string.IsNullOrEmpty(category.CategoryImage))
            {
                FileUploadHelper.DeleteFile(
                    category.CategoryImage,
                    "category");
            }
            // Delete record
            _dbContext.Categories.Remove(category);
            await _dbContext.SaveChangesAsync();
            return Json(new
            {
                success = true,
                message = "Category deleted successfully."
            });
        }

        [HttpGet]
        [Route("categoryList")]
        public async Task<JsonResult> CategoryList() {
            var categories = await _dbContext.Categories
                   .Where(c => c.Status == 1)
                   .Select(c => new
                   {
                       id = c.Id,
                       name = c.CategoryName
                   })
                   .ToListAsync();
                return Json(categories);
        }



    }
}
