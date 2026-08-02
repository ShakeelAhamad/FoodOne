using FoodOne.Data;
using FoodOne.ViewModels;
using FoodOne.Models;
using FoodOne.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace FoodOne.Controllers.Admin
{
    [Authorize]
    [Route("admin/blog")]
    public class BlogController : BaseAdminController
    {
        private readonly FoodAppDbContext _dbContext;
        private readonly IWebHostEnvironment _env;

        public BlogController(FoodAppDbContext dbContext, IWebHostEnvironment env)
        {
            _dbContext = dbContext;
            _env = env;
        }

        [Route("")]
        public IActionResult Index()
        {
            ViewBag.Title = "Blog";
            return AdminView("Index");
        }

        [HttpPost]
        [Route("list")]
        public async Task<JsonResult> List()
        {
            var form = Request.Form;
            var draw = form["draw"].FirstOrDefault();
            var start = Convert.ToInt32(form["start"].FirstOrDefault() ?? "0");
            var length = Convert.ToInt32(form["length"].FirstOrDefault() ?? "10");
            var search = form["search[value]"].FirstOrDefault()?.Trim();

            var query = _dbContext.Blogs.Include(b => b.Category).AsNoTracking();

            if (!string.IsNullOrEmpty(search))
            {
                var s = search.Trim().ToLower();
                query = query.Where(b =>
                    (b.Title != null && EF.Functions.Like(b.Title.ToLower(), $"%{s}%"))
                    || (b.Description != null && EF.Functions.Like(b.Description.ToLower(), $"%{s}%"))
                    || (b.Category != null && b.Category.CategoryName != null && EF.Functions.Like(b.Category.CategoryName.ToLower(), $"%{s}%"))
                );
            }

            var recordsTotal = await _dbContext.Blogs.CountAsync();
            var recordsFiltered = await query.CountAsync();

            var orderColIndex = form["order[0][column]"].FirstOrDefault();
            var orderDir = form["order[0][dir]"].FirstOrDefault() ?? "asc";
            var orderColumn = orderColIndex != null ? form[$"columns[{orderColIndex}][data]"].FirstOrDefault() : "Title";
            var dirDesc = string.Equals(orderDir, "desc", StringComparison.OrdinalIgnoreCase);
            switch (orderColumn)
            {
                case "Title":
                    query = dirDesc ? query.OrderByDescending(b => b.Title) : query.OrderBy(b => b.Title);
                    break;
                case "CategoryName":
                    query = dirDesc ? query.OrderByDescending(b => b.Category != null ? b.Category.CategoryName : string.Empty) : query.OrderBy(b => b.Category != null ? b.Category.CategoryName : string.Empty);
                    break;
                case "Status":
                    query = dirDesc ? query.OrderByDescending(b => b.Status) : query.OrderBy(b => b.Status);
                    break;
                default:
                    query = dirDesc ? query.OrderByDescending(b => b.Id) : query.OrderBy(b => b.Id);
                    break;
            }

            var data = await query.Skip(start).Take(length).Select(b => new {
                b.Id,
                b.Title,
                b.Description,
                b.CategoryId,
                CategoryName = b.Category != null ? b.Category.CategoryName : string.Empty,
                ImageUrl = string.IsNullOrEmpty(b.Image) ? string.Empty : ("/uploads/blog/" + b.Image),
                b.Status
            }).ToListAsync();

            return Json(new { draw = draw, recordsTotal = recordsTotal, recordsFiltered = recordsFiltered, data = data });
        }

        [HttpPost]
        [Route("add")]
        public async Task<JsonResult> Add(BlogViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Where(x => x.Value.Errors.Any()).ToDictionary(x => x.Key, x => x.Value.Errors.Select(e => e.ErrorMessage).ToArray());
                return Json(new { success = false, errors });
            }

            Blog blog;
            if (model.Id > 0)
            {
                blog = await _dbContext.Blogs.FindAsync(model.Id);
                if (blog == null) return Json(new { success = false, message = "Blog not found." });
            }
            else
            {
                blog = new Blog { CreatedAt = DateTime.Now };
                _dbContext.Blogs.Add(blog);
            }

            if (model.Image != null)
            {
                if (model.Id > 0 && !string.IsNullOrEmpty(blog.Image)) FileUploadHelper.DeleteFile(blog.Image, "blog");
                blog.Image = await FileUploadHelper.UploadFile(model.Image, "blog");
            }

            blog.Title = model.Title;
            blog.CategoryId = model.CategoryId ?? 0;
            blog.Description = model.Description;
            blog.Status = model.Status;
            blog.UpdatedAt = DateTime.Now;

            await _dbContext.SaveChangesAsync();
            return Json(new { success = true, message = model.Id > 0 ? "Blog updated successfully." : "Blog added successfully." });
        }

        [HttpPost]
        [Route("delete")]
        public async Task<JsonResult> Delete(int id)
        {
            var blog = await _dbContext.Blogs.FindAsync(id);
            if (blog == null) return Json(new { success = false, message = "Blog not found." });
            if (!string.IsNullOrEmpty(blog.Image)) FileUploadHelper.DeleteFile(blog.Image, "blog");
            _dbContext.Blogs.Remove(blog);
            await _dbContext.SaveChangesAsync();
            return Json(new { success = true, message = "Blog deleted successfully." });
        }
    }
}
