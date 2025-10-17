namespace Doanmh.Model
{
    public class Product
    {
        public int Id { get; set; }

        public string Name { get; set; }
        public decimal Price { get; set; }

        public string Description { get; set; }

        public string? ImageUrl { get; set; } // => đường dẫn ảnh

        // Nếu muốn: Danh sách các OrderItem chứa sản phẩm này
        public ICollection<OrderItem> OrderItems { get; set; }

    }
}
