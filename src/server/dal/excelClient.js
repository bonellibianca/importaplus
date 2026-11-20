import 'server-only';
import fs from 'fs';
import path from 'path';
import zlib from 'zlib';
import {
  FALLBACK_NCM,
  FALLBACK_PAISES,
  FALLBACK_PUERTOS,
  FALLBACK_OPERADORES
} from './seedData';

/**
 * Motor Nativo de Persistencia en Excel (.xlsx OpenXML & .csv) - Server-Side
 * Reemplaza por completo el uso de bases de datos SQL (Supabase / PostgreSQL),
 * operando directamente sobre los libros de Excel del proyecto:
 *  - DAL/database/ImportaPlus_ExcelDB.xlsx
 *  - Cliente/Nomenclatura_NCM_aranceles.xlsx (10.437 posiciones oficiales)
 *  - Cliente/IMPORTA_PLUS_NOMENCLADOR_COMPLETO_22092026.csv
 *  - DAL/database/*.csv (planillas vinculadas para lectura/escritura instantánea en Excel)
 */

const ROOT_DIR = process.cwd();
const EXCEL_DB_PATH = path.join(ROOT_DIR, process.env.EXCEL_DB_PATH || 'DAL/database/ImportaPlus_ExcelDB.xlsx');
const EXCEL_NCM_OFICIAL_PATH = path.join(ROOT_DIR, process.env.EXCEL_NCM_PATH || 'DAL/database/Nomenclatura_NCM_aranceles.xlsx');
const CSV_CURATED_NCM_PATH = path.join(ROOT_DIR, 'DAL/database/ncm_catalogo.csv');
const DB_FOLDER = path.join(ROOT_DIR, 'DAL/database');

let cachedCatalogoNcm = null;
let cachedPaises = null;
let cachedPuertos = null;
let cachedOperadores = null;
let lastNcmLoadTime = 0;

/**
 * Lector ligero de archivos ZIP (.xlsx OpenXML) en Node.js puro usando zlib
 * No requiere binarios externos y funciona incluso sin node_modules instalados.
 */
function extractZipEntries(buffer) {
  const entries = new Map();
  // Buscar End of Central Directory (EOCD) signature 0x06054b50
  let eocdOffset = -1;
  for (let i = buffer.length - 22; i >= Math.max(0, buffer.length - 65557); i--) {
    if (buffer.readUInt32LE(i) === 0x06054b50) {
      eocdOffset = i;
      break;
    }
  }
  if (eocdOffset === -1) return entries;

  const totalEntries = buffer.readUInt16LE(eocdOffset + 10);
  let cdOffset = buffer.readUInt32LE(eocdOffset + 16);

  for (let i = 0; i < totalEntries; i++) {
    if (cdOffset + 46 > buffer.length || buffer.readUInt32LE(cdOffset) !== 0x02014b50) break;
    const compressionMethod = buffer.readUInt16LE(cdOffset + 10);
    const compressedSize = buffer.readUInt32LE(cdOffset + 20);
    const fileNameLength = buffer.readUInt16LE(cdOffset + 28);
    const extraLength = buffer.readUInt16LE(cdOffset + 30);
    const commentLength = buffer.readUInt16LE(cdOffset + 32);
    const localHeaderOffset = buffer.readUInt32LE(cdOffset + 42);

    const fileName = buffer.toString('utf8', cdOffset + 46, cdOffset + 46 + fileNameLength);
    cdOffset += 46 + fileNameLength + extraLength + commentLength;

    if (localHeaderOffset + 30 > buffer.length || buffer.readUInt32LE(localHeaderOffset) !== 0x04034b50) continue;
    const localNameLen = buffer.readUInt16LE(localHeaderOffset + 26);
    const localExtraLen = buffer.readUInt16LE(localHeaderOffset + 28);
    const dataStart = localHeaderOffset + 30 + localNameLen + localExtraLen;
    const compressedData = buffer.subarray(dataStart, dataStart + compressedSize);

    try {
      if (compressionMethod === 0) {
        entries.set(fileName, compressedData.toString('utf8'));
      } else if (compressionMethod === 8) {
        entries.set(fileName, zlib.inflateRawSync(compressedData).toString('utf8'));
      }
    } catch (e) {
      // Ignorar entrada corrupta
    }
  }
  return entries;
}

