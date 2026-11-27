using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using AdminImportaPlus.BE;

namespace AdminImportaPlus.BLL;

/// <summary>
/// Capa BLL: Cifrado de credenciales con BCrypt y motor de Dígito Verificador Horizontal (DVH) y Vertical (DVV).
/// Compatible bit a bit con las funciones PostgreSQL de Base_ImportaPlus.sql.
/// </summary>
public static class SeguridadYDigitoVerificadorBLL
{
    private const int BcryptWorkFactor = 11;
    public const string DummyBcryptHash = "$2a$11$2JNMl4BJq3luXja8OugiIOywkepWfFi9sX6lccOFiayFaljaei5fS";

    public static bool ValidarFortalezaPassword(string? passwordPlano, out string mensajeError)
    {
        if (string.IsNullOrWhiteSpace(passwordPlano) || passwordPlano.Length < 4)
        {
            mensajeError = "La contraseña debe tener al menos 4 caracteres.";
            return false;
        }

        if (passwordPlano.Length > 72)
        {
            mensajeError = "La contraseña no debe superar los 72 caracteres (límite criptográfico BCrypt).";
            return false;
        }

        mensajeError = string.Empty;
        return true;
    }

    public static bool EsUrlImagenSegura(string? rawUrl)
    {
        if (string.IsNullOrWhiteSpace(rawUrl)) return false;
        string trimmed = rawUrl.Trim();
        if (trimmed.Length > 200000) return false;

        if (Regex.IsMatch(trimmed, @"^data:image/(jpeg|jpg|png|webp);base64,[A-Za-z0-9+/=]+$", RegexOptions.IgnoreCase))
        {
            return true;
        }

        if (!Uri.TryCreate(trimmed, UriKind.Absolute, out var uri)) return false;
        if (!string.Equals(uri.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase)) return false;
        if (uri.IsLoopback) return false;

        string host = uri.Host.ToLowerInvariant();
        if (host == "localhost" || host.EndsWith(".local", StringComparison.Ordinal) || host.EndsWith(".internal", StringComparison.Ordinal))
            return false;

        if (IPAddress.TryParse(host, out var ip))
        {
            if (IPAddress.IsLoopback(ip)) return false;
            if (ip.AddressFamily == AddressFamily.InterNetwork)
            {
                byte[] b = ip.GetAddressBytes();
                if (b[0] == 10 || b[0] == 127) return false;
                if (b[0] == 169 && b[1] == 254) return false;
                if (b[0] == 192 && b[1] == 168) return false;
                if (b[0] == 172 && b[1] >= 16 && b[1] <= 31) return false;
            }
            else if (ip.IsIPv6LinkLocal || ip.IsIPv6SiteLocal)
            {
                return false;
            }
        }

        return true;
    }

    public static string HashearPasswordBcrypt(string passwordPlano)
    {
        if (string.IsNullOrWhiteSpace(passwordPlano))
            throw new ArgumentException("La contraseña no puede estar vacía.");

        return BCrypt.Net.BCrypt.HashPassword(passwordPlano, workFactor: BcryptWorkFactor);
    }

    public static bool VerificarPasswordBcrypt(string passwordPlano, string passwordHash)
    {
        if (string.IsNullOrEmpty(passwordPlano) || string.IsNullOrWhiteSpace(passwordHash))
            return false;

        try
        {
            return BCrypt.Net.BCrypt.Verify(passwordPlano, passwordHash.Trim());
        }
        catch
        {
            return false;
        }
    }

    public static string Sha256Hex(string input)
    {
        byte[] bytes = Encoding.UTF8.GetBytes(input ?? string.Empty);
        byte[] hash = SHA256.HashData(bytes);
        return Convert.ToHexStringLower(hash);
    }

    // --- FÓRMULAS CANÓNICAS DVH (Idénticas a Base_ImportaPlus.sql) ---

    public static string CalcularDvhAdmin(AdminUsuarioBE u) =>
        Sha256Hex($"{(u.Username ?? "").Trim().ToLowerInvariant()}|{(u.PasswordHash ?? "").Trim()}|{(u.Activo ? "1" : "0")}");

