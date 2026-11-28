-- ============================================================================
-- IMPORTA+ · ACTIVACIÓN DE ESCRITURA DIRECTA DESDE AdminImportaPlus.exe
-- ============================================================================
-- Ejecutá este script UNA SOLA VEZ en Supabase -> SQL Editor -> Run.
-- Permite que cualquier usuario autenticado en AdminImportaPlus.exe (con su
-- usuario, contraseña BCrypt y permisos de botón) aplique altas, bajas y
-- modificaciones DIRECTAMENTE en Supabase usando la Publishable Key integrada,
-- SIN pedir ni exponer jamás una Secret Key (sb_secret_...).
-- ============================================================================

BEGIN;

CREATE SCHEMA IF NOT EXISTS extensions;
CREATE EXTENSION IF NOT EXISTS pgcrypto WITH SCHEMA extensions;

-- 1. Asegurar tabla public.despachantes, columnas extendidas de usuarios y RBAC
CREATE TABLE IF NOT EXISTS public.despachantes (
    id UUID DEFAULT gen_random_uuid() PRIMARY KEY,
    nombre VARCHAR(120) NOT NULL CHECK (char_length(trim(nombre)) BETWEEN 1 AND 120),
    imagen_url TEXT NOT NULL,
    whatsapp VARCHAR(40) NOT NULL,
    email VARCHAR(150) NOT NULL,
    orden INT NOT NULL DEFAULT 1 CHECK (orden BETWEEN 1 AND 999),
    activo BOOLEAN NOT NULL DEFAULT TRUE,
    created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    dvh VARCHAR(64) NOT NULL DEFAULT ''
);

CREATE INDEX IF NOT EXISTS idx_despachantes_activos_orden ON public.despachantes (activo, orden ASC);
ALTER TABLE public.despachantes ENABLE ROW LEVEL SECURITY;

GRANT SELECT (id, nombre, imagen_url, whatsapp, email, orden, activo) ON public.despachantes TO anon, authenticated;
DROP POLICY IF EXISTS "Lectura pública Despachantes Activos" ON public.despachantes;
CREATE POLICY "Lectura pública Despachantes Activos" ON public.despachantes FOR SELECT TO anon, authenticated USING (activo = true);

INSERT INTO public.despachantes (id, nombre, imagen_url, whatsapp, email, orden, activo) VALUES
('e1111111-1111-1111-1111-111111111111', 'Bonelli & Asociados Despachantes DGA', 'https://images.unsplash.com/photo-1554224155-6726b3ff858f?auto=format&fit=crop&w=600&q=80', '+5491143218800', 'despachos@importaplus.com.ar', 1, TRUE),
('e2222222-2222-2222-2222-222222222222', 'Ezeiza Express Despachos Aduaneros', 'https://images.unsplash.com/photo-1454165804606-c3d57bc86b40?auto=format&fit=crop&w=600&q=80', '+5491144802233', 'ezeiza@importaplus.com.ar', 2, TRUE),
('e3333333-3333-3333-3333-333333333333', 'Estudio Aduanero Río de la Plata', 'https://images.unsplash.com/photo-1507679799987-c73779587ccf?auto=format&fit=crop&w=600&q=80', '+5491150903344', 'aduanas@importaplus.com.ar', 3, TRUE),
('e4444444-4444-4444-4444-444444444444', 'Andes & Pacífico Despachantes Matriculados', 'https://images.unsplash.com/photo-1521791136064-7986c2920216?auto=format&fit=crop&w=600&q=80', '+5492614205544', 'cuyo@importaplus.com.ar', 4, TRUE)
ON CONFLICT (id) DO NOTHING;

ALTER TABLE public.puertos_aduanas ADD COLUMN IF NOT EXISTS google_maps_url TEXT DEFAULT '';
GRANT SELECT (id, nombre, via_ingreso, provincia, codigo_sim, operador, tipos_carga, infraestructura, latitud, longitud, paises_frecuentes, google_maps_url) ON public.puertos_aduanas TO anon, authenticated;

