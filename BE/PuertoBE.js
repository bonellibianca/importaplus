/**
 * Capa BE (Business Entities)
 * Entidad de Negocio: PuertoBE
 *
 * Modela un punto de ingreso aduanero nacional (puerto marítimo, fluvial, aéreo o paso fronterizo)
 * incluyendo su enlace oficial a Google Maps.
 */
class PuertoBE {
  constructor(data = {}) {
    this.id = data.id || '';
    this.nombre = data.name || data.nombre || '';
    this.via_ingreso = data.via_ingreso || 'MARITIMA_FLUVIAL';
    this.tipo = data.type || data.tipo || PuertoBE.formatearTipoVia(this.via_ingreso, this.nombre);
    this.provincia = data.provincia || '';
    this.codigo_sim = data.codigo_sim || '';
    this.lat = data.lat !== undefined ? Number(data.lat) : (Number(data.latitud) || 0);
    this.lng = data.lng !== undefined ? Number(data.lng) : (Number(data.longitud) || 0);
    this.latitud = this.lat;
    this.longitud = this.lng;
    this.descripcion = data.desc || data.descripcion || data.infraestructura || '';
    this.infraestructura = data.infraestructura || this.descripcion;
    this.operador = data.operador || '';
    this.google_maps_url = PuertoBE.normalizarUrlMaps(
      data.google_maps_url || data.googleMapsUrl || '',
      this.nombre,
      this.lat,
      this.lng
    );
  }

  static formatearTipoVia(viaIngreso, nombre = '') {
    const v = String(viaIngreso || '').toUpperCase();
    const n = String(nombre || '').toLowerCase();
    if (v === 'AEREA') return 'Aéreo';
    if (v === 'TERRESTRE') return 'Terrestre';
    if (n.includes('rosario') || n.includes('paran') || n.includes('fluvial')) return 'Fluvial';
    if (n.includes('buenos aires')) return 'Marítimo principal';
    return 'Marítimo';
  }

  static obtenerMapaLinksOficiales() {
    return {
      'puerto de buenos aires': 'https://www.google.com/maps/place/Encarnaci%C3%B3n+Ezcurra+320,+C1107+Cdad.+Aut%C3%B3noma+de+Buenos+Aires/@-34.5908026,-58.3704644,16.79z/data=!4m6!3m5!1s0x95a334d72473ee5b:0x11b42bbb998547cc!8m2!3d-34.6160181!4d-58.362668!16s%2Fg%2F11r_wsrldj?entry=ttu&g_ep=EgoyMDI2MTEyMy4wIKXMDSoASAFQAw%3D%3D',
      'aduana de ezeiza': 'https://www.google.com/maps?rlz=1C1CHBF_esAR1044AR1044&fvr=1&pvq=CgkvbS8wMWt5M2Q&vet=10CAAQoqAOahcKEwjA8MSXjKmYAxUAAAAAHQAAAAAQDQ..i&cs=0&um=1&ie=UTF-8&fb=1&gl=ar&sa=X&ftid=0x95bcd0f4e2dea557:0xf19b6f81d441cc3b',
      'puerto de rosario': 'https://www.google.com/maps/place/Puerto+Rosario/data=!4m2!3m1!1s0x0:0x7e1e8c154b58d906?sa=X&ved=1t:2428&ictx=111',
      'puerto de mar del plata': 'https://www.google.com/maps/place/Puerto+de+Mar+del+Plata/@-38.050254,-57.5463923,16z/data=!4m10!1m2!2m1!1sPuerto+de+Mar+del+Plata!3m6!1s0x9584ddf29129483b:0x4d4bf245aff78945!8m2!3d-38.050254!4d-57.5381526!15sChdQdWVydG8gZGUgTWFyIGRlbCBQbGF0YVoZIhdwdWVydG8gZGUgbWFyIGRlbCBwbGF0YZIBDGZpc2hpbmdfcGllcpoBJENoZERTVWhOTUc5blMwVkpRMEZuU1VSMGEweHFlbTlCUlJBQuABAPoBBAgAED4!16s%2Fg%2F120mbqxd?entry=ttu&g_ep=EgoyMDI2MTEyMy4wIKXMDSoASAFQAw%3D%3D',
      'paso cristo redentor': 'https://www.google.com/maps/place/Paso+Cristo+Redentor/data=!4m2!3m1!1s0x9687f4f987c60a21:0x85b25d9333d7e10b?sa=X&ved=1t:155783&ictx=111'
    };
  }

  static normalizarUrlMaps(rawUrl, nombre = '', lat = 0, lng = 0) {
    let url = String(rawUrl || '').trim();
    if (url) {
      if (/^(www\.)?google\./i.test(url) || /^maps\.(google\.|app\.goo\.gl)/i.test(url)) {
        url = 'https://' + (url.toLowerCase().startsWith('google.') ? 'www.' + url : url);
      }
      if (/^https?:\/\//i.test(url)) {
        return url;
      }
    }

    const claveNombre = String(nombre || '').trim().toLowerCase();
    const oficiales = PuertoBE.obtenerMapaLinksOficiales();
    if (oficiales[claveNombre]) {
      return oficiales[claveNombre];
    }

    if (lat && lng) {
      return `https://www.google.com/maps/search/?api=1&query=${encodeURIComponent(`${lat},${lng}`)}`;
    }
    if (nombre) {
      return `https://www.google.com/maps/search/?api=1&query=${encodeURIComponent(nombre)}`;
    }
    return 'https://www.google.com/maps';
  }
}

if (typeof window !== 'undefined') {
  window.PuertoBE = PuertoBE;
}
