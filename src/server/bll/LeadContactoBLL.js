import 'server-only';
import { OperadorDAL } from '../dal/OperadorDAL';
import { ForwarderDAL } from '../dal/ForwarderDAL';
import { SimulacionDAL } from '../dal/SimulacionDAL';
import { DomainValidationError } from './CalculadoraArancelariaBLL';

/**
 * Capa BLL: Gestión de Solicitudes de Cotización y Contacto con Forwarders (PN4)
 */
export class LeadContactoBLL {
  static async registrarSolicitud(leadDto) {
    const [operadoresActivos, forwardersActivos] = await Promise.all([
      OperadorDAL.obtenerActivos(),
      ForwarderDAL.obtenerGrillaCuatro()
    ]);

    const targetId = String(leadDto.operadorId || '').trim().toLowerCase();
    const operadorExiste =
      operadoresActivos.some((op) => String(op.id).toLowerCase() === targetId) ||
      forwardersActivos.some((fw) => String(fw.id).toLowerCase() === targetId);

    if (!operadorExiste) {
      throw new DomainValidationError('El operador o forwarder seleccionado no existe o no se encuentra activo.', 404);
    }

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
