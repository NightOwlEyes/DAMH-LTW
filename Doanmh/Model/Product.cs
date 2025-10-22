namespace Doanmh.Model
{
    public class Product
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }

        public string Description { get; set; } = string.Empty;

        public string? ImageUrl { get; set; } // => đường dẫn ảnh

        // Nếu muốn: Danh sách các OrderItem chứa sản phẩm này
        [System.Text.Json.Serialization.JsonIgnore]
        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

    }
}
