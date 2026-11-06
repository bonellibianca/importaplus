-- ==============================================================================
-- PROYECTO: IMPORTA+ (IMPORTAPLUS.COM.AR) - BIANCA BONELLI
-- SUPABASE (POSTGRESQL 15+)
-- Cumple con: RF01  RF02, RF03, PN1, PN2, PN3, PN4 y RLS
-- ==============================================================================

-- 1. Extensión para búsqueda incremental ultra rápida (< 250ms) por nombre de producto
CREATE EXTENSION IF NOT EXISTS pg_trgm;

-- 2. Tabla de Posiciones Arancelarias (NCM)
CREATE TABLE IF NOT EXISTS public.ncm_catalogo (
    codigo_ncm VARCHAR(15) PRIMARY KEY, -- Ej: '8471.30.12'
    descripcion TEXT NOT NULL,
    tipo_bien VARCHAR(10) DEFAULT 'GENERAL', -- 'BK' (Bien de Capital), 'BIT' (Informática/Telecom), 'GENERAL'
    aec NUMERIC(5,2) NOT NULL DEFAULT 0, -- Arancel Externo Común (%)
    iva_alicuota NUMERIC(5,2) NOT NULL DEFAULT 21.00, -- 21% o 10.5% (reducida para BK/BIT)
    anexo_excepcion VARCHAR(100),
    arancel_excepcion NUMERIC(5,2),
    condicion_excepcion TEXT,
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT NOW()
);

-- Índice GIN con trigramas para búsqueda predictiva incremental en texto libre (< 250ms)
CREATE INDEX IF NOT EXISTS idx_ncm_descripcion_trgm 
ON public.ncm_catalogo USING GIN (descripcion gin_trgm_ops);

-- Índice para búsqueda por código NCM
CREATE INDEX IF NOT EXISTS idx_ncm_codigo 
ON public.ncm_catalogo (codigo_ncm);

-- 3. Tabla de Países y Estimación Paramétrica de Flete (RF02.d)
CREATE TABLE IF NOT EXISTS public.paises (
    codigo_iso VARCHAR(3) PRIMARY KEY, -- Ej: 'CHN', 'USA', 'BRA'
    nombre VARCHAR(100) NOT NULL,
    es_mercosur BOOLEAN DEFAULT FALSE,
    flete_estimado_pct NUMERIC(5,2) DEFAULT 8.00 -- % supletorio sobre FOB si el usuario no ingresa flete
);

-- 4. Tabla de Puertos y Aduanas (PN3)
CREATE TABLE IF NOT EXISTS public.puertos_aduanas (
    id UUID DEFAULT gen_random_uuid() PRIMARY KEY,
    nombre VARCHAR(150) NOT NULL,
    via_ingreso VARCHAR(50) NOT NULL, -- 'MARITIMA_FLUVIAL', 'AEREA', 'TERRESTRE'
    provincia VARCHAR(100) NOT NULL,
    codigo_sim VARCHAR(20),
    operador VARCHAR(150),
    tipos_carga TEXT[], -- ['FCL', 'LCL', 'GENERAL', 'REFRIGERADA']
    infraestructura TEXT,
    latitud NUMERIC(10,6),
    longitud NUMERIC(10,6),
    paises_frecuentes VARCHAR(3)[] -- Códigos ISO de países que suelen operar por aquí
);

-- 5. Tabla de Despachantes y Forwarders (PN4)
CREATE TABLE IF NOT EXISTS public.operadores_logisticos (
    id UUID DEFAULT gen_random_uuid() PRIMARY KEY,
    razon_social VARCHAR(150) NOT NULL,
    tipo_operador VARCHAR(50) NOT NULL, -- 'DESPACHANTE', 'FORWARDER', 'INTEGRAL'
    matricula_dga VARCHAR(50),
    email_notificacion VARCHAR(150) NOT NULL,
    telefono VARCHAR(50),
    rubros_especialidad TEXT[], -- ['Tecnología', 'Indumentaria', 'Repuestos', 'Alimentos']
    aduanas_operativas TEXT[], -- ['Buenos Aires', 'Ezeiza', 'Rosario', 'Paso Cristo Redentor']
    modalidades TEXT[], -- ['FCL', 'LCL', 'Courier']
    experiencia_anios INT DEFAULT 5,
    rating NUMERIC(3,2) DEFAULT 5.00,
    activo BOOLEAN DEFAULT TRUE,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT NOW()
);