function decodeXmlEntities(str) {
  if (!str) return '';
  return str
    .replace(/&lt;/g, '<')
    .replace(/&gt;/g, '>')
    .replace(/&quot;/g, '"')
    .replace(/&apos;/g, "'")
    .replace(/&amp;/g, '&');
}

function parseSharedStrings(xml) {
  if (!xml) return [];
  const strings = [];
  const siRegex = /<si[^>]*>([\s\S]*?)<\/si>/g;
  let siMatch;
  while ((siMatch = siRegex.exec(xml)) !== null) {
    const inner = siMatch[1];
    const tRegex = /<t[^>]*>([\s\S]*?)<\/t>/g;
    let tMatch;
    let combined = '';
    while ((tMatch = tRegex.exec(inner)) !== null) {
      combined += decodeXmlEntities(tMatch[1]);
    }
    strings.push(combined);
  }
  return strings;
}

function colLettersToIndex(letters) {
  let idx = 0;
  for (let i = 0; i < letters.length; i++) {
    idx = idx * 26 + (letters.charCodeAt(i) - 64);
  }
  return idx - 1;
}

function parseWorksheetXml(sheetXml, sharedStrings = []) {
  if (!sheetXml) return [];
  const rows = [];
  const rowRegex = /<row[^>]*>([\s\S]*?)<\/row>/g;
  let rowMatch;

  while ((rowMatch = rowRegex.exec(sheetXml)) !== null) {
    const rowContent = rowMatch[1];
    const rowValues = [];
    const cellRegex = /<c\s+([^>]*?)(?:\/>|>([\s\S]*?)<\/c>)/g;
    let cellMatch;
    let nextColIdx = 0;

    while ((cellMatch = cellRegex.exec(rowContent)) !== null) {
      const attrs = cellMatch[1];
      const cellInner = cellMatch[2] || '';

      const rMatch = /r="([A-Z]+)\d+"/.exec(attrs);
      const tMatch = /t="([^"]+)"/.exec(attrs);
      const colIdx = rMatch ? colLettersToIndex(rMatch[1]) : nextColIdx;
      nextColIdx = colIdx + 1;

      while (rowValues.length <= colIdx) {
        rowValues.push('');
      }

      const cellType = tMatch ? tMatch[1] : '';
      let value = '';

      if (cellType === 'inlineStr') {
        const tVal = /<t[^>]*>([\s\S]*?)<\/t>/.exec(cellInner);
        value = tVal ? decodeXmlEntities(tVal[1]) : '';
      } else {
        const vVal = /<v[^>]*>([\s\S]*?)<\/v>/.exec(cellInner);
        const rawV = vVal ? decodeXmlEntities(vVal[1]) : '';
        if (cellType === 's' && rawV !== '') {
          const sIdx = parseInt(rawV, 10);
          value = sharedStrings[sIdx] !== undefined ? sharedStrings[sIdx] : rawV;
        } else {
          value = rawV;
        }
      }

      rowValues[colIdx] = value;
    }

    if (rowValues.length > 0) {
      rows.push(rowValues);
    }
  }

  return rows;
}

/**
 * Parsea un archivo CSV compatible con Excel (soporta delimitador ';' o ',')
 */
function parseCsvFile(filePath) {
  if (!fs.existsSync(filePath)) return [];
  const raw = fs.readFileSync(filePath, 'utf8').replace(/^\uFEFF/, '');
  const lines = raw.split(/\r?\n/).filter((l) => l.trim().length > 0);
  if (lines.length < 2) return [];

  const delimiter = lines[0].includes(';') ? ';' : ',';
  const headers = splitCsvLine(lines[0], delimiter).map((h) => h.trim());
  const records = [];

  for (let i = 1; i < lines.length; i++) {
    const cols = splitCsvLine(lines[i], delimiter);
    const obj = {};
    headers.forEach((h, idx) => {
      obj[h] = cols[idx] !== undefined ? cols[idx].trim() : '';
    });
    records.push(obj);
  }
  return records;
}

