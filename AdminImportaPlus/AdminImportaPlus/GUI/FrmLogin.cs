using System;
using System.Windows.Forms;
using AdminImportaPlus.BLL;

namespace AdminImportaPlus.GUI;

public partial class FrmLogin : Form
{
    private readonly AdminGestorBLL _gestor;

    public FrmLogin()
    {
        InitializeComponent();
        _gestor = new AdminGestorBLL();
        btnIngresar.Click += async (_, _) => await EjecutarLoginAsync();
    }

    private async System.Threading.Tasks.Task EjecutarLoginAsync()
    {
        btnIngresar.Enabled = false;
        btnIngresar.Text = "Verificando...";

        try
        {
            var (exito, mensaje, verificacionDV) = await _gestor.IniciarSesionAsync(
                txtUsuario.Text,
                txtPassword.Text);

            if (!exito || verificacionDV == null)
            {
                MessageBox.Show(mensaje, "Acceso Denegado", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int seccionInicial = 0;

            if (verificacionDV.SaltoDetectado)
            {
                using var frmAlerta = new FrmAlertaDigitoVerificador(_gestor, verificacionDV);
                var decision = frmAlerta.ShowDialog(this);

                if (frmAlerta.AccionElegida == AccionAlertaDV.Cancelar || decision == DialogResult.Cancel)
                {
                    return;
                }

                if (frmAlerta.AccionElegida == AccionAlertaDV.IrARestoreBackup)
                {
                    seccionInicial = FrmPanelAdmin.SeccionRestoreSupabase;
                }
            }

            Hide();
            using (var frmPanel = new FrmPanelAdmin(_gestor, seccionInicial))
            {
                frmPanel.ShowDialog();
            }
            txtPassword.Clear();
            Show();
        }
        catch (Exception ex)
        {
            MessageBox.Show("Error durante el inicio de sesión: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            btnIngresar.Enabled = true;
            btnIngresar.Text = "Ingresar al Panel";
        }
    }
}
