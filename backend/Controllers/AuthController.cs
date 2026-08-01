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

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromQuery] string? mockRole)
        {
            if (string.IsNullOrEmpty(mockRole))
            {
                return BadRequest(new { error = "Thiếu thông tin vai trò đăng nhập." });
            }

            // Map mock users
            var email = "";
            var name = "";
            var studentId = "";
            var role = "";
            Guid? projectId = null;

            if (mockRole == "FounderPhat")
            {
                email = "phathmse184629@fpt.edu.vn";
                name = "Hỷ Minh Phát";
                studentId = "SE184629";
                role = "Founder";
                var project = await _context.Projects.FirstOrDefaultAsync(p => p.Name == "Gara Startup Project Showcase");
                projectId = project?.Id;
            }
            else if (mockRole == "StudentKhanh")
            {
                email = "khanhltse184638@fpt.edu.vn";
                name = "Lê Tuấn Khanh";
                studentId = "SE184638";
                role = "Student";
            }
            else if (mockRole == "Manager")
            {
                email = "manager.mock@fpt.edu.vn";
                name = "Vườn Ươm Gara Manager";
                studentId = null;
                role = "Manager";
            }
            else
            {
                return BadRequest(new { error = "Vai trò không hợp lệ." });
            }

            // Sync user in database
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
            {
                user = new User
                {
                    Id = Guid.NewGuid(),
                    Email = email,
                    Name = name,
                    Role = role,
                    StudentId = studentId,
                    ProjectId = projectId
                };
                _context.Users.Add(user);
                await _context.SaveChangesAsync();
            }
            else
            {
                user.Role = role;
                user.Name = name;
                user.StudentId = studentId;
                user.ProjectId = projectId;
                await _context.SaveChangesAsync();
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
