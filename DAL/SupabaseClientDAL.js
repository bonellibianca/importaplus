/**
 * Capa DAL (Data Access Layer) - Cliente Web SPA (index.html)
 * Conector Directo en Tiempo Real a Supabase Cloud + Respaldo Local Offline (.NET 9)
 *
 * Corrección clave:
 * 1. Siempre prioriza la API / Supabase Cloud en vivo para que cualquier cambio (baja/alta/modificación)
 *    realizado en AdminImportaPlus se refleje instantáneamente en cualquier PC o clon de GitHub.
 * 2. Respeta listas vacías [] si el administrador eliminó todos los registros activos, sin resucitar datos semilla.
 * 3. Usa window.__IMPORTA_ADMIN_SYNC__ únicamente como respaldo offline cuando no hay conexión a internet.
 */
class SupabaseClientDAL {
  static DEFAULT_SUPABASE_URL = 'https://exqyupuliqxsxejgouho.supabase.co';
  static DEFAULT_SUPABASE_PUBLISHABLE_KEY = 'sb_publishable_HPs_CdVHnpT1PqyRUwfz9Q_g5rBulj_';

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

  static getDefaultDespachantes() {
    return [
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

  static normalizarLista(lista, fallbackDefaults, etiquetaDefecto = 'Operador Oficial') {
    if (!Array.isArray(lista)) return [];
    const defaults = fallbackDefaults || this.getDefaultForwarders();
    return lista
      .filter((f) => f && f.activo !== false)
      .sort((a, b) => (Number(a.orden) || 1) - (Number(b.orden) || 1))
      .map((f, idx) => {
        const rawImg = String(f.imagen_url || '').trim();
        return {
          id: f.id || `op-${idx + 1}`,
          nombre: String(f.nombre || etiquetaDefecto).trim().slice(0, 120),
          imagen_url: this.esUrlImagenSegura(rawImg) ? rawImg : defaults[idx % defaults.length].imagen_url,
          whatsapp: String(f.whatsapp || '').trim(),
          email: String(f.email || '').trim(),
          orden: Number(f.orden) || idx + 1,
          activo: true
        };
      });
  }

  static obtenerConfigSupabasePublica() {
    const winCfg = (typeof window !== 'undefined' && window.IMPORTA_SUPABASE_CONFIG) ? window.IMPORTA_SUPABASE_CONFIG : null;
    const url = (winCfg && winCfg.url && !winCfg.url.includes('TU_PROJECT_REF'))
      ? winCfg.url.replace(/\/+$/, '')
      : this.DEFAULT_SUPABASE_URL;
    const anonKey = (winCfg && winCfg.anonKey && winCfg.anonKey !== 'tu-anon-key')
      ? winCfg.anonKey
      : this.DEFAULT_SUPABASE_PUBLISHABLE_KEY;
    return { url, anonKey };
  }

  static construirHeadersSupabase(anonKey) {
    const headers = {
      apikey: anonKey,
      Accept: 'application/json'
    };
    if (anonKey && anonKey.startsWith('eyJ')) {
      headers.Authorization = `Bearer ${anonKey}`;
    }
    return headers;
  }

  static async obtenerForwardersGrilla() {
    // 1. Si estamos bajo servidor HTTP/Next.js, consultar endpoint en vivo
    if (typeof window !== 'undefined' && window.location.protocol.startsWith('http')) {
      try {
        const res = await fetch('/api/v1/forwarders', { cache: 'no-store' });
        if (res.ok) {
          const json = await res.json();
          if (Array.isArray(json.forwarders)) {
            return this.normalizarLista(json.forwarders, this.getDefaultForwarders(), 'Forwarder Internacional');
          }
        }
      } catch {}
    }

    // 2. Consulta directa a Supabase Cloud (funciona incluso abriendo index.html con file:///)
    const cfg = this.obtenerConfigSupabasePublica();
    if (cfg.url && cfg.anonKey) {
      try {
        const res = await fetch(
          `${cfg.url}/rest/v1/forwarders?select=id,nombre,imagen_url,whatsapp,email,orden,activo&activo=eq.true&order=orden.asc`,
          {
            headers: this.construirHeadersSupabase(cfg.anonKey),
            cache: 'no-store'
          }
        );
        if (res.ok) {
          const rows = await res.json();
          if (Array.isArray(rows)) {
            return this.normalizarLista(rows, this.getDefaultForwarders(), 'Forwarder Internacional');
          }
        }
      } catch {}
    }

    // 3. Respaldo offline sincronizado por AdminImportaPlus.exe o semilla oficial
    if (typeof window !== 'undefined' && window.__IMPORTA_ADMIN_SYNC__ && Array.isArray(window.__IMPORTA_ADMIN_SYNC__.forwarders)) {
      return this.normalizarLista(window.__IMPORTA_ADMIN_SYNC__.forwarders, this.getDefaultForwarders(), 'Forwarder Internacional');
    }

    return this.getDefaultForwarders();
  }

  static async obtenerDespachantesGrilla() {
    // 1. Si estamos bajo servidor HTTP/Next.js, consultar endpoint en vivo
    if (typeof window !== 'undefined' && window.location.protocol.startsWith('http')) {
      try {
        const res = await fetch('/api/v1/despachantes', { cache: 'no-store' });
        if (res.ok) {
          const json = await res.json();
          if (Array.isArray(json.despachantes)) {
            return this.normalizarLista(json.despachantes, this.getDefaultDespachantes(), 'Despachante de Aduana');
          }
        }
      } catch {}
    }

    // 2. Consulta directa a Supabase Cloud (tabla public.despachantes o espejo en parametros_sistema)
    const cfg = this.obtenerConfigSupabasePublica();
    if (cfg.url && cfg.anonKey) {
      try {
        const res = await fetch(
          `${cfg.url}/rest/v1/despachantes?select=id,nombre,imagen_url,whatsapp,email,orden,activo&activo=eq.true&order=orden.asc`,
          {
            headers: this.construirHeadersSupabase(cfg.anonKey),
            cache: 'no-store'
          }
        );
        if (res.ok) {
          const rows = await res.json();
          if (Array.isArray(rows)) {
            return this.normalizarLista(rows, this.getDefaultDespachantes(), 'Despachante de Aduana');
          }
        }

        const resParam = await fetch(
          `${cfg.url}/rest/v1/parametros_sistema?select=clave,descripcion&clave=eq.CATALOGO_DESPACHANTES_JSON&limit=1`,
          {
            headers: this.construirHeadersSupabase(cfg.anonKey),
            cache: 'no-store'
          }
        );
        if (resParam.ok) {
          const paramRows = await resParam.json();
          if (Array.isArray(paramRows) && paramRows.length > 0 && paramRows[0].descripcion) {
            const parsed = JSON.parse(paramRows[0].descripcion);
            if (Array.isArray(parsed)) {
              return this.normalizarLista(parsed, this.getDefaultDespachantes(), 'Despachante de Aduana');
            }
          }
        }
      } catch {}
    }

    // 3. Respaldo offline sincronizado por AdminImportaPlus.exe o semilla oficial
    if (typeof window !== 'undefined' && window.__IMPORTA_ADMIN_SYNC__ && Array.isArray(window.__IMPORTA_ADMIN_SYNC__.despachantes)) {
      return this.normalizarLista(window.__IMPORTA_ADMIN_SYNC__.despachantes, this.getDefaultDespachantes(), 'Despachante de Aduana');
    }

    return this.getDefaultDespachantes();
  }
}

if (typeof window !== 'undefined') {
  window.SupabaseClientDAL = SupabaseClientDAL;
}
