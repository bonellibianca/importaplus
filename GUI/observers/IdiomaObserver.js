/**
 * Patrón de Diseño GoF: OBSERVER (Sujeto Observable + Observadores de Idioma ES / EN)
 * Compatible tanto con la arquitectura de 4 capas (index.html / window.IdiomaSubject)
 * como con los componentes React de Next.js (import { idiomaSubject } from ...).
 */

const DICCIONARIO_IDIOMAS = {
  es: {
    langCode: 'es',
    toggleBtn: 'Español · Cambiar a EN',
    toggleBadge: 'ES',
    heroSubtitle: 'Simulador oficial de costos aduaneros y tributos ARCA / DGA',
    btnCalcular: 'CALCULAR IMPORTACIÓN',
    calculando: 'Procesando liquidación...',
    paso1: 'Paso 1 de 2 · Parámetros Comerciales',
    paso1Corto: 'Paso 1 de 2',
    tituloLiquidacion: 'Liquidación Aduanera',
    subtituloForm: 'Completá los datos para calcular el costo oficial de nacionalización.',
    infoGeneral: 'Información general',
    lblProducto: 'Producto a Importar',
    phProducto: 'Escribí el nombre del producto (ej: Notebook, Remera, Router, Calzado)...',
    buscandoProducto: 'Buscando producto...',
    sinProductos: 'No se encontraron productos con ese nombre. Probá con otro término (ej: Notebook, Remera, Calzado, Teléfono).',
    arancel: 'Arancel',
    arancelBase: 'Arancel Base',
    prodSeleccionado: 'Producto seleccionado',
    verDesc: 'Ver descripción del producto ▼',
    ocultarDesc: 'Ocultar descripción del producto ▲',
    cambiar: 'Cambiar',
    beneficioDisponible: 'Beneficio arancelario disponible:',
    arancelReducido: 'Arancel reducido',
    lblPais: 'País de origen',
    phPais: 'Buscar país de origen...',
    mercosur: 'Mercosur',
    mercosurCero: 'Mercosur · 0% DIE',
    extrazona: 'Extrazona',
    lblCantidad: 'Cantidad (Unidades)',
    lblFobUnitario: 'Valor por unidad (USD FOB)',
    lblFobTotalVivo: 'Valor total de la mercadería (FOB Total)',
    unidAbrev: 'unid.',
    costosAdicionales: 'Costos adicionales',
    opcional: 'Opcional',
    btnContactarForwarder: 'Contactar Forwarder',
    btnContactarDespachante: 'Contactar Despachante de Aduana',
    lblFlete: 'Flete internacional (USD)',
    lblSeguro: 'Seguro de transporte (USD)',
    lblCondicionImpositiva: 'Condición Impositiva del Importador',
    condInscripto: 'Responsable Inscripto / Reventa',
    condConsumo: 'Consumo Particular / No categorizado',
    alertProducto: 'Por favor escribí y seleccioná un producto a importar.',
    alertPais: 'Por favor escribí y seleccioná un país de origen.',
    alertCantidad: 'La cantidad de unidades debe ser mayor a 0.',
    alertFob: 'El valor por unidad (USD FOB) debe ser mayor a 0.',
    nuevaConsulta: '← Nueva consulta',
    volverResultados: '← Volver a resultados',
    volverDesglose: '← Volver al desglose',
    volver: '← Volver',
    origen: 'Origen',
    condicion: 'Condición',
    unidadesA: 'unidades a',
    descProductoTitulo: 'Descripción del Producto',
    costoTotalAduana: 'Costo Total Estimado Puesto en Aduana',
    costoUnitarioNac: 'Costo unitario nacionalizado:',
    unidad: 'unidad',
    incidenciaAduanera: 'Incidencia Aduanera',
    sobreFob: 'sobre FOB',
    compraFob: 'Compra FOB',
    desembolsoFinal: 'Desembolso Final',
    card1Titulo: '1. Valor Compra FOB Total',
    card1Cif: 'Valor CIF Aduanero',
    card2Titulo: '2. Impuestos & Tasas Aduaneras',
    card2Incidencia: 'Incidencia fiscal sobre compra FOB:',
    card3Titulo: '3. Costo Final Nacionalizado',
    card3Tc: 'TC Oficial BNA',
    tablaTitulo: 'Liquidación Analítica Detallada de Gravámenes Oficiales',
    thConcepto: 'Concepto Tributario',
    thAlicuota: 'Alícuota Aplicada',
    thImporte: 'Importe en USD',
    btnVerDesglose: 'VER DESGLOSE',
    btnExportarExcel: 'Exportar Planilla Excel',
    btnImprimirPdf: 'Imprimir / PDF',
    btnImprimirDesglose: 'Imprimir Desglose',
    desgloseSubtitulo: 'Liquidación Aduanera · Desglose Oficial',
    costoTotalNac: 'Costo Total Nacionalizado',
    preguntaPuertos: '¿Querés consultar los puntos aduaneros habilitados para este origen?',
    verPuertos: 'Ver Puertos y Aduanas',
    ocultarPuertos: 'Ocultar Puertos y Aduanas',
    pn3Tag: 'Logística Aduanera Argentina',
    puertosTitulo: 'Puertos y Aduanas Principales',
    puertosSugeridosPara: 'Puertos y Aduanas Sugeridos para',
    puertosSubtitulo: 'Puntos de ingreso habilitados para nacionalización de mercaderías.',
    puntosHabilitados: 'Puntos Habilitados DGA',
    // Sección Forwarders (4 cuadrados estilo grilla)
    fwdTag: 'Directorio Oficial de Agentes de Carga',
    fwdTitulo: 'Forwarders Internacionales Habilitados',
    fwdSubtitulo: 'Conectate directo por WhatsApp o copiá el correo electrónico oficial del Forwarder.',
    // Sección Despachantes de Aduana (misma interfaz grilla de 4 cuadrados)
    despTag: 'Directorio Oficial de Despachantes de Aduana',
    despTitulo: 'Despachantes de Aduana Matriculados',
    despSubtitulo: 'Conectate directo por WhatsApp o copiá el correo electrónico oficial del Despachante de Aduana.',
    btnWhatsapp: 'WhatsApp',
    btnCopiarMail: 'Copiar Mail',
    mailCopiado: '¡Mail copiado!',
    waMensajeDefault: 'Hola, me contacto desde IMPORTA+ para cotizar un flete internacional.',
    waMensajeDespachante: 'Hola, me contacto desde IMPORTA+ para consultar por despacho aduanero de importación.',
    menuTrabajaConNosotros: 'Trabaja con nosotros',
    waMensajeTrabaja: 'Hola IMPORTA+, me contacto porque quiero trabajar con ustedes.',
    verEnGoogleMaps: 'Abrir en Google Maps',
    // Conceptos de desglose
    itemFob: 'Valor total del producto (FOB)',
    noteFob: 'Precio de compra en puerto de origen',
    itemFlete: 'Flete internacional',
    noteFlete: 'Transporte internacional de origen a aduana',
    itemSeguro: 'Seguro de transporte',
    noteSeguro: 'Póliza de cobertura sobre la mercadería',
    itemCif: 'Valor Base (CIF)',
    noteCif: 'Base Imponible de Valoración DGA',
    itemDie: 'Derecho de Importación DIE',
    itemEstadistica: 'Tasa de Estadística',
    noteEstadisticaExento: 'Exento Mercosur / Bien de Capital',
    noteEstadisticaGral: 'Tasa estadística aduanera sobre valor CIF',
    itemIva: 'IVA General',
    noteIva: 'Aplicado sobre Base Imponible Interior',
    itemIvaAdic: 'Percepción IVA Adicional',
    noteIvaAdic: 'Percepción aduanera de IVA (RG 2281)',
    itemGanancias: 'Percepción Ganancias',
    itemSim: 'Arancel SIM + Digitalización',
    noteSim: 'USD 28 Malvina + USD 10 Digitalización',
    impuestosDga: 'Impuestos DGA',
    cargosSimEst: 'Cargos SIM + Tasa Estadística'
  },
  en: {
    langCode: 'en',
    toggleBtn: 'English · Switch to ES',
    toggleBadge: 'EN',
    heroSubtitle: 'Official Argentine Customs Cost & Tax Simulator (ARCA / DGA)',
    btnCalcular: 'CALCULATE IMPORT',
    calculando: 'Calculating customs duties...',
    paso1: 'Step 1 of 2 · Commercial Parameters',
    paso1Corto: 'Step 1 of 2',
    tituloLiquidacion: 'Customs Duty Calculator',
    subtituloForm: 'Enter your shipment details to calculate official landed import costs.',
    infoGeneral: 'General information',
    lblProducto: 'Product to Import',
    phProducto: 'Type product name (e.g. Notebook, T-shirt, Router, Footwear)...',
    buscandoProducto: 'Searching product...',
    sinProductos: 'No matching products found. Try another keyword (e.g. Notebook, Router, Footwear).',
    arancel: 'Tariff',
    arancelBase: 'Base Tariff',
    prodSeleccionado: 'Selected product',
    verDesc: 'Show product description ▼',
    ocultarDesc: 'Hide product description ▲',
    cambiar: 'Change',
    beneficioDisponible: 'Tariff exemption available:',
    arancelReducido: 'Reduced tariff',
    lblPais: 'Country of origin',
    phPais: 'Search country of origin...',
    mercosur: 'Mercosur',
    mercosurCero: 'Mercosur · 0% Duty',
    extrazona: 'Non-Mercosur',
    lblCantidad: 'Quantity (Units)',
    lblFobUnitario: 'Unit Value (USD FOB)',
    lblFobTotalVivo: 'Total Merchandise Value (Total FOB)',
    unidAbrev: 'units',
    costosAdicionales: 'Additional logistics costs',
    opcional: 'Optional',
    btnContactarForwarder: 'Contact Forwarder',
    btnContactarDespachante: 'Contact Customs Broker',
    lblFlete: 'International freight (USD)',
    lblSeguro: 'Transport insurance (USD)',
    lblCondicionImpositiva: 'Importer Tax Status',
    condInscripto: 'Registered VAT / Commercial Resale',
    condConsumo: 'Private Use / Uncategorized',
    alertProducto: 'Please type and select a product to import.',
    alertPais: 'Please type and select a country of origin.',
    alertCantidad: 'Unit quantity must be greater than 0.',
    alertFob: 'Unit FOB value (USD) must be greater than 0.',
    nuevaConsulta: '← New calculation',
    volverResultados: '← Back to summary',
    volverDesglose: '← Back to breakdown',
    volver: '← Back',
    origen: 'Origin',
    condicion: 'Tax Status',
    unidadesA: 'units at',
    descProductoTitulo: 'Product Description',
    costoTotalAduana: 'Estimated Total Landed Cost at Customs',
    costoUnitarioNac: 'Landed unit cost:',
    unidad: 'unit',
    incidenciaAduanera: 'Customs Tax Incidence',
    sobreFob: 'over FOB',
    compraFob: 'FOB Purchase',
    desembolsoFinal: 'Total Landed Cost',
    card1Titulo: '1. Total FOB Purchase Value',
    card1Cif: 'Customs CIF Value',
    card2Titulo: '2. Customs Duties & Taxes',
    card2Incidencia: 'Tax incidence over FOB purchase:',
    card3Titulo: '3. Final Landed Cost',
    card3Tc: 'Official BNA FX Rate',
    tablaTitulo: 'Detailed Official Customs Duties & Tax Breakdown',
    thConcepto: 'Tax / Duty Concept',
    thAlicuota: 'Applied Rate',
    thImporte: 'Amount (USD)',
    btnVerDesglose: 'VIEW FULL BREAKDOWN',
    btnExportarExcel: 'Export Excel Sheet',
    btnImprimirPdf: 'Print / PDF',
    btnImprimirDesglose: 'Print Breakdown',
    desgloseSubtitulo: 'Customs Settlement · Official Breakdown',
    costoTotalNac: 'Total Landed Cost',
    preguntaPuertos: 'Want to check authorized customs ports for this origin?',
    verPuertos: 'View Ports & Customs',
    ocultarPuertos: 'Hide Ports & Customs',
    pn3Tag: 'Argentine Customs Logistics',
    puertosTitulo: 'Main Ports & Customs Offices',
    puertosSugeridosPara: 'Recommended Ports & Customs for',
    puertosSubtitulo: 'Authorized entry terminals for merchandise clearance in Argentina.',
    puntosHabilitados: 'DGA Authorized Terminals',
    // Sección Forwarders (4 cuadrados estilo grilla)
    fwdTag: 'Official Freight Forwarders Directory',
    fwdTitulo: 'Authorized International Forwarders',
    fwdSubtitulo: 'Chat directly via WhatsApp or copy the Forwarder’s official email address.',
    // Sección Despachantes de Aduana
    despTag: 'Official Customs Brokers Directory',
    despTitulo: 'Authorized Customs Brokers',
    despSubtitulo: 'Chat directly via WhatsApp or copy the Customs Broker’s official email address.',
    btnWhatsapp: 'WhatsApp',
    btnCopiarMail: 'Copy Email',
    mailCopiado: 'Email copied!',
    waMensajeDefault: 'Hello, I am reaching out from IMPORTA+ to request an international freight quote.',
    waMensajeDespachante: 'Hello, I am reaching out from IMPORTA+ to inquire about customs clearance services.',
    menuTrabajaConNosotros: 'Work with us',
    waMensajeTrabaja: 'Hello IMPORTA+, I am reaching out because I would like to work with you.',
    verEnGoogleMaps: 'Open in Google Maps',
    // Conceptos de desglose
    itemFob: 'Total merchandise value (FOB)',
    noteFob: 'Purchase price at port of origin',
    itemFlete: 'International freight',
    noteFlete: 'International transport from origin to customs',
    itemSeguro: 'Transport insurance',
    noteSeguro: 'Cargo insurance policy coverage',
    itemCif: 'Customs Base Value (CIF)',
    noteCif: 'DGA Customs Valuation Base',
    itemDie: 'Import Duty (DIE)',
    itemEstadistica: 'Statistical Fee',
    noteEstadisticaExento: 'Exempt (Mercosur / Capital Good)',
    noteEstadisticaGral: 'Customs statistical fee on CIF value',
    itemIva: 'General VAT (IVA)',
    noteIva: 'Applied on Domestic Taxable Base',
    itemIvaAdic: 'Additional VAT Withholding',
    noteIvaAdic: 'Customs VAT perception (RG 2281)',
    itemGanancias: 'Income Tax Withholding',
    itemSim: 'SIM Fee + Document Digitization',
    noteSim: 'USD 28 Malvina System + USD 10 Digitization',
    impuestosDga: 'DGA Domestic Taxes',
    cargosSimEst: 'SIM Fees + Statistical Tax'
  }
};

