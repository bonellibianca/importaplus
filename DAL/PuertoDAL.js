/**
 * Capa DAL (Data Access Layer)
 * Repositorio / Acceso a Datos: PuertoDAL
 *
 * Consulta en tiempo real los puertos y aduanas (con sus links a Google Maps)
 * desde Next.js API / Supabase Cloud -> respaldo sincronizado web_sync.js -> semilla oficial.
 */
class PuertoDAL {
  constructor() {
    this._puertosSemilla = [
      {
        id: '11111111-1111-1111-1111-111111111111',
        name: 'Puerto de Buenos Aires',
        via_ingreso: 'MARITIMA_FLUVIAL',
        type: 'Marítimo principal',
        lat: -34.5985,
        lng: -58.3731,
        desc: 'Principal puerto de importación del país. Maneja la mayor parte del comercio exterior argentino.',
        operador: 'AGPSE (Administración General de Puertos)',
        google_maps_url: 'https://www.google.com/maps/place/Encarnaci%C3%B3n+Ezcurra+320,+C1107+Cdad.+Aut%C3%B3noma+de+Buenos+Aires/@-34.5908026,-58.3704644,16.79z/data=!4m6!3m5!1s0x95a334d72473ee5b:0x11b42bbb998547cc!8m2!3d-34.6160181!4d-58.362668!16s%2Fg%2F11r_wsrldj?entry=ttu&g_ep=EgoyMDI2MTEyMy4wIKXMDSoASAFQAw%3D%3D'
      },
      {
        id: '22222222-2222-2222-2222-222222222222',
        name: 'Aduana de Ezeiza',
        via_ingreso: 'AEREA',
        type: 'Aéreo',
        lat: -34.8222,
        lng: -58.5358,
        desc: 'Aduana del Aeropuerto Internacional Ministro Pistarini. Principal ingreso aéreo de mercadería.',
        operador: 'ARCA-Aduana / AA2000',
        google_maps_url: 'https://www.google.com/maps?rlz=1C1CHBF_esAR1044AR1044&fvr=1&pvq=CgkvbS8wMWt5M2Q&vet=10CAAQoqAOahcKEwjA8MSXjKmYAxUAAAAAHQAAAAAQDQ..i&cs=0&um=1&ie=UTF-8&fb=1&gl=ar&sa=X&ftid=0x95bcd0f4e2dea557:0xf19b6f81d441cc3b'
      },
      {
        id: '33333333-3333-3333-3333-333333333333',
        name: 'Puerto de Rosario',
        via_ingreso: 'MARITIMA_FLUVIAL',
        type: 'Fluvial',
        lat: -32.9468,
        lng: -60.6393,
        desc: 'Importante terminal fluvial sobre el Río Paraná. Relevante para granos y productos industriales.',
        operador: 'Ente Administrador Puerto Rosario',
        google_maps_url: 'https://www.google.com/maps/place/Puerto+Rosario/data=!4m2!3m1!1s0x0:0x7e1e8c154b58d906?sa=X&ved=1t:2428&ictx=111'
      },
      {
        id: '55555555-5555-5555-5555-555555555555',
        name: 'Puerto de Mar del Plata',
        via_ingreso: 'MARITIMA_FLUVIAL',
        type: 'Marítimo',
        lat: -38.0055,
        lng: -57.5426,
        desc: 'Puerto atlántico al sur de la provincia de Buenos Aires. Pesca e importaciones regionales.',
        operador: 'Consorcio de Gestión del Puerto',
        google_maps_url: 'https://www.google.com/maps/place/Puerto+de+Mar+del+Plata/@-38.050254,-57.5463923,16z/data=!4m10!1m2!2m1!1sPuerto+de+Mar+del+Plata!3m6!1s0x9584ddf29129483b:0x4d4bf245aff78945!8m2!3d-38.050254!4d-57.5381526!15sChdQdWVydG8gZGUgTWFyIGRlbCBQbGF0YVoZIhdwdWVydG8gZGUgbWFyIGRlbCBwbGF0YZIBDGZpc2hpbmdfcGllcpoBJENoZERTVWhOTUc5blMwVkpRMEZuU1VSMGEweHFlbTlCUlJBQuABAPoBBAgAED4!16s%2Fg%2F120mbqxd?entry=ttu&g_ep=EgoyMDI2MTEyMy4wIKXMDSoASAFQAw%3D%3D'
      },
      {
        id: '44444444-4444-4444-4444-444444444444',
        name: 'Paso Cristo Redentor',
        via_ingreso: 'TERRESTRE',
        type: 'Terrestre',
        lat: -32.8353,
        lng: -70.0649,
        desc: 'Principal paso fronterizo con Chile en la Cordillera de los Andes. Alto volumen de camiones.',
        operador: 'Aduana Argentina / DNV',
        google_maps_url: 'https://www.google.com/maps/place/Paso+Cristo+Redentor/data=!4m2!3m1!1s0x9687f4f987c60a21:0x85b25d9333d7e10b?sa=X&ved=1t:155783&ictx=111'
      }
    ];

    this._ciudades = [
      { name: 'Buenos Aires', lat: -34.6037, lng: -58.3816 },
      { name: 'Córdoba', lat: -31.4135, lng: -64.1811 },
      { name: 'Rosario', lat: -32.9468, lng: -60.6393 },
      { name: 'Mendoza', lat: -32.8908, lng: -68.8272 },
      { name: 'Tucumán', lat: -26.8083, lng: -65.2176 },
      { name: 'La Plata', lat: -34.9215, lng: -57.9545 },
      { name: 'Mar del Plata', lat: -38.0023, lng: -57.5575 },
      { name: 'Salta', lat: -24.7821, lng: -65.4232 },
      { name: 'Santa Fe', lat: -31.6333, lng: -60.7 },
      { name: 'San Juan', lat: -31.5375, lng: -68.5364 }
    ];
  }

