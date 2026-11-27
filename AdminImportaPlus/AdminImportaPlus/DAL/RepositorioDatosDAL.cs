using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Threading.Tasks;
using AdminImportaPlus.BE;
using AdminImportaPlus.BLL;

namespace AdminImportaPlus.DAL;

/// <summary>
/// Capa DAL (.NET 9): Persistencia Dual sobre Supabase PostgreSQL + Supabase Storage (HTTPS REST API)
/// con validación estricta de Secret Key (sb_secret_ / service_role), borrado real en nube y espejo Web sincronizado.
/// </summary>
public sealed class RepositorioDatosDAL
{
    private const string DpapiPrefix = "dpapi:";
    private const string ClaveParametroDespachantes = "CATALOGO_DESPACHANTES_JSON";
    private static readonly Encoding Utf8SinBom = new UTF8Encoding(false);
    private static readonly HttpClient Http = new() { Timeout = TimeSpan.FromSeconds(45) };

    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    public string AppBaseDir { get; }
    public string DataFolderPath { get; }
    public string ConfigFilePath { get; }
    public string LocalDbFilePath { get; }
    public string BackupsFolderPath { get; }
    public SupabaseConfigBE Config { get; private set; }

    private string _rpcUsername = string.Empty;
    private string _rpcPassword = string.Empty;
    public bool RpcDirectoActivo { get; private set; }
    public bool TieneSesionRpcActiva => !string.IsNullOrWhiteSpace(_rpcUsername) && !string.IsNullOrEmpty(_rpcPassword);
    public bool ConectadoDirectoNube => Config.EstaConfiguradoEnNube || RpcDirectoActivo;

    public void EstablecerSesionAutenticadaRpc(string username, string passwordPlano)
    {
        _rpcUsername = (username ?? string.Empty).Trim();
        _rpcPassword = passwordPlano ?? string.Empty;
    }

    public RepositorioDatosDAL()
    {
        AppBaseDir = AppContext.BaseDirectory;
        DataFolderPath = ResolverCarpetaDatosProyecto();
        Directory.CreateDirectory(DataFolderPath);

        ConfigFilePath = Path.Combine(DataFolderPath, "appsettings.supabase.json");
        LocalDbFilePath = Path.Combine(DataFolderPath, "importaplus_db.json");
        BackupsFolderPath = Path.Combine(DataFolderPath, "Backups");
        Directory.CreateDirectory(BackupsFolderPath);

        Config = CargarConfiguracion();
    }

    private static string ProtegerSecretoDpapi(string textoPlano)
    {
        if (string.IsNullOrWhiteSpace(textoPlano)) return string.Empty;
        if (textoPlano.StartsWith(DpapiPrefix, StringComparison.Ordinal)) return textoPlano;
        try
        {
            byte[] plainBytes = Encoding.UTF8.GetBytes(textoPlano);
            byte[] cipherBytes = ProtectedData.Protect(plainBytes, null, DataProtectionScope.CurrentUser);
            return DpapiPrefix + Convert.ToBase64String(cipherBytes);
        }
        catch
        {
            return textoPlano;
        }
    }

    private static string DesprotegerSecretoDpapi(string valorAlmacenado)
    {
        if (string.IsNullOrWhiteSpace(valorAlmacenado)) return string.Empty;
        if (!valorAlmacenado.StartsWith(DpapiPrefix, StringComparison.Ordinal)) return valorAlmacenado;
        try
        {
            byte[] cipherBytes = Convert.FromBase64String(valorAlmacenado[DpapiPrefix.Length..]);
            byte[] plainBytes = ProtectedData.Unprotect(cipherBytes, null, DataProtectionScope.CurrentUser);
            return Encoding.UTF8.GetString(plainBytes);
        }
        catch
        {
            return string.Empty;
        }
    }

    private string ResolverCarpetaDatosProyecto()
    {
        var dir = new DirectoryInfo(AppBaseDir);
        while (dir != null)
        {
            if (File.Exists(Path.Combine(dir.FullName, "Base_ImportaPlus.sql")))
            {
                return Path.Combine(dir.FullName, "AdminImportaPlus", "Data");
            }
            dir = dir.Parent;
        }
        return Path.Combine(AppBaseDir, "Data");
    }

    public string ResolverRutaWebWorkspace()
    {
        if (!string.IsNullOrWhiteSpace(Config.WebWorkspacePath) && Directory.Exists(Config.WebWorkspacePath))
            return Config.WebWorkspacePath;

        var dir = new DirectoryInfo(AppBaseDir);
        while (dir != null)
        {
            if (File.Exists(Path.Combine(dir.FullName, "Base_ImportaPlus.sql")) &&
                File.Exists(Path.Combine(dir.FullName, "index.html")))
            {
                return dir.FullName;
            }
            dir = dir.Parent;
        }
        return string.Empty;
    }

    public SupabaseConfigBE CargarConfiguracion()
    {
        SupabaseConfigBE cfg = new();
        bool migrarTextoPlanoADpapi = false;

        if (File.Exists(ConfigFilePath))
        {
            try
            {
                cfg = JsonSerializer.Deserialize<SupabaseConfigBE>(File.ReadAllText(ConfigFilePath, Encoding.UTF8), JsonOpts) ?? new();
                if (!string.IsNullOrWhiteSpace(cfg.ServiceRoleKey))
                {
                    if (cfg.ServiceRoleKey.StartsWith(DpapiPrefix, StringComparison.Ordinal))
                    {
                        cfg.ServiceRoleKey = DesprotegerSecretoDpapi(cfg.ServiceRoleKey);
                    }
                    else
                    {
                        migrarTextoPlanoADpapi = true;
                    }
                }
            }
            catch { }
        }

        if (string.IsNullOrWhiteSpace(cfg.SupabaseUrl) || cfg.SupabaseUrl.Contains("TU_PROJECT_REF", StringComparison.OrdinalIgnoreCase))
        {
            cfg.SupabaseUrl = SupabaseConfigBE.DefaultProjectUrl;
        }

        if (string.IsNullOrWhiteSpace(cfg.AnonKey) || cfg.AnonKey == "tu-anon-key")
        {
            cfg.AnonKey = SupabaseConfigBE.DefaultPublishableKey;
        }

        if (string.IsNullOrWhiteSpace(cfg.WebWorkspacePath))
        {
            var dir = new DirectoryInfo(AppBaseDir);
            while (dir != null)
            {
                if (File.Exists(Path.Combine(dir.FullName, "Base_ImportaPlus.sql")))
                {
                    cfg.WebWorkspacePath = dir.FullName;
                    break;
                }
                dir = dir.Parent;
            }
        }

        if (!cfg.TieneClaveSecretaValida && !string.IsNullOrWhiteSpace(cfg.WebWorkspacePath))
        {
            string envLocal = Path.Combine(cfg.WebWorkspacePath, ".env.local");
            if (File.Exists(envLocal))
            {
                foreach (var rawLine in File.ReadAllLines(envLocal))
                {
                    string line = rawLine.Trim();
                    if (string.IsNullOrEmpty(line) || line.StartsWith('#')) continue;
                    int eq = line.IndexOf('=');
                    if (eq <= 0) continue;
                    string k = line[..eq].Trim();
                    string v = line[(eq + 1)..].Trim().Trim('"', '\'');

                    if (k == "NEXT_PUBLIC_SUPABASE_URL" && !string.IsNullOrWhiteSpace(v) && !v.Contains("TU_PROJECT_REF"))
                        cfg.SupabaseUrl = v;
                    else if (k == "SUPABASE_SERVICE_ROLE_KEY" && !string.IsNullOrWhiteSpace(v) && !v.StartsWith("sb_publishable_"))
                        cfg.ServiceRoleKey = v;
                    else if (k == "NEXT_PUBLIC_SUPABASE_ANON_KEY" && !string.IsNullOrWhiteSpace(v))
                        cfg.AnonKey = v;
                }
            }
        }

        if (migrarTextoPlanoADpapi && cfg.TieneClaveSecretaValida)
        {
            PersistirConfigCifradaEnDisco(cfg);
        }

        return cfg;
    }

