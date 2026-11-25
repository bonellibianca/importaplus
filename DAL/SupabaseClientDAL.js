/**
 * Capa DAL (Data Access Layer) - Cliente Web SPA (index.html)
 * Conector Optimizado para Supabase FREE Tier + Espejo Local Admin (.NET 9)
 *
 * Estrategia de protección de cuota FREE:
 * 1. Caché local en sessionStorage/localStorage (TTL 5 minutos) para no repetir lecturas al recargar.
 * 2. Lectura de solo 4 registros activos de public.forwarders con columnas proyectadas.
 * 3. Sincronización automática con window.__IMPORTA_ADMIN_SYNC__ (generado por el .exe en .NET 9)
 *    para funcionamiento instantáneo incluso en modo file:/// o sin internet.
 */
class SupabaseClientDAL {
  static CACHE_KEY = 'IMPORTA_PLUS_FORWARDERS_CACHE_V1';
  static CACHE_TTL_MS = 5 * 60 * 1000; // 5 minutos

  static getDefaultForwarders() {
    return [
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
  }

  static esUrlImagenSegura(rawUrl) {
    if (!rawUrl || typeof rawUrl !== 'string') return false;
    const trimmed = rawUrl.trim();
    if (!trimmed || trimmed.length > 160000) return false;
    if (trimmed.startsWith('https://')) return true;
    if (trimmed.startsWith('/') && !trimmed.startsWith('//')) return true;
    if (/^[a-zA-Z0-9._-]+\.(?:png|jpg|jpeg|svg|webp)$/i.test(trimmed)) return true;
    if (/^data:image\/(jpeg|jpg|png|webp);base64,[A-Za-z0-9+/=]+$/i.test(trimmed)) return true;
    return false;
  }

  static normalizarLista(lista) {
    if (!Array.isArray(lista)) return [];
    const defaults = this.getDefaultForwarders();
    return lista
      .filter((f) => f && f.activo !== false)
      .sort((a, b) => (Number(a.orden) || 1) - (Number(b.orden) || 1))
      .map((f, idx) => {
        const rawImg = String(f.imagen_url || '').trim();
        return {
          id: f.id || `fwd-${idx + 1}`,
          nombre: String(f.nombre || 'Forwarder Internacional').trim().slice(0, 120),
          imagen_url: this.esUrlImagenSegura(rawImg) ? rawImg : defaults[idx % defaults.length].imagen_url,
          whatsapp: String(f.whatsapp || '').trim(),
          email: String(f.email || '').trim(),
          orden: Number(f.orden) || idx + 1,
          activo: true
        };
      });
  }

  static async obtenerForwardersGrilla() {
    // 1. Si el .exe Admin (.NET 9) sincronizó localmente web_sync.js (funciona hasta en file:///)
    if (typeof window !== 'undefined' && window.__IMPORTA_ADMIN_SYNC__ && Array.isArray(window.__IMPORTA_ADMIN_SYNC__.forwarders)) {
      const localSync = this.normalizarLista(window.__IMPORTA_ADMIN_SYNC__.forwarders);
      if (localSync.length > 0 && !(window.IMPORTA_SUPABASE_CONFIG && window.IMPORTA_SUPABASE_CONFIG.url)) {
        return localSync;
      }
    }

    // 2. Verificar caché de cliente (5 minutos) para cuidar lecturas de Supabase Free
    try {
      const rawCache = sessionStorage.getItem(this.CACHE_KEY);
      if (rawCache) {
        const parsed = JSON.parse(rawCache);
        if (parsed && Date.now() < parsed.expiresAt && Array.isArray(parsed.data) && parsed.data.length > 0) {
          return parsed.data;
        }
      }
    } catch {}

    // 3. Si estamos bajo servidor HTTP/Next.js, consultar el endpoint con caché Edge (/api/v1/forwarders)
    if (typeof window !== 'undefined' && window.location.protocol.startsWith('http')) {
      try {
        const res = await fetch('/api/v1/forwarders');
        if (res.ok) {
          const json = await res.json();
          const norm = this.normalizarLista(json.forwarders);
          if (norm.length > 0) {
            try {
              sessionStorage.setItem(this.CACHE_KEY, JSON.stringify({ data: norm, expiresAt: Date.now() + this.CACHE_TTL_MS }));
            } catch {}
            return norm;
          }
        }
      } catch {}
    }

    // 4. Si se configuró acceso público anon directo a Supabase en index.html
    const cfg = typeof window !== 'undefined' ? window.IMPORTA_SUPABASE_CONFIG : null;
    if (cfg && cfg.url && cfg.anonKey && !cfg.url.includes('TU_PROJECT_REF')) {
      try {
        const cleanUrl = cfg.url.replace(/\/+$/, '');
        const res = await fetch(
          `${cleanUrl}/rest/v1/forwarders?select=id,nombre,imagen_url,whatsapp,email,orden,activo&activo=eq.true&order=orden.asc`,
          {
            headers: {
              apikey: cfg.anonKey,
              Authorization: `Bearer ${cfg.anonKey}`,
              Accept: 'application/json'
            }
          }
        );
        if (res.ok) {
          const rows = await res.json();
          const norm = this.normalizarLista(rows);
          if (norm.length > 0) {
            try {
              sessionStorage.setItem(this.CACHE_KEY, JSON.stringify({ data: norm, expiresAt: Date.now() + this.CACHE_TTL_MS }));
            } catch {}
            return norm;
          }
        }
      } catch {}
    }

    // 5. Fallback sincronizado o semilla oficial de 4 cuadrados
    if (typeof window !== 'undefined' && window.__IMPORTA_ADMIN_SYNC__ && Array.isArray(window.__IMPORTA_ADMIN_SYNC__.forwarders)) {
      const syncNorm = this.normalizarLista(window.__IMPORTA_ADMIN_SYNC__.forwarders);
      if (syncNorm.length > 0) return syncNorm;
    }

    return this.getDefaultForwarders();
  }
}

if (typeof window !== 'undefined') {
  window.SupabaseClientDAL = SupabaseClientDAL;
}
