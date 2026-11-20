/**
 * Capa BE (Business Entities)
 * Entidad de Negocio: GravamenItemBE
 * 
 * Contiene EXCLUSIVAMENTE los atributos de una partida tributaria o arancelaria discriminada.
 */
class GravamenItemBE {
  constructor({
    id = '',
    codigo = '',
    nombre = '',
    baseCalculoUSD = 0,
    alicuotaPct = 0,
    montoUSD = 0,
    porcentajeSobreTotal = 0,
    descripcion = ''
  } = {}) {
    this.id = id;
    this.codigo = codigo;
    this.nombre = nombre;
    this.baseCalculoUSD = parseFloat(baseCalculoUSD) || 0;
    this.alicuotaPct = parseFloat(alicuotaPct) || 0;
    this.montoUSD = parseFloat(montoUSD) || 0;
    this.porcentajeSobreTotal = parseFloat(porcentajeSobreTotal) || 0;
    this.descripcion = descripcion;
  }
}

if (typeof window !== 'undefined') {
  window.GravamenItemBE = GravamenItemBE;
}
