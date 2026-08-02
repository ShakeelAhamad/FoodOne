using System.ComponentModel.DataAnnotations;

namespace FoodOne.ViewModels
{
    public class ChangePasswordViewModel
    {
        public int Id { get; set; }
        
        [DataType(DataType.Password)]
        [Required(ErrorMessage = "Current password is required.")]
        public string? CurrentPassword { get; set; }

        [DataType(DataType.Password)]
        [Required(ErrorMessage = "New password is required.")]
        public string? NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "The password and confirmation password do not match.")]
        [Required(ErrorMessage = "Confirm password is required.")]
        public string? ConfirmPassword { get; set; }
    } 
}
