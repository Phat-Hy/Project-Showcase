using GaraShowcase.Api.Models;
using GaraShowcase.Api.Utils;
using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace GaraShowcase.Api.Data
{
    public static class DbInitializer
    {
        public static void Initialize(GaraDbContext context)
        {
            // Run automatic database migration
            context.Database.Migrate();

            // Reset database to ensure clean setup
            context.Applications.RemoveRange(context.Applications);
            context.ApplicationLockouts.RemoveRange(context.ApplicationLockouts);
            context.Jobs.RemoveRange(context.Jobs);
            context.Milestones.RemoveRange(context.Milestones);
            context.Users.RemoveRange(context.Users);
            context.Projects.RemoveRange(context.Projects);
            context.SaveChanges();

            // 1. Seed ONLY Gara Startup Project Showcase
            var projectIdGara = Guid.Parse("bea55711-bccf-4326-ad39-e83c89c516f9");

            var project = new Project
            {
                Id = projectIdGara,
                Name = "Gara Startup Project Showcase",
                Pitch = "Nền tảng quản lý danh mục khởi nghiệp và tuyển dụng thành viên liên khoa cho trường đại học.",
                Description = "# Gara Startup Showcase\nNền tảng kết nối sinh viên thuộc các khối ngành kỹ thuật (SE), thiết kế (GD) và kinh doanh (Biz) dưới sự quản lý của vườn ươm khởi nghiệp trường đại học.",
                Status = "Active",
                StorageUsedBytes = 452140,
                LastUpdatedAt = DateTime.UtcNow
            };

            context.Projects.Add(project);
            context.SaveChanges();

            // Default hashed password (password123)
            var defaultPasswordHash = PasswordHasher.HashPassword("password123");

            // 2. Seed Users (Real Team Members + Additional Mock Students)
            var userPhatId = Guid.Parse("fca55711-bccf-4326-ad39-e83c89c516f9");
            var userDucId = Guid.Parse("dca55711-bccf-4326-ad39-e83c89c516f9");
            var userPhuId = Guid.Parse("hca55711-bccf-4326-ad39-e83c89c516f9");
            var userKhanhId = Guid.Parse("cca55711-bccf-4326-ad39-e83c89c516f9");
            
            var userBinhId = Guid.Parse("bca55711-bccf-4326-ad39-e83c89c516f9");
            var userLinhId = Guid.Parse("lca55711-bccf-4326-ad39-e83c89c516f9");
            var userHoangId = Guid.Parse("8ca55711-bccf-4326-ad39-e83c89c516f9");
            
            var userMockStudent1 = Guid.Parse("5ca55711-bccf-4326-ad39-e83c89c516f9");
            var userMockStudent2 = Guid.Parse("6ca55711-bccf-4326-ad39-e83c89c516f9");

            var users = new User[]
            {
                // Founder (Hỷ Minh Phát)
                new User
                {
                    Id = userPhatId,
                    Email = "phathmse184629@fpt.edu.vn",
                    Name = "Hỷ Minh Phát",
                    Role = "Founder",
                    PasswordHash = defaultPasswordHash,
                    StudentId = "SE184629",
                    ContactLink = "https://facebook.com/hyminhphat",
                    CvUrl = "https://crgarashowcasedev.blob.core.windows.net/cv/se184629_cv.pdf",
                    ProjectId = projectIdGara
                },

                // Team Members (Already in the Gara Showcase project roster)
                new User
                {
                    Id = userDucId,
                    Email = "ducthse184622@fpt.edu.vn",
                    Name = "Trịnh Hải Đức",
                    Role = "Student",
                    PasswordHash = defaultPasswordHash,
                    StudentId = "SE184622",
                    ContactLink = "https://facebook.com/ducth",
                    CvUrl = "https://crgarashowcasedev.blob.core.windows.net/cv/se184622_cv.pdf",
                    ProjectId = projectIdGara
                },
                new User
                {
                    Id = userPhuId,
                    Email = "phupqase180573@fpt.edu.vn",
                    Name = "Phan Quới An Phú",
                    Role = "Student",
                    PasswordHash = defaultPasswordHash,
                    StudentId = "SE180573",
                    ContactLink = "https://facebook.com/anphu",
                    CvUrl = "https://crgarashowcasedev.blob.core.windows.net/cv/se180573_cv.pdf",
                    ProjectId = projectIdGara
                },
                new User
                {
                    Id = userMockStudent1,
                    Email = "student.b@fpt.edu.vn",
                    Name = "Nguyễn Văn B (Mock GD)",
                    Role = "Student",
                    PasswordHash = defaultPasswordHash,
                    StudentId = "GD180002",
                    ContactLink = "https://facebook.com/studentb",
                    CvUrl = "https://crgarashowcasedev.blob.core.windows.net/cv/gd180002_cv.pdf",
                    ProjectId = projectIdGara
                },
                new User
                {
                    Id = userMockStudent2,
                    Email = "student.c@fpt.edu.vn",
                    Name = "Trần Thị C (Mock Biz)",
                    Role = "Student",
                    PasswordHash = defaultPasswordHash,
                    StudentId = "BA180003",
                    ContactLink = "https://facebook.com/studentc",
                    CvUrl = "https://crgarashowcasedev.blob.core.windows.net/cv/ba180003_cv.pdf",
                    ProjectId = projectIdGara
                },

                // Student Job Seekers (Ready to apply / test applications)
                new User
                {
                    Id = userKhanhId,
                    Email = "khanhltse184638@fpt.edu.vn",
                    Name = "Lê Tuấn Khanh",
                    Role = "Student",
                    PasswordHash = defaultPasswordHash,
                    StudentId = "SE184638",
                    ContactLink = "https://facebook.com/khanhlt",
                    CvUrl = "https://crgarashowcasedev.blob.core.windows.net/cv/se184638_cv.pdf"
                },
                new User
                {
                    Id = userBinhId,
                    Email = "binhbtse185566@fpt.edu.vn",
                    Name = "Bùi Thanh Bình (Mock Seeker)",
                    Role = "Student",
                    PasswordHash = defaultPasswordHash,
                    StudentId = "SE185566",
                    ContactLink = "https://facebook.com/binhbt",
                    CvUrl = "https://crgarashowcasedev.blob.core.windows.net/cv/se185566_cv.pdf"
                },
                new User
                {
                    Id = userLinhId,
                    Email = "linhptse186677@fpt.edu.vn",
                    Name = "Phạm Thu Linh (Mock Seeker)",
                    Role = "Student",
                    PasswordHash = defaultPasswordHash,
                    StudentId = "GD186677",
                    ContactLink = "https://facebook.com/linhpt",
                    CvUrl = "https://crgarashowcasedev.blob.core.windows.net/cv/gd186677_cv.pdf"
                },
                new User
                {
                    Id = userHoangId,
                    Email = "hoanglmse181234@fpt.edu.vn",
                    Name = "Lê Minh Hoàng (Mock Seeker)",
                    Role = "Student",
                    PasswordHash = defaultPasswordHash,
                    StudentId = "SE181234",
                    ContactLink = "https://facebook.com/hoanglm",
                    CvUrl = "https://crgarashowcasedev.blob.core.windows.net/cv/se181234_cv.pdf"
                },

                // Manager
                new User
                {
                    Id = Guid.Parse("mca55711-bccf-4326-ad39-e83c89c516f9"),
                    Email = "manager.mock@fpt.edu.vn",
                    Name = "Vườn Ươm Gara Manager",
                    Role = "Manager",
                    PasswordHash = defaultPasswordHash,
                    StudentId = null,
                    ContactLink = "https://gara.edu.vn",
                    CvUrl = null
                }
            };

            context.Users.AddRange(users);
            context.SaveChanges();

            // 3. Seed Gara Milestones
            var milestones = new Milestone[]
            {
                new Milestone
                {
                    Id = Guid.NewGuid(),
                    ProjectId = projectIdGara,
                    Title = "Hoàn thiện bản Slide Pitching MVP",
                    Description = "Thiết kế giao diện slide deck dạng SPA và chạy thử bản mô phỏng MVP.",
                    Done = true,
                    DateCompleted = DateTime.UtcNow.AddDays(-20),
                    CreatedAt = DateTime.UtcNow.AddDays(-30)
                },
                new Milestone
                {
                    Id = Guid.NewGuid(),
                    ProjectId = projectIdGara,
                    Title = "Phân rã hệ thống và tích hợp cổng Auth",
                    Description = "Phân rã Slide Deck và xây dựng các Cổng Sinh Viên, Sáng Lập và Quản Trị độc lập kết nối API.",
                    Done = true,
                    DateCompleted = DateTime.UtcNow.AddDays(-1),
                    CreatedAt = DateTime.UtcNow.AddDays(-10)
                },
                new Milestone
                {
                    Id = Guid.NewGuid(),
                    ProjectId = projectIdGara,
                    Title = "Tích hợp C# Web API & React Monolith",
                    Description = "Di chuyển mã nguồn sang ASP.NET Core, kết nối cơ sở dữ liệu và triển khai lên Azure Container App.",
                    Done = false,
                    CreatedAt = DateTime.UtcNow
                }
            };

            context.Milestones.AddRange(milestones);
            context.SaveChanges();

            // 4. Seed Gara Jobs
            var jobIdFullstack = Guid.Parse("1ca55711-bccf-4326-ad39-e83c89c516f9");
            var jobIdDesigner = Guid.Parse("2ca55711-bccf-4326-ad39-e83c89c516f9");
            var jobIdBizDev = Guid.Parse("3ca55711-bccf-4326-ad39-e83c89c516f9");

            var jobs = new Job[]
            {
                new Job
                {
                    Id = jobIdFullstack,
                    ProjectId = projectIdGara,
                    Title = "Kỹ sư Fullstack (.NET 9 + React)",
                    Category = "Engineering",
                    Description = "Phát triển các mô đun tuyển dụng và tải tài liệu trực quan trực tiếp lên đám mây Azure.",
                    Requirements = "Thành thạo lập trình C# ASP.NET Core, React TypeScript và sử dụng Git.",
                    Status = "Open",
                    CreatedAt = DateTime.UtcNow
                },
                new Job
                {
                    Id = jobIdDesigner,
                    ProjectId = projectIdGara,
                    Title = "UI/UX Designer (Chuyên về chuyển động)",
                    Category = "Design",
                    Description = "Thiết kế giao diện kính mờ cao cấp và các tương tác micro-animations mượt mà cho nền tảng Gara.",
                    Requirements = "Thành thạo Figma, tư duy thẩm mỹ hiện đại, hiểu biết về CSS Transitions.",
                    Status = "Open",
                    CreatedAt = DateTime.UtcNow
                },
                new Job
                {
                    Id = jobIdBizDev,
                    ProjectId = projectIdGara,
                    Title = "Chuyên viên Phát triển Thị trường (Biz)",
                    Category = "Business",
                    Description = "Tìm kiếm các đối tác trường học thực hiện thử nghiệm nền tảng tuyển dụng.",
                    Requirements = "Kỹ năng giao tiếp tốt, đam mê môi trường khởi nghiệp giáo dục.",
                    Status = "Open",
                    CreatedAt = DateTime.UtcNow
                }
            };

            context.Jobs.AddRange(jobs);
            context.SaveChanges();

            // 5. Seed Applications (To show pending applicant dashboard)
            var applications = new Application[]
            {
                new Application
                {
                    Id = Guid.NewGuid(),
                    StudentId = userBinhId,
                    JobId = jobIdFullstack,
                    CoverLetter = "Chào nhóm Gara, mình là Bình, mình có kinh nghiệm làm các project C# API học tập và muốn ứng tuyển vị trí Fullstack.",
                    Status = "Pending",
                    CreatedAt = DateTime.UtcNow.AddDays(-2)
                },
                new Application
                {
                    Id = Guid.NewGuid(),
                    StudentId = userLinhId,
                    JobId = jobIdDesigner,
                    CoverLetter = "Xin chào, mình chuyên về thiết kế UI dạng tối giản (minimalism) và có thể phụ trách thiết kế slide deck pitching cho dự án.",
                    Status = "Pending",
                    CreatedAt = DateTime.UtcNow.AddDays(-1)
                },
                new Application
                {
                    Id = Guid.NewGuid(),
                    StudentId = userHoangId,
                    JobId = jobIdBizDev,
                    CoverLetter = "Mình muốn ứng tuyển để giúp nhóm tìm kiếm đối tác và làm báo cáo tài chính.",
                    Status = "Rejected", // Seeded as Rejected to enforce Lockout testing
                    CreatedAt = DateTime.UtcNow.AddDays(-5)
                }
            };

            context.Applications.AddRange(applications);
            context.SaveChanges();

            // Add lockout record for Hoàng to test BR-06 validation
            var lockout = new ApplicationLockout
            {
                Id = Guid.NewGuid(),
                StudentId = userHoangId,
                JobId = jobIdBizDev,
                LockedUntil = DateTime.UtcNow.AddDays(25) // Locked for 25 more days
            };
            
            context.ApplicationLockouts.Add(lockout);
            context.SaveChanges();
        }
    }
}
