namespace FoodOne.Models
{
    public class Category
    {
        public int Id { get; set; }
        public string? CategoryName { get; set; }
        public string? CategoryImage { get; set; }
        public int Status { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        // Navigation Property
        public ICollection<Item>? Items { get; set; }
    }
}
