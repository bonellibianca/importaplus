import 'server-only';
import { cargarCatalogoNcmExcel } from './excelClient';
import { getCachedOrFetch, supabaseSelect, isSupabaseConfigured } from './supabaseClient';
import { PosicionArancelariaBE } from '../be/PosicionArancelariaBE';

function normalizarTexto(str) {
  return String(str || '')
    .toLowerCase()
    .normalize('NFD')
    .replace(/[\u0300-\u036f]/g, '')
    .replace(/[^a-z0-9\s]/g, ' ')
    .replace(/\s+/g, ' ')
    .trim();
}

/**
 * Capa DAL Híbrida para Posiciones NCM
 * Soporta búsqueda por descripción alfanumérica (ej: "4G", "USB", "Notebook") y por código NCM (ej: "8471.30").
 */
export class NcmDAL {
  static async _obtenerCatalogoUnificado() {
    const baseCatalogo = cargarCatalogoNcmExcel();

    if (!isSupabaseConfigured()) {
      return baseCatalogo;
    }

    const supaRows = await getCachedOrFetch('ncm:supabase_overrides', 10 * 60 * 1000, async () => {
      const allRows = [];
      const pageSize = 1000;
      for (let offset = 0; offset < 15000; offset += pageSize) {
        const batch = await supabaseSelect(
          'ncm_catalogo',
          `select=codigo_ncm,descripcion,tipo_bien,aec,iva_alicuota,anexo_excepcion,arancel_excepcion,condicion_excepcion&order=codigo_ncm.asc&offset=${offset}&limit=${pageSize}`
        );
        if (!Array.isArray(batch) || batch.length === 0) break;
        allRows.push(...batch);
        if (batch.length < pageSize) break;
      }
      return allRows;
    });

    if (!Array.isArray(supaRows) || supaRows.length === 0) {
      return baseCatalogo;
    }

    const map = new Map(baseCatalogo.map((item) => [item.codigo_ncm, item]));
    for (const r of supaRows) {
      if (!r.codigo_ncm) continue;
      const tipoBien = (r.tipo_bien || 'GENERAL').toUpperCase();
      map.set(r.codigo_ncm.trim(), {
        codigo_ncm: r.codigo_ncm.trim(),
        descripcion: r.descripcion || '',
        categoria: 'Catálogo Oficial Supabase',
        tipo_bien: tipoBien,
        aec: Number(r.aec) || 0,
        iva_alicuota: Number(r.iva_alicuota) > 0
          ? Number(r.iva_alicuota)
          : (tipoBien === 'BK' || tipoBien === 'BIT' ? 10.5 : 21.0),
        anexo_excepcion: r.anexo_excepcion || null,
        arancel_excepcion: r.arancel_excepcion !== null && r.arancel_excepcion !== undefined ? Number(r.arancel_excepcion) : null,
        condicion_excepcion: r.condicion_excepcion || null,
        esDestacado: true
      });
    }

    return Array.from(map.values());
  }

  static async buscarPorDescripcion(query, limit = 12) {
    const rawTrimmed = String(query || '').trim();
    if (rawTrimmed.length < 2) return [];

    const queryDigitsOnly = rawTrimmed.replace(/[.\s-]/g, '');
    const esConsultaCodigoNcm = /^\d{2,12}[a-zA-Z0-9]{0,4}$/.test(queryDigitsOnly) && /^\d/.test(queryDigitsOnly);

    const queryNorm = normalizarTexto(rawTrimmed);
    const tokens = queryNorm.split(/\s+/).filter((t) => t.length >= 2);
    if (!esConsultaCodigoNcm && tokens.length === 0) return [];

    const catalogo = await this._obtenerCatalogoUnificado();
    const scoredMatches = [];

    for (const item of catalogo) {
      const codigoRaw = String(item.codigo_ncm || '');
      const codigoDigits = codigoRaw.replace(/[.\s-]/g, '').toLowerCase();

      if (esConsultaCodigoNcm) {
        const qLower = queryDigitsOnly.toLowerCase();
        if (codigoDigits === qLower || codigoRaw.toLowerCase() === rawTrimmed.toLowerCase()) {
          scoredMatches.push({ item, score: 2000 + (item.esDestacado ? 300 : 0) });
          continue;
        }
        if (codigoDigits.startsWith(qLower) || codigoRaw.toLowerCase().startsWith(rawTrimmed.toLowerCase())) {
          scoredMatches.push({ item, score: 1400 + (item.esDestacado ? 300 : 0) });
          continue;
        }
      }

      if (tokens.length === 0) continue;

      const descRaw = item.descripcion || '';
      const catRaw = item.categoria || '';
      const descNorm = normalizarTexto(descRaw);
      const catNorm = normalizarTexto(catRaw);
      const fullNorm = `${descNorm} ${catNorm} ${codigoRaw.toLowerCase()}`;

      let matchesAll = true;
      for (const tok of tokens) {
        if (!fullNorm.includes(tok)) {
          matchesAll = false;
          break;
        }
      }
      if (!matchesAll) continue;

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

  static async obtenerPorCodigo(codigoNcm) {
    const cleanCode = (codigoNcm || '').trim();
    const cleanDigits = cleanCode.replace(/[.\s-]/g, '');
    const catalogo = await this._obtenerCatalogoUnificado();

    const match = catalogo.find(
      (item) =>
        item.codigo_ncm === cleanCode ||
        item.codigo_ncm.replace(/[.\s-]/g, '') === cleanDigits
    );

    return match ? new PosicionArancelariaBE(match) : null;
  }
}
