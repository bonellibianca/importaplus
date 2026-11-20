import { NextResponse } from 'next/server';
import { PuertoDAL } from '@/server/dal/PuertoDAL';
import { OperadorDAL } from '@/server/dal/OperadorDAL';
import { PaisDAL } from '@/server/dal/PaisDAL';

export const runtime = 'nodejs'; // Fuerza ejecución en Node.js puro dentro de Vercel
export const dynamic = 'force-dynamic';

export async function GET(request) {
  try {
    const { searchParams } = new URL(request.url);
    const paisIso = searchParams.get('pais') || '';
    const aduana = searchParams.get('aduana') || '';

    const [puertos, operadores, paises] = await Promise.all([
      paisIso ? PuertoDAL.obtenerSugeridosPorPais(paisIso) : PuertoDAL.obtenerTodos(),
      OperadorDAL.obtenerActivos(aduana),
      PaisDAL.obtenerTodos()
    ]);

    return NextResponse.json({
      puertos,
      operadores,
      paises
    }, {
      status: 200,
      headers: { 'Cache-Control': 'public, s-maxage=300, stale-while-revalidate=600' }
    });
  } catch (error) {
    console.error('[API_PUERTOS_ERROR]:', error.message);
    return NextResponse.json({ error: 'Error al consultar puertos y aduanas' }, { status: 500 });
  }
}
