import { NextResponse } from 'next/server';
import { checkRateLimit } from '@/server/security/rateLimiter';
import { SimularOperacionSchema } from '@/server/security/schemas';
import { verifyTurnstileToken } from '@/server/security/turnstileVerifier';
import { CalculadoraArancelariaBLL } from '@/server/bll/CalculadoraArancelariaBLL';

export const runtime = 'nodejs'; // Fuerza ejecución en Node.js puro dentro de Vercel
export const dynamic = 'force-dynamic';

export async function POST(request) {
  // 1. Extraer IP real
  const forwardedFor = request.headers.get('x-forwarded-for');
  const ip = forwardedFor ? forwardedFor.split(',')[0].trim() : '127.0.0.1';

  // 2. Control de Rate Limit (Máximo 10 simulaciones por minuto por IP)
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

    // 3. Validación de contrato estricto con Zod
    const validacion = SimularOperacionSchema.safeParse(rawBody);
    if (!validacion.success) {
      return NextResponse.json(
        { error: 'Datos comerciales inválidos', detalles: validacion.error.flatten().fieldErrors },
        { status: 400 }
      );
    }

    // 4. Verificación Anti-Bot Cloudflare Turnstile (si se incluye token)
    if (validacion.data.turnstileToken) {
      const turnstileCheck = await verifyTurnstileToken(validacion.data.turnstileToken, ip);
      if (!turnstileCheck.success) {
        return NextResponse.json(
          { error: 'Verificación de seguridad fallida. Por favor recargue la página.' },
          { status: 403 }
        );
      }
    }

    // 5. Delegación a la Capa BLL (Server-Side)
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
