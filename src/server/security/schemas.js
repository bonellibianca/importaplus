import 'server-only';
import { z } from 'zod';

/**
 * Esquema de Validación para Búsqueda Incremental NCM (RF01.1)
 * El usuario busca por nombre coloquial o técnico (< 250ms).
 */
export const BusquedaNcmSchema = z.object({
  q: z.string()
    .trim()
    .min(2, 'Ingrese al menos 2 caracteres para buscar')
    .max(80, 'La consulta de búsqueda es demasiado extensa')
    .regex(/^[a-zA-ZáéíóúÁÉÍÓÚñÑ0-9\s.,\-_/()]+$/, 'Caracteres no válidos en la búsqueda')
});

/**
 * Esquema de Validación para Simulación Aduanera (RF02 / PN1)
 */
export const SimularOperacionSchema = z.object({
  codigoNcm: z.string()
    .trim()
    .regex(/^\d{4}\.\d{2}\.\d{2}$/, 'Formato de NCM inválido (debe ser ej: 8471.30.12 o 6109.10.00)'),
  codigoPais: z.string()
    .trim()
    .length(3, 'El código de país debe ser ISO-3 de 3 letras (ej: CHN, USA, BRA)')
    .toUpperCase(),
  cantidad: z.coerce.number()
    .int('La cantidad debe ser un número entero')
    .min(1, 'La cantidad mínima es 1 unidad')
    .max(10000000, 'Cantidad fuera de rango permitido'),
  valorFobUnitario: z.coerce.number()
    .positive('El valor FOB unitario debe ser mayor a cero')
    .max(100000000, 'Valor unitario excede el máximo permitido'),
  flete: z.coerce.number()
    .nonnegative('El flete no puede ser negativo')
    .max(5000000, 'Flete excede el límite razonable')
    .nullable()
    .optional(),
  seguro: z.coerce.number()
    .nonnegative('El seguro no puede ser negativo')
    .max(1000000, 'Seguro excede el límite razonable')
    .nullable()
    .optional(),
  condicionImportador: z.enum(['REVENTA_RI', 'USO_PROPIO_CONSUMO'])
    .default('REVENTA_RI'),
  usaExcepcion: z.boolean().default(true),
  tipoCambioBNA: z.coerce.number().positive().default(1420.50),
  turnstileToken: z.string().optional()
});

/**
 * Esquema de Validación para Solicitud de Contacto a Forwarders/Despachantes (PN4)
 */
export const SolicitudLeadSchema = z.object({
  operadorId: z.string()
    .uuid('ID de operador inválido'),
  idCotizacion: z.string()
    .regex(/^IMP-\d{4}-[A-F0-9]{6,8}$/, 'Formato de ID de cotización inválido (ej: IMP-2026-8F9A2B)')
    .optional()
    .nullable(),
  nombreApellido: z.string()
    .trim()
    .min(3, 'El nombre debe tener al menos 3 caracteres')
    .max(120, 'Nombre demasiado extenso'),
  empresa: z.string()
    .trim()
    .max(120, 'Nombre de empresa demasiado extenso')
    .optional()
    .nullable(),
  email: z.string()
    .trim()
    .email('Correo electrónico no válido'),
  telefono: z.string()
    .trim()
    .min(6, 'Teléfono debe tener al menos 6 dígitos')
    .max(35, 'Teléfono demasiado extenso'),
  comentarios: z.string()
    .trim()
    .max(1000, 'Los comentarios no pueden superar 1000 caracteres')
    .optional()
    .nullable(),
  turnstileToken: z.string().optional()
});