/**
 * Interfaz Base Observador (Patrón Observer GoF)
 */
class IdiomaObserver {
  actualizarIdioma(idioma, diccionario) {
    throw new Error('El método actualizarIdioma() debe ser implementado por el Observador concreto.');
  }
}

/**
 * Sujeto Observable (Subject) que administra el idioma activo ('es' | 'en')
 * y notifica en tiempo real a todos los observadores suscritos en la interfaz.
 */
class IdiomaSubject {
  constructor() {
    this.idiomaActual = 'es';
    this.observadores = new Set();

    if (typeof window !== 'undefined') {
      try {
        localStorage.removeItem('IMPORTA_PLUS_IDIOMA');
        const guardado = sessionStorage.getItem('IMPORTA_PLUS_IDIOMA');
        if (guardado === 'es' || guardado === 'en') {
          this.idiomaActual = guardado;
        }
        document.documentElement.lang = this.idiomaActual;
      } catch {}
    }
  }

  /**
   * Suscribe un observador concreto (objeto con actualizarIdioma o callback).
   */
  suscribir(observador) {
    if (!observador) return () => {};
    this.observadores.add(observador);
    return () => this.desuscribir(observador);
  }

  /**
   * Desuscribe un observador previamente registrado.
   */
  desuscribir(observador) {
    this.observadores.delete(observador);
  }