    public static string CalcularDvhForwarder(ForwarderBE f) =>
        Sha256Hex($"{f.Id.ToString().ToLowerInvariant()}|{(f.Nombre ?? "").Trim()}|{(f.ImagenUrl ?? "").Trim()}|{(f.Whatsapp ?? "").Trim()}|{(f.Email ?? "").Trim()}|{f.Orden}|{(f.Activo ? "1" : "0")}");

    public static string CalcularDvhDespachante(DespachanteBE d) =>
        Sha256Hex($"{d.Id.ToString().ToLowerInvariant()}|{(d.Nombre ?? "").Trim()}|{(d.ImagenUrl ?? "").Trim()}|{(d.Whatsapp ?? "").Trim()}|{(d.Email ?? "").Trim()}|{d.Orden}|{(d.Activo ? "1" : "0")}");

    public static string CalcularDvhPuerto(PuertoAduanaBE p) =>
        Sha256Hex($"{p.Id.ToString().ToLowerInvariant()}|{(p.Nombre ?? "").Trim()}|{(p.ViaIngreso ?? "").Trim()}|{(p.Provincia ?? "").Trim()}|{(p.CodigoSim ?? "").Trim()}|{(p.Operador ?? "").Trim()}");

    public static string CalcularDvhPais(PaisBE p) =>
        Sha256Hex($"{(p.CodigoIso ?? "").Trim().ToUpperInvariant()}|{(p.Nombre ?? "").Trim()}|{(p.EsMercosur ? "1" : "0")}|{p.FleteEstimadoPct.ToString("0.00", CultureInfo.InvariantCulture)}");

    public static string CalcularDvhNcm(NcmPosicionBE n) =>
        Sha256Hex($"{(n.CodigoNcm ?? "").Trim()}|{(n.Descripcion ?? "").Trim()}|{(n.TipoBien ?? "").Trim().ToUpperInvariant()}|{n.Aec.ToString("0.00", CultureInfo.InvariantCulture)}|{n.IvaAlicuota.ToString("0.00", CultureInfo.InvariantCulture)}|{(n.AnexoExcepcion ?? "").Trim()}|{(n.ArancelExcepcion.HasValue ? n.ArancelExcepcion.Value.ToString("0.00", CultureInfo.InvariantCulture) : "")}");

    public static string CalcularDvvDesdeDvhs(IEnumerable<string> dvhs)
    {
        var ordenados = dvhs
            .Select(h => (h ?? string.Empty).Trim())
            .OrderBy(h => h, StringComparer.Ordinal)
            .ToList();

        string concatenado = ordenados.Count == 0 ? "EMPTY" : string.Join("|", ordenados);
        return Sha256Hex(concatenado);
    }

