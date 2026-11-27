import 'server-only';
import fs from 'fs';
import path from 'path';
import { supabaseSelect, isSupabaseConfigured } from './supabaseClient';

export const FALLBACK_FORWARDERS_4 = [
  {
    id: 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa',
    nombre: 'Bonelli & Asociados Global Logistics',
    imagen_url: 'https://images.unsplash.com/photo-1586528116311-ad8dd3c8310d?auto=format&fit=crop&w=600&q=80',
    whatsapp: '+5491143218800',
    email: 'contacto@importaplus.com.ar',
    orden: 1,
    activo: true
  },
  {
    id: 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb',
    nombre: 'NEXA Ocean & Air Forwarding',
    imagen_url: 'https://images.unsplash.com/photo-1494412574643-ff11b0a5c1c3?auto=format&fit=crop&w=600&q=80',
    whatsapp: '+5491152391100',
    email: 'operaciones@importaplus.com.ar',
    orden: 2,
    activo: true
  },
  {
    id: 'cccccccc-cccc-cccc-cccc-cccccccccccc',
    nombre: 'Ezeiza Express Cargo & Customs',
    imagen_url: 'https://images.unsplash.com/photo-1570710891163-6d3b5c47248b?auto=format&fit=crop&w=600&q=80',
    whatsapp: '+5491144802233',
    email: 'ezeiza@importaplus.com.ar',
    orden: 3,
    activo: true
  },
  {
    id: 'dddddddd-dddd-dddd-dddd-dddddddddddd',
    nombre: 'Andina Multimodal Internacional',
    imagen_url: 'https://images.unsplash.com/photo-1601584115197-04ecc0da31d7?auto=format&fit=crop&w=600&q=80',
    whatsapp: '+5492614205544',
    email: 'mendoza@importaplus.com.ar',
    orden: 4,
    activo: true
  }
];

export function esUrlImagenSegura(rawUrl) {
  if (!rawUrl || typeof rawUrl !== 'string') return false;
  const trimmed = rawUrl.trim();
  if (!trimmed || trimmed.length > 160000) return false;
  if (trimmed.startsWith('/') && !trimmed.startsWith('//')) return true;
  if (/^data:image\/(jpeg|jpg|png|webp);base64,[A-Za-z0-9+/=]+$/i.test(trimmed)) return true;
  try {
    const parsed = new URL(trimmed);
    return parsed.protocol === 'https:';
  } catch {
    return false;
  }
}

function normalizarForwarder(f, idx) {
  const rawImg = String(f.imagen_url || '').trim();
  const safeImg = esUrlImagenSegura(rawImg)
    ? rawImg
    : FALLBACK_FORWARDERS_4[idx % FALLBACK_FORWARDERS_4.length].imagen_url;

  return {
    id: String(f.id || FALLBACK_FORWARDERS_4[idx % FALLBACK_FORWARDERS_4.length].id).trim(),
    nombre: String(f.nombre || f.razon_social || 'Forwarder Internacional').trim().slice(0, 120),
    imagen_url: safeImg,
    whatsapp: String(f.whatsapp || f.telefono || '').trim().slice(0, 40),
    email: String(f.email || f.email_notificacion || '').trim().slice(0, 150),
    orden: Number(f.orden) || idx + 1,
    activo: f.activo !== false
  };
}

/**
 * Capa DAL: Forwarders en tiempo real desde Supabase Cloud (sin caché obsoleto ni resurrección de eliminados).
 */
export class ForwarderDAL {
  static async obtenerConOrigen() {
    if (isSupabaseConfigured()) {
      const rows = await supabaseSelect(
        'forwarders',
        'select=id,nombre,imagen_url,whatsapp,email,orden,activo&activo=eq.true&order=orden.asc'
      );

      // Si Supabase respondió con un array (incluso vacío si se eliminaron todos), respetar exactamente la nube
      if (Array.isArray(rows)) {
        return {
          forwarders: rows.filter((f) => f && f.activo !== false).map(normalizarForwarder),
          source: 'supabase_live'
        };
      }
    }

    const syncPath = path.join(process.cwd(), 'DAL/database/web_sync.json');
    if (fs.existsSync(syncPath)) {
      try {
        const raw = JSON.parse(fs.readFileSync(syncPath, 'utf8').replace(/^\uFEFF/, ''));
        if (Array.isArray(raw.forwarders)) {
          const activos = raw.forwarders
            .filter((f) => f && f.activo !== false)
            .sort((a, b) => (Number(a.orden) || 1) - (Number(b.orden) || 1))
            .map(normalizarForwarder);
          return { forwarders: activos, source: 'local_admin_sync' };
        }
      } catch (err) {
        console.warn('[ForwarderDAL_SYNC_WARN]:', err.message);
      }
    }

    return {
      forwarders: FALLBACK_FORWARDERS_4.map(normalizarForwarder),
      source: 'seed_fallback'
    };
  }

  static async obtenerGrillaCuatro() {
    const { forwarders } = await this.obtenerConOrigen();
    return forwarders;
  }
}
