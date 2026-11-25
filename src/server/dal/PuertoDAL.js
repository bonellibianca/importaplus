import 'server-only';
import { cargarPuertosExcel } from './excelClient';
import { getCachedOrFetch, supabaseSelect, isSupabaseConfigured } from './supabaseClient';
import { PuertoBE } from '../be/PuertoBE';

/**
 * Capa DAL: Acceso a Puertos y Aduanas (Supabase con caché L1 15 min + Respaldo local)
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

    const puertos = cargarPuertosExcel();
    return puertos.map((p) => new PuertoBE(p));
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
