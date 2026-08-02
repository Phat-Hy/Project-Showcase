using GaraShowcase.Api.Models;
using GaraShowcase.Api.Utils;
using System;
using System.Collections.Generic;
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

            // 1. Seed Startup Projects (6 original + 14 generated = 20 total)
            var projectIdGara = Guid.Parse("bea55711-bccf-4326-ad39-e83c89c516f9");
            var projectIdEdu = Guid.Parse("cea55711-bccf-4326-ad39-e83c89c516f9");
            var projectIdZodiac = Guid.Parse("dea55711-bccf-4326-ad39-e83c89c516f9");
            var projectIdGreen = Guid.Parse("eea55711-bccf-4326-ad39-e83c89c516f9");
            var projectIdBook = Guid.Parse("fea55711-bccf-4326-ad39-e83c89c516f9");
            var projectIdCrypto = Guid.Parse("9ea55711-bccf-4326-ad39-e83c89c516f9");

            var projectList = new List<Project>
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
                    LastUpdatedAt = DateTime.UtcNow.AddDays(-15)
                },
                new Project
                {
                    Id = projectIdCrypto,
                    Name = "CryptoPay Wallet",
                    Pitch = "Ví điện tử thanh toán nội bộ trường học bảo mật cao ứng dụng công nghệ chuỗi khối.",
                    Description = "# CryptoPay Campus Wallet\nGiải pháp giao dịch không tiền mặt an toàn, minh bạch dành cho các hoạt động ngoại khóa, căng tin và đóng quỹ lớp.",
                    Status = "Suspended",
                    StorageUsedBytes = 94100,
                    LastUpdatedAt = DateTime.UtcNow.AddDays(-32)
                }
            };

            // Generate 14 more projects to make 20 total
            var random = new Random(42);
            string[] startupNouns = { "AgriSmart", "HealthTrack", "SolarGrid", "FoodRescue", "EduQuest", "SmartPark", "TravelBuddy", "FitPulse", "CleanWave", "AutoCV", "AquaGrow", "RentEase", "CineMatch", "SkillShare" };
            string[] startupTechs = { "Drone", "Wearable", "Monitor", "Share", "Gamified", "Finder", "AI", "Coaching", "Ocean", "Screener", "Hydroponics", "Room", "Social", "Academy" };
            string[] startupPitches = {
                "Sử dụng thiết bị bay không người lái để theo dõi sức khỏe cây trồng tự động.",
                "Thiết bị đeo theo dõi nhịp tim và nồng độ oxy trong máu cho người cao tuổi.",
                "Giải pháp quản lý điện năng mặt trời thông minh cho hộ gia đình.",
                "Kết nối các quán ăn dư thừa thực phẩm với người có nhu cầu giá rẻ.",
                "Nền tảng học tiếng Anh qua game nhập vai tương tác cao.",
                "Ứng dụng tìm kiếm điểm đỗ xe trống và đặt chỗ trước thời gian thực.",
                "Trợ lý du lịch cá nhân hóa gợi ý lịch trình tự động bằng AI.",
                "Huấn luyện viên thể hình ảo theo dõi động tác qua camera điện thoại.",
                "Thu gom và tái chế rác thải nhựa đại dương sử dụng robot tự hành.",
                "Hệ thống lọc và chấm điểm hồ sơ xin việc tự động cho HR.",
                "Trồng rau sạch thủy canh tại nhà tự động hóa qua ứng dụng di động.",
                "Kết nối sinh viên tìm phòng trọ ghép an toàn, nhanh chóng.",
                "Mạng xã hội ghép đôi xem phim dựa trên sở thích thể loại chung.",
                "Học kỹ năng mềm trực tuyến cùng chuyên gia qua lớp học ảo tương tác."
            };

            string[] statuses = { "Active", "Draft", "At-Risk", "Suspended" };

            for (int i = 0; i < 14; i++)
            {
                var generatedProjId = Guid.Parse($"ad0cc535-f771-4a39-e83c-89c516f9b{i:d3}");
                var projStatus = statuses[random.Next(statuses.Length)];
                var staleDays = projStatus == "At-Risk" ? random.Next(15, 29) : (projStatus == "Suspended" ? random.Next(31, 50) : 0);
                
                projectList.Add(new Project
                {
                    Id = generatedProjId,
                    Name = $"{startupNouns[i]} {startupTechs[i]}",
                    Pitch = startupPitches[i],
                    Description = $"# {startupNouns[i]} {startupTechs[i]} Project\nĐây là mô tả chi tiết của dự án {startupNouns[i]} {startupTechs[i]} nhằm giải quyết các vấn đề thực tiễn của cuộc sống.",
                    Status = projStatus,
                    StorageUsedBytes = random.Next(10000, 5000000),
                    LastUpdatedAt = staleDays == 0 ? DateTime.UtcNow : DateTime.UtcNow.AddDays(-staleDays)
                });
            }

            context.Projects.AddRange(projectList);
            context.SaveChanges();

            // 2. Seed Users (100 total)
            var defaultPasswordHash = PasswordHasher.HashPassword("password123");
            var userList = new List<User>();

            // Original core accounts to keep
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

            // Add original core accounts to userList
            userList.Add(new User { Id = userPhatId, Email = "phathmse184629@fpt.edu.vn", Name = "Hỷ Minh Phát", Role = "Founder", PasswordHash = defaultPasswordHash, StudentId = "SE184629", ContactLink = "https://facebook.com/hyminhphat", CvUrl = "https://crgarashowcasedev.blob.core.windows.net/cv/se184629_cv.pdf", ProjectId = projectIdGara });
            userList.Add(new User { Id = userDucId, Email = "ducthse184622@fpt.edu.vn", Name = "Trịnh Hải Đức", Role = "Student", PasswordHash = defaultPasswordHash, StudentId = "SE184622", ContactLink = "https://facebook.com/ducth", CvUrl = "https://crgarashowcasedev.blob.core.windows.net/cv/se184622_cv.pdf", ProjectId = projectIdGara });
            userList.Add(new User { Id = userPhuId, Email = "phupqase180573@fpt.edu.vn", Name = "Phan Quới An Phú", Role = "Student", PasswordHash = defaultPasswordHash, StudentId = "SE180573", ContactLink = "https://facebook.com/anphu", CvUrl = "https://crgarashowcasedev.blob.core.windows.net/cv/se180573_cv.pdf", ProjectId = projectIdGara });
            userList.Add(new User { Id = userMockStudent1, Email = "student.b@fpt.edu.vn", Name = "Nguyễn Văn B (Mock GD)", Role = "Student", PasswordHash = defaultPasswordHash, StudentId = "GD180002", ContactLink = "https://facebook.com/studentb", CvUrl = "https://crgarashowcasedev.blob.core.windows.net/cv/gd180002_cv.pdf", ProjectId = projectIdGara });
            userList.Add(new User { Id = userMockStudent2, Email = "student.c@fpt.edu.vn", Name = "Trần Thị C (Mock Biz)", Role = "Student", PasswordHash = defaultPasswordHash, StudentId = "BA180003", ContactLink = "https://facebook.com/studentc", CvUrl = "https://crgarashowcasedev.blob.core.windows.net/cv/ba180003_cv.pdf", ProjectId = projectIdGara });
            userList.Add(new User { Id = founderEduId, Email = "edulink.founder@fpt.edu.vn", Name = "Trần Minh Quân (EduLink)", Role = "Founder", PasswordHash = defaultPasswordHash, StudentId = "SE183204", ContactLink = "https://facebook.com/edu.founder", CvUrl = "https://crgarashowcasedev.blob.core.windows.net/cv/se183204_cv.pdf", ProjectId = projectIdEdu });
            userList.Add(new User { Id = founderZodiacId, Email = "zodiac.founder@fpt.edu.vn", Name = "Võ Hoàng Yến (Zodiac)", Role = "Founder", PasswordHash = defaultPasswordHash, StudentId = "SE185112", ContactLink = "https://facebook.com/zodiac.founder", CvUrl = "https://crgarashowcasedev.blob.core.windows.net/cv/se185112_cv.pdf", ProjectId = projectIdZodiac });
            userList.Add(new User { Id = userKhanhId, Email = "khanhltse184638@fpt.edu.vn", Name = "Lê Tuấn Khanh", Role = "Student", PasswordHash = defaultPasswordHash, StudentId = "SE184638", ContactLink = "https://facebook.com/khanhlt", CvUrl = "https://crgarashowcasedev.blob.core.windows.net/cv/se184638_cv.pdf" });
            userList.Add(new User { Id = userVyId, Email = "vyntse182233@fpt.edu.vn", Name = "Trương Ngọc Vy", Role = "Student", PasswordHash = defaultPasswordHash, StudentId = "SE182233", ContactLink = "https://facebook.com/vynt", CvUrl = "https://crgarashowcasedev.blob.core.windows.net/cv/se182233_cv.pdf" });
            userList.Add(new User { Id = userBinhId, Email = "binhbtse185566@fpt.edu.vn", Name = "Bùi Thanh Bình (Mock Seeker)", Role = "Student", PasswordHash = defaultPasswordHash, StudentId = "SE185566", ContactLink = "https://facebook.com/binhbt", CvUrl = "https://crgarashowcasedev.blob.core.windows.net/cv/se185566_cv.pdf" });
            userList.Add(new User { Id = userLinhId, Email = "linhptse186677@fpt.edu.vn", Name = "Phạm Thu Linh (Mock Seeker)", Role = "Student", PasswordHash = defaultPasswordHash, StudentId = "GD186677", ContactLink = "https://facebook.com/linhpt", CvUrl = "https://crgarashowcasedev.blob.core.windows.net/cv/gd186677_cv.pdf" });
            userList.Add(new User { Id = userHoangId, Email = "hoanglmse181234@fpt.edu.vn", Name = "Lê Minh Hoàng (Mock Seeker)", Role = "Student", PasswordHash = defaultPasswordHash, StudentId = "SE181234", ContactLink = "https://facebook.com/hoanglm", CvUrl = "https://crgarashowcasedev.blob.core.windows.net/cv/se181234_cv.pdf" });
            userList.Add(new User { Id = managerId, Email = "manager.mock@fpt.edu.vn", Name = "Vườn Ươm Gara Manager", Role = "Manager", PasswordHash = defaultPasswordHash, ContactLink = "https://gara.edu.vn" });

            // Generate 87 more users to reach 100 total
            string[] firstNames = { "Nguyễn", "Trần", "Lê", "Phạm", "Huỳnh", "Hoàng", "Phan", "Vũ", "Võ", "Đặng", "Bùi", "Đỗ", "Hồ", "Ngô" };
            string[] middleNames = { "Văn", "Thị", "Minh", "Hải", "Đình", "Thanh", "Ngọc", "Hoàng", "Quốc", "Gia", "Tuyết", "Xuân" };
            string[] lastNames = { "Anh", "Bình", "Chí", "Dũng", "Giang", "Hùng", "Khoa", "Linh", "Nam", "Phong", "Quỳnh", "Sơn", "Trang", "Việt", "Yến", "Thảo", "Huy", "Tùng", "Long" };

            // We associate some users as Founders for the remaining 17 projects
            var projectsRequiringFounders = projectList.Skip(3).ToList(); // 17 projects
            
            for (int i = 0; i < 87; i++)
            {
                var generatedUserId = Guid.Parse($"ad0cc535-f771-4a39-e83c-89c516f9c{i:d3}");
                var firstName = firstNames[random.Next(firstNames.Length)];
                var middleName = middleNames[random.Next(middleNames.Length)];
                var lastName = lastNames[random.Next(lastNames.Length)];
                var fullName = $"{firstName} {middleName} {lastName}";
                var email = $"student.gen{i}@fpt.edu.vn";
                var studentId = $"SE18{1000 + i}";

                Guid? associatedProjectId = null;
                string role = "Student";

                if (i < projectsRequiringFounders.Count)
                {
                    // Make this user the Founder of one of the projects
                    role = "Founder";
                    associatedProjectId = projectsRequiringFounders[i].Id;
                    fullName += $" (Founder)";
                }
                else
                {
                    // Roster placement: randomly assign 60% of students as team members to random projects
                    if (random.NextDouble() < 0.6)
                    {
                        associatedProjectId = projectList[random.Next(projectList.Count)].Id;
                    }
                }

                userList.Add(new User
                {
                    Id = generatedUserId,
                    Email = email,
                    Name = fullName,
                    Role = role,
                    PasswordHash = defaultPasswordHash,
                    StudentId = studentId,
                    ContactLink = $"https://facebook.com/student.gen{i}",
                    CvUrl = $"https://crgarashowcasedev.blob.core.windows.net/cv/se18{1000 + i}_cv.pdf",
                    ProjectId = associatedProjectId
                });
            }

            context.Users.AddRange(userList);
            context.SaveChanges();

            // 3. Seed Milestones for all 20 projects
            var milestoneList = new List<Milestone>();
            string[] milestoneTitles1 = { "Khảo sát thị trường", "Hoàn thiện bản vẽ thiết kế", "Xây dựng hạ tầng cơ sở", "Pitching ý tưởng" };
            string[] milestoneTitles2 = { "Phát triển bản Beta", "Tích hợp dịch vụ đám mây", "Kiểm thử bảo mật", "Demo MVP" };
            string[] milestoneTitles3 = { "Nhận phản hồi người dùng", "Tối ưu hóa hiệu năng", "Chiến dịch Marketing", "Triển khai thực tế" };

            foreach (var proj in projectList)
            {
                // Add 3 milestones for each project
                milestoneList.Add(new Milestone
                {
                    Id = Guid.NewGuid(),
                    ProjectId = proj.Id,
                    Title = milestoneTitles1[random.Next(milestoneTitles1.Length)],
                    Description = "Cột mốc giai đoạn 1 nhằm thiết lập định hướng ban đầu cho dự án.",
                    Done = true,
                    DateCompleted = DateTime.UtcNow.AddDays(-random.Next(15, 30)),
                    CreatedAt = DateTime.UtcNow.AddDays(-35)
                });

                milestoneList.Add(new Milestone
                {
                    Id = Guid.NewGuid(),
                    ProjectId = proj.Id,
                    Title = milestoneTitles2[random.Next(milestoneTitles2.Length)],
                    Description = "Cột mốc giai đoạn 2 tập trung vào việc hiện thực hóa các chức năng cốt lõi.",
                    Done = random.NextDouble() > 0.4, // 60% chance of being completed
                    DateCompleted = DateTime.UtcNow.AddDays(-random.Next(1, 10)),
                    CreatedAt = DateTime.UtcNow.AddDays(-12)
                });

                milestoneList.Add(new Milestone
                {
                    Id = Guid.NewGuid(),
                    ProjectId = proj.Id,
                    Title = milestoneTitles3[random.Next(milestoneTitles3.Length)],
                    Description = "Cột mốc giai đoạn 3 hướng tới tối ưu hóa sản phẩm và triển khai thị trường.",
                    Done = false,
                    CreatedAt = DateTime.UtcNow
                });
            }

            context.Milestones.AddRange(milestoneList);
            context.SaveChanges();

            // 4. Seed Jobs (Only for Active/At-Risk projects)
            var jobList = new List<Job>();
            
            // Add original Gara jobs first (to keep ID references working!)
            var jobIdFullstack = Guid.Parse("1ca55711-bccf-4326-ad39-e83c89c516f9");
            var jobIdDesigner = Guid.Parse("2ca55711-bccf-4326-ad39-e83c89c516f9");
            var jobIdBizDev = Guid.Parse("3ca55711-bccf-4326-ad39-e83c89c516f9");

            jobList.Add(new Job { Id = jobIdFullstack, ProjectId = projectIdGara, Title = "Kỹ sư Fullstack (.NET 9 + React)", Category = "Engineering", Description = "Phát triển các mô đun tuyển dụng và tải tài liệu trực quan trực tiếp lên đám mây Azure.", Requirements = "Thành thạo lập trình C# ASP.NET Core, React TypeScript và sử dụng Git.", Status = "Open", CreatedAt = DateTime.UtcNow });
            jobList.Add(new Job { Id = jobIdDesigner, ProjectId = projectIdGara, Title = "UI/UX Designer (Chuyên về chuyển động)", Category = "Design", Description = "Thiết kế giao diện kính mờ cao cấp và các tương tác micro-animations mượt mà cho nền tảng Gara.", Requirements = "Thành thạo Figma, tư duy thẩm mỹ hiện đại, hiểu biết về CSS Transitions.", Status = "Open", CreatedAt = DateTime.UtcNow });
            jobList.Add(new Job { Id = jobIdBizDev, ProjectId = projectIdGara, Title = "Chuyên viên Phát triển Thị trường (Biz)", Category = "Business", Description = "Tìm kiếm các đối tác trường học thực hiện thử nghiệm nền tảng tuyển dụng.", Requirements = "Kỹ năng giao tiếp tốt, đam mê môi trường khởi nghiệp giáo dục.", Status = "Open", CreatedAt = DateTime.UtcNow });

            // Generate jobs for the other active/at-risk projects
            string[] jobTitles = { "Developer React", "Mobile Dev (Flutter)", "Backend Engineer (Go)", "Product Designer", "Copywriter", "Digital Marketer", "Data Analyst", "Embedded Dev" };
            string[] jobCategories = { "Engineering", "Engineering", "Engineering", "Design", "Business", "Marketing", "Business", "Engineering" };

            var eligibleProjects = projectList.Where(p => p.Id != projectIdGara && (p.Status == "Active" || p.Status == "At-Risk")).ToList();
            
            for (int i = 0; i < eligibleProjects.Count; i++)
            {
                var proj = eligibleProjects[i];
                var jobIdx = random.Next(jobTitles.Length);
                jobList.Add(new Job
                {
                    Id = Guid.Parse($"ad0cc535-f771-4a39-e83c-89c516f9d{i:d3}"),
                    ProjectId = proj.Id,
                    Title = jobTitles[jobIdx],
                    Category = jobCategories[jobIdx],
                    Description = "Tham gia phát triển giải pháp cốt lõi cùng các thành viên tài năng trong nhóm.",
                    Requirements = "Đam mê công nghệ, chủ động học hỏi và làm việc có trách nhiệm.",
                    Status = "Open",
                    CreatedAt = DateTime.UtcNow.AddDays(-random.Next(1, 10))
                });
            }

            context.Jobs.AddRange(jobList);
            context.SaveChanges();

            // 5. Seed Applications & Lockouts
            var applicationList = new List<Application>();
            
            // Core applications to keep for Hỷ Minh Phát's review panel
            applicationList.Add(new Application { Id = Guid.NewGuid(), StudentId = userBinhId, JobId = jobIdFullstack, Status = "Pending", CreatedAt = DateTime.UtcNow.AddDays(-2) });
            applicationList.Add(new Application { Id = Guid.NewGuid(), StudentId = userLinhId, JobId = jobIdDesigner, Status = "Pending", CreatedAt = DateTime.UtcNow.AddDays(-1) });
            applicationList.Add(new Application { Id = Guid.NewGuid(), StudentId = userHoangId, JobId = jobIdBizDev, Status = "Rejected", CreatedAt = DateTime.UtcNow.AddDays(-5) });

            // Generate some random applications from seekers to other open jobs
            var seekers = userList.Where(u => u.ProjectId == null && u.Role == "Student" && u.Id != userBinhId && u.Id != userLinhId && u.Id != userHoangId).ToList();
            var openJobs = jobList.Where(j => j.Id != jobIdFullstack && j.Id != jobIdDesigner && j.Id != jobIdBizDev).ToList();

            for (int i = 0; i < Math.Min(seekers.Count, openJobs.Count * 2); i++)
            {
                var seeker = seekers[i];
                var job = openJobs[random.Next(openJobs.Count)];
                var status = random.NextDouble() > 0.7 ? "Approved" : (random.NextDouble() > 0.5 ? "Rejected" : "Pending");
                
                applicationList.Add(new Application
                {
                    Id = Guid.NewGuid(),
                    StudentId = seeker.Id,
                    JobId = job.Id,
                    Status = status,
                    CreatedAt = DateTime.UtcNow.AddDays(-random.Next(1, 8))
                });

                if (status == "Approved")
                {
                    seeker.ProjectId = job.ProjectId;
                }
            }

            context.Applications.AddRange(applicationList);
            context.SaveChanges();

            // Add lockout record for Hoàng to test BR-06 validation
            var lockout = new ApplicationLockout
            {
                Id = Guid.NewGuid(),
                StudentId = userHoangId,
                JobId = jobIdBizDev,
                LockedUntil = DateTime.UtcNow.AddDays(25)
            };
            
            context.ApplicationLockouts.Add(lockout);
            context.SaveChanges();
        }
    }
}
