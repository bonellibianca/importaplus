import { NextResponse } from 'next/server';
import { checkRateLimit, getClientIp } from '@/server/security/rateLimiter';
import { PuertoDAL } from '@/server/dal/PuertoDAL';
import { OperadorDAL } from '@/server/dal/OperadorDAL';
import { PaisDAL } from '@/server/dal/PaisDAL';
import { ForwarderDAL } from '@/server/dal/ForwarderDAL';

export const runtime = 'nodejs';
export const dynamic = 'force-dynamic';

export async function GET(request) {
  const ip = getClientIp(request);
  const rateStatus = checkRateLimit(ip, 'puertos_dir', 30, 60 * 1000);
  if (!rateStatus.allowed) {
    return NextResponse.json(
      { error: `Límite de consultas excedido. Reintente en ${rateStatus.resetInSeconds}s.` },
      { status: 429, headers: { 'Retry-After': String(rateStatus.resetInSeconds) } }
    );
  }

  try {
    const { searchParams } = new URL(request.url);
    const paisIso = (searchParams.get('pais') || '').trim().slice(0, 3);
    const aduana = (searchParams.get('aduana') || '').trim().slice(0, 80);

    const [puertos, operadores, paises, forwarders] = await Promise.all([
      paisIso ? PuertoDAL.obtenerSugeridosPorPais(paisIso) : PuertoDAL.obtenerTodos(),
      OperadorDAL.obtenerActivos(aduana),
      PaisDAL.obtenerTodos(),
      ForwarderDAL.obtenerGrillaCuatro()
    ]);

    return NextResponse.json({
      puertos,
      operadores,
      paises,
      forwarders
    }, {
      status: 200,
      headers: { 'Cache-Control': 'public, s-maxage=300, stale-while-revalidate=3600' }
    });
  } catch (error) {
    console.error('[API_PUERTOS_ERROR]:', error.message);
    return NextResponse.json({ error: 'Error al consultar puertos y aduanas' }, { status: 500 });
  }
}
