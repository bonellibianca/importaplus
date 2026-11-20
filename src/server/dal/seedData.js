import 'server-only';

/**
 * Catálogo Semilla de Respaldo Local (In-Memory Fallback para Persistencia en Excel)
 * Asegura que el buscador <250ms y el motor de simulación funcionen de inmediato
 * tanto leyendo los libros de Excel (.xlsx / .csv) como en entornos Serverless.
 */

export const FALLBACK_NCM = [
  {
    codigo_ncm: '8471.30.12',
    descripcion: 'Máquinas automáticas para tratamiento o procesamiento de datos, portátiles, de peso inferior o igual a 350 g, con teclado y pantalla (Notebooks / Laptops)',
    tipo_bien: 'BIT',
    aec: 16.00,
    iva_alicuota: 10.50,
    anexo_excepcion: 'Decreto 557/2023 - Anexo I',
    arancel_excepcion: 2.00,
    condicion_excepcion: 'Tasa preferencial 2% cumpliendo especificaciones técnicas BIT'
  },
  {
    codigo_ncm: '8517.12.31',
    descripcion: 'Teléfonos móviles (celulares) y los de otras redes inalámbricas digitales',
    tipo_bien: 'BIT',
    aec: 16.00,
    iva_alicuota: 21.00,
    anexo_excepcion: null,
    arancel_excepcion: null,
    condicion_excepcion: null
  },
  {
    codigo_ncm: '6109.10.00',
    descripcion: 'T-shirts y camisetas de punto, de algodón para hombres o mujeres',
    tipo_bien: 'GENERAL',
    aec: 35.00,
    iva_alicuota: 21.00,
    anexo_excepcion: null,
    arancel_excepcion: null,
    condicion_excepcion: 'Sujeto a verificación de composición textil'
  },
  {
    codigo_ncm: '6402.19.00',
    descripcion: 'Calzado deportivo con suela y parte superior de caucho o plástico (Zapatillas running / urbanas)',
    tipo_bien: 'GENERAL',
    aec: 35.00,
    iva_alicuota: 21.00,
    anexo_excepcion: null,
    arancel_excepcion: null,
    condicion_excepcion: 'Sujeto a certificación de seguridad de calzado'
  },
  {
    codigo_ncm: '8471.41.00',
    descripcion: 'Unidades de procesamiento central integradas (Servidores / Mini PC para centros de datos)',
    tipo_bien: 'BK',
    aec: 16.00,
    iva_alicuota: 10.50,
    anexo_excepcion: 'Decreto 557/2023 - BK',
    arancel_excepcion: 0.00,
    condicion_excepcion: 'Exento de DIE bajo régimen de Bienes de Capital y 10.5% IVA'
  },
  {
    codigo_ncm: '9503.00.99',
    descripcion: 'Los demás juguetes; modelos reducidos y modelos similares para entretenimiento; rompecabezas de cualquier tipo',
    tipo_bien: 'GENERAL',
    aec: 35.00,
    iva_alicuota: 21.00,
    anexo_excepcion: null,
    arancel_excepcion: null,
    condicion_excepcion: 'Certificado de seguridad del juguete Res. 163/2005'
  },
  {
    codigo_ncm: '8708.29.99',
    descripcion: 'Partes y accesorios de carrocerías para vehículos automóviles (Autopartes y repuestos)',
    tipo_bien: 'GENERAL',
    aec: 18.00,
    iva_alicuota: 21.00,
    anexo_excepcion: null,
    arancel_excepcion: null,
    condicion_excepcion: 'Homologación técnica CHAS requerida'
  },
  {
    codigo_ncm: '8518.30.00',
    descripcion: 'Auriculares, incluidos los de casco, estén o no combinados con micrófono (Auriculares inalámbricos Bluetooth)',
    tipo_bien: 'BIT',
    aec: 18.00,
    iva_alicuota: 21.00,
    anexo_excepcion: null,
    arancel_excepcion: null,
    condicion_excepcion: 'Certificado de seguridad eléctrica ENACOM'
  },
  {
    codigo_ncm: '9004.10.00',
    descripcion: 'Gafas (anteojos) de sol con filtro UV de protección visual',
    tipo_bien: 'GENERAL',
    aec: 20.00,
    iva_alicuota: 21.00,
    anexo_excepcion: null,
    arancel_excepcion: null,
    condicion_excepcion: 'Control ANMAT y filtro UV'
  },
  {
    codigo_ncm: '3926.90.90',
    descripcion: 'Las demás manufacturas de plástico y materias de las partidas 39.01 a 39.14 (Accesorios plásticos universales)',
    tipo_bien: 'GENERAL',
    aec: 16.00,
    iva_alicuota: 21.00,
    anexo_excepcion: null,
    arancel_excepcion: null,
    condicion_excepcion: null
  }
];

