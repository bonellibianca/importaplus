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

export class DomainValidationError extends Error {
  constructor(message, statusCode = 422) {
    super(message);
    this.name = 'DomainValidationError';
    this.statusCode = statusCode;
  }
}

/**
 * Capa BLL: Orquestador Central del Motor de Liquidación Aduanera (Backend-Only)
 */
export class CalculadoraArancelariaBLL {
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

    // En producción la cotización oficial siempre es gobernada por el servidor para evitar comprobantes adulterados
    const tipoCambioBNA =
      process.env.NODE_ENV !== 'production' && typeof dto.tipoCambioBNA === 'number' && dto.tipoCambioBNA >= 100
        ? dto.tipoCambioBNA
        : await obtenerCotizacionDolarOficial();

    const [ncm, pais] = await Promise.all([
      NcmDAL.obtenerPorCodigo(codigoNcm),
      PaisDAL.obtenerPorCodigoIso(codigoPais)
    ]);

    if (!ncm) {
      throw new DomainValidationError(`La posición arancelaria NCM '${codigoNcm}' no fue encontrada en el nomenclador oficial.`, 404);
    }

    if (!pais) {
      throw new DomainValidationError(`El país con código ISO '${codigoPais}' no es válido.`, 404);
    }

    let calculador;
    if (pais.es_mercosur) {
      calculador = new LiquidacionIntrazonaBLL();
    } else if (usaExcepcion && ncm.tieneExcepcion) {
      calculador = new LiquidacionExcepcionArancelariaBLL();
    } else {
      calculador = new LiquidacionExtrazonaGeneralBLL();
    }

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

    resultado.fichaProducto = {
      codigoNcm: ncm.codigo_ncm,
      descripcion: ncm.descripcion,
      tipoBien: ncm.tipo_bien,
      paisOrigen: pais.nombre,
      codigoPais: pais.codigo_iso,
      regimenAplicado: resultado.regimenAplicado,
      condicionImportador
    };

    await SimulacionDAL.guardar(resultado);

    const puertosSugeridos = await PuertoDAL.obtenerSugeridosPorPais(pais.codigo_iso);
    resultado.puertosRecomendados = puertosSugeridos;

    return resultado;
  }
}
