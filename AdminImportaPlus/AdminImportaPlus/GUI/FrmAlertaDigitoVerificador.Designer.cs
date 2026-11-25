namespace AdminImportaPlus.GUI;

#nullable enable
partial class FrmAlertaDigitoVerificador
{
    private System.ComponentModel.IContainer? components = null;

    private System.Windows.Forms.Panel pnlHeader = null!;
    private System.Windows.Forms.Label lblTag = null!;
    private System.Windows.Forms.Label lblTitulo = null!;
    private System.Windows.Forms.Panel pnlCuerpo = null!;
    private System.Windows.Forms.Label lblSubtituloCambios = null!;
    private System.Windows.Forms.Label lblCambiosDetectados = null!;
    private System.Windows.Forms.Button btnRecalcular = null!;
    private System.Windows.Forms.Button btnIrRestore = null!;
    private System.Windows.Forms.Button btnCerrar = null!;

    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
        {
            components.Dispose();
        }
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        this.pnlHeader = new System.Windows.Forms.Panel();
        this.lblTag = new System.Windows.Forms.Label();
        this.lblTitulo = new System.Windows.Forms.Label();
        this.pnlCuerpo = new System.Windows.Forms.Panel();
        this.lblSubtituloCambios = new System.Windows.Forms.Label();
        this.lblCambiosDetectados = new System.Windows.Forms.Label();
        this.btnRecalcular = new System.Windows.Forms.Button();
        this.btnIrRestore = new System.Windows.Forms.Button();
        this.btnCerrar = new System.Windows.Forms.Button();
        this.pnlHeader.SuspendLayout();
        this.pnlCuerpo.SuspendLayout();
        this.SuspendLayout();
        // 
        // pnlHeader
        // 
        this.pnlHeader.BackColor = System.Drawing.Color.FromArgb(66, 2, 19);
        this.pnlHeader.Controls.Add(this.lblTag);
        this.pnlHeader.Controls.Add(this.lblTitulo);
        this.pnlHeader.Dock = System.Windows.Forms.DockStyle.Top;
        this.pnlHeader.Location = new System.Drawing.Point(0, 0);
        this.pnlHeader.Name = "pnlHeader";
        this.pnlHeader.Size = new System.Drawing.Size(804, 86);
        this.pnlHeader.TabIndex = 0;
        // 
        // lblTag
        // 
        this.lblTag.AutoSize = true;
        this.lblTag.Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Bold);
        this.lblTag.ForeColor = System.Drawing.Color.FromArgb(254, 205, 211);
        this.lblTag.Location = new System.Drawing.Point(24, 16);
        this.lblTag.Name = "lblTag";
        this.lblTag.Size = new System.Drawing.Size(477, 15);
        this.lblTag.TabIndex = 0;
        this.lblTag.Text = "ALERTA CRÍTICA DE INTEGRIDAD · DÍGITO VERIFICADOR HORIZONTAL Y VERTICAL";
        // 
        // lblTitulo
        // 
        this.lblTitulo.AutoSize = true;
        this.lblTitulo.Font = new System.Drawing.Font("Segoe UI", 14.5F, System.Drawing.FontStyle.Bold);
        this.lblTitulo.ForeColor = System.Drawing.Color.White;
        this.lblTitulo.Location = new System.Drawing.Point(24, 38);
        this.lblTitulo.Name = "lblTitulo";
        this.lblTitulo.Size = new System.Drawing.Size(555, 25);
        this.lblTitulo.TabIndex = 1;
        this.lblTitulo.Text = "Se detectaron modificaciones externas en la Base de Datos";
        // 
        // pnlCuerpo
        // 
        this.pnlCuerpo.AutoScroll = true;
        this.pnlCuerpo.BackColor = System.Drawing.Color.FromArgb(255, 248, 248);
        this.pnlCuerpo.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
        this.pnlCuerpo.Controls.Add(this.lblSubtituloCambios);
        this.pnlCuerpo.Controls.Add(this.lblCambiosDetectados);
        this.pnlCuerpo.Location = new System.Drawing.Point(24, 104);
        this.pnlCuerpo.Name = "pnlCuerpo";
        this.pnlCuerpo.Padding = new System.Windows.Forms.Padding(18);
        this.pnlCuerpo.Size = new System.Drawing.Size(756, 335);
        this.pnlCuerpo.TabIndex = 1;
        // 
        // lblSubtituloCambios
        // 
        this.lblSubtituloCambios.AutoSize = true;
        this.lblSubtituloCambios.Font = new System.Drawing.Font("Segoe UI", 11.5F, System.Drawing.FontStyle.Bold);
        this.lblSubtituloCambios.ForeColor = System.Drawing.Color.FromArgb(83, 6, 24);
        this.lblSubtituloCambios.Location = new System.Drawing.Point(16, 14);
        this.lblSubtituloCambios.Name = "lblSubtituloCambios";
        this.lblSubtituloCambios.Size = new System.Drawing.Size(265, 21);
        this.lblSubtituloCambios.TabIndex = 0;
        this.lblSubtituloCambios.Text = "Detalle de alteraciones detectadas:";
        // 
        // lblCambiosDetectados
        // 
        this.lblCambiosDetectados.AutoSize = true;
        this.lblCambiosDetectados.Font = new System.Drawing.Font("Segoe UI", 9.5F);
        this.lblCambiosDetectados.ForeColor = System.Drawing.Color.FromArgb(23, 23, 23);
        this.lblCambiosDetectados.Location = new System.Drawing.Point(16, 46);
        this.lblCambiosDetectados.MaximumSize = new System.Drawing.Size(700, 0);
        this.lblCambiosDetectados.Name = "lblCambiosDetectados";
        this.lblCambiosDetectados.Size = new System.Drawing.Size(160, 17);
        this.lblCambiosDetectados.TabIndex = 1;
        this.lblCambiosDetectados.Text = "Detalle de cambios aquí...";
        // 
        // btnRecalcular
        // 
        this.btnRecalcular.BackColor = System.Drawing.Color.FromArgb(83, 6, 24);
        this.btnRecalcular.Cursor = System.Windows.Forms.Cursors.Hand;
        this.btnRecalcular.FlatAppearance.BorderSize = 0;
        this.btnRecalcular.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        this.btnRecalcular.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
        this.btnRecalcular.ForeColor = System.Drawing.Color.White;
        this.btnRecalcular.Location = new System.Drawing.Point(24, 456);
        this.btnRecalcular.Name = "btnRecalcular";
        this.btnRecalcular.Size = new System.Drawing.Size(260, 44);
        this.btnRecalcular.TabIndex = 2;
        this.btnRecalcular.Text = "Recalcular Digito Verificador";
        this.btnRecalcular.UseVisualStyleBackColor = false;
        // 
        // btnIrRestore
        // 
        this.btnIrRestore.BackColor = System.Drawing.Color.White;
        this.btnIrRestore.Cursor = System.Windows.Forms.Cursors.Hand;
        this.btnIrRestore.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(83, 6, 24);
        this.btnIrRestore.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        this.btnIrRestore.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
        this.btnIrRestore.ForeColor = System.Drawing.Color.FromArgb(83, 6, 24);
        this.btnIrRestore.Location = new System.Drawing.Point(298, 456);
        this.btnIrRestore.Name = "btnIrRestore";
        this.btnIrRestore.Size = new System.Drawing.Size(250, 44);
        this.btnIrRestore.TabIndex = 3;
        this.btnIrRestore.Text = "Cargar Backup (RestoreBackup)";
        this.btnIrRestore.UseVisualStyleBackColor = false;
        // 
        // btnCerrar
        // 
        this.btnCerrar.BackColor = System.Drawing.Color.White;
        this.btnCerrar.Cursor = System.Windows.Forms.Cursors.Hand;
        this.btnCerrar.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(216, 221, 227);
        this.btnCerrar.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        this.btnCerrar.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
        this.btnCerrar.ForeColor = System.Drawing.Color.FromArgb(85, 85, 85);
        this.btnCerrar.Location = new System.Drawing.Point(630, 456);
        this.btnCerrar.Name = "btnCerrar";
        this.btnCerrar.Size = new System.Drawing.Size(150, 44);
        this.btnCerrar.TabIndex = 4;
        this.btnCerrar.Text = "Cancelar Ingreso";
        this.btnCerrar.UseVisualStyleBackColor = false;
        // 
        // FrmAlertaDigitoVerificador
        // 
        this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.BackColor = System.Drawing.Color.FromArgb(244, 246, 248);
        this.ClientSize = new System.Drawing.Size(804, 521);
        this.Controls.Add(this.btnCerrar);
        this.Controls.Add(this.btnIrRestore);
        this.Controls.Add(this.btnRecalcular);
        this.Controls.Add(this.pnlCuerpo);
        this.Controls.Add(this.pnlHeader);
        this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
        this.MaximizeBox = false;
        this.MinimizeBox = false;
        this.Name = "FrmAlertaDigitoVerificador";
        this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
        this.Text = "Recalcular Digito Verificador — Alerta de Seguridad IMPORTA+";
        this.pnlHeader.ResumeLayout(false);
        this.pnlHeader.PerformLayout();
        this.pnlCuerpo.ResumeLayout(false);
        this.pnlCuerpo.PerformLayout();
        this.ResumeLayout(false);
    }
}
