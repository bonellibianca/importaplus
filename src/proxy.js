import { NextResponse } from 'next/server';

const BOT_USER_AGENTS = [
  'python-requests',
  'curl',
  'wget',
  'scrapy',
  'postmanruntime',
  'insomnia',
  'httpclient',
  'aiohttp',
  'go-http-client',
  'libwww-perl'
];

const PROD_ALLOWED_HOSTNAMES = new Set([
  'importaplus.com.ar',
  'www.importaplus.com.ar'
]);

const DEV_ALLOWED_HOSTNAMES = new Set([
  'localhost',
  '127.0.0.1'
]);

const MUTATING_METHODS = new Set(['POST', 'PUT', 'PATCH', 'DELETE']);

function extractHostname(urlOrOrigin) {
  if (!urlOrOrigin) return '';
  try {
    return new URL(urlOrOrigin).hostname.toLowerCase();
  } catch {
    return '';
  }
}

function buildAllowedHostnames(hostHeader) {
  const allowed = new Set(PROD_ALLOWED_HOSTNAMES);
  if (hostHeader) {
    allowed.add(hostHeader);
  }

  const configuredOriginHost = extractHostname(process.env.NEXT_PUBLIC_APP_ORIGIN);
  if (configuredOriginHost) {
    allowed.add(configuredOriginHost);
  }

  // Solo autorizar el subdominio propio del deployment en Vercel (nunca *.vercel.app global)
  for (const vercelEnv of [process.env.VERCEL_URL, process.env.VERCEL_BRANCH_URL, process.env.VERCEL_PROJECT_PRODUCTION_URL]) {
    if (vercelEnv) {
      const cleanHost = vercelEnv.trim().replace(/^https?:\/\//i, '').split('/')[0].split(':')[0].toLowerCase();
      if (cleanHost) allowed.add(cleanHost);
    }
  }

  if (process.env.NODE_ENV !== 'production') {
    for (const devHost of DEV_ALLOWED_HOSTNAMES) {
      allowed.add(devHost);
    }
  }

  return allowed;
}

function extractTrustedIp(request) {
  const candidates = [
    request.headers.get('x-vercel-forwarded-for'),
    request.headers.get('cf-connecting-ip'),
    request.headers.get('x-real-ip'),
    request.headers.get('x-forwarded-for')
  ];

  for (const raw of candidates) {
    if (!raw) continue;
    const first = raw.split(',')[0].trim();
    if (first && first.length <= 64 && /^[0-9a-fA-F.:]+$/.test(first)) {
      return first;
    }
  }

  return '127.0.0.1';
}

/**
 * Anillo 1 de Seguridad: Proxy de Red Next.js 16 (src/proxy.js)
 * - Defensa en profundidad contra cabeceras internas manipuladas.
 * - Bloqueo de User-Agents automatizados y scrapers (permitiendo Vercel Cron únicamente en /api/v1/health).
 * - Protección CSRF estricta para métodos mutantes sin confiar en *.vercel.app de terceros.
 * - Propagación segura de x-client-ip saneado hacia los Route Handlers.
 */
export function proxy(request) {
  if (request.headers.has('x-middleware-subrequest')) {
    return NextResponse.json({ error: 'Cabecera de solicitud no permitida.' }, { status: 403 });
  }

  const { pathname } = request.nextUrl;

  if (pathname.startsWith('/api/')) {
    const userAgent = (request.headers.get('user-agent') || '').toLowerCase();
    const isVercelCronHealth = pathname === '/api/v1/health' && userAgent.startsWith('vercel-cron/');

    if (!isVercelCronHealth && (!userAgent || BOT_USER_AGENTS.some((bot) => userAgent.includes(bot)))) {
      return NextResponse.json(
        { error: 'Acceso automatizado no autorizado por políticas de seguridad de Importa Plus.' },
        { status: 403 }
      );
    }

    if (MUTATING_METHODS.has(request.method.toUpperCase())) {
      const origin = request.headers.get('origin') || '';
      const referer = request.headers.get('referer') || '';
      const hostHeader = (request.headers.get('host') || '').split(':')[0].toLowerCase();

      const sourceHostname = extractHostname(origin) || extractHostname(referer);
      const allowedHostnames = buildAllowedHostnames(hostHeader);

      const isAllowedOrigin =
        process.env.NODE_ENV !== 'production'
          ? (!sourceHostname || allowedHostnames.has(sourceHostname))
          : Boolean(sourceHostname && allowedHostnames.has(sourceHostname));

      if (!isAllowedOrigin) {
        return NextResponse.json(
          { error: 'Origen de petición no autorizado por políticas anti-CSRF.' },
          { status: 403 }
        );
      }
    }

    const clientIp = extractTrustedIp(request);
    const requestHeaders = new Headers(request.headers);
    requestHeaders.set('x-client-ip', clientIp);

    const response = NextResponse.next({
      request: {
        headers: requestHeaders
      }
    });
    response.headers.set('X-Robots-Tag', 'noindex, nofollow');
    return response;
  }

  return NextResponse.next();
}

export const config = {
  matcher: ['/api/:path*']
};
