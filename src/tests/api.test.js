import request from 'supertest';
import { randomUUID } from 'node:crypto';
import app from '../app.js';
import db from '../db/db.js';
import { signToken } from '../utils/token.js';

describe('Gara Showcase Backend Business Rules & RBAC Integration Tests', () => {
  let mockStudentId;
  let mockProjectId;
  let mockJobId;

  // Active role tokens
  let founderToken;
  let studentToken;
  let guestToken;

  beforeAll(async () => {
    // Run migrations and seeds to prepare test db
    await db.migrate.latest();
    await db.seed.run();

    // Fetch seeded entities
    const student = await db('users').where({ email: 'student.mock@fpt.edu.vn' }).first();
    mockStudentId = student.id;

    const project = await db('projects').where({ name: 'Gara Startup Project Showcase' }).first();
    mockProjectId = project.id;

    const job = await db('jobs').where({ project_id: mockProjectId }).first();
    mockJobId = job.id;

    // Generate tokens for testing RBAC
    founderToken = signToken({
      id: randomUUID(),
      email: 'phathmse184629@fpt.edu.vn',
      name: 'Hỷ Minh Phát',
      role: 'Founder',
      student_id: 'SE184629'
    });

    studentToken = signToken({
      id: mockStudentId,
      email: 'student.mock@fpt.edu.vn',
      name: 'Nguyễn Văn A',
      role: 'Student',
      student_id: 'SE189999'
    });

    guestToken = signToken({
      id: randomUUID(),
      email: 'guest.mock@fpt.edu.vn',
      name: 'Guest User',
      role: 'Guest',
      student_id: null
    });
  });

  afterAll(async () => {
    // Close DB Connection pool after tests complete
    await db.destroy();
  });

  describe('GET /health', () => {
    it('should return server OK status (Public)', async () => {
      const res = await request(app).get('/health');
      expect(res.status).toBe(200);
      expect(res.body.status).toBe('OK');
    });
  });

  describe('Role-Based Access Control (RBAC)', () => {
    it('should reject securing routes if no authorization header is provided', async () => {
      const res = await request(app)
        .post(`/api/projects/${mockProjectId}/upload`)
        .send({ fileSize: 100 });

      expect(res.status).toBe(401);
      expect(res.body.error).toContain('Không tìm thấy mã xác thực');
    });

    it('should reject securing routes if role is not allowed (Guest calling Founder route)', async () => {
      const res = await request(app)
        .post(`/api/projects/${mockProjectId}/upload`)
        .set('Authorization', `Bearer ${guestToken}`)
        .send({ fileSize: 100 });

      expect(res.status).toBe(403);
      expect(res.body.error).toContain('Bạn không có quyền thực hiện hành động này');
    });
  });

  describe('Project Storage Cap (BR-04)', () => {
    it('should allow file uploads under the 500MB threshold for Founders', async () => {
      const res = await request(app)
        .post(`/api/projects/${mockProjectId}/upload`)
        .set('Authorization', `Bearer ${founderToken}`)
        .send({ fileSize: 10 * 1024 * 1024 }); // 10MB

      expect(res.status).toBe(200);
      expect(res.body.message).toContain('Tải lên file thành công');
    });

    it('should reject file uploads exceeding the 500MB threshold', async () => {
      const res = await request(app)
        .post(`/api/projects/${mockProjectId}/upload`)
        .set('Authorization', `Bearer ${founderToken}`)
        .send({ fileSize: 550 * 1024 * 1024 }); // 550MB (exceeds total 500MB limit)

      expect(res.status).toBe(400);
      expect(res.body.error).toContain('Dung lượng lưu trữ vượt quá giới hạn 500MB');
    });
  });

  describe('Application Concurrency Limit (BR-05)', () => {
    it('should block submitting a new application if student already has 3 pending applications', async () => {
      // 1. Create a mock project and 3 distinct jobs
      const newProjectId = randomUUID();
      await db('projects').insert({
        id: newProjectId,
        name: 'Temporary Project',
        pitch: 'Mock pitch',
        status: 'Active'
      });

      const jobIds = [randomUUID(), randomUUID(), randomUUID()];
      for (const jobId of jobIds) {
        await db('jobs').insert({
          id: jobId,
          project_id: newProjectId,
          title: 'Mock Job',
          category: 'Engineering',
          description: 'Desc',
          status: 'Open'
        });
      }

      // 2. Submit 3 pending applications (we already have 1 seeded, so we insert 2 more)
      await db('applications').insert([
        { id: randomUUID(), student_id: mockStudentId, job_id: jobIds[0], status: 'Pending' },
        { id: randomUUID(), student_id: mockStudentId, job_id: jobIds[1], status: 'Pending' }
      ]);

      // 3. Attempt to submit a 4th pending application via API - should fail!
      const res = await request(app)
        .post('/api/applications')
        .set('Authorization', `Bearer ${studentToken}`)
        .send({
          studentId: mockStudentId,
          jobId: jobIds[2]
        });

      expect(res.status).toBe(400);
      expect(res.body.error).toContain('Bạn đã đạt giới hạn tối đa 3 đơn ứng tuyển');

      // Clean up temporary project and applications
      await db('applications').where({ student_id: mockStudentId }).whereIn('job_id', jobIds).del();
      await db('jobs').whereIn('id', jobIds).del();
      await db('projects').where({ id: newProjectId }).del();
    });
  });

  describe('Student Profile Completed Requirement (BR-04 Exception)', () => {
    it('should block applications if the student has not completed their profile (missing contact link or CV)', async () => {
      // 1. Create a student with no contact_link or cv_url in database
      const incompleteStudentId = randomUUID();
      const incompleteStudentToken = signToken({
        id: incompleteStudentId,
        email: 'incomplete@fpt.edu.vn',
        name: 'Incomplete Student',
        role: 'Student',
        student_id: 'SE181111'
      });

      await db('users').insert({
        id: incompleteStudentId,
        email: 'incomplete@fpt.edu.vn',
        name: 'Incomplete Student',
        role: 'Student',
        student_id: 'SE181111',
        contact_link: null,
        cv_url: null
      });

      // 2. Attempt to apply
      const res = await request(app)
        .post('/api/applications')
        .set('Authorization', `Bearer ${incompleteStudentToken}`)
        .send({
          studentId: incompleteStudentId,
          jobId: mockJobId
        });

      expect(res.status).toBe(400);
      expect(res.body.error).toContain('Bạn cần hoàn thiện hồ sơ sinh viên');

      // Clean up
      await db('users').where({ id: incompleteStudentId }).del();
    });
  });
});
