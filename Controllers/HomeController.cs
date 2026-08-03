using FoodOne.Data;
using FoodOne.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace FoodOne.Controllers
{
    public class HomeController : Controller
    {
        private readonly FoodAppDbContext _dbContext;
        public HomeController(FoodAppDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        public IActionResult Index()
        {
            ViewData["pageType"] = "Home";
            var categories = _dbContext.Categories.Where(c => c.Status == 1).ToList();

            return View(categories);
        }

        public IActionResult Privacy()
        {
            ViewData["pageType"] = "Home";
            return View();
        }

        public IActionResult Blog(int page = 1) {

            ViewData["pageType"] = "Blog";
            ViewBag.Categories = _dbContext.Categories.Where(c => c.Status == 1).ToList();

            int pageSize = 3;
            // Main blogs with pagination
            var totalBlogs = _dbContext.Blogs
                                       .Where(b => b.Status == 1)
                                       .Count();
            ViewBag.Blogs = _dbContext.Blogs
                              .Include(b => b.Category)
                              .Where(b => b.Status == 1)
                              .OrderByDescending(b => b.CreatedAt)
                              .Skip((page - 1) * pageSize)
                              .Take(pageSize)
                              .ToList();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalBlogs / pageSize);

            // Recent blogs (sidebar)
            ViewBag.RecentBlogs = _dbContext.Blogs
                                            .Include(b => b.Category)
                                            .Where(b => b.Status == 1)
                                            .OrderByDescending(b => b.CreatedAt)
                                            .Take(30)
                                            .ToList();

            return View();
        }

        public IActionResult About()
        {
            ViewData["pageType"] = "About-us";
            return View();
        }

        public IActionResult Contact()
        {
            ViewData["pageType"] = "contact-us";
            return View();
        }

        [HttpPost]
        public IActionResult ContactCreate(Contact contact)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return Json(new
                    {
                        success = false,
                        message = "Please fill all required fields."
                    });
                }

                contact.CreatedAt = DateTime.Now;
                contact.UpdatedAt = DateTime.Now;

                _dbContext.Contacts.Add(contact);
                _dbContext.SaveChanges();

                return Json(new
                {
                    success = true,
                    message = "Message sent successfully."
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
