import 'server-only';

/**
 * Capa BE (Business Entities) - Servidor
 * Entidad de Dominio: PuertoBE
 * Modela un punto de ingreso aduanero nacional (puerto marítimo, fluvial, aéreo o paso fronterizo).
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
    paises_frecuentes = []
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
  }
}