    public static ResultadoVerificacionDV VerificarBaseCompleta(BaseDatosCompletaBE db)
    {
        var resultado = new ResultadoVerificacionDV();
        var mapaDvv = db.DigitosVerificadores.ToDictionary(d => d.TablaNombre, d => d, StringComparer.OrdinalIgnoreCase);

        VerificarTablaGenerica(
            resultado,
            "admin_usuarios",
            db.AdminUsuarios,
            u => u.Username,
            u => u.Dvh,
            CalcularDvhAdmin,
            u => new Dictionary<string, string>
            {
                ["username"] = u.Username,
                ["password_hash"] = u.PasswordHash.Length > 15 ? u.PasswordHash[..15] + "..." : u.PasswordHash,
                ["activo"] = u.Activo.ToString().ToLowerInvariant()
            },
            mapaDvv
        );

        VerificarTablaGenerica(
            resultado,
            "forwarders",
            db.Forwarders,
            f => f.Id.ToString().ToLowerInvariant(),
            f => f.Dvh,
            CalcularDvhForwarder,
            f => new Dictionary<string, string>
            {
                ["nombre"] = f.Nombre,
                ["imagen_url"] = f.ImagenUrl,
                ["whatsapp"] = f.Whatsapp,
                ["email"] = f.Email,
                ["orden"] = f.Orden.ToString(CultureInfo.InvariantCulture),
                ["activo"] = f.Activo.ToString().ToLowerInvariant()
            },
            mapaDvv
        );

        // Si ya existe control DVV para despachantes (o registros cargados con DVH), auditarlo también
        if (mapaDvv.ContainsKey("despachantes"))
        {
            VerificarTablaGenerica(
                resultado,
                "despachantes",
                db.Despachantes,
                d => d.Id.ToString().ToLowerInvariant(),
                d => d.Dvh,
                CalcularDvhDespachante,
                d => new Dictionary<string, string>
                {
                    ["nombre"] = d.Nombre,
                    ["imagen_url"] = d.ImagenUrl,
                    ["whatsapp"] = d.Whatsapp,
                    ["email"] = d.Email,
                    ["orden"] = d.Orden.ToString(CultureInfo.InvariantCulture),
                    ["activo"] = d.Activo.ToString().ToLowerInvariant()
                },
                mapaDvv
            );
        }

        VerificarTablaGenerica(
            resultado,
            "puertos_aduanas",
            db.PuertosAduanas,
            p => p.Id.ToString().ToLowerInvariant(),
            p => p.Dvh,
            CalcularDvhPuerto,
            p => new Dictionary<string, string>
            {
                ["nombre"] = p.Nombre,
                ["via_ingreso"] = p.ViaIngreso,
                ["provincia"] = p.Provincia,
                ["codigo_sim"] = p.CodigoSim ?? "",
                ["operador"] = p.Operador ?? ""
            },
            mapaDvv
        );

        if (db.TotalPaisesEnNube.HasValue)
        {
            VerificarConteoTablaNube(resultado, "paises", db.TotalPaisesEnNube.Value, mapaDvv);
        }
        else
        {
            VerificarTablaGenerica(
                resultado,
                "paises",
                db.Paises,
                p => p.CodigoIso.ToUpperInvariant(),
                p => p.Dvh,
                CalcularDvhPais,
                p => new Dictionary<string, string>
                {
                    ["nombre"] = p.Nombre,
                    ["es_mercosur"] = p.EsMercosur.ToString().ToLowerInvariant(),
                    ["flete_estimado_pct"] = p.FleteEstimadoPct.ToString("0.00", CultureInfo.InvariantCulture)
                },
                mapaDvv
            );
        }

        if (db.TotalNcmEnNube.HasValue)
        {
            VerificarConteoTablaNube(resultado, "ncm_catalogo", db.TotalNcmEnNube.Value, mapaDvv);
        }
        else
        {
            VerificarTablaGenerica(
                resultado,
                "ncm_catalogo",
                db.NcmCatalogo,
                n => n.CodigoNcm,
                n => n.Dvh,
                CalcularDvhNcm,
                n => new Dictionary<string, string>
                {
                    ["descripcion"] = n.Descripcion.Length > 80 ? n.Descripcion[..80] : n.Descripcion,
                    ["tipo_bien"] = n.TipoBien,
                    ["aec"] = n.Aec.ToString("0.00", CultureInfo.InvariantCulture),
                    ["iva_alicuota"] = n.IvaAlicuota.ToString("0.00", CultureInfo.InvariantCulture)
                },
                mapaDvv
            );
        }

        return resultado;
    }

    private static void VerificarConteoTablaNube(
        ResultadoVerificacionDV resultado,
        string nombreTabla,
        int conteoRealNube,
        Dictionary<string, DigitoVerificadorBE> mapaDvv)
    {
        if (!mapaDvv.TryGetValue(nombreTabla, out var registroDvv))
        {
            resultado.Alteraciones.Add(
                $"⚠ [DVV FALTANTE] Tabla '{nombreTabla}': No existe registro de Dígito Verificador Vertical en 'digito_verificador' (filas actuales en Supabase: {conteoRealNube}).");
            return;
        }

        if (registroDvv.CantidadRegistros != conteoRealNube)
        {
            resultado.Alteraciones.Add(
                $"⚠ [DVV VERTICAL SALTÓ] Tabla '{nombreTabla}': Cantidad esperada {registroDvv.CantidadRegistros} ≠ Cantidad actual en Supabase {conteoRealNube}.");
        }
    }