function splitCsvLine(line, delimiter = ';') {
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

function formatCsvCell(val) {
  if (val === null || val === undefined) return '';
  const str = String(val).replace(/\r?\n/g, ' ');
  if (str.includes(';') || str.includes('"')) {
    return `"${str.replace(/"/g, '""')}"`;
  }
  return str;
}

function appendCsvRow(filePath, headers, rowObject) {
  try {
    fs.mkdirSync(path.dirname(filePath), { recursive: true });
    const exists = fs.existsSync(filePath);
    let content = '';
    if (!exists) {
      content += '\uFEFF' + headers.join(';') + '\r\n';
    }
    const line = headers.map((h) => formatCsvCell(rowObject[h])).join(';') + '\r\n';
    content += line;
    fs.appendFileSync(filePath, content, 'utf8');
    syncExcelWorkbookWithSheet(filePath, headers);
    return true;
  } catch (err) {
    console.warn('[ExcelClient_CSV_WRITE_WARNING]:', err.message);
    return false;
  }
}

/**
 * Si la librería 'xlsx' (SheetJS) está instalada en node_modules, sincroniza también
 * la hoja correspondiente dentro del libro binario ImportaPlus_ExcelDB.xlsx.
 */
function syncExcelWorkbookWithSheet(csvFilePath, headers) {
  try {
    // Carga dinámica opcional de SheetJS cuando node_modules está instalado
    const req = eval('require');
    const XLSX = req('xlsx');
    if (!XLSX || !fs.existsSync(EXCEL_DB_PATH)) return;

    const sheetName = csvFilePath.endsWith('simulaciones.csv')
      ? 'Simulaciones'
      : csvFilePath.endsWith('solicitudes_contacto.csv')
      ? 'Solicitudes_Contacto'
      : null;
    if (!sheetName) return;

    const rows = parseCsvFile(csvFilePath);
    const aoa = [headers, ...rows.map((r) => headers.map((h) => r[h] ?? ''))];
    const wb = XLSX.readFile(EXCEL_DB_PATH);
    const ws = XLSX.utils.aoa_to_sheet(aoa);
    wb.Sheets[sheetName] = ws;
    if (!wb.SheetNames.includes(sheetName)) {
      wb.SheetNames.push(sheetName);
    }
    XLSX.writeFile(wb, EXCEL_DB_PATH);
  } catch (e) {
    // En entornos sin node_modules instalados aún, la persistencia queda asegurada en la planilla CSV de Excel y se sincroniza con seed_excel.py
  }
}

/**
 * Carga y unifica el catálogo NCM desde los archivos Excel oficiales:
 * 1. Cliente/IMPORTA_PLUS_NOMENCLADOR_COMPLETO_22092026.csv (y DAL/database/ncm_catalogo.csv)
 * 2. Cliente/Nomenclatura_NCM_aranceles.xlsx (10.437 posiciones oficiales Decreto 557/2023)
 */
export function cargarCatalogoNcmExcel() {
  const now = Date.now();
  if (cachedCatalogoNcm && now - lastNcmLoadTime < 60 * 1000) {
    return cachedCatalogoNcm;
  }

  const mapByCode = new Map();

  // 1. Cargar posiciones curadas desde CSV / Excel DB
  const csvPath = fs.existsSync(path.join(DB_FOLDER, 'ncm_catalogo.csv'))
    ? path.join(DB_FOLDER, 'ncm_catalogo.csv')
    : CSV_CURATED_NCM_PATH;

  if (fs.existsSync(csvPath)) {
    const rows = parseCsvFile(csvPath);
    for (const r of rows) {
      const code = (r.codigo_ncm || r.ncm || '').trim();
      if (!code) continue;
      let tipoBien = (r.tipo_bien || 'GENERAL').toUpperCase();
      if (!r.tipo_bien) {
        const isBk = ['true', '1', 'si'].includes(String(r.bk || '').toLowerCase());
        const isBit = ['true', '1', 'si'].includes(String(r.bit || '').toLowerCase());
        tipoBien = isBk ? 'BK' : isBit ? 'BIT' : 'GENERAL';
      }
      const aec = parseFloat(r.aec) || 0;
      const anexo = r.anexo_excepcion || null;
      const arancelExRaw = r.arancel_excepcion !== undefined ? r.arancel_excepcion : r.arancel_aplicable;
      const arancelExNum = arancelExRaw !== '' && arancelExRaw !== null && arancelExRaw !== undefined
        ? parseFloat(arancelExRaw)
        : null;

      mapByCode.set(code, {
        codigo_ncm: code,
        descripcion: r.descripcion || '',
        categoria: r.categoria || 'Mercadería General',
        tipo_bien: tipoBien,
        aec,
        iva_alicuota: tipoBien === 'BK' || tipoBien === 'BIT' ? 10.5 : 21.0,
        anexo_excepcion: anexo || null,
        arancel_excepcion: anexo && arancelExNum !== null ? arancelExNum : null,
        condicion_excepcion: r.condicion_excepcion || r.condicion || null,
        esDestacado: true
      });
    }
  }

  // 2. Cargar las 10.437 posiciones oficiales desde Cliente/Nomenclatura_NCM_aranceles.xlsx
  if (fs.existsSync(EXCEL_NCM_OFICIAL_PATH)) {
    try {
      const buf = fs.readFileSync(EXCEL_NCM_OFICIAL_PATH);
      const entries = extractZipEntries(buf);
      const sharedStrings = parseSharedStrings(entries.get('xl/sharedStrings.xml'));
      const sheetRows = parseWorksheetXml(entries.get('xl/worksheets/sheet2.xml'), sharedStrings);

      for (let i = 1; i < sheetRows.length; i++) {
        const row = sheetRows[i];
        const code = (row[0] || '').trim();
        if (!code || mapByCode.has(code)) continue;

        const descCorta = (row[1] || '').trim();
        const descCompleta = (row[2] || '').trim();
        const descripcion = descCompleta || descCorta;
        const aec = parseFloat(row[3]) || 0;
        const tipoRaw = (row[4] || '').trim().toUpperCase();
        const tipoBien = tipoRaw === 'BK' || tipoRaw === 'BIT' ? tipoRaw : 'GENERAL';
        const anexoRaw = (row[5] || '').trim();
        const arancelAplicable = row[6] !== undefined && row[6] !== '' ? parseFloat(row[6]) : null;
        const condicion = (row[7] || '').trim() || null;

        mapByCode.set(code, {
          codigo_ncm: code,
          descripcion,
          categoria: descCompleta.split('>')[0]?.trim() || 'Nomenclador Oficial',
          tipo_bien: tipoBien,
          aec,
          iva_alicuota: tipoBien === 'BK' || tipoBien === 'BIT' ? 10.5 : 21.0,
          anexo_excepcion: anexoRaw ? `Decreto 557/2023 - Anexo ${anexoRaw}` : null,
          arancel_excepcion: anexoRaw && arancelAplicable !== null ? arancelAplicable : null,
          condicion_excepcion: condicion,
          esDestacado: false
        });
      }
    } catch (err) {
      console.warn('[ExcelClient_XLSX_WARNING]: Error leyendo Nomenclatura_NCM_aranceles.xlsx:', err.message);
    }
  }

  // 3. Asegurar fallback mínimo si no hubiera archivos en disco
  if (mapByCode.size === 0) {
    for (const item of FALLBACK_NCM) {
      mapByCode.set(item.codigo_ncm, { ...item, esDestacado: true });
    }
  }

  cachedCatalogoNcm = Array.from(mapByCode.values());
  lastNcmLoadTime = now;
  return cachedCatalogoNcm;
}

export function cargarPaisesExcel() {
  if (cachedPaises) return cachedPaises;
  const csvPath = path.join(DB_FOLDER, 'paises.csv');
  if (fs.existsSync(csvPath)) {
    const rows = parseCsvFile(csvPath);
    if (rows.length > 0) {
      cachedPaises = rows.map((r) => ({
        codigo_iso: (r.codigo_iso || '').trim().toUpperCase(),
        nombre: (r.nombre || '').trim(),
        es_mercosur: ['true', '1', 'si'].includes(String(r.es_mercosur || '').toLowerCase()),
        flete_estimado_pct: parseFloat(r.flete_estimado_pct) || 8.0
      }));
      return cachedPaises;
    }
  }
  cachedPaises = FALLBACK_PAISES;
  return cachedPaises;
}

export function cargarPuertosExcel() {
  if (cachedPuertos) return cachedPuertos;
  const csvPath = path.join(DB_FOLDER, 'puertos_aduanas.csv');
  if (fs.existsSync(csvPath)) {
    const rows = parseCsvFile(csvPath);
    if (rows.length > 0) {
      cachedPuertos = rows.map((r) => ({
        id: r.uuid || r.id,
        nombre: r.nombre,
        via_ingreso: r.via_ingreso || 'MARITIMA_FLUVIAL',
        provincia: r.provincia || '',
        codigo_sim: r.codigo_sim || '',
        operador: r.operador || '',
        tipos_carga: (r.tipos_carga || '').split(',').map((s) => s.trim()).filter(Boolean),
        infraestructura: r.infraestructura || '',
        latitud: parseFloat(r.latitud) || 0,
        longitud: parseFloat(r.longitud) || 0,
        paises_frecuentes: (r.paises_frecuentes || '').split(',').map((s) => s.trim()).filter(Boolean)
      }));
      return cachedPuertos;
    }
  }
  cachedPuertos = FALLBACK_PUERTOS;
  return cachedPuertos;
}

export function cargarOperadoresExcel() {
  if (cachedOperadores) return cachedOperadores;
  const csvPath = path.join(DB_FOLDER, 'operadores_logisticos.csv');
  if (fs.existsSync(csvPath)) {
    const rows = parseCsvFile(csvPath);
    if (rows.length > 0) {
      cachedOperadores = rows.map((r) => ({
        id: r.id,
        razon_social: r.razon_social,
        tipo_operador: r.tipo_operador || 'DESPACHANTE',
        matricula_dga: r.matricula_dga || '',
        email_notificacion: r.email_notificacion || '',
        telefono: r.telefono || '',
        rubros_especialidad: (r.rubros_especialidad || '').split(',').map((s) => s.trim()).filter(Boolean),
        aduanas_operativas: (r.aduanas_operativas || '').split(',').map((s) => s.trim()).filter(Boolean),
        modalidades: (r.modalidades || '').split(',').map((s) => s.trim()).filter(Boolean),
        experiencia_anios: parseInt(r.experiencia_anios, 10) || 10,
        rating: parseFloat(r.rating) || 4.9,
        activo: !['false', '0', 'no'].includes(String(r.activo || 'true').toLowerCase())
      }));
      return cachedOperadores;
    }
  }
  cachedOperadores = FALLBACK_OPERADORES;
  return cachedOperadores;
}

let cachedCotizacionDolar = null;
let lastCotizacionFetchTime = 0;

/**
 * Obtiene la cotización oficial vendedor del Banco Nación (ARS/USD).
 * Conecta en tiempo real con la API (dolarapi.com) y utiliza la planilla Excel como respaldo.
 */
export async function obtenerCotizacionDolarOficial() {
  const now = Date.now();
  if (cachedCotizacionDolar && (now - lastCotizacionFetchTime < 10 * 60 * 1000)) {
    return cachedCotizacionDolar;
  }

  try {
    const res = await fetch('https://dolarapi.com/v1/dolares/oficial', { cache: 'no-store' });
    if (res.ok) {
      const data = await res.json();
      const venta = parseFloat(data.venta || data.value_sell);
      if (venta && !isNaN(venta) && venta > 0) {
        cachedCotizacionDolar = venta;
        lastCotizacionFetchTime = now;
        return cachedCotizacionDolar;
      }
    }
  } catch (err) {
    console.warn('API Dólar no disponible en servidor. Utilizando respaldo de planilla Excel:', err.message);
  }

  try {
    const csvPath = path.join(DB_FOLDER, 'parametros_regulatorios.csv');
    if (fs.existsSync(csvPath)) {
      const rows = parseCsvFile(csvPath);
      const row = rows.find((r) => (r.parametro || '').includes('CotizacionOficialBNA'));
      if (row && row.valor) {
        cachedCotizacionDolar = parseFloat(row.valor) || 1420.50;
        return cachedCotizacionDolar;
      }
    }
  } catch (err) {}

  return 1420.50;
}

export function guardarSimulacionEnExcel(resultadoSimulacion) {
  const csvPath = path.join(DB_FOLDER, 'simulaciones.csv');
  const headers = [
    'id_cotizacion',
    'fecha_emision',
    'codigo_ncm',
    'descripcion_producto',
    'pais_origen',
    'cantidad',
    'valor_fob_unitario',
    'valor_fob_total',
    'flete_usd',
    'seguro_usd',
    'valor_cif_usd',
    'total_gravamenes_usd',
    'costo_total_nacionalizado_usd',
    'costo_unitario_final_usd',
    'cotizacion_bna_ars',
    'costo_total_estimado_ars',
    'desglose_json'
  ];

  return appendCsvRow(csvPath, headers, {
    id_cotizacion: resultadoSimulacion.idCotizacion,
    fecha_emision: resultadoSimulacion.fechaEmision || new Date().toISOString(),
    codigo_ncm: resultadoSimulacion.fichaProducto?.codigoNcm || '',
    descripcion_producto: resultadoSimulacion.fichaProducto?.descripcion || '',
    pais_origen: resultadoSimulacion.fichaProducto?.codigoPais || 'CHN',
    cantidad: resultadoSimulacion.parametrosComerciales?.cantidad || 1,
    valor_fob_unitario: resultadoSimulacion.parametrosComerciales?.valorFobUnitario || 0,
    valor_fob_total: resultadoSimulacion.parametrosComerciales?.valorFobTotal || 0,
    flete_usd: resultadoSimulacion.parametrosComerciales?.flete || 0,
    seguro_usd: resultadoSimulacion.parametrosComerciales?.seguro || 0,
    valor_cif_usd: resultadoSimulacion.parametrosComerciales?.valorCIF || 0,
    total_gravamenes_usd: resultadoSimulacion.resumenEconomico?.totalGravamenesUSD || 0,
    costo_total_nacionalizado_usd: resultadoSimulacion.resumenEconomico?.costoTotalNacionalizadoUSD || 0,
    costo_unitario_final_usd: resultadoSimulacion.resumenEconomico?.costoUnitarioFinalUSD || 0,
    cotizacion_bna_ars: resultadoSimulacion.resumenEconomico?.tipoCambioBNA || 1420.5,
    costo_total_estimado_ars: resultadoSimulacion.resumenEconomico?.costoTotalEstimadoARS || 0,
    desglose_json: JSON.stringify(resultadoSimulacion.desgloseTributario || [])
  });
}

export function buscarSimulacionEnExcelPorId(idCotizacion) {
  const csvPath = path.join(DB_FOLDER, 'simulaciones.csv');
  if (!fs.existsSync(csvPath)) return null;
  const rows = parseCsvFile(csvPath);
  return rows.find((r) => r.id_cotizacion === idCotizacion) || null;
}

export function guardarSolicitudContactoEnExcel(datosLead) {
  const csvPath = path.join(DB_FOLDER, 'solicitudes_contacto.csv');
  const leadId = `EXCEL-LEAD-${Date.now()}`;
  const headers = [
    'id_lead',
    'fecha_registro',
    'operador_id',
    'id_cotizacion',
    'nombre_apellido',
    'empresa',
    'email',
    'telefono',
    'comentarios'
  ];

  appendCsvRow(csvPath, headers, {
    id_lead: leadId,
    fecha_registro: new Date().toISOString(),
    operador_id: datosLead.operadorId,
    id_cotizacion: datosLead.idCotizacion || '',
    nombre_apellido: datosLead.nombreApellido,
    empresa: datosLead.empresa || '',
    email: datosLead.email,
    telefono: datosLead.telefono,
    comentarios: datosLead.comentarios || ''
  });

  return { exito: true, leadId };
}

export function getExcelDatabaseInfo() {
  return {
    engine: 'Excel OpenXML (.xlsx) & Planillas CSV (.csv)',
    excelDbPath: EXCEL_DB_PATH,
    officialNcmExcelPath: EXCEL_NCM_OFICIAL_PATH
  };
}
