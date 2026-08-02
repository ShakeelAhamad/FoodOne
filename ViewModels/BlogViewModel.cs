using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace FoodOne.ViewModels
{
    public class BlogViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Title is required.")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please select a category.")]
        [Range(1, int.MaxValue, ErrorMessage = "Please select a category.")]
        public int? CategoryId { get; set; }

        public IFormFile? Image { get; set; }

        [Required(ErrorMessage = "Description is required.")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please select status.")]
        public int Status { get; set; }
    }
}
