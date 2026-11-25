import test from 'node:test';
import assert from 'node:assert/strict';
import crypto from 'node:crypto';
import fs from 'node:fs';
import path from 'node:path';
import { register } from 'node:module';
import { pathToFileURL } from 'node:url';

const ROOT_DIR = process.cwd();

// Registrar resolvedor ligero en memoria para alias @/*, extensiones .js y módulo 'server-only'
const loaderCode = `
import fs from 'node:fs';
import path from 'node:path';
import { fileURLToPath, pathToFileURL } from 'node:url';

const rootDir = ${JSON.stringify(ROOT_DIR)};

export async function resolve(specifier, context, nextResolve) {
  if (specifier === 'server-only') {
    return { shortCircuit: true, url: 'data:text/javascript,export default {};' };
  }

  let target = specifier;
  if (specifier.startsWith('@/')) {
    target = pathToFileURL(path.join(rootDir, 'src', specifier.slice(2))).href;
  } else if ((specifier.startsWith('./') || specifier.startsWith('../')) && context.parentURL?.startsWith('file:')) {
    const parentDir = path.dirname(fileURLToPath(context.parentURL));
    const candidate = path.resolve(parentDir, specifier);
    if (!fs.existsSync(candidate) && fs.existsSync(candidate + '.js')) {
      target = pathToFileURL(candidate + '.js').href;
    }
  }

  if (target.startsWith('file:')) {
    const p = fileURLToPath(target);
    if (!fs.existsSync(p) && fs.existsSync(p + '.js')) {
      target = pathToFileURL(p + '.js').href;
    }
  }

  return nextResolve(target, context);
}
`;

register(`data:text/javascript;base64,${Buffer.from(loaderCode).toString('base64')}`, pathToFileURL('./'));

const {
  LiquidacionIntrazonaBLL,
  LiquidacionExtrazonaGeneralBLL,
  LiquidacionExcepcionArancelariaBLL
} = await import('../src/server/bll/templates/CalculadorAduaneroTemplate.js');
const { PosicionArancelariaBE } = await import('../src/server/be/PosicionArancelariaBE.js');
const { PaisBE } = await import('../src/server/be/PaisBE.js');
const { NcmDAL } = await import('../src/server/dal/NcmDAL.js');
const { esUrlImagenSegura, ForwarderDAL } = await import('../src/server/dal/ForwarderDAL.js');
const { sanitizeSpreadsheetCell } = await import('../src/server/dal/excelClient.js');
const { checkRateLimit } = await import('../src/server/security/rateLimiter.js');
const {
  BusquedaNcmSchema,
  SimularOperacionSchema,
  SolicitudLeadSchema
} = await import('../src/server/security/schemas.js');

test('Liquidación Aduanera PN1: Excepción Decreto 557/2023 (Notebook BIT 2% DIE, 10.5% IVA, 3% Estadística con tope)', () => {
  const ncm = new PosicionArancelariaBE({
    codigo_ncm: '8471.30.12',
    descripcion: 'Notebooks portátiles',
    tipo_bien: 'BIT',
    aec: 16.0,
    iva_alicuota: 10.5,
    anexo_excepcion: 'Anexo II Dec 557/2023',
    arancel_excepcion: 2.0
  });
  const pais = new PaisBE({ codigo_iso: 'CHN', nombre: 'China', es_mercosur: false });

  const calc = new LiquidacionExcepcionArancelariaBLL();
  const res = calc.liquidarOperacion({
    ncm,
    pais,
    cantidad: 10,
    valorFobUnitario: 500,
    fleteOpcional: 200,
    seguroOpcional: 50,
    condicionImportador: 'REVENTA_RI',
    tipoCambioBNA: 1400
  });

  // CIF = 5000 + 200 + 50 = 5250 (< 10.000 -> tope tasa estadística USD 180; 3% de 5250 = 157.50)
  assert.equal(res.parametrosComerciales.valorCIF, 5250);
  const die = res.desgloseTributario.find((g) => g.id === 'derechos_importacion');
  const est = res.desgloseTributario.find((g) => g.id === 'tasa_estadistica');
  const iva = res.desgloseTributario.find((g) => g.id === 'iva_aduanero');
  const percIva = res.desgloseTributario.find((g) => g.id === 'percepcion_iva');
  const percGan = res.desgloseTributario.find((g) => g.id === 'percepcion_ganancias');

  assert.equal(die.montoUSD, 105.0); // 2% de 5250
  assert.equal(est.montoUSD, 157.5); // 3% de 5250 <= 180
  assert.match(iva.descripcion, /Informática y Telecomunicaciones/i);
  assert.equal(percIva.alicuotaNum, 10.0);
  assert.equal(percGan.alicuotaNum, 6.0);
});

