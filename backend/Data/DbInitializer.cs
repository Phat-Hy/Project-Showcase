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

            // 1. Seed Startup Projects (6 distinct projects in different states)
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
                    Name = "EduLink NFC",
                    Pitch = "Thẻ học sinh thông minh tích hợp chip NFC giúp điểm danh nhanh và thanh toán học phí.",
                    Description = "# EduLink NFC Card\nHệ thống thẻ thông minh ứng dụng NFC giúp quản lý học sinh và liên lạc giữa phụ huynh và trường học thời gian thực.",
                    Status = "Active",
                    StorageUsedBytes = 2309100,
                    LastUpdatedAt = DateTime.UtcNow
                },
                new Project
                {
                    Id = projectIdZodiac,
                    Name = "Zodiac Tarot AI",
                    Pitch = "Ứng dụng phân tích bản đồ sao cá nhân và giải mã bài Tarot bằng công nghệ AI sinh ngẫu nhiên.",
                    Description = "# Zodiac Tarot AI Engine\nSử dụng các mô hình ngôn ngữ lớn để diễn giải thông điệp chiêm tinh học và Tarot được cá nhân hóa cao cho người dùng trẻ.",
                    Status = "Active",
                    StorageUsedBytes = 15998200,
                    LastUpdatedAt = DateTime.UtcNow
                },
                new Project
                {
                    Id = projectIdGreen,
                    Name = "GreenCycle IoT",
                    Pitch = "Hệ thống thùng rác thông minh tự phân loại rác thải nhựa tích hợp cảm biến IoT.",
                    Description = "# GreenCycle IoT Network\nPhát triển mạng lưới thùng rác công cộng thông minh tự động nén rác và gửi báo cáo đầy rác về trung tâm quản lý đô thị.",
                    Status = "Draft",
                    StorageUsedBytes = 0,
                    LastUpdatedAt = DateTime.UtcNow
                },
                new Project
                {
                    Id = projectIdBook,
                    Name = "BookHub Sharing",
                    Pitch = "Nền tảng trao đổi sách giáo khoa và tài liệu học tập cũ giữa các thế hệ sinh viên trong trường.",
                    Description = "# BookHub Platform\nGiải pháp giảm thiểu chi phí mua sách và thúc đẩy văn hóa đọc xanh thông qua mạng lưới chia sẻ sách ngang hàng.",
                    Status = "At-Risk",
                    StorageUsedBytes = 120400,
                    LastUpdatedAt = DateTime.UtcNow.AddDays(-15) // > 14 days stale
                },
                new Project
                {
                    Id = projectIdCrypto,
                    Name = "CryptoPay Wallet",
                    Pitch = "Ví điện tử thanh toán nội bộ trường học bảo mật cao ứng dụng công nghệ chuỗi khối.",
                    Description = "# CryptoPay Campus Wallet\nGiải pháp giao dịch không tiền mặt an toàn, minh bạch dành cho các hoạt động ngoại khóa, căng tin và đóng quỹ lớp.",
                    Status = "Suspended",
                    StorageUsedBytes = 94100,
                    LastUpdatedAt = DateTime.UtcNow.AddDays(-32) // > 30 days stale
                }
            };

            context.Projects.AddRange(projects);
            context.SaveChanges();

            // Default hashed password (password123)
            var defaultPasswordHash = PasswordHasher.HashPassword("password123");

            // 2. Seed Users (Real Team Members + Additional Mock Accounts)
            var userPhatId = Guid.Parse("fca55711-bccf-4326-ad39-e83c89c516f9");
            var userDucId = Guid.Parse("dca55711-bccf-4326-ad39-e83c89c516f9");
            var userPhuId = Guid.Parse("aca55711-bccf-4326-ad39-e83c89c516f9");
            var userKhanhId = Guid.Parse("cca55711-bccf-4326-ad39-e83c89c516f9");
            var userVyId = Guid.Parse("7ca55711-bccf-4326-ad39-e83c89c516f9");
            
            var userBinhId = Guid.Parse("bca55711-bccf-4326-ad39-e83c89c516f9");
            var userLinhId = Guid.Parse("1ca55711-bccf-4326-ad39-e83c89c516f9");
            var userHoangId = Guid.Parse("8ca55711-bccf-4326-ad39-e83c89c516f9");
            
            var userMockStudent1 = Guid.Parse("5ca55711-bccf-4326-ad39-e83c89c516f9");
            var userMockStudent2 = Guid.Parse("6ca55711-bccf-4326-ad39-e83c89c516f9");
            
            var managerId = Guid.Parse("2ca55711-bccf-4326-ad39-e83c89c516f9");
            
            var founderEduId = Guid.Parse("3ca55711-bccf-4326-ad39-e83c89c516f9");
            var founderZodiacId = Guid.Parse("4ca55711-bccf-4326-ad39-e83c89c516f9");
            
            var founderGreenId = Guid.Parse("ad0cc535-f771-4a39-e83c-89c516f9a001");
            var founderBookId = Guid.Parse("ad0cc535-f771-4a39-e83c-89c516f9a002");
            var founderCryptoId = Guid.Parse("ad0cc535-f771-4a39-e83c-89c516f9a003");

            var users = new User[]
            {
                // Hỷ Minh Phát (Founder of Gara Showcase)
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

                // Trịnh Hải Đức (Member of Gara Showcase)
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

                // Phan Quới An Phú (Member of Gara Showcase)
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

                // Nguyễn Văn B (Member of Gara Showcase)
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

                // Trần Thị C (Member of Gara Showcase)
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

                // EduLink NFC Founder
                new User
                {
                    Id = founderEduId,
                    Email = "edulink.founder@fpt.edu.vn",
                    Name = "Trần Minh Quân (EduLink)",
                    Role = "Founder",
                    PasswordHash = defaultPasswordHash,
                    StudentId = "SE183204",
                    ContactLink = "https://facebook.com/edu.founder",
                    CvUrl = "https://crgarashowcasedev.blob.core.windows.net/cv/se183204_cv.pdf",
                    ProjectId = projectIdEdu
                },

                // Zodiac Tarot AI Founder
                new User
                {
                    Id = founderZodiacId,
                    Email = "zodiac.founder@fpt.edu.vn",
                    Name = "Võ Hoàng Yến (Zodiac)",
                    Role = "Founder",
                    PasswordHash = defaultPasswordHash,
                    StudentId = "SE185112",
                    ContactLink = "https://facebook.com/zodiac.founder",
                    CvUrl = "https://crgarashowcasedev.blob.core.windows.net/cv/se185112_cv.pdf",
                    ProjectId = projectIdZodiac
                },

                // GreenCycle IoT Founder (Draft Startup)
                new User
                {
                    Id = founderGreenId,
                    Email = "greencycle.founder@fpt.edu.vn",
                    Name = "Phạm Đình Phong (GreenCycle)",
                    Role = "Founder",
                    PasswordHash = defaultPasswordHash,
                    StudentId = "SE187531",
                    ContactLink = "https://facebook.com/green.founder",
                    CvUrl = "https://crgarashowcasedev.blob.core.windows.net/cv/se187531_cv.pdf",
                    ProjectId = projectIdGreen
                },

                // BookHub Sharing Founder (At-Risk Startup)
                new User
                {
                    Id = founderBookId,
                    Email = "bookhub.founder@fpt.edu.vn",
                    Name = "Hoàng Thanh Trúc (BookHub)",
                    Role = "Founder",
                    PasswordHash = defaultPasswordHash,
                    StudentId = "SE189912",
                    ContactLink = "https://facebook.com/book.founder",
                    CvUrl = "https://crgarashowcasedev.blob.core.windows.net/cv/se189912_cv.pdf",
                    ProjectId = projectIdBook
                },

                // CryptoPay Wallet Founder (Suspended Startup)
                new User
                {
                    Id = founderCryptoId,
                    Email = "cryptopay.founder@fpt.edu.vn",
                    Name = "Vũ Việt Anh (CryptoPay)",
                    Role = "Founder",
                    PasswordHash = defaultPasswordHash,
                    StudentId = "SE188844",
                    ContactLink = "https://facebook.com/crypto.founder",
                    CvUrl = "https://crgarashowcasedev.blob.core.windows.net/cv/se188844_cv.pdf",
                    ProjectId = projectIdCrypto
                },

                // Lê Tuấn Khanh (Student Seeker - Ready to apply)
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

                // Trương Ngọc Vy (Student Seeker - Ready to apply)
                new User
                {
                    Id = userVyId,
                    Email = "vyntse182233@fpt.edu.vn",
                    Name = "Trương Ngọc Vy",
                    Role = "Student",
                    PasswordHash = defaultPasswordHash,
                    StudentId = "SE182233",
                    ContactLink = "https://facebook.com/vynt",
                    CvUrl = "https://crgarashowcasedev.blob.core.windows.net/cv/se182233_cv.pdf"
                },

                // Bùi Thanh Bình (Student Seeker - Mock Seeker)
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

                // Phạm Thu Linh (Student Seeker - Mock Seeker)
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

                // Lê Minh Hoàng (Student Seeker - Mock Seeker with lockout)
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

                // Vườn Ươm Gara Manager (Manager Admin)
                new User
                {
                    Id = managerId,
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

            // 3. Seed Milestones (For Gara and BookHub)
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
                },
                new Milestone
                {
                    Id = Guid.NewGuid(),
                    ProjectId = projectIdBook,
                    Title = "Khởi chạy Web App BookHub Beta",
                    Description = "Sinh viên bắt đầu tạo tài khoản trao đổi sách thử nghiệm nội bộ.",
                    Done = true,
                    DateCompleted = DateTime.UtcNow.AddDays(-25),
                    CreatedAt = DateTime.UtcNow.AddDays(-35)
                }
            };

            context.Milestones.AddRange(milestones);
            context.SaveChanges();

            // 4. Seed Jobs
            var jobIdFullstack = Guid.Parse("1ca55711-bccf-4326-ad39-e83c89c516f9");
            var jobIdDesigner = Guid.Parse("2ca55711-bccf-4326-ad39-e83c89c516f9");
            var jobIdBizDev = Guid.Parse("3ca55711-bccf-4326-ad39-e83c89c516f9");
            var jobIdEduFront = Guid.Parse("4ca55711-bccf-4326-ad39-e83c89c516f9");
            var jobIdTarotAI = Guid.Parse("5ca55711-bccf-4326-ad39-e83c89c516f9");

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
                },
                new Job
                {
                    Id = jobIdEduFront,
                    ProjectId = projectIdEdu,
                    Title = "Lập trình viên Frontend (React)",
                    Category = "Engineering",
                    Description = "Phát triển cổng giao diện thanh toán NFC nội trú trường học.",
                    Requirements = "Thành thạo ReactJS, CSS Flexbox/Grid, làm việc nhóm tốt.",
                    Status = "Open",
                    CreatedAt = DateTime.UtcNow
                },
                new Job
                {
                    Id = jobIdTarotAI,
                    ProjectId = projectIdZodiac,
                    Title = "Kỹ sư AI & Python",
                    Category = "Engineering",
                    Description = "Tích hợp các mô hình phân tích bài Tarot và chiêm tinh học tự động.",
                    Requirements = "Thành thạo Python, kinh nghiệm làm việc với OpenAI API hoặc các mô hình LLM tương tự.",
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
                    Status = "Pending",
                    CreatedAt = DateTime.UtcNow.AddDays(-2)
                },
                new Application
                {
                    Id = Guid.NewGuid(),
                    StudentId = userLinhId,
                    JobId = jobIdDesigner,
                    Status = "Pending",
                    CreatedAt = DateTime.UtcNow.AddDays(-1)
                },
                new Application
                {
                    Id = Guid.NewGuid(),
                    StudentId = userHoangId,
                    JobId = jobIdBizDev,
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
