using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Windows.Forms;
using AdminImportaPlus.BE;

namespace AdminImportaPlus.BLL;

/// <summary>
/// Capa BLL: Sistema de Perfiles, Familias y Permisos (Patrón GoF COMPOSITE) adaptado de ING-SOFTWARE-BritezG.
/// Regla de Dominio Estricta:
/// - UN PERMISO ES UN BOTÓN.
/// - En la lista de permisos SOLO Y ÚNICAMENTE aparecen BOTONES reales de la interfaz.
/// - Si un Perfil tiene asignado dicho permiso (directo o por Familia), el usuario tendrá visible y habilitado ese botón.
/// </summary>
public sealed class RolesCompositeBLL
{
    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    public static readonly IReadOnlyList<(Guid Id, string NombreBoton, string EtiquetaBoton)> CatalogoOficialBotones = new List<(Guid, string, string)>
    {
        // Botones del Menú Lateral
        (Guid.Parse("10000002-0000-0000-0000-000000000001"), "btnNavForwarders", "Menú · Forwarders"),
        (Guid.Parse("10000002-0000-0000-0000-000000000002"), "btnNavDespachantes", "Menú · Despachantes de Aduana"),
        (Guid.Parse("10000002-0000-0000-0000-000000000003"), "btnNavPuertos", "Menú · Puertos y Aduanas"),
        (Guid.Parse("10000002-0000-0000-0000-000000000004"), "btnNavUsuarios", "Menú · Gestión de Usuarios"),
        (Guid.Parse("10000002-0000-0000-0000-000000000005"), "btnNavRoles", "Menú · Gestión de Perfiles"),
        (Guid.Parse("10000002-0000-0000-0000-000000000006"), "btnNavDigitoVerificador", "Menú · Dígito Verificador"),
        (Guid.Parse("10000002-0000-0000-0000-000000000007"), "btnNavRestoreSupabase", "Menú · RestoreBackup y Nube"),
        (Guid.Parse("10000002-0000-0000-0000-000000000008"), "btnCambiarPassword", "Menú · Cambiar Contraseña"),

        // Botones ABM Forwarders
        (Guid.Parse("10000002-0000-0000-0000-000000000009"), "btnGuardarFwd", "Forwarders · Guardar Forwarder"),
        (Guid.Parse("10000002-0000-0000-0000-000000000010"), "btnNuevoFwd", "Forwarders · Nuevo / Limpiar"),
        (Guid.Parse("10000002-0000-0000-0000-000000000011"), "btnEliminarFwd", "Forwarders · Eliminar"),
        (Guid.Parse("10000002-0000-0000-0000-000000000012"), "btnSubirFoto", "Forwarders · Seleccionar Imagen"),

        // Botones ABM Despachantes de Aduana
        (Guid.Parse("10000002-0000-0000-0000-000000000013"), "btnGuardarDesp", "Despachantes · Guardar Despachante"),
        (Guid.Parse("10000002-0000-0000-0000-000000000014"), "btnNuevoDesp", "Despachantes · Nuevo / Limpiar"),
        (Guid.Parse("10000002-0000-0000-0000-000000000015"), "btnEliminarDesp", "Despachantes · Eliminar"),
        (Guid.Parse("10000002-0000-0000-0000-000000000016"), "btnSubirFotoDesp", "Despachantes · Seleccionar Imagen"),

        // Botones ABM Puertos y Aduanas
        (Guid.Parse("10000002-0000-0000-0000-000000000017"), "btnGuardarPto", "Puertos · Guardar Puerto / Aduana"),
        (Guid.Parse("10000002-0000-0000-0000-000000000018"), "btnNuevoPto", "Puertos · Nuevo Puerto"),
        (Guid.Parse("10000002-0000-0000-0000-000000000019"), "btnBorrarPto", "Puertos · Eliminar"),

        // Botones Gestión de Usuarios
        (Guid.Parse("10000002-0000-0000-0000-000000000020"), "btnCrearUsuario", "Usuarios · Crear"),
        (Guid.Parse("10000002-0000-0000-0000-000000000021"), "btnModificarUsuario", "Usuarios · Modificar"),
        (Guid.Parse("10000002-0000-0000-0000-000000000022"), "btnDesbloquearUsuario", "Usuarios · Desbloquear"),
        (Guid.Parse("10000002-0000-0000-0000-000000000023"), "btnActDesactUsuario", "Usuarios · Act/Desact"),
        (Guid.Parse("10000002-0000-0000-0000-000000000024"), "btnResetPassUsuario", "Usuarios · Cambiar Clave"),

        // Botones Dígito Verificador y RestoreBackup / Supabase
        (Guid.Parse("10000002-0000-0000-0000-000000000025"), "btnVerificarDv", "Dígito Verificador · Verificar Integridad"),
        (Guid.Parse("10000002-0000-0000-0000-000000000026"), "btnRecalcularDv", "Dígito Verificador · Recalcular DV"),
        (Guid.Parse("10000002-0000-0000-0000-000000000027"), "btnCrearBackup", "RestoreBackup · Generar Backup"),
        (Guid.Parse("10000002-0000-0000-0000-000000000028"), "btnEjecutarRestore", "RestoreBackup · Cargar Base Actual"),
        (Guid.Parse("10000002-0000-0000-0000-000000000029"), "btnGuardarSupa", "Supabase · Guardar y Probar Conexión"),
        (Guid.Parse("10000002-0000-0000-0000-000000000030"), "btnEmpujarNube", "Supabase · Sincronizar Datos")
    };

