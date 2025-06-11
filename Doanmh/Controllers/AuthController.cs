using Doanmh.Model;
using Doanmh.Repository;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

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

            return Ok("Registration successful.");
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            var user = await _userRepository.LoginAsync(loginDto.Username, loginDto.Password);
            if (user == null)
                return Unauthorized("Invalid username or password.");

            return Ok("Login successful.");
        }
    }
}
