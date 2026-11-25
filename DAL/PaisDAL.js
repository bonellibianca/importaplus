/**
 * Capa DAL (Data Access Layer)
 * Acceso a Datos: PaisDAL
 * 
 * Contiene EXCLUSIVAMENTE métodos para leer y mapear los 195 países y sus bloques desde Excel.
 */
class PaisDAL {
  constructor() {
    this._cachePaises = null;
  }

  async _cargarDesdeExcel() {
    if (this._cachePaises) return;

    const PaisClass = typeof window !== 'undefined' ? window.PaisBE : null;
    const excelLoader = typeof window !== 'undefined' ? window.ExcelDataLoaderDAL : null;

    if (excelLoader) {
      const excelDb = await excelLoader.obtenerBaseDatosExcel();
      if (excelDb && Array.isArray(excelDb.paises) && excelDb.paises.length > 0) {
        this._cachePaises = excelDb.paises.map((p) => new PaisClass({
          codigo: p.codigo_iso2 || p.codigo_iso || '',
          nombre: (p.nombre || '').split('(')[0].trim(),
          bloque: p.bloque || (p.es_mercosur ? 'MERCOSUR' : 'EXTRAZONA'),
          esIntrazona: Boolean(p.es_mercosur === true || String(p.es_mercosur).toLowerCase() === 'true'),
          fleteParametricoStd: parseFloat(p.flete_parametrico_usd || 2000),
          seguroPctStd: parseFloat(p.seguro_pct_std || 1.0)
        }));
        return;
      }
    }

    this._cachePaises = [];
  }

  async obtenerPaises() {
    await this._cargarDesdeExcel();
    return this._cachePaises;
  }

  async obtenerPorCodigo(codigo) {
    await this._cargarDesdeExcel();
    const clean = String(codigo || '').trim().toUpperCase();
    return this._cachePaises.find(p => String(p.codigo || '').toUpperCase() === clean) || null;
  }
}

if (typeof window !== 'undefined') {
  window.PaisDAL = PaisDAL;
}
