import 'server-only';

/**
 * Capa BE (Business Entities) - Servidor
 * Entidad de Dominio: PaisBE
 * Modela el país de origen/embarque de la mercadería y su régimen comercial (Intrazona / Extrazona).
 */
export class PaisBE {
  constructor({
    codigo_iso = '',
    nombre = '',
    es_mercosur = false,
    flete_estimado_pct = 8.00
  } = {}) {
    this.codigo_iso = codigo_iso;
    this.nombre = nombre;
    this.es_mercosur = Boolean(es_mercosur);
    this.flete_estimado_pct = Number(flete_estimado_pct) || 8.00;
  }
}
