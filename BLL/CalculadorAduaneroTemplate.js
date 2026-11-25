/**
 * Capa BLL (Business Logic Layer)
 * Patrón de Diseño GoF: TEMPLATE METHOD (Método Plantilla)
 * 
 * Clase Base Abstracta: CalculadorAduaneroTemplate
 * 
 * Define el esqueleto invariable del algoritmo tributario aduanero argentino (RF03):
 * 1. Cálculo de Valor FOB Total
 * 2. Determinación de Flete y Seguro (Reales o Supletorios)
 * 3. Base Imponible de Entrada Aduanera (Valor CIF)
 * 4. Determinación de Derechos de Importación Extrazona (DIE) [Hook / Variable]
 * 5. Tasa de Estadística Aduanera (con tope legal)
 * 6. Base Imponible Interior (CIF + DIE + Tasa Estadística)
 * 7. Liquidación de Impuestos Interiores y Percepciones [Hook / Variable]
 * 8. Cargos Fijos Operativos (SIM + Digitalización)
 * 9. Consolidación de Costo Total Nacionalizado y Unitario
 */
class CalculadorAduaneroTemplate {
  /**
   * MÉTODO PLANTILLA (Template Method)
   * Ejecuta la secuencia reglamentaria en cascada fijada por el Código Aduanero Argentino.
   * @param {CalculadoraArancelariaBE} be 
   * @param {Object} parametros 
   * @returns {CalculadoraArancelariaBE}
   */
  liquidarTemplate(be, parametros) {
    // 1. Paso común invariable: FOB Total
    this.calcularFob(be);

    // 2. Paso común invariable: Flete y Seguro
    this.calcularFleteYSeguro(be);

    // 3. Paso común invariable: CIF Aduanero
    this.calcularCif(be);

    // 4. PASO HOOK / POLIMÓRFICO: Derechos de Importación (DIE)
    this.determinarDerechosImportacion(be, parametros);

    // 5. Paso común: Tasa de Estadística
    this.calcularTasaEstadistica(be, parametros);

    // 6. Paso común: Base Imponible Interior
    this.calcularBaseImponibleInterior(be);

    // 7. PASO HOOK / POLIMÓRFICO: Impuestos Interiores (IVA y Percepciones RG 2281)
    this.calcularImpuestosInteriores(be, parametros);

    // 8. Paso común: Cargos fijos SIM y Digitalización
    this.calcularCargosFijos(be, parametros);

    // 9. Paso común: Consolidación de resultados y métricas
    this.consolidarTotales(be, parametros);

    // 10. Desglose detallado DTO de gravámenes
    this.generarDesgloseGravamenes(be);

    return be;
  }

  // --- Implementaciones de los Pasos Comunes Invariables ---

  calcularFob(be) {
    be.fobTotal = +(be.cantidad * be.precioFobUnitario).toFixed(2);
  }

  calcularFleteYSeguro(be) {
    if (be.esEstimacionFlete) {
      be.flete = be.pais?.fleteParametricoStd || 0;
    } else {
      be.flete = Math.max(0, parseFloat(be.flete) || 0);
    }

    if (be.esEstimacionSeguro) {
      be.seguro = +(be.fobTotal * ((be.pais?.seguroPctStd || 1.0) / 100)).toFixed(2);
    } else {
      be.seguro = Math.max(0, parseFloat(be.seguro) || 0);
    }
  }

  calcularCif(be) {
    be.cifAduanero = +(be.fobTotal + be.flete + be.seguro).toFixed(2);
  }

  calcularTasaEstadistica(be, parametros) {
    // Exenciones legales: Mercosur (Intrazona) y Bienes de Capital (BK) están exentos (0%)
    if (be.pais.esIntrazona || be.posicion.esBienDeCapital) {
      be.alicuotaEstadistica = 0.0;
      be.montoEstadistica = 0.0;
      return;
    }

    const alicuota = parametros.tasaEstadisticaPct || 3.0;
    be.alicuotaEstadistica = alicuota;

    const tasaBruta = +(be.cifAduanero * (alicuota / 100)).toFixed(2);

    // Escala de topes máximos legales según Decreto 99/2019 y prórrogas vigentes
    let topeMaximo = 150000;
    if (be.cifAduanero < 10000) {
      topeMaximo = 180;
    } else if (be.cifAduanero < 100000) {
      topeMaximo = 3000;
    } else if (be.cifAduanero < 1000000) {
      topeMaximo = 30000;
    }

    be.montoEstadistica = +(Math.min(tasaBruta, topeMaximo)).toFixed(2);
  }

  calcularBaseImponibleInterior(be) {
    be.baseImponibleInterior = +(be.cifAduanero + be.montoDie + be.montoEstadistica).toFixed(2);
  }

  calcularCargosFijos(be, parametros) {
    be.arancelSim = parametros.arancelSimFijoUSD;
    be.digitalizacionSim = parametros.digitalizacionFijaUSD;
    be.totalSimDigitalizacion = parametros.totalSimDigitalizacionUSD;
  }