    private static readonly HashSet<string> CodigosBotonesGestionados = new(
        CatalogoOficialBotones.Select(b => b.NombreBoton),
        StringComparer.OrdinalIgnoreCase);

    private readonly string _rolesFilePath;
    public RolesStoreBE Store { get; private set; }
    public event Action? OnStoreChanged;

    public RolesCompositeBLL(string dataFolder)
    {
        Directory.CreateDirectory(dataFolder);
        _rolesFilePath = Path.Combine(dataFolder, "roles_composite.json");
        Store = CargarOCrearSemilla();
    }

    public static bool EsBotonGestionadoPorPermiso(string? nombreControl) =>
        !string.IsNullOrWhiteSpace(nombreControl) && CodigosBotonesGestionados.Contains(nombreControl.Trim());

    private RolesStoreBE CargarOCrearSemilla()
    {
        if (File.Exists(_rolesFilePath))
        {
            try
            {
                var loaded = JsonSerializer.Deserialize<RolesStoreBE>(File.ReadAllText(_rolesFilePath, Encoding.UTF8), JsonOpts);
                // Validar que sea SchemaVersion >= 2 y que TODOS los permisos sean exclusivamente botones (btn...)
                if (loaded != null &&
                    loaded.SchemaVersion >= 2 &&
                    loaded.Perfiles.Count > 0 &&
                    loaded.Permisos.All(p => p.Codigo.StartsWith("btn", StringComparison.OrdinalIgnoreCase)))
                {
                    loaded.PerfilPorUsuario = new Dictionary<string, Guid>(
                        loaded.PerfilPorUsuario ?? new(),
                        StringComparer.OrdinalIgnoreCase);
                    return loaded;
                }
            }
            catch { }
        }

        var semilla = CrearSemillaBotonesPuros();
        Store = semilla;
        Guardar();
        return semilla;
    }

