import 'server-only';
import { cargarPaisesExcel } from './excelClient';
import { getCachedOrFetch, supabaseSelect, isSupabaseConfigured } from './supabaseClient';
import { PaisBE } from '../be/PaisBE';

/**
 * Capa DAL: Acceso Híbrido a Países (Supabase con caché L1 30 min + Catálogo de 195 países)
 * Corrige bug de retorno silencioso de China cuando el código ISO no existe.
 */
export class PaisDAL {
  static async obtenerTodos() {
    const basePaises = cargarPaisesExcel();
    const map = new Map(basePaises.map((p) => [p.codigo_iso.toUpperCase(), p]));

    if (isSupabaseConfigured()) {
      const supaRows = await getCachedOrFetch('paises:all', 30 * 60 * 1000, async () => {
        return await supabaseSelect('paises', 'select=codigo_iso,nombre,es_mercosur,flete_estimado_pct&order=nombre.asc');
      });

      if (Array.isArray(supaRows)) {
        for (const r of supaRows) {
          if (r.codigo_iso) {
            map.set(r.codigo_iso.toUpperCase(), {
              codigo_iso: r.codigo_iso.toUpperCase(),
              nombre: r.nombre,
              es_mercosur: Boolean(r.es_mercosur),
              flete_estimado_pct: Number(r.flete_estimado_pct) || 8.0
            });
          }
        }
      }
    }

    return Array.from(map.values())
      .sort((a, b) => String(a.nombre).localeCompare(String(b.nombre), 'es'))
      .map((p) => new PaisBE(p));
  }

  static async obtenerPorCodigoIso(codigoIso) {
    const cleanIso = (codigoIso || '').trim().toUpperCase();
    if (!cleanIso) return null;

    const todos = await this.obtenerTodos();
    const match = todos.find((p) => p.codigo_iso === cleanIso);
    return match || null;
  }
}