test('Liquidación Aduanera PN1: Intrazona Mercosur (0% DIE y 0% Tasa de Estadística) y Consumo Particular (11% Ganancias)', () => {
  const ncm = new PosicionArancelariaBE({
    codigo_ncm: '6109.10.00',
    descripcion: 'Remeras de algodón',
    tipo_bien: 'GENERAL',
    aec: 35.0,
    iva_alicuota: 21.0
  });
  const pais = new PaisBE({ codigo_iso: 'BRA', nombre: 'Brasil', es_mercosur: true });

  const calc = new LiquidacionIntrazonaBLL();
  const res = calc.liquidarOperacion({
    ncm,
    pais,
    cantidad: 100,
    valorFobUnitario: 10,
    fleteOpcional: 0,
    seguroOpcional: 0,
    condicionImportador: 'USO_PROPIO_CONSUMO',
    tipoCambioBNA: 1400
  });

  const die = res.desgloseTributario.find((g) => g.id === 'derechos_importacion');
  const est = res.desgloseTributario.find((g) => g.id === 'tasa_estadistica');
  const percGan = res.desgloseTributario.find((g) => g.id === 'percepcion_ganancias');

  assert.equal(die.montoUSD, 0);
  assert.equal(est.montoUSD, 0);
  assert.equal(percGan.alicuotaNum, 11.0);
});

test('Liquidación Aduanera PN1: Tope legal Decreto 99/2019 (USD 180 para CIF < 10.000)', () => {
  const ncm = new PosicionArancelariaBE({
    codigo_ncm: '6402.19.00',
    descripcion: 'Calzado deportivo',
    tipo_bien: 'GENERAL',
    aec: 35.0,
    iva_alicuota: 21.0
  });
  const pais = new PaisBE({ codigo_iso: 'VNM', nombre: 'Vietnam', es_mercosur: false });

  const calc = new LiquidacionExtrazonaGeneralBLL();
  const res = calc.liquidarOperacion({
    ncm,
    pais,
    cantidad: 100,
    valorFobUnitario: 90, // CIF = 9000 -> 3% sería 270, tope aplica en 180
    fleteOpcional: 0,
    seguroOpcional: 0
  });

  const est = res.desgloseTributario.find((g) => g.id === 'tasa_estadistica');
  assert.equal(est.montoUSD, 180.0);
});

test('Buscador NCM soporta códigos numéricos (8471.30) y términos alfanuméricos (4G / Notebook)', async () => {
  const porCodigo = await NcmDAL.buscarPorDescripcion('8471.30', 5);
  assert.ok(porCodigo.length > 0, 'Debe encontrar posiciones buscando por prefijo de código NCM');
  assert.ok(porCodigo.some((p) => p.codigo_ncm.startsWith('8471.30')));

  const porAlfanum = await NcmDAL.buscarPorDescripcion('4G', 5);
  assert.ok(porAlfanum.length > 0, 'Debe encontrar Smartphones buscando término alfanumérico 4G');
});

test('Seguridad Anti-CSV-Injection neutraliza fórmulas maliciosas preservando números negativos', () => {
  assert.equal(sanitizeSpreadsheetCell('=HYPERLINK("http://evil.tld")'), '\'=HYPERLINK("http://evil.tld")');
  assert.equal(sanitizeSpreadsheetCell('+SUM(A1:A2)'), "'+SUM(A1:A2)");
  assert.equal(sanitizeSpreadsheetCell('@cmd'), "'@cmd");
  assert.equal(sanitizeSpreadsheetCell('-1+2'), "'-1+2");
  assert.equal(sanitizeSpreadsheetCell('-34.5985'), '-34.5985');
  assert.equal(sanitizeSpreadsheetCell('Juan Pérez'), 'Juan Pérez');
});

