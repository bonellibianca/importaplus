using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Threading.Tasks;
using AdminImportaPlus.BE;
using AdminImportaPlus.BLL;

namespace AdminImportaPlus.DAL;

/// <summary>
/// Capa DAL (.NET 9): Persistencia Dual sobre Supabase PostgreSQL + Supabase Storage (HTTPS REST API)
/// con espejo local automático hacia la carpeta de la Web para pruebas offline y ahorro de cuota FREE.
/// </summary>
public sealed class RepositorioDatosDAL
{
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

    private string ResolverCarpetaDatosProyecto()
    {
        // Buscar la raíz del proyecto AdminImportaPlus o BB para persistir fuera de bin/Debug si existe
        var dir = new DirectoryInfo(AppBaseDir);
        while (dir != null)
        {
            if (File.Exists(Path.Combine(dir.FullName, "Base_ImportaPlus.sql")))
            {
                string adminFolder = Path.Combine(dir.FullName, "AdminImportaPlus", "Data");
                return adminFolder;
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
        if (File.Exists(ConfigFilePath))
        {
            try
            {
                cfg = JsonSerializer.Deserialize<SupabaseConfigBE>(File.ReadAllText(ConfigFilePath, Encoding.UTF8), JsonOpts) ?? new();
            }
            catch { }
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

        // Autodetectar credenciales desde .env.local de la Web si aún no se cargaron en el .exe
        if (!cfg.EstaConfiguradoEnNube && !string.IsNullOrWhiteSpace(cfg.WebWorkspacePath))
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

                    if (k == "NEXT_PUBLIC_SUPABASE_URL" && string.IsNullOrWhiteSpace(cfg.SupabaseUrl)) cfg.SupabaseUrl = v;
                    else if (k == "SUPABASE_SERVICE_ROLE_KEY" && string.IsNullOrWhiteSpace(cfg.ServiceRoleKey)) cfg.ServiceRoleKey = v;
                    else if (k == "NEXT_PUBLIC_SUPABASE_ANON_KEY" && string.IsNullOrWhiteSpace(cfg.AnonKey)) cfg.AnonKey = v;
                }
            }
        }

        return cfg;
    }