UPDATE public.puertos_aduanas SET google_maps_url = 'https://www.google.com/maps/place/Encarnaci%C3%B3n+Ezcurra+320,+C1107+Cdad.+Aut%C3%B3noma+de+Buenos+Aires/@-34.5908026,-58.3704644,16.79z/data=!4m6!3m5!1s0x95a334d72473ee5b:0x11b42bbb998547cc!8m2!3d-34.6160181!4d-58.362668!16s%2Fg%2F11r_wsrldj?entry=ttu&g_ep=EgoyMDI2MTEyMy4wIKXMDSoASAFQAw%3D%3D' WHERE lower(trim(nombre)) = 'puerto de buenos aires' AND coalesce(google_maps_url, '') = '';
UPDATE public.puertos_aduanas SET google_maps_url = 'https://www.google.com/maps?rlz=1C1CHBF_esAR1044AR1044&fvr=1&pvq=CgkvbS8wMWt5M2Q&vet=10CAAQoqAOahcKEwjA8MSXjKmYAxUAAAAAHQAAAAAQDQ..i&cs=0&um=1&ie=UTF-8&fb=1&gl=ar&sa=X&ftid=0x95bcd0f4e2dea557:0xf19b6f81d441cc3b' WHERE lower(trim(nombre)) = 'aduana de ezeiza' AND coalesce(google_maps_url, '') = '';
UPDATE public.puertos_aduanas SET google_maps_url = 'https://www.google.com/maps/place/Puerto+Rosario/data=!4m2!3m1!1s0x0:0x7e1e8c154b58d906?sa=X&ved=1t:2428&ictx=111' WHERE lower(trim(nombre)) = 'puerto de rosario' AND coalesce(google_maps_url, '') = '';
UPDATE public.puertos_aduanas SET google_maps_url = 'https://www.google.com/maps/place/Puerto+de+Mar+del+Plata/@-38.050254,-57.5463923,16z/data=!4m10!1m2!2m1!1sPuerto+de+Mar+del+Plata!3m6!1s0x9584ddf29129483b:0x4d4bf245aff78945!8m2!3d-38.050254!4d-57.5381526!15sChdQdWVydG8gZGUgTWFyIGRlbCBQbGF0YVoZIhdwdWVydG8gZGUgbWFyIGRlbCBwbGF0YZIBDGZpc2hpbmdfcGllcpoBJENoZERTVWhOTUc5blMwVkpRMEZuU1VSMGEweHFlbTlCUlJBQuABAPoBBAgAED4!16s%2Fg%2F120mbqxd?entry=ttu&g_ep=EgoyMDI2MTEyMy4wIKXMDSoASAFQAw%3D%3D' WHERE lower(trim(nombre)) = 'puerto de mar del plata' AND coalesce(google_maps_url, '') = '';
UPDATE public.puertos_aduanas SET google_maps_url = 'https://www.google.com/maps/place/Paso+Cristo+Redentor/data=!4m2!3m1!1s0x9687f4f987c60a21:0x85b25d9333d7e10b?sa=X&ved=1t:155783&ictx=111' WHERE lower(trim(nombre)) = 'paso cristo redentor' AND coalesce(google_maps_url, '') = '';

ALTER TABLE public.admin_usuarios ADD COLUMN IF NOT EXISTS dni INT DEFAULT 11111111;
ALTER TABLE public.admin_usuarios ADD COLUMN IF NOT EXISTS nombre VARCHAR(80) DEFAULT 'Administrador';
ALTER TABLE public.admin_usuarios ADD COLUMN IF NOT EXISTS apellido VARCHAR(80) DEFAULT 'General';
ALTER TABLE public.admin_usuarios ADD COLUMN IF NOT EXISTS bloqueado BOOLEAN NOT NULL DEFAULT FALSE;
ALTER TABLE public.admin_usuarios ADD COLUMN IF NOT EXISTS perfil_id UUID;

CREATE TABLE IF NOT EXISTS public.admin_seguridad_rbac (
    clave VARCHAR(80) PRIMARY KEY,
    payload JSONB NOT NULL,
    updated_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW()
);
ALTER TABLE public.admin_seguridad_rbac ENABLE ROW LEVEL SECURITY;

-- 2. RPC segura de lectura y autenticación para AdminImportaPlus.exe
CREATE OR REPLACE FUNCTION public.admin_rpc_obtener_estado(
    p_username TEXT,
    p_password TEXT
)
RETURNS JSONB
LANGUAGE plpgsql
SECURITY DEFINER
SET search_path = public, extensions, pg_temp
AS $$
DECLARE
    v_user public.admin_usuarios%ROWTYPE;
    v_rbac JSONB;