    public static RolesStoreBE CrearSemillaBotonesPuros()
    {
        var permisosBotones = CatalogoOficialBotones
            .Select(b => new PermisoDto
            {
                Id = b.Id,
                Codigo = b.NombreBoton,
                Nombre = b.EtiquetaBoton,
                NombreFormulario = "FrmPanelAdmin"
            })
            .ToList();

        Guid IdBtn(string codigo) => permisosBotones.First(p => string.Equals(p.Codigo, codigo, StringComparison.OrdinalIgnoreCase)).Id;

        var famFwd = new FamiliaDto
        {
            Id = Guid.Parse("20000002-0000-0000-0000-000000000001"),
            Codigo = "FAM_BOTONES_FORWARDERS",
            Nombre = "Botones Forwarders",
            HijosIds = new()
            {
                IdBtn("btnNavForwarders"),
                IdBtn("btnGuardarFwd"),
                IdBtn("btnNuevoFwd"),
                IdBtn("btnEliminarFwd"),
                IdBtn("btnSubirFoto")
            }
        };

        var famDesp = new FamiliaDto
        {
            Id = Guid.Parse("20000002-0000-0000-0000-000000000002"),
            Codigo = "FAM_BOTONES_DESPACHANTES",
            Nombre = "Botones Despachantes de Aduana",
            HijosIds = new()
            {
                IdBtn("btnNavDespachantes"),
                IdBtn("btnGuardarDesp"),
                IdBtn("btnNuevoDesp"),
                IdBtn("btnEliminarDesp"),
                IdBtn("btnSubirFotoDesp")
            }
        };

        var famPuertos = new FamiliaDto
        {
            Id = Guid.Parse("20000002-0000-0000-0000-000000000003"),
            Codigo = "FAM_BOTONES_PUERTOS",
            Nombre = "Botones Puertos y Aduanas",
            HijosIds = new()
            {
                IdBtn("btnNavPuertos"),
                IdBtn("btnGuardarPto"),
                IdBtn("btnNuevoPto"),
                IdBtn("btnBorrarPto")
            }
        };

        var famUsuariosPerfiles = new FamiliaDto
        {
            Id = Guid.Parse("20000002-0000-0000-0000-000000000004"),
            Codigo = "FAM_BOTONES_USUARIOS_PERFILES",
            Nombre = "Botones Usuarios y Perfiles",
            HijosIds = new()
            {
                IdBtn("btnNavUsuarios"),
                IdBtn("btnNavRoles"),
                IdBtn("btnCrearUsuario"),
                IdBtn("btnModificarUsuario"),
                IdBtn("btnDesbloquearUsuario"),
                IdBtn("btnActDesactUsuario"),
                IdBtn("btnResetPassUsuario"),
                IdBtn("btnCambiarPassword")
            }
        };

        var famSeguridadBackup = new FamiliaDto
        {
            Id = Guid.Parse("20000002-0000-0000-0000-000000000005"),
            Codigo = "FAM_BOTONES_DV_BACKUP_NUBE",
            Nombre = "Botones DV, Backup y Nube",
            HijosIds = new()
            {
                IdBtn("btnNavDigitoVerificador"),
                IdBtn("btnVerificarDv"),
                IdBtn("btnRecalcularDv"),
                IdBtn("btnNavRestoreSupabase"),
                IdBtn("btnCrearBackup"),
                IdBtn("btnEjecutarRestore"),
                IdBtn("btnGuardarSupa"),
                IdBtn("btnEmpujarNube")
            }
        };

        var perfilAdmin = new PerfilBE
        {
            Id = Guid.Parse("30000002-0000-0000-0000-000000000001"),
            Nombre = "Administrador",
            Descripcion = "Acceso total a todos los botones del sistema.",
            ComponentesAsignadosIds = new()
            {
                famFwd.Id,
                famDesp.Id,
                famPuertos.Id,
                famUsuariosPerfiles.Id,
                famSeguridadBackup.Id
            }
        };

        var perfilOperador = new PerfilBE
        {
            Id = Guid.Parse("30000002-0000-0000-0000-000000000002"),
            Nombre = "Operador Logístico",
            Descripcion = "Acceso únicamente a los botones de Forwarders, Despachantes, Puertos y Cambiar Contraseña.",
            ComponentesAsignadosIds = new()
            {
                famFwd.Id,
                famDesp.Id,
                famPuertos.Id,
                IdBtn("btnCambiarPassword")
            }
        };

        return new RolesStoreBE
        {
            SchemaVersion = 2,
            Permisos = permisosBotones,
            Familias = new() { famFwd, famDesp, famPuertos, famUsuariosPerfiles, famSeguridadBackup },
            Perfiles = new() { perfilAdmin, perfilOperador },
            PerfilPorUsuario = new(StringComparer.OrdinalIgnoreCase)
            {
                ["Admin"] = perfilAdmin.Id
            },
            FichasUsuarios = new()
            {
                new UsuarioFichaExtendidaDto
                {
                    Username = "Admin",
                    Dni = 11111111,
                    Nombre = "Administrador",
                    Apellido = "General",
                    Bloqueado = false,
                    PerfilId = perfilAdmin.Id
                }
            }
        };
    }

    public void ReemplazarStoreDesdeNube(RolesStoreBE remoto)
    {
        if (remoto == null || remoto.SchemaVersion < 2 || remoto.Perfiles.Count == 0) return;
        // Garantizar que únicamente existan botones en la lista de permisos
        remoto.Permisos = remoto.Permisos
            .Where(p => !string.IsNullOrWhiteSpace(p.Codigo) && p.Codigo.StartsWith("btn", StringComparison.OrdinalIgnoreCase))
            .ToList();
        remoto.PerfilPorUsuario = new Dictionary<string, Guid>(
            remoto.PerfilPorUsuario ?? new(),
            StringComparer.OrdinalIgnoreCase);
        Store = remoto;
        File.WriteAllText(_rolesFilePath, JsonSerializer.Serialize(Store, JsonOpts), Encoding.UTF8);
    }

