using System.ComponentModel.DataAnnotations;

namespace FoodOne.ViewModels
{
    public class ResetPasswordViewModel
    {
        [Required(ErrorMessage = "Token is required.")]
        public string? Token { get; set; }
        public bool IsTokenValid { get; set; } = true;
        public string? ErrorMessage { get; set; }
        [Required(ErrorMessage = "New password is required.")]
        [DataType(DataType.Password)]
        public string? NewPassword { get; set; }
        [DataType(DataType.Password)]
        [Required(ErrorMessage = "Confirm password is required.")]
        [Compare("NewPassword", ErrorMessage = "The password and confirmation password do not match.")]
        public string? ConfirmPassword { get; set; }
    }
}
