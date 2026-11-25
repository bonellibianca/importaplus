import 'server-only';

const ipRequestMap = new Map();
const MAX_RATE_LIMIT_ENTRIES = 5000;

/**
 * Extrae la IP cliente de forma segura priorizando cabeceras de plataforma confiables
 * (inyectadas por el Middleware Edge, Vercel o Cloudflare) y validando formato/longitud.
 */
export function getClientIp(request) {
  if (!request || !request.headers) return '127.0.0.1';

  const candidates = [
    request.headers.get('x-client-ip'),
    request.headers.get('x-vercel-forwarded-for'),
    request.headers.get('cf-connecting-ip'),
    request.headers.get('x-real-ip'),
    request.headers.get('x-forwarded-for')
  ];

  for (const raw of candidates) {
    if (!raw) continue;
    const ip = raw.split(',')[0].trim();
    if (ip && ip.length <= 64 && /^[0-9a-fA-F.:]+$/.test(ip)) {
      return ip;
    }
  }

  return '127.0.0.1';
}

/**
 * Control de Frecuencia de Solicitudes en Memoria con cota estricta anti-DoS (LRU/FIFO eviction).
 * Protege contra abusos, Denial of Wallet y agotamiento de memoria por IP spoofing.
 */
export function checkRateLimit(ip, endpointKey, limit, windowMs) {
  const now = Date.now();
  const safeIp = (ip && String(ip).length <= 64) ? String(ip) : '127.0.0.1';
  const key = `${safeIp}:${endpointKey}`;
  const existing = ipRequestMap.get(key);

  let record;
  if (!existing || now > existing.resetTime) {
    record = { count: 1, resetTime: now + windowMs };
  } else {
    record = { count: existing.count + 1, resetTime: existing.resetTime };
    ipRequestMap.delete(key);
  }

  if (ipRequestMap.size >= MAX_RATE_LIMIT_ENTRIES) {
    for (const [k, v] of ipRequestMap.entries()) {
      if (now > v.resetTime) {
        ipRequestMap.delete(k);
      }
    }
    while (ipRequestMap.size >= MAX_RATE_LIMIT_ENTRIES) {
      const oldestKey = ipRequestMap.keys().next().value;
      if (oldestKey === undefined) break;
      ipRequestMap.delete(oldestKey);
    }
  }

  ipRequestMap.set(key, record);

  const isAllowed = record.count <= limit;
  const remaining = Math.max(0, limit - record.count);
  const resetInSeconds = Math.max(1, Math.ceil((record.resetTime - now) / 1000));

  return {
    allowed: isAllowed,
    remaining,
    resetInSeconds
  };
}
