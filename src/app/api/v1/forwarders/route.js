import { NextResponse } from 'next/server';
import fs from 'fs';
import path from 'path';
import { ForwarderDAL } from '@/server/dal/ForwarderDAL';
import { isSupabaseConfigured } from '@/server/dal/supabaseClient';

export const runtime = 'nodejs';
export const dynamic = 'force-dynamic';

export async function GET() {
  try {
    // 1. Si Supabase está configurado, leer con caché L1 (5 min) cuidando la cuota FREE
    if (isSupabaseConfigured()) {
      const forwarders = await ForwarderDAL.obtenerGrillaCuatro();
      return NextResponse.json(
        { forwarders, source: 'supabase_cached' },
        {
          status: 200,
          headers: { 'Cache-Control': 'public, s-maxage=300, stale-while-revalidate=3600' }
        }
      );
    }

    // 2. Espejo sincronizado localmente por el .exe Admin (.NET 9) en desarrollo local
    const syncPath = path.join(process.cwd(), 'DAL/database/web_sync.json');
    if (fs.existsSync(syncPath)) {
      const raw = JSON.parse(fs.readFileSync(syncPath, 'utf8'));
      if (Array.isArray(raw.forwarders) && raw.forwarders.length > 0) {
        const activos = raw.forwarders
          .filter((f) => f.activo !== false)
          .sort((a, b) => (Number(a.orden) || 1) - (Number(b.orden) || 1))
          .map((f, i) => ({
            id: f.id || `fwd-${i + 1}`,
            nombre: String(f.nombre || '').slice(0, 120),
            imagen_url: f.imagen_url || '',
            whatsapp: f.whatsapp || '',
            email: f.email || '',
            orden: Number(f.orden) || i + 1
          }));
        return NextResponse.json({ forwarders: activos, source: 'local_admin_sync' });
      }
    }

    // 3. Semilla por defecto de 4 Forwarders
    const forwarders = await ForwarderDAL.obtenerGrillaCuatro();
    return NextResponse.json(
      { forwarders, source: 'seed_fallback' },
      {
        status: 200,
        headers: { 'Cache-Control': 'public, s-maxage=300, stale-while-revalidate=3600' }
      }
    );
  } catch (error) {
    console.error('[API_FORWARDERS_ERROR]:', error.message);
    return NextResponse.json({ error: 'Error al obtener forwarders' }, { status: 500 });
  }
}
