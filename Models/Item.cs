namespace FoodOne.Models
{
    public class Item
    {
        public int Id { get; set; }
        public string ItemName { get; set; } = string.Empty;
        public int CategoryId { get; set; }
        public decimal Price { get; set; }
        public string? Image { get; set; }
        public string? ItemDetail { get; set; }
        public int Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        // Navigation Property
        public Category? Category { get; set; }
    }
}
