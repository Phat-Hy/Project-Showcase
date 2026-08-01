import express from 'express';
import cron from 'node-cron';
import dotenv from 'dotenv';
import apiRouter from './routes/api.js';
import authRouter from './routes/auth.js';
import { runDormancyCheck } from './workers/dormancyWorker.js';

dotenv.config();

const app = express();
const PORT = process.env.PORT || 3000;

app.use(express.json());

// Serve static frontend assets
app.use(express.static('src/public'));

// Bind API endpoints
app.use('/api', apiRouter);
app.use('/api/auth', authRouter);

// Health check endpoint
app.get('/health', (req, res) => {
  res.json({ status: 'OK', timestamp: new Date() });
});

// Setup background worker cron job (Runs every day at midnight: 00:00)
cron.schedule('0 0 * * *', () => {
  console.log('[Scheduler] Chạy tác vụ quét trạng thái dự án định kỳ hàng ngày...');
  runDormancyCheck().catch(err => {
    console.error('[Scheduler] Tác vụ định kỳ gặp lỗi:', err.message);
  });
});

app.listen(PORT, () => {
  console.log(`[Server] Gara Showcase API Server đang chạy tại cổng ${PORT}`);
  console.log(`[Server] Khởi chạy bộ lập lịch tác vụ nền định kỳ (Cron scheduler) thành công.`);
});

export default app;
