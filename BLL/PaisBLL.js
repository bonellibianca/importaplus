/**
 * Capa BLL (Business Logic Layer)
 * Lógica de Negocio: PaisBLL
 * 
 * Contiene EXCLUSIVAMENTE métodos de consulta, búsqueda incremental sobre los 195 países,
 * clasificación de bloques (Mercosur Intrazona) y estimaciones de flete/seguro sobre PaisBE.
 */
class PaisBLL {
  constructor(paisDAL = null) {
    const DALClass = typeof window !== 'undefined' ? window.PaisDAL : null;
    this.paisDAL = paisDAL || (DALClass ? new DALClass() : null);
  }

  async obtenerPaises() {
    return await this.paisDAL.obtenerPaises();
  }

  async obtenerPorCodigo(codigo) {
    return await this.paisDAL.obtenerPorCodigo(codigo);
  }

  /**
   * Búsqueda incremental predictiva de países exclusivamente por letras (nombre)
   * @param {string} texto
   * @param {number} limite
   */
  async buscarPaises(texto, limite = 12) {
    const todos = await this.obtenerPaises();
    const limpio = String(texto || '').replace(/[^a-zA-ZáéíóúÁÉÍÓÚñÑüÜ\s]/g, '').trim();
    if (!limpio) {
      return todos.slice(0, limite);
    }

    const normalizar = (s) =>
      String(s || '')
        .toLowerCase()
        .normalize('NFD')
        .replace(/[\u0300-\u036f]/g, '');

    const term = normalizar(limpio);

    const filtrados = todos.filter((p) => {
      const nom = normalizar(p.nombre);
      return nom.includes(term);
    });

    filtrados.sort((a, b) => {
      const aNom = normalizar(a.nombre);
      const bNom = normalizar(b.nombre);
      const aStarts = aNom.startsWith(term);
      const bStarts = bNom.startsWith(term);
      if (aStarts && !bStarts) return -1;
      if (!aStarts && bStarts) return 1;
      return aNom.localeCompare(bNom);
    });

    return filtrados.slice(0, limite);
  }

  /**
   * Estima los importes de flete y seguro paramétricos basados en el país y el FOB (RF02.d).
   * @param {number} fobTotal 
   * @param {PaisBE} pais 
   */
  estimarFleteYSeguro(fobTotal, pais) {
    if (!fobTotal || fobTotal <= 0) {
      return { flete: 0, seguro: 0 };
    }
    const seguroCalculado = +(Math.max(10, fobTotal * ((pais?.seguroPctStd || 1.0) / 100))).toFixed(2);
    const fleteCalculado = pais?.fleteParametricoStd || 1800;
    return {
      flete: fleteCalculado,
      seguro: seguroCalculado
    };
  }
}

if (typeof window !== 'undefined') {
  window.PaisBLL = PaisBLL;
}
