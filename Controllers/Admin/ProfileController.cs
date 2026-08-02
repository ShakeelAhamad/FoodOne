using FoodOne.Data;
using FoodOne.Helpers;
using FoodOne.Models;
using FoodOne.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace FoodOne.Controllers.Admin
{
    [Authorize]
    [Route("admin/profile")]
    public class ProfileController : BaseAdminController
    {
        private readonly FoodAppDbContext _db;

        public ProfileController(FoodAppDbContext db)
        {
            _db = db;
        }

        [Route("")]
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userId, out var id)) return RedirectToAction("Index", "Login", new { area = "Admin" });

            var user = await _db.AdminUsers.FirstOrDefaultAsync(u => u.Id == id);
            if (user == null) return RedirectToAction("Index", "Login", new { area = "Admin" });

            var vm = new AdminProfileViewModel
            {
                Id = user.Id,
                FullName = user.FullName,
                ImageUrl = string.IsNullOrEmpty(user.ProfileImage) ? string.Empty : "/uploads/admin/" + user.ProfileImage
            };
            return AdminView("Index", vm);

            static string userProfileImage(AdminUser u)
            {
                return string.Empty;
            }
        }

        [HttpPost]
        [Route("update")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(AdminProfileViewModel model)
        {
            if (!ModelState.IsValid) return AdminView("Index", model);

            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userId, out var id)) return RedirectToAction("Index", "Login", new { area = "Admin" });

            var user = await _db.AdminUsers.FirstOrDefaultAsync(u => u.Id == id);
            if (user == null) return RedirectToAction("Index", "Login", new { area = "Admin" });

            user.FullName = model.FullName;
            user.UpdatedAt = DateTime.UtcNow;

            if (model.ImageFile != null && model.ImageFile.Length > 0)
            {
                var fileName = await FileUploadHelper.UploadFile(model.ImageFile, "admin");
                // Delete old image if exists
                if (!string.IsNullOrEmpty(user.ProfileImage))
                {
                    FileUploadHelper.DeleteFile(user.ProfileImage, "admin");
                }
                user.ProfileImage = fileName;
            }
            await _db.SaveChangesAsync();

            // update the name claim by signing in again
            var claims = new List<System.Security.Claims.Claim>
            {
                new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, user.Id.ToString()),
                new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Name, user.FullName ?? string.Empty),
                new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Email, user.Email ?? string.Empty)
            };
            var identity = new System.Security.Claims.ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new System.Security.Claims.ClaimsPrincipal(identity));
            return RedirectToAction("Index");
        }

        [HttpGet]
        [Route("change-password")]
        public IActionResult ChangePassword()
        {
            return AdminView("ChangePassword");
        }
        [HttpPost]
        [Route("update-password")]
        public async Task<JsonResult> UpdatePassword(ChangePasswordViewModel model)
        {
            var errors = ModelState.Where(x => x.Value.Errors.Any()).ToDictionary(x => x.Key, x => x.Value.Errors.Select(e => e.ErrorMessage).ToArray());
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, errors });
            }

            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userId, out var id))
            {
                return Json(new
                {
                    success = false,
                    message = "User not found."
                });
            }
            var user = await _db.AdminUsers.FindAsync(id);
            if (user == null)
            {
                return Json(new
                {
                    success = false,
                    message = "User not found."
                });
            }
            

            // If password change requested, validate and update
            if (!string.IsNullOrEmpty(model.NewPassword))
            {
                var hasher        = new Microsoft.AspNetCore.Identity.PasswordHasher<AdminUser>();
                //If Check Current Password is match or not
                var result = hasher.VerifyHashedPassword(user, user.PasswordHash, model.CurrentPassword);
                if (result == PasswordVerificationResult.Failed)
                {
                    ModelState.AddModelError("CurrentPassword", "Current password is incorrect.");
                    //var errors = ModelState.Where(x => x.Value.Errors.Any()).ToDictionary(x => x.Key, x => x.Value.Errors.Select(e => e.ErrorMessage).ToArray());
                    return Json(new { success = false, errors });
                }
                user.PasswordHash = hasher.HashPassword(user, model.NewPassword);
            }
            await _db.SaveChangesAsync();

            return Json(new { 
                success = true, message = "Password updated successfully.",
                redirectUrl = Url.Action("Logout", "Login", new { area = "Admin" })
            });
        }
    }
}
