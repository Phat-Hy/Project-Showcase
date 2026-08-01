using GaraShowcase.Api.Data;
using GaraShowcase.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace GaraShowcase.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AdminController : ControllerBase
    {
        private readonly GaraDbContext _context;

        public class StatusRequest
        {
            public string Status { get; set; } = string.Empty;
        }

        public AdminController(GaraDbContext context)
        {
            _context = context;
        }

        [HttpPatch("projects/{id}/status")]
        public async Task<IActionResult> UpdateProjectStatus(Guid id, [FromBody] StatusRequest request)
        {
            var roleClaim = User.FindFirst(ClaimTypes.Role);
            if (roleClaim?.Value != "Manager")
            {
                return StatusCode(403, new { error = "Bạn không có quyền thực hiện hành động này." });
            }

            var project = await _context.Projects.FirstOrDefaultAsync(p => p.Id == id);
            if (project == null)
            {
                return NotFound(new { error = "Không tìm thấy dự án." });
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                project.Status = request.Status;
                project.UpdatedAt = DateTime.UtcNow;

                if (request.Status == "Suspended")
                {
                    // Close all jobs under the suspended project
                    var jobs = await _context.Jobs.Where(j => j.ProjectId == id && j.Status == "Open").ToListAsync();
                    foreach (var job in jobs)
                    {
                        job.Status = "Closed";
                    }
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(new { message = $"Đã cập nhật trạng thái dự án thành: {request.Status}." });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, new { error = "Lỗi cập nhật trạng thái: " + ex.Message });
            }
        }

        [HttpPost("run-dormancy-check")]
        public async Task<IActionResult> RunDormancyCheck()
        {
            var roleClaim = User.FindFirst(ClaimTypes.Role);
            if (roleClaim?.Value != "Manager")
            {
                return StatusCode(403, new { error = "Bạn không có quyền chạy tiến trình quét." });
            }

            var projects = await _context.Projects.Where(p => p.Status != "Suspended" && p.Status != "Draft").ToListAsync();
            var now = DateTime.UtcNow;

            int scannedCount = 0;
            int warningsIssued = 0;
            int suspendedCount = 0;

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                foreach (var project in projects)
                {
                    scannedCount++;
                    var daysSinceUpdate = (now - project.LastUpdatedAt).TotalDays;

                    if (daysSinceUpdate >= 30.0)
                    {
                        project.Status = "Suspended";
                        project.UpdatedAt = now;
                        suspendedCount++;

                        // Close associated job openings
                        var jobs = await _context.Jobs.Where(j => j.ProjectId == project.Id && j.Status == "Open").ToListAsync();
                        foreach (var job in jobs)
                        {
                            job.Status = "Closed";
                        }
                    }
                    else if (daysSinceUpdate >= 14.0)
                    {
                        if (project.Status != "At-Risk")
                        {
                            project.Status = "At-Risk";
                            project.UpdatedAt = now;
                            warningsIssued++;
                        }
                    }
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(new { 
                    message = "Tiến trình quét hoàn tất thành công.",
                    results = new { scannedCount, warningsIssued, suspendedCount }
                });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, new { error = "Lỗi quét dormancy: " + ex.Message });
            }
        }

        [HttpGet("reports/csv")]
        public async Task<IActionResult> ExportCsvReport()
        {
            var roleClaim = User.FindFirst(ClaimTypes.Role);
            if (roleClaim?.Value != "Manager")
            {
                return StatusCode(403, new { error = "Bạn không có quyền xuất báo cáo." });
            }

            var projects = await _context.Projects
                .Include(p => p.TeamMembers)
                .OrderBy(p => p.Name)
                .ToListAsync();

            var csv = new StringBuilder();
            csv.AppendLine("Mã dự án,Tên dự án,Tình trạng,Bộ nhớ sử dụng (MB),Số thành viên,Ngày cập nhật cuối");

            foreach (var p in projects)
            {
                double storageMB = p.StorageUsedBytes / 1024.0 / 1024.0;
                csv.AppendLine($"{p.Id},{EscapeCsvField(p.Name)},{p.Status},{storageMB:F2},{p.TeamMembers.Count},{p.LastUpdatedAt:yyyy-MM-dd HH:mm:ss}");
            }

            var bytes = Encoding.UTF8.GetBytes(csv.ToString());
            var bom = new byte[] { 0xEF, 0xBB, 0xBF };
            var fileBytes = bom.Concat(bytes).ToArray();

            return File(fileBytes, "text/csv", $"GaraIncubatorReport_{DateTime.UtcNow:yyyyMMdd}.csv");
        }

        private static string EscapeCsvField(string field)
        {
            if (string.IsNullOrEmpty(field)) return string.Empty;
            if (field.Contains(",") || field.Contains("\"") || field.Contains("\n") || field.Contains("\r"))
            {
                return $"\"{field.Replace("\"", "\"\"")}\"";
            }
            return field;
        }
    }
}
