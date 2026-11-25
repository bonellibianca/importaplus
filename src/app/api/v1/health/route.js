import { NextResponse } from 'next/server';
import { checkRateLimit, getClientIp } from '@/server/security/rateLimiter';
import { getCachedOrFetch, supabaseSelect, isSupabaseConfigured } from '@/server/dal/supabaseClient';

export const runtime = 'nodejs';
export const dynamic = 'force-dynamic';

/**
 * Endpoint Keep-Alive protegido contra agotamiento de cuota Supabase FREE.
 * - Valida CRON_SECRET si está definido en entorno de producción.
 * - Aplica Rate Limiting por IP y caché L1 de 5 minutos para peticiones sin token de cron.
 */
export async function GET(request) {
  const ip = getClientIp(request);
  const rateStatus = checkRateLimit(ip, 'health_ping', 6, 60 * 1000);
  if (!rateStatus.allowed) {
    return NextResponse.json(
      { error: 'Too many health checks' },
      { status: 429, headers: { 'Retry-After': String(rateStatus.resetInSeconds) } }
    );
  }

  const cronSecret = (process.env.CRON_SECRET || '').trim();
  const authHeader = request.headers.get('authorization') || '';
  const isAuthorizedCron = cronSecret && authHeader === `Bearer ${cronSecret}`;

  if (process.env.NODE_ENV === 'production' && cronSecret && !isAuthorizedCron) {
    return NextResponse.json({ error: 'No autorizado' }, { status: 401 });
  }

  if (!isSupabaseConfigured()) {
    return NextResponse.json({ ok: true, supabase: 'not_configured_hybrid_mode', ts: new Date().toISOString() });
  }

  const ping = isAuthorizedCron
    ? await supabaseSelect('parametros_sistema', 'select=clave&limit=1')
    : await getCachedOrFetch('health:supabase_ping', 5 * 60 * 1000, async () => {
        return await supabaseSelect('parametros_sistema', 'select=clave&limit=1');
      });

  return NextResponse.json({
    ok: Array.isArray(ping),
    supabase: Array.isArray(ping) ? 'active' : 'degraded',
    ts: new Date().toISOString()
  });
}