  _mapearListaPuertos(rawList) {
    const PuertoClass = typeof window !== 'undefined' ? window.PuertoBE : null;
    const semillaPorId = new Map(this._puertosSemilla.map((s) => [String(s.id).toLowerCase(), s]));
    const semillaPorNombre = new Map(this._puertosSemilla.map((s) => [String(s.name).toLowerCase(), s]));

    return rawList.map((item) => {
      const idKey = String(item.id || '').toLowerCase();
      const nomKey = String(item.nombre || item.name || '').trim().toLowerCase();
      const base = semillaPorId.get(idKey) || semillaPorNombre.get(nomKey) || {};
      const combinado = {
        ...base,
        ...item,
        nombre: item.nombre || item.name || base.name || '',
        tipo: item.type || item.tipo || undefined,
        descripcion: item.infraestructura || item.desc || item.descripcion || base.desc || '',
        operador: item.operador || base.operador || '',
        lat: item.latitud !== undefined ? item.latitud : (item.lat !== undefined ? item.lat : base.lat),
        lng: item.longitud !== undefined ? item.longitud : (item.lng !== undefined ? item.lng : base.lng),
        google_maps_url: item.google_maps_url || item.googleMapsUrl || base.google_maps_url || ''
      };
      return PuertoClass ? new PuertoClass(combinado) : combinado;
    });
  }

  async obtenerTodos() {
    // 1. Endpoint API en vivo si corre sobre HTTP / Vercel
    if (typeof window !== 'undefined' && window.location && window.location.protocol.startsWith('http')) {
      try {
        const res = await fetch('/api/v1/puertos', { cache: 'no-store' });
        if (res.ok) {
          const json = await res.json();
          if (Array.isArray(json.puertos)) {
            return this._mapearListaPuertos(json.puertos);
          }
        }
      } catch {}
    }

    // 2. Consulta directa a Supabase Cloud (funciona incluso desde index.html local)
    if (typeof window !== 'undefined' && window.SupabaseClientDAL) {
      const cfg = window.SupabaseClientDAL.obtenerConfigSupabasePublica();
      if (cfg.url && cfg.anonKey) {
        try {
          const headers = window.SupabaseClientDAL.construirHeadersSupabase(cfg.anonKey);
          let res = await fetch(
            `${cfg.url}/rest/v1/puertos_aduanas?select=id,nombre,via_ingreso,provincia,codigo_sim,operador,infraestructura,latitud,longitud,google_maps_url&order=nombre.asc`,
            { headers, cache: 'no-store' }
          );
          if (!res.ok) {
            res = await fetch(
              `${cfg.url}/rest/v1/puertos_aduanas?select=id,nombre,via_ingreso,provincia,codigo_sim,operador,infraestructura,latitud,longitud&order=nombre.asc`,
              { headers, cache: 'no-store' }
            );
          }
          if (res.ok) {
            const rows = await res.json();
            if (Array.isArray(rows)) {
              // Enriquecer con CATALOGO_PUERTOS_JSON si existe en parametros_sistema
              try {
                const resParam = await fetch(
                  `${cfg.url}/rest/v1/parametros_sistema?select=descripcion&clave=eq.CATALOGO_PUERTOS_JSON&limit=1`,
                  { headers, cache: 'no-store' }
                );
                if (resParam.ok) {
                  const pRows = await resParam.json();
                  if (Array.isArray(pRows) && pRows.length > 0 && pRows[0]?.descripcion) {
                    const extras = JSON.parse(pRows[0].descripcion);
                    if (Array.isArray(extras)) {
                      const extraMap = new Map(extras.map((x) => [String(x.id || '').toLowerCase(), x]));
                      rows.forEach((r) => {
                        const ex = extraMap.get(String(r.id || '').toLowerCase());
                        if (ex && !r.google_maps_url) {
                          r.google_maps_url = ex.google_maps_url || ex.GoogleMapsUrl || '';
                        }
                      });
                    }
                  }
                }
              } catch {}
              return this._mapearListaPuertos(rows);
            }
          }
        } catch {}
      }
    }

    // 3. Respaldo sincronizado por AdminImportaPlus (web_sync.js)
    if (typeof window !== 'undefined' && window.__IMPORTA_ADMIN_SYNC__ && Array.isArray(window.__IMPORTA_ADMIN_SYNC__.puertos)) {
      return this._mapearListaPuertos(window.__IMPORTA_ADMIN_SYNC__.puertos);
    }

    return this._mapearListaPuertos(this._puertosSemilla);
  }

  async obtenerPorTipo(tipo) {
    const todos = await this.obtenerTodos();
    if (!tipo) return todos;
    return todos.filter((p) => String(p.tipo || '').toLowerCase().includes(tipo.toLowerCase()));
  }

  async obtenerCiudadesReferencia() {
    return [...this._ciudades];
  }
}

if (typeof window !== 'undefined') {
  window.PuertoDAL = PuertoDAL;
}
