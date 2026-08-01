import { randomUUID } from 'node:crypto';
import db from '../db/db.js';

// Retrieve all open jobs (filtering out jobs of Suspended projects per BR-08)
export async function getOpenJobs(req, res) {
  try {
    const jobs = await db('jobs')
      .join('projects', 'jobs.project_id', 'projects.id')
      .select(
        'jobs.id',
        'jobs.title',
        'jobs.category',
        'jobs.description',
        'jobs.requirements',
        'jobs.status',
        'jobs.created_at',
        'projects.id as project_id',
        'projects.name as project_name',
        'projects.status as project_status'
      )
      .where('jobs.status', 'Open')
      .whereNot('projects.status', 'Suspended') // BR-08: Hide jobs of Suspended projects
      .orderBy('jobs.created_at', 'desc');

    res.json(jobs);
  } catch (error) {
    res.status(500).json({ error: 'Lỗi truy xuất danh sách tuyển dụng: ' + error.message });
  }
}

export async function createJob(req, res) {
  const { projectId, title, category, description, requirements } = req.body;

  if (!projectId || !title || !category || !description) {
    return res.status(400).json({ error: 'Thiếu thông tin tuyển dụng bắt buộc.' });
  }

  try {
    const project = await db('projects').where({ id: projectId }).first();
    if (!project) {
      return res.status(404).json({ error: 'Không tìm thấy dự án tương ứng.' });
    }

    if (project.status === 'Suspended') {
      return res.status(400).json({ error: 'Không thể đăng tuyển dụng cho dự án đang bị Tạm ngưng (Suspended).' });
    }

    const job = {
      id: randomUUID(),
      project_id: projectId,
      title,
      category,
      description,
      requirements: requirements || '',
      status: 'Open'
    };

    await db('jobs').insert(job);
    res.status(201).json(job);
  } catch (error) {
    res.status(500).json({ error: 'Lỗi đăng tuyển dụng: ' + error.message });
  }
}
