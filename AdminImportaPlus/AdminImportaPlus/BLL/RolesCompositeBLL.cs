using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Windows.Forms;
using AdminImportaPlus.BE;

namespace AdminImportaPlus.BLL;

/// <summary>
/// Capa BLL: Gestión de Roles, Permisos, Familias y Perfiles con Patrón GoF COMPOSITE.
/// Aplica la regla estricta de negocio que impide asignar un mismo permiso dos veces a un Perfil
/// (ya sea de forma directa o heredado mediante una Familia de Permisos).
/// </summary>
public sealed class RolesCompositeBLL
{
    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    private readonly string _rolesFilePath;
    public RolesStoreBE Store { get; private set; }

    public RolesCompositeBLL(string dataFolder)
    {
        Directory.CreateDirectory(dataFolder);
        _rolesFilePath = Path.Combine(dataFolder, "roles_composite.json");
        Store = CargarOCrearSemilla();
    }

    private RolesStoreBE CargarOCrearSemilla()
    {
        if (File.Exists(_rolesFilePath))
        {
            try
            {
                var loaded = JsonSerializer.Deserialize<RolesStoreBE>(File.ReadAllText(_rolesFilePath, Encoding.UTF8), JsonOpts);
                if (loaded != null && loaded.Permisos.Count > 0)
                    return loaded;
            }
            catch { }
        }

        var pFwdVer = new PermisoDto { Id = Guid.Parse("10000001-0000-0000-0000-000000000001"), Codigo = "FWD_VER", Nombre = "Ver Forwarders" };
        var pFwdAlta = new PermisoDto { Id = Guid.Parse("10000001-0000-0000-0000-000000000002"), Codigo = "FWD_ALTA", Nombre = "Alta de Forwarders" };
        var pFwdMod = new PermisoDto { Id = Guid.Parse("10000001-0000-0000-0000-000000000003"), Codigo = "FWD_MODIFICAR", Nombre = "Modificar Forwarders" };
        var pFwdBaja = new PermisoDto { Id = Guid.Parse("10000001-0000-0000-0000-000000000004"), Codigo = "FWD_BAJA", Nombre = "Eliminar Forwarders" };

        var pPtoAbm = new PermisoDto { Id = Guid.Parse("10000001-0000-0000-0000-000000000005"), Codigo = "PTO_ABM", Nombre = "ABM Puertos y Aduanas" };
        var pDvVer = new PermisoDto { Id = Guid.Parse("10000001-0000-0000-0000-000000000006"), Codigo = "DV_VER", Nombre = "Auditar Dígito Verificador" };
        var pDvRecalc = new PermisoDto { Id = Guid.Parse("10000001-0000-0000-0000-000000000007"), Codigo = "DV_RECALCULAR", Nombre = "Recalcular Dígito Verificador" };

        var pBkCrear = new PermisoDto { Id = Guid.Parse("10000001-0000-0000-0000-000000000008"), Codigo = "BACKUP_CREAR", Nombre = "Generar Backup" };
        var pBkRestore = new PermisoDto { Id = Guid.Parse("10000001-0000-0000-0000-000000000009"), Codigo = "BACKUP_RESTAURAR", Nombre = "Ejecutar RestoreBackup" };
        var pSupaCfg = new PermisoDto { Id = Guid.Parse("10000001-0000-0000-0000-000000000010"), Codigo = "SUPA_CONFIG", Nombre = "Configurar Supabase Cloud" };
        var pRoles = new PermisoDto { Id = Guid.Parse("10000001-0000-0000-0000-000000000011"), Codigo = "ROLES_GESTION", Nombre = "Gestionar Roles y Perfiles" };

        var famFwd = new FamiliaDto
        {
            Id = Guid.Parse("20000001-0000-0000-0000-000000000001"),
            Codigo = "FAM_FORWARDERS",
            Nombre = "Gestión de Forwarders",
            HijosIds = new() { pFwdVer.Id, pFwdAlta.Id, pFwdMod.Id, pFwdBaja.Id }
        };

        var famSeguridad = new FamiliaDto
        {
            Id = Guid.Parse("20000001-0000-0000-0000-000000000002"),
            Codigo = "FAM_SEGURIDAD",
            Nombre = "Seguridad e Integridad DV",
            HijosIds = new() { pDvVer.Id, pDvRecalc.Id, pRoles.Id }
        };

        var famInfra = new FamiliaDto
        {
            Id = Guid.Parse("20000001-0000-0000-0000-000000000003"),
            Codigo = "FAM_BACKUP_NUBE",
            Nombre = "Backup y Conectividad Nube",
            HijosIds = new() { pBkCrear.Id, pBkRestore.Id, pSupaCfg.Id }
        };

        var perfilAdmin = new PerfilBE
        {
            Id = Guid.Parse("30000001-0000-0000-0000-000000000001"),
            Nombre = "Administrador Total",
            Descripcion = "Acceso integral a Forwarders, Puertos, Roles, DVH/DVV y RestoreBackup.",
            ComponentesAsignadosIds = new() { famFwd.Id, pPtoAbm.Id, famSeguridad.Id, famInfra.Id }
        };

        var perfilOperador = new PerfilBE
        {
            Id = Guid.Parse("30000001-0000-0000-0000-000000000002"),
            Nombre = "Operador Logístico",
            Descripcion = "Gestiona Forwarders y Puertos sin acceso a seguridad ni restore.",
            ComponentesAsignadosIds = new() { famFwd.Id, pPtoAbm.Id }
        };

        var semilla = new RolesStoreBE
        {
            Permisos = new() { pFwdVer, pFwdAlta, pFwdMod, pFwdBaja, pPtoAbm, pDvVer, pDvRecalc, pBkCrear, pBkRestore, pSupaCfg, pRoles },
            Familias = new() { famFwd, famSeguridad, famInfra },
            Perfiles = new() { perfilAdmin, perfilOperador },
            PerfilPorUsuario = new(StringComparer.OrdinalIgnoreCase) { ["Admin"] = perfilAdmin.Id }
        };

        Store = semilla;
        Guardar();
        return semilla;
    }

