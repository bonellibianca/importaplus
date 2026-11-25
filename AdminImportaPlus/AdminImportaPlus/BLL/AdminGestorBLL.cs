using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AdminImportaPlus.BE;
using AdminImportaPlus.DAL;

namespace AdminImportaPlus.BLL;

/// <summary>
/// Capa BLL (.NET 9): Orquestador de Login BCrypt con anti-fuerza bruta, Restablecimiento Seguro de Contraseña,
/// Control de Acceso Basado en Roles (Patrón Composite), ABM Integral, Dígito Verificador y RestoreBackup firmado.
/// </summary>
public sealed class AdminGestorBLL
{
    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    private const int MaxIntentosFallidos = 5;
    private static readonly TimeSpan DuracionBloqueoLogin = TimeSpan.FromSeconds(60);
    private int _intentosFallidosConsecutivos;
    private DateTimeOffset? _bloqueadoHastaUtc;

    public RepositorioDatosDAL Dal { get; }
    public RolesCompositeBLL Roles { get; }
    public AdminUsuarioBE? AdminActual { get; private set; }

    public AdminGestorBLL(RepositorioDatosDAL? dal = null)
    {
        Dal = dal ?? new RepositorioDatosDAL();
        Roles = new RolesCompositeBLL(Dal.DataFolderPath);
    }

    public bool UsuarioActualTienePermiso(string codigoPermiso)
    {
        string user = AdminActual?.Username ?? "Admin";
        return Roles.TienePermiso(user, codigoPermiso);
    }

    private (bool Permitido, string Mensaje) VerificarPermisoOperacion(string codigoPermiso)
    {
        if (AdminActual == null)
            return (false, "Sesión no autenticada. Iniciá sesión nuevamente.");

        if (!Roles.TienePermiso(AdminActual.Username, codigoPermiso))
            return (false, $"Acceso denegado: tu perfil no posee el permiso requerido ('{codigoPermiso}').");

        return (true, string.Empty);
    }

    // --- 1. LOGIN CON BCRYPT + ANTI-FUERZA BRUTA + ANTI-ENUMERACIÓN + DVH/DVV ---

    public async Task<(bool Exito, string Mensaje, ResultadoVerificacionDV? VerificacionDV)> IniciarSesionAsync(
        string username,
        string passwordPlano)
    {
        if (_bloqueadoHastaUtc.HasValue && DateTimeOffset.UtcNow < _bloqueadoHastaUtc.Value)
        {
            int segRestantes = Math.Max(1, (int)Math.Ceiling((_bloqueadoHastaUtc.Value - DateTimeOffset.UtcNow).TotalSeconds));
            return (false, $"Demasiados intentos fallidos. Aguardá {segRestantes} segundos antes de volver a intentar.", null);
        }

        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrEmpty(passwordPlano))
            return (false, "Ingresá el usuario y la contraseña.", null);

        var db = await Dal.ObtenerBaseDatosActualAsync();
        var admin = db.AdminUsuarios.FirstOrDefault(u =>
            string.Equals(u.Username.Trim(), username.Trim(), StringComparison.OrdinalIgnoreCase));

        // Mitigación de enumeración de usuarios por tiempos de respuesta (ejecutar BCrypt siempre)
        string hashObjetivo = (admin != null && admin.Activo)
            ? admin.PasswordHash
            : SeguridadYDigitoVerificadorBLL.DummyBcryptHash;

        bool passwordValida = SeguridadYDigitoVerificadorBLL.VerificarPasswordBcrypt(passwordPlano, hashObjetivo);

        if (admin == null || !admin.Activo || !passwordValida)
        {
            _intentosFallidosConsecutivos++;
            if (_intentosFallidosConsecutivos >= MaxIntentosFallidos)
            {
                _bloqueadoHastaUtc = DateTimeOffset.UtcNow.Add(DuracionBloqueoLogin);
                _intentosFallidosConsecutivos = 0;
                return (false, "Cuenta bloqueada temporalmente por 60 segundos tras múltiples intentos fallidos.", null);
            }
            return (false, "Credenciales inválidas o cuenta inactiva.", null);
        }

        _intentosFallidosConsecutivos = 0;
        _bloqueadoHastaUtc = null;

