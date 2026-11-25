/**
 * Capa DAL (Data Access Layer)
 * Acceso y Persistencia en Excel: ExcelDataLoaderDAL
 *
 * Centraliza la lectura de las hojas del libro maestro de Excel
 * (DAL/database/ImportaPlus_ExcelDB.xlsx, Cliente/Nomenclatura_NCM_aranceles.xlsx
 * y planillas CSV asociadas) así como la persistencia y exportación directa de
 * comprobantes y solicitudes de contacto en formato Excel (.csv UTF-8 BOM con ';' / .xlsx).
 */
class ExcelDataLoaderDAL {
  static _cacheDatos = null;

  static async obtenerBaseDatosExcel() {
    if (this._cacheDatos) {
      return this._cacheDatos;
    }

    // 1. Si se encuentra pre-cargado el bundle sincronizado desde Excel (modo file:/// o HTTP)
    if (typeof window !== 'undefined' && window.__EXCEL_DATABASE__) {
      this._cacheDatos = window.__EXCEL_DATABASE__;
      return this._cacheDatos;
    }

    // 2. Intentar lectura por HTTP fetch de las planillas CSV de DAL/database/
    if (typeof window !== 'undefined' && window.location.protocol.startsWith('http')) {
      try {
        const [ncmRes, paisesRes] = await Promise.all([
          fetch('DAL/database/ncm_catalogo.csv'),
          fetch('DAL/database/paises.csv')
        ]);
        if (ncmRes.ok && paisesRes.ok) {
          const ncmText = await ncmRes.text();
          const paisesText = await paisesRes.text();
          this._cacheDatos = {
            posicionesDestacadas: this.parsearCsvTexto(ncmText),
            paises: this.parsearCsvTexto(paisesText)
          };
          return this._cacheDatos;
        }
      } catch (e) {
        console.warn('ExcelDataLoaderDAL: Usando fallback local.', e);
      }
    }

    return null;
  }

  static _dividirLineaCsv(line, delimiter = ';') {
    const result = [];
    let current = '';
    let inQuotes = false;
    for (let i = 0; i < line.length; i++) {
      const ch = line[i];
      if (ch === '"') {
        if (inQuotes && line[i + 1] === '"') {
          current += '"';
          i++;
        } else {
          inQuotes = !inQuotes;
        }
      } else if (ch === delimiter && !inQuotes) {
        result.push(current);
        current = '';
      } else {
        current += ch;
      }
    }
    result.push(current);
    return result;
  }

  static parsearCsvTexto(csvText) {
    const clean = (csvText || '').replace(/^\uFEFF/, '');
    const lines = clean.split(/\r?\n/).filter((l) => l.trim().length > 0);
    if (lines.length < 2) return [];
    const delimiter = lines[0].includes(';') ? ';' : ',';
    const headers = this._dividirLineaCsv(lines[0], delimiter).map((h) => h.trim());
    const rows = [];
    for (let i = 1; i < lines.length; i++) {
      const cols = this._dividirLineaCsv(lines[i], delimiter);
      const obj = {};
      headers.forEach((h, idx) => {
        obj[h] = (cols[idx] || '').trim();
      });
      rows.push(obj);
    }
    return rows;
  }

  /**
   * Registra una simulación en el almacén persistente de Excel del cliente
   */
  static registrarSimulacionEnExcel(be) {
    if (!be || typeof window === 'undefined') return false;
    try {
      const key = 'IMPORTA_PLUS_EXCEL_SIMULACIONES';
      const prev = JSON.parse(localStorage.getItem(key) || '[]');
      prev.push({
        id_cotizacion: be.hashCotizacion || be.idCotizacion,
        fecha_emision: (be.fechaHora instanceof Date ? be.fechaHora : new Date()).toISOString(),
        codigo_ncm: be.posicion?.codigoNcm || be.fichaProducto?.codigoNcm || '',
        descripcion: be.posicion?.descripcion || be.fichaProducto?.descripcion || '',
        pais_origen: be.pais?.nombre || be.fichaProducto?.paisOrigen || '',
        cantidad: be.cantidad || be.parametrosComerciales?.cantidad || 1,
        fob_unitario_usd: be.precioFobUnitario || be.parametrosComerciales?.valorFobUnitario || 0,
        fob_total_usd: be.fobTotal || be.parametrosComerciales?.valorFobTotal || 0,
        flete_usd: be.flete ?? be.parametrosComerciales?.flete ?? 0,
        seguro_usd: be.seguro ?? be.parametrosComerciales?.seguro ?? 0,
        cif_aduanero_usd: be.cifAduanero || be.parametrosComerciales?.valorCIF || 0,
        total_gravamenes_usd: be.totalGravamenes || be.resumenEconomico?.totalGravamenesUSD || 0,
        costo_total_nacionalizado_usd: be.costoTotalNacionalizado || be.resumenEconomico?.costoTotalNacionalizadoUSD || 0,
        costo_unitario_final_usd: be.costoUnitarioNacionalizado || be.resumenEconomico?.costoUnitarioFinalUSD || 0,
        cotizacion_bna_ars: be.cotizacionBna || be.resumenEconomico?.tipoCambioBNA || 1420.5,
        costo_total_ars: be.costoTotalARS || be.resumenEconomico?.costoTotalEstimadoARS || 0
      });
      localStorage.setItem(key, JSON.stringify(prev.slice(-200)));
      return true;
    } catch (e) {
      return false;
    }
  }