    public void Guardar()
    {
        Store.SchemaVersion = 2;
        File.WriteAllText(_rolesFilePath, JsonSerializer.Serialize(Store, JsonOpts), Encoding.UTF8);
        OnStoreChanged?.Invoke();
    }

    // --- LISTAS PARA LAS 3 GRILLAS (Idéntico a Perfiles.cs de ING-SOFTWARE-BritezG) ---

    public List<ItemGrillaRbacBE> ObtenerPermisosBotonesGrilla() =>
        Store.Permisos
            .Where(p => p.Codigo.StartsWith("btn", StringComparison.OrdinalIgnoreCase))
            .Select(p => new ItemGrillaRbacBE { Id = p.Id, Codigo = p.Codigo, Nombre = p.Nombre })
            .ToList();

    public List<ItemGrillaRbacBE> ObtenerFamiliasGrilla() =>
        Store.Familias
            .OrderBy(f => f.Nombre)
            .Select(f => new ItemGrillaRbacBE { Id = f.Id, Codigo = f.Codigo, Nombre = f.Nombre })
            .ToList();

    public List<ItemGrillaRbacBE> ObtenerPerfilesGrilla() =>
        Store.Perfiles
            .OrderBy(p => p.Nombre)
            .Select(p => new ItemGrillaRbacBE { Id = p.Id, Codigo = p.Nombre, Nombre = p.Nombre })
            .ToList();

    // --- DESCUBRIMIENTO DINÁMICO DE BOTONES POR REFLECTION (Como FormManager en ING-SOFTWARE-BritezG) ---

    public static List<string> ObtenerFormulariosConBotones() => new() { "FrmPanelAdmin" };

    public List<(string NombreControl, string EtiquetaLegible)> ObtenerBotonesDeFormulario(string nombreFormulario)
    {
        var resultado = new List<(string, string)>();
        var mapaOficial = CatalogoOficialBotones.ToDictionary(x => x.NombreBoton, x => x.EtiquetaBoton, StringComparer.OrdinalIgnoreCase);

        foreach (var item in CatalogoOficialBotones)
        {
            resultado.Add((item.NombreBoton, item.EtiquetaBoton));
        }

        // Escanear por Reflection cualquier Button adicional declarado en el formulario
        var asm = Assembly.GetExecutingAssembly();
        var tipoForm = asm.GetTypes().FirstOrDefault(t =>
            typeof(Form).IsAssignableFrom(t) &&
            string.Equals(t.Name, nombreFormulario, StringComparison.OrdinalIgnoreCase));

        if (tipoForm != null)
        {
            var campos = tipoForm.GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            foreach (var c in campos)
            {
                if (!typeof(Button).IsAssignableFrom(c.FieldType)) continue;
                if (string.Equals(c.Name, "btnCerrarSesion", StringComparison.OrdinalIgnoreCase)) continue;
                if (string.Equals(c.Name, "btnExaminarBk", StringComparison.OrdinalIgnoreCase)) continue;
                if (!c.Name.StartsWith("btn", StringComparison.OrdinalIgnoreCase)) continue;
                if (mapaOficial.ContainsKey(c.Name)) continue;

                resultado.Add((c.Name, $"Botón · {c.Name}"));
            }
        }

        return resultado;
    }

    public Dictionary<Guid, ComponentePermiso> ConstruirGrafoComposite()
    {
        var mapa = new Dictionary<Guid, ComponentePermiso>();

        foreach (var p in Store.Permisos)
        {
            if (!p.Codigo.StartsWith("btn", StringComparison.OrdinalIgnoreCase)) continue;
            mapa[p.Id] = new PermisoSimple
            {
                Id = p.Id,
                Codigo = p.Codigo,
                Nombre = p.Nombre,
                NombreFormulario = p.NombreFormulario
            };
        }

        foreach (var f in Store.Familias)
        {
            mapa[f.Id] = new FamiliaPermisos { Id = f.Id, Codigo = f.Codigo, Nombre = f.Nombre };
        }

        foreach (var f in Store.Familias)
        {
            if (mapa[f.Id] is not FamiliaPermisos fam) continue;
            foreach (var hijoId in f.HijosIds)
            {
                if (mapa.TryGetValue(hijoId, out var comp))
                {
                    try { fam.Agregar(comp); } catch { }
                }
            }
        }

        return mapa;
    }

