import 'server-only';
import { cargarPaisesExcel } from './excelClient';
import { PaisBE } from '../be/PaisBE';

/**
 * Capa DAL: Acceso a Datos de Países y Fletes Paramétricos (Persistencia en Excel)
 */
export class PaisDAL {
  static async obtenerTodos() {
    const paises = cargarPaisesExcel();
    return paises.map((p) => new PaisBE(p));
  }

  static async obtenerPorCodigoIso(codigoIso) {
    const cleanIso = (codigoIso || '').trim().toUpperCase();
    const paises = cargarPaisesExcel();
    const match = paises.find((p) => p.codigo_iso === cleanIso);
    return match ? new PaisBE(match) : new PaisBE(paises[0]);
  }
}
