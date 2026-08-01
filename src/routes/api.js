import express from 'express';
import { 
  getAllProjects, 
  getProjectById, 
  createProject, 
  uploadProjectFile, 
  addMilestone 
} from '../controllers/projectController.js';
import { 
  getOpenJobs, 
  createJob 
} from '../controllers/jobController.js';
import { 
  applyToJob, 
  getStudentApplications 
} from '../controllers/applicationController.js';
import { runDormancyCheck } from '../workers/dormancyWorker.js';
import { requireAuth, checkRole } from '../middlewares/authMiddleware.js';

const router = express.Router();

// --- PUBLIC ROUTES (Read-Only) ---
router.get('/projects', getAllProjects);
router.get('/projects/:id', getProjectById);
router.get('/jobs', getOpenJobs);

// --- SECURE ROUTES (RBAC Protected) ---

// Create projects - Founders and Managers only
router.post('/projects', requireAuth, checkRole(['Founder', 'Manager']), createProject);

// Upload files - Project Founders only (Enforces BR-04)
router.post('/projects/:id/upload', requireAuth, checkRole(['Founder']), uploadProjectFile);

// Add Milestones - Project Founders only (Enforces BR-08)
router.post('/projects/:projectId/milestones', requireAuth, checkRole(['Founder']), addMilestone);

// Post open roles - Project Founders only
router.post('/jobs', requireAuth, checkRole(['Founder']), createJob);

// Submit job applications - Students and Founders only (Enforces BR-05)
router.post('/applications', requireAuth, checkRole(['Student', 'Founder']), applyToJob);

// Get student's own applications
router.get('/applications/student/:studentId', requireAuth, checkRole(['Student', 'Founder', 'Manager']), getStudentApplications);

// Trigger Inactivity scans manually - Managers only (Enforces BR-08)
router.post('/admin/run-dormancy-check', requireAuth, checkRole(['Manager']), async (req, res) => {
  try {
    const results = await runDormancyCheck();
    res.json({ message: 'Quét trạng thái thành công.', results });
  } catch (error) {
    res.status(500).json({ error: 'Lỗi kích hoạt quét trạng thái: ' + error.message });
  }
});

export default router;
