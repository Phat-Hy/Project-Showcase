import { verifyToken } from '../utils/token.js';

function getCookieToken(req) {
  if (!req.headers.cookie) return null;
  const cookies = req.headers.cookie.split(';').reduce((acc, cookie) => {
    const parts = cookie.trim().split('=');
    const key = parts[0];
    const val = parts.slice(1).join('=');
    acc[key] = val;
    return acc;
  }, {});
  return cookies['token'] || null;
}

export function requireAuth(req, res, next) {
  const authHeader = req.headers.authorization;
  let token = null;

  if (authHeader && authHeader.startsWith('Bearer ')) {
    token = authHeader.substring(7);
  } else if (req.query && req.query.token) {
    token = req.query.token;
  } else {
    token = getCookieToken(req);
  }

  if (!token) {
    return res.status(401).json({ error: 'Không tìm thấy mã xác thực. Vui lòng đăng nhập.' });
  }

  const decoded = verifyToken(token);
  if (!decoded) {
    return res.status(401).json({ error: 'Mã xác thực không hợp lệ hoặc đã hết hạn.' });
  }

  // Populate req.user for use in downstream controllers
  req.user = {
    id: decoded.id,
    email: decoded.email,
    name: decoded.name,
    role: decoded.role,
    student_id: decoded.student_id
  };

  next();
}

// Role-Based Access Control Middleware
export function checkRole(allowedRoles = []) {
  return (req, res, next) => {
    if (!req.user) {
      return res.status(401).json({ error: 'Bạn cần đăng nhập trước khi thực hiện hành động này.' });
    }

    if (!allowedRoles.includes(req.user.role)) {
      return res.status(403).json({ 
        error: `Bạn không có quyền thực hiện hành động này. Yêu cầu quyền: ${allowedRoles.join(', ')}. Vai trò của bạn: ${req.user.role}` 
      });
    }

    next();
  };
}
