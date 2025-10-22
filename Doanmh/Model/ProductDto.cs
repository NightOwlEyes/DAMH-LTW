namespace Doanmh.Model
{
    public class ProductDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public decimal Price { get; set; }
        public string? Description { get; set; }
        // For JSON requests include the URL; file uploads are handled via multipart/form-data in the controller
        public string? ImageUrl { get; set; }
    }
}
