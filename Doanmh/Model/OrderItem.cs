using System.ComponentModel.DataAnnotations.Schema;

namespace Doanmh.Model
{
    public class OrderItem
    {
        public int Id { get; set; }

        public int OrderId { get; set; }
        [System.Text.Json.Serialization.JsonIgnore]
        public Order Order { get; set; } = null!;
        [ForeignKey("Order")]
        public int ProductId { get; set; }
        public Product Product { get; set; } = null!;
        [ForeignKey("Product")]
        public int Quantity { get; set; }
      

        public decimal Price { get; set; }              // ✅ Đúng kiểu
        public string ProductName { get; set; } = string.Empty;
    }
}
