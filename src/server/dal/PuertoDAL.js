import 'server-only';
import fs from 'fs';
import path from 'path';
import { cargarPuertosExcel } from './excelClient';
import { supabaseSelect, isSupabaseConfigured } from './supabaseClient';
import { PuertoBE } from '../be/PuertoBE';

const CLAVE_PARAM_PUERTOS = 'CATALOGO_PUERTOS_JSON';

/**
 * Capa DAL: Acceso en tiempo real a Puertos y Aduanas con links de Google Maps
 * Prioridad: Supabase Cloud (puertos_aduanas + parametros_sistema) -> Espejo Admin web_sync.json -> Respaldo CSV/Semilla
 */
export class PuertoDAL {
  static async obtenerMapaLinksDesdeParametros() {
    if (!isSupabaseConfigured()) return new Map();
    try {
      const paramRows = await supabaseSelect(
        'parametros_sistema',
        `select=clave,descripcion&clave=eq.${CLAVE_PARAM_PUERTOS}&limit=1`
      );
      if (Array.isArray(paramRows) && paramRows.length > 0 && paramRows[0]?.descripcion) {
        const parsed = JSON.parse(paramRows[0].descripcion);
        if (Array.isArray(parsed)) {
          return new Map(parsed.map((p) => [String(p.id || '').toLowerCase(), p]));
        }
      }
    } catch {}
    return new Map();
  }

  static async obtenerTodos() {
    const basePuertos = cargarPuertosExcel();
    const baseMap = new Map(basePuertos.map((p) => [String(p.id || '').toLowerCase(), p]));

    if (isSupabaseConfigured()) {
      let supaRows = await supabaseSelect(
        'puertos_aduanas',
        'select=id,nombre,via_ingreso,provincia,codigo_sim,operador,tipos_carga,infraestructura,latitud,longitud,paises_frecuentes,google_maps_url&order=nombre.asc'
      );

      if (!Array.isArray(supaRows)) {
        supaRows = await supabaseSelect(
          'puertos_aduanas',
          'select=id,nombre,via_ingreso,provincia,codigo_sim,operador,tipos_carga,infraestructura,latitud,longitud,paises_frecuentes&order=nombre.asc'
        );
      }

      if (Array.isArray(supaRows)) {
        const paramMap = await this.obtenerMapaLinksDesdeParametros();
        return supaRows.map((p) => {
          const idKey = String(p.id || '').toLowerCase();
          const previo = baseMap.get(idKey) || {};
          const extraParam = paramMap.get(idKey) || {};
          return new PuertoBE({
            ...previo,
            ...p,
            google_maps_url: p.google_maps_url || extraParam.google_maps_url || extraParam.GoogleMapsUrl || previo.google_maps_url || ''
          });
        });
      }
    }

    const syncPath = path.join(process.cwd(), 'DAL/database/web_sync.json');
    if (fs.existsSync(syncPath)) {
      try {
        const raw = JSON.parse(fs.readFileSync(syncPath, 'utf8').replace(/^\uFEFF/, ''));
        if (Array.isArray(raw.puertos)) {
          return raw.puertos.map((sp) => {
            const previo = baseMap.get(String(sp.id || '').toLowerCase()) || {};
            return new PuertoBE({
              ...previo,
              ...sp,
              tipos_carga: Array.isArray(sp.tipos_carga) && sp.tipos_carga.length > 0
                ? sp.tipos_carga
                : (previo.tipos_carga || ['GENERAL']),
              paises_frecuentes: Array.isArray(sp.paises_frecuentes) && sp.paises_frecuentes.length > 0
                ? sp.paises_frecuentes
                : (previo.paises_frecuentes || [])
            });
          });
        }
      } catch {}
    }

    return basePuertos.map((p) => new PuertoBE(p));
  }

  static async obtenerSugeridosPorPais(codigoIso) {
    const cleanIso = (codigoIso || '').trim().toUpperCase();
    const puertos = await this.obtenerTodos();
    if (!cleanIso) return puertos;

    // Incluir puertos asociados al país Y cualquier puerto nuevo creado en AdminImportaPlus (sin lista cerrada de países)
    const matches = puertos.filter(
      (p) => !Array.isArray(p.paises_frecuentes) || p.paises_frecuentes.length === 0 || p.paises_frecuentes.includes(cleanIso)
    );

    return matches.length > 0 ? matches : puertos;
  }
}
