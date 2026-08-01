using GaraShowcase.Api.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace GaraShowcase.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly GaraDbContext _context;

        public UsersController(GaraDbContext context)
        {
            _context = context;
        }

        public class UpdateProfileRequest
        {
            public string? ContactLink { get; set; }
            public string? CvUrl { get; set; }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProfile(Guid id, [FromBody] UpdateProfileRequest request)
        {
            // Auth check
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            var userRoleClaim = User.FindFirst(ClaimTypes.Role);

            if (userIdClaim == null)
            {
                return Unauthorized(new { error = "Chưa xác thực người dùng." });
            }

            var currentUserId = Guid.Parse(userIdClaim.Value);
            var currentUserRole = userRoleClaim?.Value ?? "Guest";

            if (currentUserRole != "Manager" && currentUserId != id)
            {
                return StatusCode(403, new { error = "Bạn không có quyền sửa đổi hồ sơ này." });
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
            if (user == null)
            {
                return NotFound(new { error = "Không tìm thấy người dùng." });
            }

            user.ContactLink = request.ContactLink;
            user.CvUrl = request.CvUrl;
            user.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(new { message = "Cập nhật hồ sơ thành công.", user });
        }
    }
}
