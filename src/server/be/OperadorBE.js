import 'server-only';

/**
 * Capa BE (Business Entities) - Servidor
 * Entidad de Dominio: OperadorBE
 * Modela un operador logístico, despachante de aduana matriculado ante DGA o agente de carga forwarder.
 */
export class OperadorBE {
  constructor({
    id = '',
    razon_social = '',
    tipo_operador = 'DESPACHANTE', // 'DESPACHANTE' | 'FORWARDER' | 'INTEGRAL'
    matricula_dga = '',
    email_notificacion = '',
    telefono = '',
    rubros_especialidad = [],
    aduanas_operativas = [],
    modalidades = [],
    experiencia_anios = 5,
    rating = 5.0,
    activo = true
  } = {}) {
    this.id = id;
    this.razon_social = razon_social;
    this.tipo_operador = tipo_operador;
    this.matricula_dga = matricula_dga;
    this.email_notificacion = email_notificacion;
    this.telefono = telefono;
    this.rubros_especialidad = Array.isArray(rubros_especialidad) ? rubros_especialidad : [];
    this.aduanas_operativas = Array.isArray(aduanas_operativas) ? aduanas_operativas : [];
    this.modalidades = Array.isArray(modalidades) ? modalidades : [];
    this.experiencia_anios = Number(experiencia_anios) || 5;
    this.rating = Number(rating) || 5.0;
    this.activo = Boolean(activo);
  }
}
