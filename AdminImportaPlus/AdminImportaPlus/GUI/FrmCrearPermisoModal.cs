using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using AdminImportaPlus.BLL;

namespace AdminImportaPlus.GUI;

/// <summary>
/// Diálogo modal adaptado de ING-SOFTWARE-BritezG (FrmCrearPermiso):
/// Soporta alta de Permiso (exclusivamente Botones reales de la interfaz), Familia y Perfil.
/// </summary>
public sealed class FrmCrearPermisoModal : Form
{
    public enum ModoFormulario
    {
        PermisoBoton,
        Perfil,
        Familia
    }

    private sealed class BotonOpcionItem
    {
        public string NombreControl { get; init; } = string.Empty;
        public string EtiquetaLegible { get; init; } = string.Empty;
        public bool YaRegistrado { get; init; }
        public override string ToString() =>
            YaRegistrado ? $"{EtiquetaLegible} ({NombreControl}) [Ya en lista]" : $"{EtiquetaLegible} ({NombreControl})";
    }

    public string NombrePermiso { get; private set; } = string.Empty;
    public string NombreFormulario { get; private set; } = "FrmPanelAdmin";
    public string NombreBoton { get; private set; } = string.Empty;
    public bool TieneBotonAsignado { get; private set; }
    public bool IncluirBotonSeleccionadoEnFamilia { get; private set; }

    private readonly ModoFormulario _modoActual;
    private readonly RolesCompositeBLL _roles;

    private readonly Panel _pnlHeader = new();
    private readonly Label _lblTitulo = new();
    private readonly Label _lblSubtitulo = new();

    private readonly Label _lblFormulario = new();
    private readonly ComboBox _cmbFormularios = new();
    private readonly Label _lblBoton = new();
    private readonly ComboBox _cmbBotones = new();
    private readonly Label _lblNombre = new();
    private readonly TextBox _txtNombre = new();
    private readonly CheckBox _chkIncluirBotonInicial = new();

    private readonly AppleButton _btnAceptar = new();
    private readonly AppleButton _btnCancelar = new();

    public FrmCrearPermisoModal(ModoFormulario modo, RolesCompositeBLL roles, string? nombreBotonSeleccionadoGrilla = null)
    {
        _modoActual = modo;
        _roles = roles;

        ConstruirInterfaz(nombreBotonSeleccionadoGrilla);
        ConfigurarModoVisual(nombreBotonSeleccionadoGrilla);
    }

