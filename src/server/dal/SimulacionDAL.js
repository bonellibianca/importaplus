import 'server-only';
import {
  guardarSimulacionEnExcel,
  buscarSimulacionEnExcelPorId
} from './excelClient';
import { supabaseInsert, supabaseRpc, isSupabaseConfigured } from './supabaseClient';

const memoriaSimulaciones = new Map();

/**
 * Capa DAL: Persistencia Inmutable de Cotizaciones en Supabase (service_role) + Caché RAM
 */
export class SimulacionDAL {
  static async guardar(resultadoSimulacion) {
    const id = resultadoSimulacion.idCotizacion;

    memoriaSimulaciones.set(id, resultadoSimulacion);
    if (memoriaSimulaciones.size > 500) {
      const firstKey = memoriaSimulaciones.keys().next().value;
      memoriaSimulaciones.delete(firstKey);
    }

    if (isSupabaseConfigured()) {
      const insertRes = await supabaseInsert('simulaciones', {
        id_cotizacion: id,
        codigo_ncm: resultadoSimulacion.fichaProducto?.codigoNcm || null,
        pais_origen: resultadoSimulacion.fichaProducto?.codigoPais || null,
        cantidad: resultadoSimulacion.parametrosComerciales?.cantidad || 1,
        valor_fob_unitario: resultadoSimulacion.parametrosComerciales?.valorFobUnitario || 0,
        valor_fob_total: resultadoSimulacion.parametrosComerciales?.valorFobTotal || 0,
        flete_usd: resultadoSimulacion.parametrosComerciales?.flete || 0,
        seguro_usd: resultadoSimulacion.parametrosComerciales?.seguro || 0,
        valor_cif_usd: resultadoSimulacion.parametrosComerciales?.valorCIF || 0,
        desglose_impuestos: (resultadoSimulacion.desgloseTributario || []).map((g) => ({
          id: g.id,
          concepto: g.concepto,
          alicuota: g.alicuota,
          montoUSD: g.montoUSD
        })),
        total_gravamenes_usd: resultadoSimulacion.resumenEconomico?.totalGravamenesUSD || 0,
        costo_total_nacionalizado_usd: resultadoSimulacion.resumenEconomico?.costoTotalNacionalizadoUSD || 0,
        costo_unitario_final_usd: resultadoSimulacion.resumenEconomico?.costoUnitarioFinalUSD || 0,
        cotizacion_bna_ars: resultadoSimulacion.resumenEconomico?.tipoCambioBNA || 1420.5
      });

      if (insertRes.ok) {
        return id;
      }
    }

    // Respaldo en entorno local sin credenciales Supabase configuradas
    if (process.env.NODE_ENV !== 'production') {
      guardarSimulacionEnExcel(resultadoSimulacion);
    }

    return id;
  }

  static async obtenerPorId(idCotizacion) {
    if (memoriaSimulaciones.has(idCotizacion)) {
      return memoriaSimulaciones.get(idCotizacion);
    }

    if (isSupabaseConfigured()) {
      const rows = await supabaseRpc('obtener_simulacion_por_id', { p_id_cotizacion: idCotizacion });
      if (Array.isArray(rows) && rows.length > 0) {
        return rows[0];
      }
    }

    return buscarSimulacionEnExcelPorId(idCotizacion) || null;
  }
}
