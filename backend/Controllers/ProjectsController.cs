using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using GaraShowcase.Api.Data;
using GaraShowcase.Api.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace GaraShowcase.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProjectsController : ControllerBase
    {
        private readonly GaraDbContext _context;
        private readonly string? _blobConnectionString;
        private const long MAX_PROJECT_STORAGE_BYTES = 500 * 1024 * 1024; // 500MB
        private const long MAX_SINGLE_FILE_BYTES = 10 * 1024 * 1024; // 10MB individual limit

        public ProjectsController(GaraDbContext context)
        {
            _context = context;
            _blobConnectionString = Environment.GetEnvironmentVariable("AZURE_STORAGE_CONNECTION_STRING");
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var roleClaim = User.FindFirst(ClaimTypes.Role);
            var role = roleClaim?.Value ?? "Guest";

            IQueryable<Project> query = _context.Projects
                .Include(p => p.Milestones)
                .Include(p => p.TeamMembers);

            // Filter drafts for guest / student roles
            if (role == "Guest" || role == "Student")
            {
                query = query.Where(p => p.Status == "Active" || p.Status == "At-Risk");
            }

            var projects = await query.OrderBy(p => p.Name).ToListAsync();
            return Ok(projects);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var project = await _context.Projects
                .Include(p => p.Milestones)
                .Include(p => p.TeamMembers)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (project == null)
            {
                return NotFound(new { error = "Không tìm thấy dự án." });
            }

            return Ok(project);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Project project)
        {
            var roleClaim = User.FindFirst(ClaimTypes.Role);
            if (roleClaim?.Value != "Founder" && roleClaim?.Value != "Manager")
            {
                return StatusCode(403, new { error = "Bạn không có quyền tạo dự án." });
            }

            if (string.IsNullOrEmpty(project.Name) || string.IsNullOrEmpty(project.Pitch))
            {
                return BadRequest(new { error = "Tên dự án và mô tả tóm tắt là bắt buộc." });
            }

            project.Id = Guid.NewGuid();
            project.Status = "Draft"; // Default to Draft state
            project.StorageUsedBytes = 0;
            project.LastUpdatedAt = DateTime.UtcNow;
            project.CreatedAt = DateTime.UtcNow;
            project.UpdatedAt = DateTime.UtcNow;

            try
            {
                _context.Projects.Add(project);
                await _context.SaveChangesAsync();
                return CreatedAtAction(nameof(GetById), new { id = project.Id }, project);
            }
            catch (Exception ex)
            {
                if (ex.InnerException?.Message.Contains("unique") == true || ex.Message.Contains("unique"))
                {
                    return BadRequest(new { error = "Tên dự án đã tồn tại trên hệ thống." });
                }
                return StatusCode(500, new { error = "Lỗi lưu dự án: " + ex.Message });
            }
        }

        [HttpPost("{id}/milestones")]
        public async Task<IActionResult> AddMilestone(Guid id, [FromBody] Milestone milestone)
        {
            if (string.IsNullOrEmpty(milestone.Title))
            {
                return BadRequest(new { error = "Tiêu đề cột mốc là bắt buộc." });
            }

            var project = await _context.Projects.FirstOrDefaultAsync(p => p.Id == id);
            if (project == null)
            {
                return NotFound(new { error = "Không tìm thấy dự án." });
            }

            milestone.Id = Guid.NewGuid();
            milestone.ProjectId = id;
            milestone.CreatedAt = DateTime.UtcNow;

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                _context.Milestones.Add(milestone);

                // Reset last updated and restore status (BR-09)
                project.Status = "Active";
                project.LastUpdatedAt = DateTime.UtcNow;
                project.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Created("", new { message = "Thêm cột mốc thành công và kích hoạt lại dự án.", milestoneId = milestone.Id });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, new { error = "Lỗi thêm cột mốc: " + ex.Message });
            }
        }

        [HttpPost("{id}/upload")]
        public async Task<IActionResult> UploadFile(Guid id, IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(new { error = "Không tìm thấy tệp tải lên hoặc tệp rỗng." });
            }

            // Enforce 10MB individual cap
            if (file.Length > MAX_SINGLE_FILE_BYTES)
            {
                return BadRequest(new { error = "Kích thước tệp tải lên vượt quá giới hạn cho phép (Tối đa 10MB)." });
            }

            var project = await _context.Projects.FirstOrDefaultAsync(p => p.Id == id);
            if (project == null)
            {
                return NotFound(new { error = "Không tìm thấy dự án." });
            }

            var currentStorage = project.StorageUsedBytes;
            var newStorage = currentStorage + file.Length;

            // Enforce 500MB cumulative cap (BR-04)
            if (newStorage > MAX_PROJECT_STORAGE_BYTES)
            {
                double currentMB = currentStorage / 1024.0 / 1024.0;
                double fileMB = file.Length / 1024.0 / 1024.0;
                return BadRequest(new { 
                    error = $"Dung lượng lưu trữ vượt quá giới hạn 500MB cho phép. (Hiện tại: {currentMB:F2}MB, File mới: {fileMB:F2}MB)" 
                });
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
                    var blobClient = containerClient.GetBlobClient($"projects/{id}/{uniqueFileName}");

                    using (var stream = file.OpenReadStream())
                    {
                        await blobClient.UploadAsync(stream, new BlobHttpHeaders { ContentType = file.ContentType });
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
                fileUrl = $"/uploads/mock_{Guid.NewGuid()}_{file.FileName}";
            }

            project.StorageUsedBytes = newStorage;
            project.LastUpdatedAt = DateTime.UtcNow;
            project.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Tải lên file thành công.", fileUrl, storageUsedBytes = newStorage });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] ProjectUpdateDto dto)
        {
            var project = await _context.Projects.FirstOrDefaultAsync(p => p.Id == id);
            if (project == null)
            {
                return NotFound(new { error = "Không tìm thấy dự án." });
            }

            if (dto.Pitch != null) project.Pitch = dto.Pitch;
            if (dto.Description != null) project.Description = dto.Description;

            project.LastUpdatedAt = DateTime.UtcNow;
            project.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return Ok(new { message = "Cập nhật thông tin dự án thành công.", project });
        }

        [HttpDelete("{id}/members/{memberId}")]
        public async Task<IActionResult> RemoveMember(Guid id, Guid memberId)
        {
            var project = await _context.Projects.Include(p => p.TeamMembers).FirstOrDefaultAsync(p => p.Id == id);
            if (project == null)
            {
                return NotFound(new { error = "Không tìm thấy dự án." });
            }

            var member = await _context.Users.FirstOrDefaultAsync(u => u.Id == memberId && u.ProjectId == id);
            if (member == null)
            {
                return NotFound(new { error = "Không tìm thấy thành viên trong dự án này." });
            }

            if (member.Role == "Founder")
            {
                return BadRequest(new { error = "Không thể loại bỏ nhà sáng lập ra khỏi dự án." });
            }

            member.ProjectId = null;
            project.LastUpdatedAt = DateTime.UtcNow;
            project.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return Ok(new { message = "Đã loại bỏ thành viên ra khỏi dự án." });
        }
    }

    public class ProjectUpdateDto
    {
        public string? Pitch { get; set; }
        public string? Description { get; set; }
    }
}
