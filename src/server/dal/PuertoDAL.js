import 'server-only';
import fs from 'fs';
import path from 'path';
import { cargarPuertosExcel } from './excelClient';
import { getCachedOrFetch, supabaseSelect, isSupabaseConfigured } from './supabaseClient';
import { PuertoBE } from '../be/PuertoBE';

/**
 * Capa DAL: Acceso a Puertos y Aduanas (Supabase con caché L1 15 min -> Espejo Admin web_sync.json -> Respaldo CSV/Semilla)
 */
export class PuertoDAL {
  static async obtenerTodos() {
    if (isSupabaseConfigured()) {
      const supaRows = await getCachedOrFetch('puertos:all', 15 * 60 * 1000, async () => {
        return await supabaseSelect(
          'puertos_aduanas',
          'select=id,nombre,via_ingreso,provincia,codigo_sim,operador,tipos_carga,infraestructura,latitud,longitud,paises_frecuentes&order=nombre.asc'
        );
      });

      if (Array.isArray(supaRows) && supaRows.length > 0) {
        return supaRows.map((p) => new PuertoBE(p));
      }
    }

    const basePuertos = cargarPuertosExcel();
    const syncPath = path.join(process.cwd(), 'DAL/database/web_sync.json');
    if (fs.existsSync(syncPath)) {
      try {
        const raw = JSON.parse(fs.readFileSync(syncPath, 'utf8').replace(/^\uFEFF/, ''));
        if (Array.isArray(raw.puertos) && raw.puertos.length > 0) {
          const baseMap = new Map(basePuertos.map((p) => [String(p.id).toLowerCase(), p]));
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

    const matches = puertos.filter(
      (p) => Array.isArray(p.paises_frecuentes) && p.paises_frecuentes.includes(cleanIso)
    );

    return matches.length > 0 ? matches : puertos.slice(0, 3);
  }
}
