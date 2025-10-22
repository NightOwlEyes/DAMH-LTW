using Doanmh.Model;
using Doanmh.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using System.IO;

namespace Doanmh.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    [ApiExplorerSettings(GroupName = "product")]
    public class ProductApiController : ControllerBase
    {
        private readonly IProductRepository _productRepository;
        private readonly IWebHostEnvironment _env;

        public ProductApiController(IProductRepository productRepository, IWebHostEnvironment env)
        {
            _productRepository = productRepository;
            _env = env;
        }

        [HttpGet]
        public async Task<IActionResult> GetProducts()
        {
            try
            {
                var products = await _productRepository.GetProductsAsync();
                return Ok(products);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetProductById(int id)
        {
            try
            {
                var product = await _productRepository.GetProductByIdAsync(id);
                if (product == null)
                    return NotFound($"Product with ID {id} not found");
                return Ok(product);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [RequestSizeLimit(10_000_000)]
        public async Task<IActionResult> AddProduct([FromBody] ProductDto productDto)
        {
            try // <--- Khối try lớn bắt đầu
            {
                if (productDto == null) // <--- Khối if (data from form-data)
                {
                    // Thử đọc từ form data nếu không có JSON body
                    var form = await Request.ReadFormAsync();
                    
                    if (!form.ContainsKey("Name") || !form.ContainsKey("Price"))
                    {
                        return BadRequest("Missing required fields");
                    }

                    var name = form["Name"].ToString();
                    // Lưu ý: Có thể cần xử lý lỗi nếu form["Price"] không phải là số (decimal.Parse)
                    if (!decimal.TryParse(form["Price"], out var price))
                    {
                        return BadRequest("Invalid price format");
                    }
                    var description = form.ContainsKey("Description") ? form["Description"].ToString() : string.Empty;
                    var imageFile = form.Files.GetFile("ImageFile");

                    string? imagePath = null;

                    if (imageFile != null && imageFile.Length > 0)
                    {
                        var uploads = Path.Combine(_env.WebRootPath, "images");
                        Directory.CreateDirectory(uploads);
                        var fileName = Guid.NewGuid() + Path.GetExtension(imageFile.FileName);
                        var filePath = Path.Combine(uploads, fileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await imageFile.CopyToAsync(stream);
                        }

                        imagePath = "/images/" + fileName;
                    }

                    var product = new Product
                    {
                        Name = name,
                        Price = price,
                        Description = description,
                        ImageUrl = imagePath
                    };

                    await _productRepository.AddProductAsync(product);
                    return CreatedAtAction(nameof(GetProductById), new { id = product.Id }, product);
                } // <--- Kết thúc khối if
                else // <--- Khối else (data from JSON body)
                {
                    // Xử lý dữ liệu từ JSON body
                    var product = new Product
                    {
                        Name = productDto.Name,
                        Price = productDto.Price,
                        Description = productDto.Description ?? string.Empty,
                        ImageUrl = productDto.ImageUrl
                    };

                    await _productRepository.AddProductAsync(product);
                    return CreatedAtAction(nameof(GetProductById), new { id = product.Id }, product);
                }
            } // <--- THIẾU DẤU NÀY: Kết thúc khối try lớn
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        } // <--- ĐÃ CÓ DẤU NÀY: Kết thúc phương thức AddProduct


        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProduct(int id, [FromBody] ProductDto productDto)
        {
            try
            {
                var existingProduct = await _productRepository.GetProductByIdAsync(id);
                if (existingProduct == null)
                {
                    return NotFound($"Product with ID {id} not found");
                }

                // Cập nhật thông tin sản phẩm
                existingProduct.Name = productDto.Name;
                existingProduct.Price = productDto.Price;
                existingProduct.Description = productDto.Description ?? existingProduct.Description;

                await _productRepository.UpdateProductAsync(existingProduct);
                return Ok(existingProduct);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            try
            {
                await _productRepository.DeleteProductAsync(id);
                return NoContent();
            }
            catch
            {
                return StatusCode(500, "Internal server error");
            }
        }
    } // <--- Dấu này kết thúc class ProductApiController
} // <--- Dấu này kết thúc namespace Doanmh.Controllers