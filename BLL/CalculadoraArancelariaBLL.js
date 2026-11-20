/**
 * Capa BLL (Business Logic Layer)
 * Lógica de Negocio: CalculadoraArancelariaBLL
 * 
 * Actúa como Coordinador del Servicio de Liquidación Aduanera.
 * Aplica el Patrón GoF TEMPLATE METHOD seleccionando la estrategia/plantilla
 * regulatoria correspondiente (Intrazona, Extrazona General o Excepción Arancelaria)
 * y delegando la ejecución secuencial normada.
 */
class CalculadoraArancelariaBLL {
  constructor(calculadoraDAL = null) {
    const DALClass = typeof window !== 'undefined' ? window.CalculadoraArancelariaDAL : null;
    this.calculadoraDAL = calculadoraDAL || (DALClass ? new DALClass() : null);
  }

  /**
   * Valida que la entidad portadora contenga los parámetros obligatorios mínimos (RF02).
   * @param {CalculadoraArancelariaBE} be 
   */
  validar(be) {
    if (!be) {
      throw new Error('La entidad CalculadoraArancelariaBE es nula o indefinida.');
    }
    if (!be.posicion) {
      throw new Error('Debe seleccionar una posición arancelaria oficial (NCM).');
    }
    if (!be.pais) {
      throw new Error('Debe especificar el país de origen/procedencia.');
    }
    if (be.cantidad <= 0) {
      throw new Error('La cantidad de unidades debe ser mayor a cero.');
    }
    if (be.precioFobUnitario <= 0) {
      throw new Error('El precio FOB unitario en USD debe ser mayor a cero.');
    }
    return true;
  }

  /**
   * Selecciona el calculador concreto según la normativa aplicable (Patrón Template Method)
   * @param {CalculadoraArancelariaBE} be 
   * @returns {CalculadorAduaneroTemplate}
   */
  obtenerTemplateLiquidacion(be) {
    if (be.pais.esIntrazona) {
      const IntrazonaClass = typeof window !== 'undefined' ? window.LiquidacionIntrazonaBLL : null;
      return IntrazonaClass ? new IntrazonaClass() : null;
    }
    if (be.aplicarExcepcion && be.posicion.poseeExcepcion) {
      const ExcepcionClass = typeof window !== 'undefined' ? window.LiquidacionExcepcionArancelariaBLL : null;
      return ExcepcionClass ? new ExcepcionClass() : null;
    }
    const GeneralClass = typeof window !== 'undefined' ? window.LiquidacionExtrazonaGeneralBLL : null;
    return GeneralClass ? new GeneralClass() : null;
  }

  /**
   * Ejecuta el algoritmo tributario aduanero oficial argentino utilizando el Template Method (RF03).
   * @param {CalculadoraArancelariaBE} be 
   * @returns {Promise<CalculadoraArancelariaBE>}
   */
  async liquidar(be) {
    this.validar(be);

    // 1. Obtener parámetros regulatorios de la DAL
    const parametros = await this.calculadoraDAL.obtenerParametrosRegulatorios();

    // 2. Resolver y ejecutar el Template Method según el régimen del bien y origen
    const template = this.obtenerTemplateLiquidacion(be);
    if (!template) {
      throw new Error('No se encontró una plantilla de liquidación compatible con el régimen aduanero.');
    }

    // Ejecuta el flujo invariable con pasos hooks
    template.liquidarTemplate(be, parametros);

    // 3. Persistir en la DAL para trazabilidad
    await this.calculadoraDAL.guardarSimulacion(be);
    return be;
  }
}

if (typeof window !== 'undefined') {
  window.CalculadoraArancelariaBLL = CalculadoraArancelariaBLL;
}
