BEGIN;

CREATE SCHEMA IF NOT EXISTS extensions;
CREATE EXTENSION IF NOT EXISTS pg_trgm WITH SCHEMA extensions;
CREATE EXTENSION IF NOT EXISTS pgcrypto WITH SCHEMA extensions;

SET search_path = public, extensions;

CREATE OR REPLACE FUNCTION public.fn_sha256_hex(p_cadena TEXT)
RETURNS VARCHAR(64)
LANGUAGE sql
IMMUTABLE
STRICT
SET search_path = public, pg_temp
AS $$
    SELECT encode(sha256(convert_to(p_cadena, 'UTF8')), 'hex')::VARCHAR(64);
$$;

CREATE TABLE IF NOT EXISTS public.ncm_catalogo (
    codigo_ncm VARCHAR(15) PRIMARY KEY CHECK (char_length(trim(codigo_ncm)) BETWEEN 4 AND 15),
    descripcion TEXT NOT NULL CHECK (char_length(trim(descripcion)) BETWEEN 2 AND 4000),
    tipo_bien VARCHAR(10) NOT NULL DEFAULT 'GENERAL' CHECK (tipo_bien IN ('GENERAL', 'BK', 'BIT')),
    aec NUMERIC(5,2) NOT NULL DEFAULT 0.00 CHECK (aec >= 0 AND aec <= 100),
    iva_alicuota NUMERIC(5,2) NOT NULL DEFAULT 21.00 CHECK (iva_alicuota >= 0 AND iva_alicuota <= 100),
    anexo_excepcion VARCHAR(120),
    arancel_excepcion NUMERIC(5,2) CHECK (arancel_excepcion IS NULL OR (arancel_excepcion >= 0 AND arancel_excepcion <= 100)),
    condicion_excepcion TEXT,
    updated_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    dvh VARCHAR(64) NOT NULL DEFAULT ''
);

ALTER TABLE public.ncm_catalogo ADD COLUMN IF NOT EXISTS dvh VARCHAR(64) NOT NULL DEFAULT '';
ALTER TABLE public.ncm_catalogo DROP CONSTRAINT IF EXISTS ncm_catalogo_descripcion_check;
ALTER TABLE public.ncm_catalogo ADD CONSTRAINT ncm_catalogo_descripcion_check CHECK (char_length(trim(descripcion)) BETWEEN 2 AND 4000);

CREATE INDEX IF NOT EXISTS idx_ncm_descripcion_trgm
ON public.ncm_catalogo USING GIN (descripcion gin_trgm_ops);

DROP INDEX IF EXISTS public.idx_ncm_codigo;

CREATE TABLE IF NOT EXISTS public.paises (
    codigo_iso VARCHAR(3) PRIMARY KEY CHECK (codigo_iso ~ '^[A-Z]{3}$'),
    nombre VARCHAR(100) NOT NULL CHECK (char_length(trim(nombre)) BETWEEN 2 AND 100),
    es_mercosur BOOLEAN NOT NULL DEFAULT FALSE,
    flete_estimado_pct NUMERIC(5,2) NOT NULL DEFAULT 8.00 CHECK (flete_estimado_pct >= 0 AND flete_estimado_pct <= 100),
    dvh VARCHAR(64) NOT NULL DEFAULT ''
);

ALTER TABLE public.paises ADD COLUMN IF NOT EXISTS dvh VARCHAR(64) NOT NULL DEFAULT '';

CREATE TABLE IF NOT EXISTS public.puertos_aduanas (
    id UUID DEFAULT gen_random_uuid() PRIMARY KEY,
    nombre VARCHAR(150) NOT NULL CHECK (char_length(trim(nombre)) BETWEEN 3 AND 150),
    via_ingreso VARCHAR(50) NOT NULL CHECK (via_ingreso IN ('MARITIMA_FLUVIAL', 'AEREA', 'TERRESTRE')),
    provincia VARCHAR(100) NOT NULL CHECK (char_length(trim(provincia)) >= 2),
    codigo_sim VARCHAR(30) UNIQUE,
    operador VARCHAR(150),
    tipos_carga TEXT[] DEFAULT ARRAY[]::TEXT[],
    infraestructura TEXT,
    latitud NUMERIC(10,6) CHECK (latitud IS NULL OR (latitud BETWEEN -90 AND 90)),
    longitud NUMERIC(10,6) CHECK (longitud IS NULL OR (longitud BETWEEN -180 AND 180)),
    paises_frecuentes VARCHAR(3)[] DEFAULT ARRAY[]::VARCHAR(3)[],
    dvh VARCHAR(64) NOT NULL DEFAULT ''
);

ALTER TABLE public.puertos_aduanas ADD COLUMN IF NOT EXISTS dvh VARCHAR(64) NOT NULL DEFAULT '';

CREATE TABLE IF NOT EXISTS public.forwarders (
    id UUID DEFAULT gen_random_uuid() PRIMARY KEY,
    nombre VARCHAR(120) NOT NULL CHECK (char_length(trim(nombre)) BETWEEN 1 AND 120),
    imagen_url TEXT NOT NULL CHECK (char_length(trim(imagen_url)) BETWEEN 1 AND 1024),
    whatsapp VARCHAR(40) NOT NULL CHECK (char_length(trim(whatsapp)) BETWEEN 6 AND 40),
    email VARCHAR(150) NOT NULL CHECK (email ~* '^[A-Z0-9._%+-]+@[A-Z0-9.-]+\.[A-Z]{2,}$'),
    orden INT NOT NULL DEFAULT 1 CHECK (orden BETWEEN 1 AND 100),
    activo BOOLEAN NOT NULL DEFAULT TRUE,
    created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    dvh VARCHAR(64) NOT NULL DEFAULT ''
);

CREATE INDEX IF NOT EXISTS idx_forwarders_activos_orden
ON public.forwarders (activo, orden ASC);

CREATE TABLE IF NOT EXISTS public.operadores_logisticos (
    id UUID DEFAULT gen_random_uuid() PRIMARY KEY,
    razon_social VARCHAR(150) NOT NULL,
    tipo_operador VARCHAR(50) NOT NULL CHECK (tipo_operador IN ('DESPACHANTE', 'FORWARDER', 'INTEGRAL')),
    matricula_dga VARCHAR(50) UNIQUE,
    email_notificacion VARCHAR(150) NOT NULL CHECK (email_notificacion ~* '^[A-Z0-9._%+-]+@[A-Z0-9.-]+\.[A-Z]{2,}$'),
    telefono VARCHAR(50),
    rubros_especialidad TEXT[],
    aduanas_operativas TEXT[],
    modalidades TEXT[],
    experiencia_anios INT DEFAULT 5 CHECK (experiencia_anios >= 0 AND experiencia_anios <= 150),
    rating NUMERIC(3,2) DEFAULT 5.00 CHECK (rating >= 0 AND rating <= 5.00),
    activo BOOLEAN DEFAULT TRUE,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT NOW()
);