    public void Guardar()
    {
        File.WriteAllText(_rolesFilePath, JsonSerializer.Serialize(Store, JsonOpts), Encoding.UTF8);
    }

    public Dictionary<Guid, ComponentePermiso> ConstruirGrafoComposite()
    {
        var mapa = new Dictionary<Guid, ComponentePermiso>();

        foreach (var p in Store.Permisos)
        {
            mapa[p.Id] = new PermisoSimple { Id = p.Id, Codigo = p.Codigo, Nombre = p.Nombre };
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

    public (bool Ok, string Mensaje) CrearPermiso(string codigo, string nombre)
    {
        codigo = (codigo ?? string.Empty).Trim().ToUpperInvariant().Replace(" ", "_");
        nombre = (nombre ?? string.Empty).Trim();

        if (string.IsNullOrWhiteSpace(codigo) || string.IsNullOrWhiteSpace(nombre))
            return (false, "Ingresá el código y el nombre del permiso.");

        if (Store.Permisos.Any(p => string.Equals(p.Codigo, codigo, StringComparison.OrdinalIgnoreCase)))
            return (false, $"Ya existe un permiso con el código '{codigo}'.");

        Store.Permisos.Add(new PermisoDto { Id = Guid.NewGuid(), Codigo = codigo, Nombre = nombre });
        Guardar();
        return (true, $"Permiso '{nombre} ({codigo})' creado correctamente.");
    }

    public (bool Ok, string Mensaje) CrearFamilia(string nombre)
    {
        nombre = (nombre ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(nombre))
            return (false, "Ingresá un nombre para la familia de permisos.");

        if (Store.Familias.Any(f => string.Equals(f.Nombre, nombre, StringComparison.OrdinalIgnoreCase)))
            return (false, $"Ya existe una familia llamada '{nombre}'.");

        string codigo = "FAM_" + new string(nombre.ToUpperInvariant().Where(c => char.IsLetterOrDigit(c) || c == ' ').ToArray()).Replace(" ", "_");
        Store.Familias.Add(new FamiliaDto { Id = Guid.NewGuid(), Codigo = codigo, Nombre = nombre });
        Guardar();
        return (true, $"Familia '{nombre}' creada correctamente.");
    }

    public (bool Ok, string Mensaje) AgregarComponenteAFamilia(Guid familiaId, Guid componenteId)
    {
        var famDto = Store.Familias.FirstOrDefault(f => f.Id == familiaId);
        if (famDto == null) return (false, "Familia no encontrada.");

        var grafo = ConstruirGrafoComposite();
        if (!grafo.TryGetValue(familiaId, out var famComp) || famComp is not FamiliaPermisos fam)
            return (false, "Familia inválida.");

        if (!grafo.TryGetValue(componenteId, out var hijo))
            return (false, "Permiso o sub-familia no encontrado.");

        if (famDto.HijosIds.Contains(componenteId))
            return (false, $"'{hijo.Nombre}' ya pertenece a la familia '{fam.Nombre}'.");

        // Validar regla de unicidad y ciclos dentro de la propia familia
        try
        {
            fam.Agregar(hijo);
        }
        catch (InvalidOperationException ex)
        {
            return (false, ex.Message);
        }

        // Validar que agregar este hijo a la familia NO genere duplicados en ningún Perfil que ya use esta familia
        var nuevosCodigos = hijo.ObtenerCodigosEfectivos();
        foreach (var perfil in Store.Perfiles)
        {
            bool usaEstaFamilia = perfil.ComponentesAsignadosIds.Any(cid =>
                cid == familiaId || (grafo.TryGetValue(cid, out var c) && c is FamiliaPermisos fp && fp.ContieneComponenteId(familiaId)));

            if (!usaEstaFamilia) continue;

            // Verificar permisos existentes en ese perfil (excluyendo la propia familia actual antes del cambio)
            var codigosOtros = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var cid in perfil.ComponentesAsignadosIds)
            {
                if (cid == familiaId) continue;
                if (grafo.TryGetValue(cid, out var c))
                {
                    foreach (var cod in c.ObtenerCodigosEfectivos()) codigosOtros.Add(cod);
                }
            }

            var conflicto = codigosOtros.Intersect(nuevosCodigos, StringComparer.OrdinalIgnoreCase).ToList();
            if (conflicto.Count > 0)
            {
                return (false,
                    $"Regla de unicidad: No se puede agregar '{hijo.Nombre}' a la familia '{fam.Nombre}' porque el perfil '{perfil.Nombre}' ya posee el permiso ({string.Join(", ", conflicto)}) por otra asignación.");
            }
        }

        famDto.HijosIds.Add(componenteId);
        Guardar();
        return (true, $"Se agregó '{hijo.Nombre}' a la familia '{fam.Nombre}'.");
    }

    public (bool Ok, string Mensaje) QuitarComponenteDeFamilia(Guid familiaId, Guid componenteId)
    {
        var famDto = Store.Familias.FirstOrDefault(f => f.Id == familiaId);
        if (famDto == null) return (false, "Familia no encontrada.");
        famDto.HijosIds.Remove(componenteId);
        Guardar();
        return (true, "Componente quitado de la familia.");
    }

    public (bool Ok, string Mensaje) CrearPerfil(string nombre, string descripcion = "")
    {
        nombre = (nombre ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(nombre))
            return (false, "Ingresá el nombre del perfil.");

        if (Store.Perfiles.Any(p => string.Equals(p.Nombre, nombre, StringComparison.OrdinalIgnoreCase)))
            return (false, $"Ya existe un perfil llamado '{nombre}'.");

        Store.Perfiles.Add(new PerfilBE { Id = Guid.NewGuid(), Nombre = nombre, Descripcion = (descripcion ?? string.Empty).Trim() });
        Guardar();
        return (true, $"Perfil '{nombre}' creado correctamente.");
    }

    /// <summary>
    /// REGLA ESTRICTA: Impide asignar un permiso (simple o contenido dentro de una familia)
    /// dos veces al mismo Perfil, ya sea de forma directa o heredada.
    /// </summary>
    public (bool Ok, string Mensaje) AsignarComponenteAPerfil(Guid perfilId, Guid componenteId)
    {
        var perfil = Store.Perfiles.FirstOrDefault(p => p.Id == perfilId);
        if (perfil == null) return (false, "Perfil no encontrado.");

        var grafo = ConstruirGrafoComposite();
        if (!grafo.TryGetValue(componenteId, out var candidato))
            return (false, "Permiso o familia no encontrado.");

        if (perfil.ComponentesAsignadosIds.Contains(componenteId))
            return (false, $"Regla de unicidad: '{candidato.Nombre}' ya se encuentra asignado al perfil '{perfil.Nombre}'.");

        // Obtener TODOS los permisos efectivos ya presentes en el perfil (directos + heredados de familias)
        var permisosActualesPerfil = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var asignadoId in perfil.ComponentesAsignadosIds)
        {
            if (!grafo.TryGetValue(asignadoId, out var compAsignado)) continue;
            foreach (var cod in compAsignado.ObtenerCodigosEfectivos())
            {
                permisosActualesPerfil.TryAdd(cod, compAsignado.Nombre);
            }
        }

        // Comparar contra los permisos efectivos del candidato
        var codigosCandidato = candidato.ObtenerCodigosEfectivos();
        if (candidato.EsFamilia && codigosCandidato.Count == 0)
            return (false, $"La familia '{candidato.Nombre}' está vacía. Agregale al menos un permiso antes de asignarla.");

        var conflictos = new List<string>();
        foreach (var cod in codigosCandidato)
        {
            if (permisosActualesPerfil.TryGetValue(cod, out var origenExistente))
            {
                conflictos.Add($"{cod} (ya otorgado por '{origenExistente}')");
            }
        }

        if (conflictos.Count > 0)
        {
            return (false,
                $"Regla anti-duplicados violada: No se puede asignar '{candidato.Nombre}' al perfil '{perfil.Nombre}' porque duplicaría el/los permiso(s): {string.Join("; ", conflictos)}.");
        }

        perfil.ComponentesAsignadosIds.Add(componenteId);
        Guardar();
        return (true, $"'{candidato.Nombre}' asignado correctamente al perfil '{perfil.Nombre}'.");
    }

