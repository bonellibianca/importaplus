import 'server-only';

/**
 * Control de Frecuencia de Solicitudes en Memoria (Sliding Window Rate Limiter)
 * Protege contra abusos, Denial of Wallet y scraping masivo de NCM sin requerir Redis externo.
 */
const ipRequestMap = new Map();

/**
 * Verifica si la IP solicitante se encuentra dentro de su cuota de peticiones
 * @param {string} ip - Dirección IP cliente (extraída de x-forwarded-for o x-real-ip)
 * @param {string} endpointKey - Identificador ('search', 'simular', 'leads')
 * @param {number} limit - Límite de llamadas permitidas
 * @param {number} windowMs - Ventana de tiempo en milisegundos
 * @returns {{ allowed: boolean, remaining: number, resetInSeconds: number }}
 */
export function checkRateLimit(ip, endpointKey, limit, windowMs) {
  const now = Date.now();
  const safeIp = ip || '127.0.0.1';
  const key = `${safeIp}:${endpointKey}`;
  const record = ipRequestMap.get(key) || { count: 0, resetTime: now + windowMs };

  if (now > record.resetTime) {
    record.count = 1;
    record.resetTime = now + windowMs;
  } else {
    record.count += 1;
  }

  ipRequestMap.set(key, record);

  // Limpieza periódica de memoria para evitar Memory Leaks en Serverless
  if (ipRequestMap.size > 5000) {
    for (const [k, v] of ipRequestMap.entries()) {
      if (now > v.resetTime) {
        ipRequestMap.delete(k);
      }
    }
  }

  const isAllowed = record.count <= limit;
  const remaining = Math.max(0, limit - record.count);
  const resetInSeconds = Math.max(1, Math.ceil((record.resetTime - now) / 1000));

  return {
    allowed: isAllowed,
    remaining,
    resetInSeconds
  };
}
