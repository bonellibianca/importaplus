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

const ALLOWED_HOSTNAMES = new Set([
  'importaplus.com.ar',
  'www.importaplus.com.ar',
  'localhost',
  '127.0.0.1'
]);

function extractHostname(urlOrOrigin) {
  if (!urlOrOrigin) return '';
  try {
    return new URL(urlOrOrigin).hostname.toLowerCase();
  } catch {
    return '';
  }
}

/**
 * Anillo 1 de Seguridad: Middleware Edge en Vercel
 * - Bloqueo de User-Agents automatizados y scrapers.
 * - Protección CSRF estricta validando hostname exacto (corrige bug de bypass por subcadena).
 */
export function middleware(request) {
  const { pathname } = request.nextUrl;

  if (pathname.startsWith('/api/')) {
    const userAgent = (request.headers.get('user-agent') || '').toLowerCase();

    if (!userAgent || BOT_USER_AGENTS.some((bot) => userAgent.includes(bot))) {
      return NextResponse.json(
        { error: 'Acceso automatizado no autorizado por políticas de seguridad de Importa Plus.' },
        { status: 403 }
      );
    }

    if (request.method === 'POST') {
      const origin = request.headers.get('origin') || '';
      const referer = request.headers.get('referer') || '';
      const hostHeader = (request.headers.get('host') || '').split(':')[0].toLowerCase();

      const sourceHostname = extractHostname(origin) || extractHostname(referer);
      const isAllowedOrigin =
        process.env.NODE_ENV !== 'production' ||
        (sourceHostname && (
          sourceHostname === hostHeader ||
          ALLOWED_HOSTNAMES.has(sourceHostname) ||
          sourceHostname.endsWith('.vercel.app')
        ));

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