  /**
   * Notifica a todos los observadores registrados sobre el cambio de idioma.
   */
  notificar() {
    const dicc = this.obtenerDiccionario();
    this.observadores.forEach((obs) => {
      try {
        if (typeof obs === 'function') {
          obs(this.idiomaActual, dicc);
        } else if (obs && typeof obs.actualizarIdioma === 'function') {
          obs.actualizarIdioma(this.idiomaActual, dicc);
        }
      } catch (err) {
        console.warn('[IdiomaSubject] Error notificando observador:', err);
      }
    });
  }

  cambiarIdioma(nuevoIdioma) {
    if (nuevoIdioma !== 'es' && nuevoIdioma !== 'en') return;
    if (this.idiomaActual === nuevoIdioma) return;

    this.idiomaActual = nuevoIdioma;
    if (typeof window !== 'undefined') {
      try {
        sessionStorage.setItem('IMPORTA_PLUS_IDIOMA', nuevoIdioma);
        document.documentElement.lang = nuevoIdioma;
      } catch {}
    }
    this.notificar();
  }

  alternarIdioma() {
    this.cambiarIdioma(this.idiomaActual === 'es' ? 'en' : 'es');
  }

  obtenerIdioma() {
    return this.idiomaActual;
  }

  obtenerDiccionario() {
    return DICCIONARIO_IDIOMAS[this.idiomaActual] || DICCIONARIO_IDIOMAS.es;
  }

  t(clave) {
    const dicc = this.obtenerDiccionario();
    return dicc[clave] !== undefined ? dicc[clave] : (DICCIONARIO_IDIOMAS.es[clave] || clave);
  }
}

const instanciaGlobalIdioma = (typeof window !== 'undefined' && window.idiomaSubject)
  ? window.idiomaSubject
  : new IdiomaSubject();

if (typeof window !== 'undefined') {
  window.IdiomaObserver = IdiomaObserver;
  window.IdiomaSubject = IdiomaSubject;
  window.idiomaSubject = instanciaGlobalIdioma;
}

if (typeof module !== 'undefined' && module.exports) {
  module.exports = {
    IdiomaObserver,
    IdiomaSubject,
    idiomaSubject: instanciaGlobalIdioma,
    DICCIONARIO_IDIOMAS
  };
}