-- 6. Tabla de Simulaciones Emitidas (PN1 / PN2 - Trazabilidad Inmutable con Hash ID)
CREATE TABLE IF NOT EXISTS public.simulaciones (
    id_cotizacion VARCHAR(20) PRIMARY KEY, -- Hash único ej: IMP-2026-8F9A2B
    codigo_ncm VARCHAR(15) REFERENCES public.ncm_catalogo(codigo_ncm),
    pais_origen VARCHAR(3) REFERENCES public.paises(codigo_iso),
    cantidad INT NOT NULL,
    valor_fob_unitario NUMERIC(12,2) NOT NULL,
    valor_fob_total NUMERIC(14,2) NOT NULL,
    flete_usd NUMERIC(12,2) NOT NULL,
    seguro_usd NUMERIC(12,2) NOT NULL,
    valor_cif_usd NUMERIC(14,2) NOT NULL,
    desglose_impuestos JSONB NOT NULL,
    total_gravamenes_usd NUMERIC(14,2) NOT NULL,
    costo_total_nacionalizado_usd NUMERIC(14,2) NOT NULL,
    costo_unitario_final_usd NUMERIC(12,2) NOT NULL,
    cotizacion_bna_ars NUMERIC(12,2),
    created_at TIMESTAMP WITH TIME ZONE DEFAULT NOW()
);

-- 7. Tabla de Leads / Solicitudes de Contacto (PN4)
CREATE TABLE IF NOT EXISTS public.solicitudes_contacto (
    id UUID DEFAULT gen_random_uuid() PRIMARY KEY,
    operador_id UUID REFERENCES public.operadores_logisticos(id),
    id_cotizacion VARCHAR(20) REFERENCES public.simulaciones(id_cotizacion),
    nombre_apellido VARCHAR(150) NOT NULL,
    empresa VARCHAR(150),
    email VARCHAR(150) NOT NULL,
    telefono VARCHAR(50) NOT NULL,
    comentarios TEXT,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT NOW()
);

-- ==============================================================================
-- 8. SEGURIDAD: ROW LEVEL SECURITY (RLS)
-- ==============================================================================
ALTER TABLE public.ncm_catalogo ENABLE ROW LEVEL SECURITY;
ALTER TABLE public.paises ENABLE ROW LEVEL SECURITY;
ALTER TABLE public.puertos_aduanas ENABLE ROW LEVEL SECURITY;
ALTER TABLE public.operadores_logisticos ENABLE ROW LEVEL SECURITY;
ALTER TABLE public.simulaciones ENABLE ROW LEVEL SECURITY;
ALTER TABLE public.solicitudes_contacto ENABLE ROW LEVEL SECURITY;

-- Políticas de lectura pública para catálogos no sensibles
DROP POLICY IF EXISTS "Lectura pública NCM" ON public.ncm_catalogo;
CREATE POLICY "Lectura pública NCM" ON public.ncm_catalogo FOR SELECT USING (true);

DROP POLICY IF EXISTS "Lectura pública Países" ON public.paises;
CREATE POLICY "Lectura pública Países" ON public.paises FOR SELECT USING (true);

DROP POLICY IF EXISTS "Lectura pública Puertos" ON public.puertos_aduanas;
CREATE POLICY "Lectura pública Puertos" ON public.puertos_aduanas FOR SELECT USING (true);

DROP POLICY IF EXISTS "Lectura pública Operadores" ON public.operadores_logisticos;
CREATE POLICY "Lectura pública Operadores" ON public.operadores_logisticos FOR SELECT USING (activo = true);

-- Políticas de inserción y lectura para simulaciones
-- Lectura pública para validación por ID / hash
DROP POLICY IF EXISTS "Lectura simulación por ID" ON public.simulaciones;
CREATE POLICY "Lectura simulación por ID" ON public.simulaciones FOR SELECT USING (true);


DROP POLICY IF EXISTS "Inserción simulaciones" ON public.simulaciones;
-- No se crea política de INSERT para 'anon'. Solo el rol 'service_role' tiene permiso de inserción.


