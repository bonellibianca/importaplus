import { NextResponse } from 'next/server';
import { supabaseSelect, isSupabaseConfigured } from '@/server/dal/supabaseClient';

export const runtime = 'nodejs';
export const dynamic = 'force-dynamic';

/**
 * Endpoint ligero Keep-Alive para evitar que Supabase FREE entre en pausa tras 7 días sin actividad.
 * Ejecutado automáticamente cada 5 días mediante Vercel Cron (1 lectura mínima de 1 fila).
 */
export async function GET() {
  if (!isSupabaseConfigured()) {
    return NextResponse.json({ ok: true, supabase: 'not_configured_hybrid_mode', ts: new Date().toISOString() });
  }

  const ping = await supabaseSelect('parametros_sistema', 'select=clave&limit=1');
  return NextResponse.json({
    ok: Array.isArray(ping),
    supabase: Array.isArray(ping) ? 'active' : 'degraded',
    ts: new Date().toISOString()
  });
}
