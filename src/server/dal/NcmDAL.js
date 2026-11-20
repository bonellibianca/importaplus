import 'server-only';
import { cargarCatalogoNcmExcel } from './excelClient';
import { PosicionArancelariaBE } from '../be/PosicionArancelariaBE';

/**
 * Capa DAL: Acceso a Datos de Posiciones Arancelarias NCM mediante Excel
 * Lee directamente del catálogo Excel (Cliente/Nomenclatura_NCM_aranceles.xlsx
 * y DAL/database/ImportaPlus_ExcelDB.xlsx) cumpliendo con respuesta < 250ms.
 */
export class NcmDAL {
  /**
   * Búsqueda predictiva incremental por descripción, nombre coloquial o código NCM
   * @param {string} query - Término de búsqueda (ej: "computadora", "remera", "notebook")
   * @param {number} limit - Límite de resultados (máximo 12 para evitar sobrecarga)
   * @returns {Promise<PosicionArancelariaBE[]>}
   */
  static async buscarPorDescripcion(query, limit = 12) {
    const cleanQuery = (query || '').trim().toLowerCase();
    if (!cleanQuery) return [];

    const cleanDigits = cleanQuery.replace(/[.\s-]/g, '');
    const catalogo = cargarCatalogoNcmExcel();

    const matches = [];
    for (const item of catalogo) {
      const descLower = (item.descripcion || '').toLowerCase();
      const catLower = (item.categoria || '').toLowerCase();
      const codeClean = (item.codigo_ncm || '').replace(/[.\s-]/g, '');

      if (
        descLower.includes(cleanQuery) ||
        catLower.includes(cleanQuery) ||
        item.codigo_ncm.includes(cleanQuery) ||
        (cleanDigits.length >= 2 && codeClean.includes(cleanDigits))
      ) {
        matches.push(item);
      }
    }

    // Priorizar posiciones frecuentes curadas y coincidencias al inicio
    matches.sort((a, b) => {
      if (a.esDestacado && !b.esDestacado) return -1;
      if (!a.esDestacado && b.esDestacado) return 1;
      const aStarts = a.descripcion.toLowerCase().startsWith(cleanQuery);
      const bStarts = b.descripcion.toLowerCase().startsWith(cleanQuery);
      if (aStarts && !bStarts) return -1;
      if (!aStarts && bStarts) return 1;
      return 0;
    });

    return matches.slice(0, limit).map((item) => new PosicionArancelariaBE(item));
  }

  /**
   * Obtiene una posición NCM exacta por su código de 8 dígitos desde el catálogo Excel
   * @param {string} codigoNcm - Código NCM (ej: '8471.30.12')
   * @returns {Promise<PosicionArancelariaBE | null>}
   */
  static async obtenerPorCodigo(codigoNcm) {
    const cleanCode = (codigoNcm || '').trim();
    const cleanDigits = cleanCode.replace(/[.\s-]/g, '');
    const catalogo = cargarCatalogoNcmExcel();

    const match = catalogo.find(
      (item) =>
        item.codigo_ncm === cleanCode ||
        item.codigo_ncm.replace(/[.\s-]/g, '') === cleanDigits
    );

    return match ? new PosicionArancelariaBE(match) : null;
  }
}
