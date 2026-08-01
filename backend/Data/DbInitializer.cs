using GaraShowcase.Api.Models;
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

            // Reset database to ensure clean single-project setup
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

            // 2. Seed Users & Team Placements
            var users = new User[]
            {
                // Founder Hỷ Minh Phát (owns Gara Showcase)
                new User
                {
                    Id = Guid.Parse("fca55711-bccf-4326-ad39-e83c89c516f9"),
                    Email = "phathmse184629@fpt.edu.vn",
                    Name = "Hỷ Minh Phát",
                    Role = "Founder",
                    StudentId = "SE184629",
                    ContactLink = "https://facebook.com/hyminhphat",
                    CvUrl = "https://crgarashowcasedev.blob.core.windows.net/cv/se184629_cv.pdf",
                    ProjectId = projectIdGara
                },

                // Team Member Trịnh Hải Đức (already in Gara Showcase team roster)
                new User
                {
                    Id = Guid.Parse("dca55711-bccf-4326-ad39-e83c89c516f9"),
                    Email = "ducthse184622@fpt.edu.vn",
                    Name = "Trịnh Hải Đức",
                    Role = "Student",
                    StudentId = "SE184622",
                    ContactLink = "https://facebook.com/ducth",
                    CvUrl = "https://crgarashowcasedev.blob.core.windows.net/cv/se184622_cv.pdf",
                    ProjectId = projectIdGara
                },

                // Team Member Phan Quới An Phú (already in Gara Showcase team roster)
                new User
                {
                    Id = Guid.Parse("hca55711-bccf-4326-ad39-e83c89c516f9"),
                    Email = "phupqase180573@fpt.edu.vn",
                    Name = "Phan Quới An Phú",
                    Role = "Student",
                    StudentId = "SE180573",
                    ContactLink = "https://facebook.com/anphu",
                    CvUrl = "https://crgarashowcasedev.blob.core.windows.net/cv/se180573_cv.pdf",
                    ProjectId = projectIdGara
                },

                // Student Job Seeker Lê Tuấn Khanh (Ready to apply and join Gara team)
                new User
                {
                    Id = Guid.Parse("cca55711-bccf-4326-ad39-e83c89c516f9"),
                    Email = "khanhltse184638@fpt.edu.vn",
                    Name = "Lê Tuấn Khanh",
                    Role = "Student",
                    StudentId = "SE184638",
                    ContactLink = "https://facebook.com/khanhlt",
                    CvUrl = "https://crgarashowcasedev.blob.core.windows.net/cv/se184638_cv.pdf"
                },

                // Manager
                new User
                {
                    Id = Guid.Parse("mca55711-bccf-4326-ad39-e83c89c516f9"),
                    Email = "manager.mock@fpt.edu.vn",
                    Name = "Vườn Ươm Gara Manager",
                    Role = "Manager",
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
            var jobs = new Job[]
            {
                new Job
                {
                    Id = Guid.Parse("1ca55711-bccf-4326-ad39-e83c89c516f9"),
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
                    Id = Guid.NewGuid(),
                    ProjectId = projectIdGara,
                    Title = "UI/UX Designer (Chuyên về chuyển động)",
                    Category = "Design",
                    Description = "Thiết kế giao diện kính mờ cao cấp và các tương tác micro-animations mượt mà cho nền tảng Gara.",
                    Requirements = "Thành thạo Figma, tư duy thẩm mỹ hiện đại, hiểu biết về CSS Transitions.",
                    Status = "Open",
                    CreatedAt = DateTime.UtcNow
                }
            };

            context.Jobs.AddRange(jobs);
            context.SaveChanges();
        }
    }
}