  /**
   * Registra una solicitud de contacto a despachante/forwarder en el almacén Excel
   */
  static registrarSolicitudLeadEnExcel(lead) {
    if (!lead || typeof window === 'undefined') return false;
    try {
      const key = 'IMPORTA_PLUS_EXCEL_LEADS';
      const prev = JSON.parse(localStorage.getItem(key) || '[]');
      prev.push({
        id_lead: `EXCEL-LEAD-${Date.now()}`,
        fecha_registro: new Date().toISOString(),
        id_cotizacion: lead.idCotizacion || '',
        nombre: lead.nombre || '',
        email: lead.email || '',
        telefono: lead.telefono || '',
        consulta: lead.consulta || ''
      });
      localStorage.setItem(key, JSON.stringify(prev.slice(-200)));
      return true;
    } catch (e) {
      return false;
    }
  }

  /**
   * Exporta y descarga directamente una simulación como planilla de Microsoft Excel (.csv con BOM UTF-8 y ';')
   * Cumple con PN2 - RF01 ("Exportar Planilla Excel (XLSX / CSV)")
   */
  static exportarComprobanteExcel(be) {
    if (!be || typeof window === 'undefined') return;

    const idCotizacion = be.hashCotizacion || be.idCotizacion || 'IMP-2026';
    const descripcion = be.posicion?.descripcion || be.fichaProducto?.descripcion || 'Mercadería';
    const pais = be.pais?.nombre || be.fichaProducto?.paisOrigen || 'Extrazona';
    const cantidad = be.cantidad || be.parametrosComerciales?.cantidad || 1;
    const fobUnit = be.precioFobUnitario || be.parametrosComerciales?.valorFobUnitario || 0;
    const fobTotal = be.fobTotal || be.parametrosComerciales?.valorFobTotal || 0;
    const flete = be.flete ?? be.parametrosComerciales?.flete ?? 0;
    const seguro = be.seguro ?? be.parametrosComerciales?.seguro ?? 0;
    const cif = be.cifAduanero || be.parametrosComerciales?.valorCIF || 0;
    const totalGrav = be.totalGravamenes || be.resumenEconomico?.totalGravamenesUSD || 0;
    const costoNac = be.costoTotalNacionalizado || be.resumenEconomico?.costoTotalNacionalizadoUSD || 0;
    const costoUnit = be.costoUnitarioNacionalizado || be.resumenEconomico?.costoUnitarioFinalUSD || 0;
    const tcBna = be.cotizacionBna || be.resumenEconomico?.tipoCambioBNA || 1420.5;
    const costoArs = be.costoTotalARS || be.resumenEconomico?.costoTotalEstimadoARS || 0;

    if (typeof window !== 'undefined' && window.ExcelExportHelper) {
      const desglose = Array.isArray(be.desgloseTributario) && be.desgloseTributario.length > 0
        ? be.desgloseTributario
        : [
            { concepto: 'Derecho de Importación (DIE)', alicuota: `${be.alicuotaDie || 0}%`, montoUSD: Number(be.montoDie || 0) },
            { concepto: 'Tasa de Estadística (Dec. 99/2019)', alicuota: `${be.alicuotaEstadistica || 0}%`, montoUSD: Number(be.montoEstadistica || 0) },
            { concepto: 'Base Imponible Interior', alicuota: 'CIF + DIE + Estadística', montoUSD: Number(be.baseImponibleInterior || 0), esSubtotal: true },
            { concepto: 'IVA Aduanero DGA', alicuota: `${be.alicuotaIva || 21}%`, montoUSD: Number(be.montoIva || 0) },
            { concepto: 'Percepción IVA Adicional (RG 2281)', alicuota: `${be.alicuotaIvaAdicional || 20}%`, montoUSD: Number(be.montoIvaAdicional || 0) },
            { concepto: 'Percepción Imp. Ganancias (RG 2281)', alicuota: `${be.alicuotaGanancias || 6}%`, montoUSD: Number(be.montoGanancias || 0) },
            { concepto: 'Arancel SIM + Digitalización', alicuota: 'Cargo Fijo Oficial', montoUSD: Number(be.totalSimDigitalizacion || 38) }
          ];

      window.ExcelExportHelper.exportarComprobanteXlsx({
        idCotizacion,
        fechaEmision: be.fechaHora,
        descripcion,
        pais,
        regimen: be.tratamientoArancelario || be.fichaProducto?.regimenAplicado || 'Régimen General Aduanero',
        cantidad,
        fobUnit,
        fobTotal,
        flete,
        seguro,
        cif,
        desglose,
        incidenciaPct: be.ratioSobrecostoPct ?? 0,
        totalGrav: totalGrav,
        costoNac,
        costoUnit,
        tcBna,
        costoArs
      });
      return;
    }

    const filas = [
      ['IMPORTA+ | COMPROBANTE OFICIAL DE SIMULACIÓN ADUANERA (ARCA / DGA)', '', ''],
      ['ID Comprobante', idCotizacion, ''],
      ['Fecha de Emisión', new Date().toLocaleString('es-AR'), ''],
      ['Descripción Oficial', `"${String(descripcion).replace(/"/g, '""')}"`, ''],
      ['País de Origen', pais, ''],
      ['Cantidad de Unidades', cantidad, ''],
      ['', '', ''],
      ['CONCEPTO / PARTIDA ADUANERA', 'ALÍCUOTA / DETALLE', 'IMPORTE (USD)'],
      ['Valor FOB Unitario', 'Precio unitario origen', fobUnit.toFixed(2)],
      ['Valor FOB Total Mercadería', `${cantidad} unidades`, fobTotal.toFixed(2)],
      ['Flete Internacional', 'Transporte internacional', flete.toFixed(2)],
      ['Seguro Internacional', 'Cobertura internacional', seguro.toFixed(2)],
      ['VALOR BASE CIF ADUANERO', 'Base Imponible DGA', cif.toFixed(2)]
    ];

    if (Array.isArray(be.desgloseTributario) && be.desgloseTributario.length > 0) {
      be.desgloseTributario.forEach((item) => {
        filas.push([
          `"${String(item.concepto || '').replace(/"/g, '""')}"`,
          item.alicuota || '-',
          Number(item.montoUSD || 0).toFixed(2)
        ]);
      });
    } else {
      filas.push(['Derecho de Importación (DIE)', `${be.alicuotaDie || 0}%`, Number(be.montoDie || 0).toFixed(2)]);
      filas.push(['Tasa de Estadística (Dec. 99/2019)', `${be.alicuotaEstadistica || 0}%`, Number(be.montoEstadistica || 0).toFixed(2)]);
      filas.push(['Base Imponible Interior', 'CIF + DIE + Estadística', Number(be.baseImponibleInterior || 0).toFixed(2)]);
      filas.push(['IVA Aduanero DGA', `${be.alicuotaIva || 21}%`, Number(be.montoIva || 0).toFixed(2)]);
      filas.push(['Percepción IVA Adicional (RG 2281)', `${be.alicuotaIvaAdicional || 20}%`, Number(be.montoIvaAdicional || 0).toFixed(2)]);
      filas.push(['Percepción Imp. Ganancias (RG 2281)', `${be.alicuotaGanancias || 6}%`, Number(be.montoGanancias || 0).toFixed(2)]);
      filas.push(['Arancel SIM + Digitalización', 'Cargo Fijo Oficial', Number(be.totalSimDigitalizacion || 38).toFixed(2)]);
    }

    filas.push(['', '', '']);
    filas.push(['TOTAL IMPUESTOS Y TASAS ADUANERAS', 'Gravámenes DGA', totalGrav.toFixed(2)]);
    filas.push(['COSTO TOTAL NACIONALIZADO (USD)', 'Puesto en Plaza', costoNac.toFixed(2)]);
    filas.push(['COSTO UNITARIO NACIONALIZADO (USD)', 'Por Unidad', costoUnit.toFixed(2)]);
    filas.push(['Tipo de Cambio Oficial BNA (ARS/USD)', 'Referencia BNA', tcBna.toFixed(2)]);
    filas.push(['COSTO TOTAL ESTIMADO (ARS)', 'Pesos Argentinos', costoArs.toFixed(2)]);

    const csvContent = '\uFEFF' + filas.map((f) => f.join(';')).join('\r\n');
    const blob = new Blob([csvContent], { type: 'text/csv;charset=utf-8;' });
    const url = URL.createObjectURL(blob);
    const link = document.createElement('a');
    link.href = url;
    link.setAttribute('download', `Comprobante_ImportaPlus_${idCotizacion}.csv`);
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
    URL.revokeObjectURL(url);
  }
}

if (typeof window !== 'undefined') {
  window.ExcelDataLoaderDAL = ExcelDataLoaderDAL;
}