    private void ConstruirInterfaz(string? nombreBotonSeleccionadoGrilla)
    {
        Text = "Gestión de Perfiles y Permisos (Botones)";
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        BackColor = TemaImportaPlus.SurfaceBg;
        ClientSize = new Size(460, 365);
        Font = TemaImportaPlus.FontNormal;

        _pnlHeader.Dock = DockStyle.Top;
        _pnlHeader.Height = 68;
        _pnlHeader.BackColor = TemaImportaPlus.HeaderBurgundy;
        _pnlHeader.Padding = new Padding(20, 12, 20, 10);

        _lblTitulo.AutoSize = true;
        _lblTitulo.Font = new Font("Segoe UI Semibold", 12.5F);
        _lblTitulo.ForeColor = Color.White;
        _lblTitulo.Location = new Point(20, 12);

        _lblSubtitulo.AutoSize = true;
        _lblSubtitulo.Font = new Font("Segoe UI", 8.5F);
        _lblSubtitulo.ForeColor = Color.FromArgb(245, 215, 222);
        _lblSubtitulo.Location = new Point(20, 39);

        _pnlHeader.Controls.Add(_lblTitulo);
        _pnlHeader.Controls.Add(_lblSubtitulo);

        _lblFormulario.AutoSize = true;
        _lblFormulario.Font = TemaImportaPlus.FontEtiqueta;
        _lblFormulario.ForeColor = TemaImportaPlus.TextBody;
        _lblFormulario.Location = new Point(24, 84);
        _lblFormulario.Text = "PANTALLA / FORMULARIO DEL SISTEMA:";

        _cmbFormularios.DropDownStyle = ComboBoxStyle.DropDownList;
        _cmbFormularios.Location = new Point(24, 104);
        _cmbFormularios.Size = new Size(412, 26);
        _cmbFormularios.SelectedIndexChanged += CmbFormularios_SelectedIndexChanged;

        _lblBoton.AutoSize = true;
        _lblBoton.Font = TemaImportaPlus.FontEtiqueta;
        _lblBoton.ForeColor = TemaImportaPlus.TextBody;
        _lblBoton.Location = new Point(24, 142);
        _lblBoton.Text = "BOTÓN A HABILITAR COMO PERMISO (OBLIGATORIO):";

        _cmbBotones.DropDownStyle = ComboBoxStyle.DropDownList;
        _cmbBotones.Location = new Point(24, 162);
        _cmbBotones.Size = new Size(412, 26);
        _cmbBotones.SelectedIndexChanged += CmbBotones_SelectedIndexChanged;

        _lblNombre.AutoSize = true;
        _lblNombre.Font = TemaImportaPlus.FontEtiqueta;
        _lblNombre.ForeColor = TemaImportaPlus.TextBody;
        _lblNombre.Location = new Point(24, 204);
        _lblNombre.Text = "NOMBRE VISIBLE DEL BOTÓN EN LA LISTA DE PERMISOS:";

        _txtNombre.Location = new Point(24, 224);
        _txtNombre.Size = new Size(412, 26);
        _txtNombre.MaxLength = 90;

        _chkIncluirBotonInicial.AutoSize = true;
        _chkIncluirBotonInicial.Font = new Font("Segoe UI", 8.8F);
        _chkIncluirBotonInicial.ForeColor = TemaImportaPlus.TextDark;
        _chkIncluirBotonInicial.Location = new Point(24, 152);
        _chkIncluirBotonInicial.Visible = false;
        if (!string.IsNullOrWhiteSpace(nombreBotonSeleccionadoGrilla))
        {
            _chkIncluirBotonInicial.Text = $"Incluir inicialmente el botón seleccionado: '{nombreBotonSeleccionadoGrilla}'";
            _chkIncluirBotonInicial.Checked = true;
        }

        _btnCancelar.BackColor = Color.White;
        _btnCancelar.BorderColor = TemaImportaPlus.BorderNeutral;
        _btnCancelar.BorderThickness = 1;
        _btnCancelar.CornerRadius = 10;
        _btnCancelar.ForeColor = TemaImportaPlus.TextDark;
        _btnCancelar.HoverBackColor = Color.FromArgb(238, 238, 242);
        _btnCancelar.Location = new Point(224, 294);
        _btnCancelar.Size = new Size(100, 38);
        _btnCancelar.Text = "Cancelar";
        _btnCancelar.Click += (_, _) =>
        {
            DialogResult = DialogResult.Cancel;
            Close();
        };

        _btnAceptar.BackColor = TemaImportaPlus.BrandPrimary;
        _btnAceptar.CornerRadius = 10;
        _btnAceptar.ForeColor = Color.White;
        _btnAceptar.HoverBackColor = TemaImportaPlus.BrandHover;
        _btnAceptar.Location = new Point(332, 294);
        _btnAceptar.Size = new Size(104, 38);
        _btnAceptar.Text = "Guardar";
        _btnAceptar.Click += BtnAceptar_Click;

        Controls.Add(_pnlHeader);
        Controls.Add(_lblFormulario);
        Controls.Add(_cmbFormularios);
        Controls.Add(_lblBoton);
        Controls.Add(_cmbBotones);
        Controls.Add(_lblNombre);
        Controls.Add(_txtNombre);
        Controls.Add(_chkIncluirBotonInicial);
        Controls.Add(_btnCancelar);
        Controls.Add(_btnAceptar);

        AcceptButton = _btnAceptar;
        CancelButton = _btnCancelar;
    }

    private void ConfigurarModoVisual(string? nombreBotonSeleccionadoGrilla)
    {
        switch (_modoActual)
        {
            case ModoFormulario.PermisoBoton:
                _lblTitulo.Text = "Registrar Botón como Permiso";
                _lblSubtitulo.Text = "Un permiso es exclusivamente un botón del sistema.";
                var forms = RolesCompositeBLL.ObtenerFormulariosConBotones();
                _cmbFormularios.DataSource = forms;
                if (forms.Count > 0) _cmbFormularios.SelectedIndex = 0;
                break;

            case ModoFormulario.Perfil:
                _lblTitulo.Text = "Crear Nuevo Perfil (Rol)";
                _lblSubtitulo.Text = "Definí un nuevo rol para asignarle botones o familias de botones.";
                _lblNombre.Text = "NOMBRE DEL NUEVO PERFIL:";
                _txtNombre.PlaceholderText = "Ej: Despachante Senior, Auditor...";
                CompactarSinCombos(mostrarCheckFamilia: false);
                break;

            case ModoFormulario.Familia:
                _lblTitulo.Text = "Crear Nueva Familia de Botones";
                _lblSubtitulo.Text = "Agrupá varios botones para asignarlos juntos a uno o más perfiles.";
                _lblNombre.Text = "NOMBRE DE LA NUEVA FAMILIA:";
                _txtNombre.PlaceholderText = "Ej: Gestión Operativa Aduanera...";
                CompactarSinCombos(mostrarCheckFamilia: !string.IsNullOrWhiteSpace(nombreBotonSeleccionadoGrilla));
                break;
        }
    }

