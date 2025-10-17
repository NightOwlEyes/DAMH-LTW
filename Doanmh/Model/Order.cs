using System.ComponentModel.DataAnnotations;

namespace Doanmh.Model
{
    public class Order
    {
        public int Id { get; set; }


     
        public DateTime OrderDate { get; set; } = DateTime.Now;
        public string Address { get; set; }
        public string Note { get; set; }

        public int UserId { get; set; }

        public List<OrderItem> Items { get; set; }
    }
}
