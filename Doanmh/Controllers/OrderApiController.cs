using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Doanmh.Model;
using Microsoft.AspNetCore.Authorization;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace Doanmh.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public OrderApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllOrders()
        {
            var orders = await _context.Orders
                .Include(o => o.Items)
                .ThenInclude(oi => oi.Product)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            return Ok(orders);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetOrderDetail(int id)
        {
            var order = await _context.Orders
                .Include(o => o.Items)
                .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
                return NotFound();

            return Ok(order);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> CreateOrder([FromBody] OrderCreateDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return BadRequest(new { errors });
            }

            Console.WriteLine($"[CreateOrder] Nhận dữ liệu - Address: {dto.Address}, Note: {dto.Note}");

            try
            {
                var userIdStr = User.FindFirst("id")?.Value;
                Console.WriteLine($"[CreateOrder] UserId (chuỗi) từ token: {userIdStr}");

                if (userIdStr == null)
                    return Unauthorized("Token không chứa user id");

                if (!int.TryParse(userIdStr, out int userId))
                {
                    Console.WriteLine("[CreateOrder] Không thể parse userId");
                    return BadRequest("UserId trong token không hợp lệ");
                }

                Console.WriteLine($"[CreateOrder] userId sau khi parse: {userId}");

                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                {
                    Console.WriteLine("[CreateOrder] User không tồn tại trong database");
                    return NotFound("User không tồn tại");
                }

                var cartItems = await _context.CartItems
                    .Where(c => c.UserId == userId)
                    .Include(c => c.Product)
                    .ToListAsync();

                Console.WriteLine($"[CreateOrder] Số lượng sản phẩm trong giỏ hàng: {cartItems.Count}");

                if (!cartItems.Any())
                {
                    Console.WriteLine("[CreateOrder] Giỏ hàng trống");
                    return BadRequest("Giỏ hàng trống");
                }

                var order = new Order
                {
                    UserId = userId,
                    Address = dto.Address,
                    Note = dto.Note,
                    OrderDate = DateTime.Now,
                    Items = cartItems.Select(c => new OrderItem
                    {
                        ProductId = c.ProductId,
                        Quantity = c.Quantity,
                        Price = c.Product.Price,
                        ProductName = c.Product.Name
                    }).ToList()
                };

                _context.Orders.Add(order);
                _context.CartItems.RemoveRange(cartItems);
                await _context.SaveChangesAsync();

                Console.WriteLine("[CreateOrder] Đặt hàng thành công");
                return Ok(order);
            }
            catch (Exception ex)
            {
                Console.WriteLine("[CreateOrder] Lỗi máy chủ: " + ex.Message);
                Console.WriteLine("[CreateOrder] Inner Exception: " + ex.InnerException?.Message);
                return StatusCode(500, "Lỗi máy chủ: " + ex.Message);
            }
        }
    }
}
