/**
 * Capa BE (Business Entities)
 * Entidad de Negocio: PosicionArancelariaBE
 * 
 * Contiene EXCLUSIVAMENTE los atributos oficiales de la posición arancelaria NCM.
 */
class PosicionArancelariaBE {
  constructor({
    codigoNcm = '',
    descripcion = '',
    categoria = '',
    capitulo = '',
    arancelExternoComun = 0,
    esBienDeCapital = false,
    esInformaticaTelecom = false,
    poseeExcepcion = false,
    anexoExcepcion = '',
    arancelAplicableExcepcion = 0,
    condicionExcepcion = ''
  } = {}) {
    this.codigoNcm = codigoNcm;
    this.descripcion = descripcion;
    this.categoria = categoria;
    this.capitulo = capitulo;
    this.arancelExternoComun = parseFloat(arancelExternoComun) || 0;
    this.esBienDeCapital = Boolean(esBienDeCapital);
    this.esInformaticaTelecom = Boolean(esInformaticaTelecom);
    this.poseeExcepcion = Boolean(poseeExcepcion);
    this.anexoExcepcion = anexoExcepcion || '';
    this.arancelAplicableExcepcion = parseFloat(arancelAplicableExcepcion) || 0;
    this.condicionExcepcion = condicionExcepcion || '';
  }
}

if (typeof window !== 'undefined') {
  window.PosicionArancelariaBE = PosicionArancelariaBE;
}
