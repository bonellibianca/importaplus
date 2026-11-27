using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace AdminImportaPlus.BE;

public sealed class SupabaseConfigBE
{
    public const string DefaultProjectUrl = "https://exqyupuliqxsxejgouho.supabase.co";
    public const string DefaultPublishableKey = "sb_publishable_HPs_CdVHnpT1PqyRUwfz9Q_g5rBulj_";
    public const string DefaultServiceRoleKey = ""; // Opcional: se usa conexión RPC directa con Publishable Key + sesión autenticada

    public string SupabaseUrl { get; set; } = DefaultProjectUrl;
    public string ServiceRoleKey { get; set; } = DefaultServiceRoleKey;
    public string AnonKey { get; set; } = DefaultPublishableKey;
    public string WebWorkspacePath { get; set; } = string.Empty;

    [JsonIgnore]
    public bool EsPublishableEnCampoSecreto =>
        !string.IsNullOrWhiteSpace(ServiceRoleKey) &&
        ServiceRoleKey.Trim().StartsWith("sb_publishable_", StringComparison.OrdinalIgnoreCase);

    [JsonIgnore]
    public bool TieneClaveSecretaValida =>
        !string.IsNullOrWhiteSpace(ServiceRoleKey) &&
        !EsPublishableEnCampoSecreto &&
        !ServiceRoleKey.Contains("tu-service-role-key", StringComparison.OrdinalIgnoreCase) &&
        !ServiceRoleKey.EndsWith("...", StringComparison.Ordinal);

    [JsonIgnore]
    public bool EstaConfiguradoEnNube =>
        !string.IsNullOrWhiteSpace(SupabaseUrl) &&
        SupabaseUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase) &&
        !SupabaseUrl.Contains("TU_PROJECT_REF", StringComparison.OrdinalIgnoreCase) &&
        TieneClaveSecretaValida;
}

public sealed class AdminUsuarioBE
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; } = Guid.NewGuid();

    [JsonPropertyName("dni")]
    public int Dni { get; set; } = 11111111;

    [JsonPropertyName("nombre")]
    public string Nombre { get; set; } = "Administrador";

    [JsonPropertyName("apellido")]
    public string Apellido { get; set; } = "General";

    [JsonPropertyName("username")]
    public string Username { get; set; } = "Admin";

    [JsonPropertyName("password_hash")]
    public string PasswordHash { get; set; } = string.Empty;

    [JsonPropertyName("bloqueado")]
    public bool Bloqueado { get; set; } = false;

    [JsonPropertyName("activo")]
    public bool Activo { get; set; } = true;

    [JsonPropertyName("perfil_id")]
    public Guid? PerfilId { get; set; }

    [JsonPropertyName("ultimo_acceso")]
    public DateTimeOffset? UltimoAcceso { get; set; }

    [JsonPropertyName("password_updated_at")]
    public DateTimeOffset PasswordUpdatedAt { get; set; } = DateTimeOffset.UtcNow;

    [JsonPropertyName("dvh")]
    public string Dvh { get; set; } = string.Empty;
}

public sealed class ForwarderBE
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; } = Guid.NewGuid();

    [JsonPropertyName("nombre")]
    public string Nombre { get; set; } = string.Empty; // Máximo 120 caracteres

    [JsonPropertyName("imagen_url")]
    public string ImagenUrl { get; set; } = string.Empty;

    [JsonPropertyName("whatsapp")]
    public string Whatsapp { get; set; } = string.Empty;

    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;

    [JsonPropertyName("orden")]
    public int Orden { get; set; } = 1;

    [JsonPropertyName("activo")]
    public bool Activo { get; set; } = true;

    [JsonPropertyName("dvh")]
    public string Dvh { get; set; } = string.Empty;
}

public sealed class DespachanteBE
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; } = Guid.NewGuid();

    [JsonPropertyName("nombre")]
    public string Nombre { get; set; } = string.Empty; // Máximo 120 caracteres

    [JsonPropertyName("imagen_url")]
    public string ImagenUrl { get; set; } = string.Empty;

    [JsonPropertyName("whatsapp")]
    public string Whatsapp { get; set; } = string.Empty;

    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;

    [JsonPropertyName("orden")]
    public int Orden { get; set; } = 1;

    [JsonPropertyName("activo")]
    public bool Activo { get; set; } = true;

    [JsonPropertyName("dvh")]
    public string Dvh { get; set; } = string.Empty;
}

