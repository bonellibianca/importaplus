/**
 * Capa BE (Business Entities)
 * Entidad de Negocio: PaisBE
 * 
 * Contiene EXCLUSIVAMENTE los atributos del país de origen/procedencia.
 */
class PaisBE {
  constructor({
    codigo = '',
    nombre = '',
    bloque = 'EXTRAZONA',
    esIntrazona = false,
    fleteParametricoStd = 2000,
    seguroPctStd = 1.0
  } = {}) {
    this.codigo = codigo;
    this.nombre = nombre;
    this.bloque = bloque;
    this.esIntrazona = Boolean(esIntrazona);
    this.fleteParametricoStd = parseFloat(fleteParametricoStd) || 0;
    this.seguroPctStd = parseFloat(seguroPctStd) || 1.0;
  }
}

if (typeof window !== 'undefined') {
  window.PaisBE = PaisBE;
}
