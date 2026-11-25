using System;
using System.Windows.Forms;
using AdminImportaPlus.BE;
using AdminImportaPlus.BLL;

namespace AdminImportaPlus.GUI;

public partial class FrmAlertaDigitoVerificador : Form
{
    private readonly AdminGestorBLL? _gestor;

    public AccionAlertaDV AccionElegida { get; private set; } = AccionAlertaDV.Cancelar;

    public FrmAlertaDigitoVerificador()
    {
        InitializeComponent();
    }

    public FrmAlertaDigitoVerificador(AdminGestorBLL gestor, ResultadoVerificacionDV resultadoDV) : this()
    {
        _gestor = gestor;
        lblSubtituloCambios.Text = $"Detalle de alteraciones detectadas ({resultadoDV.Alteraciones.Count} inconsistencia/s encontrada/s):";
        lblCambiosDetectados.Text = resultadoDV.ObtenerTextoLabelCambios();

        btnRecalcular.Click += async (_, _) => await EjecutarRecalculoAsync();
        btnIrRestore.Click += (_, _) =>
        {
            AccionElegida = AccionAlertaDV.IrARestoreBackup;
            DialogResult = DialogResult.Retry;
            Close();
        };
        btnCerrar.Click += (_, _) =>
        {
            AccionElegida = AccionAlertaDV.Cancelar;
            DialogResult = DialogResult.Cancel;
            Close();
        };
    }

    private async System.Threading.Tasks.Task EjecutarRecalculoAsync()
    {
        if (_gestor == null) return;
        btnRecalcular.Enabled = false;
        btnRecalcular.Text = "Recalculando DVH y DVV...";
        try
        {
            var res = await _gestor.RecalcularDigitosVerificadoresAsync();
            MessageBox.Show(
                res.Mensaje,
                "Dígito Verificador Recalculado",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);

            AccionElegida = AccionAlertaDV.Recalculado;
            DialogResult = DialogResult.OK;
            Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show("Error al recalcular DV: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            btnRecalcular.Enabled = true;
            btnRecalcular.Text = "Recalcular Digito Verificador";
        }
    }
}

public enum AccionAlertaDV
{
    Cancelar,
    Recalculado,
    IrARestoreBackup
}
