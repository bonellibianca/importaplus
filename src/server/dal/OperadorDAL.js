import 'server-only';
import { cargarOperadoresExcel, guardarSolicitudContactoEnExcel } from './excelClient';
import { supabaseInsert, isSupabaseConfigured } from './supabaseClient';
import { OperadorBE } from '../be/OperadorBE';

/**
 * Capa DAL: Operadores y registro protegido de solicitudes de contacto en Supabase
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

  static async guardarSolicitudContacto(datosLead) {
    if (isSupabaseConfigured()) {
      const res = await supabaseInsert('solicitudes_contacto', {
        operador_id: datosLead.operadorId || null,
        id_cotizacion: datosLead.idCotizacion || null,
        nombre_apellido: datosLead.nombreApellido,
        empresa: datosLead.empresa || null,
        email: datosLead.email,
        telefono: datosLead.telefono,
        comentarios: datosLead.comentarios || null
      });
      if (res.ok) {
        return { exito: true, leadId: `SUPA-LEAD-${Date.now()}` };
      }
    }

    return guardarSolicitudContactoEnExcel(datosLead);
  }
}