CREATE TABLE IF NOT EXISTS public.simulaciones (
    id_cotizacion VARCHAR(25) PRIMARY KEY CHECK (id_cotizacion ~ '^IMP-[A-Z0-9-]{6,20}$'),
    codigo_ncm VARCHAR(15) REFERENCES public.ncm_catalogo(codigo_ncm) ON UPDATE CASCADE,
    pais_origen VARCHAR(3) REFERENCES public.paises(codigo_iso) ON UPDATE CASCADE,
    cantidad INT NOT NULL CHECK (cantidad > 0 AND cantidad <= 10000000),
    valor_fob_unitario NUMERIC(12,2) NOT NULL CHECK (valor_fob_unitario > 0),
    valor_fob_total NUMERIC(14,2) NOT NULL CHECK (valor_fob_total > 0),
    flete_usd NUMERIC(12,2) NOT NULL DEFAULT 0 CHECK (flete_usd >= 0),
    seguro_usd NUMERIC(12,2) NOT NULL DEFAULT 0 CHECK (seguro_usd >= 0),
    valor_cif_usd NUMERIC(14,2) NOT NULL CHECK (valor_cif_usd > 0),
    desglose_impuestos JSONB NOT NULL CHECK (pg_column_size(desglose_impuestos) <= 16384),
    total_gravamenes_usd NUMERIC(14,2) NOT NULL CHECK (total_gravamenes_usd >= 0),
    costo_total_nacionalizado_usd NUMERIC(14,2) NOT NULL CHECK (costo_total_nacionalizado_usd > 0),
    costo_unitario_final_usd NUMERIC(12,2) NOT NULL CHECK (costo_unitario_final_usd > 0),
    cotizacion_bna_ars NUMERIC(12,2) CHECK (cotizacion_bna_ars IS NULL OR cotizacion_bna_ars > 0),
    created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW()
);

CREATE INDEX IF NOT EXISTS idx_simulaciones_created_at
ON public.simulaciones (created_at DESC);

CREATE TABLE IF NOT EXISTS public.solicitudes_contacto (
    id UUID DEFAULT gen_random_uuid() PRIMARY KEY,
    operador_id UUID,
    id_cotizacion VARCHAR(25) REFERENCES public.simulaciones(id_cotizacion) ON DELETE SET NULL,
    nombre_apellido VARCHAR(150) NOT NULL CHECK (char_length(trim(nombre_apellido)) BETWEEN 2 AND 150),
    empresa VARCHAR(150),
    email VARCHAR(150) NOT NULL CHECK (email ~* '^[A-Z0-9._%+-]+@[A-Z0-9.-]+\.[A-Z]{2,}$'),
    telefono VARCHAR(50) NOT NULL CHECK (char_length(trim(telefono)) BETWEEN 6 AND 50),
    comentarios TEXT CHECK (comentarios IS NULL OR char_length(comentarios) <= 1000),
    created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW()
);

CREATE TABLE IF NOT EXISTS public.admin_usuarios (
    id UUID DEFAULT gen_random_uuid() PRIMARY KEY,
    username VARCHAR(50) NOT NULL UNIQUE CHECK (char_length(trim(username)) BETWEEN 3 AND 50),
    password_hash VARCHAR(100) NOT NULL CHECK (password_hash ~ '^\$2[aby]\$\d{2}\$.{53}$'),
    activo BOOLEAN NOT NULL DEFAULT TRUE,
    ultimo_acceso TIMESTAMP WITH TIME ZONE,
    password_updated_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    dvh VARCHAR(64) NOT NULL DEFAULT ''
);

CREATE TABLE IF NOT EXISTS public.digito_verificador (
    tabla_nombre VARCHAR(80) PRIMARY KEY,
    dvv VARCHAR(64) NOT NULL,
    cantidad_registros INT NOT NULL DEFAULT 0 CHECK (cantidad_registros >= 0),
    snapshot_control JSONB NOT NULL DEFAULT '[]'::JSONB,
    updated_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    updated_by VARCHAR(80) NOT NULL DEFAULT 'SYSTEM'
);

CREATE TABLE IF NOT EXISTS public.parametros_sistema (
    clave VARCHAR(80) PRIMARY KEY,
    valor VARCHAR(250) NOT NULL,
    descripcion TEXT,
    updated_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW()
);

ALTER TABLE public.ncm_catalogo ENABLE ROW LEVEL SECURITY;
ALTER TABLE public.paises ENABLE ROW LEVEL SECURITY;
ALTER TABLE public.puertos_aduanas ENABLE ROW LEVEL SECURITY;
ALTER TABLE public.forwarders ENABLE ROW LEVEL SECURITY;
ALTER TABLE public.operadores_logisticos ENABLE ROW LEVEL SECURITY;
ALTER TABLE public.simulaciones ENABLE ROW LEVEL SECURITY;
ALTER TABLE public.solicitudes_contacto ENABLE ROW LEVEL SECURITY;
ALTER TABLE public.admin_usuarios ENABLE ROW LEVEL SECURITY;
ALTER TABLE public.digito_verificador ENABLE ROW LEVEL SECURITY;
ALTER TABLE public.parametros_sistema ENABLE ROW LEVEL SECURITY;

REVOKE ALL ON ALL TABLES IN SCHEMA public FROM PUBLIC, anon, authenticated;
REVOKE ALL ON ALL SEQUENCES IN SCHEMA public FROM PUBLIC, anon, authenticated;

GRANT USAGE ON SCHEMA public TO anon, authenticated, service_role;
GRANT ALL ON ALL TABLES IN SCHEMA public TO service_role;
GRANT ALL ON ALL SEQUENCES IN SCHEMA public TO service_role;

GRANT SELECT (codigo_ncm, descripcion, tipo_bien, aec, iva_alicuota, anexo_excepcion, arancel_excepcion, condicion_excepcion)
ON public.ncm_catalogo TO anon, authenticated;

GRANT SELECT (codigo_iso, nombre, es_mercosur, flete_estimado_pct)
ON public.paises TO anon, authenticated;

GRANT SELECT (id, nombre, via_ingreso, provincia, codigo_sim, operador, tipos_carga, infraestructura, latitud, longitud, paises_frecuentes)
ON public.puertos_aduanas TO anon, authenticated;

GRANT SELECT (id, nombre, imagen_url, whatsapp, email, orden, activo)
ON public.forwarders TO anon, authenticated;

GRANT SELECT (clave, valor, descripcion)
ON public.parametros_sistema TO anon, authenticated;

DROP POLICY IF EXISTS "Lectura pública NCM" ON public.ncm_catalogo;
CREATE POLICY "Lectura pública NCM" ON public.ncm_catalogo FOR SELECT TO anon, authenticated USING (true);

DROP POLICY IF EXISTS "Lectura pública Países" ON public.paises;
CREATE POLICY "Lectura pública Países" ON public.paises FOR SELECT TO anon, authenticated USING (true);

