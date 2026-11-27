using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using AdminImportaPlus.BE;
using AdminImportaPlus.BLL;

namespace AdminImportaPlus.GUI;

/// <summary>
/// Diálogo modal adaptado de ING-SOFTWARE-BritezG (FrmSeleccionarFamilia):
/// Permite vincular o desvincular una Familia Hija dentro de una Familia Padre (Patrón Composite).
/// </summary>
public sealed class FrmSeleccionarFamiliaModal : Form
{
    public Guid IdFamiliaOrigen { get; private set; }
    public Guid IdFamiliaDestino { get; private set; }
    public string NombreFamiliaOrigen { get; private set; } = string.Empty;
    public string NombreFamiliaDestino { get; private set; } = string.Empty;
    public bool EsVinculacion { get; private set; }

    private readonly RolesCompositeBLL _roles;
    private readonly ComboBox _cmbOrigen = new();
    private readonly ComboBox _cmbDestino = new();

    public FrmSeleccionarFamiliaModal(RolesCompositeBLL roles)
    {
        _roles = roles;
        ConstruirInterfaz();
        CargarFamilias();
    }

    private void ConstruirInterfaz()
    {
        Text = "Relación Jerárquica entre Familias";
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        BackColor = TemaImportaPlus.SurfaceBg;
        ClientSize = new Size(460, 295);
        Font = TemaImportaPlus.FontNormal;

        var pnlHeader = new Panel
        {
            Dock = DockStyle.Top,
            Height = 66,
            BackColor = TemaImportaPlus.HeaderBurgundy
        };

        var lblTit = new Label
        {
            AutoSize = true,
            Font = new Font("Segoe UI Semibold", 12F),
            ForeColor = Color.White,
            Location = new Point(20, 12),
            Text = "🔗 Relación Familia a Familia (Composite)"
        };

        var lblSub = new Label
        {
            AutoSize = true,
            Font = new Font("Segoe UI", 8.5F),
            ForeColor = Color.FromArgb(245, 215, 222),
            Location = new Point(20, 38),
            Text = "Vinculá o desvinculá una sub-familia dentro de una familia contenedora."
        };
        pnlHeader.Controls.Add(lblTit);
        pnlHeader.Controls.Add(lblSub);

        var lblOrigen = new Label
        {
            AutoSize = true,
            Font = TemaImportaPlus.FontEtiqueta,
            ForeColor = TemaImportaPlus.TextBody,
            Location = new Point(24, 84),
            Text = "FAMILIA HIJA (ORIGEN A INCLUIR):"
        };

        _cmbOrigen.DropDownStyle = ComboBoxStyle.DropDownList;
        _cmbOrigen.Location = new Point(24, 104);
        _cmbOrigen.Size = new Size(412, 26);

        var lblDestino = new Label
        {
            AutoSize = true,
            Font = TemaImportaPlus.FontEtiqueta,
            ForeColor = TemaImportaPlus.TextBody,
            Location = new Point(24, 148),
            Text = "FAMILIA PADRE (CONTENEDORA DESTINO):"
        };

        _cmbDestino.DropDownStyle = ComboBoxStyle.DropDownList;
        _cmbDestino.Location = new Point(24, 168);
        _cmbDestino.Size = new Size(412, 26);

        var btnVincular = new AppleButton
        {
            BackColor = TemaImportaPlus.BrandPrimary,
            CornerRadius = 10,
            ForeColor = Color.White,
            HoverBackColor = TemaImportaPlus.BrandHover,
            Location = new Point(24, 226),
            Size = new Size(132, 38),
            Text = "Vincular"
        };
        btnVincular.Click += (_, _) => ConfirmarSeleccion(vincular: true);

        var btnDesvincular = new AppleButton
        {
            BackColor = Color.FromArgb(255, 241, 242),
            BorderColor = Color.FromArgb(254, 205, 211),
            BorderThickness = 1,
            CornerRadius = 10,
            ForeColor = Color.FromArgb(159, 18, 57),
            HoverBackColor = Color.FromArgb(254, 226, 226),
            Location = new Point(166, 226),
            Size = new Size(138, 38),
            Text = "Desvincular"
        };
        btnDesvincular.Click += (_, _) => ConfirmarSeleccion(vincular: false);

        var btnCancelar = new AppleButton
        {
            BackColor = Color.White,
            BorderColor = TemaImportaPlus.BorderNeutral,
            BorderThickness = 1,
            CornerRadius = 10,
            ForeColor = TemaImportaPlus.TextDark,
            HoverBackColor = Color.FromArgb(238, 238, 242),
            Location = new Point(314, 226),
            Size = new Size(122, 38),
            Text = "Cancelar"
        };
        btnCancelar.Click += (_, _) =>
        {
            DialogResult = DialogResult.Cancel;
            Close();
        };

        Controls.Add(pnlHeader);
        Controls.Add(lblOrigen);
        Controls.Add(_cmbOrigen);
        Controls.Add(lblDestino);
        Controls.Add(_cmbDestino);
        Controls.Add(btnVincular);
        Controls.Add(btnDesvincular);
        Controls.Add(btnCancelar);

        CancelButton = btnCancelar;
    }

    private void CargarFamilias()
    {
        var familias = _roles.ObtenerFamiliasGrilla();

        _cmbOrigen.DisplayMember = nameof(ItemGrillaRbacBE.Nombre);
        _cmbOrigen.ValueMember = nameof(ItemGrillaRbacBE.Id);
        _cmbOrigen.DataSource = familias.ToList();
        _cmbOrigen.SelectedIndex = -1;

        _cmbDestino.DisplayMember = nameof(ItemGrillaRbacBE.Nombre);
        _cmbDestino.ValueMember = nameof(ItemGrillaRbacBE.Id);
        _cmbDestino.DataSource = familias.ToList();
        _cmbDestino.SelectedIndex = -1;
    }

    private void ConfirmarSeleccion(bool vincular)
    {
        if (_cmbOrigen.SelectedItem is not ItemGrillaRbacBE origen || _cmbDestino.SelectedItem is not ItemGrillaRbacBE destino)
        {
            MessageBox.Show("Seleccioná tanto la Familia Hija (Origen) como la Familia Padre (Destino).", "Selección Requerida", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (origen.Id == destino.Id)
        {
            MessageBox.Show("Una familia no puede vincularse ni desvincularse consigo misma.", "Validación Composite", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        IdFamiliaOrigen = origen.Id;
        IdFamiliaDestino = destino.Id;
        NombreFamiliaOrigen = origen.Nombre;
        NombreFamiliaDestino = destino.Nombre;
        EsVinculacion = vincular;

        DialogResult = DialogResult.OK;
        Close();
    }
}
