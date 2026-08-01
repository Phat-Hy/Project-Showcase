import { randomUUID } from 'node:crypto';
import db from '../db/db.js';

export async function applyToJob(req, res) {
  const { studentId, jobId } = req.body;

  if (!studentId || !jobId) {
    return res.status(400).json({ error: 'Thiếu thông tin sinh viên hoặc tin tuyển dụng.' });
  }

  try {
    // 1. Verify user exists and is a student or founder
    const user = await db('users').where({ id: studentId }).first();
    if (!user) {
      return res.status(404).json({ error: 'Không tìm thấy thông tin sinh viên.' });
    }

    // Enforce BR-04 Exception: Must have completed resume profile (contact link and CV url)
    if (!user.contact_link || !user.cv_url) {
      return res.status(400).json({ 
        error: 'Bạn cần hoàn thiện hồ sơ sinh viên (cập nhật liên kết liên hệ và tệp CV PDF) trước khi ứng tuyển.' 
      });
    }

    // 2. Verify job exists and is open
    const job = await db('jobs').where({ id: jobId, status: 'Open' }).first();
    if (!job) {
      return res.status(404).json({ error: 'Công việc này hiện không còn nhận đơn ứng tuyển.' });
    }

    // Check if job belongs to a suspended project (should not be applicable)
    const project = await db('projects').where({ id: job.project_id }).first();
    if (project.status === 'Suspended') {
      return res.status(400).json({ error: 'Dự án này đã bị Tạm ngưng (Suspended), không thể nộp đơn.' });
    }

    // 3. Enforce Concurrency Limit (BR-05 / FR-006)
    const pendingApplications = await db('applications')
      .where({ student_id: studentId, status: 'Pending' })
      .count('id as count')
      .first();

    const pendingCount = parseInt(pendingApplications.count, 10);
    if (pendingCount >= 3) {
      return res.status(400).json({ 
        error: 'Bạn đã đạt giới hạn tối đa 3 đơn ứng tuyển đang chờ duyệt đồng thời.' 
      });
    }

    // Check duplicate application
    const duplicate = await db('applications')
      .where({ student_id: studentId, job_id: jobId })
      .whereIn('status', ['Pending', 'Approved'])
      .first();

    if (duplicate) {
      return res.status(400).json({ error: 'Bạn đã nộp đơn ứng tuyển cho vai trò này trước đó rồi.' });
    }

    // 4. Save Application
    const newApplication = {
      id: randomUUID(),
      student_id: studentId,
      job_id: jobId,
      status: 'Pending'
    };

    await db('applications').insert(newApplication);
    res.status(201).json({ message: 'Nộp đơn ứng tuyển thành công.', application: newApplication });
  } catch (error) {
    res.status(500).json({ error: 'Lỗi nộp đơn ứng tuyển: ' + error.message });
  }
}

export async function getStudentApplications(req, res) {
  const { studentId } = req.params;
  try {
    const applications = await db('applications')
      .join('jobs', 'applications.job_id', 'jobs.id')
      .join('projects', 'jobs.project_id', 'projects.id')
      .select(
        'applications.id',
        'applications.status as application_status',
        'applications.created_at',
        'jobs.title as job_title',
        'projects.name as project_name'
      )
      .where({ 'applications.student_id': studentId })
      .orderBy('applications.created_at', 'desc');

    res.json(applications);
  } catch (error) {
    res.status(500).json({ error: 'Lỗi truy xuất danh sách đơn ứng tuyển: ' + error.message });
  }
}
