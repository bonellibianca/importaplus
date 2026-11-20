/**
 * Capa BE (Business Entities)
 * Entidad de Negocio: ExcepcionArancelariaBE
 * 
 * Contiene EXCLUSIVAMENTE los atributos de una Excepción Arancelaria reglamentada (Decreto 557/2023).
 */
class ExcepcionArancelariaBE {
  constructor({
    poseeExcepcion = false,
    anexoExcepcion = '',
    arancelAplicable = 0,
    condicion = ''
  } = {}) {
    this.poseeExcepcion = Boolean(poseeExcepcion);
    this.anexoExcepcion = anexoExcepcion || '';
    this.arancelAplicable = parseFloat(arancelAplicable) || 0;
    this.condicion = condicion || '';
  }

  get tieneCondicionEspecial() {
    return Boolean(this.condicion && this.condicion.trim().length > 0);
  }
}

if (typeof window !== 'undefined') {
  window.ExcepcionArancelariaBE = ExcepcionArancelariaBE;
}
