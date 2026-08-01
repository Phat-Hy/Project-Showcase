import { randomUUID } from 'node:crypto';
import db from '../db/db.js';

// Max 500MB = 500 * 1024 * 1024 bytes
const MAX_STORAGE_LIMIT_BYTES = 524288000;

export async function getAllProjects(req, res) {
  try {
    const projects = await db('projects').select('*').orderBy('name', 'asc');
    res.json(projects);
  } catch (error) {
    res.status(500).json({ error: 'Lỗi truy xuất danh sách dự án: ' + error.message });
  }
}

export async function getProjectById(req, res) {
  const { id } = req.params;
  try {
    const project = await db('projects').where({ id }).first();
    if (!project) {
      return res.status(404).json({ error: 'Không tìm thấy dự án.' });
    }
    const milestones = await db('milestones').where({ project_id: id }).orderBy('created_at', 'asc');
    res.json({ ...project, milestones });
  } catch (error) {
    res.status(500).json({ error: 'Lỗi truy xuất chi tiết dự án: ' + error.message });
  }
}

export async function createProject(req, res) {
  const { name, pitch, description } = req.body;
  if (!name || !pitch) {
    return res.status(400).json({ error: 'Tên dự án và tóm tắt (pitch) là bắt buộc.' });
  }

  try {
    const newProject = {
      id: randomUUID(),
      name,
      pitch,
      description: description || '',
      status: 'Active',
      last_updated_at: db.fn.now(),
      storage_used_bytes: 0
    };
    await db('projects').insert(newProject);
    res.status(201).json(newProject);
  } catch (error) {
    if (error.message.includes('unique constraint') || error.code === '23505') {
      return res.status(400).json({ error: 'Tên dự án đã tồn tại trên hệ thống.' });
    }
    res.status(500).json({ error: 'Lỗi tạo dự án: ' + error.message });
  }
}

// Enforce BR-04: File upload size verification
export async function uploadProjectFile(req, res) {
  const { id } = req.params;
  const { fileSize } = req.body; // Mock file size in bytes

  if (!fileSize || isNaN(fileSize) || fileSize <= 0) {
    return res.status(400).json({ error: 'Dung lượng file tải lên không hợp lệ.' });
  }

  try {
    const project = await db('projects').where({ id }).first();
    if (!project) {
      return res.status(404).json({ error: 'Không tìm thấy dự án.' });
    }

    const currentStorage = parseInt(project.storage_used_bytes, 10);
    const newStorage = currentStorage + parseInt(fileSize, 10);

    // Enforce 500MB Cap (BR-04)
    if (newStorage > MAX_STORAGE_LIMIT_BYTES) {
      return res.status(400).json({ 
        error: `Dung lượng lưu trữ vượt quá giới hạn 500MB cho phép. (Hiện tại: ${(currentStorage / 1024 / 1024).toFixed(2)}MB, File mới: ${(fileSize / 1024 / 1024).toFixed(2)}MB)` 
      });
    }

    await db('projects')
      .where({ id })
      .update({
        storage_used_bytes: newStorage,
        last_updated_at: db.fn.now()
      });

    res.json({ message: 'Tải lên file thành công.', storage_used_bytes: newStorage });
  } catch (error) {
    res.status(500).json({ error: 'Lỗi tải lên tệp: ' + error.message });
  }
}

// Enforce BR-08: Add Milestone resets inactivity status
export async function addMilestone(req, res) {
  const { projectId } = req.params;
  const { title, description } = req.body;

  if (!title) {
    return res.status(400).json({ error: 'Tiêu đề cột mốc là bắt buộc.' });
  }

  try {
    const project = await db('projects').where({ id: projectId }).first();
    if (!project) {
      return res.status(404).json({ error: 'Không tìm thấy dự án.' });
    }

    const milestoneId = randomUUID();
    await db.transaction(async (trx) => {
      // 1. Insert Milestone
      await trx('milestones').insert({
        id: milestoneId,
        project_id: projectId,
        title,
        description: description || '',
        done: false
      });

      // 2. Reset last_updated_at and restore status to 'Active' (BR-08)
      await trx('projects')
        .where({ id: projectId })
        .update({
          status: 'Active',
          last_updated_at: trx.fn.now(),
          updated_at: trx.fn.now()
        });
    });

    res.status(201).json({ message: 'Thêm cột mốc thành công và kích hoạt lại dự án.', milestoneId });
  } catch (error) {
    res.status(500).json({ error: 'Lỗi thêm cột mốc: ' + error.message });
  }
}
