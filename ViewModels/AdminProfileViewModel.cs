using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace FoodOne.ViewModels
{
    public class AdminProfileViewModel
    {
        public int Id { get; set; }

        [Required]
        public string FullName { get; set; } = string.Empty;

        public string ImageUrl { get; set; } = string.Empty;

        public IFormFile? ImageFile { get; set; }
        
    }
}
