using System.ComponentModel.DataAnnotations;

namespace FoodOne.ViewModels
{
    public class ItemViewModel
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "Item name is required.")]
        public string ItemName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please select a category.")]
        [Range(1, int.MaxValue, ErrorMessage = "Please select a category.")]
        public int? CategoryId { get; set; }

        [Required(ErrorMessage = "Price is required.")]
        [Range(typeof(decimal), "0.01", "999999999", ErrorMessage = "Price must be greater than 0.")]
        public decimal? Price { get; set; }

        public IFormFile? Image { get; set; }

        [Required(ErrorMessage = "Item details are required.")]
        public string ItemDetail { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please select status.")]
        public int Status { get; set; }
        
    }
}
