import { NextResponse } from 'next/server';
import { checkRateLimit, getClientIp } from '@/server/security/rateLimiter';
import { SolicitudLeadSchema } from '@/server/security/schemas';
import { verifyTurnstileToken } from '@/server/security/turnstileVerifier';
import { LeadContactoBLL } from '@/server/bll/LeadContactoBLL';
import { DomainValidationError } from '@/server/bll/CalculadoraArancelariaBLL';

export const runtime = 'nodejs';
export const dynamic = 'force-dynamic';

export async function POST(request) {
  const ip = getClientIp(request);

  const rateStatus = checkRateLimit(ip, 'leads_endpoint', 4, 60 * 60 * 1000);
  if (!rateStatus.allowed) {
    return NextResponse.json(
      { error: `Límite de solicitudes alcanzado. Intente nuevamente en ${Math.ceil(rateStatus.resetInSeconds / 60)} minutos.` },
      { status: 429, headers: { 'Retry-After': String(rateStatus.resetInSeconds) } }
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
    const validacion = SolicitudLeadSchema.safeParse(rawBody);
    if (!validacion.success) {
      return NextResponse.json(
        { error: 'Datos de contacto inválidos', detalles: validacion.error.flatten().fieldErrors },
        { status: 400 }
      );
    }

    const turnstileCheck = await verifyTurnstileToken(validacion.data.turnstileToken, ip);
    if (!turnstileCheck.success) {
      return NextResponse.json(
        { error: 'Validación anti-bot requerida.' },
        { status: 403 }
      );
    }

    const resultado = await LeadContactoBLL.registrarSolicitud(validacion.data);

    return NextResponse.json(resultado, { status: 201 });
  } catch (error) {
    if (error instanceof DomainValidationError) {
      return NextResponse.json(
        { error: error.message },
        { status: error.statusCode || 404 }
      );
    }

    console.error('[API_LEADS_ERROR]:', error.message);
    return NextResponse.json(
      { error: 'No se pudo procesar la solicitud de contacto' },
      { status: 500 }
    );
  }
}
