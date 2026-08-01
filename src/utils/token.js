import { createHmac } from 'node:crypto';

// Use a secret key from environment or fallback to default for local dev
const JWT_SECRET = process.env.JWT_SECRET || 'gara-showcase-super-secret-key-12345';

export function signToken(payload, expiresInMs = 86400000) { // Default 24 hours
  const header = { alg: 'HS256', typ: 'JWT' };
  const exp = Date.now() + expiresInMs;
  const fullPayload = { ...payload, exp };

  const sHeader = Buffer.from(JSON.stringify(header)).toString('base64url');
  const sPayload = Buffer.from(JSON.stringify(fullPayload)).toString('base64url');

  const signature = createHmac('sha256', JWT_SECRET)
    .update(`${sHeader}.${sPayload}`)
    .digest('base64url');

  return `${sHeader}.${sPayload}.${signature}`;
}

export function verifyToken(token) {
  try {
    const parts = token.split('.');
    if (parts.length !== 3) return null;

    const [sHeader, sPayload, signature] = parts;
    const expectedSignature = createHmac('sha256', JWT_SECRET)
      .update(`${sHeader}.${sPayload}`)
      .digest('base64url');

    if (signature !== expectedSignature) {
      return null; // Signature mismatch
    }

    const payload = JSON.parse(Buffer.from(sPayload, 'base64url').toString('utf8'));
    if (Date.now() > payload.exp) {
      return null; // Token expired
    }

    return payload;
  } catch (err) {
    return null;
  }
}
