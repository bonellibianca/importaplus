import 'server-only';
import crypto from 'crypto';
import { GravamenItemBE } from '../../be/GravamenItemBE';

/**
 * Capa BLL (Business Logic Layer) - Servidor Node.js
 * Patrón de Diseño GoF: TEMPLATE METHOD (Método Plantilla)
 */
export class CalculadorAduaneroTemplate {
  liquidarOperacion(contexto) {
    const { cantidad, valorFobUnitario, condicionImportador = 'REVENTA_RI', tipoCambioBNA = 1420.50 } = contexto;
    const cantSegura = Math.max(1, Number(cantidad) || 1);

    // 1. Valor FOB Total
    const fobTotal = this.calcularFobTotal(cantSegura, valorFobUnitario);

    // 2. Flete y Seguro Internacional
    const { flete, seguro, cif } = this.calcularValorCIF(fobTotal, contexto);

    // 3. Paso Polimórfico (Hook): Derechos de Importación (DIE)
    const dieItem = this.calcularDerechoImportacion(cif, contexto);

    // 4. Tasa de Estadística (3% con escala de topes legales según Decreto 99/2019)
    const estadisticaItem = this.calcularTasaEstadistica(cif, contexto);

    // 5. Base Imponible Interior para Tributos Nacionales
    const baseImponibleInterior = Number((cif + dieItem.montoUSD + estadisticaItem.montoUSD).toFixed(2));

    // 6. Impuesto al Valor Agregado (IVA DGA)
    const ivaItem = this.calcularIVA(baseImponibleInterior, contexto);

    // 7. Percepción IVA Adicional (RG AFIP 2281)
    const percepcionIvaItem = this.calcularPercepcionIVA(baseImponibleInterior, ivaItem.alicuotaNum);

    // 8. Percepción Impuesto a las Ganancias (RG AFIP 2281: 6% Reventa vs 11% Consumo Particular)
    const percepcionGananciasItem = this.calcularPercepcionGanancias(baseImponibleInterior, condicionImportador);

    // 9. Arancel SIM ($28) + Tasa de Digitalización ($10) = $38 Fijo
    const arancelSimItem = this.calcularArancelesFijosSIM();

    const gravamenes = [
      dieItem,
      estadisticaItem,
      new GravamenItemBE({
        id: 'base_interior',
        concepto: 'Base Imponible Aduanera (CIF + DIE + Estadística)',
        alicuota: '-',
        alicuotaNum: 0,
        montoUSD: baseImponibleInterior,
        esSubtotal: true,
        descripcion: 'Base imponible oficial sobre la que se calculan los tributos interiores (IVA y Percepciones)'
      }),
      ivaItem,
      percepcionIvaItem,
      percepcionGananciasItem,
      arancelSimItem
    ];

    const totalGravamenesUSD = Number((
      dieItem.montoUSD +
      estadisticaItem.montoUSD +
      ivaItem.montoUSD +
      percepcionIvaItem.montoUSD +
      percepcionGananciasItem.montoUSD +
      arancelSimItem.montoUSD
    ).toFixed(2));

    const costoTotalNacionalizadoUSD = Number((cif + totalGravamenesUSD).toFixed(2));
    const costoUnitarioFinalUSD = Number((costoTotalNacionalizadoUSD / cantSegura).toFixed(2));
    const incidenciaFiscalPct = fobTotal > 0 ? Number(((totalGravamenesUSD / fobTotal) * 100).toFixed(1)) : 0;
    const sobrecostoTotalPct = fobTotal > 0 ? Number((((costoTotalNacionalizadoUSD - fobTotal) / fobTotal) * 100).toFixed(1)) : 0;
    const costoTotalEstimadoARS = Number((costoTotalNacionalizadoUSD * tipoCambioBNA).toFixed(2));

    const hashRandom = crypto.randomBytes(3).toString('hex').toUpperCase();
    const idCotizacion = `IMP-${new Date().getFullYear()}-${hashRandom}`;

    return {
      idCotizacion,
      fechaEmision: new Date().toISOString(),
      regimenAplicado: this.getNombreRegimen(contexto),
      parametrosComerciales: {
        cantidad: cantSegura,
        valorFobUnitario,
        valorFobTotal: fobTotal,
        flete,
        seguro,
        valorCIF: cif
      },
      desgloseTributario: gravamenes,
      resumenEconomico: {
        totalGravamenesUSD,
        costoTotalNacionalizadoUSD,
        costoUnitarioFinalUSD,
        incidenciaFiscalPct,
        sobrecostoTotalPct,
        tipoCambioBNA,
        costoTotalEstimadoARS
      }
    };
  }

