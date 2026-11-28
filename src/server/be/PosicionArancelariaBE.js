import 'server-only';

/**
 * Capa BE (Business Entities) - Servidor
 * Entidad de Dominio: PosicionArancelariaBE
 * Representa la posición arancelaria del Nomenclador Común del Mercosur (NCM).
 */
export class PosicionArancelariaBE {
  constructor({
    codigo_ncm = '',
    descripcion = '',
    categoria = '',
    tipo_bien = 'GENERAL', // 'BK' (Bien de Capital), 'BIT' (Informática/Telecom), 'GENERAL'
    aec = 0,
    iva_alicuota = 21.0,
    anexo_excepcion = null,
    arancel_excepcion = null,
    condicion_excepcion = null
  } = {}) {
    this.codigo_ncm = codigo_ncm;
    this.descripcion = descripcion;
    this.categoria = categoria || (descripcion.includes('>') ? descripcion.split('>')[0].trim() : '');
    this.tipo_bien = tipo_bien;
    this.aec = Number(aec) || 0;
    this.iva_alicuota = Number(iva_alicuota) || 21.0;
    this.anexo_excepcion = anexo_excepcion;
    this.arancel_excepcion = arancel_excepcion !== null ? Number(arancel_excepcion) : null;
    this.condicion_excepcion = condicion_excepcion;
  }

  get esBienDeCapital() {
    return this.tipo_bien === 'BK';
  }

  get esInformaticaTelecom() {
    return this.tipo_bien === 'BIT';
  }

  get tieneExcepcion() {
    return Boolean(this.anexo_excepcion && this.arancel_excepcion !== null);
  }
}
