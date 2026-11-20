/**
 * Capa BE (Business Entities)
 * Entidad de Negocio: CalculadoraArancelariaBE
 * 
 * Cumple con el estándar académico estricto de objeto anémico / portador de datos:
 * - Contiene EXCLUSIVAMENTE atributos, constructor, propiedades y getters/setters.
 * - NO contiene métodos con algoritmos ni lógica de negocio (eso reside en CalculadoraArancelariaBLL).
 * - NO contiene métodos de acceso a datos ni persistencia (eso reside en CalculadoraArancelariaDAL).
 */
class CalculadoraArancelariaBE {
  constructor({
    // --- Atributos de Entrada / Parámetros Comerciales (RF01 / RF02) ---
    posicion = null,              // Instancia de PosicionArancelariaBE
    cantidad = 1,                 // Cantidad de unidades a adquirir
    precioFobUnitario = 0,        // Precio FOB unitario en USD
    pais = null,                  // Instancia de PaisBE (procedencia)
    flete = 0,                    // Flete internacional en USD
    seguro = 0,                   // Seguro internacional en USD
    esEstimacionFlete = false,    // Flag de cálculo supletorio paramétrico
    esEstimacionSeguro = false,   // Flag de cálculo supletorio legal 1% FOB
    esInscriptoGanancias = true,  // Condición impositiva: Responsable Inscripto (6%) vs No Cat (11%)
    aplicarExcepcion = true,      // Decisión sobre alícuota reducida (Dec. 557/2023)
    condicionAceptada = false,    // Aceptación de requisitos técnicos de excepción

    // --- Atributos de Salida / Liquidación Tributaria Resultante (RF03 / RF04) ---
    fobTotal = 0,                 // Valor FOB Total mercadería (USD)
    cifAduanero = 0,              // Base de valoración CIF aduanera (USD)
    tratamientoArancelario = '',  // Intrazona Mercosur vs Extrazona AEC / Excepción
    alicuotaDie = 0,              // Alícuota porcentual DIE (%)
    montoDie = 0,                 // Importe DIE liquidado (USD)
    alicuotaEstadistica = 0,      // Alícuota Tasa de Estadística (%)
    montoEstadistica = 0,         // Importe Tasa de Estadística con topes (USD)
    baseImponibleInterior = 0,    // Base de cálculo para IVA y percepciones interiores (USD)
    alicuotaIva = 21,             // Alícuota IVA general (21%) o reducida BK (10.5%)
    montoIva = 0,                 // Importe IVA DGA (USD)
    alicuotaIvaAdicional = 20,    // Alícuota Percepción IVA Adicional (20% o 10% BK)
    montoIvaAdicional = 0,        // Importe Percepción IVA Adicional (USD)
    alicuotaGanancias = 6,        // Alícuota Percepción Impuesto a las Ganancias (6% u 11%)
    montoGanancias = 0,           // Importe Percepción Ganancias (USD)
    arancelSim = 28,              // Cargo fijo Sistema Informático Malvina (USD)
    digitalizacionSim = 10,       // Cargo fijo digitalización documental (USD)
    totalSimDigitalizacion = 38,  // Total cargos fijos SIM + Digitalización (USD)
    totalGravamenes = 0,          // Sumatoria de tributos aduaneros (USD)
    costoTotalNacionalizado = 0,  // Desembolso final puesto en plaza (USD)
    costoUnitarioNacionalizado = 0,// Costo unitario puesto en plaza (USD)
    ratioSobrecostoPct = 0,       // % de incremento aduanero respecto al FOB inicial
    gravamenes = [],              // Lista de instancias de GravamenItemBE
    cotizacionBna = 1420.50,      // Tipo de cambio oficial BNA vendedor (USD a ARS)
    costoTotalARS = 0,            // Costo total nacionalizado expresado en Pesos Argentinos
    costoUnitarioARS = 0,         // Costo unitario nacionalizado en Pesos Argentinos
    fechaHora = new Date(),       // Marca temporal de la simulación
    hashCotizacion = ''           // Identificador alfanumérico único para trazabilidad
  } = {}) {
    this.posicion = posicion;
    this.cantidad = Math.max(1, parseInt(cantidad) || 1);
    this.precioFobUnitario = Math.max(0, parseFloat(precioFobUnitario) || 0);
    this.pais = pais;
    this.flete = Math.max(0, parseFloat(flete) || 0);
    this.seguro = Math.max(0, parseFloat(seguro) || 0);
    this.esEstimacionFlete = Boolean(esEstimacionFlete);
    this.esEstimacionSeguro = Boolean(esEstimacionSeguro);
    this.esInscriptoGanancias = Boolean(esInscriptoGanancias);
    this.aplicarExcepcion = Boolean(aplicarExcepcion);
    this.condicionAceptada = Boolean(condicionAceptada);

    this.fobTotal = parseFloat(fobTotal) || 0;
    this.cifAduanero = parseFloat(cifAduanero) || 0;
    this.tratamientoArancelario = tratamientoArancelario;
    this.alicuotaDie = parseFloat(alicuotaDie) || 0;
    this.montoDie = parseFloat(montoDie) || 0;
    this.alicuotaEstadistica = parseFloat(alicuotaEstadistica) || 0;
    this.montoEstadistica = parseFloat(montoEstadistica) || 0;
    this.baseImponibleInterior = parseFloat(baseImponibleInterior) || 0;
    this.alicuotaIva = parseFloat(alicuotaIva) || 0;
    this.montoIva = parseFloat(montoIva) || 0;
    this.alicuotaIvaAdicional = parseFloat(alicuotaIvaAdicional) || 0;
    this.montoIvaAdicional = parseFloat(montoIvaAdicional) || 0;
    this.alicuotaGanancias = parseFloat(alicuotaGanancias) || 0;
    this.montoGanancias = parseFloat(montoGanancias) || 0;
    this.arancelSim = parseFloat(arancelSim) || 28;
    this.digitalizacionSim = parseFloat(digitalizacionSim) || 10;
    this.totalSimDigitalizacion = parseFloat(totalSimDigitalizacion) || 38;
    this.totalGravamenes = parseFloat(totalGravamenes) || 0;
    this.costoTotalNacionalizado = parseFloat(costoTotalNacionalizado) || 0;
    this.costoUnitarioNacionalizado = parseFloat(costoUnitarioNacionalizado) || 0;
    this.ratioSobrecostoPct = parseFloat(ratioSobrecostoPct) || 0;
    this.gravamenes = gravamenes;
    this.cotizacionBna = parseFloat(cotizacionBna) || 1420.50;
    this.costoTotalARS = parseFloat(costoTotalARS) || 0;
    this.costoUnitarioARS = parseFloat(costoUnitarioARS) || 0;
    this.fechaHora = fechaHora;
    this.hashCotizacion = hashCotizacion;
  }
}

if (typeof window !== 'undefined') {
  window.CalculadoraArancelariaBE = CalculadoraArancelariaBE;
}
