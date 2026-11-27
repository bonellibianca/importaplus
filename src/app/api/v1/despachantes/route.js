import { NextResponse } from 'next/server';
import { checkRateLimit, getClientIp } from '@/server/security/rateLimiter';
import { DespachanteDAL } from '@/server/dal/DespachanteDAL';

export const runtime = 'nodejs';
export const dynamic = 'force-dynamic';

export async function GET(request) {
  const ip = getClientIp(request);
  const rateStatus = checkRateLimit(ip, 'despachantes_dir', 60, 60 * 1000);
  if (!rateStatus.allowed) {
    return NextResponse.json(
      { error: `Límite de consultas excedido. Reintente en ${rateStatus.resetInSeconds}s.` },
      { status: 429, headers: { 'Retry-After': String(rateStatus.resetInSeconds) } }
    );
  }

  try {
    const { despachantes, source } = await DespachanteDAL.obtenerConOrigen();
    return NextResponse.json(
      { despachantes, source },
      {
        status: 200,
        headers: { 'Cache-Control': 'no-store' }
      }
    );
  } catch (error) {
    console.error('[API_DESPACHANTES_ERROR]:', error.message);
    return NextResponse.json({ error: 'Error al obtener despachantes de aduana' }, { status: 500 });
  }
}
