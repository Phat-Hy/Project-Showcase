import db from '../db/db.js';

export async function runDormancyCheck() {
  console.log('[DormancyWorker] Bắt đầu quét kiểm tra trạng thái hoạt động...');
  const now = new Date();
  
  // 14 ngày trước
  const fourteenDaysAgo = new Date();
  fourteenDaysAgo.setDate(now.getDate() - 14);

  // 30 ngày trước
  const thirtyDaysAgo = new Date();
  thirtyDaysAgo.setDate(now.getDate() - 30);

  try {
    // 1. Quét chuyển trạng thái: Active -> At-Risk (Quá hạn 14 ngày)
    const atRiskCount = await db('projects')
      .where('status', 'Active')
      .where('last_updated_at', '<', fourteenDaysAgo)
      .update({
        status: 'At-Risk',
        updated_at: db.fn.now()
      });

    if (atRiskCount > 0) {
      console.log(`[DormancyWorker] Đã chuyển ${atRiskCount} dự án sang trạng thái cảnh báo "At-Risk".`);
    }

    // 2. Quét chuyển trạng thái: At-Risk -> Suspended (Quá hạn 30 ngày)
    const suspendedCount = await db('projects')
      .where('status', 'At-Risk')
      .where('last_updated_at', '<', thirtyDaysAgo)
      .update({
        status: 'Suspended',
        updated_at: db.fn.now()
      });

    if (suspendedCount > 0) {
      console.log(`[DormancyWorker] Đã chuyển ${suspendedCount} dự án sang trạng thái đình chỉ "Suspended".`);
    }

    console.log('[DormancyWorker] Hoàn tất quét trạng thái hoạt động.');
    return { atRiskCount, suspendedCount };
  } catch (error) {
    console.error('[DormancyWorker] Gặp lỗi khi quét trạng thái:', error.message);
    throw error;
  }
}