export const FALLBACK_PAISES = [
  { codigo_iso: 'CHN', nombre: 'China (Extrazona)', es_mercosur: false, flete_estimado_pct: 10.00 },
  { codigo_iso: 'USA', nombre: 'Estados Unidos (Extrazona)', es_mercosur: false, flete_estimado_pct: 8.00 },
  { codigo_iso: 'BRA', nombre: 'Brasil (Mercosur - Intrazona)', es_mercosur: true, flete_estimado_pct: 5.00 },
  { codigo_iso: 'DEU', nombre: 'Alemania (Extrazona)', es_mercosur: false, flete_estimado_pct: 9.00 },
  { codigo_iso: 'ITA', nombre: 'Italia (Extrazona)', es_mercosur: false, flete_estimado_pct: 9.00 },
  { codigo_iso: 'VNM', nombre: 'Vietnam (Extrazona)', es_mercosur: false, flete_estimado_pct: 11.00 },
  { codigo_iso: 'ESP', nombre: 'España (Extrazona)', es_mercosur: false, flete_estimado_pct: 8.50 },
  { codigo_iso: 'URY', nombre: 'Uruguay (Mercosur - Intrazona)', es_mercosur: true, flete_estimado_pct: 4.00 }
];

export const FALLBACK_PUERTOS = [
  {
    id: '11111111-1111-1111-1111-111111111111',
    nombre: 'Puerto de Buenos Aires',
    via_ingreso: 'MARITIMA_FLUVIAL',
    provincia: 'Ciudad Autónoma de Buenos Aires',
    codigo_sim: 'ADU-001-PBA',
    operador: 'AGPSE (Administración General de Puertos)',
    tipos_carga: ['FCL', 'LCL', 'GENERAL', 'REFRIGERADA'],
    infraestructura: 'Terminales 1, 2, 3, 4 y 5 (Puerto Nuevo). Escáneres de alta penetración no intrusivos.',
    latitud: -34.5985,
    longitud: -58.3731,
    paises_frecuentes: ['CHN', 'USA', 'DEU', 'ITA', 'ESP', 'BRA']
  },
  {
    id: '22222222-2222-2222-2222-222222222222',
    nombre: 'Aduana de Ezeiza',
    via_ingreso: 'AEREA',
    provincia: 'Buenos Aires',
    codigo_sim: 'ADU-073-EZE',
    operador: 'ARCA-DGA / Aeropuertos Argentina 2000',
    tipos_carga: ['GENERAL', 'COURIER', 'VALOR', 'PERECEDERA'],
    infraestructura: 'Terminal de Cargas Aéreas (TCA) con cámaras frigoríficas y depósito de alta seguridad.',
    latitud: -34.8222,
    longitud: -58.5358,
    paises_frecuentes: ['USA', 'CHN', 'DEU', 'ESP', 'ITA']
  },
  {
    id: '33333333-3333-3333-3333-333333333333',
    nombre: 'Puerto de Rosario',
    via_ingreso: 'MARITIMA_FLUVIAL',
    provincia: 'Santa Fe',
    codigo_sim: 'ADU-052-ROS',
    operador: 'Ente Administrador Puerto Rosario (ENAPRO)',
    tipos_carga: ['FCL', 'LCL', 'GRANOS', 'INDUSTRIAL'],
    infraestructura: 'Muelle de aguas profundas sobre la Hidrovía Paraná-Paraguay. Conexión ferroviaria directa.',
    latitud: -32.9468,
    longitud: -60.6393,
    paises_frecuentes: ['BRA', 'URY', 'CHN', 'USA']
  },
  {
    id: '44444444-4444-4444-4444-444444444444',
    nombre: 'Paso Cristo Redentor',
    via_ingreso: 'TERRESTRE',
    provincia: 'Mendoza',
    codigo_sim: 'ADU-042-PCR',
    operador: 'Gendarmería Nacional / ARCA-DGA',
    tipos_carga: ['FCL', 'GENERAL', 'CARGAS PELIGROSAS'],
    infraestructura: 'Complejo Fronterizo Los Horcones (Argentina) - Los Libertadores (Chile). Corredor Bioceánico.',
    latitud: -32.8353,
    longitud: -70.0649,
    paises_frecuentes: ['BRA', 'URY']
  },
  {
    id: '55555555-5555-5555-5555-555555555555',
    nombre: 'Puerto de Mar del Plata',
    via_ingreso: 'MARITIMA_FLUVIAL',
    provincia: 'Buenos Aires',
    codigo_sim: 'ADU-038-MDP',
    operador: 'Consorcio Portuario Regional Mar del Plata',
    tipos_carga: ['FCL', 'REFRIGERADA', 'PESQUERA'],
    infraestructura: 'Terminal de contenedores y carga frigorífica para industria textil, pesquera e industrial regional.',
    latitud: -38.0055,
    longitud: -57.5426,
    paises_frecuentes: ['ESP', 'CHN', 'USA', 'BRA']
  }
];