DROP POLICY IF EXISTS "Lectura pública Puertos" ON public.puertos_aduanas;
CREATE POLICY "Lectura pública Puertos" ON public.puertos_aduanas FOR SELECT TO anon, authenticated USING (true);

DROP POLICY IF EXISTS "Lectura pública Forwarders Activos" ON public.forwarders;
CREATE POLICY "Lectura pública Forwarders Activos" ON public.forwarders FOR SELECT TO anon, authenticated USING (activo = true);

DROP POLICY IF EXISTS "Lectura pública Parámetros" ON public.parametros_sistema;
CREATE POLICY "Lectura pública Parámetros" ON public.parametros_sistema FOR SELECT TO anon, authenticated USING (true);

DROP POLICY IF EXISTS "Lectura pública Operadores" ON public.operadores_logisticos;
DROP POLICY IF EXISTS "Lectura simulación por ID" ON public.simulaciones;
DROP POLICY IF EXISTS "Inserción simulaciones" ON public.simulaciones;
DROP POLICY IF EXISTS "Inserción pública de leads" ON public.solicitudes_contacto;

CREATE OR REPLACE FUNCTION public.obtener_simulacion_por_id(p_id_cotizacion VARCHAR)
RETURNS TABLE (
    id_cotizacion VARCHAR,
    codigo_ncm VARCHAR,
    pais_origen VARCHAR,
    cantidad INT,
    valor_fob_total NUMERIC,
    valor_cif_usd NUMERIC,
    total_gravamenes_usd NUMERIC,
    costo_total_nacionalizado_usd NUMERIC,
    costo_unitario_final_usd NUMERIC,
    created_at TIMESTAMP WITH TIME ZONE
)
LANGUAGE sql
SECURITY DEFINER
STABLE
SET search_path = public, pg_temp
AS $$
    SELECT
        s.id_cotizacion,
        s.codigo_ncm,
        s.pais_origen,
        s.cantidad,
        s.valor_fob_total,
        s.valor_cif_usd,
        s.total_gravamenes_usd,
        s.costo_total_nacionalizado_usd,
        s.costo_unitario_final_usd,
        s.created_at
    FROM public.simulaciones s
    WHERE s.id_cotizacion = trim(p_id_cotizacion)
    LIMIT 1;
$$;

REVOKE ALL ON FUNCTION public.obtener_simulacion_por_id(VARCHAR) FROM PUBLIC;
GRANT EXECUTE ON FUNCTION public.obtener_simulacion_por_id(VARCHAR) TO anon, authenticated, service_role;

CREATE OR REPLACE FUNCTION public.purgar_simulaciones_antiguas(p_dias INT DEFAULT 90)
RETURNS INT
LANGUAGE plpgsql
SECURITY DEFINER
SET search_path = public, pg_temp
AS $$
DECLARE
    v_borradas INT;
BEGIN
    DELETE FROM public.simulaciones
    WHERE created_at < NOW() - make_interval(days => GREATEST(7, p_dias));
    GET DIAGNOSTICS v_borradas = ROW_COUNT;
    RETURN v_borradas;
END;
$$;

REVOKE ALL ON FUNCTION public.purgar_simulaciones_antiguas(INT) FROM PUBLIC, anon, authenticated;
GRANT EXECUTE ON FUNCTION public.purgar_simulaciones_antiguas(INT) TO service_role;

DO $$
BEGIN
    IF EXISTS (
        SELECT 1 FROM information_schema.tables
        WHERE table_schema = 'storage' AND table_name = 'buckets'
    ) THEN
        INSERT INTO storage.buckets (id, name, public, file_size_limit, allowed_mime_types)
        VALUES (
            'forwarders',
            'forwarders',
            TRUE,
            524288,
            ARRAY['image/webp', 'image/png', 'image/jpeg']
        )
        ON CONFLICT (id) DO UPDATE
        SET public = EXCLUDED.public,
            file_size_limit = EXCLUDED.file_size_limit,
            allowed_mime_types = EXCLUDED.allowed_mime_types;

        DROP POLICY IF EXISTS "Lectura pública fotos forwarders" ON storage.objects;
        CREATE POLICY "Lectura pública fotos forwarders"
        ON storage.objects FOR SELECT
        TO public
        USING (bucket_id = 'forwarders');
    END IF;
END $$;

INSERT INTO public.parametros_sistema (clave, valor, descripcion) VALUES
('TasaEstadisticaPct', '3.00', 'Alícuota general Tasa de Estadística Aduanera (%)'),
('IvaGeneralPct', '21.00', 'Alícuota IVA General DGA (%)'),
('IvaReducidoBKPct', '10.50', 'Alícuota IVA Reducido Bienes de Capital / Informática (%)'),
('PercepcionIvaGeneralPct', '20.00', 'Percepción IVA Adicional General RG 2281 (%)'),
('PercepcionIvaReducidoPct', '10.00', 'Percepción IVA Adicional Reducida RG 2281 (%)'),
('PercepcionGananciasGeneralPct', '6.00', 'Percepción Ganancias Responsable Inscripto / Reventa (%)'),
('PercepcionGananciasNoCatPct', '11.00', 'Percepción Ganancias Consumo Particular / No categorizado (%)'),
('ArancelSimFijoUSD', '28.00', 'Arancel fijo Sistema Informático Malvina (USD)'),
('DigitalizacionFijaUSD', '10.00', 'Tasa fija de Digitalización Documental (USD)'),
('CotizacionOficialBNA', '1420.50', 'Tipo de cambio oficial vendedor BNA de referencia (ARS/USD)')
ON CONFLICT (clave) DO UPDATE
SET valor = EXCLUDED.valor, descripcion = EXCLUDED.descripcion, updated_at = NOW();

