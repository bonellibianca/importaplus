import 'server-only';

/**
 * Capa BE (Business Entities) - Servidor
 * Entidad de Dominio: GravamenItemBE
 * Representa una partida o concepto tributario liquidado individualmente en el comprobante aduanero.
 */
export class GravamenItemBE {
  constructor({
    id = '',
    concepto = '',
    alicuota = '-',
    alicuotaNum = 0,
    montoUSD = 0,
    esSubtotal = false,
    descripcion = ''
  } = {}) {
    this.id = id;
    this.concepto = concepto;
    this.alicuota = alicuota;
    this.alicuotaNum = Number(alicuotaNum) || 0;
    this.montoUSD = Number(Number(montoUSD).toFixed(2));
    this.esSubtotal = Boolean(esSubtotal);
    this.descripcion = descripcion;
  }
}
