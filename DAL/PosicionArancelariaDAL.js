/**
 * Capa DAL (Data Access Layer)
 * Acceso a Datos: PosicionArancelariaDAL
 * 
 * Contiene EXCLUSIVAMENTE métodos para leer, consultar y mapear las posiciones NCM
 * desde la persistencia en Excel (ExcelDataLoaderDAL + datos_excels.js con las 10.437 posiciones NCM).
 */
class PosicionArancelariaDAL {
  constructor() {
    this._cachePosiciones = null;
  }

  /**
   * Normaliza texto para búsqueda: minúsculas, sin tildes ni diéresis, solo caracteres alfabéticos/espacios.
   */
  _normalizar(str) {
    return String(str || '')
      .toLowerCase()
      .normalize('NFD')
      .replace(/[\u0300-\u036f]/g, '')
      .replace(/[^a-z0-9\s]/g, ' ')
      .replace(/\s+/g, ' ')
      .trim();
  }

  /**
   * Limpia descripciones provenientes del nomenclador oficial eliminando sub-items redundantes con codificación '?'
   */
  _limpiarDescripcionVisual(rawDesc) {
    if (!rawDesc) return '';
    let limpia = String(rawDesc)
      .replace(/«|»|<<|>>/g, '"')
      .replace(/\*\s*/g, '')
      .trim();

    // Si termina con un paréntesis de sub-items SIM (ej: "(--Los dem?s | ...)") lo quitamos de la vista limpia
    const idxParen = limpia.lastIndexOf(' (');
    if (idxParen > 15) {
      const sub = limpia.slice(idxParen);
      if (sub.includes('--') || sub.includes('?') || sub.includes('|')) {
        limpia = limpia.slice(0, idxParen).trim();
      }
    }

    // Extraer los segmentos más representativos si la jerarquía '>' es muy extensa
    const partes = limpia.split(' > ').map((s) => s.trim()).filter(Boolean);
    if (partes.length >= 2) {
      const primero = partes[0].length > 110 ? partes[0].slice(0, 110).replace(/[;,:]+$/, '') + '...' : partes[0];
      const ultimos = partes.slice(1).join(' › ');
      return `${primero} › ${ultimos}`;
    }
    return limpia;
  }

  /**
   * Extrae el nombre específico del producto (último segmento significativo de la jerarquía NCM)
   */
  _extraerNombreEspecifico(rawDesc, categoria) {
    const limpia = String(rawDesc || '').replace(/\s*\([^\)]*\)\s*$/, '').trim();
    const partes = limpia.split(' > ').map((s) => s.trim()).filter(Boolean);
    if (partes.length === 0) return categoria || '';

