using GaraShowcase.Api.Data;
using GaraShowcase.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace GaraShowcase.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ApplicationsController : ControllerBase
    {
        private readonly GaraDbContext _context;

        public class ApplyRequest
        {
            public Guid StudentId { get; set; }
            public Guid JobId { get; set; }
        }

        public class ReviewRequest
        {
            public string Status { get; set; } = string.Empty; // Approved, Rejected
        }

        public ApplicationsController(GaraDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> Apply([FromBody] ApplyRequest request)
        {
            if (request.StudentId == Guid.Empty || request.JobId == Guid.Empty)
            {
                return BadRequest(new { error = "Thiếu thông tin sinh viên hoặc tin tuyển dụng." });
            }

            try
            {
                // 1. Verify user exists
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == request.StudentId);
                if (user == null)
                {
                    return NotFound(new { error = "Không tìm thấy thông tin sinh viên." });
                }

                // Enforce BR-04 Exception: Must have completed resume profile
                if (string.IsNullOrEmpty(user.ContactLink) || string.IsNullOrEmpty(user.CvUrl))
                {
                    return BadRequest(new { 
                        error = "Bạn cần hoàn thiện hồ sơ sinh viên (cập nhật liên kết liên hệ và tệp CV PDF) trước khi ứng tuyển." 
                    });
                }

                // 2. Verify job exists and is open
                var job = await _context.Jobs.Include(j => j.Project).FirstOrDefaultAsync(j => j.Id == request.JobId && j.Status == "Open");
                if (job == null)
                {
                    return BadRequest(new { error = "Công việc này hiện không còn nhận đơn ứng tuyển." });
                }

                if (job.Project!.Status == "Suspended")
                {
                    return BadRequest(new { error = "Dự án này đã bị Tạm ngưng (Suspended), không thể nộp đơn." });
                }

                // 3. Enforce Lockout (BR-06)
                var now = DateTime.UtcNow;
                var activeLockout = await _context.ApplicationLockouts
                    .FirstOrDefaultAsync(l => l.StudentId == request.StudentId && l.JobId == request.JobId && l.LockedUntil > now);

                if (activeLockout != null)
                {
                    var daysRemaining = (activeLockout.LockedUntil - now).TotalDays;
                    return BadRequest(new { 
                        error = $"Bạn đang bị tạm dừng nộp đơn vào vị trí này do bị từ chối trước đó. (Còn lại: {daysRemaining:F0} ngày)" 
                    });
                }

                // 4. Enforce Concurrency Limit (BR-05)
                var pendingCount = await _context.Applications
                    .Where(a => a.StudentId == request.StudentId && a.Status == "Pending")
                    .CountAsync();

                if (pendingCount >= 3)
                {
                    return BadRequest(new { 
                        error = "Bạn đã đạt giới hạn tối đa 3 đơn ứng tuyển đang chờ duyệt đồng thời." 
                    });
                }

                // Check duplicate application
                var duplicate = await _context.Applications
                    .FirstOrDefaultAsync(a => a.StudentId == request.StudentId && a.JobId == request.JobId && (a.Status == "Pending" || a.Status == "Approved"));

                if (duplicate != null)
                {
                    return BadRequest(new { error = "Bạn đã nộp đơn ứng tuyển cho vai trò này trước đó rồi." });
                }

                // 5. Save Application
                var newApplication = new Application
                {
                    Id = Guid.NewGuid(),
                    StudentId = request.StudentId,
                    JobId = request.JobId,
                    Status = "Pending",
                    CreatedAt = DateTime.UtcNow
                };

                _context.Applications.Add(newApplication);
                await _context.SaveChangesAsync();

                return Created("", new { message = "Nộp đơn ứng tuyển thành công.", application = newApplication });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Lỗi nộp đơn ứng tuyển: " + ex.Message });
            }
        }

        [HttpGet("student/{studentId}")]
        public async Task<IActionResult> GetStudentApplications(Guid studentId)
        {
            var applications = await _context.Applications
                .Include(a => a.Job)
                .ThenInclude(j => j!.Project)
                .Where(a => a.StudentId == studentId)
                .Select(a => new {
                    a.Id,
                    application_status = a.Status,
                    a.CreatedAt,
                    job_title = a.Job!.Title,
                    project_name = a.Job.Project!.Name
                })
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();

            return Ok(applications);
        }

        [HttpGet("project/{projectId}")]
        public async Task<IActionResult> GetProjectApplications(Guid projectId)
        {
            var roleClaim = User.FindFirst(ClaimTypes.Role);
            if (roleClaim?.Value != "Founder" && roleClaim?.Value != "Manager")
            {
                return StatusCode(403, new { error = "Bạn không có quyền truy cập dữ liệu ứng viên." });
            }

            var applications = await _context.Applications
                .Include(a => a.Job)
                .Include(a => a.Student)
                .Where(a => a.Job!.ProjectId == projectId)
                .Select(a => new {
                    a.Id,
                    a.Status,
                    a.CreatedAt,
                    job_title = a.Job!.Title,
                    student_name = a.Student!.Name,
                    student_id = a.Student.StudentId,
                    student_email = a.Student.Email,
                    student_contact = a.Student.ContactLink,
                    student_cv = a.Student.CvUrl
                })
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();

            return Ok(applications);
        }

        [HttpPatch("{id}/status")]
        public async Task<IActionResult> ReviewApplication(Guid id, [FromBody] ReviewRequest request)
        {
            if (request.Status != "Approved" && request.Status != "Rejected")
            {
                return BadRequest(new { error = "Trạng thái phê duyệt không hợp lệ." });
            }

            var application = await _context.Applications
                .Include(a => a.Job)
                .Include(a => a.Student)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (application == null)
            {
                return NotFound(new { error = "Không tìm thấy đơn ứng tuyển." });
            }

            var roleClaim = User.FindFirst(ClaimTypes.Role);
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            var currentUserId = Guid.Parse(userIdClaim!.Value);

            if (roleClaim?.Value != "Manager")
            {
                var isFounderOwner = await _context.Users.AnyAsync(u => u.Id == currentUserId && u.ProjectId == application.Job!.ProjectId);
                if (!isFounderOwner)
                {
                    return StatusCode(403, new { error = "Bạn không có quyền phê duyệt hồ sơ cho dự án này." });
                }
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                application.Status = request.Status;

                if (request.Status == "Rejected")
                {
                    var lockout = new ApplicationLockout
                    {
                        Id = Guid.NewGuid(),
                        StudentId = application.StudentId,
                        JobId = application.JobId,
                        LockedUntil = DateTime.UtcNow.AddDays(30),
                        CreatedAt = DateTime.UtcNow
                    };
                    _context.ApplicationLockouts.Add(lockout);
                }
                else if (request.Status == "Approved")
                {
                    application.Student!.ProjectId = application.Job!.ProjectId;
                    application.Student.UpdatedAt = DateTime.UtcNow;
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(new { message = $"Đã {(request.Status == "Approved" ? "chấp thuận" : "từ chối")} ứng viên thành công." });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, new { error = "Lỗi xử lý duyệt đơn: " + ex.Message });
            }
        }
    }
}
