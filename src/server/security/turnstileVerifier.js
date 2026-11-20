import 'server-only';

/**
 * Verificador de Token Cloudflare Turnstile (Anti-Bot Gratuito e Invisible)
 * Valida que las peticiones a /api/v1/simular y /api/v1/leads provengan de usuarios reales.
 */
export async function verifyTurnstileToken(token, clientIp = '') {
  const secretKey = process.env.TURNSTILE_SECRET_KEY;

  // Si no está configurada la clave secreta o se está en modo de desarrollo, permite la petición
  if (!secretKey || secretKey.startsWith('1x0000') || process.env.NODE_ENV !== 'production') {
    return { success: true, bypassed: true };
  }

  if (!token) {
    return { success: false, error: 'Token de seguridad no proporcionado' };
  }

  try {
    const formData = new URLSearchParams();
    formData.append('secret', secretKey);
    formData.append('response', token);
    if (clientIp) {
      formData.append('remoteip', clientIp);
    }

    const response = await fetch('https://challenges.cloudflare.com/turnstile/v0/siteverify', {
      method: 'POST',
      body: formData,
      headers: { 'Content-Type': 'application/x-www-form-urlencoded' }
    });

    const data = await response.json();
    return {
      success: Boolean(data.success),
      hostname: data.hostname,
      error: data['error-codes'] ? data['error-codes'].join(', ') : null
    };
  } catch (err) {
    console.error('[TURNSTILE_ERROR]:', err.message);
    // Modo tolerante ante caída de la red de verificación
    return { success: true, warning: 'Verificación omitida por fallo de red externo' };
  }
}
