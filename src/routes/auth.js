import express from 'express';
import { Issuer, custom } from 'openid-client';
import { signToken } from '../utils/token.js';
import db from '../db/db.js';

const router = express.Router();

// Allow OIDC library to run in local development HTTP (non-HTTPS) environments
custom.setHttpOptionsDefaults({
  timeout: 5000,
});

let oidcClient = null;

// Initialize OIDC client dynamically if configured in environment
async function getOidcClient() {
  if (oidcClient) return oidcClient;

  const issuerUrl = process.env.OIDC_ISSUER_URL;
  const clientId = process.env.OIDC_CLIENT_ID;
  const clientSecret = process.env.OIDC_CLIENT_SECRET;
  const redirectUri = process.env.OIDC_REDIRECT_URI || 'http://localhost:3000/api/auth/callback';

  if (!issuerUrl || !clientId) {
    // If not configured, OIDC client stays null (triggers Mock Mode)
    return null;
  }

  try {
    const issuer = await Issuer.discover(issuerUrl);
    oidcClient = new issuer.Client({
      client_id: clientId,
      client_secret: clientSecret,
      redirect_uris: [redirectUri],
      response_types: ['code'],
    });
    return oidcClient;
  } catch (err) {
    console.error('[SSO] Không thể kết nối với OIDC Issuer:', err.message);
    return null;
  }
}

// GET /api/auth/login
router.get('/login', async (req, res) => {
  const client = await getOidcClient();
  const mockRole = req.query.mockRole || 'Student'; // 'Student', 'Founder', 'Manager'
  
  if (client) {
    // 1. Real SSO Mode
    const authorizationUrl = client.authorizationUrl({
      scope: 'openid email profile',
      state: 'gara-state-key-random', // In production, generate a cryptographically random state
    });
    res.redirect(authorizationUrl);
  } else {
    // 2. Mock SSO Mode (Saves local setup/connection barriers)
    console.log(`[SSO] Đang khởi chạy Mock Login với vai trò: ${mockRole}`);
    
    // Choose mock identity based on role requested
    let mockEmail = 'student.mock@fpt.edu.vn';
    let mockName = 'Nguyễn Văn A';
    let studentId = 'SE189999';

    if (mockRole === 'Founder') {
      mockEmail = 'phathmse184629@fpt.edu.vn';
      mockName = 'Hỷ Minh Phát';
      studentId = 'SE184629';
    } else if (mockRole === 'Manager') {
      mockEmail = 'manager.mock@fpt.edu.vn';
      mockName = 'Vườn Ươm Gara Manager';
      studentId = null;
    }

    try {
      // Sync mock user with database
      let user = await db('users').where({ email: mockEmail }).first();
      if (!user) {
        // If mock database table is empty, auto-create
        user = {
          id: '00000000-0000-0000-0000-000000000001',
          email: mockEmail,
          name: mockName,
          role: mockRole,
          student_id: studentId
        };
        await db('users').insert(user);
      }

      // Generate local session token
      const token = signToken({
        id: user.id,
        email: user.email,
        name: user.name,
        role: user.role,
        student_id: user.student_id
      });

      // Redirect back to frontend simulator with the generated token
      const frontendRedirectUrl = process.env.FRONTEND_URL || 'http://localhost:3000';
      res.redirect(`${frontendRedirectUrl}/?token=${token}`);
    } catch (err) {
      res.status(500).json({ error: 'Lỗi đồng bộ Mock Login: ' + err.message });
    }
  }
});

// GET /api/auth/callback (SSO Callback)
router.get('/callback', async (req, res) => {
  const client = await getOidcClient();
  if (!client) {
    return res.status(400).json({ error: 'OAuth/OIDC Client chưa được cấu hình ở môi trường này.' });
  }

  const params = client.callbackParams(req);
  try {
    const redirectUri = process.env.OIDC_REDIRECT_URI || 'http://localhost:3000/api/auth/callback';
    const tokenSet = await client.callback(redirectUri, params, { state: 'gara-state-key-random' });
    
    // Extract user profile claims from ID Token
    const claims = tokenSet.claims();
    const email = claims.email;
    const name = claims.name || claims.preferred_username || 'Sinh viên Gara';
    
    // Synchronize authenticated user with database
    let user = await db('users').where({ email }).first();
    if (!user) {
      // Create user profile on first login (Default to Student role)
      user = {
        id: claims.sub, // Use OIDC subject as user UUID
        email,
        name,
        role: 'Student', // Default role
        student_id: email.includes('se') ? email.split('@')[0].replace(/[^0-9]/g, '') : null
      };
      await db('users').insert(user);
    }

    // Generate local JWT token for subsequent API requests
    const sessionToken = signToken({
      id: user.id,
      email: user.email,
      name: user.name,
      role: user.role,
      student_id: user.student_id
    });

    const frontendRedirectUrl = process.env.FRONTEND_URL || 'http://localhost:3000';
    res.redirect(`${frontendRedirectUrl}/?token=${sessionToken}`);
  } catch (error) {
    res.status(500).json({ error: 'Xác thực OIDC thất bại: ' + error.message });
  }
});

export default router;