    const ultimo = partes[partes.length - 1];
    const esGenerico = /^(los demas|las demas|otros|otras)$/i.test(this._normalizar(ultimo));
    if (esGenerico && partes.length >= 2) {
      return `${partes[partes.length - 2]} (${ultimo})`;
    }
    return ultimo;
  }

  async _cargarDesdeExcel() {
    if (this._cachePosiciones) return;

    const PosicionClass = typeof window !== 'undefined' ? window.PosicionArancelariaBE : null;
    const excelLoader = typeof window !== 'undefined' ? window.ExcelDataLoaderDAL : null;
    const mapaPorCodigo = new Map();

    // 1. Cargar posiciones destacadas curadas (con nombres comerciales y coloquiales)
    if (excelLoader) {
      const excelDb = await excelLoader.obtenerBaseDatosExcel();
      if (excelDb && Array.isArray(excelDb.posicionesDestacadas) && excelDb.posicionesDestacadas.length > 0) {
        excelDb.posicionesDestacadas.forEach((item) => {
          const codigo = String(item.codigo_ncm || '').trim();
          if (!codigo) return;

          const aec = parseFloat(item.aec || 0);
          const poseeEx = Boolean(
            item.anexo_excepcion ||
            (item.arancel_excepcion !== null && item.arancel_excepcion !== undefined && item.arancel_excepcion !== '')
          );
          const arancelAplicable = poseeEx && item.arancel_excepcion !== null && item.arancel_excepcion !== ''
            ? parseFloat(item.arancel_excepcion)
            : aec;

          const inst = PosicionClass
            ? new PosicionClass({
                codigoNcm: codigo,
                descripcion: item.descripcion,
                categoria: item.categoria || 'Mercadería General',
                capitulo: item.capitulo || codigo.slice(0, 2),
                arancelExternoComun: aec,
                alicuotaIva: item.iva_alicuota !== undefined && item.iva_alicuota !== '' ? parseFloat(item.iva_alicuota) : null,
                esBienDeCapital: item.tipo_bien === 'BK',
                esInformaticaTelecom: item.tipo_bien === 'BIT',
                poseeExcepcion: poseeEx,
                anexoExcepcion: item.anexo_excepcion || '',
                arancelAplicableExcepcion: arancelAplicable,
                condicionExcepcion: item.condicion_excepcion || ''
              })
            : null;

          if (inst) {
            inst.esDestacado = true;
            inst._nombreEspecificoNorm = this._normalizar(item.descripcion);
            inst._categoriaNorm = this._normalizar(item.categoria);
            inst._textoBusquedaNorm = this._normalizar(`${item.descripcion} ${item.categoria || ''}`);
            inst._esLosDemas = false;
            mapaPorCodigo.set(codigo, inst);
          }
        });
      }
    }

    // 2. Cargar el catálogo completo de 10.437 posiciones oficiales de Excel (window.EXCEL_NCM_DATA en datos_excels.js)
    const rawExcelNcm = typeof window !== 'undefined' && Array.isArray(window.EXCEL_NCM_DATA)
      ? window.EXCEL_NCM_DATA
      : [];

    for (let i = 0; i < rawExcelNcm.length; i++) {
      const row = rawExcelNcm[i];
      const codigo = String(row.c || '').trim();
      if (!codigo) continue;

      if (mapaPorCodigo.has(codigo)) {
        // Enriquecer el vocabulario de búsqueda de la posición destacada con el texto completo del Excel
        const existente = mapaPorCodigo.get(codigo);
        existente._textoBusquedaNorm += ' ' + this._normalizar(row.d) + ' ' + this._normalizar(row.cat);
        continue;
      }

      const aec = parseFloat(row.aec || 0);
      const anexoRaw = String(row.ax || '').trim();
      const poseeEx = Boolean(anexoRaw);
      const arancelAplicable = poseeEx && row.ap !== undefined && row.ap !== null && row.ap !== ''
        ? parseFloat(row.ap)
        : aec;
      const tipoRaw = String(row.t || '').trim().toUpperCase();

      const descVisual = this._limpiarDescripcionVisual(row.d);
      const nombreEsp = this._extraerNombreEspecifico(row.d, row.cat);
      const catLimpia = String(row.cat || 'Mercadería General').replace(/«|»/g, '"').trim();

      const inst = PosicionClass
        ? new PosicionClass({
            codigoNcm: codigo,
            descripcion: descVisual || row.d || '',
            categoria: catLimpia,
            capitulo: row.cap || codigo.slice(0, 2),
            arancelExternoComun: aec,
            esBienDeCapital: tipoRaw === 'BK',
            esInformaticaTelecom: tipoRaw === 'BIT',
            poseeExcepcion: poseeEx,
            anexoExcepcion: anexoRaw ? (anexoRaw.startsWith('Anexo') ? anexoRaw : `Anexo ${anexoRaw} Dec. 557/2023`) : '',
            arancelAplicableExcepcion: arancelAplicable,
            condicionExcepcion: row.co || ''
          })
        : null;

      if (inst) {
        inst.esDestacado = false;
        inst._nombreEspecificoNorm = this._normalizar(nombreEsp);
        inst._categoriaNorm = this._normalizar(catLimpia);
        inst._textoBusquedaNorm = this._normalizar(`${nombreEsp} ${descVisual} ${catLimpia} ${row.d || ''}`);
        inst._esLosDemas = /\b(los demas|las demas)\b/.test(inst._nombreEspecificoNorm);
        mapaPorCodigo.set(codigo, inst);
      }
    }

    this._cachePosiciones = Array.from(mapaPorCodigo.values());
  }

  async obtenerTodas() {
    await this._cargarDesdeExcel();
    return this._cachePosiciones;
  }

  /**
   * Búsqueda incremental refactorizada EXCLUSIVAMENTE por nombre / descripción del producto (sin NCM).
   * Soporta múltiples palabras, insensible a tildes/mayúsculas, y ordena por relevancia semántica.
   */
  async buscarPorTexto(texto, limite = 12) {
    await this._cargarDesdeExcel();
    if (!this._cachePosiciones || this._cachePosiciones.length === 0) return [];

    const rawTrimmed = String(texto || '').trim();
    const soloDigitos = rawTrimmed.replace(/[.\s-]/g, '');
    const esCodigoNcm = /^\d{2,12}[a-zA-Z0-9]{0,4}$/.test(soloDigitos) && /^\d/.test(soloDigitos);

    const queryNorm = this._normalizar(rawTrimmed);
    if (!esCodigoNcm && (!queryNorm || queryNorm.length < 2)) {
      return [];
    }

    const tokens = queryNorm ? queryNorm.split(/\s+/).filter((t) => t.length >= 2) : [];
    if (!esCodigoNcm && tokens.length === 0) return [];

    const resultadosPuntuados = [];

    for (let i = 0; i < this._cachePosiciones.length; i++) {
      const pos = this._cachePosiciones[i];
      if (esCodigoNcm) {
        const posDigits = String(pos.codigoNcm || '').replace(/[.\s-]/g, '').toLowerCase();
        const qDigits = soloDigitos.toLowerCase();
        if (posDigits === qDigits) {
          resultadosPuntuados.push({ pos, score: 2000 + (pos.esDestacado ? 300 : 0) });
          continue;
        }
        if (posDigits.startsWith(qDigits)) {
          resultadosPuntuados.push({ pos, score: 1400 + (pos.esDestacado ? 300 : 0) });
          continue;
        }
      }

      if (tokens.length === 0) continue;
      const textoTotal = pos._textoBusquedaNorm || '';

      // Todos los tokens buscados deben estar presentes en el nombre/descripción/categoría
      let cumpleTodos = true;
      for (let t = 0; t < tokens.length; t++) {
        if (!textoTotal.includes(tokens[t])) {
          cumpleTodos = false;
          break;
        }
      }
      if (!cumpleTodos) continue;

      let score = 0;
      const espNorm = pos._nombreEspecificoNorm || '';
      const catNorm = pos._categoriaNorm || '';

      // 1. Priorizar productos destacados de uso frecuente
      if (pos.esDestacado) {
        score += 600;
      }

      // 2. Coincidencia exacta o al inicio del nombre específico del producto
      if (espNorm.startsWith(queryNorm)) {
        score += 500;
      } else if (espNorm.includes(` ${queryNorm}`)) {
        score += 380;
      } else if (espNorm.includes(queryNorm)) {
        score += 250;
      }

      // 3. Puntuar cada palabra buscada en el nombre específico y categoría
      for (let t = 0; t < tokens.length; t++) {
        const tok = tokens[t];
        if (espNorm.startsWith(tok) || espNorm.includes(` ${tok}`)) {
          score += 180;
        } else if (espNorm.includes(tok)) {
          score += 90;
        }
        if (catNorm.startsWith(tok) || catNorm.includes(` ${tok}`)) {
          score += 80;
        }
      }

      // 4. Penalizar posiciones genéricas residuales ("Los demás") frente a productos específicos
      if (pos._esLosDemas) {
        score -= 120;
      }

      resultadosPuntuados.push({ pos, score });
    }

    resultadosPuntuados.sort((a, b) => {
      if (b.score !== a.score) return b.score - a.score;
      return a.pos.descripcion.length - b.pos.descripcion.length;
    });

    return resultadosPuntuados.slice(0, limite).map((r) => r.pos);
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
