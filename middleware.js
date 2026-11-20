import { NextResponse } from 'next/server';

const BOT_USER_AGENTS = [
  'python-requests',
  'curl',
  'wget',
  'scrapy',
  'postmanruntime',
  'insomnia',
  'httpclient',
  'aiohttp'
];

/**
 * Anillo 1 de Seguridad: Middleware Edge en Vercel
 * Intercepta peticiones antes de llegar al motor Node.js y a la capa de persistencia en Excel.
 * - Bloqueo de User-Agents automatizados y scrapers.
 * - Protección CSRF y Hotlinking validando Origin / Referer en métodos POST.
 */
export function middleware(request) {
  const { pathname } = request.nextUrl;

  // Proteger todas las rutas de nuestra API privada (/api/v1/ y /api/)
  if (pathname.startsWith('/api/')) {
    const userAgent = (request.headers.get('user-agent') || '').toLowerCase();

    // 1. Bloquear herramientas de scraping y clientes HTTP automatizados
    if (!userAgent || BOT_USER_AGENTS.some((bot) => userAgent.includes(bot))) {
      return NextResponse.json(
        { error: 'Acceso automatizado no autorizado por políticas de seguridad de Importa Plus.' },
        { status: 403 }
      );
    }

    // 2. Verificar Origin / Referer en métodos POST (CSRF & Hotlinking Protection)
    if (request.method === 'POST') {
      const origin = request.headers.get('origin') || '';
      const referer = request.headers.get('referer') || '';
      const host = request.headers.get('host') || '';

      const source = origin || referer;
      const isAllowedOrigin =
        (source && (
          source.includes(host) ||
          source.includes('importaplus.com.ar') ||
          source.includes('localhost')
        )) ||
        process.env.NODE_ENV !== 'production';

      if (!isAllowedOrigin) {
        return NextResponse.json(
          { error: 'Origen de petición no autorizado por políticas anti-CSRF.' },
          { status: 403 }
        );
      }
    }

    const forwardedFor = request.headers.get('x-forwarded-for');
    const ip = forwardedFor ? forwardedFor.split(',')[0].trim() : '127.0.0.1';

    const response = NextResponse.next();
    response.headers.set('X-Client-IP', ip);
    response.headers.set('X-Robots-Tag', 'noindex, nofollow');
    return response;
  }

  return NextResponse.next();
}

export const config = {
  matcher: ['/api/:path*']
};
