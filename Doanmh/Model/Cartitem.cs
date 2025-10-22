namespace Doanmh.Model
{
    public class CartItem
    {
        public int Id { get; set; }

        public int ProductId { get; set; }
        public Product Product { get; set; }

        public int UserId { get; set; } // BẮT BUỘC PHẢI CÓ
        public User User { get; set; }

        public int Quantity { get; set; }
    }
}
