import { NextResponse } from 'next/server';
import { checkRateLimit, getClientIp } from '@/server/security/rateLimiter';
import { SimularOperacionSchema } from '@/server/security/schemas';
import { verifyTurnstileToken } from '@/server/security/turnstileVerifier';
import { CalculadoraArancelariaBLL, DomainValidationError } from '@/server/bll/CalculadoraArancelariaBLL';

export const runtime = 'nodejs';
export const dynamic = 'force-dynamic';

export async function POST(request) {
  const ip = getClientIp(request);

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

  let rawBody;
  try {
    rawBody = await request.json();
  } catch {
    return NextResponse.json(
      { error: 'Cuerpo JSON malformado o vacío.' },
      { status: 400 }
    );
  }

  try {
    const validacion = SimularOperacionSchema.safeParse(rawBody);
    if (!validacion.success) {
      return NextResponse.json(
        { error: 'Datos comerciales inválidos', detalles: validacion.error.flatten().fieldErrors },
        { status: 400 }
      );
    }

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
    if (error instanceof DomainValidationError) {
      return NextResponse.json(
        { error: error.message },
        { status: error.statusCode || 422 }
      );
    }

    console.error('[API_SIMULAR_ERROR]:', error.message);
    return NextResponse.json(
      { error: 'No se pudo procesar la liquidación aduanera en este momento.' },
      { status: 500 }
    );
  }
}
