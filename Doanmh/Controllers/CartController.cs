using Doanmh.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Doanmh.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    [ApiExplorerSettings(GroupName = "cart")]
    public class CartController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public CartController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Thêm vào giỏ hàng
        [HttpPost("add")]
        public async Task<IActionResult> AddToCart([FromBody] CartDto item)
        {
            var userIdStr = User.FindFirst("id")?.Value;
            if (!int.TryParse(userIdStr, out int userId))
                return Unauthorized("Token không hợp lệ");

            var product = await _context.Products.FindAsync(item.ProductId);
            if (product == null)
                return NotFound("Không tìm thấy sản phẩm.");

            var existingItem = await _context.CartItems
                .FirstOrDefaultAsync(c => c.UserId == userId && c.ProductId == item.ProductId);

            if (existingItem != null)
            {
                existingItem.Quantity += item.Quantity;
            }
            else
            {
                var cartItem = new CartItem
                {
                    UserId = userId,
                    ProductId = item.ProductId,
                    Quantity = item.Quantity
                };
                _context.CartItems.Add(cartItem);
            }

            await _context.SaveChangesAsync();
            return Ok("Đã thêm vào giỏ hàng");
        }

        // Xem giỏ hàng
        [HttpGet]
        public async Task<IActionResult> GetCart()
        {
            var userIdStr = User.FindFirst("id")?.Value;
            if (!int.TryParse(userIdStr, out int userId))
                return Unauthorized("Token không hợp lệ");

            var cartItems = await _context.CartItems
                .Where(c => c.UserId == userId)
                .Include(c => c.Product)
                .Select(c => new
                {
                    c.ProductId,
                    Name = c.Product.Name,
                    Price = c.Product.Price,
                    c.Quantity
                })
                .ToListAsync();

            return Ok(cartItems);
        }

        // Xóa sản phẩm khỏi giỏ hàng
        [HttpDelete("remove/{productId}")]
        public async Task<IActionResult> RemoveFromCart(int productId)
        {
            var userIdStr = User.FindFirst("id")?.Value;
            if (!int.TryParse(userIdStr, out int userId))
                return Unauthorized("Token không hợp lệ");

            var item = await _context.CartItems
                .FirstOrDefaultAsync(c => c.UserId == userId && c.ProductId == productId);

            if (item != null)
            {
                _context.CartItems.Remove(item);
                await _context.SaveChangesAsync();
            }

            return Ok("Đã xóa khỏi giỏ hàng");
        }

        // Xóa toàn bộ giỏ hàng
        [HttpPost("clear")]
        public async Task<IActionResult> ClearCart()
        {
            var userIdStr = User.FindFirst("id")?.Value;
            if (!int.TryParse(userIdStr, out int userId))
                return Unauthorized("Token không hợp lệ");

            var items = await _context.CartItems
                .Where(c => c.UserId == userId)
                .ToListAsync();

            _context.CartItems.RemoveRange(items);
            await _context.SaveChangesAsync();

            return Ok("Đã xóa toàn bộ giỏ hàng");
        }
    }
}
