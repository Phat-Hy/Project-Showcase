using GaraShowcase.Api.Data;
using GaraShowcase.Api.Models;
using GaraShowcase.Api.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace GaraShowcase.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly GaraDbContext _context;

        public AuthController(GaraDbContext context)
        {
            _context = context;
        }

        public class LoginRequest
        {
            public string Email { get; set; } = string.Empty;
            public string Password { get; set; } = string.Empty;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Password))
            {
                return BadRequest(new { error = "Thiếu thông tin Email hoặc Mật khẩu." });
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email.ToLower() == request.Email.Trim().ToLower());

            if (user == null || !PasswordHasher.VerifyPassword(request.Password, user.PasswordHash))
            {
                return BadRequest(new { error = "Email hoặc Mật khẩu không chính xác." });
            }

            // Sign JWT token
            var token = TokenHelper.SignToken(user.Id, user.Email, user.Name, user.Role, user.StudentId);

            // Append cookie
            Response.Cookies.Append("token", token, new CookieOptions
            {
                HttpOnly = true,
                Secure = false, // Allow HTTP locally
                SameSite = SameSiteMode.Lax,
                Expires = DateTimeOffset.UtcNow.AddDays(7)
            });

            return Ok(new { message = "Đăng nhập thành công.", user });
        }

        [HttpPost("logout")]
        public IActionResult Logout()
        {
            Response.Cookies.Delete("token");
            return Ok(new { message = "Đăng xuất thành công." });
        }

        [HttpGet("me")]
        public async Task<IActionResult> GetCurrentUser()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return Unauthorized(new { error = "Chưa đăng nhập." });
            }

            if (!Guid.TryParse(userIdClaim.Value, out var userId))
            {
                return BadRequest(new { error = "ID người dùng không hợp lệ." });
            }

            var user = await _context.Users
                .Include(u => u.Project)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
            {
                return NotFound(new { error = "Không tìm thấy hồ sơ người dùng." });
            }

            return Ok(user);
        }
    }
}
