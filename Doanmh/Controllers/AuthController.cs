using Doanmh.Model;
using Doanmh.Repository;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using Doanmh.Helpers;

namespace Doanmh.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IUserRepository _userRepository;

        public AuthController(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto userDto)
        {
            var result = await _userRepository.RegisterAsync(userDto);

            if (!result)
                return BadRequest("User already exists.");

            return Ok(new { message = "Register  successful." });
        }

        [HttpPost("register-admin")]
        public async Task<IActionResult> RegisterAdmin([FromBody] RegisterDto userDto)
        {
            var result = await _userRepository.RegisterAdminAsync(userDto);

            if (!result)
                return BadRequest("Admin user already exists.");

            return Ok(new { message = "Admin register successful." });
        }

        [HttpPost("test-login")]
        public async Task<IActionResult> TestLogin([FromBody] LoginDto loginDto)
        {
            var user = await _userRepository.LoginAsync(loginDto.Username, loginDto.Password);
            if (user == null)
                return Unauthorized("Invalid username or password.");
            
            var token = JwtHelper.GenerateToken(user);
            
            return Ok(new
            {
                message = "Login successful.",
                token = token,
                user = new { 
                    id = user.Id, 
                    username = user.Username, 
                    role = user.Role,
                    fullName = user.FullName
                },
                bearerToken = $"Bearer {token}" // Thêm format Bearer để dễ copy
            });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            var user = await _userRepository.LoginAsync(loginDto.Username, loginDto.Password);
            if (user == null)
                return Unauthorized("Invalid username or password.");
            // Tạo token
            var token = JwtHelper.GenerateToken(user); // bạn cần tự tạo helper này

            return Ok(new
            {
                message = "Login successful.",
                token = token,
                user = new { 
                    id = user.Id, 
                    username = user.Username, 
                    role = user.Role,
                    fullName = user.FullName
                }
            });



        }
    }
}