test('Seguridad de URLs en Forwarders bloquea javascript:, http:// e IPs no seguras', async () => {
  assert.equal(esUrlImagenSegura('javascript:alert(1)'), false);
  assert.equal(esUrlImagenSegura('http://127.0.0.1:8080/internal.png'), false);
  assert.equal(esUrlImagenSegura('//evil.example.com/img.png'), false);
  assert.equal(esUrlImagenSegura('https://images.unsplash.com/photo-1586528116311-ad8dd3c8310d'), true);
  assert.equal(esUrlImagenSegura('/logo-importa.png'), true);

  const fwds = await ForwarderDAL.obtenerGrillaCuatro();
  assert.equal(fwds.length, 4);
});

test('Validación Zod rechaza manipulación de tipoCambioBNA y caracteres ilegales', () => {
  const tcManipulado = SimularOperacionSchema.safeParse({
    codigoNcm: '8471.30.12',
    codigoPais: 'CHN',
    cantidad: 2,
    valorFobUnitario: 500,
    tipoCambioBNA: 0.01
  });
  assert.equal(tcManipulado.success, false, 'Debe rechazar tipoCambioBNA adulterado (< 100)');

  const xssSearch = BusquedaNcmSchema.safeParse({ q: '<script>alert(1)</script>' });
  assert.equal(xssSearch.success, false, 'Debe rechazar etiquetas HTML en búsqueda NCM');

  const badPhone = SolicitudLeadSchema.safeParse({
    operadorId: 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa',
    nombreApellido: 'María Gómez',
    email: 'maria@empresa.com.ar',
    telefono: '123456;DROP TABLE'
  });
  assert.equal(badPhone.success, false, 'Debe rechazar caracteres inválidos en teléfono');
});

test('Rate Limiter mantiene memoria acotada ante ráfaga de >5200 IPs distintas', () => {
  for (let i = 0; i < 5300; i++) {
    const r = checkRateLimit(`198.51.100.${i}`, 'flood_test', 2, 60000);
    assert.equal(r.allowed, true);
  }
  const blocked = checkRateLimit('203.0.113.99', 'quota_test', 1, 60000);
  const blocked2 = checkRateLimit('203.0.113.99', 'quota_test', 1, 60000);
  assert.equal(blocked.allowed, true);
  assert.equal(blocked2.allowed, false);
});

test('Integridad criptográfica DVH y DVV de AdminImportaPlus/Data/importaplus_db.json', () => {
  const dbPath = path.join(ROOT_DIR, 'AdminImportaPlus', 'Data', 'importaplus_db.json');
  const db = JSON.parse(fs.readFileSync(dbPath, 'utf8').replace(/^\uFEFF/, ''));

  const sha256 = (s) => crypto.createHash('sha256').update(s, 'utf8').digest('hex');
  const calcDvv = (dvhs) => {
    const sorted = [...dvhs].sort();
    return sha256(sorted.length === 0 ? 'EMPTY' : sorted.join('|'));
  };

  assert.ok(db.Paises.length > 0, 'Paises en importaplus_db.json no debe estar vacío');
  assert.ok(db.NcmCatalogo.length > 0, 'NcmCatalogo en importaplus_db.json no debe estar vacío');
  assert.equal(db.Forwarders.length, 4, 'Debe contener los 4 Forwarders institucionales limpios');

  for (const f of db.Forwarders) {
    const expected = sha256(`${f.id.toLowerCase()}|${f.nombre.trim()}|${f.imagen_url.trim()}|${f.whatsapp.trim()}|${f.email.trim()}|${f.orden}|${f.activo ? '1' : '0'}`);
    assert.equal(f.dvh, expected, `DVH inválido para Forwarder ${f.nombre}`);
  }

  const dvvMap = new Map(db.DigitosVerificadores.map((d) => [d.tabla_nombre, d]));
  assert.equal(dvvMap.get('forwarders').dvv, calcDvv(db.Forwarders.map((x) => x.dvh)));
  assert.equal(dvvMap.get('puertos_aduanas').dvv, calcDvv(db.PuertosAduanas.map((x) => x.dvh)));
  assert.equal(dvvMap.get('paises').dvv, calcDvv(db.Paises.map((x) => x.dvh)));
  assert.equal(dvvMap.get('ncm_catalogo').dvv, calcDvv(db.NcmCatalogo.map((x) => x.dvh)));
  assert.equal(dvvMap.get('admin_usuarios').dvv, calcDvv(db.AdminUsuarios.map((x) => x.dvh)));
});