    private void PersistirConfigCifradaEnDisco(SupabaseConfigBE cfgEnMemoria)
    {
        try
        {
            var discoDto = new SupabaseConfigBE
            {
                SupabaseUrl = cfgEnMemoria.SupabaseUrl,
                ServiceRoleKey = ProtegerSecretoDpapi(cfgEnMemoria.ServiceRoleKey),
                AnonKey = cfgEnMemoria.AnonKey,
                WebWorkspacePath = cfgEnMemoria.WebWorkspacePath
            };
            File.WriteAllText(ConfigFilePath, JsonSerializer.Serialize(discoDto, JsonOpts), Encoding.UTF8);
        }
        catch { }
    }

    public void GuardarConfiguracion(SupabaseConfigBE nuevaConfig)
    {
        nuevaConfig.SupabaseUrl = string.IsNullOrWhiteSpace(nuevaConfig.SupabaseUrl)
            ? SupabaseConfigBE.DefaultProjectUrl
            : nuevaConfig.SupabaseUrl.Trim().TrimEnd('/');
        nuevaConfig.ServiceRoleKey = (nuevaConfig.ServiceRoleKey ?? string.Empty).Trim();
        nuevaConfig.AnonKey = string.IsNullOrWhiteSpace(nuevaConfig.AnonKey)
            ? SupabaseConfigBE.DefaultPublishableKey
            : nuevaConfig.AnonKey.Trim();

        Config = nuevaConfig;
        PersistirConfigCifradaEnDisco(Config);

        string webRoot = ResolverRutaWebWorkspace();
        if (!string.IsNullOrWhiteSpace(webRoot))
        {
            SincronizarEnvLocalWeb(webRoot);
        }
    }

    private void SincronizarEnvLocalWeb(string webRoot)
    {
        try
        {
            string envLocalPath = Path.Combine(webRoot, ".env.local");
            var sb = new StringBuilder();
            sb.AppendLine("# Generado/Sincronizado desde AdminImportaPlus.exe (.NET 9)");
            sb.AppendLine($"NEXT_PUBLIC_SUPABASE_URL={Config.SupabaseUrl}");
            if (!string.IsNullOrWhiteSpace(Config.AnonKey))
                sb.AppendLine($"NEXT_PUBLIC_SUPABASE_ANON_KEY={Config.AnonKey}");
            sb.AppendLine($"SUPABASE_SERVICE_ROLE_KEY={(Config.TieneClaveSecretaValida ? Config.ServiceRoleKey : string.Empty)}");
            sb.AppendLine("NEXT_PUBLIC_APP_ORIGIN=https://importaplus.com.ar");
            File.WriteAllText(envLocalPath, sb.ToString(), Encoding.UTF8);
        }
        catch { }
    }

    public async Task RegistrarUltimoAccesoLocalAsync(string username, DateTimeOffset timestamp)
    {
        if (!File.Exists(LocalDbFilePath)) return;
        try
        {
            var local = JsonSerializer.Deserialize<BaseDatosCompletaBE>(
                await File.ReadAllTextAsync(LocalDbFilePath, Encoding.UTF8), JsonOpts);
            if (local == null) return;
            var u = local.AdminUsuarios.FirstOrDefault(a =>
                string.Equals(a.Username.Trim(), username.Trim(), StringComparison.OrdinalIgnoreCase));
            if (u != null)
            {
                u.UltimoAcceso = timestamp;
                await File.WriteAllTextAsync(LocalDbFilePath, JsonSerializer.Serialize(local, JsonOpts), Encoding.UTF8);
            }
        }
        catch { }
    }

    public async Task<BaseDatosCompletaBE> ObtenerBaseDatosActualAsync(RolesCompositeBLL? rolesBll = null)
    {
        if (Config.EstaConfiguradoEnNube)
        {
            var remota = await IntentarLeerDesdeSupabaseAsync(rolesBll);
            if (remota != null)
            {
                EnriquecerUsuariosConFichasLocales(remota, rolesBll);
                GuardarEspejoLocalYSincronizarWeb(remota);
                return remota;
            }
        }
        else if (TieneSesionRpcActiva)
        {
            var rpcRead = await IntentarAutenticarYObtenerEstadoRpcAsync(_rpcUsername, _rpcPassword, rolesBll);
            if (rpcRead.RpcDisponible && rpcRead.Datos != null)
            {
                RpcDirectoActivo = true;
                EnriquecerUsuariosConFichasLocales(rpcRead.Datos, rolesBll);
                GuardarEspejoLocalYSincronizarWeb(rpcRead.Datos);
                return rpcRead.Datos;
            }
        }

        if (File.Exists(LocalDbFilePath))
        {
            try
            {
                var local = JsonSerializer.Deserialize<BaseDatosCompletaBE>(
                    await File.ReadAllTextAsync(LocalDbFilePath, Encoding.UTF8), JsonOpts);
                if (local != null && local.AdminUsuarios.Count > 0)
                {
                    if (local.Despachantes == null || local.Despachantes.Count == 0)
                    {
                        // Si viene de una versión anterior sin Despachantes aún, cargar semilla inicial de Despachantes
                        if (!local.DigitosVerificadores.Any(d => string.Equals(d.TablaNombre, "despachantes", StringComparison.OrdinalIgnoreCase)))
                        {
                            local.Despachantes = CrearSemillaDespachantes();
                            SeguridadYDigitoVerificadorBLL.RecalcularTodoEnMemoria(local, "MIGRATION_DESP");
                            GuardarEspejoLocalYSincronizarWeb(local);
                        }
                    }
                    EnriquecerUsuariosConFichasLocales(local, rolesBll);
                    return local;
                }
            }
            catch { }
        }

        var semilla = CrearSemillaInicial();
        EnriquecerUsuariosConFichasLocales(semilla, rolesBll);
        await GuardarBaseDatosCompletaAsync(semilla, sincronizarNube: Config.EstaConfiguradoEnNube);
        return semilla;
    }

    private static void EnriquecerUsuariosConFichasLocales(BaseDatosCompletaBE db, RolesCompositeBLL? rolesBll)
    {
        if (rolesBll == null) return;
        foreach (var u in db.AdminUsuarios)
        {
            var ficha = rolesBll.Store.FichasUsuarios.FirstOrDefault(f =>
                string.Equals(f.Username, u.Username, StringComparison.OrdinalIgnoreCase));

            if (ficha != null)
            {
                if (u.Dni <= 0 || u.Dni == 11111111) u.Dni = ficha.Dni;
                if (string.IsNullOrWhiteSpace(u.Nombre) || u.Nombre == "Administrador") u.Nombre = ficha.Nombre;
                if (string.IsNullOrWhiteSpace(u.Apellido) || u.Apellido == "General") u.Apellido = ficha.Apellido;
                u.Bloqueado = u.Bloqueado || ficha.Bloqueado;
                u.PerfilId ??= ficha.PerfilId;
            }

            if (!u.PerfilId.HasValue)
            {
                var perf = rolesBll.ObtenerPerfilDeUsuario(u.Username);
                if (perf != null) u.PerfilId = perf.Id;
            }
        }
    }

