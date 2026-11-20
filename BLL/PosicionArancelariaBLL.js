/**
 * Capa BLL (Business Logic Layer)
 * Lógica de Negocio: PosicionArancelariaBLL
 * 
 * Contiene EXCLUSIVAMENTE métodos de búsqueda predictiva, validación y reglas de excepciones sobre PosicionArancelariaBE.
 */
class PosicionArancelariaBLL {
  constructor(posicionDAL = null) {
    const DALClass = typeof window !== 'undefined' ? window.PosicionArancelariaDAL : null;
    this.posicionDAL = posicionDAL || (DALClass ? new DALClass() : null);
  }

  /**
   * Búsqueda predictiva incremental por nombre o descripción (RF01.1).
   * @param {string} texto 
   * @param {number} limite 
   */
  async buscarPosiciones(texto, limite = 12) {
    if (!texto || texto.trim().length < 2) {
      return [];
    }
    return await this.posicionDAL.buscarPorTexto(texto, limite);
  }

  async obtenerPorCodigo(codigoNcm) {
    return await this.posicionDAL.obtenerPorCodigo(codigoNcm);
  }

  async obtenerTodas() {
    return await this.posicionDAL.obtenerTodas();
  }

  /**
   * Determina el régimen arancelario resultante de una posición (RF01.2).
   * @param {PosicionArancelariaBE} posicion 
   * @param {boolean} aplicarExcepcion 
   */
  evaluarRegimen(posicion, aplicarExcepcion = true) {
    if (!posicion) return null;

    if (posicion.poseeExcepcion && aplicarExcepcion) {
      return {
        tipo: 'EXCEPCION',
        alicuota: posicion.arancelAplicableExcepcion,
        decreto: posicion.anexoExcepcion,
        condicion: posicion.condicionExcepcion,
        tieneCondicion: Boolean(posicion.condicionExcepcion && posicion.condicionExcepcion.trim().length > 0)
      };
    }

    return {
      tipo: 'AEC_GENERAL',
      alicuota: posicion.arancelExternoComun,
      decreto: 'Arancel Externo Común Mercosur',
      condicion: '',
      tieneCondicion: false
    };
  }
}

if (typeof window !== 'undefined') {
  window.PosicionArancelariaBLL = PosicionArancelariaBLL;
}
