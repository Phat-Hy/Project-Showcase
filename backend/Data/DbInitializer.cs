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

            // Check if seeded
            if (context.Users.Any())
            {
                return; // Database has been seeded
            }

            // 1. Seed Projects
            var projectIdGara = Guid.Parse("bea55711-bccf-4326-ad39-e83c89c516f9");
            var projectIdEdu = Guid.Parse("cea55711-bccf-4326-ad39-e83c89c516f9");

            var projects = new Project[]
            {
                new Project
                {
                    Id = projectIdGara,
                    Name = "Gara Startup Project Showcase",
                    Pitch = "Nền tảng quản lý danh mục khởi nghiệp và tuyển dụng thành viên liên khoa cho trường đại học.",
                    Description = "# Gara Startup Showcase\nNền tảng kết nối sinh viên thuộc các khối ngành kỹ thuật (SE), thiết kế (GD) và kinh doanh (Biz) dưới sự quản lý của vườn ươm khởi nghiệp trường đại học.",
                    Status = "Active",
                    StorageUsedBytes = 452140,
                    LastUpdatedAt = DateTime.UtcNow
                },
                new Project
                {
                    Id = projectIdEdu,
                    Name = "EduLink Platform",
                    Pitch = "Hệ thống điểm danh và quản lý học tập thông minh dựa trên thẻ NFC.",
                    Description = "# EduLink Platform\nHệ thống điểm danh tiện lợi, lưu trữ lịch sử lên lớp và tương tác bài học trực quan.",
                    Status = "Active",
                    StorageUsedBytes = 1205421,
                    LastUpdatedAt = DateTime.UtcNow
                }
            };

            context.Projects.AddRange(projects);
            context.SaveChanges();

            // 2. Seed Users
            var users = new User[]
            {
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
                new User
                {
                    Id = Guid.Parse("dca55711-bccf-4326-ad39-e83c89c516f9"),
                    Email = "ducthse180000@fpt.edu.vn",
                    Name = "Trịnh Hải Đức",
                    Role = "Founder",
                    StudentId = "SE180000",
                    ContactLink = "https://facebook.com/ducth",
                    CvUrl = "https://crgarashowcasedev.blob.core.windows.net/cv/se180000_cv.pdf",
                    ProjectId = projectIdGara
                },
                new User
                {
                    Id = Guid.Parse("cca55711-bccf-4326-ad39-e83c89c516f9"),
                    Email = "student.mock@fpt.edu.vn",
                    Name = "Nguyễn Văn A",
                    Role = "Student",
                    StudentId = "SE189999",
                    ContactLink = "https://facebook.com/studenta",
                    CvUrl = "https://crgarashowcasedev.blob.core.windows.net/cv/se189999_cv.pdf"
                },
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

            // 3. Seed Milestones
            var milestones = new Milestone[]
            {
                new Milestone
                {
                    Id = Guid.NewGuid(),
                    ProjectId = projectIdGara,
                    Title = "Hoàn thiện bản Slide Pitching MVP",
                    Description = "Thiết kế giao diện slide deck dạng SPA và chạy thử bản mô phỏng MVP.",
                    Done = true,
                    DateCompleted = DateTime.UtcNow
                },
                new Milestone
                {
                    Id = Guid.NewGuid(),
                    ProjectId = projectIdGara,
                    Title = "Phân rã hệ thống và tích hợp cổng Auth",
                    Description = "Phân rã Slide Deck và xây dựng các Cổng Sinh Viên, Sáng Lập và Quản Trị độc lập kết nối API.",
                    Done = false
                }
            };

            context.Milestones.AddRange(milestones);
            context.SaveChanges();

            // 4. Seed Jobs
            var jobIdDev = Guid.Parse("1ca55711-bccf-4326-ad39-e83c89c516f9");
            var jobs = new Job[]
            {
                new Job
                {
                    Id = jobIdDev,
                    ProjectId = projectIdGara,
                    Title = "Kỹ sư Fullstack (.NET + React)",
                    Category = "Engineering",
                    Description = "Phát triển các mô đun tuyển dụng và tải tài liệu trực quan trực tiếp lên đám mây Azure.",
                    Requirements = "Thành thạo lập trình C# ASP.NET Core, React TypeScript và sử dụng Git.",
                    Status = "Open"
                },
                new Job
                {
                    Id = Guid.NewGuid(),
                    ProjectId = projectIdGara,
                    Title = "UI/UX Designer",
                    Category = "Design",
                    Description = "Thiết kế giao diện kính mờ cao cấp và các tương tác micro-animations mượt mà cho nền tảng Gara.",
                    Requirements = "Thành thạo Figma, tư duy thẩm mỹ hiện đại, hiểu biết về CSS Transitions.",
                    Status = "Open"
                }
            };

            context.Jobs.AddRange(jobs);
            context.SaveChanges();
        }
    }
}