DROP POLICY IF EXISTS "Inserción pública de leads" ON public.solicitudes_contacto;
-- No se crea política de INSERT para 'anon'. Inserción exclusiva vía service_role del backend.

-- ==============================================================================
-- 9. SEEDS / DATOS INICIALES PRECISIONADOS
-- ==============================================================================

-- Países
INSERT INTO public.paises (codigo_iso, nombre, es_mercosur, flete_estimado_pct) VALUES
('CHN', 'China (Extrazona)', FALSE, 10.00),
('USA', 'Estados Unidos (Extrazona)', FALSE, 8.00),
('BRA', 'Brasil (Mercosur - Intrazona)', TRUE, 5.00),
('DEU', 'Alemania (Extrazona)', FALSE, 9.00),
('ITA', 'Italia (Extrazona)', FALSE, 9.00),
('VNM', 'Vietnam (Extrazona)', FALSE, 11.00),
('ESP', 'España (Extrazona)', FALSE, 8.50),
('URY', 'Uruguay (Mercosur - Intrazona)', TRUE, 4.00)
ON CONFLICT (codigo_iso) DO UPDATE 
SET nombre = EXCLUDED.nombre, es_mercosur = EXCLUDED.es_mercosur, flete_estimado_pct = EXCLUDED.flete_estimado_pct;

-- Puertos y Aduanas Oficiales de la República Argentina (PN3)
INSERT INTO public.puertos_aduanas (nombre, via_ingreso, provincia, codigo_sim, operador, tipos_carga, infraestructura, latitud, longitud, paises_frecuentes) VALUES
('Puerto de Buenos Aires', 'MARITIMA_FLUVIAL', 'Ciudad Autónoma de Buenos Aires', 'ADU-001-PBA', 'AGPSE (Administración General de Puertos)', ARRAY['FCL', 'LCL', 'GENERAL', 'REFRIGERADA'], 'Terminales 1, 2, 3, 4 y 5 (Puerto Nuevo). Escáneres de alta penetración no intrusivos.', -34.598500, -58.373100, ARRAY['CHN', 'USA', 'DEU', 'ITA', 'ESP', 'BRA']),
('Aduana de Ezeiza', 'AEREA', 'Buenos Aires', 'ADU-073-EZE', 'ARCA-DGA / Aeropuertos Argentina 2000', ARRAY['GENERAL', 'COURIER', 'VALOR', 'PERECEDERA'], 'Terminal de Cargas Aéreas (TCA) con cámaras frigoríficas certificadas y depósito fiscal de alta seguridad.', -34.822200, -58.535800, ARRAY['USA', 'CHN', 'DEU', 'ESP', 'ITA']),
('Puerto de Rosario', 'MARITIMA_FLUVIAL', 'Santa Fe', 'ADU-052-ROS', 'Ente Administrador Puerto Rosario (ENAPRO)', ARRAY['FCL', 'LCL', 'GRANOS', 'INDUSTRIAL'], 'Muelle de aguas profundas sobre la Hidrovía Paraná-Paraguay. Conexión ferroviaria directa.', -32.946800, -60.639300, ARRAY['BRA', 'URY', 'CHN', 'USA']),
('Paso Cristo Redentor', 'TERRESTRE', 'Mendoza', 'ADU-042-PCR', 'Gendarmería Nacional / ARCA-DGA', ARRAY['FCL', 'GENERAL', 'CARGAS PELIGROSAS'], 'Complejo Fronterizo Los Horcones (Argentina) - Los Libertadores (Chile). Corredor Bioceánico.', -32.835300, -70.064900, ARRAY['BRA', 'URY']),
('Puerto de Mar del Plata', 'MARITIMA_FLUVIAL', 'Buenos Aires', 'ADU-038-MDP', 'Consorcio Portuario Regional Mar del Plata', ARRAY['FCL', 'REFRIGERADA', 'PESQUERA'], 'Terminal de contenedores y carga frigorífica para industria textil, pesquera e industrial regional.', -38.005500, -57.542600, ARRAY['ESP', 'CHN', 'USA', 'BRA'])
ON CONFLICT DO NOTHING;

