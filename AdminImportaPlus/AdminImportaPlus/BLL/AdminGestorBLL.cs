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
/// Capa BLL (.NET 9): Orquestador de Login BCrypt, Restablecimiento de Contraseña,
/// ABM Integral de la Web (Forwarders, Puertos, Países, NCM), Dígito Verificador y RestoreBackup.
/// </summary>
public sealed class AdminGestorBLL
{
    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    public RepositorioDatosDAL Dal { get; }
    public RolesCompositeBLL Roles { get; }
    public AdminUsuarioBE? AdminActual { get; private set; }

    public AdminGestorBLL(RepositorioDatosDAL? dal = null)
    {
        Dal = dal ?? new RepositorioDatosDAL();
        Roles = new RolesCompositeBLL(Dal.DataFolderPath);
    }

    // --- 1. LOGIN CON BCRYPT + VERIFICACIÓN AUTOMÁTICA DE DÍGITO VERIFICADOR ---

    public async Task<(bool Exito, string Mensaje, ResultadoVerificacionDV? VerificacionDV)> IniciarSesionAsync(
        string username,
        string passwordPlano)
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrEmpty(passwordPlano))
            return (false, "Ingresá el usuario y la contraseña.", null);

        var db = await Dal.ObtenerBaseDatosActualAsync();
        var admin = db.AdminUsuarios.FirstOrDefault(u =>
            string.Equals(u.Username.Trim(), username.Trim(), StringComparison.OrdinalIgnoreCase));

        if (admin == null || !admin.Activo)
            return (false, "Usuario administrador inexistente o inactivo.", null);

        if (!SeguridadYDigitoVerificadorBLL.VerificarPasswordBcrypt(passwordPlano, admin.PasswordHash))
            return (false, "Usuario o contraseña incorrectos.", null);

        // Verificar Dígito Verificador Horizontal (DVH) y Vertical (DVV) al logearse el Admin
        var resultadoDV = SeguridadYDigitoVerificadorBLL.VerificarBaseCompleta(db);
        AdminActual = admin;

        return (true, "Autenticación exitosa.", resultadoDV);
    }

    public async Task<(bool Ok, string Mensaje)> RestablecerPasswordAdminAsync(
        string username,
        string nuevaPassword,
        string confirmarPassword)
    {
        if (string.IsNullOrWhiteSpace(username))
            return (false, "El usuario es obligatorio.");

        if (string.IsNullOrWhiteSpace(nuevaPassword) || nuevaPassword.Length < 4)
            return (false, "La nueva contraseña debe tener al menos 4 caracteres.");

        if (!string.Equals(nuevaPassword, confirmarPassword, StringComparison.Ordinal))
            return (false, "Las contraseñas ingresadas no coinciden.");

        var db = await Dal.ObtenerBaseDatosActualAsync();
        var admin = db.AdminUsuarios.FirstOrDefault(u =>
            string.Equals(u.Username.Trim(), username.Trim(), StringComparison.OrdinalIgnoreCase));

        if (admin == null)
            return (false, $"No se encontró la cuenta '{username}'.");

        admin.PasswordHash = SeguridadYDigitoVerificadorBLL.HashearPasswordBcrypt(nuevaPassword);
        admin.PasswordUpdatedAt = DateTimeOffset.UtcNow;

        // Actualizar DVH de admin_usuarios y su DVV vertical manteniendo el estado del resto de las tablas
        admin.Dvh = SeguridadYDigitoVerificadorBLL.CalcularDvhAdmin(admin);
        ActualizarDvvDeTablaAdmin(db, username);

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

    // --- 3. ABM DE FORWARDERS (SIN LÍMITE DE CANTIDAD, ORDEN ÚNICO OBLIGATORIO) ---

    public async Task<(bool Ok, string Mensaje)> GuardarForwarderAsync(ForwarderBE item, string? rutaImagenLocal = null)
    {
        item.Nombre = (item.Nombre ?? string.Empty).Trim();
        item.Whatsapp = (item.Whatsapp ?? string.Empty).Trim();
        item.Email = (item.Email ?? string.Empty).Trim();
        item.ImagenUrl = (item.ImagenUrl ?? string.Empty).Trim();

        if (string.IsNullOrWhiteSpace(item.Nombre))
            return (false, "El nombre del Forwarder es obligatorio.");

        if (item.Nombre.Length > 120)
            return (false, "El nombre del Forwarder no puede superar los 120 caracteres.");

        if (item.Orden < 1)
            return (false, "El número de orden debe ser mayor o igual a 1.");

        if (string.IsNullOrWhiteSpace(item.Whatsapp) || Regex.Replace(item.Whatsapp, "[^0-9]", "").Length < 6)
            return (false, "Ingresá un número de WhatsApp válido con código de país (ej: +5491143218800).");

        if (!Regex.IsMatch(item.Email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            return (false, "Ingresá una dirección de Mail válida.");

        var db = await Dal.ObtenerBaseDatosActualAsync();

        // Regla de unicidad de Orden: impide que dos Forwarders compartan la misma posición (ej: dos en orden 1)
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
            return (false, "Seleccioná una imagen o ingresá una URL de foto para el Forwarder.");

        int idx = db.Forwarders.FindIndex(f => f.Id == item.Id);
        if (idx >= 0) db.Forwarders[idx] = item;
        else db.Forwarders.Add(item);

        SeguridadYDigitoVerificadorBLL.RecalcularTodoEnMemoria(db, AdminActual?.Username ?? "Admin");
        var res = await Dal.GuardarBaseDatosCompletaAsync(db);
        return (res.Ok, $"Forwarder '{item.Nombre}' (Orden #{item.Orden}) guardado exitosamente. {res.Detalle}");
    }

    public async Task<(bool Ok, string Mensaje)> EliminarForwarderAsync(Guid id)
    {
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

    // --- 4. ABM PUERTOS Y ADUANAS, PAÍSES Y CATÁLOGO NCM (GESTIÓN INTEGRAL DE LA WEB) ---

    public async Task<(bool Ok, string Mensaje)> GuardarPuertoAsync(PuertoAduanaBE puerto)
    {
        puerto.Nombre = (puerto.Nombre ?? string.Empty).Trim();
        puerto.Provincia = (puerto.Provincia ?? string.Empty).Trim();
        puerto.CodigoSim = (puerto.CodigoSim ?? string.Empty).Trim();
        puerto.Operador = (puerto.Operador ?? string.Empty).Trim();

        if (string.IsNullOrWhiteSpace(puerto.Nombre) || string.IsNullOrWhiteSpace(puerto.Provincia))
            return (false, "El nombre del puerto/aduana y la provincia son obligatorios.");

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
        var db = await Dal.ObtenerBaseDatosActualAsync();
        db.PuertosAduanas.RemoveAll(p => p.Id == id);
        await Dal.EliminarRegistroSupabaseAsync("puertos_aduanas", $"id=eq.{id}");
        SeguridadYDigitoVerificadorBLL.RecalcularTodoEnMemoria(db, AdminActual?.Username ?? "Admin");
        var res = await Dal.GuardarBaseDatosCompletaAsync(db);
        return (res.Ok, $"Puerto eliminado. {res.Detalle}");
    }

    public async Task<(bool Ok, string Mensaje)> GuardarNcmAsync(NcmPosicionBE ncm)
    {
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
        var db = await Dal.ObtenerBaseDatosActualAsync();
        db.NcmCatalogo.RemoveAll(n => string.Equals(n.CodigoNcm, codigoNcm, StringComparison.OrdinalIgnoreCase));
        await Dal.EliminarRegistroSupabaseAsync("ncm_catalogo", $"codigo_ncm=eq.{Uri.EscapeDataString(codigoNcm)}");
        SeguridadYDigitoVerificadorBLL.RecalcularTodoEnMemoria(db, AdminActual?.Username ?? "Admin");
        var res = await Dal.GuardarBaseDatosCompletaAsync(db);
        return (res.Ok, $"Posición NCM '{codigoNcm}' eliminada. {res.Detalle}");
    }

    public async Task<(bool Ok, string Mensaje)> GuardarPaisAsync(PaisBE pais)
    {
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

    // --- 5. SISTEMA DE RESTORE / BACKUP DE LA BASE DE DATOS ACTUAL ---

    public async Task<(bool Ok, string Mensaje, string RutaArchivo)> CrearBackupBaseActualAsync(string? rutaPersonalizada = null)
    {
        var db = await Dal.ObtenerBaseDatosActualAsync();
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

    public async Task<(bool Valido, BackupPackageBE? Paquete, string Resumen)> InspeccionarArchivoBackupAsync(string rutaArchivo)
    {
        if (!File.Exists(rutaArchivo))
            return (false, null, "El archivo de backup seleccionado no existe.");

        try
        {
            string contenido = await File.ReadAllTextAsync(rutaArchivo, Encoding.UTF8);
            var paquete = JsonSerializer.Deserialize<BackupPackageBE>(contenido, JsonOpts);
            if (paquete?.Datos == null)
                return (false, null, "El archivo no posee el formato válido de Backup IMPORTA+.");

            string jsonDatos = JsonSerializer.Serialize(paquete.Datos, JsonOpts);
            string firmaCalculada = SeguridadYDigitoVerificadorBLL.Sha256Hex(jsonDatos);
            bool firmaCoincide = string.Equals(paquete.FirmaIntegridadSha256, firmaCalculada, StringComparison.OrdinalIgnoreCase);

            string resumen =
                $"Fecha del Backup: {paquete.FechaBackup.LocalDateTime:dd/MM/yyyy HH:mm:ss} | Autor: {paquete.GeneradoPor}" + Environment.NewLine +
                $"Tablas incluidas: {paquete.Datos.Forwarders.Count} Forwarders, {paquete.Datos.PuertosAduanas.Count} Puertos, " +
                $"{paquete.Datos.Paises.Count} Países, {paquete.Datos.NcmCatalogo.Count} Posiciones NCM, {paquete.Datos.AdminUsuarios.Count} Admin." + Environment.NewLine +
                $"Firma Criptográfica SHA-256: {(firmaCoincide ? "VÁLIDA (" + firmaCalculada[..14] + "...)" : "ADVERTENCIA - Firma alterada")}";

            return (true, paquete, resumen);
        }
        catch (Exception ex)
        {
            return (false, null, $"Error al leer el archivo de Backup: {ex.Message}");
        }
    }

    public async Task<(bool Ok, string Mensaje)> RestaurarBaseDesdeBackupAsync(string rutaArchivo)
    {
        var inspeccion = await InspeccionarArchivoBackupAsync(rutaArchivo);
        if (!inspeccion.Valido || inspeccion.Paquete == null)
            return (false, inspeccion.Resumen);

        var dbRestaurada = inspeccion.Paquete.Datos;
        SeguridadYDigitoVerificadorBLL.RecalcularTodoEnMemoria(dbRestaurada, $"RESTORE_{AdminActual?.Username ?? "Admin"}");

        var guardado = await Dal.GuardarBaseDatosCompletaAsync(
            dbRestaurada,
            sincronizarNube: true,
            reemplazarTablasEnRestore: true);

        return (guardado.Ok, $"RestoreBackup completado exitosamente: la base de datos actual fue cargada y sus Dígitos Verificadores fueron sincronizados. {guardado.Detalle}");
    }
}