  calcularFobTotal(cantidad, valorFobUnitario) {
    return Number((cantidad * valorFobUnitario).toFixed(2));
  }

  calcularValorCIF(fobTotal, { fleteOpcional, seguroOpcional }) {
    const flete = (fleteOpcional !== null && fleteOpcional !== undefined && fleteOpcional !== '')
      ? Math.max(0, Number(Number(fleteOpcional).toFixed(2)))
      : 0;

    const seguro = (seguroOpcional !== null && seguroOpcional !== undefined && seguroOpcional !== '')
      ? Math.max(0, Number(Number(seguroOpcional).toFixed(2)))
      : 0;

    const cif = Number((fobTotal + flete + seguro).toFixed(2));

    return { flete, seguro, cif };
  }

  calcularTasaEstadistica(cif, { ncm, pais }) {
    if (pais.es_mercosur || ncm.tipo_bien === 'BK') {
      return new GravamenItemBE({
        id: 'tasa_estadistica',
        concepto: 'Tasa de Estadística (Exento Mercosur / Bien de Capital)',
        alicuota: '0%',
        alicuotaNum: 0,
        montoUSD: 0,
        descripcion: 'Exento conforme normativa del Mercosur y régimen de Bienes de Capital'
      });
    }

    const tasaBruta = Number((cif * 0.03).toFixed(2));

    let topeMaximo = 150000;
    if (cif < 10000) {
      topeMaximo = 180;
    } else if (cif < 100000) {
      topeMaximo = 3000;
    } else if (cif < 1000000) {
      topeMaximo = 30000;
    }

    const montoFinal = Number(Math.min(tasaBruta, topeMaximo).toFixed(2));

    return new GravamenItemBE({
      id: 'tasa_estadistica',
      concepto: 'Tasa de Estadística (3% con tope legal Dec. 99/2019)',
      alicuota: '3%',
      alicuotaNum: 3.0,
      montoUSD: montoFinal,
      descripcion: `3% sobre valor CIF (tope legal aplicable: USD ${topeMaximo})`
    });
  }

  calcularIVA(baseImponible, { ncm }) {
    const explicitIva = Number(ncm.iva_alicuota);
    const alicuota = explicitIva > 0
      ? explicitIva
      : (ncm.tipo_bien === 'BK' || ncm.tipo_bien === 'BIT' ? 10.5 : 21);
    const monto = Number((baseImponible * (alicuota / 100)).toFixed(2));

    let descripcionIva = 'Alícuota general de importación DGA';
    if (alicuota === 10.5) {
      descripcionIva = ncm.tipo_bien === 'BIT'
        ? 'Alícuota reducida 10.5% por Bien de Informática y Telecomunicaciones (BIT)'
        : 'Alícuota reducida 10.5% por Bien de Capital (BK)';
    }

    return new GravamenItemBE({
      id: 'iva_aduanero',
      concepto: `Impuesto al Valor Agregado (IVA ${alicuota}%)`,
      alicuota: `${alicuota}%`,
      alicuotaNum: alicuota,
      montoUSD: monto,
      descripcion: descripcionIva
    });
  }

  calcularPercepcionIVA(baseImponible, alicuotaIvaAplicada) {
    const alicuota = alicuotaIvaAplicada === 10.5 ? 10.0 : 20.0;
    const monto = Number((baseImponible * (alicuota / 100)).toFixed(2));

    return new GravamenItemBE({
      id: 'percepcion_iva',
      concepto: `Percepción de IVA Adicional (RG 2281 - ${alicuota}%)`,
      alicuota: `${alicuota}%`,
      alicuotaNum: alicuota,
      montoUSD: monto,
      descripcion: 'Régimen de percepción aduanera del IVA según Resolución General AFIP 2281'
    });
  }