-- Operadores Logísticos y Despachantes Matriculados (PN4)
INSERT INTO public.operadores_logisticos (razon_social, tipo_operador, matricula_dga, email_notificacion, telefono, rubros_especialidad, aduanas_operativas, modalidades, experiencia_anios, rating, activo) VALUES
('Bonelli & Asociados Comercio Exterior', 'DESPACHANTE', 'DGA-48921-B', 'contacto@importaplus.com.ar', '+54 11 4321-8800', ARRAY['Tecnología', 'Electrónica', 'Bienes de Capital', 'Textil'], ARRAY['Buenos Aires', 'Ezeiza', 'Rosario'], ARRAY['FCL', 'LCL', 'Courier'], 12, 4.95, TRUE),
('NEXA Logistics Global Forwarding', 'FORWARDER', 'FWD-0842-AR', 'operaciones@importaplus.com.ar', '+54 11 5239-1100', ARRAY['Carga General', 'Maquinaria', 'Repuestos Automotrices'], ARRAY['Buenos Aires', 'Rosario', 'Paso Cristo Redentor'], ARRAY['FCL', 'LCL', 'Consolidado Marítimo'], 15, 4.90, TRUE),
('Ezeiza Express Brokerage & Logistics', 'DESPACHANTE', 'DGA-31205-E', 'ezeiza@importaplus.com.ar', '+54 11 4480-2233', ARRAY['Courier Urgente', 'Tecnología', 'Farmacéutica', 'Muestras'], ARRAY['Ezeiza'], ARRAY['Aéreo', 'Courier', 'Carga Crítica'], 8, 4.85, TRUE),
('Andina Cargas Internacionales', 'FORWARDER', 'FWD-1290-CUYO', 'mendoza@importaplus.com.ar', '+54 261 420-5544', ARRAY['Terrestre Mercosur', 'Agroindustria', 'Repuestos'], ARRAY['Paso Cristo Redentor', 'Rosario'], ARRAY['Terrestre FTL/LTL', 'Multimodal'], 18, 4.80, TRUE)
ON CONFLICT DO NOTHING;

-- Posiciones NCM de Prueba Frecuentes
INSERT INTO public.ncm_catalogo (codigo_ncm, descripcion, tipo_bien, aec, iva_alicuota, anexo_excepcion, arancel_excepcion, condicion_excepcion) VALUES
('8471.30.12', 'Máquinas automáticas para tratamiento o procesamiento de datos, portátiles, de peso inferior o igual a 350 g, con teclado y pantalla (Notebooks / Laptops)', 'BIT', 16.00, 10.50, 'Decreto 557/2023 - Anexo I', 2.00, 'Tasa preferencial arancelaria del 2% sujeta a cumplimiento de especificaciones técnicas BIT'),
('8517.12.31', 'Teléfonos móviles (celulares) y los de otras redes inalámbricas', 'BIT', 16.00, 21.00, NULL, NULL, NULL),
('6109.10.00', 'T-shirts y camisetas de punto, de algodón para hombres o mujeres', 'GENERAL', 35.00, 21.00, NULL, NULL, 'Sujeto a verificación de etiquetado de origen y composición textil'),
('6402.19.00', 'Calzado deportivo con suela y parte superior de caucho o plástico', 'GENERAL', 35.00, 21.00, NULL, NULL, 'Control de marcas y certificados de seguridad'),
('8471.41.00', 'Las demás máquinas automáticas para tratamiento o procesamiento de datos que comprendan en la misma envoltura al menos una unidad central de proceso y una de entrada y salida', 'BK', 16.00, 10.50, 'Decreto 557/2023 - Bien de Capital', 0.00, 'Exento de DIE bajo régimen BK y alícuota reducida de IVA 10.5%'),
('9503.00.99', 'Los demás juguetes; modelos reducidos y modelos similares para entretenimiento; rompecabezas de cualquier tipo', 'GENERAL', 35.00, 21.00, NULL, NULL, 'Requiere certificado de seguridad del juguete según Res. 163/2005'),
('8708.29.99', 'Partes y accesorios de carrocerías para vehículos automóviles', 'GENERAL', 18.00, 21.00, NULL, NULL, 'Homologación técnica CHAS requerida')
ON CONFLICT (codigo_ncm) DO UPDATE 
SET descripcion = EXCLUDED.descripcion, tipo_bien = EXCLUDED.tipo_bien, aec = EXCLUDED.aec, iva_alicuota = EXCLUDED.iva_alicuota;