INSERT INTO public.paises (codigo_iso, nombre, es_mercosur, flete_estimado_pct) VALUES
('AFG', 'Afganistán', FALSE, 12.00),
('ALB', 'Albania', FALSE, 9.50),
('DEU', 'Alemania', FALSE, 9.00),
('AND', 'Andorra', FALSE, 9.00),
('AGO', 'Angola', FALSE, 11.00),
('ATG', 'Antigua y Barbuda', FALSE, 8.50),
('SAU', 'Arabia Saudita', FALSE, 10.50),
('DZA', 'Argelia', FALSE, 10.00),
('ARG', 'Argentina', TRUE, 2.00),
('ARM', 'Armenia', FALSE, 11.00),
('AUS', 'Australia', FALSE, 11.50),
('AUT', 'Austria', FALSE, 9.50),
('AZE', 'Azerbaiyán', FALSE, 11.00),
('BHS', 'Bahamas', FALSE, 8.00),
('BGD', 'Bangladés', FALSE, 11.50),
('BRB', 'Barbados', FALSE, 8.50),
('BHR', 'Baréin', FALSE, 10.50),
('BEL', 'Bélgica', FALSE, 9.00),
('BLZ', 'Belice', FALSE, 8.00),
('BEN', 'Benín', FALSE, 11.00),
('BLR', 'Bielorrusia', FALSE, 10.50),
('MMR', 'Birmania (Myanmar)', FALSE, 11.50),
('BOL', 'Bolivia', TRUE, 4.50),
('BIH', 'Bosnia y Herzegovina', FALSE, 9.50),
('BWA', 'Botsuana', FALSE, 11.00),
('BRA', 'Brasil', TRUE, 5.00),
('BRN', 'Brunéi', FALSE, 11.50),
('BGR', 'Bulgaria', FALSE, 9.50),
('BFA', 'Burkina Faso', FALSE, 11.00),
('BDI', 'Burundi', FALSE, 11.50),
('BTN', 'Bután', FALSE, 12.00),
('CPV', 'Cabo Verde', FALSE, 10.00),
('KHM', 'Camboya', FALSE, 11.50),
('CMR', 'Camerún', FALSE, 11.00),
('CAN', 'Canadá', FALSE, 8.50),
('QAT', 'Catar', FALSE, 10.50),
('TCD', 'Chad', FALSE, 11.50),
('CHL', 'Chile', FALSE, 5.50),
('CHN', 'China', FALSE, 10.00),
('CYP', 'Chipre', FALSE, 9.50),
('VAT', 'Ciudad del Vaticano', FALSE, 9.00),
('COL', 'Colombia', FALSE, 7.00),
('COM', 'Comoras', FALSE, 11.50),
('COG', 'Congo', FALSE, 11.00),
('PRK', 'Corea del Norte', FALSE, 11.50),
('KOR', 'Corea del Sur', FALSE, 10.50),
('CIV', 'Costa de Marfil', FALSE, 10.50),
('CRI', 'Costa Rica', FALSE, 7.50),
('HRV', 'Croacia', FALSE, 9.50),
('CUB', 'Cuba', FALSE, 8.00),
('DNK', 'Dinamarca', FALSE, 9.00),
('DMA', 'Dominica', FALSE, 8.50),
('ECU', 'Ecuador', FALSE, 6.50),
('EGY', 'Egipto', FALSE, 10.00),
('SLV', 'El Salvador', FALSE, 7.50),
('ARE', 'Emiratos Árabes Unidos', FALSE, 10.50),
('ERI', 'Eritrea', FALSE, 11.00),
('SVK', 'Eslovaquia', FALSE, 9.50),
('SVN', 'Eslovenia', FALSE, 9.50),
('ESP', 'España', FALSE, 8.50),
('USA', 'Estados Unidos', FALSE, 8.00),
('EST', 'Estonia', FALSE, 9.50),
('SWZ', 'Esuatini', FALSE, 11.00),
('ETH', 'Etiopía', FALSE, 11.00),
('PHL', 'Filipinas', FALSE, 11.00),
('FIN', 'Finlandia', FALSE, 9.50),
('FJI', 'Fiyi', FALSE, 12.00),
('FRA', 'Francia', FALSE, 9.00),
('GAB', 'Gabón', FALSE, 11.00),
('GMB', 'Gambia', FALSE, 10.50),
('GEO', 'Georgia', FALSE, 10.50),
('GHA', 'Ghana', FALSE, 10.50),
('GRD', 'Granada', FALSE, 8.50),
('GRC', 'Grecia', FALSE, 9.50),
('GTM', 'Guatemala', FALSE, 7.50),
('GIN', 'Guinea', FALSE, 10.50),
('GNB', 'Guinea-Bisáu', FALSE, 10.50),
('GNQ', 'Guinea Ecuatorial', FALSE, 11.00),
('GUY', 'Guyana', FALSE, 7.50),
('HTI', 'Haití', FALSE, 8.00),
('HND', 'Honduras', FALSE, 7.50),
('HUN', 'Hungría', FALSE, 9.50),
('IND', 'India', FALSE, 10.50),
('IDN', 'Indonesia', FALSE, 11.00),
('IRQ', 'Irak', FALSE, 11.00),
('IRN', 'Irán', FALSE, 11.00),
('IRL', 'Irlanda', FALSE, 9.00),
('ISL', 'Islandia', FALSE, 9.50),
('MHL', 'Islas Marshall', FALSE, 12.00),
('SLB', 'Islas Salomón', FALSE, 12.00),
('ISR', 'Israel', FALSE, 10.00),
('ITA', 'Italia', FALSE, 9.00),
('JAM', 'Jamaica', FALSE, 8.00),
('JPN', 'Japón', FALSE, 11.00),
('JOR', 'Jordania', FALSE, 10.00),
('KAZ', 'Kazajistán', FALSE, 11.50),
('KEN', 'Kenia', FALSE, 11.00),
('KGZ', 'Kirguistán', FALSE, 11.50),
('KIR', 'Kiribati', FALSE, 12.00),
('KWT', 'Kuwait', FALSE, 10.50),
('LAO', 'Laos', FALSE, 11.50),
('LSO', 'Lesoto', FALSE, 11.00),
('LVA', 'Letonia', FALSE, 9.50),
('LBN', 'Líbano', FALSE, 10.00),
('LBR', 'Liberia', FALSE, 10.50),
('LBY', 'Libia', FALSE, 10.00),
('LIE', 'Liechtenstein', FALSE, 9.00),
('LTU', 'Lituania', FALSE, 9.50),
('LUX', 'Luxemburgo', FALSE, 9.00),
('MKD', 'Macedonia del Norte', FALSE, 9.50),
('MDG', 'Madagascar', FALSE, 11.50),
('MYS', 'Malasia', FALSE, 11.00),
('MWI', 'Malaui', FALSE, 11.50),
('MDV', 'Maldivas', FALSE, 11.50),
('MLI', 'Malí', FALSE, 11.00),
('MLT', 'Malta', FALSE, 9.50),
('MAR', 'Marruecos', FALSE, 9.50),
('MUS', 'Mauricio', FALSE, 11.50),
('MRT', 'Mauritania', FALSE, 10.50),
('MEX', 'México', FALSE, 7.50),
('FSM', 'Micronesia', FALSE, 12.00),
('MDA', 'Moldavia', FALSE, 10.00),
('MCO', 'Mónaco', FALSE, 9.00),
('MNG', 'Mongolia', FALSE, 11.50),
('MNE', 'Montenegro', FALSE, 9.50),
('MOZ', 'Mozambique', FALSE, 11.00),
('NAM', 'Namibia', FALSE, 10.50),
('NRU', 'Nauru', FALSE, 12.00),
('NPL', 'Nepal', FALSE, 11.50),
('NIC', 'Nicaragua', FALSE, 7.50),
('NER', 'Níger', FALSE, 11.00),
('NGA', 'Nigeria', FALSE, 10.50),
('NOR', 'Noruega', FALSE, 9.50),
('NZL', 'Nueva Zelanda', FALSE, 11.50),
('OMN', 'Omán', FALSE, 10.50),
('NLD', 'Países Bajos', FALSE, 9.00),
('PAK', 'Pakistán', FALSE, 11.00),
('PLW', 'Palaos', FALSE, 12.00),
('PSE', 'Palestina', FALSE, 10.00),
('PAN', 'Panamá', FALSE, 7.00),
('PNG', 'Papúa Nueva Guinea', FALSE, 12.00),
('PRY', 'Paraguay', TRUE, 4.50),
('PER', 'Perú', FALSE, 6.00),
('POL', 'Polonia', FALSE, 9.50),
('PRT', 'Portugal', FALSE, 8.50),
('GBR', 'Reino Unido', FALSE, 9.00),
('CAF', 'República Centroafricana', FALSE, 11.50),
('CZE', 'República Checa', FALSE, 9.50),
('COD', 'República Democrática del Congo', FALSE, 11.50),
('DOM', 'República Dominicana', FALSE, 8.00),
('RWA', 'Ruanda', FALSE, 11.50),
('ROU', 'Rumania', FALSE, 9.50),
('RUS', 'Rusia', FALSE, 10.50),
('WSM', 'Samoa', FALSE, 12.00),
('KNA', 'San Cristóbal y Nieves', FALSE, 8.50),
('SMR', 'San Marino', FALSE, 9.00),
('VCT', 'San Vicente y las Granadinas', FALSE, 8.50),
('LCA', 'Santa Lucía', FALSE, 8.50),
('STP', 'Santo Tomé y Príncipe', FALSE, 11.00),
('SEN', 'Senegal', FALSE, 10.00),
('SRB', 'Serbia', FALSE, 9.50),
('SYC', 'Seychelles', FALSE, 11.50),
('SLE', 'Sierra Leona', FALSE, 10.50),
('SGP', 'Singapur', FALSE, 10.50),
('SYR', 'Siria', FALSE, 10.50),
('SOM', 'Somalia', FALSE, 11.50),
('LKA', 'Sri Lanka', FALSE, 11.00),
('ZAF', 'Sudáfrica', FALSE, 10.00),
('SDN', 'Sudán', FALSE, 11.00),
('SSD', 'Sudán del Sur', FALSE, 11.50),
('SWE', 'Suecia', FALSE, 9.50),
('CHE', 'Suiza', FALSE, 9.00),
('SUR', 'Surinam', FALSE, 7.50),
('THA', 'Tailandia', FALSE, 11.00),
('TWN', 'Taiwán', FALSE, 10.50),
('TZA', 'Tanzania', FALSE, 11.00),
('TJK', 'Tayikistán', FALSE, 11.50),
('TLS', 'Timor Oriental', FALSE, 12.00),
('TGO', 'Togo', FALSE, 10.50),
('TON', 'Tonga', FALSE, 12.00),
('TTO', 'Trinidad y Tobago', FALSE, 8.00),
('TUN', 'Túnez', FALSE, 9.50),
('TKM', 'Turkmenistán', FALSE, 11.50),
('TUR', 'Turquía', FALSE, 9.50),
('TUV', 'Tuvalu', FALSE, 12.00),
('UKR', 'Ucrania', FALSE, 10.00),
('UGA', 'Uganda', FALSE, 11.00),
('URY', 'Uruguay', TRUE, 4.00),
('UZB', 'Uzbekistán', FALSE, 11.50),
('VUT', 'Vanuatu', FALSE, 12.00),
('VEN', 'Venezuela', FALSE, 7.00),
('VNM', 'Vietnam', FALSE, 11.00),
('YEM', 'Yemen', FALSE, 11.00),
('DJI', 'Yibuti', FALSE, 11.00),
('ZMB', 'Zambia', FALSE, 11.00),
('ZWE', 'Zimbabue', FALSE, 11.00)
ON CONFLICT (codigo_iso) DO UPDATE SET nombre = EXCLUDED.nombre, es_mercosur = EXCLUDED.es_mercosur, flete_estimado_pct = EXCLUDED.flete_estimado_pct;

