import { randomUUID } from 'node:crypto';

export async function seed(knex) {
  // 1. Deleting existing records in reverse dependency order
  await knex('applications').del();
  await knex('jobs').del();
  await knex('milestones').del();
  await knex('projects').del();
  await knex('users').del();

  // 2. Insert Users (Founding Team & Mock Users)
  const userIdPhat = randomUUID();
  const userIdDuc = randomUUID();
  const userIdPhu = randomUUID();
  const userIdKhanh = randomUUID();
  const userIdStudent = randomUUID();
  const userIdManager = randomUUID();

  await knex('users').insert([
    {
      id: userIdPhat,
      email: 'phathmse184629@fpt.edu.vn',
      name: 'Hỷ Minh Phát',
      role: 'Founder',
      student_id: 'SE184629'
    },
    {
      id: userIdDuc,
      email: 'ducthse180000@fpt.edu.vn',
      name: 'Trịnh Hải Đức',
      role: 'Founder',
      student_id: 'SE180000'
    },
    {
      id: userIdPhu,
      email: 'phupqase180001@fpt.edu.vn',
      name: 'Phan Quới An Phú',
      role: 'Founder',
      student_id: 'SE180001'
    },
    {
      id: userIdKhanh,
      email: 'khanhltkse180002@fpt.edu.vn',
      name: 'Lê Tuấn Khanh',
      role: 'Founder',
      student_id: 'SE180002'
    },
    {
      id: userIdStudent,
      email: 'student.mock@fpt.edu.vn',
      name: 'Nguyễn Văn A',
      role: 'Student',
      student_id: 'SE189999'
    },
    {
      id: userIdManager,
      email: 'manager.mock@fpt.edu.vn',
      name: 'Vườn Ươm Gara Manager',
      role: 'Manager',
      student_id: null
    }
  ]);

  // 3. Insert Projects
  const projectIdGara = randomUUID();
  const projectIdEdu = randomUUID();

  await knex('projects').insert([
    {
      id: projectIdGara,
      name: 'Gara Startup Project Showcase',
      pitch: 'Nền tảng quản lý danh mục khởi nghiệp và tuyển dụng thành viên liên khoa cho trường đại học.',
      description: '# Gara Startup Showcase\nNền tảng kết nối sinh viên thuộc các khối ngành kỹ thuật (SE), thiết kế (GD) và kinh doanh (Biz) dưới sự quản lý của vườn ươm khởi nghiệp trường đại học.',
      status: 'Active',
      last_updated_at: knex.fn.now(),
      storage_used_bytes: 452140 // ~440 KB
    },
    {
      id: projectIdEdu,
      name: 'EduLink Platform',
      pitch: 'Hệ thống điểm danh và quản lý học tập thông minh dựa trên thẻ NFC.',
      description: '# EduLink Platform\nHệ thống điểm danh tiện lợi, lưu trữ lịch sử lên lớp và tương tác bài học trực quan.',
      status: 'Active',
      last_updated_at: knex.fn.now(),
      storage_used_bytes: 1205421 // ~1.1 MB
    }
  ]);

  // 4. Insert Milestones
  await knex('milestones').insert([
    {
      id: randomUUID(),
      project_id: projectIdGara,
      title: 'Hoàn thiện bản Slide Pitching MVP',
      description: 'Thiết kế giao diện slide deck dạng SPA và chạy thử bản mô phỏng MVP.',
      done: true,
      date_completed: knex.fn.now()
    },
    {
      id: randomUUID(),
      project_id: projectIdGara,
      title: 'Thiết kế Database & API RESTful',
      description: 'Lập cấu hình CSDL PostgreSQL bằng Knex migrations và viết API lõi.',
      done: false,
      date_completed: null
    },
    {
      id: randomUUID(),
      project_id: projectIdEdu,
      title: 'Sản xuất thử nghiệm phần cứng NFC Reader',
      description: 'Lắp ráp mô hình vi điều khiển đọc thẻ RFID/NFC giao tiếp qua Wi-Fi.',
      done: true,
      date_completed: knex.fn.now()
    }
  ]);

  // 5. Insert Job Listings
  const jobIdFrontend = randomUUID();
  const jobIdDesign = randomUUID();

  await knex('jobs').insert([
    {
      id: jobIdFrontend,
      project_id: projectIdGara,
      title: 'Tuyển dụng Lập trình viên Frontend (JS/CSS/HTML)',
      category: 'Engineering',
      description: 'Tìm kiếm thành viên hỗ trợ hoàn thiện giao diện ứng dụng SPA, yêu cầu hiểu vững về CSS Grid/Flexbox và Vanilla Javascript.',
      requirements: 'Sinh viên năm 2 trở lên khối ngành SE, có sản phẩm demo cá nhân.',
      status: 'Open'
    },
    {
      id: jobIdDesign,
      project_id: projectIdGara,
      title: 'UI/UX Designer',
      category: 'Design',
      description: 'Thiết kế giao diện cho các tính năng tuyển dụng và trang dashboard quản trị của Incubator.',
      requirements: 'Thành thạo Figma, yêu thích trường phái thiết kế tối giản, hiện đại.',
      status: 'Open'
    }
  ]);

  // 6. Insert Mock Applications (Student nộp đơn)
  await knex('applications').insert([
    {
      id: randomUUID(),
      student_id: userIdStudent,
      job_id: jobIdFrontend,
      status: 'Pending'
    }
  ]);
}