    public async Task<BaseDatosCompletaBE> ObtenerBaseDatosCompletaParaBackupAsync(RolesCompositeBLL? rolesBll = null)
    {
        var db = await ObtenerBaseDatosActualAsync(rolesBll);
        if (Config.EstaConfiguradoEnNube && (db.Paises.Count == 0 || db.NcmCatalogo.Count == 0))
        {
            try
            {
                var paisesRemotos = await GetTablaSupabaseAsync<PaisBE>("/rest/v1/paises?select=*&order=codigo_iso.asc");
                var ncmRemotos = await GetTablaSupabaseAsync<NcmPosicionBE>("/rest/v1/ncm_catalogo?select=*&order=codigo_ncm.asc");
                if (paisesRemotos != null && paisesRemotos.Count > 0) db.Paises = paisesRemotos;
                if (ncmRemotos != null && ncmRemotos.Count > 0) db.NcmCatalogo = ncmRemotos;
            }
            catch { }
        }
        return db;
    }

    public static List<DespachanteBE> CrearSemillaDespachantes() => new()
    {
        new()
        {
            Id = Guid.Parse("e1111111-1111-1111-1111-111111111111"),
            Nombre = "Bonelli & Asociados Despachantes DGA",
            ImagenUrl = "https://images.unsplash.com/photo-1554224155-6726b3ff858f?auto=format&fit=crop&w=600&q=80",
            Whatsapp = "+5491143218800",
            Email = "despachos@importaplus.com.ar",
            Orden = 1,
            Activo = true
        },
        new()
        {
            Id = Guid.Parse("e2222222-2222-2222-2222-222222222222"),
            Nombre = "Ezeiza Express Despachos Aduaneros",
            ImagenUrl = "https://images.unsplash.com/photo-1454165804606-c3d57bc86b40?auto=format&fit=crop&w=600&q=80",
            Whatsapp = "+5491144802233",
            Email = "ezeiza@importaplus.com.ar",
            Orden = 2,
            Activo = true
        },
        new()
        {
            Id = Guid.Parse("e3333333-3333-3333-3333-333333333333"),
            Nombre = "Estudio Aduanero Río de la Plata",
            ImagenUrl = "https://images.unsplash.com/photo-1507679799987-c73779587ccf?auto=format&fit=crop&w=600&q=80",
            Whatsapp = "+5491150903344",
            Email = "aduanas@importaplus.com.ar",
            Orden = 3,
            Activo = true
        },
        new()
        {
            Id = Guid.Parse("e4444444-4444-4444-4444-444444444444"),
            Nombre = "Andes & Pacífico Despachantes Matriculados",
            ImagenUrl = "https://images.unsplash.com/photo-1521791136064-7986c2920216?auto=format&fit=crop&w=600&q=80",
            Whatsapp = "+5492614205544",
            Email = "cuyo@importaplus.com.ar",
            Orden = 4,
            Activo = true
        }
    };

    public BaseDatosCompletaBE CrearSemillaInicial()
    {
        var db = new BaseDatosCompletaBE
        {
            AdminUsuarios = new List<AdminUsuarioBE>
            {
                new()
                {
                    Id = Guid.Parse("99999999-9999-9999-9999-999999999999"),
                    Dni = 11111111,
                    Nombre = "Administrador",
                    Apellido = "General",
                    Username = "Admin",
                    PasswordHash = SeguridadYDigitoVerificadorBLL.HashearPasswordBcrypt("1234"),
                    Bloqueado = false,
                    Activo = true,
                    PerfilId = Guid.Parse("30000002-0000-0000-0000-000000000001"),
                    PasswordUpdatedAt = DateTimeOffset.UtcNow
                }
            },
            Forwarders = new List<ForwarderBE>
            {
                new()
                {
                    Id = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                    Nombre = "Bonelli & Asociados Global Logistics",
                    ImagenUrl = "https://images.unsplash.com/photo-1586528116311-ad8dd3c8310d?auto=format&fit=crop&w=600&q=80",
                    Whatsapp = "+5491143218800",
                    Email = "contacto@importaplus.com.ar",
                    Orden = 1,
                    Activo = true
                },
                new()
                {
                    Id = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"),
                    Nombre = "NEXA Ocean & Air Forwarding",
                    ImagenUrl = "https://images.unsplash.com/photo-1494412574643-ff11b0a5c1c3?auto=format&fit=crop&w=600&q=80",
                    Whatsapp = "+5491152391100",
                    Email = "operaciones@importaplus.com.ar",
                    Orden = 2,
                    Activo = true
                },
                new()
                {
                    Id = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc"),
                    Nombre = "Ezeiza Express Cargo & Customs",
                    ImagenUrl = "https://images.unsplash.com/photo-1570710891163-6d3b5c47248b?auto=format&fit=crop&w=600&q=80",
                    Whatsapp = "+5491144802233",
                    Email = "ezeiza@importaplus.com.ar",
                    Orden = 3,
                    Activo = true
                },
                new()
                {
                    Id = Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddddddd"),
                    Nombre = "Andina Multimodal Internacional",
                    ImagenUrl = "https://images.unsplash.com/photo-1601584115197-04ecc0da31d7?auto=format&fit=crop&w=600&q=80",
                    Whatsapp = "+5492614205544",
                    Email = "mendoza@importaplus.com.ar",
                    Orden = 4,
                    Activo = true
                }
            },
            Despachantes = CrearSemillaDespachantes(),
            PuertosAduanas = new List<PuertoAduanaBE>
            {
                new()
                {
                    Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                    Nombre = "Puerto de Buenos Aires",
                    ViaIngreso = "MARITIMA_FLUVIAL",
                    Provincia = "Ciudad Autónoma de Buenos Aires",
                    CodigoSim = "ADU-001-PBA",
                    Operador = "AGPSE (Administración General de Puertos)",
                    Infraestructura = "Terminales 1, 2, 3, 4 y 5 (Puerto Nuevo). Escáneres de alta penetración.",
                    Latitud = -34.598500m,
                    Longitud = -58.373100m
                },
                new()
                {
                    Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                    Nombre = "Aduana de Ezeiza",
                    ViaIngreso = "AEREA",
                    Provincia = "Buenos Aires",
                    CodigoSim = "ADU-073-EZE",
                    Operador = "ARCA-DGA / Aeropuertos Argentina 2000",
                    Infraestructura = "Terminal de Cargas Aéreas (TCA) con cámaras frigoríficas y depósito fiscal.",
                    Latitud = -34.822200m,
                    Longitud = -58.535800m
                },
                new()
                {
                    Id = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                    Nombre = "Puerto de Rosario",
                    ViaIngreso = "MARITIMA_FLUVIAL",
                    Provincia = "Santa Fe",
                    CodigoSim = "ADU-052-ROS",
                    Operador = "Ente Administrador Puerto Rosario (ENAPRO)",
                    Infraestructura = "Muelle de aguas profundas sobre la Hidrovía Paraná-Paraguay.",
                    Latitud = -32.946800m,
                    Longitud = -60.639300m
                },
                new()
                {
                    Id = Guid.Parse("44444444-4444-4444-4444-444444444444"),
                    Nombre = "Paso Cristo Redentor",
                    ViaIngreso = "TERRESTRE",
                    Provincia = "Mendoza",
                    CodigoSim = "ADU-042-PCR",
                    Operador = "Gendarmería Nacional / ARCA-DGA",
                    Infraestructura = "Complejo Fronterizo Los Horcones - Corredor Bioceánico.",
                    Latitud = -32.835300m,
                    Longitud = -70.064900m
                },
                new()
                {
                    Id = Guid.Parse("55555555-5555-5555-5555-555555555555"),
                    Nombre = "Puerto de Mar del Plata",
                    ViaIngreso = "MARITIMA_FLUVIAL",
                    Provincia = "Buenos Aires",
                    CodigoSim = "ADU-038-MDP",
                    Operador = "Consorcio Portuario Regional Mar del Plata",
                    Infraestructura = "Terminal de contenedores y carga frigorífica industrial regional.",
                    Latitud = -38.005500m,
                    Longitud = -57.542600m
                }
            },
            Paises = new List<PaisBE>
            {
                new() { CodigoIso = "CHN", Nombre = "China", EsMercosur = false, FleteEstimadoPct = 10.00m },
                new() { CodigoIso = "USA", Nombre = "Estados Unidos", EsMercosur = false, FleteEstimadoPct = 8.00m },
                new() { CodigoIso = "BRA", Nombre = "Brasil", EsMercosur = true, FleteEstimadoPct = 5.00m },
                new() { CodigoIso = "URY", Nombre = "Uruguay", EsMercosur = true, FleteEstimadoPct = 4.00m },
                new() { CodigoIso = "PRY", Nombre = "Paraguay", EsMercosur = true, FleteEstimadoPct = 4.50m },
                new() { CodigoIso = "BOL", Nombre = "Bolivia", EsMercosur = true, FleteEstimadoPct = 4.50m },
                new() { CodigoIso = "DEU", Nombre = "Alemania", EsMercosur = false, FleteEstimadoPct = 9.00m },
                new() { CodigoIso = "ITA", Nombre = "Italia", EsMercosur = false, FleteEstimadoPct = 9.00m },
                new() { CodigoIso = "ESP", Nombre = "España", EsMercosur = false, FleteEstimadoPct = 8.50m },
                new() { CodigoIso = "VNM", Nombre = "Vietnam", EsMercosur = false, FleteEstimadoPct = 11.00m }
            },
            NcmCatalogo = new List<NcmPosicionBE>
            {
                new() { CodigoNcm = "8471.30.12", Descripcion = "Notebooks y Laptops portátiles de procesamiento de datos", TipoBien = "BIT", Aec = 16.00m, IvaAlicuota = 10.50m, AnexoExcepcion = "Decreto 557/2023 - Anexo I", ArancelExcepcion = 2.00m },
                new() { CodigoNcm = "8517.12.31", Descripcion = "Teléfonos móviles (celulares) y de redes inalámbricas", TipoBien = "BIT", Aec = 16.00m, IvaAlicuota = 21.00m },
                new() { CodigoNcm = "6109.10.00", Descripcion = "T-shirts y camisetas de punto de algodón", TipoBien = "GENERAL", Aec = 35.00m, IvaAlicuota = 21.00m },
                new() { CodigoNcm = "6402.19.00", Descripcion = "Calzado deportivo con suela y capellada de caucho o plástico", TipoBien = "GENERAL", Aec = 35.00m, IvaAlicuota = 21.00m },
                new() { CodigoNcm = "8471.41.00", Descripcion = "Unidades de proceso digitales integradas (Servidores BK)", TipoBien = "BK", Aec = 16.00m, IvaAlicuota = 10.50m, AnexoExcepcion = "Decreto 557/2023 - Bien de Capital", ArancelExcepcion = 0.00m }
            }
        };

        SeguridadYDigitoVerificadorBLL.RecalcularTodoEnMemoria(db, "INITIAL_SEED");
        return db;
    }