  calcularPercepcionGanancias(baseImponible, condicionImportador) {
    const esConsumo = condicionImportador === 'USO_PROPIO_CONSUMO';
    const alicuota = esConsumo ? 11.0 : 6.0;
    const monto = Number((baseImponible * (alicuota / 100)).toFixed(2));

    return new GravamenItemBE({
      id: 'percepcion_ganancias',
      concepto: `Percepción Impuesto a las Ganancias (RG 2281 - ${alicuota}%)`,
      alicuota: `${alicuota}%`,
      alicuotaNum: alicuota,
      montoUSD: monto,
      descripcion: esConsumo
        ? 'Alícuota 11% para importación con destino a uso/consumo particular o no categorizado'
        : 'Alícuota general 6% para reventa comercial / Responsable Inscripto'
    });
  }

  calcularArancelesFijosSIM() {
    return new GravamenItemBE({
      id: 'arancel_sim',
      concepto: 'Arancel SIM + Digitalización Documental',
      alicuota: 'Fijo',
      alicuotaNum: 0,
      montoUSD: 38.00,
      descripcion: 'Arancel del Sistema Informático Malvina (USD 28) + Digitalización Oficial (USD 10)'
    });
  }

  calcularDerechoImportacion(cif, contexto) {
    throw new Error('calcularDerechoImportacion() debe ser implementado por la subclase');
  }

  getNombreRegimen(contexto) {
    return 'Régimen General Aduanero';
  }
}

export class LiquidacionIntrazonaBLL extends CalculadorAduaneroTemplate {
  calcularDerechoImportacion(cif, { ncm, pais }) {
    return new GravamenItemBE({
      id: 'derechos_importacion',
      concepto: 'Derechos de Importación (DIE - Intrazona Mercosur 0%)',
      alicuota: '0%',
      alicuotaNum: 0,
      montoUSD: 0,
      descripcion: 'Tratamiento arancelario preferencial Mercosur (Acuerdo ACE 18)'
    });
  }

  getNombreRegimen() {
    return 'Intrazona Mercosur (0% DIE)';
  }
}

export class LiquidacionExtrazonaGeneralBLL extends CalculadorAduaneroTemplate {
  calcularDerechoImportacion(cif, { ncm }) {
    const alicuota = Number(ncm.aec || 0);
    const monto = Number((cif * (alicuota / 100)).toFixed(2));

    return new GravamenItemBE({
      id: 'derechos_importacion',
      concepto: `Derechos de Importación Extrazona (DIE - AEC ${alicuota}%)`,
      alicuota: `${alicuota}%`,
      alicuotaNum: alicuota,
      montoUSD: monto,
      descripcion: 'Arancel Externo Común correspondiente a la posición NCM'
    });
  }

  getNombreRegimen() {
    return 'Extrazona General (Arancel Externo Común AEC)';
  }
}

export class LiquidacionExcepcionArancelariaBLL extends CalculadorAduaneroTemplate {
  calcularDerechoImportacion(cif, { ncm }) {
    const alicuota = Number(ncm.arancel_excepcion ?? ncm.aec);
    const monto = Number((cif * (alicuota / 100)).toFixed(2));

    return new GravamenItemBE({
      id: 'derechos_importacion',
      concepto: `Derechos de Importación Reducidos (${ncm.anexo_excepcion || 'Excepción'} - ${alicuota}%)`,
      alicuota: `${alicuota}%`,
      alicuotaNum: alicuota,
      montoUSD: monto,
      descripcion: ncm.condicion_excepcion || 'Régimen preferencial de excepción arancelaria por decreto sectorial'
    });
  }

  getNombreRegimen({ ncm }) {
    return `Excepción Arancelaria (${ncm.anexo_excepcion || 'Decreto 557/2023'})`;
  }
}
