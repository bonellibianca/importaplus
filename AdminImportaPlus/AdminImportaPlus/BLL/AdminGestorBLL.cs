using System;
using System.Collections.Generic;
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
/// Capa BLL (.NET 9): Orquestador de Login BCrypt con bloqueo de cuenta tras 3 intentos fallidos (ING-SOFTWARE-BritezG),
/// Gestión de Usuarios, Control de Acceso Basado en Botones (Patrón Composite), ABM Forwarders, ABM Despachantes de Aduana,
/// ABM Puertos, Dígito Verificador y RestoreBackup firmado.
/// </summary>
public sealed class AdminGestorBLL
{
    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    private const int MaxIntentosPorUsuario = 3;
    private readonly Dictionary<string, int> _intentosFallidosPorUsuario = new(StringComparer.OrdinalIgnoreCase);

    public RepositorioDatosDAL Dal { get; }
    public RolesCompositeBLL Roles { get; }
    public AdminUsuarioBE? AdminActual { get; private set; }

    public AdminGestorBLL(RepositorioDatosDAL? dal = null)
    {
        Dal = dal ?? new RepositorioDatosDAL();
        Roles = new RolesCompositeBLL(Dal.DataFolderPath);
        Roles.OnStoreChanged += () => _ = Dal.IntentarSincronizarRbacStoreAsync(Roles.Store);
    }

    public bool EsAdministradorActual()
    {
        if (AdminActual == null) return false;
        if (string.Equals(AdminActual.Username, "Admin", StringComparison.OrdinalIgnoreCase)) return true;
        var perf = Roles.ObtenerPerfilDeUsuario(AdminActual.Username, AdminActual.PerfilId);
        return perf != null && string.Equals(perf.Nombre, "Administrador", StringComparison.OrdinalIgnoreCase);
    }

    public bool UsuarioActualTieneBoton(string nombreBoton)
    {
        if (AdminActual == null) return false;
        return Roles.TienePermiso(AdminActual.Username, nombreBoton, AdminActual.PerfilId);
    }

    private (bool Permitido, string Mensaje) VerificarPermisoBoton(string nombreBoton)
    {
        if (AdminActual == null)
            return (false, "Sesión no autenticada. Iniciá sesión nuevamente.");

        if (!UsuarioActualTieneBoton(nombreBoton))
            return (false, $"Acceso denegado: tu perfil no tiene habilitado este botón ('{nombreBoton}').");

        return (true, string.Empty);
    }

    // --- 1. LOGIN CON BCRYPT + BLOQUEO DE CUENTA TRAS 3 INTENTOS (ING-SOFTWARE-BritezG) + DVH/DVV ---