    public (bool Ok, string Mensaje) QuitarComponenteDePerfil(Guid perfilId, Guid componenteId)
    {
        var perfil = Store.Perfiles.FirstOrDefault(p => p.Id == perfilId);
        if (perfil == null) return (false, "Perfil no encontrado.");
        if (perfil.ComponentesAsignadosIds.Remove(componenteId))
        {
            Guardar();
            return (true, "Asignación removida del perfil.");
        }
        return (false, "Seleccioná un nodo raíz asignado directamente al perfil para quitarlo.");
    }

    /// <summary>
    /// Construye el árbol visual jerárquico para el TreeView del perfil seleccionado.
    /// </summary>
    public TreeNode ConstruirArbolPerfil(Guid perfilId)
    {
        var perfil = Store.Perfiles.FirstOrDefault(p => p.Id == perfilId);
        var grafo = ConstruirGrafoComposite();

        if (perfil == null)
            return new TreeNode("Sin perfil seleccionado");

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

    private static TreeNode CrearNodoRecursivo(ComponentePermiso comp)
    {
        if (comp.EsFamilia)
        {
            var nodoFam = new TreeNode($"📁 Familia: {comp.Nombre}") { Tag = comp.Id };
            foreach (var hijo in comp.ObtenerHijos())
            {
                nodoFam.Nodes.Add(CrearNodoRecursivo(hijo));
            }
            return nodoFam;
        }

        return new TreeNode($"🔑 {comp.Nombre} [{comp.Codigo}]") { Tag = comp.Id };
    }
}