BEGIN
    SELECT * INTO v_user
    FROM public.admin_usuarios
    WHERE lower(trim(username)) = lower(trim(coalesce(p_username, '')))
    LIMIT 1;

    IF NOT FOUND THEN
        RETURN jsonb_build_object('status', 'NOT_FOUND');
    END IF;

    IF NOT v_user.activo THEN
        RETURN jsonb_build_object('status', 'INACTIVE');
    END IF;

    IF coalesce(v_user.bloqueado, FALSE) THEN
        RETURN jsonb_build_object('status', 'LOCKED');
    END IF;

    IF extensions.crypt(coalesce(p_password, ''), v_user.password_hash) <> v_user.password_hash THEN
        RETURN jsonb_build_object('status', 'BAD_PASSWORD');
    END IF;

    UPDATE public.admin_usuarios SET ultimo_acceso = NOW() WHERE id = v_user.id;

    SELECT payload INTO v_rbac
    FROM public.admin_seguridad_rbac
    WHERE clave = 'ROLES_STORE_V2'
    LIMIT 1;

    RETURN jsonb_build_object(
        'status', 'OK',
        'admin_usuarios', coalesce((SELECT jsonb_agg(to_jsonb(u) ORDER BY u.username) FROM public.admin_usuarios u), '[]'::jsonb),
        'forwarders', coalesce((SELECT jsonb_agg(to_jsonb(f) ORDER BY f.orden) FROM public.forwarders f), '[]'::jsonb),
        'despachantes', coalesce((SELECT jsonb_agg(to_jsonb(d) ORDER BY d.orden) FROM public.despachantes d), '[]'::jsonb),
        'puertos_aduanas', coalesce((SELECT jsonb_agg(to_jsonb(p) ORDER BY p.nombre) FROM public.puertos_aduanas p), '[]'::jsonb),
        'digito_verificador', coalesce((
            SELECT jsonb_agg(
                CASE
                    WHEN dv.tabla_nombre IN ('paises', 'ncm_catalogo') THEN
                        jsonb_build_object(
                            'tabla_nombre', dv.tabla_nombre,
                            'dvv', dv.dvv,
                            'cantidad_registros', dv.cantidad_registros,
                            'snapshot_control', '[]'::jsonb,
                            'updated_at', dv.updated_at,
                            'updated_by', dv.updated_by
                        )
                    ELSE to_jsonb(dv)
                END
            )
            FROM public.digito_verificador dv
        ), '[]'::jsonb),
        'rbac_store', v_rbac,
        'total_paises', (SELECT count(*)::INT FROM public.paises),
        'total_ncm', (SELECT count(*)::INT FROM public.ncm_catalogo)
    );
END;
$$;

REVOKE ALL ON FUNCTION public.admin_rpc_obtener_estado(TEXT, TEXT) FROM PUBLIC;
GRANT EXECUTE ON FUNCTION public.admin_rpc_obtener_estado(TEXT, TEXT) TO anon, authenticated, service_role;

-- 3. RPC segura de escritura/borrado directo para usuarios autenticados en AdminImportaPlus.exe
CREATE OR REPLACE FUNCTION public.admin_rpc_sincronizar_estado(
    p_username TEXT,
    p_password TEXT,
    p_payload JSONB
)
RETURNS JSONB
LANGUAGE plpgsql
SECURITY DEFINER
SET search_path = public, extensions, pg_temp
AS $$
DECLARE
    v_user public.admin_usuarios%ROWTYPE;