export const FALLBACK_OPERADORES = [
  {
    id: 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa',
    razon_social: 'Bonelli & Asociados Comercio Exterior',
    tipo_operador: 'DESPACHANTE',
    matricula_dga: 'DGA-48921-B',
    email_notificacion: 'contacto@importaplus.com.ar',
    telefono: '+54 11 4321-8800',
    rubros_especialidad: ['Tecnología', 'Electrónica', 'Bienes de Capital', 'Textil'],
    aduanas_operativas: ['Buenos Aires', 'Ezeiza', 'Rosario'],
    modalidades: ['FCL', 'LCL', 'Courier'],
    experiencia_anios: 12,
    rating: 4.95,
    activo: true
  },
  {
    id: 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb',
    razon_social: 'Importa+ Global Forwarding',
    tipo_operador: 'FORWARDER',
    matricula_dga: 'FWD-0842-AR',
    email_notificacion: 'operaciones@importaplus.com.ar',
    telefono: '+54 11 5239-1100',
    rubros_especialidad: ['Carga General', 'Maquinaria', 'Repuestos Automotrices'],
    aduanas_operativas: ['Buenos Aires', 'Rosario', 'Paso Cristo Redentor'],
    modalidades: ['FCL', 'LCL', 'Consolidado Marítimo'],
    experiencia_anios: 15,
    rating: 4.90,
    activo: true
  },
  {
    id: 'cccccccc-cccc-cccc-cccc-cccccccccccc',
    razon_social: 'Ezeiza Express Brokerage & Logistics',
    tipo_operador: 'DESPACHANTE',
    matricula_dga: 'DGA-31205-E',
    email_notificacion: 'ezeiza@importaplus.com.ar',
    telefono: '+54 11 4480-2233',
    rubros_especialidad: ['Courier Urgente', 'Tecnología', 'Farmacéutica', 'Muestras'],
    aduanas_operativas: ['Ezeiza'],
    modalidades: ['Aéreo', 'Courier', 'Carga Crítica'],
    experiencia_anios: 8,
    rating: 4.85,
    activo: true
  },
  {
    id: 'dddddddd-dddd-dddd-dddd-dddddddddddd',
    razon_social: 'Andina Cargas Internacionales',
    tipo_operador: 'FORWARDER',
    matricula_dga: 'FWD-1290-CUYO',
    email_notificacion: 'mendoza@importaplus.com.ar',
    telefono: '+54 261 420-5544',
    rubros_especialidad: ['Terrestre Mercosur', 'Agroindustria', 'Repuestos'],
    aduanas_operativas: ['Paso Cristo Redentor', 'Rosario'],
    modalidades: ['Terrestre FTL/LTL', 'Multimodal'],
    experiencia_anios: 18,
    rating: 4.80,
    activo: true
  }
];
