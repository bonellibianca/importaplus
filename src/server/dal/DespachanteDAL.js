import 'server-only';
import fs from 'fs';
import path from 'path';
import { supabaseSelect, isSupabaseConfigured } from './supabaseClient';
import { esUrlImagenSegura } from './ForwarderDAL';

export const FALLBACK_DESPACHANTES_4 = [
  {
    id: 'e1111111-1111-1111-1111-111111111111',
    nombre: 'Bonelli & Asociados Despachantes DGA',
    imagen_url: 'https://images.unsplash.com/photo-1554224155-6726b3ff858f?auto=format&fit=crop&w=600&q=80',
    whatsapp: '+5491143218800',
    email: 'despachos@importaplus.com.ar',
    orden: 1,
    activo: true
  },
  {
    id: 'e2222222-2222-2222-2222-222222222222',
    nombre: 'Ezeiza Express Despachos Aduaneros',
    imagen_url: 'https://images.unsplash.com/photo-1454165804606-c3d57bc86b40?auto=format&fit=crop&w=600&q=80',
    whatsapp: '+5491144802233',
    email: 'ezeiza@importaplus.com.ar',
    orden: 2,
    activo: true
  },
  {
    id: 'e3333333-3333-3333-3333-333333333333',
    nombre: 'Estudio Aduanero Río de la Plata',
    imagen_url: 'https://images.unsplash.com/photo-1507679799987-c73779587ccf?auto=format&fit=crop&w=600&q=80',
    whatsapp: '+5491150903344',
    email: 'aduanas@importaplus.com.ar',
    orden: 3,
    activo: true
  },
  {
    id: 'e4444444-4444-4444-4444-444444444444',
    nombre: 'Andes & Pacífico Despachantes Matriculados',
    imagen_url: 'https://images.unsplash.com/photo-1521791136064-7986c2920216?auto=format&fit=crop&w=600&q=80',
    whatsapp: '+5492614205544',
    email: 'cuyo@importaplus.com.ar',
    orden: 4,
    activo: true
  }
];

function normalizarDespachante(d, idx) {
  const rawImg = String(d.imagen_url || '').trim();
  const safeImg = esUrlImagenSegura(rawImg)
    ? rawImg
    : FALLBACK_DESPACHANTES_4[idx % FALLBACK_DESPACHANTES_4.length].imagen_url;

  return {
    id: String(d.id || FALLBACK_DESPACHANTES_4[idx % FALLBACK_DESPACHANTES_4.length].id).trim(),
    nombre: String(d.nombre || d.razon_social || 'Despachante de Aduana').trim().slice(0, 120),
    imagen_url: safeImg,
    whatsapp: String(d.whatsapp || d.telefono || '').trim().slice(0, 40),
    email: String(d.email || d.email_notificacion || '').trim().slice(0, 150),
    orden: Number(d.orden) || idx + 1,
    activo: d.activo !== false
  };
}

/**
 * Capa DAL: Despachantes de Aduana en tiempo real desde Supabase Cloud
 * (Soporta tabla dedicada public.despachantes + espejo en parametros_sistema + respaldo local).
 */
export class DespachanteDAL {
  static async obtenerConOrigen() {
    if (isSupabaseConfigured()) {
      const rows = await supabaseSelect(
        'despachantes',
        'select=id,nombre,imagen_url,whatsapp,email,orden,activo&activo=eq.true&order=orden.asc'
      );

      if (Array.isArray(rows)) {
        return {
          despachantes: rows.filter((d) => d && d.activo !== false).map(normalizarDespachante),
          source: 'supabase_live'
        };
      }

      // Respaldo automático en Supabase parametros_sistema si la tabla SQL aún no fue creada en el editor SQL
      const paramRows = await supabaseSelect(
        'parametros_sistema',
        'select=clave,descripcion&clave=eq.CATALOGO_DESPACHANTES_JSON&limit=1'
      );
      if (Array.isArray(paramRows) && paramRows.length > 0 && paramRows[0].descripcion) {
        try {
          const parsed = JSON.parse(paramRows[0].descripcion);
          if (Array.isArray(parsed)) {
            const activos = parsed
              .filter((d) => d && d.activo !== false)
              .sort((a, b) => (Number(a.orden) || 1) - (Number(b.orden) || 1))
              .map(normalizarDespachante);
            return { despachantes: activos, source: 'supabase_param_sync' };
          }
        } catch {}
      }
    }

    const syncPath = path.join(process.cwd(), 'DAL/database/web_sync.json');
    if (fs.existsSync(syncPath)) {
      try {
        const raw = JSON.parse(fs.readFileSync(syncPath, 'utf8').replace(/^\uFEFF/, ''));
        if (Array.isArray(raw.despachantes)) {
          const activos = raw.despachantes
            .filter((d) => d && d.activo !== false)
            .sort((a, b) => (Number(a.orden) || 1) - (Number(b.orden) || 1))
            .map(normalizarDespachante);
          return { despachantes: activos, source: 'local_admin_sync' };
        }
      } catch (err) {
        console.warn('[DespachanteDAL_SYNC_WARN]:', err.message);
      }
    }

    return {
      despachantes: FALLBACK_DESPACHANTES_4.map(normalizarDespachante),
      source: 'seed_fallback'
    };
  }

  static async obtenerGrilla() {
    const { despachantes } = await this.obtenerConOrigen();
    return despachantes;
  }
}