    private static void VerificarTablaGenerica<T>(
        ResultadoVerificacionDV resultado,
        string nombreTabla,
        List<T> filasActuales,
        Func<T, string> getPk,
        Func<T, string> getDvhAlmacenado,
        Func<T, string> calcularDvhReal,
        Func<T, Dictionary<string, string>> extraerMapaCampos,
        Dictionary<string, DigitoVerificadorBE> mapaDvv)
    {
        mapaDvv.TryGetValue(nombreTabla, out var registroDvv);
        var snapshotPorPk = registroDvv?.SnapshotControl?
            .GroupBy(s => s.Pk, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase)
            ?? new Dictionary<string, SnapshotFilaBE>(StringComparer.OrdinalIgnoreCase);

        var pksActuales = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var dvhsRealesCalculados = new List<string>();

        foreach (var fila in filasActuales)
        {
            string pk = getPk(fila);
            pksActuales.Add(pk);

            string dvhGuardado = (getDvhAlmacenado(fila) ?? string.Empty).Trim();
            string dvhCalculado = calcularDvhReal(fila);
            dvhsRealesCalculados.Add(dvhCalculado);

            var camposActuales = extraerMapaCampos(fila);
            snapshotPorPk.TryGetValue(pk, out var snapAnterior);

            if (!string.Equals(dvhGuardado, dvhCalculado, StringComparison.OrdinalIgnoreCase))
            {
                string detalleCampos = DescribirDiferenciasCampos(snapAnterior?.Campos, camposActuales);
                resultado.Alteraciones.Add(
                    $"⚠ [DVH HORIZONTAL SALTÓ] Tabla '{nombreTabla}' · Registro [{pk}]: {detalleCampos} " +
                    $"(DVH almacenado: {CortarHash(dvhGuardado)} ≠ DVH calculado: {CortarHash(dvhCalculado)}).");
            }
            else if (snapAnterior != null && !string.Equals(snapAnterior.Dvh, dvhCalculado, StringComparison.OrdinalIgnoreCase))
            {
                string detalleCampos = DescribirDiferenciasCampos(snapAnterior.Campos, camposActuales);
                resultado.Alteraciones.Add(
                    $"⚠ [ALTERACIÓN CON DVH REESCRITO] Tabla '{nombreTabla}' · Registro [{pk}]: {detalleCampos}");
            }
            else if (registroDvv != null && snapAnterior == null)
            {
                string resumenNuevo = string.Join(", ", camposActuales.Select(kv => $"{kv.Key}='{kv.Value}'"));
                resultado.Alteraciones.Add(
                    $"⚠ [INSERCIÓN EXTERNA DETECTADA] Tabla '{nombreTabla}': Se agregó el registro [{pk}] fuera del sistema ({resumenNuevo}).");
            }
        }

        if (registroDvv != null)
        {
            foreach (var snap in snapshotPorPk.Values)
            {
                if (!pksActuales.Contains(snap.Pk))
                {
                    string infoEliminado = snap.Campos != null && snap.Campos.Count > 0
                        ? string.Join(", ", snap.Campos.Take(3).Select(kv => $"{kv.Key}='{kv.Value}'"))
                        : snap.Pk;
                    resultado.Alteraciones.Add(
                        $"⚠ [ELIMINACIÓN EXTERNA - DVV SALTÓ] Tabla '{nombreTabla}': El registro [{snap.Pk}] ({infoEliminado}) fue borrado directamente en la base de datos.");
                }
            }

            string dvvCalculadoReal = CalcularDvvDesdeDvhs(dvhsRealesCalculados);
            if (!string.Equals(registroDvv.Dvv?.Trim(), dvvCalculadoReal, StringComparison.OrdinalIgnoreCase) ||
                registroDvv.CantidadRegistros != filasActuales.Count)
            {
                resultado.Alteraciones.Add(
                    $"⚠ [DVV VERTICAL SALTÓ] Tabla '{nombreTabla}': El Dígito Verificador Vertical no coincide " +
                    $"(Filas esperadas: {registroDvv.CantidadRegistros}, Filas actuales: {filasActuales.Count} | " +
                    $"DVV guardado: {CortarHash(registroDvv.Dvv)} ≠ DVV real: {CortarHash(dvhsRealesCalculados.Count > 0 ? dvvCalculadoReal : "EMPTY")}).");
            }
        }
        else
        {
            resultado.Alteraciones.Add(
                $"⚠ [DVV FALTANTE] Tabla '{nombreTabla}': No existe registro de Dígito Verificador Vertical en 'digito_verificador'.");
        }
    }

