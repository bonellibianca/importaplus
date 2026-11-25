import 'server-only';
import { getCachedOrFetch, supabaseSelect, isSupabaseConfigured } from './supabaseClient';

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

function normalizarForwarder(f, idx) {
  return {
    id: f.id || `fwd-${idx + 1}`,
    nombre: String(f.nombre || f.razon_social || 'Forwarder Internacional').trim().slice(0, 120),
    imagen_url: String(f.imagen_url || FALLBACK_FORWARDERS_4[idx % 4].imagen_url).trim(),
    whatsapp: String(f.whatsapp || f.telefono || '').trim(),
    email: String(f.email || f.email_notificacion || '').trim(),
    orden: Number(f.orden) || idx + 1,
    activo: f.activo !== false
  };
}

/**
 * Capa DAL: Forwarders gestionados desde el .exe en .NET 9 sobre Supabase.
 * Sin límite de cantidad: la Web los renderiza en grilla de 4 por fila (4, abajo 4, etc.).
 */
export class ForwarderDAL {
  static async obtenerGrillaCuatro() {
    if (isSupabaseConfigured()) {
      const rows = await getCachedOrFetch('forwarders:grid_all', 5 * 60 * 1000, async () => {
        return await supabaseSelect(
          'forwarders',
          'select=id,nombre,imagen_url,whatsapp,email,orden,activo&activo=eq.true&order=orden.asc'
        );
      });

      if (Array.isArray(rows) && rows.length > 0) {
        return rows.map(normalizarForwarder);
      }
    }

    return FALLBACK_FORWARDERS_4.map(normalizarForwarder);
  }
}