        var resultadoDV = SeguridadYDigitoVerificadorBLL.VerificarBaseCompleta(db);
        admin.UltimoAcceso = DateTimeOffset.UtcNow;
        AdminActual = admin;

        // Registrar marca de último acceso sin alterar DVH (ultimo_acceso no integra la fórmula DVH)
        if (!resultadoDV.SaltoDetectado)
        {
            await Dal.RegistrarUltimoAccesoLocalAsync(admin.Username, admin.UltimoAcceso.Value);
        }

        return (true, "Autenticación exitosa.", resultadoDV);
    }

    public async Task<(bool Ok, string Mensaje)> RestablecerPasswordAdminAsync(
        string username,
        string passwordActual,
        string nuevaPassword,
        string confirmarPassword)
    {
        if (string.IsNullOrWhiteSpace(username))
            return (false, "El usuario es obligatorio.");

        if (string.IsNullOrEmpty(passwordActual))
            return (false, "Debés ingresar la contraseña actual para confirmar tu identidad.");

        if (!SeguridadYDigitoVerificadorBLL.ValidarFortalezaPassword(nuevaPassword, out string motivoDebil))
            return (false, motivoDebil);

        if (!string.Equals(nuevaPassword, confirmarPassword, StringComparison.Ordinal))
            return (false, "Las nuevas contraseñas ingresadas no coinciden.");

        if (string.Equals(passwordActual, nuevaPassword, StringComparison.Ordinal))
            return (false, "La nueva contraseña debe ser diferente a la actual.");

        var db = await Dal.ObtenerBaseDatosActualAsync();
        var admin = db.AdminUsuarios.FirstOrDefault(u =>
            string.Equals(u.Username.Trim(), username.Trim(), StringComparison.OrdinalIgnoreCase));

        if (admin == null || !SeguridadYDigitoVerificadorBLL.VerificarPasswordBcrypt(passwordActual, admin.PasswordHash))
            return (false, "La contraseña actual es incorrecta.");

        admin.PasswordHash = SeguridadYDigitoVerificadorBLL.HashearPasswordBcrypt(nuevaPassword);
        admin.PasswordUpdatedAt = DateTimeOffset.UtcNow;

        admin.Dvh = SeguridadYDigitoVerificadorBLL.CalcularDvhAdmin(admin);
        ActualizarDvvDeTablaAdmin(db, admin.Username);

        var guardado = await Dal.GuardarBaseDatosCompletaAsync(db);
        return (guardado.Ok, $"Contraseña actualizada exitosamente. {guardado.Detalle}");
    }

    private static void ActualizarDvvDeTablaAdmin(BaseDatosCompletaBE db, string usuario)
    {
        db.DigitosVerificadores.RemoveAll(d => string.Equals(d.TablaNombre, "admin_usuarios", StringComparison.OrdinalIgnoreCase));
        db.DigitosVerificadores.Add(new DigitoVerificadorBE
        {
            TablaNombre = "admin_usuarios",
            Dvv = SeguridadYDigitoVerificadorBLL.CalcularDvvDesdeDvhs(db.AdminUsuarios.Select(x => x.Dvh)),
            CantidadRegistros = db.AdminUsuarios.Count,
            UpdatedAt = DateTimeOffset.UtcNow,
            UpdatedBy = usuario,
            SnapshotControl = db.AdminUsuarios.OrderBy(x => x.Username).Select(u => new SnapshotFilaBE
            {
                Pk = u.Username,
                Dvh = u.Dvh,
                Campos = new System.Collections.Generic.Dictionary<string, object?>
                {
                    ["username"] = u.Username,
                    ["password_hash"] = u.PasswordHash.Length > 15 ? u.PasswordHash[..15] + "..." : u.PasswordHash,
                    ["activo"] = u.Activo.ToString().ToLowerInvariant()
                }
            }).ToList()
        });
    }

    // --- 2. DÍGITO VERIFICADOR (VERIFICAR Y RECALCULAR) ---

    public async Task<ResultadoVerificacionDV> VerificarIntegridadActualAsync()
    {
        var db = await Dal.ObtenerBaseDatosActualAsync();
        return SeguridadYDigitoVerificadorBLL.VerificarBaseCompleta(db);
    }

    public async Task<(bool Ok, string Mensaje)> RecalcularDigitosVerificadoresAsync()
    {
        var perm = VerificarPermisoOperacion("DV_RECALCULAR");
        if (!perm.Permitido) return (false, perm.Mensaje);

        string actor = AdminActual?.Username ?? "Admin";
        if (Dal.Config.EstaConfiguradoEnNube)
        {
            var rpc = await Dal.RecalcularDigitosEnSupabaseRpcAsync(actor);
            if (rpc.Ok)
            {
                return (true, $"Dígitos Verificadores (DVH / DVV) recalculados en Supabase sobre todas las tablas. {rpc.Detalle}");
            }
        }

        var db = await Dal.ObtenerBaseDatosActualAsync();
        SeguridadYDigitoVerificadorBLL.RecalcularTodoEnMemoria(db, actor);
        var res = await Dal.GuardarBaseDatosCompletaAsync(db);
        return (res.Ok, $"Dígitos Verificadores Horizontales (DVH) y Verticales (DVV) recalculados correctamente. {res.Detalle}");
    }

    public static int ObtenerSiguienteOrdenDisponible(BaseDatosCompletaBE db)
    {
        if (db.Forwarders.Count == 0) return 1;
        return db.Forwarders.Max(f => f.Orden) + 1;
    }

    // --- 3. ABM DE FORWARDERS (ORDEN ÚNICO OBLIGATORIO + VALIDACIÓN ANTI-SSRF/XSS DE URL) ---

    public async Task<(bool Ok, string Mensaje)> GuardarForwarderAsync(ForwarderBE item, string? rutaImagenLocal = null)
    {
        var db = await Dal.ObtenerBaseDatosActualAsync();
        bool esExistente = db.Forwarders.Any(f => f.Id == item.Id);
        var perm = VerificarPermisoOperacion(esExistente ? "FWD_MODIFICAR" : "FWD_ALTA");
        if (!perm.Permitido) return (false, perm.Mensaje);

        item.Nombre = (item.Nombre ?? string.Empty).Trim();
        item.Whatsapp = (item.Whatsapp ?? string.Empty).Trim();
        item.Email = (item.Email ?? string.Empty).Trim();
        item.ImagenUrl = (item.ImagenUrl ?? string.Empty).Trim();

        if (string.IsNullOrWhiteSpace(item.Nombre))
            return (false, "El nombre del Forwarder es obligatorio.");

        if (item.Nombre.Length > 120)
            return (false, "El nombre del Forwarder no puede superar los 120 caracteres.");

        if (item.Orden < 1 || item.Orden > 999)
            return (false, "El número de orden debe estar comprendido entre 1 y 999.");

        if (string.IsNullOrWhiteSpace(item.Whatsapp) || Regex.Replace(item.Whatsapp, "[^0-9]", "").Length < 6)
            return (false, "Ingresá un número de WhatsApp válido con código de país (ej: +5491143218800).");

        if (!Regex.IsMatch(item.Email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            return (false, "Ingresá una dirección de Mail válida.");

        var conflictoOrden = db.Forwarders.FirstOrDefault(f => f.Id != item.Id && f.Orden == item.Orden);
        if (conflictoOrden != null)
        {
            int sugerido = ObtenerSiguienteOrdenDisponible(db);
            return (false,
                $"Orden duplicado: el orden {item.Orden} ya está ocupado por '{conflictoOrden.Nombre}'. Elegí otro orden (sugerido: {sugerido}) o reasigná primero el existente.");
        }

        if (!string.IsNullOrWhiteSpace(rutaImagenLocal) && File.Exists(rutaImagenLocal))
        {
            item.ImagenUrl = await Dal.ProcesarYSubirImagenForwarderAsync(rutaImagenLocal, item.Id);
        }

        if (string.IsNullOrWhiteSpace(item.ImagenUrl))
            return (false, "Seleccioná una imagen local o ingresá una URL HTTPS segura para el Forwarder.");

        if (!SeguridadYDigitoVerificadorBLL.EsUrlImagenSegura(item.ImagenUrl))
            return (false, "La URL de imagen debe comenzar con https:// (dominio público válido) o ser una imagen subida desde el equipo.");

        int idx = db.Forwarders.FindIndex(f => f.Id == item.Id);
        if (idx >= 0) db.Forwarders[idx] = item;
        else db.Forwarders.Add(item);

        SeguridadYDigitoVerificadorBLL.RecalcularTodoEnMemoria(db, AdminActual?.Username ?? "Admin");
        var res = await Dal.GuardarBaseDatosCompletaAsync(db);
        return (res.Ok, $"Forwarder '{item.Nombre}' (Orden #{item.Orden}) guardado exitosamente. {res.Detalle}");
    }

    public async Task<(bool Ok, string Mensaje)> EliminarForwarderAsync(Guid id)
    {
        var perm = VerificarPermisoOperacion("FWD_BAJA");
        if (!perm.Permitido) return (false, perm.Mensaje);

        var db = await Dal.ObtenerBaseDatosActualAsync();
        var existente = db.Forwarders.FirstOrDefault(f => f.Id == id);
        if (existente == null)
            return (false, "No se encontró el Forwarder seleccionado.");

        db.Forwarders.RemoveAll(f => f.Id == id);
        await Dal.EliminarRegistroSupabaseAsync("forwarders", $"id=eq.{id}");

        SeguridadYDigitoVerificadorBLL.RecalcularTodoEnMemoria(db, AdminActual?.Username ?? "Admin");
        var res = await Dal.GuardarBaseDatosCompletaAsync(db);
        return (res.Ok, $"Forwarder '{existente.Nombre}' eliminado. {res.Detalle}");
    }

    // --- 4. ABM PUERTOS Y ADUANAS, PAÍSES Y CATÁLOGO NCM ---

    public async Task<(bool Ok, string Mensaje)> GuardarPuertoAsync(PuertoAduanaBE puerto)
    {
        var perm = VerificarPermisoOperacion("PTO_ABM");
        if (!perm.Permitido) return (false, perm.Mensaje);

        puerto.Nombre = (puerto.Nombre ?? string.Empty).Trim();
        puerto.Provincia = (puerto.Provincia ?? string.Empty).Trim();
        puerto.CodigoSim = (puerto.CodigoSim ?? string.Empty).Trim();
        puerto.Operador = (puerto.Operador ?? string.Empty).Trim();

        if (string.IsNullOrWhiteSpace(puerto.Nombre) || string.IsNullOrWhiteSpace(puerto.Provincia))
            return (false, "El nombre del puerto/aduana y la provincia son obligatorios.");

        if (puerto.Latitud.HasValue && (puerto.Latitud.Value < -90m || puerto.Latitud.Value > 90m))
            return (false, "La latitud debe encontrarse entre -90 y 90 grados.");

        if (puerto.Longitud.HasValue && (puerto.Longitud.Value < -180m || puerto.Longitud.Value > 180m))
            return (false, "La longitud debe encontrarse entre -180 y 180 grados.");

        var db = await Dal.ObtenerBaseDatosActualAsync();
        int idx = db.PuertosAduanas.FindIndex(p => p.Id == puerto.Id);
        if (idx >= 0) db.PuertosAduanas[idx] = puerto;
        else db.PuertosAduanas.Add(puerto);

        SeguridadYDigitoVerificadorBLL.RecalcularTodoEnMemoria(db, AdminActual?.Username ?? "Admin");
        var res = await Dal.GuardarBaseDatosCompletaAsync(db);
        return (res.Ok, $"Puerto/Aduana '{puerto.Nombre}' actualizado. {res.Detalle}");
    }

    public async Task<(bool Ok, string Mensaje)> EliminarPuertoAsync(Guid id)
    {
        var perm = VerificarPermisoOperacion("PTO_ABM");
        if (!perm.Permitido) return (false, perm.Mensaje);

        var db = await Dal.ObtenerBaseDatosActualAsync();
        db.PuertosAduanas.RemoveAll(p => p.Id == id);
        await Dal.EliminarRegistroSupabaseAsync("puertos_aduanas", $"id=eq.{id}");
        SeguridadYDigitoVerificadorBLL.RecalcularTodoEnMemoria(db, AdminActual?.Username ?? "Admin");
        var res = await Dal.GuardarBaseDatosCompletaAsync(db);
        return (res.Ok, $"Puerto eliminado. {res.Detalle}");
    }

    public async Task<(bool Ok, string Mensaje)> GuardarNcmAsync(NcmPosicionBE ncm)
    {
        var perm = VerificarPermisoOperacion("PTO_ABM");
        if (!perm.Permitido) return (false, perm.Mensaje);

        ncm.CodigoNcm = (ncm.CodigoNcm ?? string.Empty).Trim();
        ncm.Descripcion = (ncm.Descripcion ?? string.Empty).Trim();
        ncm.TipoBien = (ncm.TipoBien ?? "GENERAL").Trim().ToUpperInvariant();

        if (!Regex.IsMatch(ncm.CodigoNcm, @"^\d{4}\.\d{2}\.\d{2}"))
            return (false, "El código NCM debe respetar el formato oficial (ej: 8471.30.12).");

        if (string.IsNullOrWhiteSpace(ncm.Descripcion))
            return (false, "La descripción del producto NCM es obligatoria.");

        var db = await Dal.ObtenerBaseDatosActualAsync();
        int idx = db.NcmCatalogo.FindIndex(n => string.Equals(n.CodigoNcm, ncm.CodigoNcm, StringComparison.OrdinalIgnoreCase));
        if (idx >= 0) db.NcmCatalogo[idx] = ncm;
        else db.NcmCatalogo.Add(ncm);

        SeguridadYDigitoVerificadorBLL.RecalcularTodoEnMemoria(db, AdminActual?.Username ?? "Admin");
        var res = await Dal.GuardarBaseDatosCompletaAsync(db);
        return (res.Ok, $"Posición NCM '{ncm.CodigoNcm}' guardada. {res.Detalle}");
    }

    public async Task<(bool Ok, string Mensaje)> EliminarNcmAsync(string codigoNcm)
    {
        var perm = VerificarPermisoOperacion("PTO_ABM");
        if (!perm.Permitido) return (false, perm.Mensaje);

        var db = await Dal.ObtenerBaseDatosActualAsync();
        db.NcmCatalogo.RemoveAll(n => string.Equals(n.CodigoNcm, codigoNcm, StringComparison.OrdinalIgnoreCase));
        await Dal.EliminarRegistroSupabaseAsync("ncm_catalogo", $"codigo_ncm=eq.{Uri.EscapeDataString(codigoNcm)}");
        SeguridadYDigitoVerificadorBLL.RecalcularTodoEnMemoria(db, AdminActual?.Username ?? "Admin");
        var res = await Dal.GuardarBaseDatosCompletaAsync(db);
        return (res.Ok, $"Posición NCM '{codigoNcm}' eliminada. {res.Detalle}");
    }

    public async Task<(bool Ok, string Mensaje)> GuardarPaisAsync(PaisBE pais)
    {
        var perm = VerificarPermisoOperacion("PTO_ABM");
        if (!perm.Permitido) return (false, perm.Mensaje);

        pais.CodigoIso = (pais.CodigoIso ?? string.Empty).Trim().ToUpperInvariant();
        pais.Nombre = (pais.Nombre ?? string.Empty).Trim();
        if (pais.CodigoIso.Length != 3 || string.IsNullOrWhiteSpace(pais.Nombre))
            return (false, "Ingresá un código ISO-3 de 3 letras (ej: CHN, BRA) y el nombre del país.");

        var db = await Dal.ObtenerBaseDatosActualAsync();
        int idx = db.Paises.FindIndex(p => string.Equals(p.CodigoIso, pais.CodigoIso, StringComparison.OrdinalIgnoreCase));
        if (idx >= 0) db.Paises[idx] = pais;
        else db.Paises.Add(pais);

        SeguridadYDigitoVerificadorBLL.RecalcularTodoEnMemoria(db, AdminActual?.Username ?? "Admin");
        var res = await Dal.GuardarBaseDatosCompletaAsync(db);
        return (res.Ok, $"País '{pais.Nombre}' ({pais.CodigoIso}) guardado. {res.Detalle}");
    }

    // --- 5. SISTEMA DE RESTORE / BACKUP CON VERIFICACIÓN ESTRICTA DE FIRMA SHA-256 ---

    public async Task<(bool Ok, string Mensaje, string RutaArchivo)> CrearBackupBaseActualAsync(string? rutaPersonalizada = null)
    {
        var perm = VerificarPermisoOperacion("BACKUP_CREAR");
        if (!perm.Permitido) return (false, perm.Mensaje, string.Empty);

        var db = await Dal.ObtenerBaseDatosCompletaParaBackupAsync();
        string ruta = rutaPersonalizada ?? Path.Combine(
            Dal.BackupsFolderPath,
            $"ImportaPlus_Backup_{DateTime.Now:yyyyMMdd_HHmmss}.ipbak.json");

        string datosSerializados = JsonSerializer.Serialize(db, JsonOpts);
        string firmaSha256 = SeguridadYDigitoVerificadorBLL.Sha256Hex(datosSerializados);

        var paquete = new BackupPackageBE
        {
            FechaBackup = DateTimeOffset.Now,
            GeneradoPor = AdminActual?.Username ?? "Admin",
            FirmaIntegridadSha256 = firmaSha256,
            Datos = db
        };

        Directory.CreateDirectory(Path.GetDirectoryName(ruta)!);
        await File.WriteAllTextAsync(ruta, JsonSerializer.Serialize(paquete, JsonOpts), Encoding.UTF8);

        return (true, $"Backup generado exitosamente con firma SHA-256 ({firmaSha256[..12]}...).", ruta);
    }

    public async Task<(bool Valido, bool FirmaIntegra, BackupPackageBE? Paquete, string Resumen)> InspeccionarArchivoBackupAsync(string rutaArchivo)
    {
        if (!File.Exists(rutaArchivo))
            return (false, false, null, "El archivo de backup seleccionado no existe.");

        try
        {
            string contenido = await File.ReadAllTextAsync(rutaArchivo, Encoding.UTF8);
            var paquete = JsonSerializer.Deserialize<BackupPackageBE>(contenido, JsonOpts);
            if (paquete?.Datos == null)
                return (false, false, null, "El archivo no posee el formato válido de Backup IMPORTA+.");

            string jsonDatos = JsonSerializer.Serialize(paquete.Datos, JsonOpts);
            string firmaCalculada = SeguridadYDigitoVerificadorBLL.Sha256Hex(jsonDatos);
            bool firmaCoincide = string.Equals(paquete.FirmaIntegridadSha256, firmaCalculada, StringComparison.OrdinalIgnoreCase);

            string resumen =
                $"Fecha del Backup: {paquete.FechaBackup.LocalDateTime:dd/MM/yyyy HH:mm:ss} | Autor: {paquete.GeneradoPor}" + Environment.NewLine +
                $"Tablas incluidas: {paquete.Datos.Forwarders.Count} Forwarders, {paquete.Datos.PuertosAduanas.Count} Puertos, " +
                $"{paquete.Datos.Paises.Count} Países, {paquete.Datos.NcmCatalogo.Count} Posiciones NCM, {paquete.Datos.AdminUsuarios.Count} Admin." + Environment.NewLine +
                $"Firma Criptográfica SHA-256: {(firmaCoincide ? "VÁLIDA (" + firmaCalculada[..14] + "...)" : "INVÁLIDA - Archivo alterado externamente")}";

            return (true, firmaCoincide, paquete, resumen);
        }
        catch (Exception ex)
        {
            return (false, false, null, $"Error al leer el archivo de Backup: {ex.Message}");
        }
    }

    public async Task<(bool Ok, string Mensaje)> RestaurarBaseDesdeBackupAsync(string rutaArchivo)
    {
        var perm = VerificarPermisoOperacion("BACKUP_RESTAURAR");
        if (!perm.Permitido) return (false, perm.Mensaje);

        var inspeccion = await InspeccionarArchivoBackupAsync(rutaArchivo);
        if (!inspeccion.Valido || inspeccion.Paquete == null)
            return (false, inspeccion.Resumen);

        if (!inspeccion.FirmaIntegra)
            return (false, "Operación bloqueada por seguridad: la firma criptográfica SHA-256 del archivo de backup no coincide con su contenido.");

        var dbRestaurada = inspeccion.Paquete.Datos;
        SeguridadYDigitoVerificadorBLL.RecalcularTodoEnMemoria(dbRestaurada, $"RESTORE_{AdminActual?.Username ?? "Admin"}");

        var guardado = await Dal.GuardarBaseDatosCompletaAsync(
            dbRestaurada,
            sincronizarNube: true,
            reemplazarTablasEnRestore: true);

        return (guardado.Ok, $"RestoreBackup completado exitosamente: la base de datos actual fue cargada y sus Dígitos Verificadores fueron sincronizados. {guardado.Detalle}");
    }
}