    private static string ValorCampoAString(object? val)
    {
        if (val == null) return string.Empty;
        if (val is System.Text.Json.JsonElement je)
        {
            return je.ValueKind switch
            {
                System.Text.Json.JsonValueKind.String => je.GetString() ?? string.Empty,
                System.Text.Json.JsonValueKind.True => "true",
                System.Text.Json.JsonValueKind.False => "false",
                System.Text.Json.JsonValueKind.Null or System.Text.Json.JsonValueKind.Undefined => string.Empty,
                _ => je.ToString()
            };
        }
        return val.ToString() ?? string.Empty;
    }

    private static string DescribirDiferenciasCampos(Dictionary<string, object?>? antes, Dictionary<string, string> ahora)
    {
        if (antes == null || antes.Count == 0)
        {
            string actual = string.Join(", ", ahora.Take(3).Select(kv => $"{kv.Key}='{kv.Value}'"));
            return $"Dato alterado externamente (valores actuales: {actual})";
        }

        var cambios = new List<string>();
        foreach (var kv in ahora)
        {
            if (antes.TryGetValue(kv.Key, out var valorAnteriorObj))
            {
                string valorAnterior = ValorCampoAString(valorAnteriorObj);
                if (!string.Equals(valorAnterior.Trim(), kv.Value?.Trim(), StringComparison.OrdinalIgnoreCase))
                {
                    cambios.Add($"campo '{kv.Key}' modificado ('{valorAnterior}' → '{kv.Value}')");
                }
            }
        }

        return cambios.Count > 0
            ? string.Join("; ", cambios)
            : "contenido del registro alterado externamente";
    }

    private static string CortarHash(string? hash)
    {
        if (string.IsNullOrWhiteSpace(hash)) return "VACÍO";
        return hash.Length > 12 ? hash[..12] + "..." : hash;
    }

