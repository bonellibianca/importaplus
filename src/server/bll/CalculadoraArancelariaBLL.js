import 'server-only';
import { NcmDAL } from '../dal/NcmDAL';
import { PaisDAL } from '../dal/PaisDAL';
import { PuertoDAL } from '../dal/PuertoDAL';
import { SimulacionDAL } from '../dal/SimulacionDAL';
import { obtenerCotizacionDolarOficial } from '../dal/excelClient';
import {
  LiquidacionIntrazonaBLL,
  LiquidacionExtrazonaGeneralBLL,
  LiquidacionExcepcionArancelariaBLL
} from './templates/CalculadorAduaneroTemplate';

/**
 * Capa BLL: Orquestador Central del Motor de Liquidación Aduanera
 * Protege la propiedad intelectual de las fórmulas de Bianca Bonelli (Backend-Only)
 */
export class CalculadoraArancelariaBLL {
  /**
   * Procesa la simulación comercial y aduanera completa (RF03 / PN1)
   */
  static async procesarSimulacion(dto) {
    const {
      codigoNcm,
      codigoPais,
      cantidad,
      valorFobUnitario,
      flete = null,
      seguro = null,
      condicionImportador = 'REVENTA_RI',
      usaExcepcion = true
    } = dto;

    const tipoCambioBNA = dto.tipoCambioBNA && dto.tipoCambioBNA !== 1420.50
      ? dto.tipoCambioBNA
      : await obtenerCotizacionDolarOficial();

    // 1. Obtener datos de dominio en paralelo desde la capa DAL
    const [ncm, pais] = await Promise.all([
      NcmDAL.obtenerPorCodigo(codigoNcm),
      PaisDAL.obtenerPorCodigoIso(codigoPais)
    ]);

    if (!ncm) {
      throw new Error(`La posición arancelaria NCM '${codigoNcm}' no fue encontrada.`);
    }

    if (!pais) {
      throw new Error(`El país con código ISO '${codigoPais}' no es válido.`);
    }

    // 2. Selección de Estrategia Concreta del Patrón Template Method
    let calculador;
    if (pais.es_mercosur) {
      calculador = new LiquidacionIntrazonaBLL();
    } else if (usaExcepcion && ncm.tieneExcepcion) {
      calculador = new LiquidacionExcepcionArancelariaBLL();
    } else {
      calculador = new LiquidacionExtrazonaGeneralBLL();
    }

    // 3. Ejecución del algoritmo reglamentario en cascada
    const resultado = calculador.liquidarOperacion({
      ncm,
      pais,
      cantidad,
      valorFobUnitario,
      fleteOpcional: flete,
      seguroOpcional: seguro,
      condicionImportador,
      tipoCambioBNA
    });

    // Enriquecer la ficha del producto
    resultado.fichaProducto = {
      codigoNcm: ncm.codigo_ncm,
      descripcion: ncm.descripcion,
      tipoBien: ncm.tipo_bien,
      paisOrigen: pais.nombre,
      codigoPais: pais.codigo_iso,
      regimenAplicado: resultado.regimenAplicado,
      condicionImportador
    };

    // 4. Persistir comprobante inmutable con Hash ID en DAL (PN1 / PN2)
    await SimulacionDAL.guardar(resultado);

    // 5. Obtener puertos y aduanas recomendados según el país de procedencia (PN3)
    const puertosSugeridos = await PuertoDAL.obtenerSugeridosPorPais(pais.codigo_iso);
    resultado.puertosRecomendados = puertosSugeridos;

    return resultado;
  }
}
