/**
 * Generador nativo de libros Excel (.xlsx OpenXML) con estilos, ancho de columnas y formato numérico.
 * - Cero dependencias externas (compatible 100% con file:/// y Next.js/Vercel).
 * - Encabezados de columnas en Verde Manzana (#8FD14F) con letra en Negrita.
 * - Columnas amplias y bordes finos para evitar texto pegado.
 */
(function (root) {
  function sanitizeFormulaText(val) {
    const str = String(val ?? '');
    if (/^[=+\-@\t\r]/.test(str) && !/^-[0-9]+(?:\.[0-9]+)?$/.test(str)) {
      return `'${str}`;
    }
    return str;
  }

  function escapeXml(str) {
    return sanitizeFormulaText(str)
      .replace(/&/g, '&amp;')
      .replace(/</g, '&lt;')
      .replace(/>/g, '&gt;')
      .replace(/"/g, '&quot;')
      .replace(/'/g, '&apos;');
  }

  function colIndexToLetter(idx) {
    let s = '';
    let n = idx + 1;
    while (n > 0) {
      const m = (n - 1) % 26;
      s = String.fromCharCode(65 + m) + s;
      n = Math.floor((n - m) / 26);
    }
    return s;
  }

  // Tabla CRC-32 estándar para empaquetado ZIP válido
  const CRC_TABLE = (() => {
    const t = new Uint32Array(256);
    for (let i = 0; i < 256; i++) {
      let c = i;
      for (let k = 0; k < 8; k++) {
        c = (c & 1) ? (0xEDB88320 ^ (c >>> 1)) : (c >>> 1);
      }
      t[i] = c >>> 0;
    }
    return t;
  })();

  function crc32(bytes) {
    let c = 0xFFFFFFFF;
    for (let i = 0; i < bytes.length; i++) {
      c = CRC_TABLE[(c ^ bytes[i]) & 0xFF] ^ (c >>> 8);
    }
    return (c ^ 0xFFFFFFFF) >>> 0;
  }

  function createZipBlob(files) {
    const encoder = new TextEncoder();
    const localParts = [];
    const centralParts = [];
    let offset = 0;

    for (const f of files) {
      const nameBytes = encoder.encode(f.name);
      const dataBytes = typeof f.content === 'string' ? encoder.encode(f.content) : f.content;
      const crc = crc32(dataBytes);
      const len = dataBytes.length;

      // Local file header (30 bytes + name)
      const lh = new Uint8Array(30 + nameBytes.length);
      const lv = new DataView(lh.buffer);
      lv.setUint32(0, 0x04034B50, true);
      lv.setUint16(4, 20, true);
      lv.setUint16(6, 0x0800, true); // UTF-8
      lv.setUint16(8, 0, true); // STORE (sin compresión)
      lv.setUint16(10, 0, true);
      lv.setUint16(12, 0, true);
      lv.setUint32(14, crc, true);
      lv.setUint32(18, len, true);
      lv.setUint32(22, len, true);
      lv.setUint16(26, nameBytes.length, true);
      lv.setUint16(28, 0, true);
      lh.set(nameBytes, 30);

      localParts.push(lh, dataBytes);

      // Central directory header (46 bytes + name)
      const ch = new Uint8Array(46 + nameBytes.length);
      const cv = new DataView(ch.buffer);
      cv.setUint32(0, 0x02014B50, true);
      cv.setUint16(4, 20, true);
      cv.setUint16(6, 20, true);
      cv.setUint16(8, 0x0800, true);
      cv.setUint16(10, 0, true);
      cv.setUint16(12, 0, true);
      cv.setUint16(14, 0, true);
      cv.setUint32(16, crc, true);
      cv.setUint32(20, len, true);
      cv.setUint32(24, len, true);
      cv.setUint16(28, nameBytes.length, true);
      cv.setUint16(30, 0, true);
      cv.setUint16(32, 0, true);
      cv.setUint16(34, 0, true);
      cv.setUint16(36, 0, true);
      cv.setUint32(38, 0, true);
      cv.setUint32(42, offset, true);
      ch.set(nameBytes, 46);

      centralParts.push(ch);
      offset += lh.length + len;
    }

    const centralSize = centralParts.reduce((s, p) => s + p.length, 0);
    const eocd = new Uint8Array(22);
    const ev = new DataView(eocd.buffer);
    ev.setUint32(0, 0x06054B50, true);
    ev.setUint16(4, 0, true);
    ev.setUint16(6, 0, true);
    ev.setUint16(8, files.length, true);
    ev.setUint16(10, files.length, true);
    ev.setUint32(12, centralSize, true);
    ev.setUint32(16, offset, true);
    ev.setUint16(20, 0, true);

    return new Blob([...localParts, ...centralParts, eocd], {
      type: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet'
    });
  }

  function buildStylesXml() {
    return `<?xml version="1.0" encoding="UTF-8" standalone="yes"?>
<styleSheet xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main">
  <numFmts count="1">
    <numFmt numFmtId="164" formatCode="#,##0.00"/>
  </numFmts>
  <fonts count="4">
    <font><sz val="11"/><color rgb="FF171717"/><name val="Calibri"/></font>
    <font><b/><sz val="11"/><color rgb="FF171717"/><name val="Calibri"/></font>
    <font><b/><sz val="12"/><color rgb="FFFFFFFF"/><name val="Calibri"/></font>
    <font><b/><sz val="11"/><color rgb="FFFFFFFF"/><name val="Calibri"/></font>
  </fonts>
  <fills count="7">
    <fill><patternFill patternType="none"/></fill>
    <fill><patternFill patternType="gray125"/></fill>
    <fill><patternFill patternType="solid"><fgColor rgb="FF420213"/></patternFill></fill>
    <fill><patternFill patternType="solid"><fgColor rgb="FF8FD14F"/></patternFill></fill>
    <fill><patternFill patternType="solid"><fgColor rgb="FFF4F6F8"/></patternFill></fill>
    <fill><patternFill patternType="solid"><fgColor rgb="FFFFF0F2"/></patternFill></fill>
    <fill><patternFill patternType="solid"><fgColor rgb="FF530618"/></patternFill></fill>
  </fills>
  <borders count="2">
    <border><left/><right/><top/><bottom/><diagonal/></border>
    <border>
      <left style="thin"><color rgb="FFD8DDE3"/></left>
      <right style="thin"><color rgb="FFD8DDE3"/></right>
      <top style="thin"><color rgb="FFD8DDE3"/></top>
      <bottom style="thin"><color rgb="FFD8DDE3"/></bottom>
      <diagonal/>
    </border>
  </borders>
  <cellStyleXfs count="1">
    <xf numFmtId="0" fontId="0" fillId="0" borderId="0"/>
  </cellStyleXfs>
  <cellXfs count="11">
    <xf numFmtId="0" fontId="0" fillId="0" borderId="0" xfId="0"/>
    <xf numFmtId="0" fontId="2" fillId="2" borderId="1" xfId="0" applyFont="1" applyFill="1" applyBorder="1" applyAlignment="1"><alignment horizontal="left" vertical="center"/></xf>
    <xf numFmtId="0" fontId="1" fillId="4" borderId="1" xfId="0" applyFont="1" applyFill="1" applyBorder="1" applyAlignment="1"><alignment horizontal="left" vertical="center" wrapText="1"/></xf>
    <xf numFmtId="0" fontId="0" fillId="0" borderId="1" xfId="0" applyFont="1" applyBorder="1" applyAlignment="1"><alignment horizontal="left" vertical="center" wrapText="1"/></xf>
    <xf numFmtId="0" fontId="1" fillId="3" borderId="1" xfId="0" applyFont="1" applyFill="1" applyBorder="1" applyAlignment="1"><alignment horizontal="center" vertical="center" wrapText="1"/></xf>
    <xf numFmtId="0" fontId="0" fillId="0" borderId="1" xfId="0" applyFont="1" applyBorder="1" applyAlignment="1"><alignment horizontal="left" vertical="center" wrapText="1"/></xf>
    <xf numFmtId="164" fontId="0" fillId="0" borderId="1" xfId="0" applyNumberFormat="1" applyFont="1" applyBorder="1" applyAlignment="1"><alignment horizontal="right" vertical="center"/></xf>
    <xf numFmtId="0" fontId="1" fillId="5" borderId="1" xfId="0" applyFont="1" applyFill="1" applyBorder="1" applyAlignment="1"><alignment horizontal="left" vertical="center" wrapText="1"/></xf>
    <xf numFmtId="164" fontId="1" fillId="5" borderId="1" xfId="0" applyNumberFormat="1" applyFont="1" applyFill="1" applyBorder="1" applyAlignment="1"><alignment horizontal="right" vertical="center"/></xf>
    <xf numFmtId="0" fontId="3" fillId="6" borderId="1" xfId="0" applyFont="1" applyFill="1" applyBorder="1" applyAlignment="1"><alignment horizontal="left" vertical="center" wrapText="1"/></xf>
    <xf numFmtId="164" fontId="3" fillId="6" borderId="1" xfId="0" applyNumberFormat="1" applyFont="1" applyFill="1" applyBorder="1" applyAlignment="1"><alignment horizontal="right" vertical="center"/></xf>
  </cellXfs>
  <cellStyles count="1">
    <cellStyle name="Normal" xfId="0" builtinId="0"/>
  </cellStyles>
</styleSheet>`;
  }

  function buildWorksheetXml(rows) {
    const colWidths = [58, 30, 24];
    const colsXml = colWidths
      .map((w, idx) => `<col min="${idx + 1}" max="${idx + 1}" width="${w}" customWidth="1"/>`)
      .join('');

    const rowsXml = rows.map((row, rIdx) => {
      const rNum = rIdx + 1;
      const height = row.height || 24;
      const cellsXml = row.cells.map((cell, cIdx) => {
        const ref = `${colIndexToLetter(cIdx)}${rNum}`;
        const styleAttr = cell.style !== undefined ? ` s="${cell.style}"` : '';
        if (typeof cell.value === 'number' && !Number.isNaN(cell.value)) {
          return `<c r="${ref}"${styleAttr}><v>${cell.value}</v></c>`;
        }
        return `<c r="${ref}"${styleAttr} t="inlineStr"><is><t>${escapeXml(cell.value ?? '')}</t></is></c>`;
      }).join('');
      return `<row r="${rNum}" ht="${height}" customHeight="1">${cellsXml}</row>`;
    }).join('');

    return `<?xml version="1.0" encoding="UTF-8" standalone="yes"?>
<worksheet xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main">
  <sheetViews><sheetView workbookViewId="0"/></sheetViews>
  <sheetFormatPr defaultRowHeight="24"/>
  <cols>${colsXml}</cols>
  <sheetData>${rowsXml}</sheetData>
  <mergeCells count="1"><mergeCell ref="A1:C1"/></mergeCells>
</worksheet>`;
  }

  function exportarComprobanteXlsx(data) {
    if (!data || typeof window === 'undefined') return;

    const idCotizacion = data.idCotizacion || 'IMP-2026';
    const descripcion = data.descripcion || 'Mercadería';
    const pais = data.pais || 'Extrazona';
    const regimen = data.regimen || 'Régimen General Aduanero';
    const cantidad = Number(data.cantidad || 1);
    const fobUnit = Number(data.fobUnit || 0);
    const fobTotal = Number(data.fobTotal || 0);
    const flete = Number(data.flete || 0);
    const seguro = Number(data.seguro || 0);
    const cif = Number(data.cif || 0);
    const desglose = Array.isArray(data.desglose) ? data.desglose : [];
    const totalGrav = Number(data.totalGrav || 0);
    const costoNac = Number(data.costoNac || 0);
    const costoUnit = Number(data.costoUnit || 0);
    const tcBna = Number(data.tcBna || 1420.5);
    const costoArs = Number(data.costoArs || 0);
    const incidenciaPct = data.incidenciaPct ?? 0;

    const rows = [
      {
        height: 32,
        cells: [
          { value: 'IMPORTA+  |  COMPROBANTE OFICIAL DE SIMULACIÓN ADUANERA (ARCA / DGA)', style: 1 },
          { value: '', style: 1 },
          { value: '', style: 1 }
        ]
      },
      { height: 22, cells: [{ value: 'ID Comprobante', style: 2 }, { value: idCotizacion, style: 3 }, { value: '', style: 3 }] },
      { height: 22, cells: [{ value: 'Fecha de Emisión', style: 2 }, { value: new Date(data.fechaEmision || Date.now()).toLocaleString('es-AR'), style: 3 }, { value: '', style: 3 }] },
      { height: 36, cells: [{ value: 'Descripción del Producto', style: 2 }, { value: descripcion, style: 3 }, { value: '', style: 3 }] },
      { height: 22, cells: [{ value: 'País de Origen', style: 2 }, { value: pais, style: 3 }, { value: '', style: 3 }] },
      { height: 22, cells: [{ value: 'Régimen Aplicado', style: 2 }, { value: regimen, style: 3 }, { value: '', style: 3 }] },
      { height: 22, cells: [{ value: 'Cantidad de Unidades', style: 2 }, { value: cantidad, style: 3 }, { value: '', style: 3 }] },
      { height: 14, cells: [{ value: '', style: 0 }, { value: '', style: 0 }, { value: '', style: 0 }] },
      // Encabezados de columnas en VERDE MANZANA (#8FD14F) y NEGRITA
      {
        height: 28,
        cells: [
          { value: 'CONCEPTO / PARTIDA ADUANERA', style: 4 },
          { value: 'ALÍCUOTA / DETALLE', style: 4 },
          { value: 'IMPORTE (USD)', style: 4 }
        ]
      },
      { height: 24, cells: [{ value: 'Valor FOB Unitario', style: 5 }, { value: 'Precio unitario origen', style: 5 }, { value: fobUnit, style: 6 }] },
      { height: 24, cells: [{ value: 'Valor FOB Total Mercadería', style: 5 }, { value: `${cantidad} unidades`, style: 5 }, { value: fobTotal, style: 6 }] },
      { height: 24, cells: [{ value: 'Flete Internacional', style: 5 }, { value: 'Transporte internacional', style: 5 }, { value: flete, style: 6 }] },
      { height: 24, cells: [{ value: 'Seguro Internacional', style: 5 }, { value: 'Póliza de cobertura', style: 5 }, { value: seguro, style: 6 }] },
      { height: 26, cells: [{ value: 'VALOR BASE CIF ADUANERO', style: 7 }, { value: 'Base Imponible DGA', style: 7 }, { value: cif, style: 8 }] }
    ];

    for (const item of desglose) {
      const esSub = Boolean(item.esSubtotal);
      rows.push({
        height: 24,
        cells: [
          { value: item.concepto || '', style: esSub ? 7 : 5 },
          { value: item.alicuota || '-', style: esSub ? 7 : 5 },
          { value: Number(item.montoUSD || 0), style: esSub ? 8 : 6 }
        ]
      });
    }

    rows.push({ height: 14, cells: [{ value: '', style: 0 }, { value: '', style: 0 }, { value: '', style: 0 }] });
    rows.push({ height: 26, cells: [{ value: 'TOTAL GRAVÁMENES ADUANEROS', style: 7 }, { value: `+${incidenciaPct}% sobre FOB`, style: 7 }, { value: totalGrav, style: 8 }] });
    rows.push({ height: 28, cells: [{ value: 'COSTO TOTAL NACIONALIZADO (USD)', style: 9 }, { value: 'Puesto en Plaza', style: 9 }, { value: costoNac, style: 10 }] });
    rows.push({ height: 24, cells: [{ value: 'COSTO UNITARIO FINAL (USD)', style: 5 }, { value: 'Por Unidad', style: 5 }, { value: costoUnit, style: 6 }] });
    rows.push({ height: 24, cells: [{ value: 'Tipo de Cambio Oficial BNA', style: 5 }, { value: 'ARS / USD', style: 5 }, { value: tcBna, style: 6 }] });
    rows.push({ height: 26, cells: [{ value: 'COSTO TOTAL ESTIMADO (ARS)', style: 7 }, { value: 'Pesos Argentinos', style: 7 }, { value: costoArs, style: 8 }] });

    const contentTypes = `<?xml version="1.0" encoding="UTF-8" standalone="yes"?>
<Types xmlns="http://schemas.openxmlformats.org/package/2006/content-types">
  <Default Extension="rels" ContentType="application/vnd.openxmlformats-package.relationships+xml"/>
  <Default Extension="xml" ContentType="application/xml"/>
  <Override PartName="/xl/workbook.xml" ContentType="application/vnd.openxmlformats-officedocument.spreadsheetml.sheet.main+xml"/>
  <Override PartName="/xl/worksheets/sheet1.xml" ContentType="application/vnd.openxmlformats-officedocument.spreadsheetml.worksheet+xml"/>
  <Override PartName="/xl/styles.xml" ContentType="application/vnd.openxmlformats-officedocument.spreadsheetml.styles+xml"/>
</Types>`;

    const rels = `<?xml version="1.0" encoding="UTF-8" standalone="yes"?>
<Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships">
  <Relationship Id="rId1" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument" Target="xl/workbook.xml"/>
</Relationships>`;

    const workbook = `<?xml version="1.0" encoding="UTF-8" standalone="yes"?>
<workbook xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main" xmlns:r="http://schemas.openxmlformats.org/officeDocument/2006/relationships">
  <sheets><sheet name="Liquidación IMPORTA+" sheetId="1" r:id="rId1"/></sheets>
</workbook>`;

    const workbookRels = `<?xml version="1.0" encoding="UTF-8" standalone="yes"?>
<Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships">
  <Relationship Id="rId1" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/worksheet" Target="worksheets/sheet1.xml"/>
  <Relationship Id="rId2" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/styles" Target="styles.xml"/>
</Relationships>`;

    const blob = createZipBlob([
      { name: '[Content_Types].xml', content: contentTypes },
      { name: '_rels/.rels', content: rels },
      { name: 'xl/workbook.xml', content: workbook },
      { name: 'xl/_rels/workbook.xml.rels', content: workbookRels },
      { name: 'xl/styles.xml', content: buildStylesXml() },
      { name: 'xl/worksheets/sheet1.xml', content: buildWorksheetXml(rows) }
    ]);

    const safeDownloadId = String(idCotizacion || 'IMP-2026').replace(/[^a-zA-Z0-9_-]/g, '');
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = `Comprobante_ImportaPlus_${safeDownloadId || 'IMP'}.xlsx`;
    document.body.appendChild(a);
    a.click();
    document.body.removeChild(a);
    URL.revokeObjectURL(url);
  }

  root.ExcelExportHelper = { exportarComprobanteXlsx };
  if (typeof module !== 'undefined' && module.exports) {
    module.exports = { exportarComprobanteXlsx };
  }
})(typeof window !== 'undefined' ? window : globalThis);
