/**
 * Capa DAL (Data Access Layer)
 * Acceso a Datos: CalculadoraArancelariaDAL
 * 
 * Contiene EXCLUSIVAMENTE métodos de acceso, lectura y persistencia de parámetros y liquidaciones en Excel.
 */
class CalculadoraArancelariaDAL {
  constructor() {
    this._parametrosCache = null;
    this._historialSimulaciones = [];
  }

  /**
   * Obtiene los parámetros impositivos oficiales almacenados en la base de datos Excel.
   * Conecta en tiempo real con la API oficial de cotización BNA (dolarapi.com) con respaldo automático en Excel.
   */
  async obtenerParametrosRegulatorios() {
    if (this._parametrosCache) {
      return this._parametrosCache;
    }

    const excelLoader = typeof window !== 'undefined' ? window.ExcelDataLoaderDAL : null;
    let params = null;
    if (excelLoader) {
      const excelDb = await excelLoader.obtenerBaseDatosExcel();
      if (excelDb && excelDb.parametrosRegulatorios) {
        params = { ...excelDb.parametrosRegulatorios };
      }
    }

    if (!params) {
      params = {
        tasaEstadisticaPct: 3.0,
        tasaEstadisticaTope10k: 180.0,
        tasaEstadisticaTope100k: 3000.0,
        tasaEstadisticaTope1M: 30000.0,
        tasaEstadisticaTopeMax: 150000.0,
        ivaGeneralPct: 21.0,
        ivaReducidoBKPct: 10.5,
        percepcionIvaGeneralPct: 20.0,
        percepcionIvaReducidoPct: 10.0,
        percepcionGananciasGeneralPct: 6.0,
        percepcionGananciasNoCatPct: 11.0,
        arancelSimFijoUSD: 28.0,
        digitalizacionFijaUSD: 10.0,
        totalSimDigitalizacionUSD: 38.0,
        cotizacionOficialBNA: 1420.50
      };
    }

    // Consulta en vivo a la API oficial de tipo de cambio BNA con fallback automático al Excel
    try {
      const resp = await fetch('https://dolarapi.com/v1/dolares/oficial');
      if (resp.ok) {
        const data = await resp.json();
        const venta = parseFloat(data.venta || data.value_sell);
        if (venta && !isNaN(venta) && venta > 0) {
          params.cotizacionOficialBNA = venta;
          params.fuenteCotizacion = 'API Dólar Oficial BNA (en vivo)';
          params.fechaCotizacion = data.fechaActualizacion || new Date().toISOString();
        }
      }
    } catch (e) {
      console.warn('API Dólar no disponible en este momento. Utilizando cotización de respaldo Excel ($' + params.cotizacionOficialBNA + '):', e.message);
    }

    this._parametrosCache = params;
    return this._parametrosCache;
  }

  /**
   * Persiste una liquidación calculada en el registro Excel / memoria.
   * @param {CalculadoraArancelariaBE} be
   */
  async guardarSimulacion(be) {
    if (!be) return false;
    this._historialSimulaciones.push({
      hash: be.hashCotizacion,
      fecha: be.fechaHora.toISOString(),
      ncm: be.posicion?.codigoNcm,
      fobTotal: be.fobTotal,
      cifAduanero: be.cifAduanero,
      costoTotalUSD: be.costoTotalNacionalizado,
      costoTotalARS: be.costoTotalARS
    });
    if (typeof window !== 'undefined' && window.ExcelDataLoaderDAL) {
      window.ExcelDataLoaderDAL.registrarSimulacionEnExcel(be);
    }
    return true;
  }

  /**
   * Recupera el historial de simulaciones persistidas.
   */
  async obtenerHistorial() {
    return this._historialSimulaciones;
  }
}

if (typeof window !== 'undefined') {
  window.CalculadoraArancelariaDAL = CalculadoraArancelariaDAL;
}
