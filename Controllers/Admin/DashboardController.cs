using FoodOne.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace FoodOne.Controllers.Admin
{
    [Authorize]
    [Route("admin/dashboard")]
    public class DashboardController : BaseAdminController
    {

        [Route("")]
        public IActionResult Index()
        {
            ViewBag.Title = "Dashboard";
            return AdminView("Index");
        }
    }
}
