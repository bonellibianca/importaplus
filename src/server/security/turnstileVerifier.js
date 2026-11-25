import 'server-only';

/**
 * Determina si Cloudflare Turnstile se encuentra configurado activamente en producción.
 */
export function isTurnstileEnforced() {
  const secretKey = process.env.TURNSTILE_SECRET_KEY;
  return Boolean(
    process.env.NODE_ENV === 'production' &&
    secretKey &&
    secretKey !== 'tu-secret-key-turnstile' &&
    !secretKey.startsWith('1x0000')
  );
}

function isAllowedTurnstileHostname(hostname) {
  if (!hostname) return true;
  const clean = String(hostname).toLowerCase();
  const allowed = new Set(['importaplus.com.ar', 'www.importaplus.com.ar']);
  try {
    if (process.env.NEXT_PUBLIC_APP_ORIGIN) {
      allowed.add(new URL(process.env.NEXT_PUBLIC_APP_ORIGIN).hostname.toLowerCase());
    }
  } catch {}
  for (const vercelEnv of [process.env.VERCEL_URL, process.env.VERCEL_PROJECT_PRODUCTION_URL]) {
    if (vercelEnv) {
      allowed.add(vercelEnv.replace(/^https?:\/\//i, '').split('/')[0].toLowerCase());
    }
  }
  return allowed.has(clean);
}

/**
 * Verificador de Token Cloudflare Turnstile (Anti-Bot) con timeout de red y validación de hostname.
 */
export async function verifyTurnstileToken(token, clientIp = '') {
  if (!isTurnstileEnforced()) {
    return { success: true, bypassed: true };
  }

  if (!token || typeof token !== 'string' || token.length > 2048) {
    return { success: false, error: 'Token de seguridad anti-bot requerido' };
  }

  const controller = new AbortController();
  const timer = setTimeout(() => controller.abort(), 5000);

  try {
    const formData = new URLSearchParams();
    formData.append('secret', process.env.TURNSTILE_SECRET_KEY);
    formData.append('response', token);
    if (clientIp) {
      formData.append('remoteip', clientIp);
    }

    const response = await fetch('https://challenges.cloudflare.com/turnstile/v0/siteverify', {
      method: 'POST',
      body: formData,
      headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
      signal: controller.signal
    });

    if (!response.ok) {
      return { success: false, error: 'Servicio de verificación no disponible' };
    }

    const data = await response.json();
    if (!data.success) {
      return {
        success: false,
        hostname: data.hostname,
        error: data['error-codes'] ? data['error-codes'].join(', ') : 'Verificación rechazada'
      };
    }

    if (!isAllowedTurnstileHostname(data.hostname)) {
      return { success: false, error: 'Hostname de desafío anti-bot no coincide' };
    }

    return {
      success: true,
      hostname: data.hostname,
      error: null
    };
  } catch (err) {
    console.error('[TURNSTILE_ERROR]:', err.message);
    return { success: false, error: 'Fallo al verificar token anti-bot' };
  } finally {
    clearTimeout(timer);
  }
}