INSERT INTO public.puertos_aduanas (id, nombre, via_ingreso, provincia, codigo_sim, operador, tipos_carga, infraestructura, latitud, longitud, paises_frecuentes) VALUES
('11111111-1111-1111-1111-111111111111', 'Puerto de Buenos Aires', 'MARITIMA_FLUVIAL', 'Ciudad Autónoma de Buenos Aires', 'ADU-001-PBA', 'AGPSE (Administración General de Puertos)', ARRAY['FCL', 'LCL', 'GENERAL', 'REFRIGERADA'], 'Terminales 1, 2, 3, 4 y 5 (Puerto Nuevo). Escáneres de alta penetración no intrusivos.', -34.598500, -58.373100, ARRAY['CHN', 'USA', 'DEU', 'ITA', 'ESP', 'BRA', 'VNM', 'JPN', 'KOR']),
('22222222-2222-2222-2222-222222222222', 'Aduana de Ezeiza', 'AEREA', 'Buenos Aires', 'ADU-073-EZE', 'ARCA-DGA / Aeropuertos Argentina 2000', ARRAY['GENERAL', 'COURIER', 'VALOR', 'PERECEDERA'], 'Terminal de Cargas Aéreas (TCA) con cámaras frigoríficas certificadas y depósito fiscal de alta seguridad.', -34.822200, -58.535800, ARRAY['USA', 'CHN', 'DEU', 'ESP', 'ITA', 'JPN', 'FRA', 'GBR']),
('33333333-3333-3333-3333-333333333333', 'Puerto de Rosario', 'MARITIMA_FLUVIAL', 'Santa Fe', 'ADU-052-ROS', 'Ente Administrador Puerto Rosario (ENAPRO)', ARRAY['FCL', 'LCL', 'GRANOS', 'INDUSTRIAL'], 'Muelle de aguas profundas sobre la Hidrovía Paraná-Paraguay. Conexión ferroviaria directa.', -32.946800, -60.639300, ARRAY['BRA', 'URY', 'PRY', 'BOL', 'CHN', 'USA']),
('44444444-4444-4444-4444-444444444444', 'Paso Cristo Redentor', 'TERRESTRE', 'Mendoza', 'ADU-042-PCR', 'Gendarmería Nacional / ARCA-DGA', ARRAY['FCL', 'GENERAL', 'CARGAS PELIGROSAS'], 'Complejo Fronterizo Los Horcones (Argentina) - Los Libertadores (Chile). Corredor Bioceánico.', -32.835300, -70.064900, ARRAY['CHL', 'BRA', 'URY', 'CHN']),
('55555555-5555-5555-5555-555555555555', 'Puerto de Mar del Plata', 'MARITIMA_FLUVIAL', 'Buenos Aires', 'ADU-038-MDP', 'Consorcio Portuario Regional Mar del Plata', ARRAY['FCL', 'REFRIGERADA', 'PESQUERA'], 'Terminal de contenedores y carga frigorífica para industria textil, pesquera e industrial regional.', -38.005500, -57.542600, ARRAY['ESP', 'CHN', 'USA', 'BRA'])
ON CONFLICT (codigo_sim) DO UPDATE
SET nombre = EXCLUDED.nombre,
    via_ingreso = EXCLUDED.via_ingreso,
    provincia = EXCLUDED.provincia,
    operador = EXCLUDED.operador,
    tipos_carga = EXCLUDED.tipos_carga,
    infraestructura = EXCLUDED.infraestructura,
    latitud = EXCLUDED.latitud,
    longitud = EXCLUDED.longitud,
    paises_frecuentes = EXCLUDED.paises_frecuentes;

