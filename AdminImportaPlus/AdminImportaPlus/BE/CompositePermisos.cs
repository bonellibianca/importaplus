using System;
using System.Collections.Generic;
using System.Linq;

namespace AdminImportaPlus.BE;

/// <summary>
/// Patrón de Diseño GoF: COMPOSITE (Componente Base Abstracto)
/// Permite tratar de forma uniforme tanto a un Permiso Simple (Hoja) como a una Familia de Permisos (Compuesto).
/// </summary>
public abstract class ComponentePermiso
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public abstract bool EsFamilia { get; }

    public abstract IReadOnlyList<ComponentePermiso> ObtenerHijos();
    public abstract HashSet<string> ObtenerCodigosEfectivos();
    public abstract void Agregar(ComponentePermiso hijo);
    public abstract void Remover(ComponentePermiso hijo);

    public bool ContienePermiso(string codigo) =>
        ObtenerCodigosEfectivos().Contains((codigo ?? string.Empty).Trim().ToUpperInvariant());

    public override string ToString() => EsFamilia ? $"[Familia] {Nombre}" : $"{Nombre} ({Codigo})";
}

/// <summary>
/// Hoja (Leaf) del Patrón Composite: representa un permiso atómico individual del sistema.
/// </summary>
public sealed class PermisoSimple : ComponentePermiso
{
    public override bool EsFamilia => false;

    public override IReadOnlyList<ComponentePermiso> ObtenerHijos() => Array.Empty<ComponentePermiso>();

    public override HashSet<string> ObtenerCodigosEfectivos() =>
        new(StringComparer.OrdinalIgnoreCase) { Codigo.Trim().ToUpperInvariant() };

    public override void Agregar(ComponentePermiso hijo) =>
        throw new InvalidOperationException("Un permiso simple (hoja) no puede contener hijos.");

    public override void Remover(ComponentePermiso hijo) =>
        throw new InvalidOperationException("Un permiso simple (hoja) no contiene hijos.");
}

/// <summary>
/// Compuesto (Composite): agrupa permisos simples y/o sub-familias de permisos.
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
            throw new InvalidOperationException("Una familia no puede agregarse a sí misma.");

        if (hijo is FamiliaPermisos subFamilia && subFamilia.ContieneComponenteId(Id))
            throw new InvalidOperationException($"Ciclo detectado: la familia '{subFamilia.Nombre}' ya contiene a '{Nombre}'.");

        var actuales = ObtenerCodigosEfectivos();
        var nuevos = hijo.ObtenerCodigosEfectivos();
        var duplicados = actuales.Intersect(nuevos, StringComparer.OrdinalIgnoreCase).ToList();
        if (duplicados.Count > 0)
            throw new InvalidOperationException(
                $"Regla de unicidad: la familia '{Nombre}' ya contiene el/los permiso(s): {string.Join(", ", duplicados)}.");

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
/// Perfil de Usuario (Rol): agrupa Permisos Simples y/o Familias de Permisos garantizando
/// estrictamente que NINGÚN permiso atómico pueda asignarse dos veces al mismo perfil.
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
/// DTOs planos para persistencia JSON del árbol Composite (Permisos, Familias y Perfiles).
/// </summary>
public sealed class PermisoDto
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
}

public sealed class FamiliaDto
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public List<Guid> HijosIds { get; set; } = new();
}

public sealed class RolesStoreBE
{
    public List<PermisoDto> Permisos { get; set; } = new();
    public List<FamiliaDto> Familias { get; set; } = new();
    public List<PerfilBE> Perfiles { get; set; } = new();
    public Dictionary<string, Guid> PerfilPorUsuario { get; set; } = new(StringComparer.OrdinalIgnoreCase);
}
