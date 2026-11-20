import 'server-only';
import {
  guardarSimulacionEnExcel,
  buscarSimulacionEnExcelPorId
} from './excelClient';

const memoriaSimulaciones = new Map();

/**
 * Capa DAL: Persistencia Inmutable de Cotizaciones y Comprobantes en Excel (PN1/PN2)
 */
export class SimulacionDAL {
  /**
   * Guarda una instantánea (snapshot) inmutable de la cotización calculada en memoria y en Excel
   */
  static async guardar(resultadoSimulacion) {
    const id = resultadoSimulacion.idCotizacion;

    // 1. Guardar en caché de memoria para acceso inmediato
    memoriaSimulaciones.set(id, resultadoSimulacion);
    if (memoriaSimulaciones.size > 500) {
      const firstKey = memoriaSimulaciones.keys().next().value;
      memoriaSimulaciones.delete(firstKey);
    }

    // 2. Persistir en la planilla Excel DAL/database/simulaciones.csv vinculada a ImportaPlus_ExcelDB.xlsx
    guardarSimulacionEnExcel(resultadoSimulacion);

    return id;
  }

  /**
   * Recupera una cotización por su Hash ID único desde memoria o desde la planilla Excel (PN4)
   */
  static async obtenerPorId(idCotizacion) {
    if (memoriaSimulaciones.has(idCotizacion)) {
      return memoriaSimulaciones.get(idCotizacion);
    }

    const registroExcel = buscarSimulacionEnExcelPorId(idCotizacion);
    if (registroExcel) {
      return registroExcel;
    }

    return null;
  }
}