BEGIN
    SELECT * INTO v_user
    FROM public.admin_usuarios
    WHERE lower(trim(username)) = lower(trim(coalesce(p_username, '')))
    LIMIT 1;

    IF NOT FOUND OR NOT v_user.activo OR coalesce(v_user.bloqueado, FALSE) THEN
        RAISE EXCEPTION 'Sesión administrativa inválida o sin permisos activos.';
    END IF;

    IF extensions.crypt(coalesce(p_password, ''), v_user.password_hash) <> v_user.password_hash THEN
        RAISE EXCEPTION 'Credenciales de sesión inválidas para escribir en Supabase.';
    END IF;

    -- Sincronizar Forwarders (incluye eliminación real de registros quitados)
    IF p_payload ? 'forwarders' THEN
        DELETE FROM public.forwarders
        WHERE id NOT IN (
            SELECT (elem->>'id')::UUID
            FROM jsonb_array_elements(p_payload->'forwarders') AS elem
            WHERE elem ? 'id'
        );

        INSERT INTO public.forwarders (id, nombre, imagen_url, whatsapp, email, orden, activo, dvh, updated_at)
        SELECT
            (elem->>'id')::UUID,
            elem->>'nombre',
            elem->>'imagen_url',
            elem->>'whatsapp',
            elem->>'email',
            coalesce((elem->>'orden')::INT, 1),
            coalesce((elem->>'activo')::BOOLEAN, TRUE),
            coalesce(elem->>'dvh', ''),
            NOW()
        FROM jsonb_array_elements(p_payload->'forwarders') AS elem
        ON CONFLICT (id) DO UPDATE SET
            nombre = EXCLUDED.nombre,
            imagen_url = EXCLUDED.imagen_url,
            whatsapp = EXCLUDED.whatsapp,
            email = EXCLUDED.email,
            orden = EXCLUDED.orden,
            activo = EXCLUDED.activo,
            dvh = EXCLUDED.dvh,
            updated_at = NOW();
    END IF;

    -- Sincronizar Despachantes de Aduana (incluye eliminación real de registros quitados)
    IF p_payload ? 'despachantes' THEN
        DELETE FROM public.despachantes
        WHERE id NOT IN (
            SELECT (elem->>'id')::UUID
            FROM jsonb_array_elements(p_payload->'despachantes') AS elem
            WHERE elem ? 'id'
        );

        INSERT INTO public.despachantes (id, nombre, imagen_url, whatsapp, email, orden, activo, dvh, updated_at)
        SELECT
            (elem->>'id')::UUID,
            elem->>'nombre',
            elem->>'imagen_url',
            elem->>'whatsapp',
            elem->>'email',
            coalesce((elem->>'orden')::INT, 1),
            coalesce((elem->>'activo')::BOOLEAN, TRUE),
            coalesce(elem->>'dvh', ''),
            NOW()
        FROM jsonb_array_elements(p_payload->'despachantes') AS elem
        ON CONFLICT (id) DO UPDATE SET
            nombre = EXCLUDED.nombre,
            imagen_url = EXCLUDED.imagen_url,
            whatsapp = EXCLUDED.whatsapp,
            email = EXCLUDED.email,
            orden = EXCLUDED.orden,
            activo = EXCLUDED.activo,
            dvh = EXCLUDED.dvh,
            updated_at = NOW();

        INSERT INTO public.parametros_sistema (clave, valor, descripcion, updated_at)
        VALUES ('CATALOGO_DESPACHANTES_JSON', 'SYNC_RPC', (p_payload->'despachantes')::TEXT, NOW())
        ON CONFLICT (clave) DO UPDATE SET
            valor = EXCLUDED.valor,
            descripcion = EXCLUDED.descripcion,
            updated_at = NOW();
    END IF;

    -- Sincronizar Puertos y Aduanas
    IF p_payload ? 'puertos_aduanas' THEN
        DELETE FROM public.puertos_aduanas
        WHERE id NOT IN (
            SELECT (elem->>'id')::UUID
            FROM jsonb_array_elements(p_payload->'puertos_aduanas') AS elem
            WHERE elem ? 'id'
        );

        INSERT INTO public.puertos_aduanas (id, nombre, via_ingreso, provincia, codigo_sim, operador, infraestructura, latitud, longitud, google_maps_url, dvh)
        SELECT
            (elem->>'id')::UUID,
            elem->>'nombre',
            elem->>'via_ingreso',
            elem->>'provincia',
            elem->>'codigo_sim',
            elem->>'operador',
            elem->>'infraestructura',
            NULLIF(elem->>'latitud', '')::NUMERIC,
            NULLIF(elem->>'longitud', '')::NUMERIC,
            coalesce(elem->>'google_maps_url', ''),
            coalesce(elem->>'dvh', '')
        FROM jsonb_array_elements(p_payload->'puertos_aduanas') AS elem
        ON CONFLICT (id) DO UPDATE SET
            nombre = EXCLUDED.nombre,
            via_ingreso = EXCLUDED.via_ingreso,
            provincia = EXCLUDED.provincia,
            codigo_sim = EXCLUDED.codigo_sim,
            operador = EXCLUDED.operador,
            infraestructura = EXCLUDED.infraestructura,
            latitud = EXCLUDED.latitud,
            longitud = EXCLUDED.longitud,
            google_maps_url = EXCLUDED.google_maps_url,
            dvh = EXCLUDED.dvh;

        INSERT INTO public.parametros_sistema (clave, valor, descripcion, updated_at)
        VALUES ('CATALOGO_PUERTOS_JSON', 'SYNC_RPC', (p_payload->'puertos_aduanas')::TEXT, NOW())
        ON CONFLICT (clave) DO UPDATE SET
            valor = EXCLUDED.valor,
            descripcion = EXCLUDED.descripcion,
            updated_at = NOW();
    END IF;

    -- Sincronizar Usuarios Administrativos
    IF p_payload ? 'admin_usuarios' AND jsonb_array_length(p_payload->'admin_usuarios') > 0 THEN
        DELETE FROM public.admin_usuarios
        WHERE id NOT IN (
            SELECT (elem->>'id')::UUID
            FROM jsonb_array_elements(p_payload->'admin_usuarios') AS elem
            WHERE elem ? 'id'
        );

        INSERT INTO public.admin_usuarios (id, dni, nombre, apellido, username, password_hash, bloqueado, activo, perfil_id, password_updated_at, dvh)
        SELECT
            (elem->>'id')::UUID,
            coalesce((elem->>'dni')::INT, 11111111),
            coalesce(elem->>'nombre', 'Usuario'),
            coalesce(elem->>'apellido', 'Sistema'),
            elem->>'username',
            elem->>'password_hash',
            coalesce((elem->>'bloqueado')::BOOLEAN, FALSE),
            coalesce((elem->>'activo')::BOOLEAN, TRUE),
            NULLIF(elem->>'perfil_id', '')::UUID,
            coalesce((elem->>'password_updated_at')::TIMESTAMPTZ, NOW()),
            coalesce(elem->>'dvh', '')
        FROM jsonb_array_elements(p_payload->'admin_usuarios') AS elem
        ON CONFLICT (username) DO UPDATE SET
            dni = EXCLUDED.dni,
            nombre = EXCLUDED.nombre,
            apellido = EXCLUDED.apellido,
            password_hash = EXCLUDED.password_hash,
            bloqueado = EXCLUDED.bloqueado,
            activo = EXCLUDED.activo,
            perfil_id = EXCLUDED.perfil_id,
            password_updated_at = EXCLUDED.password_updated_at,
            dvh = EXCLUDED.dvh;
    END IF;

    -- Sincronizar Perfiles y Permisos (Botones)
    IF p_payload ? 'rbac_store' AND p_payload->'rbac_store' IS NOT NULL AND jsonb_typeof(p_payload->'rbac_store') = 'object' THEN
        INSERT INTO public.admin_seguridad_rbac (clave, payload, updated_at)
        VALUES ('ROLES_STORE_V2', p_payload->'rbac_store', NOW())
        ON CONFLICT (clave) DO UPDATE SET
            payload = EXCLUDED.payload,
            updated_at = NOW();
    END IF;

    -- Sincronizar Dígitos Verificadores
    IF p_payload ? 'digito_verificador' THEN
        INSERT INTO public.digito_verificador (tabla_nombre, dvv, cantidad_registros, snapshot_control, updated_at, updated_by)
        SELECT
            elem->>'tabla_nombre',
            elem->>'dvv',
            coalesce((elem->>'cantidad_registros')::INT, 0),
            coalesce(elem->'snapshot_control', '[]'::jsonb),
            NOW(),
            p_username
        FROM jsonb_array_elements(p_payload->'digito_verificador') AS elem
        WHERE (elem->>'tabla_nombre') NOT IN ('paises', 'ncm_catalogo')
        ON CONFLICT (tabla_nombre) DO UPDATE SET
            dvv = EXCLUDED.dvv,
            cantidad_registros = EXCLUDED.cantidad_registros,
            snapshot_control = EXCLUDED.snapshot_control,
            updated_at = NOW(),
            updated_by = EXCLUDED.updated_by;
    END IF;

    RETURN jsonb_build_object('ok', TRUE, 'updated_by', p_username, 'ts', NOW());
END;
$$;

REVOKE ALL ON FUNCTION public.admin_rpc_sincronizar_estado(TEXT, TEXT, JSONB) FROM PUBLIC;
GRANT EXECUTE ON FUNCTION public.admin_rpc_sincronizar_estado(TEXT, TEXT, JSONB) TO anon, authenticated, service_role;

COMMIT;
