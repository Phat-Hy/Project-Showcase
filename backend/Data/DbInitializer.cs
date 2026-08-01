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

            // Clear existing seed data to force refresh if needed (Optional, but let's overwrite if we want new data)
            // For safety, let's check if the expanded seed dataset is already loaded
            if (context.Projects.Count() > 2)
            {
                return; // Already populated with active datasets
            }

            // Remove smaller previous seeds to prevent unique constraints conflicts during update
            if (context.Projects.Any())
            {
                context.Applications.RemoveRange(context.Applications);
                context.ApplicationLockouts.RemoveRange(context.ApplicationLockouts);
                context.Jobs.RemoveRange(context.Jobs);
                context.Milestones.RemoveRange(context.Milestones);
                context.Users.RemoveRange(context.Users);
                context.Projects.RemoveRange(context.Projects);
                context.SaveChanges();
            }

            // 1. Seed Projects (Active, Draft, At-Risk, Suspended)
            var projectIdGara = Guid.Parse("bea55711-bccf-4326-ad39-e83c89c516f9");
            var projectIdEdu = Guid.Parse("cea55711-bccf-4326-ad39-e83c89c516f9");
            var projectIdZodiac = Guid.Parse("dea55711-bccf-4326-ad39-e83c89c516f9");
            var projectIdGreen = Guid.Parse("eea55711-bccf-4326-ad39-e83c89c516f9");
            var projectIdBook = Guid.Parse("fea55711-bccf-4326-ad39-e83c89c516f9");
            var projectIdCrypto = Guid.Parse("9ea55711-bccf-4326-ad39-e83c89c516f9");

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
                    Description = "# EduLink Platform\nHệ thống điểm danh tiện lợi, lưu trữ lịch sử lên lớp và tương tác bài học trực quan dựa trên phần cứng NFC kết nối đám mây.",
                    Status = "Active",
                    StorageUsedBytes = 1205421,
                    LastUpdatedAt = DateTime.UtcNow
                },
                new Project
                {
                    Id = projectIdZodiac,
                    Name = "Zodiac Tarot AI Broker",
                    Pitch = "Hệ thống phân tích bản đồ sao và tư vấn tử vi tự động dựa trên mô hình ngôn ngữ lớn (LLM).",
                    Description = "# Zodiac Tarot AI Broker\nDịch vụ cung cấp thông tin giải mã chiêm tinh học cá nhân hóa thông qua chatbot tích hợp mô hình AI Gemini.",
                    Status = "Active",
                    StorageUsedBytes = 4521400,
                    LastUpdatedAt = DateTime.UtcNow
                },
                new Project
                {
                    Id = projectIdGreen,
                    Name = "GreenCycle Hub",
                    Pitch = "Nền tảng thu gom rác thải tái chế tích điểm đổi quà tại các khu học xá đại học.",
                    Description = "# GreenCycle Hub\nGiải pháp thúc đẩy lối sống xanh tại trường học bằng mô hình số hóa điểm thưởng và kết nối các đối tác tái chế ngoại vi.",
                    Status = "Draft", // New Project in Vetting Queue
                    StorageUsedBytes = 0,
                    LastUpdatedAt = DateTime.UtcNow
                },
                new Project
                {
                    Id = projectIdBook,
                    Name = "FPT BookShare",
                    Pitch = "Sàn giao dịch và trao đổi giáo trình học thuật cũ dành riêng cho sinh viên học xá.",
                    Description = "# FPT BookShare\nKênh kết nối mua bán giáo trình đã qua sử dụng, giảm thiểu chi phí tài liệu học tập cho tân sinh viên.",
                    Status = "At-Risk", // No updates in 15 days
                    StorageUsedBytes = 3524102,
                    LastUpdatedAt = DateTime.UtcNow.AddDays(-15)
                },
                new Project
                {
                    Id = projectIdCrypto,
                    Name = "CryptoPay University",
                    Pitch = "Cổng thanh toán học phí bằng stablecoin thử nghiệm nội bộ dành cho ngành Fintech.",
                    Description = "# CryptoPay University\nĐề án nghiên cứu tích hợp ví Web3 và giao thức thanh toán học phí nhanh chóng.",
                    Status = "Suspended", // Suspended project (BR-08)
                    StorageUsedBytes = 12542100,
                    LastUpdatedAt = DateTime.UtcNow.AddDays(-35)
                }
            };

            context.Projects.AddRange(projects);
            context.SaveChanges();

            // 2. Seed Users & Team Placements
            var users = new User[]
            {
                // Founders
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
                    ProjectId = projectIdEdu
                },
                new User
                {
                    Id = Guid.Parse("hca55711-bccf-4326-ad39-e83c89c516f9"),
                    Email = "hoanglmse181234@fpt.edu.vn",
                    Name = "Lê Minh Hoàng",
                    Role = "Founder",
                    StudentId = "SE181234",
                    ContactLink = "https://facebook.com/hoanglm",
                    CvUrl = "https://crgarashowcasedev.blob.core.windows.net/cv/se181234_cv.pdf",
                    ProjectId = projectIdZodiac
                },
                new User
                {
                    Id = Guid.Parse("vca55711-bccf-4326-ad39-e83c89c516f9"),
                    Email = "vyntse182233@fpt.edu.vn",
                    Name = "Trương Ngọc Vy",
                    Role = "Founder",
                    StudentId = "GD182233",
                    ContactLink = "https://facebook.com/vynt",
                    CvUrl = "https://crgarashowcasedev.blob.core.windows.net/cv/gd182233_cv.pdf",
                    ProjectId = projectIdGreen
                },

                // Active Team Placements (Students placed in startup rosters)
                new User
                {
                    Id = Guid.Parse("cca55711-bccf-4326-ad39-e83c89c516f9"),
                    Email = "student.mock@fpt.edu.vn",
                    Name = "Nguyễn Văn A",
                    Role = "Student",
                    StudentId = "SE189999",
                    ContactLink = "https://facebook.com/studenta",
                    CvUrl = "https://crgarashowcasedev.blob.core.windows.net/cv/se189999_cv.pdf",
                    ProjectId = projectIdEdu // Placed in EduLink team!
                },
                new User
                {
                    Id = Guid.Parse("bca55711-bccf-4326-ad39-e83c89c516f9"),
                    Email = "binhbtse185566@fpt.edu.vn",
                    Name = "Bùi Thanh Bình",
                    Role = "Student",
                    StudentId = "SE185566",
                    ContactLink = "https://facebook.com/binhbt",
                    CvUrl = "https://crgarashowcasedev.blob.core.windows.net/cv/se185566_cv.pdf",
                    ProjectId = projectIdGara // Placed in Gara team!
                },

                // Job Seekers
                new User
                {
                    Id = Guid.Parse("lca55711-bccf-4326-ad39-e83c89c516f9"),
                    Email = "linhptse186677@fpt.edu.vn",
                    Name = "Phạm Thu Linh",
                    Role = "Student",
                    StudentId = "GD186677",
                    ContactLink = "https://facebook.com/linhpt",
                    CvUrl = "https://crgarashowcasedev.blob.core.windows.net/cv/gd186677_cv.pdf"
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

            // 3. Seed Milestones
            var milestones = new Milestone[]
            {
                // Gara Showcase milestones
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
                    Title = "Tích hợp luồng upload và CSDL hoàn chỉnh",
                    Description = "Phát triển bộ máy C# API gieo mầm dữ liệu, kiểm tra giới hạn 10MB và cấm ứng tuyển 30 ngày.",
                    Done = false,
                    CreatedAt = DateTime.UtcNow
                },

                // EduLink milestones
                new Milestone
                {
                    Id = Guid.NewGuid(),
                    ProjectId = projectIdEdu,
                    Title = "Thiết kế thành công bo mạch thu sóng NFC",
                    Description = "Hoàn thiện sơ đồ nguyên lý và in thử nghiệm bo mạch phần cứng.",
                    Done = true,
                    DateCompleted = DateTime.UtcNow.AddDays(-40),
                    CreatedAt = DateTime.UtcNow.AddDays(-50)
                },
                new Milestone
                {
                    Id = Guid.NewGuid(),
                    ProjectId = projectIdEdu,
                    Title = "Tích hợp API điểm danh vào hệ thống trường học",
                    Description = "Kết nối dịch vụ đọc thẻ NFC với cơ sở dữ liệu học tập tập trung.",
                    Done = false,
                    CreatedAt = DateTime.UtcNow
                },

                // Zodiac Tarot AI milestones
                new Milestone
                {
                    Id = Guid.NewGuid(),
                    ProjectId = projectIdZodiac,
                    Title = "Huấn luyện Prompt mẫu chiêm tinh cho Gemini",
                    Description = "Cấu hình system instructions và tinh chỉnh tham số sinh từ của chatbot chiêm tinh.",
                    Done = true,
                    DateCompleted = DateTime.UtcNow.AddDays(-5),
                    CreatedAt = DateTime.UtcNow.AddDays(-8)
                }
            };

            context.Milestones.AddRange(milestones);
            context.SaveChanges();

            // 4. Seed Jobs
            var jobs = new Job[]
            {
                // Gara Showcase Jobs
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
                },

                // EduLink Jobs
                new Job
                {
                    Id = Guid.NewGuid(),
                    ProjectId = projectIdEdu,
                    Title = "Chuyên viên Phát triển Thị trường",
                    Category = "Business",
                    Description = "Tìm kiếm các đối tác trường tiểu học và trung học thử nghiệm sản phẩm điểm danh thông minh.",
                    Requirements = "Kỹ năng thuyết trình xuất sắc, năng động, đam mê công nghệ giáo dục.",
                    Status = "Open",
                    CreatedAt = DateTime.UtcNow
                },

                // Zodiac Tarot AI Jobs
                new Job
                {
                    Id = Guid.NewGuid(),
                    ProjectId = projectIdZodiac,
                    Title = "Lập trình viên React Native (App di động)",
                    Category = "Engineering",
                    Description = "Xây dựng ứng dụng di động đa nền tảng kết nối API giải mã chiêm tinh học.",
                    Requirements = "Thành thạo Javascript/Typescript, React Native, có kinh nghiệm đẩy app lên Store.",
                    Status = "Open",
                    CreatedAt = DateTime.UtcNow
                },

                // GreenCycle Hub Jobs
                new Job
                {
                    Id = Guid.NewGuid(),
                    ProjectId = projectIdGreen,
                    Title = "Cộng tác viên truyền thông nội bộ",
                    Category = "Marketing",
                    Description = "Sáng tạo nội dung quảng bá chiến dịch thu gom rác thải đổi quà tại trường học.",
                    Requirements = "Kỹ năng viết lách tốt, có khả năng thiết kế Canva hoặc chỉnh sửa video ngắn.",
                    Status = "Open",
                    CreatedAt = DateTime.UtcNow
                }
            };

            context.Jobs.AddRange(jobs);
            context.SaveChanges();
        }
    }
}