    public async Task<(bool Ok, string Detalle)> GuardarBaseDatosCompletaAsync(
        BaseDatosCompletaBE db,
        bool sincronizarNube = true,
        bool reemplazarTablasEnRestore = false,
        RolesCompositeBLL? rolesBll = null)
    {
        GuardarEspejoLocalYSincronizarWeb(db);

        if (!sincronizarNube)
        {
            return (true, "Guardado en base local y sincronizado con la carpeta Web local.");
        }

        if (Config.EstaConfiguradoEnNube)
        {
            return await SincronizarHaciaSupabaseAsync(db, reemplazarTablasEnRestore, rolesBll);
        }

        if (TieneSesionRpcActiva)
        {
            return await SincronizarHaciaSupabasePorRpcAsync(db, rolesBll);
        }

        return (true, "Cambios guardados.");
    }

    private sealed class RpcEstadoRespuestaDto
    {
        public string status { get; set; } = string.Empty;
        public List<AdminUsuarioBE>? admin_usuarios { get; set; }
        public List<ForwarderBE>? forwarders { get; set; }
        public List<DespachanteBE>? despachantes { get; set; }
        public List<PuertoAduanaBE>? puertos_aduanas { get; set; }
        public List<DigitoVerificadorBE>? digito_verificador { get; set; }
        public RolesStoreBE? rbac_store { get; set; }
        public int? total_paises { get; set; }
        public int? total_ncm { get; set; }
    }

    private HttpRequestMessage CrearRequestPublishableRpc(string relativePath)
    {
        string urlBase = string.IsNullOrWhiteSpace(Config.SupabaseUrl)
            ? SupabaseConfigBE.DefaultProjectUrl
            : Config.SupabaseUrl.TrimEnd('/');
        string pubKey = string.IsNullOrWhiteSpace(Config.AnonKey)
            ? SupabaseConfigBE.DefaultPublishableKey
            : Config.AnonKey.Trim();

        var req = new HttpRequestMessage(HttpMethod.Post, $"{urlBase}{relativePath}");
        req.Headers.Add("apikey", pubKey);
        if (pubKey.StartsWith("eyJ", StringComparison.Ordinal))
        {
            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", pubKey);
        }
        req.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        return req;
    }

    public async Task<(bool RpcDisponible, string Status, BaseDatosCompletaBE? Datos)> IntentarAutenticarYObtenerEstadoRpcAsync(
        string username,
        string passwordPlano,
        RolesCompositeBLL? rolesBll = null)
    {
        try
        {
            using var req = CrearRequestPublishableRpc("/rest/v1/rpc/admin_rpc_obtener_estado");
            string body = JsonSerializer.Serialize(new { p_username = username, p_password = passwordPlano }, JsonOpts);
            req.Content = new StringContent(body, Encoding.UTF8, "application/json");

            using var res = await Http.SendAsync(req);
            if (!res.IsSuccessStatusCode)
            {
                return (false, "RPC_NOT_AVAILABLE", null);
            }

            string raw = await res.Content.ReadAsStringAsync();
            var dto = JsonSerializer.Deserialize<RpcEstadoRespuestaDto>(raw, JsonOpts);
            if (dto == null || string.IsNullOrWhiteSpace(dto.status))
            {
                return (false, "INVALID_RESPONSE", null);
            }

            if (!string.Equals(dto.status, "OK", StringComparison.OrdinalIgnoreCase))
            {
                return (true, dto.status, null);
            }

            if (rolesBll != null && dto.rbac_store != null)
            {
                rolesBll.ReemplazarStoreDesdeNube(dto.rbac_store);
            }

            var db = new BaseDatosCompletaBE
            {
                AdminUsuarios = dto.admin_usuarios ?? new(),
                Forwarders = dto.forwarders ?? new(),
                Despachantes = dto.despachantes ?? CrearSemillaDespachantes(),
                PuertosAduanas = dto.puertos_aduanas ?? new(),
                Paises = new(),
                NcmCatalogo = new(),
                DigitosVerificadores = dto.digito_verificador ?? new(),
                TotalPaisesEnNube = dto.total_paises,
                TotalNcmEnNube = dto.total_ncm
            };

            RpcDirectoActivo = true;
            return (true, "OK", db);
        }
        catch
        {
            return (false, "NETWORK_OR_MISSING", null);
        }
    }

