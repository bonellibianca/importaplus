import 'server-only';
import { cargarOperadoresExcel, guardarSolicitudContactoEnExcel } from './excelClient';
import { OperadorBE } from '../be/OperadorBE';

/**
 * Capa DAL: Acceso a Datos de Despachantes y Forwarders Matriculados (PN4 - Persistencia en Excel)
 */
export class OperadorDAL {
  static async obtenerActivos(aduanaNombre = '') {
    const cleanAduana = (aduanaNombre || '').trim().toLowerCase();
    let list = cargarOperadoresExcel().filter((op) => op.activo);

    if (cleanAduana) {
      const filtered = list.filter((op) =>
        op.aduanas_operativas.some((a) => a.toLowerCase().includes(cleanAduana))
      );
      if (filtered.length > 0) list = filtered;
    }

    list.sort((a, b) => b.rating - a.rating);
    return list.map((op) => new OperadorBE(op));
  }

  /**
   * Registra la solicitud de contacto con el operador logístico directamente en la planilla Excel
   */
  static async guardarSolicitudContacto(datosLead) {
    return guardarSolicitudContactoEnExcel(datosLead);
  }
}