  consolidarTotales(be, parametros) {
    be.totalGravamenes = +(
      be.montoDie +
      be.montoEstadistica +
      be.montoIva +
      be.montoIvaAdicional +
      be.montoGanancias +
      be.totalSimDigitalizacion
    ).toFixed(2);

    be.costoTotalNacionalizado = +(be.cifAduanero + be.totalGravamenes).toFixed(2);
    be.costoUnitarioNacionalizado = +(be.costoTotalNacionalizado / be.cantidad).toFixed(2);

    be.ratioSobrecostoPct = be.fobTotal > 0
      ? +(((be.costoTotalNacionalizado - be.fobTotal) / be.fobTotal) * 100).toFixed(1)
      : 0;

    be.cotizacionBna = parametros.cotizacionOficialBNA;
    be.costoTotalARS = +(be.costoTotalNacionalizado * be.cotizacionBna).toFixed(2);
    be.costoUnitarioARS = +(be.costoUnitarioNacionalizado * be.cotizacionBna).toFixed(2);

    be.fechaHora = new Date();
    if (!be.hashCotizacion) {
      const chars = 'ABCDEFGHJKLMNPQRSTUVWXYZ23456789';
      let res = 'IMP-';
      for (let i = 0; i < 8; i++) {
        res += chars.charAt(Math.floor(Math.random() * chars.length));
      }
      be.hashCotizacion = res;
    }
  }

  generarDesgloseGravamenes(be) {
    const GravamenClass = typeof window !== 'undefined' ? window.GravamenItemBE : null;
    if (!GravamenClass) return;

    be.gravamenes = [
      new GravamenClass({
        id: 'die',
        codigo: 'DIE / AEC',
        nombre: 'Derecho de Importación Extrazona (DIE)',
        baseCalculoUSD: be.cifAduanero,
        alicuotaPct: be.alicuotaDie,
        montoUSD: be.montoDie,
        porcentajeSobreTotal: +( (be.montoDie / be.totalGravamenes) * 100 || 0).toFixed(1),
        descripcion: be.pais.esIntrazona ? 'Exento por acuerdo Mercosur (Intrazona)' : 'Arancel externo según posición y régimen aplicable'
      }),
      new GravamenClass({
        id: 'estadistica',
        codigo: 'TASA EST.',
        nombre: 'Tasa de Estadística Aduanera',
        baseCalculoUSD: be.cifAduanero,
        alicuotaPct: be.alicuotaEstadistica,
        montoUSD: be.montoEstadistica,
        porcentajeSobreTotal: +( (be.montoEstadistica / be.totalGravamenes) * 100 || 0).toFixed(1),
        descripcion: (be.pais.esIntrazona || be.posicion.esBienDeCapital)
          ? 'Exento por acuerdo Mercosur (Intrazona) o Bien de Capital (BK)'
          : '3% sobre valor CIF sujeto a escala de topes legales (Decreto 99/2019 y prórrogas)'
      }),
      new GravamenClass({
        id: 'iva',
        codigo: 'IVA DGA',
        nombre: `Impuesto al Valor Agregado (${be.alicuotaIva}%)`,
        baseCalculoUSD: be.baseImponibleInterior,
        alicuotaPct: be.alicuotaIva,
        montoUSD: be.montoIva,
        porcentajeSobreTotal: +( (be.montoIva / be.totalGravamenes) * 100 || 0).toFixed(1),
        descripcion: be.posicion.esBienDeCapital ? 'Tasa reducida 10.5% por tratarse de Bien de Capital (BK)' : 'Tasa general aduanera del 21%'
      }),
      new GravamenClass({
        id: 'iva_adic',
        codigo: 'IVA ADIC.',
        nombre: `Percepción de IVA Adicional (${be.alicuotaIvaAdicional}%)`,
        baseCalculoUSD: be.baseImponibleInterior,
        alicuotaPct: be.alicuotaIvaAdicional,
        montoUSD: be.montoIvaAdicional,
        porcentajeSobreTotal: +( (be.montoIvaAdicional / be.totalGravamenes) * 100 || 0).toFixed(1),
        descripcion: 'Régimen de percepción aduanera según RG AFIP 2281'
      }),
      new GravamenClass({
        id: 'ganancias',
        codigo: 'GANANCIAS',
        nombre: `Percepción Impuesto a las Ganancias (${be.alicuotaGanancias}%)`,
        baseCalculoUSD: be.baseImponibleInterior,
        alicuotaPct: be.alicuotaGanancias,
        montoUSD: be.montoGanancias,
        porcentajeSobreTotal: +( (be.montoGanancias / be.totalGravamenes) * 100 || 0).toFixed(1),
        descripcion: be.esInscriptoGanancias ? 'Alícuota general 6% para responsables inscriptos' : 'Alícuota 11% para sujetos no categorizados'
      }),
      new GravamenClass({
        id: 'sim_dig',
        codigo: 'SIM + DIG',
        nombre: 'Arancel SIM + Tasa de Digitalización',
        baseCalculoUSD: be.totalSimDigitalizacion,
        alicuotaPct: 0,
        montoUSD: be.totalSimDigitalizacion,
        porcentajeSobreTotal: +( (be.totalSimDigitalizacion / be.totalGravamenes) * 100 || 0).toFixed(1),
        descripcion: 'Cargos fijos operativos del Sistema Informático Malvina (USD 28) y Digitalización (USD 10)'
      })
    ];
  }

