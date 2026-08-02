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

        public class RegisterRequest
        {
            public string Email { get; set; } = string.Empty;
            public string Password { get; set; } = string.Empty;
            public string Name { get; set; } = string.Empty;
            public string Role { get; set; } = string.Empty; // Student or Founder
            public string StudentId { get; set; } = string.Empty;
            public string ProjectName { get; set; } = string.Empty;
            public string ProjectPitch { get; set; } = string.Empty;
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
                Secure = true, // Required for HTTPS on Azure. Allowed on localhost HTTP by modern browsers.
                SameSite = SameSiteMode.Lax,
                Expires = DateTimeOffset.UtcNow.AddDays(7)
            });

            return Ok(new { message = "Đăng nhập thành công.", user });
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Password) || 
                string.IsNullOrEmpty(request.Name) || string.IsNullOrEmpty(request.Role))
            {
                return BadRequest(new { error = "Thiếu thông tin đăng ký bắt buộc." });
            }

            if (request.Role != "Student" && request.Role != "Founder")
            {
                return BadRequest(new { error = "Vai trò (Role) không hợp lệ. Phải là Student hoặc Founder." });
            }

            if (!request.Email.EndsWith("@fpt.edu.vn", StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest(new { error = "Đăng ký yêu cầu email đuôi @fpt.edu.vn của trường học." });
            }

            var existingUser = await _context.Users.AnyAsync(u => u.Email.ToLower() == request.Email.Trim().ToLower());
            if (existingUser)
            {
                return BadRequest(new { error = "Email này đã được sử dụng đăng ký trước đó." });
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                Guid? projectId = null;

                if (request.Role == "Founder")
                {
                    if (string.IsNullOrEmpty(request.ProjectName) || string.IsNullOrEmpty(request.ProjectPitch))
                    {
                        return BadRequest(new { error = "Sáng lập viên (Founder) cần điền tên dự án và giới thiệu ngắn (pitch)." });
                    }

                    var newProject = new Project
                    {
                        Id = Guid.NewGuid(),
                        Name = request.ProjectName,
                        Pitch = request.ProjectPitch,
                        Description = $"# {request.ProjectName}\nChào mừng đến với trang dự án {request.ProjectName}. Đây là phần mô tả chi tiết được định dạng bằng Markdown.",
                        Status = "Draft",
                        StorageUsedBytes = 0,
                        LastUpdatedAt = DateTime.UtcNow
                    };

                    _context.Projects.Add(newProject);
                    await _context.SaveChangesAsync();
                    projectId = newProject.Id;
                }

                var newUser = new User
                {
                    Id = Guid.NewGuid(),
                    Email = request.Email.Trim().ToLower(),
                    PasswordHash = PasswordHasher.HashPassword(request.Password),
                    Name = request.Name,
                    Role = request.Role,
                    StudentId = string.IsNullOrEmpty(request.StudentId) ? null : request.StudentId.Trim().ToUpper(),
                    ProjectId = projectId,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.Users.Add(newUser);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                // Sign JWT token for auto login after register
                var token = TokenHelper.SignToken(newUser.Id, newUser.Email, newUser.Name, newUser.Role, newUser.StudentId);
                Response.Cookies.Append("token", token, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Lax,
                    Expires = DateTimeOffset.UtcNow.AddDays(7)
                });

                return Created("", new { message = "Đăng ký tài khoản thành công.", user = newUser });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, new { error = "Lỗi xử lý đăng ký tài khoản: " + ex.Message });
            }
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

        [HttpGet("debug")]
        public async Task<IActionResult> DebugUsers()
        {
            try
            {
                var users = await _context.Users
                    .Select(u => new { u.Email, u.Name, u.Role, HasPassword = !string.IsNullOrEmpty(u.PasswordHash) })
                    .ToListAsync();
                
                var projects = await _context.Projects
                    .Select(p => new { p.Name, p.Status })
                    .ToListAsync();

                return Ok(new { users, projects });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message, stackTrace = ex.StackTrace });
            }
        }
    }
}
