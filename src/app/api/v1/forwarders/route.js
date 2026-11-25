import { NextResponse } from 'next/server';
import { checkRateLimit, getClientIp } from '@/server/security/rateLimiter';
import { ForwarderDAL } from '@/server/dal/ForwarderDAL';

export const runtime = 'nodejs';
export const dynamic = 'force-dynamic';

export async function GET(request) {
  const ip = getClientIp(request);
  const rateStatus = checkRateLimit(ip, 'forwarders_dir', 30, 60 * 1000);
  if (!rateStatus.allowed) {
    return NextResponse.json(
      { error: `Límite de consultas excedido. Reintente en ${rateStatus.resetInSeconds}s.` },
      { status: 429, headers: { 'Retry-After': String(rateStatus.resetInSeconds) } }
    );
  }

  try {
    const { forwarders, source } = await ForwarderDAL.obtenerConOrigen();
    const cacheHeader = source === 'local_admin_sync'
      ? 'no-store'
      : 'public, s-maxage=300, stale-while-revalidate=3600';

    return NextResponse.json(
      { forwarders, source },
      {
        status: 200,
        headers: { 'Cache-Control': cacheHeader }
      }
    );
  } catch (error) {
    console.error('[API_FORWARDERS_ERROR]:', error.message);
    return NextResponse.json({ error: 'Error al obtener forwarders' }, { status: 500 });
  }
}