  /**
   * PASO HOOK CON IMPLEMENTACIÓN POR DEFECTO:
   * Liquidación de Impuestos Interiores (IVA y Percepciones RG AFIP 2281).
   * Centralizado en la clase base para evitar código duplicado (DRY).
   * Puede ser sobrescrito por una subclase si un régimen fiscal específico lo amerita.
   * @param {CalculadoraArancelariaBE} be 
   * @param {Object} parametros 
   */
  calcularImpuestosInteriores(be, parametros) {
    const alicuotaIva = (be.posicion && Number(be.posicion.alicuotaIva) > 0)
      ? Number(be.posicion.alicuotaIva)
      : (be.posicion.esBienDeCapital ? parametros.ivaReducidoBKPct : parametros.ivaGeneralPct);
    be.alicuotaIva = alicuotaIva;
    be.montoIva = +(be.baseImponibleInterior * (alicuotaIva / 100)).toFixed(2);

    const alicuotaIvaAdic = alicuotaIva === 10.5 ? parametros.percepcionIvaReducidoPct : parametros.percepcionIvaGeneralPct;
    be.alicuotaIvaAdicional = alicuotaIvaAdic;
    be.montoIvaAdicional = +(be.baseImponibleInterior * (alicuotaIvaAdic / 100)).toFixed(2);

    const alicuotaGanancias = be.esInscriptoGanancias ? parametros.percepcionGananciasGeneralPct : parametros.percepcionGananciasNoCatPct;
    be.alicuotaGanancias = alicuotaGanancias;
    be.montoGanancias = +(be.baseImponibleInterior * (alicuotaGanancias / 100)).toFixed(2);
  }

  // --- Métodos Abstractos Hooks (A implementar obligatoriamente por cada subclase concreta) ---

  determinarDerechosImportacion(be, parametros) {
    throw new Error('El método determinarDerechosImportacion() debe ser implementado por la subclase');
  }
}

/**
 * Subclase Concreta 1: Régimen Intrazona (Mercosur - Acuerdo ACE 18)
 * - DIE = 0% por trato arancelario preferencial intrazona.
 * - Tasa Estadística = 0% para Mercosur.
 */
class LiquidacionIntrazonaBLL extends CalculadorAduaneroTemplate {
  determinarDerechosImportacion(be, parametros) {
    be.tratamientoArancelario = 'Intrazona Mercosur (Acuerdo ACE 18 - 0% DIE)';
    be.alicuotaDie = 0.0;
    be.montoDie = 0.0;
  }
}

/**
 * Subclase Concreta 2: Régimen General Extrazona (Arancel Externo Común AEC)
 * - Aplica el arancel general del AEC según NCM.
 * - Tasa Estadística 3% con tope legal.
 */
class LiquidacionExtrazonaGeneralBLL extends CalculadorAduaneroTemplate {
  determinarDerechosImportacion(be, parametros) {
    be.tratamientoArancelario = 'Extrazona (Régimen General AEC)';
    be.alicuotaDie = be.posicion.arancelExternoComun;
    be.montoDie = +(be.cifAduanero * (be.alicuotaDie / 100)).toFixed(2);
  }
}

/**
 * Subclase Concreta 3: Régimen de Excepción Arancelaria (Decreto 557/2023 o Bienes Especiales)
 * - Aplica alícuota reducida por decreto sectorial o anexo.
 * - Alícuotas de IVA diferenciadas si es Bien de Capital (BK) o Informática (BIT).
 */
class LiquidacionExcepcionArancelariaBLL extends CalculadorAduaneroTemplate {
  determinarDerechosImportacion(be, parametros) {
    be.tratamientoArancelario = `Excepción Arancelaria (${be.posicion.anexoExcepcion || 'Decreto 557/2023'})`;
    be.alicuotaDie = be.posicion.arancelAplicableExcepcion;
    be.montoDie = +(be.cifAduanero * (be.alicuotaDie / 100)).toFixed(2);
  }
}

if (typeof window !== 'undefined') {
  window.CalculadorAduaneroTemplate = CalculadorAduaneroTemplate;
  window.LiquidacionIntrazonaBLL = LiquidacionIntrazonaBLL;
  window.LiquidacionExtrazonaGeneralBLL = LiquidacionExtrazonaGeneralBLL;
  window.LiquidacionExcepcionArancelariaBLL = LiquidacionExcepcionArancelariaBLL;
}