    // --- OPERACIONES DE ALTA (AGREGAR) ---

    public (bool Ok, string Mensaje) RegistrarPermisoBoton(string nombreFormulario, string nombreBoton, string etiquetaBoton)
    {
        nombreBoton = (nombreBoton ?? string.Empty).Trim();
        etiquetaBoton = (etiquetaBoton ?? string.Empty).Trim();

        if (string.IsNullOrWhiteSpace(nombreBoton) || !nombreBoton.StartsWith("btn", StringComparison.OrdinalIgnoreCase))
            return (false, "Un permiso debe corresponder única y exclusivamente a un BOTÓN del sistema.");

        if (Store.Permisos.Any(p => string.Equals(p.Codigo, nombreBoton, StringComparison.OrdinalIgnoreCase)))
            return (false, $"El botón '{etiquetaBoton}' ({nombreBoton}) ya se encuentra en la lista de permisos.");

        var oficial = CatalogoOficialBotones.FirstOrDefault(b => string.Equals(b.NombreBoton, nombreBoton, StringComparison.OrdinalIgnoreCase));
        Guid id = oficial != default ? oficial.Id : Guid.NewGuid();
        string nombreFinal = string.IsNullOrWhiteSpace(etiquetaBoton)
            ? (oficial != default ? oficial.EtiquetaBoton : nombreBoton)
            : etiquetaBoton;

        Store.Permisos.Add(new PermisoDto
        {
            Id = id,
            Codigo = nombreBoton,
            Nombre = nombreFinal,
            NombreFormulario = string.IsNullOrWhiteSpace(nombreFormulario) ? "FrmPanelAdmin" : nombreFormulario
        });
        Guardar();
        return (true, $"Botón '{nombreFinal}' incorporado a la lista de permisos.");
    }

    public (bool Ok, string Mensaje, Guid FamiliaId) CrearFamilia(string nombre)
    {
        nombre = (nombre ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(nombre))
            return (false, "El nombre de la familia no puede estar vacío.", Guid.Empty);

        if (Store.Familias.Any(f => string.Equals(f.Nombre, nombre, StringComparison.OrdinalIgnoreCase)))
            return (false, $"Ya existe una familia registrada con el nombre '{nombre}'.", Guid.Empty);

        string codigo = "FAM_" + new string(nombre.ToUpperInvariant().Where(c => char.IsLetterOrDigit(c) || c == ' ').ToArray()).Replace(" ", "_");
        var nueva = new FamiliaDto { Id = Guid.NewGuid(), Codigo = codigo, Nombre = nombre };
        Store.Familias.Add(nueva);
        Guardar();
        return (true, $"Familia '{nombre}' creada con éxito.", nueva.Id);
    }

    public (bool Ok, string Mensaje) CrearPerfil(string nombre, string descripcion = "")
    {
        nombre = (nombre ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(nombre))
            return (false, "El nombre del perfil no puede estar vacío.");

        if (Store.Perfiles.Any(p => string.Equals(p.Nombre, nombre, StringComparison.OrdinalIgnoreCase)))
            return (false, $"Ya existe un perfil registrado con el nombre '{nombre}'.");

        Store.Perfiles.Add(new PerfilBE
        {
            Id = Guid.NewGuid(),
            Nombre = nombre,
            Descripcion = (descripcion ?? string.Empty).Trim()
        });
        Guardar();
        return (true, $"¡Perfil '{nombre}' creado en el sistema con éxito!");
    }