INSERT INTO public.forwarders (id, nombre, imagen_url, whatsapp, email, orden, activo) VALUES
('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa', 'Bonelli & Asociados Global Logistics', 'https://images.unsplash.com/photo-1586528116311-ad8dd3c8310d?auto=format&fit=crop&w=600&q=80', '+5491143218800', 'contacto@importaplus.com.ar', 1, TRUE),
('bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb', 'NEXA Ocean & Air Forwarding', 'https://images.unsplash.com/photo-1494412574643-ff11b0a5c1c3?auto=format&fit=crop&w=600&q=80', '+5491152391100', 'operaciones@importaplus.com.ar', 2, TRUE),
('cccccccc-cccc-cccc-cccc-cccccccccccc', 'Ezeiza Express Cargo & Customs', 'https://images.unsplash.com/photo-1570710891163-6d3b5c47248b?auto=format&fit=crop&w=600&q=80', '+5491144802233', 'ezeiza@importaplus.com.ar', 3, TRUE),
('dddddddd-dddd-dddd-dddd-dddddddddddd', 'Andina Multimodal Internacional', 'https://images.unsplash.com/photo-1601584115197-04ecc0da31d7?auto=format&fit=crop&w=600&q=80', '+5492614205544', 'mendoza@importaplus.com.ar', 4, TRUE)
ON CONFLICT (id) DO UPDATE
SET nombre = EXCLUDED.nombre,
    imagen_url = EXCLUDED.imagen_url,
    whatsapp = EXCLUDED.whatsapp,
    email = EXCLUDED.email,
    orden = EXCLUDED.orden,
    activo = EXCLUDED.activo,
    updated_at = NOW();

