using FoodOne.Data;
using Microsoft.AspNetCore.Mvc;

namespace FoodOne.Controllers
{
    [Area("Admin")]
    public class BaseAdminController : Controller
    {
        protected ViewResult AdminView(string viewName)
        {
            string controllerName = RouteData.Values["controller"]!.ToString()!;

            return View($"~/Views/Admin/{controllerName}/{viewName}.cshtml");
        }

        // Overload to allow passing a model to the admin view: return AdminView("Index", vm);
        protected ViewResult AdminView<TModel>(string viewName, TModel model)
        {
            string controllerName = RouteData.Values["controller"]!.ToString()!;
            return View($"~/Views/Admin/{controllerName}/{viewName}.cshtml", model);
        }
    }
}