    public void GuardarConfiguracion(SupabaseConfigBE nuevaConfig)
    {
        nuevaConfig.SupabaseUrl = (nuevaConfig.SupabaseUrl ?? string.Empty).Trim().TrimEnd('/');
        nuevaConfig.ServiceRoleKey = (nuevaConfig.ServiceRoleKey ?? string.Empty).Trim();
        nuevaConfig.AnonKey = (nuevaConfig.AnonKey ?? string.Empty).Trim();
        Config = nuevaConfig;
        File.WriteAllText(ConfigFilePath, JsonSerializer.Serialize(Config, JsonOpts), Encoding.UTF8);

        // Si existe la carpeta Web y las claves son reales, sincronizar .env.local automáticamente
        string webRoot = ResolverRutaWebWorkspace();
        if (!string.IsNullOrWhiteSpace(webRoot) && Config.EstaConfiguradoEnNube)
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
            sb.AppendLine($"SUPABASE_SERVICE_ROLE_KEY={Config.ServiceRoleKey}");
            sb.AppendLine("NEXT_PUBLIC_APP_ORIGIN=https://importaplus.com.ar");
            File.WriteAllText(envLocalPath, sb.ToString(), Encoding.UTF8);
        }
        catch { }
    }

    /// <summary>
    /// Obtiene el estado actual completo de la base de datos (desde Supabase Cloud si está configurado,
    /// o desde el almacén persistente local inicializado con Admin/1234 y los 4 Forwarders).
    /// </summary>
    public async Task<BaseDatosCompletaBE> ObtenerBaseDatosActualAsync()
    {
        if (Config.EstaConfiguradoEnNube)
        {
            var remota = await IntentarLeerDesdeSupabaseAsync();
            if (remota != null)
            {
                GuardarEspejoLocalYSincronizarWeb(remota);
                return remota;
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
                    return local;
                }
            }
            catch { }
        }

        var semilla = CrearSemillaInicial();
        await GuardarBaseDatosCompletaAsync(semilla, sincronizarNube: Config.EstaConfiguradoEnNube);
        return semilla;
    }

    public BaseDatosCompletaBE CrearSemillaInicial()
    {
        var db = new BaseDatosCompletaBE
        {
            AdminUsuarios = new List<AdminUsuarioBE>
            {
                new()
                {
                    Id = Guid.Parse("99999999-9999-9999-9999-999999999999"),
                    Username = "Admin",
                    PasswordHash = SeguridadYDigitoVerificadorBLL.HashearPasswordBcrypt("1234"),
                    Activo = true,
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
        bool reemplazarTablasEnRestore = false)
    {
        GuardarEspejoLocalYSincronizarWeb(db);

        if (!sincronizarNube || !Config.EstaConfiguradoEnNube)
        {
            return (true, "Guardado en base local y sincronizado con la Web.");
        }

        return await SincronizarHaciaSupabaseAsync(db, reemplazarTablasEnRestore);
    }

    private void GuardarEspejoLocalYSincronizarWeb(BaseDatosCompletaBE db)
    {
        File.WriteAllText(LocalDbFilePath, JsonSerializer.Serialize(db, JsonOpts), Encoding.UTF8);

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
                    .ToList()
            };

            string jsonSync = JsonSerializer.Serialize(payloadWeb, JsonOpts);
            File.WriteAllText(Path.Combine(dalDbFolder, "web_sync.json"), jsonSync, Encoding.UTF8);
            File.WriteAllText(Path.Combine(dalDbFolder, "web_sync.js"), $"window.__IMPORTA_ADMIN_SYNC__ = {jsonSync};\r\n", Encoding.UTF8);
        }
        catch { }
    }

    // --- CONECTIVIDAD SUPABASE REST + STORAGE API (HTTPS PUERTO 443) ---

    public async Task<(bool Ok, string Mensaje)> ProbarConexionSupabaseAsync()
    {
        if (!Config.EstaConfiguradoEnNube)
            return (false, "Ingresá una Project URL válida (https://...) y la Service Role Key.");

        try
        {
            using var req = CrearRequestSupabase(HttpMethod.Get, "/rest/v1/forwarders?select=id&limit=1");
            using var res = await Http.SendAsync(req);
            if (res.IsSuccessStatusCode)
                return (true, "Conexión exitosa con Supabase PostgreSQL (API REST autenticada con Service Role).");

            string err = await res.Content.ReadAsStringAsync();
            return (false, $"Supabase respondió HTTP {(int)res.StatusCode}: {err}");
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
        if (!Config.ServiceRoleKey.StartsWith("sb_secret_", StringComparison.Ordinal))
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

    private async Task<BaseDatosCompletaBE?> IntentarLeerDesdeSupabaseAsync()
    {
        try
        {
            var adminsTask = GetTablaSupabaseAsync<AdminUsuarioBE>("/rest/v1/admin_usuarios?select=*&order=username.asc");
            var fwdsTask = GetTablaSupabaseAsync<ForwarderBE>("/rest/v1/forwarders?select=*&order=orden.asc");
            var puertosTask = GetTablaSupabaseAsync<PuertoAduanaBE>("/rest/v1/puertos_aduanas?select=*&order=nombre.asc");

            // Leer DVV con snapshot solo para tablas administrables en desktop y metadata liviana para paises/ncm_catalogo (10.439 filas)
            var dvvConSnapTask = GetTablaSupabaseAsync<DigitoVerificadorBE>(
                "/rest/v1/digito_verificador?select=*&tabla_nombre=in.(admin_usuarios,forwarders,puertos_aduanas)");
            var dvvLivianoTask = GetTablaSupabaseAsync<DigitoVerificadorBE>(
                "/rest/v1/digito_verificador?select=tabla_nombre,dvv,cantidad_registros,updated_at,updated_by&tabla_nombre=in.(paises,ncm_catalogo)");

            var countPaisesTask = ObtenerConteoExactoTablaSupabaseAsync("paises");
            var countNcmTask = ObtenerConteoExactoTablaSupabaseAsync("ncm_catalogo");

            await Task.WhenAll(adminsTask, fwdsTask, puertosTask, dvvConSnapTask, dvvLivianoTask, countPaisesTask, countNcmTask);

            if (adminsTask.Result == null || fwdsTask.Result == null)
                return null;

            var dvvs = new List<DigitoVerificadorBE>();
            if (dvvConSnapTask.Result != null) dvvs.AddRange(dvvConSnapTask.Result);
            if (dvvLivianoTask.Result != null) dvvs.AddRange(dvvLivianoTask.Result);

            return new BaseDatosCompletaBE
            {
                AdminUsuarios = adminsTask.Result,
                Forwarders = fwdsTask.Result,
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

    private async Task<(bool Ok, string Detalle)> SincronizarHaciaSupabaseAsync(BaseDatosCompletaBE db, bool limpiarFilasHuerfanas)
    {
        try
        {
            if (limpiarFilasHuerfanas)
            {
                await BorrarTodosSupabaseAsync("forwarders", "id=not.is.null");
                await BorrarTodosSupabaseAsync("puertos_aduanas", "id=not.is.null");
                await BorrarTodosSupabaseAsync("admin_usuarios", "id=not.is.null");
                if (db.Paises.Count > 0) await BorrarTodosSupabaseAsync("paises", "codigo_iso=not.is.null");
                if (db.NcmCatalogo.Count > 0) await BorrarTodosSupabaseAsync("ncm_catalogo", "codigo_ncm=not.is.null");
            }

            await UpsertSupabaseAsync("admin_usuarios?on_conflict=username", db.AdminUsuarios);
            await UpsertSupabaseAsync("forwarders?on_conflict=id", db.Forwarders);
            await UpsertSupabaseAsync("puertos_aduanas?on_conflict=id", db.PuertosAduanas);
            if (db.Paises.Count > 0) await UpsertSupabaseAsync("paises?on_conflict=codigo_iso", db.Paises);
            if (db.NcmCatalogo.Count > 0) await UpsertSupabaseAsync("ncm_catalogo?on_conflict=codigo_ncm", db.NcmCatalogo);

            // Actualizar DVH/DVV en PostgreSQL para incluir también los 196 países y 10.439 NCM
            var rpcRes = await RecalcularDigitosEnSupabaseRpcAsync("ADMIN_EXE");
            if (!rpcRes.Ok)
            {
                await UpsertSupabaseAsync("digito_verificador?on_conflict=tabla_nombre", db.DigitosVerificadores);
            }

            return (true, "Sincronizado correctamente con Supabase Cloud y espejo Web.");
        }
        catch (Exception ex)
        {
            return (false, $"Guardado localmente, pero falló sincronización Supabase: {ex.Message}");
        }
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

    /// <summary>
    /// Optimiza la imagen seleccionada por el Admin (recorte cuadrado máx 480x480 JPEG ~35 KB)
    /// para cuidar la cuota de Supabase FREE, subiéndola al Bucket 'forwarders' si está en nube
    /// o generando URL pública local / Data URI compacta.
    /// </summary>
    public async Task<string> ProcesarYSubirImagenForwarderAsync(string rutaImagenOrigen, Guid forwarderId)
    {
        byte[] jpegOptimizado = OptimizarImagenCuadradaJpeg(rutaImagenOrigen, 480, 82L);
        string nombreArchivo = $"fwd_{forwarderId:N}_{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}.jpg";

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

        // Guardar también en public/uploads/forwarders del proyecto Web si existe y retornar Data URI compacta (< 40 KB)
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