    private void CompactarSinCombos(bool mostrarCheckFamilia)
    {
        _lblFormulario.Visible = false;
        _cmbFormularios.Visible = false;
        _lblBoton.Visible = false;
        _cmbBotones.Visible = false;

        _lblNombre.Location = new Point(24, 90);
        _txtNombre.Location = new Point(24, 112);

        if (mostrarCheckFamilia)
        {
            _chkIncluirBotonInicial.Visible = true;
            _chkIncluirBotonInicial.Location = new Point(24, 150);
            _btnCancelar.Location = new Point(224, 192);
            _btnAceptar.Location = new Point(332, 192);
            ClientSize = new Size(460, 250);
        }
        else
        {
            _chkIncluirBotonInicial.Visible = false;
            _btnCancelar.Location = new Point(224, 162);
            _btnAceptar.Location = new Point(332, 162);
            ClientSize = new Size(460, 220);
        }
    }

    private void CmbFormularios_SelectedIndexChanged(object? sender, EventArgs e)
    {
        if (_cmbFormularios.SelectedItem is not string formSeleccionado)
        {
            _cmbBotones.DataSource = null;
            _cmbBotones.Enabled = false;
            return;
        }

        var codigosExistentes = new HashSet<string>(
            _roles.Store.Permisos.Select(p => p.Codigo),
            StringComparer.OrdinalIgnoreCase);

        var opciones = _roles.ObtenerBotonesDeFormulario(formSeleccionado)
            .Select(b => new BotonOpcionItem
            {
                NombreControl = b.NombreControl,
                EtiquetaLegible = b.EtiquetaLegible,
                YaRegistrado = codigosExistentes.Contains(b.NombreControl)
            })
            .OrderBy(o => o.YaRegistrado)
            .ThenBy(o => o.EtiquetaLegible)
            .ToList();

        _cmbBotones.DataSource = opciones;
        _cmbBotones.Enabled = opciones.Count > 0;

        int idxDisponible = opciones.FindIndex(o => !o.YaRegistrado);
        _cmbBotones.SelectedIndex = idxDisponible >= 0 ? idxDisponible : (opciones.Count > 0 ? 0 : -1);
    }

    private void CmbBotones_SelectedIndexChanged(object? sender, EventArgs e)
    {
        if (_cmbBotones.SelectedItem is BotonOpcionItem item)
        {
            _txtNombre.Text = item.EtiquetaLegible;
        }
    }

    private void BtnAceptar_Click(object? sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(_txtNombre.Text))
        {
            MessageBox.Show("El nombre no puede estar vacío.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            _txtNombre.Focus();
            return;
        }

        if (_modoActual == ModoFormulario.PermisoBoton)
        {
            if (_cmbFormularios.SelectedItem is not string formName || _cmbBotones.SelectedItem is not BotonOpcionItem btnItem)
            {
                MessageBox.Show("Seleccioná la pantalla y el botón que deseás registrar como permiso.", "Selección Obligatoria", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (btnItem.YaRegistrado)
            {
                MessageBox.Show(
                    $"El botón '{btnItem.EtiquetaLegible}' ({btnItem.NombreControl}) ya figura en la lista de Permisos.\n\nPodés seleccionarlo directamente en la columna 'Permisos (Botones)' para asignarlo a un Perfil o Familia.",
                    "Botón ya registrado",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                return;
            }

            NombreFormulario = formName;
            NombreBoton = btnItem.NombreControl;
            TieneBotonAsignado = true;
        }
        else
        {
            TieneBotonAsignado = false;
            IncluirBotonSeleccionadoEnFamilia = _modoActual == ModoFormulario.Familia && _chkIncluirBotonInicial.Visible && _chkIncluirBotonInicial.Checked;
        }

        NombrePermiso = _txtNombre.Text.Trim();
        DialogResult = DialogResult.OK;
        Close();
    }
}