public sealed class PuertoAduanaBE
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; } = Guid.NewGuid();

    [JsonPropertyName("nombre")]
    public string Nombre { get; set; } = string.Empty;

    [JsonPropertyName("via_ingreso")]
    public string ViaIngreso { get; set; } = "MARITIMA_FLUVIAL";

    [JsonPropertyName("provincia")]
    public string Provincia { get; set; } = string.Empty;

    [JsonPropertyName("codigo_sim")]
    public string CodigoSim { get; set; } = string.Empty;

    [JsonPropertyName("operador")]
    public string Operador { get; set; } = string.Empty;

    [JsonPropertyName("infraestructura")]
    public string Infraestructura { get; set; } = string.Empty;

    [JsonPropertyName("latitud")]
    public decimal? Latitud { get; set; }

    [JsonPropertyName("longitud")]
    public decimal? Longitud { get; set; }

    [JsonPropertyName("dvh")]
    public string Dvh { get; set; } = string.Empty;
}

public sealed class PaisBE
{
    [JsonPropertyName("codigo_iso")]
    public string CodigoIso { get; set; } = string.Empty;

    [JsonPropertyName("nombre")]
    public string Nombre { get; set; } = string.Empty;

    [JsonPropertyName("es_mercosur")]
    public bool EsMercosur { get; set; }

    [JsonPropertyName("flete_estimado_pct")]
    public decimal FleteEstimadoPct { get; set; } = 8.00m;

    [JsonPropertyName("dvh")]
    public string Dvh { get; set; } = string.Empty;
}

public sealed class NcmPosicionBE
{
    [JsonPropertyName("codigo_ncm")]
    public string CodigoNcm { get; set; } = string.Empty;

    [JsonPropertyName("descripcion")]
    public string Descripcion { get; set; } = string.Empty;

    [JsonPropertyName("tipo_bien")]
    public string TipoBien { get; set; } = "GENERAL";

    [JsonPropertyName("aec")]
    public decimal Aec { get; set; }

    [JsonPropertyName("iva_alicuota")]
    public decimal IvaAlicuota { get; set; } = 21.00m;

    [JsonPropertyName("anexo_excepcion")]
    public string? AnexoExcepcion { get; set; }

    [JsonPropertyName("arancel_excepcion")]
    public decimal? ArancelExcepcion { get; set; }

    [JsonPropertyName("condicion_excepcion")]
    public string? CondicionExcepcion { get; set; }

    [JsonPropertyName("dvh")]
    public string Dvh { get; set; } = string.Empty;
}

public sealed class SnapshotFilaBE
{
    [JsonPropertyName("pk")]
    public string Pk { get; set; } = string.Empty;

    [JsonPropertyName("dvh")]
    public string Dvh { get; set; } = string.Empty;

    [JsonPropertyName("campos")]
    public Dictionary<string, object?> Campos { get; set; } = new();
}

public sealed class DigitoVerificadorBE
{
    [JsonPropertyName("tabla_nombre")]
    public string TablaNombre { get; set; } = string.Empty;

    [JsonPropertyName("dvv")]
    public string Dvv { get; set; } = string.Empty;

    [JsonPropertyName("cantidad_registros")]
    public int CantidadRegistros { get; set; }

    [JsonPropertyName("snapshot_control")]
    public List<SnapshotFilaBE> SnapshotControl { get; set; } = new();

    [JsonPropertyName("updated_at")]
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

    [JsonPropertyName("updated_by")]
    public string UpdatedBy { get; set; } = "ADMIN_EXE";
}

public sealed class ResultadoVerificacionDV
{
    public bool SaltoDetectado => Alteraciones.Count > 0;
    public List<string> Alteraciones { get; set; } = new();
    public DateTime FechaVerificacion { get; set; } = DateTime.Now;

    public string ObtenerTextoLabelCambios()
    {
        if (!SaltoDetectado)
        {
            return "Integridad verificada correctamente: los Dígitos Verificadores Horizontales (DVH) y Verticales (DVV) coinciden con la base de datos.";
        }

        return string.Join(Environment.NewLine + Environment.NewLine, Alteraciones);
    }
}

public sealed class BaseDatosCompletaBE
{
    public List<AdminUsuarioBE> AdminUsuarios { get; set; } = new();
    public List<ForwarderBE> Forwarders { get; set; } = new();
    public List<DespachanteBE> Despachantes { get; set; } = new();
    public List<PuertoAduanaBE> PuertosAduanas { get; set; } = new();
    public List<PaisBE> Paises { get; set; } = new();
    public List<NcmPosicionBE> NcmCatalogo { get; set; } = new();
    public List<DigitoVerificadorBE> DigitosVerificadores { get; set; } = new();

    [JsonIgnore]
    public int? TotalPaisesEnNube { get; set; }

    [JsonIgnore]
    public int? TotalNcmEnNube { get; set; }
}

public sealed class BackupPackageBE
{
    public string Sistema { get; set; } = "IMPORTA+ ADMIN (.NET 9)";
    public string Version { get; set; } = "1.1.0";
    public DateTimeOffset FechaBackup { get; set; } = DateTimeOffset.UtcNow;
    public string GeneradoPor { get; set; } = "Admin";
    public string FirmaIntegridadSha256 { get; set; } = string.Empty;
    public BaseDatosCompletaBE Datos { get; set; } = new();
}
