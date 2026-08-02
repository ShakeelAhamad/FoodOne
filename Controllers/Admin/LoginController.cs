using FoodOne.Data;
using FoodOne.Models;
using FoodOne.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Net.Mail;
using System.Security.Claims;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace FoodOne.Controllers.Admin
{
    [AllowAnonymous]
    [Route("admin")]
    public class LoginController : BaseAdminController
    {
        private readonly FoodAppDbContext _dbContext;

        public LoginController(FoodAppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [Route("")]
        [AllowAnonymous]
        public IActionResult Index()
        {
            // if already authenticated, go to dashboard
            if (User?.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Dashboard", new { area = "Admin" });
            }

            return AdminView("Index");
        }

        [HttpPost]
        [Route("loginaction")]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> LoginAction(AdminLoginViewModel model)
        {
            var errors = ModelState.Where(x => x.Value.Errors.Any()).ToDictionary(x => x.Key, x => x.Value.Errors.Select(e => e.ErrorMessage).ToArray());
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, errors });
            }

            //Check if the user exists and is active
            var user = await _dbContext.AdminUsers.FirstOrDefaultAsync(u => u.Email == model.Email && u.Status == 1);
            if (user == null)
            {
                ModelState.AddModelError("Email", "Email is incorrect.");
                errors = ModelState.Where(x => x.Value.Errors.Any()).ToDictionary(x => x.Key, x => x.Value.Errors.Select(e => e.ErrorMessage).ToArray());
                return Json(new { success = false, errors });
            }

            //Check if the password is correct
            var hasher = new PasswordHasher<AdminUser>();
            var result = hasher.VerifyHashedPassword(user, user.PasswordHash, model.Password);
            if (result == PasswordVerificationResult.Failed)
            {
                ModelState.AddModelError("Password", "Password is incorrect.");
                errors = ModelState.Where(x => x.Value.Errors.Any()).ToDictionary(x => x.Key, x => x.Value.Errors.Select(e => e.ErrorMessage).ToArray());
                return Json(new { success = false, errors });
            }

            // Create claims and sign-in
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.FullName ?? string.Empty),
                new Claim(ClaimTypes.Email, user.Email)
            };

            var identity   = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal  = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, new AuthenticationProperties
            {
                IsPersistent = model.RememberMe
            });
            return Json(new
            {
                success = true,
                message = "Login successfully.",
                redirectUrl = Url.Action("Index", "Dashboard", new { area = "Admin" })
            });
        }


        [Route("logout")]
        [AllowAnonymous]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index");
        }

        [HttpGet]
        [AllowAnonymous]
        [Route("forgot-password")]
        public IActionResult ForgotPassword()
        {
            return AdminView("ForgotPassword");
        }

        [HttpPost]
        [AllowAnonymous]
        [Route("forgot-password")]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> ForgotPasswordPost(string email)
        {
            var errors = ModelState.Where(x => x.Value.Errors.Any()).ToDictionary(x => x.Key, x => x.Value.Errors.Select(e => e.ErrorMessage).ToArray());
            if (string.IsNullOrEmpty(email))
            {
                return Json(new { success = false, errors = ModelState.Where(x => x.Value.Errors.Any()).ToDictionary(x => x.Key, x => x.Value.Errors.Select(e => e.ErrorMessage).ToArray()) });
            }

            var user = await _dbContext.AdminUsers.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
            {
                ModelState.AddModelError("email", "No account was found with this email address.");
                errors = ModelState.Where(x => x.Value.Errors.Any()).ToDictionary(x => x.Key, x => x.Value.Errors.Select(e => e.ErrorMessage).ToArray());
                // do not reveal user absence
                return Json(new { success = false, errors});
            }

            // generate token
            var token = Guid.NewGuid().ToString("N") + "" + Guid.NewGuid().ToString("N");
            user.PasswordResetToken = token;
            user.PasswordResetExpires = DateTime.UtcNow.AddHours(1);
            await _dbContext.SaveChangesAsync();

            // send email via Gmail SMTP
            var resetUrl = Url.Action("ResetPassword", "Login", new { area = "Admin", token = token }, Request.Scheme);
            try
            {
                await SendResetEmail(user.Email, resetUrl);
            }
            catch
            {
                // log or ignore for now
            }

            return Json(new
            {
                success = true,
                message = "Password reset link has been sent to your email.",
            });
        }

            [HttpGet]
        [AllowAnonymous]
        [Route("reset-password")]
        public async Task<IActionResult> ResetPassword(string token)
        {
            var model = new ResetPasswordViewModel();

            if (string.IsNullOrEmpty(token))
            {
                model.IsTokenValid = false;
                model.ErrorMessage = "Password reset link is missing.";
                return View("~/Views/Admin/Login/ResetPassword.cshtml", model);
            }

            var user = await _dbContext.AdminUsers.FirstOrDefaultAsync(x =>
                x.PasswordResetToken == token);

            if (user == null)
            {
                model.IsTokenValid = false;
                model.ErrorMessage = "Invalid password reset link.";
                return View("~/Views/Admin/Login/ResetPassword.cshtml", model);
            }

            if (user.PasswordResetExpires == null ||
                user.PasswordResetExpires <= DateTime.UtcNow)
            {
                model.IsTokenValid = false;
                model.ErrorMessage = "This password reset link has expired. Please request a new password reset email.";
                return View("~/Views/Admin/Login/ResetPassword.cshtml", model);
            }

            model.Token = token;
            return View("~/Views/Admin/Login/ResetPassword.cshtml", model);
        }

        [HttpPost]
        [AllowAnonymous]
        [Route("reset-password")]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> ResetPasswordPost(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Where(x => x.Value.Errors.Any()).ToDictionary(x => x.Key, x => x.Value.Errors.Select(e => e.ErrorMessage).ToArray());
                return Json(new { success = false, errors });
            }
            var user = await _dbContext.AdminUsers.FirstOrDefaultAsync(u => u.PasswordResetToken == model.Token && u.PasswordResetExpires > DateTime.UtcNow);
            if (user == null)
            {
                return Json(new { success = false, message = "Invalid password reset link." });
            }

            var hasher                 = new PasswordHasher<AdminUser>();
            user.PasswordHash          = hasher.HashPassword(user, model.NewPassword);
            user.PasswordResetToken    = null;
            user.PasswordResetExpires  = null;
            await _dbContext.SaveChangesAsync();

            return Json(new
            {
                success = true,
                message = "Your password has been reset successfully. Please log in with your new password.",
                redirectUrl = Url.Action("Index", "Login", new { area = "Admin" })
            });
        }
        private async Task SendResetEmail(string toEmail, string resetUrl)
        {
            try
            {
                var config = HttpContext.RequestServices.GetRequiredService<IConfiguration>();

                var gmailUser = config["SmtpSettings:UserName"];
                var gmailPass = config["SmtpSettings:Password"];

                MailMessage message = new MailMessage();

                message.From = new MailAddress(gmailUser);
                message.To.Add(toEmail);
                message.Subject = "Password Reset";

                message.Body = $@"
                        <!DOCTYPE html>
                    <html>
                    <head>
                        <meta charset='UTF-8'>
                    </head>
                    <body style='font-family: Arial, Helvetica, sans-serif; color:#333; line-height:1.6;'>

                        <h2 style='color:#0d6efd;'>Password Reset Request</h2>

                        <p>
                            We received a request to reset the password for your account. If you made this request,
                            please click the button below to create a new password. For your security, this password
                            reset link is valid for a limited time and can only be used once.
                        </p>

                        <p>
                            If you did not request a password reset, you can safely ignore this email. Your account
                            will remain secure, and no changes will be made unless you use the link below. We recommend
                            contacting our support team if you believe someone attempted to access your account without
                            your permission.
                        </p>

                        <p style='margin:30px 0;'>
                            <a href='{resetUrl}'
                               target='_blank'
                               style='background:#0d6efd;
                                      color:#ffffff;
                                      padding:12px 24px;
                                      text-decoration:none;
                                      border-radius:5px;
                                      display:inline-block;'>
                                Reset Your Password
                            </a>
                        </p>

                        <p>
                            If the button above does not work, copy and paste the following link into your web browser:
                        </p>

                        <p>
                            <a href='{resetUrl}' target='_blank'>{resetUrl}</a>
                        </p>

                        <hr>

                        <p style='font-size:13px; color:#777;'>
                            This is an automated email. Please do not reply to this message.
                            Thank you for using our services.
                        </p>

                    </body>
                    </html>
            ";

                message.IsBodyHtml = true;

                using var smtp = new SmtpClient("smtp.gmail.com", 587);

                smtp.EnableSsl = true;
                smtp.UseDefaultCredentials = false;
                smtp.Credentials = new NetworkCredential(gmailUser, gmailPass);

                await smtp.SendMailAsync(message);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}