    private async Task<(bool Ok, string Detalle)> SincronizarHaciaSupabasePorRpcAsync(
        BaseDatosCompletaBE db,
        RolesCompositeBLL? rolesBll = null)
    {
        try
        {
            using var req = CrearRequestPublishableRpc("/rest/v1/rpc/admin_rpc_sincronizar_estado");
            var payload = new
            {
                p_username = _rpcUsername,
                p_password = _rpcPassword,
                p_payload = new
                {
                    forwarders = db.Forwarders,
                    despachantes = db.Despachantes,
                    puertos_aduanas = db.PuertosAduanas,
                    admin_usuarios = db.AdminUsuarios,
                    digito_verificador = db.DigitosVerificadores,
                    rbac_store = rolesBll?.Store
                }
            };

            string body = JsonSerializer.Serialize(payload, JsonOpts);
            req.Content = new StringContent(body, Encoding.UTF8, "application/json");

            using var res = await Http.SendAsync(req);
            if (res.IsSuccessStatusCode)
            {
                RpcDirectoActivo = true;
                return (true, "✔ Cambios aplicados directamente en Supabase Cloud.");
            }

            string err = await res.Content.ReadAsStringAsync();
            if ((int)res.StatusCode == 404 || err.Contains("admin_rpc_sincronizar_estado", StringComparison.OrdinalIgnoreCase))
            {
                return (false, "⚠ Falta ejecutar una sola vez el script 'Activar_Escritura_Directa_Supabase.sql' en el SQL Editor de Supabase para activar el guardado directo sin clave secreta.");
            }

            return (false, $"⚠ Supabase rechazó la operación (HTTP {(int)res.StatusCode}): {err}");
        }
        catch (Exception ex)
        {
            return (false, $"⚠ No se pudo conectar con Supabase Cloud: {ex.Message}");
        }
    }