    public async Task<(bool Exito, string Mensaje, ResultadoVerificacionDV? VerificacionDV)> IniciarSesionAsync(
        string username,
        string passwordPlano)
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrEmpty(passwordPlano))
            return (false, "Debe ingresar usuario y contraseña.", null);

        string cleanUser = username.Trim();

        // Si no hay clave secreta local, intentar autenticación y lectura directa por RPC en Supabase usando la Publishable Key integrada
        BaseDatosCompletaBE? dbDesdeRpc = null;
        if (!Dal.Config.EstaConfiguradoEnNube)
        {
            var rpcAuth = await Dal.IntentarAutenticarYObtenerEstadoRpcAsync(cleanUser, passwordPlano, Roles);
            if (rpcAuth.RpcDisponible)
            {
                if (rpcAuth.Status == "NOT_FOUND")
                    return (false, "Usuario no existe.", null);
                if (rpcAuth.Status == "INACTIVE")
                    return (false, "La cuenta se encuentra inactiva. Contacte al Administrador.", null);
                if (rpcAuth.Status == "LOCKED")
                    return (false, "Cuenta bloqueada por intentos fallidos. Contacte al Administrador en Gestión de Usuarios para desbloquearla.", null);
                if (rpcAuth.Status == "BAD_PASSWORD")
                {
                    _intentosFallidosPorUsuario.TryGetValue(cleanUser, out int prevRpc);
                    int actRpc = prevRpc + 1;
                    _intentosFallidosPorUsuario[cleanUser] = actRpc;
                    int restRpc = Math.Max(0, MaxIntentosPorUsuario - actRpc);
                    return (false, $"Contraseña inválida. Intentos: {actRpc}/{MaxIntentosPorUsuario}. Intentos restantes: {restRpc}.", null);
                }
                if (rpcAuth.Status == "OK" && rpcAuth.Datos != null)
                {
                    dbDesdeRpc = rpcAuth.Datos;
                }
            }
        }

        var db = dbDesdeRpc ?? await Dal.ObtenerBaseDatosActualAsync(Roles);
        var admin = db.AdminUsuarios.FirstOrDefault(u =>
            string.Equals(u.Username.Trim(), cleanUser, StringComparison.OrdinalIgnoreCase));

        string hashObjetivo = (admin != null && admin.Activo)
            ? admin.PasswordHash
            : SeguridadYDigitoVerificadorBLL.DummyBcryptHash;

        bool passwordValida = SeguridadYDigitoVerificadorBLL.VerificarPasswordBcrypt(passwordPlano, hashObjetivo);

        if (admin == null)
            return (false, "Usuario no existe.", null);

        if (!admin.Activo)
            return (false, "La cuenta se encuentra inactiva. Contacte al Administrador.", null);

        if (admin.Bloqueado)
            return (false, "Cuenta bloqueada por intentos fallidos. Contacte al Administrador en Gestión de Usuarios para desbloquearla.", null);

        if (!passwordValida)
        {
            _intentosFallidosPorUsuario.TryGetValue(cleanUser, out int prev);
            int actuales = prev + 1;
            _intentosFallidosPorUsuario[cleanUser] = actuales;

            // No bloquear la única cuenta raíz 'Admin' para evitar lockout irreversible, sí bloquear el resto de usuarios tras 3 intentos
            if (actuales >= MaxIntentosPorUsuario && !string.Equals(admin.Username, "Admin", StringComparison.OrdinalIgnoreCase))
            {
                admin.Bloqueado = true;
                var perfId = admin.PerfilId ?? Roles.ObtenerPerfilDeUsuario(admin.Username)?.Id ?? Guid.Empty;
                if (perfId != Guid.Empty)
                {
                    Roles.AsignarPerfilAUsuario(admin.Username, perfId, admin.Dni, admin.Nombre, admin.Apellido, bloqueado: true);
                }
                await Dal.GuardarBaseDatosCompletaAsync(db, rolesBll: Roles);
                return (false, "Cuenta bloqueada por 3 intentos fallidos. Contacte al Administrador.", null);
            }

            int restantes = Math.Max(0, MaxIntentosPorUsuario - actuales);
            return (false, $"Contraseña inválida. Intentos: {actuales}/{MaxIntentosPorUsuario}. Intentos restantes: {restantes}.", null);
        }

        _intentosFallidosPorUsuario.Remove(cleanUser);
        Dal.EstablecerSesionAutenticadaRpc(admin.Username, passwordPlano);

        var resultadoDV = SeguridadYDigitoVerificadorBLL.VerificarBaseCompleta(db);
        admin.UltimoAcceso = DateTimeOffset.UtcNow;
        AdminActual = admin;

        if (resultadoDV.SaltoDetectado && !EsAdministradorActual())
        {
            AdminActual = null;
            Dal.EstablecerSesionAutenticadaRpc(string.Empty, string.Empty);
            return (false, "¡ALERTA CRÍTICA! Se ha detectado una alteración externa en la base de datos. Solo un Administrador puede iniciar sesión para restaurar la integridad.", null);
        }

        if (!resultadoDV.SaltoDetectado)
        {
            await Dal.RegistrarUltimoAccesoLocalAsync(admin.Username, admin.UltimoAcceso.Value);
        }

        return (true, "Inicio de sesión con éxito.", resultadoDV);
    }

    // --- 2. GESTIÓN DE USUARIOS (Copiado y adaptado de ING-SOFTWARE-BritezG) ---

    public async Task<(bool Ok, string Mensaje, string PasswordGenerada)> CrearUsuarioAsync(
        int dni,
        string nombre,
        string apellido,
        string username,
        Guid perfilId)
    {
        var perm = VerificarPermisoBoton("btnCrearUsuario");
        if (!perm.Permitido) return (false, perm.Mensaje, string.Empty);

        nombre = (nombre ?? string.Empty).Trim();
        apellido = (apellido ?? string.Empty).Trim();
        username = (username ?? string.Empty).Trim();

        if (dni < 10000000 || dni > 99999999)
            return (false, "El DNI debe ser un número entero válido de 8 dígitos.", string.Empty);

        if (string.IsNullOrWhiteSpace(nombre) || string.IsNullOrWhiteSpace(apellido))
            return (false, "Por favor ingrese Nombre y Apellido.", string.Empty);

        if (string.IsNullOrWhiteSpace(username) || username.Length < 3)
            return (false, "El Nombre de Usuario debe tener al menos 3 caracteres.", string.Empty);

        var perfil = Roles.Store.Perfiles.FirstOrDefault(p => p.Id == perfilId);
        if (perfil == null)
            return (false, "Por favor seleccione un Rol / Perfil válido para el usuario.", string.Empty);

        var db = await Dal.ObtenerBaseDatosActualAsync(Roles);
        if (db.AdminUsuarios.Any(u => string.Equals(u.Username, username, StringComparison.OrdinalIgnoreCase)))
            return (false, $"El nombre de usuario '{username}' ya existe.", string.Empty);

        if (db.AdminUsuarios.Any(u => u.Dni == dni))
            return (false, $"El DNI '{dni}' ya está registrado con otro usuario.", string.Empty);

        // Regla idéntica a ING-SOFTWARE-BritezG: contraseña inicial = Nombre + DNI
        string passwordInicial = $"{nombre.Replace(" ", "")}{dni}";

        var nuevo = new AdminUsuarioBE
        {
            Id = Guid.NewGuid(),
            Dni = dni,
            Nombre = nombre,
            Apellido = apellido,
            Username = username,
            PasswordHash = SeguridadYDigitoVerificadorBLL.HashearPasswordBcrypt(passwordInicial),
            Bloqueado = false,
            Activo = true,
            PerfilId = perfil.Id,
            PasswordUpdatedAt = DateTimeOffset.UtcNow
        };

        db.AdminUsuarios.Add(nuevo);
        Roles.AsignarPerfilAUsuario(nuevo.Username, perfil.Id, dni, nombre, apellido, bloqueado: false);

        SeguridadYDigitoVerificadorBLL.RecalcularTodoEnMemoria(db, AdminActual?.Username ?? "Admin");
        var res = await Dal.GuardarBaseDatosCompletaAsync(db, rolesBll: Roles);
        return (res.Ok, $"Usuario '{username}' creado exitosamente.\nContraseña inicial asignada: {passwordInicial}\n\n{res.Detalle}", passwordInicial);
    }

    public async Task<(bool Ok, string Mensaje)> ModificarUsuarioAsync(
        Guid usuarioId,
        string nombre,
        string apellido,
        string nuevoUsername,
        Guid perfilId)
    {
        var perm = VerificarPermisoBoton("btnModificarUsuario");
        if (!perm.Permitido) return (false, perm.Mensaje);

        nombre = (nombre ?? string.Empty).Trim();
        apellido = (apellido ?? string.Empty).Trim();
        nuevoUsername = (nuevoUsername ?? string.Empty).Trim();

        if (string.IsNullOrWhiteSpace(nombre) || string.IsNullOrWhiteSpace(apellido) || string.IsNullOrWhiteSpace(nuevoUsername))
            return (false, "Nombre, Apellido y Nombre de Usuario son obligatorios.");

        var perfil = Roles.Store.Perfiles.FirstOrDefault(p => p.Id == perfilId);
        if (perfil == null)
            return (false, "Seleccione un Perfil válido.");

        var db = await Dal.ObtenerBaseDatosActualAsync(Roles);
        var u = db.AdminUsuarios.FirstOrDefault(x => x.Id == usuarioId);
        if (u == null)
            return (false, "No se encontró el usuario a modificar.");

        if (db.AdminUsuarios.Any(x => x.Id != usuarioId && string.Equals(x.Username, nuevoUsername, StringComparison.OrdinalIgnoreCase)))
            return (false, $"Ya existe otro usuario ocupando el Nombre de Usuario '{nuevoUsername}'.");

        string usernameAnterior = u.Username;
        u.Nombre = nombre;
        u.Apellido = apellido;
        u.Username = nuevoUsername;
        u.PerfilId = perfil.Id;

        if (!string.Equals(usernameAnterior, nuevoUsername, StringComparison.OrdinalIgnoreCase))
        {
            Roles.Store.PerfilPorUsuario.Remove(usernameAnterior);
            Roles.Store.FichasUsuarios.RemoveAll(f => string.Equals(f.Username, usernameAnterior, StringComparison.OrdinalIgnoreCase));
        }

        Roles.AsignarPerfilAUsuario(u.Username, perfil.Id, u.Dni, u.Nombre, u.Apellido, u.Bloqueado);
        if (AdminActual != null && AdminActual.Id == u.Id)
        {
            AdminActual = u;
        }

        SeguridadYDigitoVerificadorBLL.RecalcularTodoEnMemoria(db, AdminActual?.Username ?? "Admin");
        var res = await Dal.GuardarBaseDatosCompletaAsync(db, rolesBll: Roles);
        return (res.Ok, $"Usuario '{u.Username}' modificado correctamente. {res.Detalle}");
    }

    public async Task<(bool Ok, string Mensaje)> DesbloquearUsuarioAsync(Guid usuarioId)
    {
        var perm = VerificarPermisoBoton("btnDesbloquearUsuario");
        if (!perm.Permitido) return (false, perm.Mensaje);

        var db = await Dal.ObtenerBaseDatosActualAsync(Roles);
        var u = db.AdminUsuarios.FirstOrDefault(x => x.Id == usuarioId);
        if (u == null) return (false, "No se encontró el usuario seleccionado.");

        if (!u.Bloqueado)
            return (false, $"El usuario '{u.Username}' no se encuentra bloqueado.");

        u.Bloqueado = false;
        _intentosFallidosPorUsuario.Remove(u.Username);

        var perfId = u.PerfilId ?? Roles.ObtenerPerfilDeUsuario(u.Username)?.Id ?? Roles.Store.Perfiles.First().Id;
        Roles.AsignarPerfilAUsuario(u.Username, perfId, u.Dni, u.Nombre, u.Apellido, bloqueado: false);

        var res = await Dal.GuardarBaseDatosCompletaAsync(db, rolesBll: Roles);
        return (res.Ok, $"Usuario '{u.Username}' desbloqueado correctamente. {res.Detalle}");
    }

    public async Task<(bool Ok, string Mensaje)> CambiarEstadoUsuarioAsync(Guid usuarioId)
    {
        var perm = VerificarPermisoBoton("btnActDesactUsuario");
        if (!perm.Permitido) return (false, perm.Mensaje);

        var db = await Dal.ObtenerBaseDatosActualAsync(Roles);
        var u = db.AdminUsuarios.FirstOrDefault(x => x.Id == usuarioId);
        if (u == null) return (false, "No se encontró el usuario seleccionado.");

        if (u.Activo && string.Equals(u.Username, "Admin", StringComparison.OrdinalIgnoreCase))
            return (false, "No se puede desactivar al usuario Administrador raíz.");

        if (u.Activo && AdminActual != null && u.Id == AdminActual.Id)
            return (false, "No podés desactivar tu propia cuenta en sesión activa.");

        u.Activo = !u.Activo;
        SeguridadYDigitoVerificadorBLL.RecalcularTodoEnMemoria(db, AdminActual?.Username ?? "Admin");
        var res = await Dal.GuardarBaseDatosCompletaAsync(db, rolesBll: Roles);
        string nuevoEstado = u.Activo ? "ACTIVADO" : "DESACTIVADO";
        return (res.Ok, $"Usuario '{u.Username}' {nuevoEstado} correctamente. {res.Detalle}");
    }

    public async Task<(bool Ok, string Mensaje)> CambiarClaveUsuarioDesdeGestionAsync(Guid usuarioId, string nuevaPassword)
    {
        var perm = VerificarPermisoBoton("btnResetPassUsuario");
        if (!perm.Permitido) return (false, perm.Mensaje);

        if (!SeguridadYDigitoVerificadorBLL.ValidarFortalezaPassword(nuevaPassword, out string motivo))
            return (false, motivo);

        var db = await Dal.ObtenerBaseDatosActualAsync(Roles);
        var u = db.AdminUsuarios.FirstOrDefault(x => x.Id == usuarioId);
        if (u == null) return (false, "Usuario no encontrado.");

        u.PasswordHash = SeguridadYDigitoVerificadorBLL.HashearPasswordBcrypt(nuevaPassword);
        u.PasswordUpdatedAt = DateTimeOffset.UtcNow;
        u.Bloqueado = false;
        _intentosFallidosPorUsuario.Remove(u.Username);

        var perfId = u.PerfilId ?? Roles.ObtenerPerfilDeUsuario(u.Username)?.Id ?? Roles.Store.Perfiles.First().Id;
        Roles.AsignarPerfilAUsuario(u.Username, perfId, u.Dni, u.Nombre, u.Apellido, bloqueado: false);

        SeguridadYDigitoVerificadorBLL.RecalcularTodoEnMemoria(db, AdminActual?.Username ?? "Admin");
        var res = await Dal.GuardarBaseDatosCompletaAsync(db, rolesBll: Roles);
        if (res.Ok && AdminActual != null && u.Id == AdminActual.Id)
        {
            Dal.EstablecerSesionAutenticadaRpc(u.Username, nuevaPassword);
        }
        return (res.Ok, $"La contraseña del usuario '{u.Username}' fue actualizada exitosamente. {res.Detalle}");
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

        var db = await Dal.ObtenerBaseDatosActualAsync(Roles);
        var admin = db.AdminUsuarios.FirstOrDefault(u =>
            string.Equals(u.Username.Trim(), username.Trim(), StringComparison.OrdinalIgnoreCase));

        if (admin == null || !SeguridadYDigitoVerificadorBLL.VerificarPasswordBcrypt(passwordActual, admin.PasswordHash))
            return (false, "La contraseña actual es incorrecta.");

        admin.PasswordHash = SeguridadYDigitoVerificadorBLL.HashearPasswordBcrypt(nuevaPassword);
        admin.PasswordUpdatedAt = DateTimeOffset.UtcNow;

        SeguridadYDigitoVerificadorBLL.RecalcularTodoEnMemoria(db, admin.Username);
        var guardado = await Dal.GuardarBaseDatosCompletaAsync(db, rolesBll: Roles);
        if (guardado.Ok)
        {
            Dal.EstablecerSesionAutenticadaRpc(admin.Username, nuevaPassword);
        }
        return (guardado.Ok, $"Contraseña actualizada exitosamente. {guardado.Detalle}");
    }

    // --- 3. DÍGITO VERIFICADOR (VERIFICAR Y RECALCULAR) ---

    public async Task<ResultadoVerificacionDV> VerificarIntegridadActualAsync()
    {
        var db = await Dal.ObtenerBaseDatosActualAsync(Roles);
        return SeguridadYDigitoVerificadorBLL.VerificarBaseCompleta(db);
    }

    public async Task<(bool Ok, string Mensaje)> RecalcularDigitosVerificadoresAsync()
    {
        if (AdminActual != null && !EsAdministradorActual())
        {
            var perm = VerificarPermisoBoton("btnRecalcularDv");
            if (!perm.Permitido) return (false, perm.Mensaje);
        }

        string actor = AdminActual?.Username ?? "Admin";
        if (Dal.Config.EstaConfiguradoEnNube)
        {
            var rpc = await Dal.RecalcularDigitosEnSupabaseRpcAsync(actor);
            if (rpc.Ok)
            {
                return (true, $"Dígitos Verificadores (DVH / DVV) recalculados en Supabase sobre todas las tablas. {rpc.Detalle}");
            }
        }

        var db = await Dal.ObtenerBaseDatosActualAsync(Roles);
        SeguridadYDigitoVerificadorBLL.RecalcularTodoEnMemoria(db, actor);
        var res = await Dal.GuardarBaseDatosCompletaAsync(db, rolesBll: Roles);
        return (res.Ok, $"Dígitos Verificadores Horizontales (DVH) y Verticales (DVV) recalculados correctamente. {res.Detalle}");
    }

    public static int ObtenerSiguienteOrdenDisponible(BaseDatosCompletaBE db)
    {
        if (db.Forwarders.Count == 0) return 1;
        return db.Forwarders.Max(f => f.Orden) + 1;
    }

    public static int ObtenerSiguienteOrdenDespachante(BaseDatosCompletaBE db)
    {
        if (db.Despachantes.Count == 0) return 1;
        return db.Despachantes.Max(d => d.Orden) + 1;
    }

    // --- 4. ABM DE FORWARDERS ---

    public async Task<(bool Ok, string Mensaje)> GuardarForwarderAsync(ForwarderBE item, string? rutaImagenLocal = null)
    {
        var perm = VerificarPermisoBoton("btnGuardarFwd");
        if (!perm.Permitido) return (false, perm.Mensaje);

        var db = await Dal.ObtenerBaseDatosActualAsync(Roles);

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
        var res = await Dal.GuardarBaseDatosCompletaAsync(db, rolesBll: Roles);
        return (res.Ok, $"Forwarder '{item.Nombre}' (Orden #{item.Orden}) guardado exitosamente.\n{res.Detalle}");
    }

    public async Task<(bool Ok, string Mensaje)> EliminarForwarderAsync(Guid id)
    {
        var perm = VerificarPermisoBoton("btnEliminarFwd");
        if (!perm.Permitido) return (false, perm.Mensaje);

        var db = await Dal.ObtenerBaseDatosActualAsync(Roles);
        var existente = db.Forwarders.FirstOrDefault(f => f.Id == id);
        if (existente == null)
            return (false, "No se encontró el Forwarder seleccionado.");

        db.Forwarders.RemoveAll(f => f.Id == id);
        if (Dal.Config.EstaConfiguradoEnNube)
        {
            await Dal.EliminarRegistroSupabaseAsync("forwarders", $"id=eq.{id}");
        }

        SeguridadYDigitoVerificadorBLL.RecalcularTodoEnMemoria(db, AdminActual?.Username ?? "Admin");
        var res = await Dal.GuardarBaseDatosCompletaAsync(db, rolesBll: Roles);
        return (res.Ok, $"Forwarder '{existente.Nombre}' eliminado.\n{res.Detalle}");
    }

    // --- 5. ABM DE DESPACHANTES DE ADUANA ---

    public async Task<(bool Ok, string Mensaje)> GuardarDespachanteAsync(DespachanteBE item, string? rutaImagenLocal = null)
    {
        var perm = VerificarPermisoBoton("btnGuardarDesp");
        if (!perm.Permitido) return (false, perm.Mensaje);

        var db = await Dal.ObtenerBaseDatosActualAsync(Roles);

        item.Nombre = (item.Nombre ?? string.Empty).Trim();
        item.Whatsapp = (item.Whatsapp ?? string.Empty).Trim();
        item.Email = (item.Email ?? string.Empty).Trim();
        item.ImagenUrl = (item.ImagenUrl ?? string.Empty).Trim();

        if (string.IsNullOrWhiteSpace(item.Nombre))
            return (false, "El nombre del Despachante de Aduana es obligatorio.");

        if (item.Nombre.Length > 120)
            return (false, "El nombre del Despachante de Aduana no puede superar los 120 caracteres.");

        if (item.Orden < 1 || item.Orden > 999)
            return (false, "El número de orden debe estar comprendido entre 1 y 999.");

        if (string.IsNullOrWhiteSpace(item.Whatsapp) || Regex.Replace(item.Whatsapp, "[^0-9]", "").Length < 6)
            return (false, "Ingresá un número de WhatsApp válido con código de país (ej: +5491143218800).");

        if (!Regex.IsMatch(item.Email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            return (false, "Ingresá una dirección de Mail válida.");

        var conflictoOrden = db.Despachantes.FirstOrDefault(d => d.Id != item.Id && d.Orden == item.Orden);
        if (conflictoOrden != null)
        {
            int sugerido = ObtenerSiguienteOrdenDespachante(db);
            return (false,
                $"Orden duplicado: el orden {item.Orden} ya está ocupado por '{conflictoOrden.Nombre}'. Elegí otro orden (sugerido: {sugerido}) o reasigná primero el existente.");
        }

        if (!string.IsNullOrWhiteSpace(rutaImagenLocal) && File.Exists(rutaImagenLocal))
        {
            item.ImagenUrl = await Dal.ProcesarYSubirImagenDespachanteAsync(rutaImagenLocal, item.Id);
        }

        if (string.IsNullOrWhiteSpace(item.ImagenUrl))
            return (false, "Seleccioná una imagen local o ingresá una URL HTTPS segura para el Despachante de Aduana.");

        if (!SeguridadYDigitoVerificadorBLL.EsUrlImagenSegura(item.ImagenUrl))
            return (false, "La URL de imagen debe comenzar con https:// (dominio público válido) o ser una imagen subida desde el equipo.");

        int idx = db.Despachantes.FindIndex(d => d.Id == item.Id);
        if (idx >= 0) db.Despachantes[idx] = item;
        else db.Despachantes.Add(item);

        SeguridadYDigitoVerificadorBLL.RecalcularTodoEnMemoria(db, AdminActual?.Username ?? "Admin");
        var res = await Dal.GuardarBaseDatosCompletaAsync(db, rolesBll: Roles);
        return (res.Ok, $"Despachante de Aduana '{item.Nombre}' (Orden #{item.Orden}) guardado exitosamente.\n{res.Detalle}");
    }

    public async Task<(bool Ok, string Mensaje)> EliminarDespachanteAsync(Guid id)
    {
        var perm = VerificarPermisoBoton("btnEliminarDesp");
        if (!perm.Permitido) return (false, perm.Mensaje);

        var db = await Dal.ObtenerBaseDatosActualAsync(Roles);
        var existente = db.Despachantes.FirstOrDefault(d => d.Id == id);
        if (existente == null)
            return (false, "No se encontró el Despachante de Aduana seleccionado.");

        db.Despachantes.RemoveAll(d => d.Id == id);
        if (Dal.Config.EstaConfiguradoEnNube)
        {
            try { await Dal.EliminarRegistroSupabaseAsync("despachantes", $"id=eq.{id}"); } catch { }
        }

        SeguridadYDigitoVerificadorBLL.RecalcularTodoEnMemoria(db, AdminActual?.Username ?? "Admin");
        var res = await Dal.GuardarBaseDatosCompletaAsync(db, rolesBll: Roles);
        return (res.Ok, $"Despachante de Aduana '{existente.Nombre}' eliminado.\n{res.Detalle}");
    }

    // --- 6. ABM PUERTOS Y ADUANAS ---

    public async Task<(bool Ok, string Mensaje)> GuardarPuertoAsync(PuertoAduanaBE puerto)
    {
        var perm = VerificarPermisoBoton("btnGuardarPto");
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

        var db = await Dal.ObtenerBaseDatosActualAsync(Roles);
        int idx = db.PuertosAduanas.FindIndex(p => p.Id == puerto.Id);
        if (idx >= 0) db.PuertosAduanas[idx] = puerto;
        else db.PuertosAduanas.Add(puerto);

        SeguridadYDigitoVerificadorBLL.RecalcularTodoEnMemoria(db, AdminActual?.Username ?? "Admin");
        var res = await Dal.GuardarBaseDatosCompletaAsync(db, rolesBll: Roles);
        return (res.Ok, $"Puerto/Aduana '{puerto.Nombre}' actualizado.\n{res.Detalle}");
    }

    public async Task<(bool Ok, string Mensaje)> EliminarPuertoAsync(Guid id)
    {
        var perm = VerificarPermisoBoton("btnBorrarPto");
        if (!perm.Permitido) return (false, perm.Mensaje);

        var db = await Dal.ObtenerBaseDatosActualAsync(Roles);
        db.PuertosAduanas.RemoveAll(p => p.Id == id);
        if (Dal.Config.EstaConfiguradoEnNube)
        {
            await Dal.EliminarRegistroSupabaseAsync("puertos_aduanas", $"id=eq.{id}");
        }
        SeguridadYDigitoVerificadorBLL.RecalcularTodoEnMemoria(db, AdminActual?.Username ?? "Admin");
        var res = await Dal.GuardarBaseDatosCompletaAsync(db, rolesBll: Roles);
        return (res.Ok, $"Puerto eliminado.\n{res.Detalle}");
    }

    // --- 7. SISTEMA DE RESTORE / BACKUP CON VERIFICACIÓN ESTRICTA DE FIRMA SHA-256 ---

    public async Task<(bool Ok, string Mensaje, string RutaArchivo)> CrearBackupBaseActualAsync(string? rutaPersonalizada = null)
    {
        var perm = VerificarPermisoBoton("btnCrearBackup");
        if (!perm.Permitido) return (false, perm.Mensaje, string.Empty);

        var db = await Dal.ObtenerBaseDatosCompletaParaBackupAsync(Roles);
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
                $"Tablas incluidas: {paquete.Datos.Forwarders.Count} Forwarders, {paquete.Datos.Despachantes.Count} Despachantes, {paquete.Datos.PuertosAduanas.Count} Puertos, " +
                $"{paquete.Datos.Paises.Count} Países, {paquete.Datos.NcmCatalogo.Count} NCM, {paquete.Datos.AdminUsuarios.Count} Usuarios." + Environment.NewLine +
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
        if (!EsAdministradorActual())
        {
            var perm = VerificarPermisoBoton("btnEjecutarRestore");
            if (!perm.Permitido) return (false, perm.Mensaje);
        }

        var inspeccion = await InspeccionarArchivoBackupAsync(rutaArchivo);
        if (!inspeccion.Valido || inspeccion.Paquete == null)
            return (false, inspeccion.Resumen);

        if (!inspeccion.FirmaIntegra)
            return (false, "Operación bloqueada por seguridad: la firma criptográfica SHA-256 del archivo de backup no coincide con su contenido.");

        var dbRestaurada = inspeccion.Paquete.Datos;
        if (dbRestaurada.Despachantes == null || dbRestaurada.Despachantes.Count == 0)
            dbRestaurada.Despachantes = RepositorioDatosDAL.CrearSemillaDespachantes();

        SeguridadYDigitoVerificadorBLL.RecalcularTodoEnMemoria(dbRestaurada, $"RESTORE_{AdminActual?.Username ?? "Admin"}");

        var guardado = await Dal.GuardarBaseDatosCompletaAsync(
            dbRestaurada,
            sincronizarNube: true,
            reemplazarTablasEnRestore: true,
            rolesBll: Roles);

        return (guardado.Ok, $"RestoreBackup completado exitosamente: la base de datos actual fue cargada y sus Dígitos Verificadores fueron sincronizados.\n{guardado.Detalle}");
    }
}
