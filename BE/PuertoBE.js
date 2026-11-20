/**
 * Capa BE (Business Entities)
 * Entidad de Negocio: PuertoBE
 * 
 * Modela un punto de ingreso aduanero nacional (puerto marítimo, fluvial, aéreo o paso fronterizo).
 */
class PuertoBE {
  constructor(data = {}) {
    this.id = data.id || '';
    this.nombre = data.name || data.nombre || '';
    this.tipo = data.type || data.tipo || 'Marítimo';
    this.lat = data.lat || 0;
    this.lng = data.lng || 0;
    this.descripcion = data.desc || data.descripcion || '';
    this.operador = data.operador || '';
  }
}

if (typeof window !== 'undefined') {
  window.PuertoBE = PuertoBE;
}