    public (bool Ok, string Mensaje) AgregarComponenteAFamilia(Guid familiaPadreId, Guid componenteHijoId)
    {
        var famDto = Store.Familias.FirstOrDefault(f => f.Id == familiaPadreId);
        if (famDto == null) return (false, "Familia destino no encontrada.");

        var grafo = ConstruirGrafoComposite();
        if (!grafo.TryGetValue(familiaPadreId, out var famComp) || famComp is not FamiliaPermisos fam)
            return (false, "Familia inválida.");

        if (!grafo.TryGetValue(componenteHijoId, out var hijo))
            return (false, "Botón o sub-familia no encontrado.");

        if (famDto.HijosIds.Contains(componenteHijoId))
            return (false, $"'{hijo.Nombre}' ya se encuentra dentro de la familia '{fam.Nombre}'.");

        try
        {
            fam.Agregar(hijo);
        }
        catch (InvalidOperationException ex)
        {
            return (false, ex.Message);
        }

        // Validar que agregar este hijo no genere botones duplicados en ningún Perfil que utilice esta familia
        var nuevosCodigos = hijo.ObtenerCodigosEfectivos();
        foreach (var perfil in Store.Perfiles)
        {
            bool usaEstaFamilia = perfil.ComponentesAsignadosIds.Any(cid =>
                cid == familiaPadreId || (grafo.TryGetValue(cid, out var c) && c is FamiliaPermisos fp && fp.ContieneComponenteId(familiaPadreId)));

            if (!usaEstaFamilia) continue;

            var codigosOtros = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var cid in perfil.ComponentesAsignadosIds)
            {
                if (cid == familiaPadreId) continue;
                if (grafo.TryGetValue(cid, out var c))
                {
                    foreach (var cod in c.ObtenerCodigosEfectivos()) codigosOtros.Add(cod);
                }
            }

            var conflicto = codigosOtros.Intersect(nuevosCodigos, StringComparer.OrdinalIgnoreCase).ToList();
            if (conflicto.Count > 0)
            {
                return (false,
                    $"Regla anti-duplicados: No se puede agregar '{hijo.Nombre}' a '{fam.Nombre}' porque el perfil '{perfil.Nombre}' ya posee ese botón por otra asignación.");
            }
        }

