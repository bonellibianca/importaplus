import 'server-only';
import { OperadorDAL } from '../dal/OperadorDAL';
import { SimulacionDAL } from '../dal/SimulacionDAL';

/**
 * Capa BLL: Gestión de Solicitudes de Cotización y Contacto con Forwarders (PN4)
 */
export class LeadContactoBLL {
  static async registrarSolicitud(leadDto) {
    let cotizacionAsociada = null;
    if (leadDto.idCotizacion) {
      cotizacionAsociada = await SimulacionDAL.obtenerPorId(leadDto.idCotizacion);
    }

    const resultado = await OperadorDAL.guardarSolicitudContacto({
      operadorId: leadDto.operadorId,
      idCotizacion: leadDto.idCotizacion,
      nombreApellido: leadDto.nombreApellido,
      empresa: leadDto.empresa,
      email: leadDto.email,
      telefono: leadDto.telefono,
      comentarios: leadDto.comentarios
    });

    return {
      exito: true,
      mensaje: 'Solicitud enviada exitosamente al operador logístico.',
      leadId: resultado.leadId,
      idCotizacion: leadDto.idCotizacion,
      tieneCotizacionVerificada: Boolean(cotizacionAsociada)
    };
  }
}