    public static void RecalcularTodoEnMemoria(BaseDatosCompletaBE db, string usuario = "Admin")
    {
        foreach (var u in db.AdminUsuarios) u.Dvh = CalcularDvhAdmin(u);
        foreach (var f in db.Forwarders) f.Dvh = CalcularDvhForwarder(f);
        foreach (var d in db.Despachantes) d.Dvh = CalcularDvhDespachante(d);
        foreach (var p in db.PuertosAduanas) p.Dvh = CalcularDvhPuerto(p);
        foreach (var p in db.Paises) p.Dvh = CalcularDvhPais(p);
        foreach (var n in db.NcmCatalogo) n.Dvh = CalcularDvhNcm(n);

        var ahora = DateTimeOffset.UtcNow;
        db.DigitosVerificadores = new List<DigitoVerificadorBE>
        {
            new()
            {
                TablaNombre = "admin_usuarios",
                Dvv = CalcularDvvDesdeDvhs(db.AdminUsuarios.Select(x => x.Dvh)),
                CantidadRegistros = db.AdminUsuarios.Count,
                UpdatedAt = ahora,
                UpdatedBy = usuario,
                SnapshotControl = db.AdminUsuarios.OrderBy(x => x.Username).Select(u => new SnapshotFilaBE
                {
                    Pk = u.Username,
                    Dvh = u.Dvh,
                    Campos = new Dictionary<string, object?>
                    {
                        ["username"] = u.Username,
                        ["password_hash"] = u.PasswordHash.Length > 15 ? u.PasswordHash[..15] + "..." : u.PasswordHash,
                        ["activo"] = u.Activo.ToString().ToLowerInvariant()
                    }
                }).ToList()
            },
            new()
            {
                TablaNombre = "forwarders",
                Dvv = CalcularDvvDesdeDvhs(db.Forwarders.Select(x => x.Dvh)),
                CantidadRegistros = db.Forwarders.Count,
                UpdatedAt = ahora,
                UpdatedBy = usuario,
                SnapshotControl = db.Forwarders.OrderBy(x => x.Orden).Select(f => new SnapshotFilaBE
                {
                    Pk = f.Id.ToString().ToLowerInvariant(),
                    Dvh = f.Dvh,
                    Campos = new Dictionary<string, object?>
                    {
                        ["nombre"] = f.Nombre,
                        ["imagen_url"] = f.ImagenUrl,
                        ["whatsapp"] = f.Whatsapp,
                        ["email"] = f.Email,
                        ["orden"] = f.Orden.ToString(CultureInfo.InvariantCulture),
                        ["activo"] = f.Activo.ToString().ToLowerInvariant()
                    }
                }).ToList()
            },
            new()
            {
                TablaNombre = "despachantes",
                Dvv = CalcularDvvDesdeDvhs(db.Despachantes.Select(x => x.Dvh)),
                CantidadRegistros = db.Despachantes.Count,
                UpdatedAt = ahora,
                UpdatedBy = usuario,
                SnapshotControl = db.Despachantes.OrderBy(x => x.Orden).Select(d => new SnapshotFilaBE
                {
                    Pk = d.Id.ToString().ToLowerInvariant(),
                    Dvh = d.Dvh,
                    Campos = new Dictionary<string, object?>
                    {
                        ["nombre"] = d.Nombre,
                        ["imagen_url"] = d.ImagenUrl,
                        ["whatsapp"] = d.Whatsapp,
                        ["email"] = d.Email,
                        ["orden"] = d.Orden.ToString(CultureInfo.InvariantCulture),
                        ["activo"] = d.Activo.ToString().ToLowerInvariant()
                    }
                }).ToList()
            },
            new()
            {
                TablaNombre = "puertos_aduanas",
                Dvv = CalcularDvvDesdeDvhs(db.PuertosAduanas.Select(x => x.Dvh)),
                CantidadRegistros = db.PuertosAduanas.Count,
                UpdatedAt = ahora,
                UpdatedBy = usuario,
                SnapshotControl = db.PuertosAduanas.OrderBy(x => x.Nombre).Select(p => new SnapshotFilaBE
                {
                    Pk = p.Id.ToString().ToLowerInvariant(),
                    Dvh = p.Dvh,
                    Campos = new Dictionary<string, object?>
                    {
                        ["nombre"] = p.Nombre,
                        ["via_ingreso"] = p.ViaIngreso,
                        ["provincia"] = p.Provincia,
                        ["codigo_sim"] = p.CodigoSim ?? "",
                        ["operador"] = p.Operador ?? ""
                    }
                }).ToList()
            },
            new()
            {
                TablaNombre = "paises",
                Dvv = CalcularDvvDesdeDvhs(db.Paises.Select(x => x.Dvh)),
                CantidadRegistros = db.Paises.Count,
                UpdatedAt = ahora,
                UpdatedBy = usuario,
                SnapshotControl = db.Paises.OrderBy(x => x.CodigoIso).Select(p => new SnapshotFilaBE
                {
                    Pk = p.CodigoIso.ToUpperInvariant(),
                    Dvh = p.Dvh,
                    Campos = new Dictionary<string, object?>
                    {
                        ["nombre"] = p.Nombre,
                        ["es_mercosur"] = p.EsMercosur.ToString().ToLowerInvariant(),
                        ["flete_estimado_pct"] = p.FleteEstimadoPct.ToString("0.00", CultureInfo.InvariantCulture)
                    }
                }).ToList()
            },
            new()
            {
                TablaNombre = "ncm_catalogo",
                Dvv = CalcularDvvDesdeDvhs(db.NcmCatalogo.Select(x => x.Dvh)),
                CantidadRegistros = db.NcmCatalogo.Count,
                UpdatedAt = ahora,
                UpdatedBy = usuario,
                SnapshotControl = db.NcmCatalogo.OrderBy(x => x.CodigoNcm).Select(n => new SnapshotFilaBE
                {
                    Pk = n.CodigoNcm,
                    Dvh = n.Dvh,
                    Campos = new Dictionary<string, object?>
                    {
                        ["descripcion"] = n.Descripcion.Length > 80 ? n.Descripcion[..80] : n.Descripcion,
                        ["tipo_bien"] = n.TipoBien,
                        ["aec"] = n.Aec.ToString("0.00", CultureInfo.InvariantCulture),
                        ["iva_alicuota"] = n.IvaAlicuota.ToString("0.00", CultureInfo.InvariantCulture)
                    }
                }).ToList()
            }
        };
    }
}
