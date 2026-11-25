import { NextResponse } from 'next/server';
import { checkRateLimit } from '@/server/security/rateLimiter';
import { SimularOperacionSchema } from '@/server/security/schemas';
import { verifyTurnstileToken } from '@/server/security/turnstileVerifier';
import { CalculadoraArancelariaBLL } from '@/server/bll/CalculadoraArancelariaBLL';

export const runtime = 'nodejs';
export const dynamic = 'force-dynamic';

export async function POST(request) {
  const forwardedFor = request.headers.get('x-forwarded-for');
  const ip = forwardedFor ? forwardedFor.split(',')[0].trim() : '127.0.0.1';

  const rateStatus = checkRateLimit(ip, 'simular_endpoint', 10, 60 * 1000);
  if (!rateStatus.allowed) {
    return NextResponse.json(
      { error: `Demasiadas solicitudes de simulación. Intente nuevamente en ${rateStatus.resetInSeconds} segundos.` },
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
    const rawBody = await request.json();

    const validacion = SimularOperacionSchema.safeParse(rawBody);
    if (!validacion.success) {
      return NextResponse.json(
        { error: 'Datos comerciales inválidos', detalles: validacion.error.flatten().fieldErrors },
        { status: 400 }
      );
    }

    // Corrección Bug de Seguridad: verificar siempre Turnstile cuando está activo en producción
    const turnstileCheck = await verifyTurnstileToken(validacion.data.turnstileToken, ip);
    if (!turnstileCheck.success) {
      return NextResponse.json(
        { error: 'Verificación de seguridad fallida. Por favor recargue la página.' },
        { status: 403 }
      );
    }

    const resultado = await CalculadoraArancelariaBLL.procesarSimulacion(validacion.data);

    return NextResponse.json(
      {
        simulacion: resultado,
        puertosSugeridos: resultado.puertosRecomendados || []
      },
      {
        status: 200,
        headers: {
          'X-RateLimit-Remaining': String(rateStatus.remaining),
          'Cache-Control': 'no-store, max-age=0'
        }
      }
    );
  } catch (error) {
    console.error('[API_SIMULAR_ERROR]:', error.message);
    return NextResponse.json(
      { error: error.message || 'No se pudo procesar la liquidación aduanera' },
      { status: 500 }
    );
  }
}
