import { NextResponse } from 'next/server';
import { checkRateLimit } from '@/server/security/rateLimiter';
import { BusquedaNcmSchema } from '@/server/security/schemas';
import { NcmDAL } from '@/server/dal/NcmDAL';

export const runtime = 'nodejs'; // Fuerza ejecución en Node.js puro dentro de Vercel
export const dynamic = 'force-dynamic';

export async function GET(request) {
  // 1. Extraer IP real detrás del proxy/Edge de Vercel
  const forwardedFor = request.headers.get('x-forwarded-for');
  const ip = forwardedFor ? forwardedFor.split(',')[0].trim() : '127.0.0.1';

  // 2. Control de Rate Limiting (Máx 25 peticiones por cada 10 segundos por IP)
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

    // Si tiene menos de 2 caracteres, devuelve array vacío inmediatamente (< 5ms)
    if (rawQuery.trim().length < 2) {
      return NextResponse.json({ results: [] });
    }

    // 3. Validación con Zod
    const validacion = BusquedaNcmSchema.safeParse({ q: rawQuery });
    if (!validacion.success) {
      return NextResponse.json(
        { error: 'Parámetro de búsqueda inválido', detalles: validacion.error.flatten().fieldErrors },
        { status: 400 }
      );
    }

    // 4. Consulta a la capa DAL sobre el catálogo Excel (< 250ms)
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
