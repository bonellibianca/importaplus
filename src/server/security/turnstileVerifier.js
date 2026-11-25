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

/**
 * Verificador de Token Cloudflare Turnstile (Anti-Bot)
 * Corrige bug de omisión cuando el cliente no envía turnstileToken y evita fail-open inseguro.
 */
export async function verifyTurnstileToken(token, clientIp = '') {
  if (!isTurnstileEnforced()) {
    return { success: true, bypassed: true };
  }

  if (!token) {
    return { success: false, error: 'Token de seguridad anti-bot requerido' };
  }

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
      headers: { 'Content-Type': 'application/x-www-form-urlencoded' }
    });

    if (!response.ok) {
      return { success: false, error: 'Servicio de verificación no disponible' };
    }

    const data = await response.json();
    return {
      success: Boolean(data.success),
      hostname: data.hostname,
      error: data['error-codes'] ? data['error-codes'].join(', ') : null
    };
  } catch (err) {
    console.error('[TURNSTILE_ERROR]:', err.message);
    return { success: false, error: 'Fallo al verificar token anti-bot' };
  }
}