    private void GuardarEspejoLocalYSincronizarWeb(BaseDatosCompletaBE db)
    {
        BaseDatosCompletaBE paraDisco = db;

        if ((db.TotalPaisesEnNube.HasValue && db.Paises.Count == 0) ||
            (db.TotalNcmEnNube.HasValue && db.NcmCatalogo.Count == 0))
        {
            BaseDatosCompletaBE? previo = null;
            if (File.Exists(LocalDbFilePath))
            {
                try
                {
                    previo = JsonSerializer.Deserialize<BaseDatosCompletaBE>(File.ReadAllText(LocalDbFilePath, Encoding.UTF8), JsonOpts);
                }
                catch { }
            }
            var semillaRespaldo = previo != null && previo.Paises.Count > 0 && previo.NcmCatalogo.Count > 0
                ? previo
                : CrearSemillaInicial();

            paraDisco = new BaseDatosCompletaBE
            {
                AdminUsuarios = db.AdminUsuarios,
                Forwarders = db.Forwarders,
                Despachantes = db.Despachantes,
                PuertosAduanas = db.PuertosAduanas,
                Paises = db.Paises.Count > 0 ? db.Paises : semillaRespaldo.Paises,
                NcmCatalogo = db.NcmCatalogo.Count > 0 ? db.NcmCatalogo : semillaRespaldo.NcmCatalogo,
                DigitosVerificadores = db.DigitosVerificadores
                    .Where(d => !string.Equals(d.TablaNombre, "paises", StringComparison.OrdinalIgnoreCase) &&
                                !string.Equals(d.TablaNombre, "ncm_catalogo", StringComparison.OrdinalIgnoreCase))
                    .ToList()
            };

            foreach (var p in paraDisco.Paises) p.Dvh = SeguridadYDigitoVerificadorBLL.CalcularDvhPais(p);
            foreach (var n in paraDisco.NcmCatalogo) n.Dvh = SeguridadYDigitoVerificadorBLL.CalcularDvhNcm(n);

            var ahora = DateTimeOffset.UtcNow;
            paraDisco.DigitosVerificadores.Add(new DigitoVerificadorBE
            {
                TablaNombre = "paises",
                Dvv = SeguridadYDigitoVerificadorBLL.CalcularDvvDesdeDvhs(paraDisco.Paises.Select(x => x.Dvh)),
                CantidadRegistros = paraDisco.Paises.Count,
                UpdatedAt = ahora,
                UpdatedBy = "LOCAL_MIRROR",
                SnapshotControl = paraDisco.Paises.OrderBy(x => x.CodigoIso).Select(p => new SnapshotFilaBE
                {
                    Pk = p.CodigoIso.ToUpperInvariant(),
                    Dvh = p.Dvh,
                    Campos = new Dictionary<string, object?>
                    {
                        ["nombre"] = p.Nombre,
                        ["es_mercosur"] = p.EsMercosur.ToString().ToLowerInvariant(),
                        ["flete_estimado_pct"] = p.FleteEstimadoPct.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture)
                    }
                }).ToList()
            });

            paraDisco.DigitosVerificadores.Add(new DigitoVerificadorBE
            {
                TablaNombre = "ncm_catalogo",
                Dvv = SeguridadYDigitoVerificadorBLL.CalcularDvvDesdeDvhs(paraDisco.NcmCatalogo.Select(x => x.Dvh)),
                CantidadRegistros = paraDisco.NcmCatalogo.Count,
                UpdatedAt = ahora,
                UpdatedBy = "LOCAL_MIRROR",
                SnapshotControl = paraDisco.NcmCatalogo.OrderBy(x => x.CodigoNcm).Select(n => new SnapshotFilaBE
                {
                    Pk = n.CodigoNcm,
                    Dvh = n.Dvh,
                    Campos = new Dictionary<string, object?>
                    {
                        ["descripcion"] = n.Descripcion.Length > 80 ? n.Descripcion[..80] : n.Descripcion,
                        ["tipo_bien"] = n.TipoBien,
                        ["aec"] = n.Aec.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture),
                        ["iva_alicuota"] = n.IvaAlicuota.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture)
                    }
                }).ToList()
            });
        }

        File.WriteAllText(LocalDbFilePath, JsonSerializer.Serialize(paraDisco, JsonOpts), Encoding.UTF8);

        string webRoot = ResolverRutaWebWorkspace();
        if (string.IsNullOrWhiteSpace(webRoot)) return;

        try
        {
            string dalDbFolder = Path.Combine(webRoot, "DAL", "database");
            Directory.CreateDirectory(dalDbFolder);

            var payloadWeb = new
            {
                updatedAt = DateTimeOffset.UtcNow.ToString("O"),
                forwarders = db.Forwarders
                    .Where(f => f.Activo)
                    .OrderBy(f => f.Orden)
                    .Select(f => new
                    {
                        id = f.Id.ToString(),
                        nombre = f.Nombre.Length > 120 ? f.Nombre[..120] : f.Nombre,
                        imagen_url = f.ImagenUrl,
                        whatsapp = f.Whatsapp,
                        email = f.Email,
                        orden = f.Orden,
                        activo = f.Activo
                    })
                    .ToList(),
                despachantes = db.Despachantes
                    .Where(d => d.Activo)
                    .OrderBy(d => d.Orden)
                    .Select(d => new
                    {
                        id = d.Id.ToString(),
                        nombre = d.Nombre.Length > 120 ? d.Nombre[..120] : d.Nombre,
                        imagen_url = d.ImagenUrl,
                        whatsapp = d.Whatsapp,
                        email = d.Email,
                        orden = d.Orden,
                        activo = d.Activo
                    })
                    .ToList(),
                puertos = db.PuertosAduanas
                    .OrderBy(p => p.Nombre)
                    .Select(p => new
                    {
                        id = p.Id.ToString(),
                        nombre = p.Nombre,
                        via_ingreso = p.ViaIngreso,
                        provincia = p.Provincia,
                        codigo_sim = p.CodigoSim,
                        operador = p.Operador,
                        infraestructura = p.Infraestructura,
                        latitud = p.Latitud ?? 0m,
                        longitud = p.Longitud ?? 0m
                    })
                    .ToList()
            };

            string jsonSync = JsonSerializer.Serialize(payloadWeb, JsonOpts);
            File.WriteAllText(Path.Combine(dalDbFolder, "web_sync.json"), jsonSync, Utf8SinBom);
            File.WriteAllText(Path.Combine(dalDbFolder, "web_sync.js"), $"window.__IMPORTA_ADMIN_SYNC__ = {jsonSync};\r\n", Utf8SinBom);
        }
        catch { }
    }

    // --- CONECTIVIDAD SUPABASE REST + STORAGE API (HTTPS PUERTO 443) ---

    public async Task<(bool Ok, string Mensaje)> ProbarConexionSupabaseAsync()
    {
        if (!Config.EstaConfiguradoEnNube && TieneSesionRpcActiva)
        {
            var rpcTest = await IntentarAutenticarYObtenerEstadoRpcAsync(_rpcUsername, _rpcPassword);
            if (rpcTest.RpcDisponible && rpcTest.Status == "OK")
            {
                return (true, "✔ Conexión directa autenticada con Supabase Cloud activa (sin necesidad de ingresar Secret Key).");
            }

            return (false, "⚠ Falta ejecutar por única vez el script 'Activar_Escritura_Directa_Supabase.sql' en el SQL Editor de Supabase.");
        }

        if (!Config.EstaConfiguradoEnNube)
        {
            return (false, "Iniciá sesión nuevamente o ejecutá 'Activar_Escritura_Directa_Supabase.sql' en Supabase SQL Editor.");
        }

        try
        {
            using var req = CrearRequestSupabase(HttpMethod.Get, "/rest/v1/admin_usuarios?select=id&limit=1");
            using var res = await Http.SendAsync(req);
            if (res.IsSuccessStatusCode)
                return (true, "✔ Conexión directa autenticada con Supabase Cloud activa.");

            string err = await res.Content.ReadAsStringAsync();
            return (false, $"Supabase rechazó la conexión con HTTP {(int)res.StatusCode}: {err}");
        }
        catch (Exception ex)
        {
            return (false, $"Error de red conectando a Supabase: {ex.Message}");
        }
    }

    private HttpRequestMessage CrearRequestSupabase(HttpMethod method, string relativePath)
    {
        var req = new HttpRequestMessage(method, $"{Config.SupabaseUrl.TrimEnd('/')}{relativePath}");
        req.Headers.Add("apikey", Config.ServiceRoleKey);
        if (Config.ServiceRoleKey.StartsWith("eyJ", StringComparison.Ordinal))
        {
            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", Config.ServiceRoleKey);
        }
        req.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        return req;
    }

    private async Task<List<T>?> GetTablaSupabaseAsync<T>(string endpointQuery)
    {
        var acumulado = new List<T>();
        const int pageSize = 1000;
        int offset = 0;

        while (true)
        {
            using var req = CrearRequestSupabase(HttpMethod.Get, endpointQuery);
            req.Headers.TryAddWithoutValidation("Range", $"{offset}-{offset + pageSize - 1}");
            req.Headers.TryAddWithoutValidation("Range-Unit", "items");
            using var res = await Http.SendAsync(req);
            if (!res.IsSuccessStatusCode)
            {
                return offset == 0 ? null : acumulado;
            }

            string raw = await res.Content.ReadAsStringAsync();
            var lote = JsonSerializer.Deserialize<List<T>>(raw, JsonOpts);
            if (lote == null || lote.Count == 0) break;

            acumulado.AddRange(lote);
            if (lote.Count < pageSize) break;
            offset += pageSize;
        }

        return acumulado;
    }

    private async Task<int?> ObtenerConteoExactoTablaSupabaseAsync(string tabla)
    {
        try
        {
            using var req = CrearRequestSupabase(HttpMethod.Head, $"/rest/v1/{tabla}?select=*");
            req.Headers.Add("Prefer", "count=exact");
            using var res = await Http.SendAsync(req);
            if (!res.IsSuccessStatusCode) return null;

            if (res.Content.Headers.TryGetValues("Content-Range", out var values))
            {
                string range = values.FirstOrDefault() ?? string.Empty;
                int slash = range.LastIndexOf('/');
                if (slash >= 0 && int.TryParse(range[(slash + 1)..], out int total))
                    return total;
            }
            return null;
        }
        catch
        {
            return null;
        }
    }

    public async Task<(bool Ok, string Detalle)> RecalcularDigitosEnSupabaseRpcAsync(string usuario)
    {
        if (!Config.EstaConfiguradoEnNube)
            return (false, "Supabase no configurado.");

        try
        {
            using var req = CrearRequestSupabase(HttpMethod.Post, "/rest/v1/rpc/recalcular_digitos_verificadores");
            string json = JsonSerializer.Serialize(new { p_usuario = usuario }, JsonOpts);
            req.Content = new StringContent(json, Encoding.UTF8, "application/json");
            using var res = await Http.SendAsync(req);
            if (res.IsSuccessStatusCode)
                return (true, "DVH y DVV recalculados en Supabase PostgreSQL sobre la totalidad de los registros.");

            string err = await res.Content.ReadAsStringAsync();
            return (false, $"RPC DV respondió HTTP {(int)res.StatusCode}: {err}");
        }
        catch (Exception ex)
        {
            return (false, $"Error invocando RPC DV en Supabase: {ex.Message}");
        }
    }

    private sealed class ParametroSistemaRow
    {
        public string clave { get; set; } = string.Empty;
        public string valor { get; set; } = string.Empty;
        public string? descripcion { get; set; }
    }

    private sealed class AdminSeguridadRbacRow
    {
        public string clave { get; set; } = string.Empty;
        public RolesStoreBE? payload { get; set; }
    }

    private async Task<BaseDatosCompletaBE?> IntentarLeerDesdeSupabaseAsync(RolesCompositeBLL? rolesBll = null)
    {
        try
        {
            var adminsTask = GetTablaSupabaseAsync<AdminUsuarioBE>("/rest/v1/admin_usuarios?select=*&order=username.asc");
            var fwdsTask = GetTablaSupabaseAsync<ForwarderBE>("/rest/v1/forwarders?select=*&order=orden.asc");
            var despsTask = GetTablaSupabaseAsync<DespachanteBE>("/rest/v1/despachantes?select=*&order=orden.asc");
            var paramDespTask = GetTablaSupabaseAsync<ParametroSistemaRow>($"/rest/v1/parametros_sistema?select=clave,valor,descripcion&clave=eq.{ClaveParametroDespachantes}&limit=1");
            var puertosTask = GetTablaSupabaseAsync<PuertoAduanaBE>("/rest/v1/puertos_aduanas?select=*&order=nombre.asc");

            var dvvConSnapTask = GetTablaSupabaseAsync<DigitoVerificadorBE>(
                "/rest/v1/digito_verificador?select=*&tabla_nombre=in.(admin_usuarios,forwarders,despachantes,puertos_aduanas)");
            var dvvLivianoTask = GetTablaSupabaseAsync<DigitoVerificadorBE>(
                "/rest/v1/digito_verificador?select=tabla_nombre,dvv,cantidad_registros,updated_at,updated_by&tabla_nombre=in.(paises,ncm_catalogo)");

            var countPaisesTask = ObtenerConteoExactoTablaSupabaseAsync("paises");
            var countNcmTask = ObtenerConteoExactoTablaSupabaseAsync("ncm_catalogo");

            Task<List<AdminSeguridadRbacRow>?>? rbacTask = rolesBll != null
                ? GetTablaSupabaseAsync<AdminSeguridadRbacRow>("/rest/v1/admin_seguridad_rbac?select=clave,payload&clave=eq.ROLES_STORE_V2&limit=1")
                : null;

            await Task.WhenAll(adminsTask, fwdsTask, despsTask, paramDespTask, puertosTask, dvvConSnapTask, dvvLivianoTask, countPaisesTask, countNcmTask);
            if (rbacTask != null)
            {
                var rbacRows = await rbacTask;
                if (rbacRows != null && rbacRows.Count > 0 && rbacRows[0].payload != null)
                {
                    rolesBll!.ReemplazarStoreDesdeNube(rbacRows[0].payload!);
                }
            }

            if (adminsTask.Result == null || fwdsTask.Result == null)
                return null;

            List<DespachanteBE> despachantesFinales;
            if (despsTask.Result != null)
            {
                despachantesFinales = despsTask.Result;
            }
            else if (paramDespTask.Result != null && paramDespTask.Result.Count > 0 && !string.IsNullOrWhiteSpace(paramDespTask.Result[0].descripcion))
            {
                despachantesFinales = JsonSerializer.Deserialize<List<DespachanteBE>>(paramDespTask.Result[0].descripcion!, JsonOpts)
                    ?? CrearSemillaDespachantes();
            }
            else
            {
                despachantesFinales = CrearSemillaDespachantes();
            }

            var dvvs = new List<DigitoVerificadorBE>();
            if (dvvConSnapTask.Result != null) dvvs.AddRange(dvvConSnapTask.Result);
            if (dvvLivianoTask.Result != null) dvvs.AddRange(dvvLivianoTask.Result);

            return new BaseDatosCompletaBE
            {
                AdminUsuarios = adminsTask.Result,
                Forwarders = fwdsTask.Result,
                Despachantes = despachantesFinales,
                PuertosAduanas = puertosTask.Result ?? new(),
                Paises = new(),
                NcmCatalogo = new(),
                DigitosVerificadores = dvvs,
                TotalPaisesEnNube = countPaisesTask.Result,
                TotalNcmEnNube = countNcmTask.Result
            };
        }
        catch
        {
            return null;
        }
    }

    private async Task<(bool Ok, string Detalle)> SincronizarHaciaSupabaseAsync(
        BaseDatosCompletaBE db,
        bool limpiarFilasHuerfanas,
        RolesCompositeBLL? rolesBll = null)
    {
        try
        {
            // 1. Purgar en Supabase cualquier Forwarder, Puerto o Usuario eliminado localmente para que otras PCs / GitHub NUNCA vean registros borrados
            await PurgarEliminadosEnTablaUuidAsync("forwarders", db.Forwarders.Select(f => f.Id));
            await PurgarEliminadosEnTablaUuidAsync("puertos_aduanas", db.PuertosAduanas.Select(p => p.Id));
            await PurgarEliminadosEnTablaUuidAsync("admin_usuarios", db.AdminUsuarios.Select(u => u.Id));

            if (limpiarFilasHuerfanas)
            {
                if (db.Paises.Count > 0) await BorrarTodosSupabaseAsync("paises", "codigo_iso=not.is.null");
                if (db.NcmCatalogo.Count > 0) await BorrarTodosSupabaseAsync("ncm_catalogo", "codigo_ncm=not.is.null");
            }

            // 2. Sincronizar admin_usuarios (con fallback automático si aún no se corrió ALTER TABLE en Supabase)
            await UpsertAdminUsuariosCompatibleAsync(db.AdminUsuarios);

            // 3. Sincronizar Forwarders y Puertos
            await UpsertSupabaseAsync("forwarders?on_conflict=id", db.Forwarders);
            await UpsertSupabaseAsync("puertos_aduanas?on_conflict=id", db.PuertosAduanas);

            // 4. Sincronizar Despachantes de Aduana (en tabla public.despachantes si existe + espejo en parametros_sistema)
            bool tablaDespachantesExiste = await IntentarSincronizarTablaDespachantesAsync(db.Despachantes);
            await SincronizarEspejoDespachantesParametroAsync(db.Despachantes);

            if (db.Paises.Count > 0) await UpsertSupabaseAsync("paises?on_conflict=codigo_iso", db.Paises);
            if (db.NcmCatalogo.Count > 0) await UpsertSupabaseAsync("ncm_catalogo?on_conflict=codigo_ncm", db.NcmCatalogo);

            if (rolesBll != null)
            {
                await IntentarSincronizarRbacStoreAsync(rolesBll.Store);
            }

            var rpcRes = await RecalcularDigitosEnSupabaseRpcAsync("ADMIN_EXE");
            if (!rpcRes.Ok)
            {
                var dvParaSubir = tablaDespachantesExiste
                    ? db.DigitosVerificadores
                    : db.DigitosVerificadores.Where(d => !string.Equals(d.TablaNombre, "despachantes", StringComparison.OrdinalIgnoreCase)).ToList();
                await UpsertSupabaseAsync("digito_verificador?on_conflict=tabla_nombre", dvParaSubir);
            }

            return (true, "✔ Sincronizado en tiempo real con Supabase Cloud y espejo Web.");
        }
        catch (Exception ex)
        {
            return (false, $"⚠ Guardado localmente, pero falló la sincronización con Supabase Cloud: {ex.Message}");
        }
    }

    private async Task UpsertAdminUsuariosCompatibleAsync(List<AdminUsuarioBE> usuarios)
    {
        if (usuarios.Count == 0) return;

        try
        {
            await UpsertSupabaseAsync("admin_usuarios?on_conflict=username", usuarios);
        }
        catch
        {
            // Fallback si public.admin_usuarios aún no tiene las columnas extendidas (dni, nombre, apellido, bloqueado, perfil_id)
            var core = usuarios.Select(u => new
            {
                id = u.Id,
                username = u.Username,
                password_hash = u.PasswordHash,
                activo = u.Activo,
                ultimo_acceso = u.UltimoAcceso,
                password_updated_at = u.PasswordUpdatedAt,
                dvh = u.Dvh
            }).ToList();

            await UpsertSupabaseAsync("admin_usuarios?on_conflict=username", core);
        }
    }

    private async Task<bool> IntentarSincronizarTablaDespachantesAsync(List<DespachanteBE> despachantes)
    {
        try
        {
            await PurgarEliminadosEnTablaUuidAsync("despachantes", despachantes.Select(d => d.Id));
            await UpsertSupabaseAsync("despachantes?on_conflict=id", despachantes);
            return true;
        }
        catch
        {
            return false;
        }
    }

    private async Task SincronizarEspejoDespachantesParametroAsync(List<DespachanteBE> despachantes)
    {
        var dto = new List<object>
        {
            new
            {
                clave = ClaveParametroDespachantes,
                valor = $"SYNC_{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}",
                descripcion = JsonSerializer.Serialize(despachantes, JsonOpts),
                updated_at = DateTimeOffset.UtcNow
            }
        };
        await UpsertSupabaseAsync("parametros_sistema?on_conflict=clave", dto);
    }

    public async Task IntentarSincronizarRbacStoreAsync(RolesStoreBE store)
    {
        if (Config.EstaConfiguradoEnNube)
        {
            try
            {
                var payload = new List<object>
                {
                    new
                    {
                        clave = "ROLES_STORE_V2",
                        payload = store,
                        updated_at = DateTimeOffset.UtcNow
                    }
                };
                await UpsertSupabaseAsync("admin_seguridad_rbac?on_conflict=clave", payload);
            }
            catch { }
            return;
        }

        if (TieneSesionRpcActiva)
        {
            try
            {
                using var req = CrearRequestPublishableRpc("/rest/v1/rpc/admin_rpc_sincronizar_estado");
                string body = JsonSerializer.Serialize(new
                {
                    p_username = _rpcUsername,
                    p_password = _rpcPassword,
                    p_payload = new { rbac_store = store }
                }, JsonOpts);
                req.Content = new StringContent(body, Encoding.UTF8, "application/json");
                using var res = await Http.SendAsync(req);
            }
            catch { }
        }
    }

    private async Task PurgarEliminadosEnTablaUuidAsync(string tabla, IEnumerable<Guid> idsVigentes)
    {
        var lista = idsVigentes.Distinct().ToList();
        string filtro = lista.Count == 0
            ? "id=not.is.null"
            : $"id=not.in.({string.Join(",", lista.Select(g => g.ToString()))})";

        await BorrarTodosSupabaseAsync(tabla, filtro);
    }

    public async Task EliminarRegistroSupabaseAsync(string tabla, string filtroQuery)
    {
        if (!Config.EstaConfiguradoEnNube) return;
        await BorrarTodosSupabaseAsync(tabla, filtroQuery);
    }

    private async Task BorrarTodosSupabaseAsync(string tabla, string filtro)
    {
        using var req = CrearRequestSupabase(HttpMethod.Delete, $"/rest/v1/{tabla}?{filtro}");
        using var res = await Http.SendAsync(req);
        res.EnsureSuccessStatusCode();
    }

    private async Task UpsertSupabaseAsync<T>(string tablaConConflict, List<T> items)
    {
        if (items.Count == 0) return;
        const int batchSize = 500;
        for (int i = 0; i < items.Count; i += batchSize)
        {
            var lote = items.Skip(i).Take(batchSize).ToList();
            using var req = CrearRequestSupabase(HttpMethod.Post, $"/rest/v1/{tablaConConflict}");
            req.Headers.Add("Prefer", "resolution=merge-duplicates,return=minimal");
            string json = JsonSerializer.Serialize(lote, JsonOpts);
            req.Content = new StringContent(json, Encoding.UTF8, "application/json");
            using var res = await Http.SendAsync(req);
            res.EnsureSuccessStatusCode();
        }
    }

    public Task<string> ProcesarYSubirImagenForwarderAsync(string rutaImagenOrigen, Guid forwarderId) =>
        ProcesarYSubirImagenOperadorAsync(rutaImagenOrigen, forwarderId, "fwd");

    public Task<string> ProcesarYSubirImagenDespachanteAsync(string rutaImagenOrigen, Guid despachanteId) =>
        ProcesarYSubirImagenOperadorAsync(rutaImagenOrigen, despachanteId, "desp");

    private async Task<string> ProcesarYSubirImagenOperadorAsync(string rutaImagenOrigen, Guid operadorId, string prefijo)
    {
        byte[] jpegOptimizado = OptimizarImagenCuadradaJpeg(rutaImagenOrigen, 480, 82L);
        string nombreArchivo = $"{prefijo}_{operadorId:N}_{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}.jpg";

        if (Config.EstaConfiguradoEnNube)
        {
            try
            {
                using var req = CrearRequestSupabase(HttpMethod.Post, $"/storage/v1/object/forwarders/{nombreArchivo}");
                req.Headers.Add("x-upsert", "true");
                req.Headers.Add("Cache-Control", "max-age=31536000");
                req.Content = new ByteArrayContent(jpegOptimizado);
                req.Content.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");

                using var res = await Http.SendAsync(req);
                if (res.IsSuccessStatusCode)
                {
                    return $"{Config.SupabaseUrl.TrimEnd('/')}/storage/v1/object/public/forwarders/{nombreArchivo}";
                }
            }
            catch { }
        }

        string webRoot = ResolverRutaWebWorkspace();
        if (!string.IsNullOrWhiteSpace(webRoot))
        {
            try
            {
                string uploadsDir = Path.Combine(webRoot, "public", "uploads", "forwarders");
                Directory.CreateDirectory(uploadsDir);
                await File.WriteAllBytesAsync(Path.Combine(uploadsDir, nombreArchivo), jpegOptimizado);
            }
            catch { }
        }

        return $"data:image/jpeg;base64,{Convert.ToBase64String(jpegOptimizado)}";
    }

    private static byte[] OptimizarImagenCuadradaJpeg(string rutaImagen, int ladoMaxPx, long calidadJpeg)
    {
        using var original = Image.FromFile(rutaImagen);
        int ladoOrigen = Math.Min(original.Width, original.Height);
        int cropX = (original.Width - ladoOrigen) / 2;
        int cropY = (original.Height - ladoOrigen) / 2;

        int ladoFinal = Math.Min(ladoMaxPx, ladoOrigen);
        using var destino = new Bitmap(ladoFinal, ladoFinal);
        using (var g = Graphics.FromImage(destino))
        {
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.SmoothingMode = SmoothingMode.HighQuality;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            g.DrawImage(
                original,
                new Rectangle(0, 0, ladoFinal, ladoFinal),
                new Rectangle(cropX, cropY, ladoOrigen, ladoOrigen),
                GraphicsUnit.Pixel);
        }

        var encoder = ImageCodecInfo.GetImageEncoders().First(c => c.FormatID == ImageFormat.Jpeg.Guid);
        using var encoderParams = new EncoderParameters(1);
        encoderParams.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, calidadJpeg);

        using var ms = new MemoryStream();
        destino.Save(ms, encoder, encoderParams);
        return ms.ToArray();
    }
}