INSERT INTO public.ncm_catalogo (codigo_ncm, descripcion, tipo_bien, aec, iva_alicuota, anexo_excepcion, arancel_excepcion, condicion_excepcion) VALUES
('8471.30.12', 'Máquinas automáticas para tratamiento o procesamiento de datos, digitales, portátiles, de peso inferior o igual a 350 g, con teclado y pantalla (Notebooks / Netbooks / Laptops)', 'BIT', 16.00, 10.50, 'Anexo II Dec 557/2023', 2.00, 'Sujeto a verificación técnica de homologación'),
('8471.30.19', 'Las demás máquinas automáticas para tratamiento o procesamiento de datos, portátiles (Tablets y computadoras táctiles)', 'BIT', 16.00, 10.50, NULL, NULL, NULL),
('8471.41.00', 'Computadoras personales de escritorio (Desktop PC) que incluyan unidad de procesamiento, memoria y entrada/salida', 'BIT', 16.00, 10.50, NULL, NULL, NULL),
('8471.70.10', 'Unidades de memoria de disco magnético rígido o estado sólido (Discos SSD y HDD internos)', 'BIT', 0.00, 10.50, 'Lista BIT Mercosur', 0.00, 'Insumo de informática con alícuota cero'),
('8471.70.20', 'Unidades de memoria portátiles tipo pendrive y tarjetas de memoria flash (Flash Drive / MicroSD)', 'BIT', 2.00, 10.50, NULL, NULL, NULL),
('8471.60.52', 'Teclados mecánicos y de membrana para computadoras y terminales de procesamiento de datos', 'BIT', 12.00, 10.50, NULL, NULL, NULL),
('8471.60.53', 'Ratones ópticos, inalámbricos y dispositivos señaladores (Mouse gamer / trackballs)', 'BIT', 12.00, 10.50, NULL, NULL, NULL),
('8517.13.00', 'Teléfonos inteligentes (Smartphones) celulares digitales con conectividad 4G/5G', 'BIT', 16.00, 10.50, NULL, NULL, 'Requiere certificación ENACOM'),
('8517.62.72', 'Routers inalámbricos para redes Wi-Fi residenciales y corporativas (Routers / Modems)', 'BIT', 12.00, 10.50, 'Anexo I Dec 557/2023', 2.00, 'Equipos de conectividad banda ancha'),
('8528.52.00', 'Monitores para computadoras con pantallas planas LCD, LED y OLED con entrada HDMI/DisplayPort', 'BIT', 16.00, 10.50, NULL, NULL, NULL),
('8504.40.10', 'Cargadores rápidos de baterías para teléfonos móviles, fuentes conmutadas y adaptadores USB-C', 'GENERAL', 14.00, 21.00, NULL, NULL, 'Requiere Seguridad Eléctrica IRAM'),
('8518.30.00', 'Auriculares con micrófono incorporado, incluso inalámbricos Bluetooth tipo TWS', 'GENERAL', 18.00, 21.00, NULL, NULL, NULL),
('6109.10.00', 'T-shirts y camisetas interiores de punto de algodón (Remeras de algodón para hombres y mujeres)', 'GENERAL', 35.00, 21.00, NULL, NULL, 'Requiere declaración jurada de composición textil'),
('6110.20.00', 'Suéteres, pulóveres, chalecos y artículos similares de punto, de algodón', 'GENERAL', 35.00, 21.00, NULL, NULL, NULL),
('6203.42.00', 'Pantalones largos, con peto y pantalones cortos de mezclilla (Jeans denim) para hombres', 'GENERAL', 35.00, 21.00, NULL, NULL, NULL),
('6204.62.00', 'Pantalones largos, shorts y bermudas de algodón para mujeres y niñas', 'GENERAL', 35.00, 21.00, NULL, NULL, NULL),
('6402.19.00', 'Calzado de tenis, baloncesto, gimnasia, entrenamiento y calzado similar (Zapatillas deportivas)', 'GENERAL', 35.00, 21.00, NULL, NULL, 'Etiquetado de origen y composición en suela y capellada'),
('6403.99.90', 'Calzado con suela de caucho, plástico o cuero regenerado y parte superior de cuero natural (Zapatos de vestir)', 'GENERAL', 35.00, 21.00, NULL, NULL, NULL),
('6115.95.00', 'Medias, calcetines y soquetes de punto de algodón', 'GENERAL', 35.00, 21.00, NULL, NULL, NULL),
('6211.43.00', 'Camperas, cazadoras y ropa de abrigo deportiva de fibras sintéticas o artificiales', 'GENERAL', 35.00, 21.00, NULL, NULL, NULL),
('8428.39.90', 'Aparatos elevadores y transportadores continuos mecánicos para mercancías (Cintas transportadoras industriales)', 'BK', 14.00, 10.50, 'Régimen BK Bienes de Capital', 0.00, 'Bien de Capital (BK): Alícuota DIE 0% e IVA reducido al 10.5%'),
('8477.10.10', 'Máquinas de moldear por inyección de termoplásticos de fuerza de cierre superior o igual a 50 toneladas (Inyectoras de plástico)', 'BK', 14.00, 10.50, 'Régimen BK Bienes de Capital', 2.00, 'Bien de Capital (BK): IVA reducido al 10.5%'),
('8458.11.99', 'Tornos horizontales de control numérico computarizado (Torno CNC metalmecánico de precisión)', 'BK', 14.00, 10.50, 'Régimen BK Bienes de Capital', 2.00, 'Bien de Capital (BK): IVA reducido al 10.5%'),
('8443.32.10', 'Impresoras industriales de inyección de tinta continua de gran formato (Plotters de impresión comercial)', 'BK', 14.00, 10.50, 'Régimen BK Bienes de Capital', 0.00, 'Bien de Capital (BK): IVA reducido al 10.5%'),
('8501.52.90', 'Motores de corriente alterna polifásicos con potencia superior a 750W y hasta 75kW para maquinaria industrial', 'BK', 14.00, 10.50, 'Régimen BK Bienes de Capital', 2.00, 'Bien de Capital (BK): IVA reducido al 10.5%'),
('8419.89.99', 'Intercambiadores de calor e instalaciones térmicas para pasteurización y procesamiento de alimentos', 'BK', 14.00, 10.50, 'Régimen BK Bienes de Capital', 0.00, 'Bien de Capital (BK): IVA reducido al 10.5%'),
('8708.29.99', 'Partes y accesorios de carrocerías para automóviles, furgones y camiones (Paragolpes, guardabarros, puertas)', 'GENERAL', 18.00, 21.00, NULL, NULL, 'Requiere Certificado CHAS (Homologación de Autopartes de Seguridad)'),
('8708.30.90', 'Frenos de disco y tambor, servofrenos y sus partes (Pastillas y discos de freno)', 'GENERAL', 16.00, 21.00, NULL, NULL, 'Requiere Certificado CHAS'),
('8708.80.00', 'Sistemas de amortiguación de suspensión y sus partes (Amortiguadores hidráulicos y de gas)', 'GENERAL', 16.00, 21.00, NULL, NULL, 'Requiere Certificado CHAS'),
('8708.93.00', 'Embragues mecánicos e hidráulicos y sus partes (Placa y disco de embrague, bombín)', 'GENERAL', 16.00, 21.00, NULL, NULL, NULL),
('8512.20.11', 'Faros ópticos principales delanteros y lámparas para vehículos automotores', 'GENERAL', 18.00, 21.00, NULL, NULL, 'Requiere Certificado CHAS'),
('0901.21.00', 'Café tostado, sin descafeinar, en grano o molido para consumo', 'GENERAL', 10.00, 21.00, NULL, NULL, 'Intervención de INAL / SENASA'),
('1806.32.10', 'Chocolate con leche en bloques, tabletas o barras relleno (Barras de chocolate artesanal o premium)', 'GENERAL', 20.00, 21.00, NULL, NULL, 'Intervención de INAL / SENASA'),
('2106.90.90', 'Preparaciones alimenticias para nutrición deportiva, proteínas aisladas de suero (Whey Protein y aminoácidos)', 'GENERAL', 14.00, 21.00, NULL, NULL, 'Aprobación RNE y RNPA ante INAL/ANMAT'),
('3303.00.10', 'Perfumes (extractos de perfumería) y aguas de colonia de marcas internacionales', 'GENERAL', 18.00, 21.00, NULL, NULL, 'Intervención ANMAT Cosméticos'),
('3304.99.90', 'Cremas faciales y corporales hidratantes, sérums antiedad y protectores solares', 'GENERAL', 18.00, 21.00, NULL, NULL, 'Intervención ANMAT Cosméticos'),
('3305.10.00', 'Champúes y acondicionadores para el cuidado capilar', 'GENERAL', 18.00, 21.00, NULL, NULL, 'Intervención ANMAT Cosméticos'),
('9503.00.99', 'Juguetes interactivos, figuras de acción coleccionables, bloques de construcción (Lego) y muñecos', 'GENERAL', 35.00, 21.00, NULL, NULL, 'Ensayo de Seguridad de Juguetes e informe toxicológico de ftalatos'),
('9504.50.00', 'Consolas de videojuegos hogareñas y portátiles (PlayStation, Xbox, Nintendo Switch)', 'GENERAL', 20.00, 21.00, NULL, NULL, 'Certificado de Seguridad Eléctrica'),
('9617.00.10', 'Termos y demás recipientes isotérmicos de acero inoxidable para bebidas frías y calientes (Termos Stanley)', 'GENERAL', 18.00, 21.00, NULL, NULL, 'Certificado de contacto con alimentos INAL'),
('3924.10.00', 'Vajilla, envases herméticos (tuppers) y demás artículos para servicio de mesa o cocina de plástico', 'GENERAL', 16.00, 21.00, NULL, NULL, 'Certificado de contacto con alimentos INAL')
ON CONFLICT (codigo_ncm) DO UPDATE SET descripcion = EXCLUDED.descripcion, tipo_bien = EXCLUDED.tipo_bien, aec = EXCLUDED.aec, iva_alicuota = EXCLUDED.iva_alicuota, anexo_excepcion = EXCLUDED.anexo_excepcion, arancel_excepcion = EXCLUDED.arancel_excepcion, condicion_excepcion = EXCLUDED.condicion_excepcion, updated_at = NOW();


INSERT INTO public.admin_usuarios (id, username, password_hash, activo)
VALUES (
    '99999999-9999-9999-9999-999999999999',
    'Admin',
    '$2a$11$2JNMl4BJq3luXja8OugiIOywkepWfFi9sX6lccOFiayFaljaei5fS',
    TRUE
)
ON CONFLICT (username) DO NOTHING;

