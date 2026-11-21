import 'server-only';
import { cargarCatalogoNcmExcel } from './excelClient';
import { PosicionArancelariaBE } from '../be/PosicionArancelariaBE';

/**
 * Normaliza texto para búsqueda: minúsculas, sin tildes ni signos.
 */
function normalizarTexto(str) {
  return String(str || '')
    .toLowerCase()
    .normalize('NFD')
    .replace(/[\u0300-\u036f]/g, '')
    .replace(/[^a-z\s]/g, ' ')
    .replace(/\s+/g, ' ')
    .trim();
}

/**
 * Capa DAL: Acceso a Datos de Posiciones Arancelarias NCM mediante Excel
 * Lee directamente del catálogo Excel (Cliente/Nomenclatura_NCM_aranceles.xlsx
 * y DAL/database/ImportaPlus_ExcelDB.xlsx) cumpliendo con respuesta < 250ms.
 */
export class NcmDAL {
  /**
   * Búsqueda predictiva incremental EXCLUSIVAMENTE por nombre / descripción del producto (no por código NCM)
   * @param {string} query - Término de búsqueda por nombre (ej: "computadora", "remera", "notebook")
   * @param {number} limit - Límite de resultados (máximo 12 para evitar sobrecarga)
   * @returns {Promise<PosicionArancelariaBE[]>}
   */
  static async buscarPorDescripcion(query, limit = 12) {
    const queryNorm = normalizarTexto(query);
    if (!queryNorm || queryNorm.length < 2) return [];

    const tokens = queryNorm.split(/\s+/).filter((t) => t.length >= 2);
    if (tokens.length === 0) return [];

    const catalogo = cargarCatalogoNcmExcel();
    const scoredMatches = [];

    for (const item of catalogo) {
      const descRaw = item.descripcion || '';
      const catRaw = item.categoria || '';
      const descNorm = normalizarTexto(descRaw);
      const catNorm = normalizarTexto(catRaw);
      const fullNorm = `${descNorm} ${catNorm}`;

      // Verificar que todos los términos buscados aparezcan en el nombre/descripción/categoría
      let matchesAll = true;
      for (const tok of tokens) {
        if (!fullNorm.includes(tok)) {
          matchesAll = false;
          break;
        }
      }
      if (!matchesAll) continue;

      // Extraer el segmento específico final si la descripción tiene jerarquía '>'
      const partes = descRaw.split('>').map((s) => s.trim()).filter(Boolean);
      const ultimoSeg = partes.length > 0 ? partes[partes.length - 1] : descRaw;
      const espNorm = normalizarTexto(ultimoSeg);
      const esLosDemas = /^(los demas|las demas)$/.test(espNorm);

      let score = 0;
      if (item.esDestacado) score += 600;
      if (espNorm.startsWith(queryNorm)) score += 500;
      else if (espNorm.includes(` ${queryNorm}`)) score += 380;
      else if (espNorm.includes(queryNorm)) score += 250;

      for (const tok of tokens) {
        if (espNorm.startsWith(tok) || espNorm.includes(` ${tok}`)) score += 180;
        else if (espNorm.includes(tok)) score += 90;
        if (catNorm.startsWith(tok) || catNorm.includes(` ${tok}`)) score += 80;
      }

      if (esLosDemas) score -= 120;

      scoredMatches.push({ item, score });
    }

    scoredMatches.sort((a, b) => {
      if (b.score !== a.score) return b.score - a.score;
      return (a.item.descripcion || '').length - (b.item.descripcion || '').length;
    });

    return scoredMatches.slice(0, limit).map(({ item }) => new PosicionArancelariaBE(item));
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
