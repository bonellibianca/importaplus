import 'server-only';

/**
 * Cliente Ligero Supabase PostgREST + RPC con Caché L1 en Memoria (Protección Cuota FREE)
 * - Incluye configuración pública oficial de solo lectura (RLS) por defecto para que cualquier
 *   desarrollador que clone el repositorio desde GitHub lea en tiempo real los cambios de Supabase
 *   sin depender de archivos locales desconectados.
 */

export const DEFAULT_PUBLIC_SUPABASE_URL = 'https://exqyupuliqxsxejgouho.supabase.co';
export const DEFAULT_PUBLIC_SUPABASE_PUBLISHABLE_KEY = 'sb_publishable_HPs_CdVHnpT1PqyRUwfz9Q_g5rBulj_';

const memoryCache = new Map();
const MAX_CACHE_ENTRIES = 400;

function isPlaceholder(val) {
  return (
    !val ||
    val.includes('TU_PROJECT_REF') ||
    val.endsWith('...') ||
    val === 'tu-anon-key' ||
    val === 'tu-service-role-key'
  );
}

function getSupabaseEnv() {
  const rawUrl = (process.env.NEXT_PUBLIC_SUPABASE_URL || process.env.SUPABASE_URL || '').trim().replace(/\/+$/, '');
  const rawAnon = (process.env.NEXT_PUBLIC_SUPABASE_ANON_KEY || '').trim();
  const rawService = (process.env.SUPABASE_SERVICE_ROLE_KEY || '').trim();

  const url = (!isPlaceholder(rawUrl) && rawUrl.startsWith('https://'))
    ? rawUrl
    : DEFAULT_PUBLIC_SUPABASE_URL;

  const anonKey = !isPlaceholder(rawAnon)
    ? rawAnon
    : DEFAULT_PUBLIC_SUPABASE_PUBLISHABLE_KEY;

  // Una clave sb_publishable_ NO es clave secreta de escritura (service_role)
  const validService = !isPlaceholder(rawService) && !rawService.startsWith('sb_publishable_');
  const serviceKey = validService ? rawService : '';

  const validUrl = url.startsWith('https://') && !isPlaceholder(url);
  const validAnon = !isPlaceholder(anonKey);

  return {
    url,
    anonKey,
    serviceKey,
    canRead: validUrl && (validAnon || validService),
    canWrite: validUrl && validService
  };
}

export function isSupabaseConfigured() {
  return getSupabaseEnv().canRead;
}

export function invalidateSupabaseCache(prefix = '') {
  if (!prefix) {
    memoryCache.clear();
    return;
  }
  for (const key of memoryCache.keys()) {
    if (key.startsWith(prefix)) {
      memoryCache.delete(key);
    }
  }
}

export async function getCachedOrFetch(cacheKey, ttlMs, fetcher) {
  const now = Date.now();
  const hit = memoryCache.get(cacheKey);
  if (hit && now < hit.expiresAt) {
    return hit.data;
  }

  const data = await fetcher();
  if (data !== null && data !== undefined) {
    if (memoryCache.size >= MAX_CACHE_ENTRIES) {
      const oldestKey = memoryCache.keys().next().value;
      memoryCache.delete(oldestKey);
    }
    memoryCache.set(cacheKey, { data, expiresAt: now + ttlMs });
  }
  return data;
}

function buildSupabaseHeaders(token, extra = {}) {
  const headers = {
    apikey: token,
    ...extra
  };
  // Solo adjuntar Authorization Bearer cuando el token es un JWT (eyJ...);
  // las nuevas API keys opacas de Supabase (sb_publishable_ / sb_secret_) usan apikey directamente.
  if (token && token.startsWith('eyJ')) {
    headers.Authorization = `Bearer ${token}`;
  }
  return headers;
}

/**
 * Consulta SELECT sobre PostgREST usando anonKey/publishableKey (respeta RLS) o serviceKey si se solicita.
 */
export async function supabaseSelect(table, queryString = 'select=*', { useServiceRole = false, timeoutMs = 4500 } = {}) {
  const env = getSupabaseEnv();
  if (!env.canRead) return null;

  const token = (useServiceRole && env.serviceKey) ? env.serviceKey : (env.anonKey || env.serviceKey);
  const controller = new AbortController();
  const timer = setTimeout(() => controller.abort(), timeoutMs);

  try {
    const res = await fetch(`${env.url}/rest/v1/${table}?${queryString}`, {
      method: 'GET',
      headers: buildSupabaseHeaders(token, { Accept: 'application/json' }),
      cache: 'no-store',
      signal: controller.signal
    });

    if (!res.ok) {
      const errText = await res.text().catch(() => '');
      console.warn(`[SUPABASE_SELECT_WARN] ${table} (${res.status}):`, errText.slice(0, 180));
      return null;
    }
    return await res.json();
  } catch (err) {
    console.warn(`[SUPABASE_SELECT_FALLBACK] ${table}:`, err.message);
    return null;
  } finally {
    clearTimeout(timer);
  }
}

/**
 * Inserción protegida usando exclusivamente service_role / secret key (Backend-Only).
 */
export async function supabaseInsert(table, payload, { timeoutMs = 5000 } = {}) {
  const env = getSupabaseEnv();
  if (!env.canWrite) return { ok: false, reason: 'not_configured' };

  const controller = new AbortController();
  const timer = setTimeout(() => controller.abort(), timeoutMs);

  try {
    const res = await fetch(`${env.url}/rest/v1/${table}`, {
      method: 'POST',
      headers: buildSupabaseHeaders(env.serviceKey, {
        'Content-Type': 'application/json',
        Prefer: 'return=minimal'
      }),
      body: JSON.stringify(payload),
      signal: controller.signal
    });

    if (!res.ok) {
      const errText = await res.text().catch(() => '');
      console.warn(`[SUPABASE_INSERT_WARN] ${table} (${res.status}):`, errText.slice(0, 200));
      return { ok: false, status: res.status, error: errText };
    }
    return { ok: true };
  } catch (err) {
    console.warn(`[SUPABASE_INSERT_FALLBACK] ${table}:`, err.message);
    return { ok: false, error: err.message };
  } finally {
    clearTimeout(timer);
  }
}

/**
 * Invoca una función RPC PostgreSQL (ej: obtener_simulacion_por_id).
 */
export async function supabaseRpc(fnName, args = {}, { useServiceRole = false, timeoutMs = 4500 } = {}) {
  const env = getSupabaseEnv();
  if (!env.canRead) return null;

  const token = (useServiceRole && env.serviceKey) ? env.serviceKey : (env.anonKey || env.serviceKey);
  const controller = new AbortController();
  const timer = setTimeout(() => controller.abort(), timeoutMs);

  try {
    const res = await fetch(`${env.url}/rest/v1/rpc/${fnName}`, {
      method: 'POST',
      headers: buildSupabaseHeaders(token, {
        'Content-Type': 'application/json'
      }),
      body: JSON.stringify(args),
      signal: controller.signal
    });

    if (!res.ok) return null;
    return await res.json();
  } catch {
    return null;
  } finally {
    clearTimeout(timer);
  }
}
