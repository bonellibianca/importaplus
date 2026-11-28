import 'server-only';

const LINKS_OFICIALES_POR_NOMBRE = {
  'puerto de buenos aires': 'https://www.google.com/maps/place/Encarnaci%C3%B3n+Ezcurra+320,+C1107+Cdad.+Aut%C3%B3noma+de+Buenos+Aires/@-34.5908026,-58.3704644,16.79z/data=!4m6!3m5!1s0x95a334d72473ee5b:0x11b42bbb998547cc!8m2!3d-34.6160181!4d-58.362668!16s%2Fg%2F11r_wsrldj?entry=ttu&g_ep=EgoyMDI2MTEyMy4wIKXMDSoASAFQAw%3D%3D',
  'aduana de ezeiza': 'https://www.google.com/maps?rlz=1C1CHBF_esAR1044AR1044&fvr=1&pvq=CgkvbS8wMWt5M2Q&vet=10CAAQoqAOahcKEwjA8MSXjKmYAxUAAAAAHQAAAAAQDQ..i&cs=0&um=1&ie=UTF-8&fb=1&gl=ar&sa=X&ftid=0x95bcd0f4e2dea557:0xf19b6f81d441cc3b',
  'puerto de rosario': 'https://www.google.com/maps/place/Puerto+Rosario/data=!4m2!3m1!1s0x0:0x7e1e8c154b58d906?sa=X&ved=1t:2428&ictx=111',
  'puerto de mar del plata': 'https://www.google.com/maps/place/Puerto+de+Mar+del+Plata/@-38.050254,-57.5463923,16z/data=!4m10!1m2!2m1!1sPuerto+de+Mar+del+Plata!3m6!1s0x9584ddf29129483b:0x4d4bf245aff78945!8m2!3d-38.050254!4d-57.5381526!15sChdQdWVydG8gZGUgTWFyIGRlbCBQbGF0YVoZIhdwdWVydG8gZGUgbWFyIGRlbCBwbGF0YZIBDGZpc2hpbmdfcGllcpoBJENoZERTVWhOTUc5blMwVkpRMEZuU1VSMGEweHFlbTlCUlJBQuABAPoBBAgAED4!16s%2Fg%2F120mbqxd?entry=ttu&g_ep=EgoyMDI2MTEyMy4wIKXMDSoASAFQAw%3D%3D',
  'paso cristo redentor': 'https://www.google.com/maps/place/Paso+Cristo+Redentor/data=!4m2!3m1!1s0x9687f4f987c60a21:0x85b25d9333d7e10b?sa=X&ved=1t:155783&ictx=111'
};

export function normalizarGoogleMapsUrl(rawUrl, nombre = '', latitud = 0, longitud = 0) {
  let url = String(rawUrl || '').trim();
  if (url) {
    if (/^(www\.)?google\./i.test(url) || /^maps\.(google\.|app\.goo\.gl)/i.test(url)) {
      url = 'https://' + (url.toLowerCase().startsWith('google.') ? 'www.' + url : url);
    }
    if (/^https?:\/\//i.test(url)) {
      return url;
    }
  }

  const key = String(nombre || '').trim().toLowerCase();
  if (LINKS_OFICIALES_POR_NOMBRE[key]) {
    return LINKS_OFICIALES_POR_NOMBRE[key];
  }

  if (latitud && longitud) {
    return `https://www.google.com/maps/search/?api=1&query=${encodeURIComponent(`${latitud},${longitud}`)}`;
  }
  if (nombre) {
    return `https://www.google.com/maps/search/?api=1&query=${encodeURIComponent(nombre)}`;
  }
  return 'https://www.google.com/maps';
}

/**
 * Capa BE (Business Entities) - Servidor
 * Entidad de Dominio: PuertoBE
 */
export class PuertoBE {
  constructor({
    id = '',
    nombre = '',
    via_ingreso = 'MARITIMA_FLUVIAL',
    provincia = '',
    codigo_sim = '',
    operador = '',
    tipos_carga = [],
    infraestructura = '',
    latitud = 0,
    longitud = 0,
    paises_frecuentes = [],
    google_maps_url = ''
  } = {}) {
    this.id = id;
    this.nombre = nombre;
    this.via_ingreso = via_ingreso;
    this.provincia = provincia;
    this.codigo_sim = codigo_sim;
    this.operador = operador;
    this.tipos_carga = Array.isArray(tipos_carga) ? tipos_carga : [];
    this.infraestructura = infraestructura;
    this.latitud = Number(latitud) || 0;
    this.longitud = Number(longitud) || 0;
    this.paises_frecuentes = Array.isArray(paises_frecuentes) ? paises_frecuentes : [];
    this.google_maps_url = normalizarGoogleMapsUrl(google_maps_url, this.nombre, this.latitud, this.longitud);
  }
}
