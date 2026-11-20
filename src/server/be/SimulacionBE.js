import 'server-only';

/**
 * Capa BE (Business Entities) - Servidor
 * Entidad de Dominio: SimulacionBE
 * Modela el comprobante certificado e inmutable con Hash ID único (IMP-YYYY-XXXXXX).
 */
export class SimulacionBE {
  constructor({
    idCotizacion = '',
    fechaEmision = new Date().toISOString(),
    fichaProducto = {},
    parametrosComerciales = {},
    desgloseTributario = [],
    resumenEconomico = {},
    puertosRecomendados = []
  } = {}) {
    this.idCotizacion = idCotizacion;
    this.fechaEmision = fechaEmision;
    this.fichaProducto = fichaProducto;
    this.parametrosComerciales = parametrosComerciales;
    this.desgloseTributario = Array.isArray(desgloseTributario) ? desgloseTributario : [];
    this.resumenEconomico = resumenEconomico;
    this.puertosRecomendados = Array.isArray(puertosRecomendados) ? puertosRecomendados : [];
  }
}
