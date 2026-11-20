import 'server-only';
import { cargarPuertosExcel } from './excelClient';
import { PuertoBE } from '../be/PuertoBE';

/**
 * Capa DAL: Acceso a Datos de Puertos y Aduanas Nacionales (PN3 - Persistencia en Excel)
 */
export class PuertoDAL {
  static async obtenerTodos() {
    const puertos = cargarPuertosExcel();
    return puertos.map((p) => new PuertoBE(p));
  }

  static async obtenerSugeridosPorPais(codigoIso) {
    const cleanIso = (codigoIso || '').trim().toUpperCase();
    const puertos = cargarPuertosExcel();

    const matches = puertos.filter(
      (p) => Array.isArray(p.paises_frecuentes) && p.paises_frecuentes.includes(cleanIso)
    );

    return (matches.length > 0 ? matches : puertos.slice(0, 3)).map((p) => new PuertoBE(p));
  }
}
