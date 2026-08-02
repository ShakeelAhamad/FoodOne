using System.ComponentModel.DataAnnotations;

namespace FoodOne.ViewModels
{
    public class CategoryViewModel
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "Category name is required.")]
        public string CategoryName { get; set; } = string.Empty;
        public IFormFile? CategoryImage { get; set; }
        [Required(ErrorMessage = "Please select status.")]
        public int Status { get; set; }
    }
}
