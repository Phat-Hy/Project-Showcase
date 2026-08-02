using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using GaraShowcase.Api.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Security.Claims;
using System.Threading.Tasks;

namespace GaraShowcase.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly GaraDbContext _context;
        private readonly string? _blobConnectionString;

        public UsersController(GaraDbContext context)
        {
            _context = context;
            _blobConnectionString = Environment.GetEnvironmentVariable("AZURE_STORAGE_CONNECTION_STRING");
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
            if (request.CvUrl != null)
            {
                user.CvUrl = request.CvUrl;
            }
            user.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(new { message = "Cập nhật hồ sơ thành công.", user });
        }

        [HttpPost("{id}/upload-cv")]
        public async Task<IActionResult> UploadCv(Guid id, IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(new { error = "Không tìm thấy tệp tải lên hoặc tệp rỗng." });
            }

            // Verify file is a PDF
            var extension = Path.GetExtension(file.FileName).ToLower();
            if (extension != ".pdf")
            {
                return BadRequest(new { error = "Hệ thống chỉ chấp nhận tải lên tệp định dạng PDF." });
            }

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
                return StatusCode(403, new { error = "Bạn không có quyền đăng tải CV cho tài khoản này." });
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
            if (user == null)
            {
                return NotFound(new { error = "Không tìm thấy thông tin học viên." });
            }

            // Stream upload to Azure Blob Storage if connection config is present
            var fileUrl = "";
            if (!string.IsNullOrEmpty(_blobConnectionString))
            {
                try
                {
                    var blobServiceClient = new BlobServiceClient(_blobConnectionString);
                    var containerClient = blobServiceClient.GetBlobContainerClient("media");
                    
                    await containerClient.CreateIfNotExistsAsync(PublicAccessType.Blob);

                    var uniqueFileName = $"{Guid.NewGuid()}_{file.FileName}";
                    var blobClient = containerClient.GetBlobClient($"cvs/{id}/{uniqueFileName}");

                    using (var stream = file.OpenReadStream())
                    {
                        await blobClient.UploadAsync(stream, new BlobHttpHeaders { ContentType = "application/pdf" });
                    }

                    fileUrl = blobClient.Uri.ToString();
                }
                catch (Exception ex)
                {
                    return StatusCode(500, new { error = "Lỗi truyền tải dữ liệu lên Azure Storage: " + ex.Message });
                }
            }
            else
            {
                // Local mockup fallback URL
                fileUrl = $"/uploads/mock_cv_{Guid.NewGuid()}_{file.FileName}";
            }

            // Save CV URL to user profile
            user.CvUrl = fileUrl;
            user.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Tải lên tệp CV thành công.", cvUrl = fileUrl });
        }
    }
}
