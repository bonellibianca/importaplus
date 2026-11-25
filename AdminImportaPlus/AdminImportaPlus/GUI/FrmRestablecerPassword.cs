using System.Windows.Forms;
using AdminImportaPlus.BLL;

namespace AdminImportaPlus.GUI;

public partial class FrmRestablecerPassword : Form
{
    private readonly AdminGestorBLL? _gestor;

    public FrmRestablecerPassword()
    {
        InitializeComponent();
    }

    public FrmRestablecerPassword(AdminGestorBLL gestor, string usuarioInicial = "Admin") : this()
    {
        _gestor = gestor;
        txtUsuario.Text = usuarioInicial;
        btnGuardar.Click += async (_, _) => await GuardarNuevaPasswordAsync();
    }

    private async System.Threading.Tasks.Task GuardarNuevaPasswordAsync()
    {
        if (_gestor == null) return;
        btnGuardar.Enabled = false;
        try
        {
            var res = await _gestor.RestablecerPasswordAdminAsync(
                txtUsuario.Text,
                txtNuevaPassword.Text,
                txtConfirmarPassword.Text);

            if (!res.Ok)
            {
                MessageBox.Show(res.Mensaje, "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            MessageBox.Show(res.Mensaje, "Contraseña Actualizada", MessageBoxButtons.OK, MessageBoxIcon.Information);
            DialogResult = DialogResult.OK;
            Close();
        }
        finally
        {
            btnGuardar.Enabled = true;
        }
    }
}