        famDto.HijosIds.Add(componenteHijoId);
        Guardar();
        return (true, $"¡'{hijo.Nombre}' asignado a la familia '{fam.Nombre}' con éxito!");
    }

    public (bool Ok, string Mensaje) AsignarComponenteAPerfil(Guid perfilId, Guid componenteId)
    {
        var perfil = Store.Perfiles.FirstOrDefault(p => p.Id == perfilId);
        if (perfil == null) return (false, "Perfil no encontrado.");

        var grafo = ConstruirGrafoComposite();
        if (!grafo.TryGetValue(componenteId, out var candidato))
            return (false, "Botón o familia no encontrado.");

        if (perfil.ComponentesAsignadosIds.Contains(componenteId))
            return (false, $"'{candidato.Nombre}' ya se encuentra asignado directamente al perfil '{perfil.Nombre}'.");

        var permisosActualesPerfil = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var asignadoId in perfil.ComponentesAsignadosIds)
        {
            if (!grafo.TryGetValue(asignadoId, out var compAsignado)) continue;
            foreach (var cod in compAsignado.ObtenerCodigosEfectivos())
            {
                permisosActualesPerfil.TryAdd(cod, compAsignado.Nombre);
            }
        }

        var codigosCandidato = candidato.ObtenerCodigosEfectivos();
        if (candidato.EsFamilia && codigosCandidato.Count == 0)
            return (false, $"La familia '{candidato.Nombre}' está vacía. Asignale al menos un botón antes de vincularla a un perfil.");

        var conflictos = new List<string>();
        foreach (var cod in codigosCandidato)
        {
            if (permisosActualesPerfil.TryGetValue(cod, out var origenExistente))
            {
                var nomBtn = Store.Permisos.FirstOrDefault(p => string.Equals(p.Codigo, cod, StringComparison.OrdinalIgnoreCase))?.Nombre ?? cod;
                conflictos.Add($"'{nomBtn}' (ya otorgado por '{origenExistente}')");
            }
        }

        if (conflictos.Count > 0)
        {
            return (false,
                $"No se puede asignar '{candidato.Nombre}' al perfil '{perfil.Nombre}' porque duplicaría botones ya asignados: {string.Join("; ", conflictos)}.");
        }

        perfil.ComponentesAsignadosIds.Add(componenteId);
        Guardar();
        return (true, $"¡'{candidato.Nombre}' asignado al perfil '{perfil.Nombre}' con éxito!");
    }

    // --- OPERACIONES DE BAJA / DESVINCULACIÓN (ELIMINAR) ---

    public (bool Ok, string Mensaje) DesvincularComponenteDePerfil(Guid perfilId, Guid componenteId)
    {
        var perfil = Store.Perfiles.FirstOrDefault(p => p.Id == perfilId);
        if (perfil == null) return (false, "Perfil no encontrado.");

        if (!perfil.ComponentesAsignadosIds.Contains(componenteId))
        {
            return (false,
                $"El elemento seleccionado no está asignado directamente al perfil '{perfil.Nombre}'.\n\nEs probable que esté heredado a través de una Familia contenedora (como se observa en el árbol Jerarquía Composite). Para quitarlo, desvinculá la Familia o quitalo desde la Familia.");
        }

        perfil.ComponentesAsignadosIds.Remove(componenteId);
        Guardar();
        return (true, $"Desvinculado correctamente del perfil '{perfil.Nombre}'.");
    }

    public (bool Ok, string Mensaje) DesvincularComponenteDeFamilia(Guid familiaPadreId, Guid componenteHijoId)
    {
        var famDto = Store.Familias.FirstOrDefault(f => f.Id == familiaPadreId);
        if (famDto == null) return (false, "Familia no encontrada.");

        if (!famDto.HijosIds.Contains(componenteHijoId))
            return (false, $"El elemento seleccionado no se encuentra asignado directamente dentro de la familia '{famDto.Nombre}'.");

        famDto.HijosIds.Remove(componenteHijoId);
        Guardar();
        return (true, $"Desvinculado correctamente de la familia '{famDto.Nombre}'.");
    }

    public (bool Ok, string Mensaje) EliminarPerfil(Guid perfilId, IEnumerable<AdminUsuarioBE> usuariosActuales)
    {
        var perfil = Store.Perfiles.FirstOrDefault(p => p.Id == perfilId);
        if (perfil == null) return (false, "Perfil no encontrado.");

        if (string.Equals(perfil.Nombre, "Administrador", StringComparison.OrdinalIgnoreCase))
            return (false, "El perfil 'Administrador' principal del sistema no puede eliminarse.");

        bool tieneUsuarios = Store.PerfilPorUsuario.Values.Any(pid => pid == perfilId) ||
                             usuariosActuales.Any(u => u.PerfilId == perfilId);

        if (tieneUsuarios)
            return (false, "No se puede eliminar el perfil porque hay usuarios asignados a este rol (los usuarios no pueden quedar sin perfil).");

        Store.Perfiles.RemoveAll(p => p.Id == perfilId);
        Guardar();
        return (true, $"¡Perfil '{perfil.Nombre}' eliminado del sistema con éxito!");
    }

    public (bool Ok, string Mensaje) EliminarFamilia(Guid familiaId)
    {
        var fam = Store.Familias.FirstOrDefault(f => f.Id == familiaId);
        if (fam == null) return (false, "Familia no encontrada.");

        foreach (var p in Store.Perfiles)
            p.ComponentesAsignadosIds.RemoveAll(id => id == familiaId);

        foreach (var otraFam in Store.Familias)
            otraFam.HijosIds.RemoveAll(id => id == familiaId);

        Store.Familias.RemoveAll(f => f.Id == familiaId);
        Guardar();
        return (true, $"¡Familia '{fam.Nombre}' eliminada y desvinculada en cascada con éxito!");
    }

    public (bool Ok, string Mensaje) EliminarPermisoBoton(Guid permisoId)
    {
        var perm = Store.Permisos.FirstOrDefault(p => p.Id == permisoId);
        if (perm == null) return (false, "Botón no encontrado.");

        foreach (var p in Store.Perfiles)
            p.ComponentesAsignadosIds.RemoveAll(id => id == permisoId);

        foreach (var f in Store.Familias)
            f.HijosIds.RemoveAll(id => id == permisoId);

        Store.Permisos.RemoveAll(p => p.Id == permisoId);
        Guardar();
        return (true, $"¡Botón '{perm.Nombre}' eliminado y desvinculado de todos los Perfiles y Familias!");
    }

    // --- ÁRBOLES VISUALES JERÁRQUICOS (Idéntico a Perfiles.cs de ING-SOFTWARE-BritezG) ---

    public TreeNode ConstruirArbolPerfil(Guid perfilId)
    {
        var perfil = Store.Perfiles.FirstOrDefault(p => p.Id == perfilId);
        var grafo = ConstruirGrafoComposite();

        if (perfil == null)
            return new TreeNode("Seleccione un Perfil o Familia");

        var raiz = new TreeNode($"👤 Perfil: {perfil.Nombre}") { Tag = perfil.Id };
        foreach (var compId in perfil.ComponentesAsignadosIds)
        {
            if (grafo.TryGetValue(compId, out var comp))
            {
                raiz.Nodes.Add(CrearNodoRecursivo(comp));
            }
        }
        return raiz;
    }

    public TreeNode ConstruirArbolFamilia(Guid familiaId)
    {
        var grafo = ConstruirGrafoComposite();
        if (!grafo.TryGetValue(familiaId, out var comp) || !comp.EsFamilia)
            return new TreeNode("Seleccione una Familia");

        var raiz = new TreeNode($"📦 Familia: {comp.Nombre}") { Tag = comp.Id };
        foreach (var hijo in comp.ObtenerHijos())
        {
            raiz.Nodes.Add(CrearNodoRecursivo(hijo));
        }
        return raiz;
    }

    private static TreeNode CrearNodoRecursivo(ComponentePermiso comp)
    {
        if (comp.EsFamilia)
        {
            var nodoFam = new TreeNode($"📦 {comp.Nombre}") { Tag = comp.Id };
            foreach (var hijo in comp.ObtenerHijos())
            {
                nodoFam.Nodes.Add(CrearNodoRecursivo(hijo));
            }
            return nodoFam;
        }

        return new TreeNode($"🔘 {comp.Nombre}") { Tag = comp.Id };
    }

    // --- RESOLUCIÓN DE PERFIL Y BOTONES HABILITADOS POR USUARIO ---

    public PerfilBE? ObtenerPerfilDeUsuario(string? username, Guid? perfilIdExplicito = null)
    {
        if (perfilIdExplicito.HasValue)
        {
            var porId = Store.Perfiles.FirstOrDefault(p => p.Id == perfilIdExplicito.Value);
            if (porId != null) return porId;
        }

        if (!string.IsNullOrWhiteSpace(username) && Store.PerfilPorUsuario.TryGetValue(username.Trim(), out var pid))
        {
            var encontrado = Store.Perfiles.FirstOrDefault(p => p.Id == pid);
            if (encontrado != null) return encontrado;
        }

        if (string.Equals(username?.Trim(), "Admin", StringComparison.OrdinalIgnoreCase))
        {
            return Store.Perfiles.FirstOrDefault(p =>
                string.Equals(p.Nombre, "Administrador", StringComparison.OrdinalIgnoreCase))
                ?? Store.Perfiles.FirstOrDefault();
        }

        return null;
    }

    public void AsignarPerfilAUsuario(string username, Guid perfilId, int dni = 11111111, string nombre = "", string apellido = "", bool bloqueado = false)
    {
        if (string.IsNullOrWhiteSpace(username)) return;
        string clean = username.Trim();
        Store.PerfilPorUsuario[clean] = perfilId;

        var ficha = Store.FichasUsuarios.FirstOrDefault(f => string.Equals(f.Username, clean, StringComparison.OrdinalIgnoreCase));
        if (ficha == null)
        {
            Store.FichasUsuarios.Add(new UsuarioFichaExtendidaDto
            {
                Username = clean,
                Dni = dni,
                Nombre = nombre,
                Apellido = apellido,
                Bloqueado = bloqueado,
                PerfilId = perfilId
            });
        }
        else
        {
            ficha.Dni = dni;
            ficha.Nombre = nombre;
            ficha.Apellido = apellido;
            ficha.Bloqueado = bloqueado;
            ficha.PerfilId = perfilId;
        }

        Guardar();
    }

    public HashSet<string> ObtenerBotonesPermitidosUsuario(string? username, Guid? perfilIdExplicito = null)
    {
        var efectivos = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var perfil = ObtenerPerfilDeUsuario(username, perfilIdExplicito);
        if (perfil == null) return efectivos;

        var grafo = ConstruirGrafoComposite();
        foreach (var compId in perfil.ComponentesAsignadosIds)
        {
            if (grafo.TryGetValue(compId, out var comp))
            {
                foreach (var cod in comp.ObtenerCodigosEfectivos())
                {
                    efectivos.Add(cod);
                }
            }
        }

        return efectivos;
    }

    public bool TienePermiso(string? username, string nombreBoton, Guid? perfilIdExplicito = null)
    {
        if (string.IsNullOrWhiteSpace(nombreBoton)) return false;
        return ObtenerBotonesPermitidosUsuario(username, perfilIdExplicito).Contains(nombreBoton.Trim());
    }
}
