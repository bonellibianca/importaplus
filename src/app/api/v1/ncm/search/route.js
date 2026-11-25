import { NextResponse } from 'next/server';
import { checkRateLimit, getClientIp } from '@/server/security/rateLimiter';
import { BusquedaNcmSchema } from '@/server/security/schemas';
import { NcmDAL } from '@/server/dal/NcmDAL';

export const runtime = 'nodejs';
export const dynamic = 'force-dynamic';

export async function GET(request) {
  const ip = getClientIp(request);

  const rateStatus = checkRateLimit(ip, 'ncm_search', 25, 10 * 1000);
  if (!rateStatus.allowed) {
    return NextResponse.json(
      { error: `Límite de búsquedas alcanzado. Aguarde ${rateStatus.resetInSeconds}s.` },
      {
        status: 429,
        headers: {
          'Retry-After': String(rateStatus.resetInSeconds),
          'X-RateLimit-Remaining': '0'
        }
      }
    );
  }

  try {
    const { searchParams } = new URL(request.url);
    const rawQuery = searchParams.get('q') || '';

    if (rawQuery.trim().length < 2) {
      return NextResponse.json({ results: [] });
    }

    const validacion = BusquedaNcmSchema.safeParse({ q: rawQuery });
    if (!validacion.success) {
      return NextResponse.json(
        { error: 'Parámetro de búsqueda inválido', detalles: validacion.error.flatten().fieldErrors },
        { status: 400 }
      );
    }

    const results = await NcmDAL.buscarPorDescripcion(validacion.data.q, 10);

    return NextResponse.json(
      { results },
      {
        status: 200,
        headers: {
          'Cache-Control': 'public, s-maxage=60, stale-while-revalidate=300',
          'X-RateLimit-Remaining': String(rateStatus.remaining)
        }
      }
    );
  } catch (error) {
    console.error('[API_NCM_SEARCH_ERROR]:', error.message);
    return NextResponse.json({ error: 'Error al consultar nomenclador' }, { status: 500 });
  }
}
