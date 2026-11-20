/**
 * Capa DAL (Data Access Layer)
 * Acceso a Datos: PosicionArancelariaDAL
 * 
 * Contiene EXCLUSIVAMENTE métodos para leer, consultar y mapear las posiciones NCM
 * desde la persistencia en Excel (ExcelDataLoaderDAL).
 */
class PosicionArancelariaDAL {
  constructor() {
    this._cachePosiciones = null;
  }

  async _cargarDesdeExcel() {
    if (this._cachePosiciones) return;

    const PosicionClass = typeof window !== 'undefined' ? window.PosicionArancelariaBE : null;
    const excelLoader = typeof window !== 'undefined' ? window.ExcelDataLoaderDAL : null;

    if (excelLoader) {
      const excelDb = await excelLoader.obtenerBaseDatosExcel();
      if (excelDb && Array.isArray(excelDb.posicionesDestacadas) && excelDb.posicionesDestacadas.length > 0) {
        this._cachePosiciones = excelDb.posicionesDestacadas.map((item) => {
          const aec = parseFloat(item.aec || 0);
          const poseeEx = Boolean(
            item.anexo_excepcion ||
            (item.arancel_excepcion !== null && item.arancel_excepcion !== undefined && item.arancel_excepcion !== '')
          );
          const arancelAplicable = poseeEx && item.arancel_excepcion !== null && item.arancel_excepcion !== ''
            ? parseFloat(item.arancel_excepcion)
            : aec;

          return new PosicionClass({
            codigoNcm: item.codigo_ncm,
            descripcion: item.descripcion,
            categoria: item.categoria || 'Mercadería General',
            capitulo: item.capitulo || String(item.codigo_ncm || '').slice(0, 2),
            arancelExternoComun: aec,
            esBienDeCapital: item.tipo_bien === 'BK',
            esInformaticaTelecom: item.tipo_bien === 'BIT',
            poseeExcepcion: poseeEx,
            anexoExcepcion: item.anexo_excepcion || '',
            arancelAplicableExcepcion: arancelAplicable,
            condicionExcepcion: item.condicion_excepcion || ''
          });
        });
        return;
      }
    }

    this._cachePosiciones = [];
  }

  async obtenerTodas() {
    await this._cargarDesdeExcel();
    return this._cachePosiciones;
  }

  async buscarPorTexto(texto, limite = 12) {
    await this._cargarDesdeExcel();
    if (!texto || !texto.trim()) return [];

    const term = texto.trim().toLowerCase();

    const filtrados = this._cachePosiciones.filter(pos => {
      const descLower = pos.descripcion.toLowerCase();
      const catLower = pos.categoria.toLowerCase();
      return descLower.includes(term) || catLower.includes(term);
    });

    filtrados.sort((a, b) => {
      const aDesc = a.descripcion.toLowerCase();
      const bDesc = b.descripcion.toLowerCase();
      if (aDesc.startsWith(term) && !bDesc.startsWith(term)) return -1;
      if (!aDesc.startsWith(term) && bDesc.startsWith(term)) return 1;
      return 0;
    });

    return filtrados.slice(0, limite);
  }

  async obtenerPorCodigo(codigoNcm) {
    await this._cargarDesdeExcel();
    const cleanSearch = (codigoNcm || '').trim().replace(/[.\s-]/g, '');
    return this._cachePosiciones.find(pos => pos.codigoNcm.replace(/[.\s-]/g, '') === cleanSearch) || null;
  }
}

if (typeof window !== 'undefined') {
  window.PosicionArancelariaDAL = PosicionArancelariaDAL;
}
