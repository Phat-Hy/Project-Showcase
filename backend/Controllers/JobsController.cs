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
    public class JobsController : ControllerBase
    {
        private readonly GaraDbContext _context;

        public JobsController(GaraDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetOpenJobs()
        {
            var jobs = await _context.Jobs
                .Include(j => j.Project)
                .Where(j => j.Status == "Open" && j.Project!.Status != "Suspended")
                .OrderByDescending(j => j.CreatedAt)
                .ToListAsync();

            return Ok(jobs);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Job job)
        {
            var roleClaim = User.FindFirst(ClaimTypes.Role);
            if (roleClaim?.Value != "Founder" && roleClaim?.Value != "Manager")
            {
                return StatusCode(403, new { error = "Bạn không có quyền đăng tin tuyển dụng." });
            }

            if (string.IsNullOrEmpty(job.Title) || string.IsNullOrEmpty(job.Description))
            {
                return BadRequest(new { error = "Tiêu đề và mô tả tuyển dụng là bắt buộc." });
            }

            job.Id = Guid.NewGuid();
            job.Status = "Open";
            job.CreatedAt = DateTime.UtcNow;

            _context.Jobs.Add(job);
            await _context.SaveChangesAsync();

            return Created("", job);
        }
    }
}
