using System;
using System.Collections.Generic;
using System.Linq;

namespace AdminImportaPlus.BE;

/// <summary>
/// Patrón de Diseño GoF: COMPOSITE (Componente Base Abstracto)
/// Regla de Dominio Intuitiva: Un Permiso Simple (Hoja) ES UN BOTÓN real de la interfaz.
/// En la lista de permisos únicamente aparecen BOTONES; si un Perfil posee dicho permiso
/// (de forma directa o heredado por una Familia), el usuario con ese Perfil tiene habilitado/visible ese botón.
/// </summary>
public abstract class ComponentePermiso
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Codigo { get; set; } = string.Empty; // Exactamente el Control.Name del Botón (ej: btnGuardarFwd) o código de Familia
    public string Nombre { get; set; } = string.Empty; // Etiqueta legible del Botón o de la Familia
    public string NombreFormulario { get; set; } = "FrmPanelAdmin";
    public abstract bool EsFamilia { get; }

    public abstract IReadOnlyList<ComponentePermiso> ObtenerHijos();
    public abstract HashSet<string> ObtenerCodigosEfectivos();
    public abstract void Agregar(ComponentePermiso hijo);
    public abstract void Remover(ComponentePermiso hijo);

    public bool ContienePermiso(string codigoBoton) =>
        ObtenerCodigosEfectivos().Contains((codigoBoton ?? string.Empty).Trim());

    public override string ToString() => EsFamilia ? $"[Familia] {Nombre}" : Nombre;
}

/// <summary>
/// Hoja (Leaf) del Patrón Composite: representa un BOTÓN concreto del sistema.
/// </summary>
public sealed class PermisoSimple : ComponentePermiso
{
    public override bool EsFamilia => false;

    public override IReadOnlyList<ComponentePermiso> ObtenerHijos() => Array.Empty<ComponentePermiso>();

    public override HashSet<string> ObtenerCodigosEfectivos() =>
        new(StringComparer.OrdinalIgnoreCase) { Codigo.Trim() };

    public override void Agregar(ComponentePermiso hijo) =>
        throw new InvalidOperationException("Un botón (permiso hoja) no puede contener hijos.");

    public override void Remover(ComponentePermiso hijo) =>
        throw new InvalidOperationException("Un botón (permiso hoja) no contiene hijos.");
}

/// <summary>
/// Compuesto (Composite): Familia que agrupa Botones (Permisos Simples) y/o Sub-Familias.
/// </summary>
public sealed class FamiliaPermisos : ComponentePermiso
{
    private readonly List<ComponentePermiso> _hijos = new();

    public override bool EsFamilia => true;

    public override IReadOnlyList<ComponentePermiso> ObtenerHijos() => _hijos.AsReadOnly();

    public override HashSet<string> ObtenerCodigosEfectivos()
    {
        var set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var hijo in _hijos)
        {
            foreach (var cod in hijo.ObtenerCodigosEfectivos())
            {
                set.Add(cod);
            }
        }
        return set;
    }

    public override void Agregar(ComponentePermiso hijo)
    {
        if (hijo == null) throw new ArgumentNullException(nameof(hijo));
        if (hijo.Id == Id)
            throw new InvalidOperationException("Error de recursividad: Una familia no puede contenerse a sí misma.");

        if (hijo is FamiliaPermisos subFamilia && subFamilia.ContieneComponenteId(Id))
            throw new InvalidOperationException($"Bucle detectado: No puede agregar '{subFamilia.Nombre}' dentro de '{Nombre}' porque '{Nombre}' ya está dentro de '{subFamilia.Nombre}'.");

        var actuales = ObtenerCodigosEfectivos();
        var nuevos = hijo.ObtenerCodigosEfectivos();
        var duplicados = actuales.Intersect(nuevos, StringComparer.OrdinalIgnoreCase).ToList();
        if (duplicados.Count > 0)
            throw new InvalidOperationException(
                $"Regla de unicidad: la familia '{Nombre}' ya contiene el/los botón(es): {string.Join(", ", duplicados)}.");

        _hijos.Add(hijo);
    }

    public override void Remover(ComponentePermiso hijo)
    {
        _hijos.RemoveAll(h => h.Id == hijo.Id);
    }

    public bool ContieneComponenteId(Guid idBuscado)
    {
        if (Id == idBuscado) return true;
        foreach (var h in _hijos)
        {
            if (h.Id == idBuscado) return true;
            if (h is FamiliaPermisos fam && fam.ContieneComponenteId(idBuscado)) return true;
        }
        return false;
    }
}

/// <summary>
/// Perfil de Usuario (Rol): agrupa Botones (Permisos) y/o Familias de Botones garantizando
/// que ningún botón pueda asignarse dos veces al mismo perfil.
/// </summary>
public sealed class PerfilBE
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Nombre { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public List<Guid> ComponentesAsignadosIds { get; set; } = new();

    public override string ToString() => Nombre;
}

/// <summary>
/// Fila visual para las 3 grillas de Gestión de Perfiles (idéntico al modelo de ING-SOFTWARE-BritezG).
/// </summary>
public sealed class ItemGrillaRbacBE
{
    public Guid Id { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
}

/// <summary>
/// DTOs planos para persistencia JSON / Supabase del árbol Composite (Botones, Familias, Perfiles y Metadatos de Usuario).
/// </summary>
public sealed class PermisoDto
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Codigo { get; set; } = string.Empty; // Nombre exacto del control Botón (ej: btnGuardarFwd)
    public string Nombre { get; set; } = string.Empty; // Texto descriptivo del Botón en la grilla
    public string NombreFormulario { get; set; } = "FrmPanelAdmin";
}

public sealed class FamiliaDto
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public List<Guid> HijosIds { get; set; } = new();
}

public sealed class UsuarioFichaExtendidaDto
{
    public string Username { get; set; } = string.Empty;
    public int Dni { get; set; } = 11111111;
    public string Nombre { get; set; } = string.Empty;
    public string Apellido { get; set; } = string.Empty;
    public bool Bloqueado { get; set; }
    public Guid PerfilId { get; set; }
}

public sealed class RolesStoreBE
{
    public int SchemaVersion { get; set; } = 2; // Versión 2: Permiso == Botón exclusivamente
    public List<PermisoDto> Permisos { get; set; } = new();
    public List<FamiliaDto> Familias { get; set; } = new();
    public List<PerfilBE> Perfiles { get; set; } = new();
    public Dictionary<string, Guid> PerfilPorUsuario { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    public List<UsuarioFichaExtendidaDto> FichasUsuarios { get; set; } = new();
}