CREATE OR REPLACE FUNCTION public.recalcular_digitos_verificadores(p_usuario VARCHAR DEFAULT 'SQL_INIT')
RETURNS VOID
LANGUAGE plpgsql
SECURITY DEFINER
SET search_path = public, pg_temp
AS $$
BEGIN
    UPDATE public.admin_usuarios
    SET dvh = public.fn_sha256_hex(
        lower(trim(username)) || '|' || trim(password_hash) || '|' || (CASE WHEN activo THEN '1' ELSE '0' END)
    )
    WHERE TRUE;

    UPDATE public.forwarders
    SET dvh = public.fn_sha256_hex(
        lower(id::text) || '|' || trim(nombre) || '|' || trim(imagen_url) || '|' || trim(whatsapp) || '|' || trim(email) || '|' || orden::text || '|' || (CASE WHEN activo THEN '1' ELSE '0' END)
    )
    WHERE TRUE;

    UPDATE public.puertos_aduanas
    SET dvh = public.fn_sha256_hex(
        lower(id::text) || '|' || trim(nombre) || '|' || trim(via_ingreso) || '|' || trim(provincia) || '|' || trim(coalesce(codigo_sim, '')) || '|' || trim(coalesce(operador, ''))
    )
    WHERE TRUE;

    UPDATE public.paises
    SET dvh = public.fn_sha256_hex(
        upper(trim(codigo_iso)) || '|' || trim(nombre) || '|' || (CASE WHEN es_mercosur THEN '1' ELSE '0' END) || '|' || trim(to_char(flete_estimado_pct, 'FM999990.00'))
    )
    WHERE TRUE;

    UPDATE public.ncm_catalogo
    SET dvh = public.fn_sha256_hex(
        trim(codigo_ncm) || '|' || trim(descripcion) || '|' || upper(trim(tipo_bien)) || '|' || trim(to_char(aec, 'FM999990.00')) || '|' || trim(to_char(iva_alicuota, 'FM999990.00')) || '|' || trim(coalesce(anexo_excepcion, '')) || '|' || coalesce(trim(to_char(arancel_excepcion, 'FM999990.00')), '')
    )
    WHERE TRUE;

    INSERT INTO public.digito_verificador (tabla_nombre, dvv, cantidad_registros, snapshot_control, updated_at, updated_by)
    SELECT
        'admin_usuarios',
        public.fn_sha256_hex(coalesce(string_agg(dvh, '|' ORDER BY dvh COLLATE "C" ASC), 'EMPTY')),
        count(*)::INT,
        coalesce(jsonb_agg(jsonb_build_object(
            'pk', username,
            'dvh', dvh,
            'campos', jsonb_build_object(
                'username', username,
                'password_hash', left(password_hash, 15) || '...',
                'activo', activo
            )
        ) ORDER BY username), '[]'::jsonb),
        NOW(),
        p_usuario
    FROM public.admin_usuarios
    ON CONFLICT (tabla_nombre) DO UPDATE
    SET dvv = EXCLUDED.dvv,
        cantidad_registros = EXCLUDED.cantidad_registros,
        snapshot_control = EXCLUDED.snapshot_control,
        updated_at = NOW(),
        updated_by = EXCLUDED.updated_by;

    INSERT INTO public.digito_verificador (tabla_nombre, dvv, cantidad_registros, snapshot_control, updated_at, updated_by)
    SELECT
        'forwarders',
        public.fn_sha256_hex(coalesce(string_agg(dvh, '|' ORDER BY dvh COLLATE "C" ASC), 'EMPTY')),
        count(*)::INT,
        coalesce(jsonb_agg(jsonb_build_object(
            'pk', lower(id::text),
            'dvh', dvh,
            'campos', jsonb_build_object(
                'nombre', nombre,
                'imagen_url', imagen_url,
                'whatsapp', whatsapp,
                'email', email,
                'orden', orden,
                'activo', activo
            )
        ) ORDER BY orden), '[]'::jsonb),
        NOW(),
        p_usuario
    FROM public.forwarders
    ON CONFLICT (tabla_nombre) DO UPDATE
    SET dvv = EXCLUDED.dvv,
        cantidad_registros = EXCLUDED.cantidad_registros,
        snapshot_control = EXCLUDED.snapshot_control,
        updated_at = NOW(),
        updated_by = EXCLUDED.updated_by;

    INSERT INTO public.digito_verificador (tabla_nombre, dvv, cantidad_registros, snapshot_control, updated_at, updated_by)
    SELECT
        'puertos_aduanas',
        public.fn_sha256_hex(coalesce(string_agg(dvh, '|' ORDER BY dvh COLLATE "C" ASC), 'EMPTY')),
        count(*)::INT,
        coalesce(jsonb_agg(jsonb_build_object(
            'pk', lower(id::text),
            'dvh', dvh,
            'campos', jsonb_build_object(
                'nombre', nombre,
                'via_ingreso', via_ingreso,
                'provincia', provincia,
                'codigo_sim', coalesce(codigo_sim, ''),
                'operador', coalesce(operador, '')
            )
        ) ORDER BY nombre), '[]'::jsonb),
        NOW(),
        p_usuario
    FROM public.puertos_aduanas
    ON CONFLICT (tabla_nombre) DO UPDATE
    SET dvv = EXCLUDED.dvv,
        cantidad_registros = EXCLUDED.cantidad_registros,
        snapshot_control = EXCLUDED.snapshot_control,
        updated_at = NOW(),
        updated_by = EXCLUDED.updated_by;

    INSERT INTO public.digito_verificador (tabla_nombre, dvv, cantidad_registros, snapshot_control, updated_at, updated_by)
    SELECT
        'paises',
        public.fn_sha256_hex(coalesce(string_agg(dvh, '|' ORDER BY dvh COLLATE "C" ASC), 'EMPTY')),
        count(*)::INT,
        coalesce(jsonb_agg(jsonb_build_object(
            'pk', codigo_iso,
            'dvh', dvh,
            'campos', jsonb_build_object(
                'nombre', nombre,
                'es_mercosur', es_mercosur,
                'flete_estimado_pct', trim(to_char(flete_estimado_pct, 'FM999990.00'))
            )
        ) ORDER BY codigo_iso), '[]'::jsonb),
        NOW(),
        p_usuario
    FROM public.paises
    ON CONFLICT (tabla_nombre) DO UPDATE
    SET dvv = EXCLUDED.dvv,
        cantidad_registros = EXCLUDED.cantidad_registros,
        snapshot_control = EXCLUDED.snapshot_control,
        updated_at = NOW(),
        updated_by = EXCLUDED.updated_by;

    INSERT INTO public.digito_verificador (tabla_nombre, dvv, cantidad_registros, snapshot_control, updated_at, updated_by)
    SELECT
        'ncm_catalogo',
        public.fn_sha256_hex(coalesce(string_agg(dvh, '|' ORDER BY dvh COLLATE "C" ASC), 'EMPTY')),
        count(*)::INT,
        coalesce(jsonb_agg(jsonb_build_object(
            'pk', codigo_ncm,
            'dvh', dvh,
            'campos', jsonb_build_object(
                'descripcion', left(descripcion, 80),
                'tipo_bien', tipo_bien,
                'aec', trim(to_char(aec, 'FM999990.00')),
                'iva_alicuota', trim(to_char(iva_alicuota, 'FM999990.00'))
            )
        ) ORDER BY codigo_ncm), '[]'::jsonb),
        NOW(),
        p_usuario
    FROM public.ncm_catalogo
    ON CONFLICT (tabla_nombre) DO UPDATE
    SET dvv = EXCLUDED.dvv,
        cantidad_registros = EXCLUDED.cantidad_registros,
        snapshot_control = EXCLUDED.snapshot_control,
        updated_at = NOW(),
        updated_by = EXCLUDED.updated_by;
END;
$$;

REVOKE ALL ON FUNCTION public.recalcular_digitos_verificadores(VARCHAR) FROM PUBLIC, anon, authenticated;
GRANT EXECUTE ON FUNCTION public.recalcular_digitos_verificadores(VARCHAR) TO service_role;

SELECT public.recalcular_digitos_verificadores('INITIAL_SETUP');

COMMIT;
