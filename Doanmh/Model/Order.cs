using Doanmh.Model;

public class Order
{
    public int Id { get; set; }

    public int UserId { get; set; }
    public User User { get; set; } // ✅ Navigation property PHẢI CÓ

    public DateTime OrderDate { get; set; } = DateTime.Now;
    public string Address { get; set; } = string.Empty;
    public string Note { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; } = 0m;

    public List<OrderItem> Items { get; set; } = new List<OrderItem>();
}