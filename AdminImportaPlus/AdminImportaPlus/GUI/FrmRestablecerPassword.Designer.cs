namespace AdminImportaPlus.GUI;

#nullable enable
partial class FrmRestablecerPassword
{
    private System.ComponentModel.IContainer? components = null;

    private System.Windows.Forms.Panel pnlHeader = null!;
    private System.Windows.Forms.Label lblHeader = null!;
    private System.Windows.Forms.Panel pnlCard = null!;
    private System.Windows.Forms.Label lblUser = null!;
    private System.Windows.Forms.TextBox txtUsuario = null!;
    private System.Windows.Forms.Label lblPass1 = null!;
    private System.Windows.Forms.TextBox txtNuevaPassword = null!;
    private System.Windows.Forms.Label lblPass2 = null!;
    private System.Windows.Forms.TextBox txtConfirmarPassword = null!;
    private System.Windows.Forms.Button btnGuardar = null!;

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
        this.lblHeader = new System.Windows.Forms.Label();
        this.pnlCard = new System.Windows.Forms.Panel();
        this.lblUser = new System.Windows.Forms.Label();
        this.txtUsuario = new System.Windows.Forms.TextBox();
        this.lblPass1 = new System.Windows.Forms.Label();
        this.txtNuevaPassword = new System.Windows.Forms.TextBox();
        this.lblPass2 = new System.Windows.Forms.Label();
        this.txtConfirmarPassword = new System.Windows.Forms.TextBox();
        this.btnGuardar = new System.Windows.Forms.Button();
        this.pnlHeader.SuspendLayout();
        this.pnlCard.SuspendLayout();
        this.SuspendLayout();
        // 
        // pnlHeader
        // 
        this.pnlHeader.BackColor = System.Drawing.Color.FromArgb(66, 2, 19);
        this.pnlHeader.Controls.Add(this.lblHeader);
        this.pnlHeader.Dock = System.Windows.Forms.DockStyle.Top;
        this.pnlHeader.Location = new System.Drawing.Point(0, 0);
        this.pnlHeader.Name = "pnlHeader";
        this.pnlHeader.Size = new System.Drawing.Size(444, 68);
        this.pnlHeader.TabIndex = 0;
        // 
        // lblHeader
        // 
        this.lblHeader.AutoSize = true;
        this.lblHeader.Font = new System.Drawing.Font("Segoe UI", 13F, System.Drawing.FontStyle.Bold);
        this.lblHeader.ForeColor = System.Drawing.Color.White;
        this.lblHeader.Location = new System.Drawing.Point(22, 20);
        this.lblHeader.Name = "lblHeader";
        this.lblHeader.Size = new System.Drawing.Size(270, 25);
        this.lblHeader.TabIndex = 0;
        this.lblHeader.Text = "Restablecer Contraseña";
        // 
        // pnlCard
        // 
        this.pnlCard.BackColor = System.Drawing.Color.White;
        this.pnlCard.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
        this.pnlCard.Controls.Add(this.btnGuardar);
        this.pnlCard.Controls.Add(this.txtConfirmarPassword);
        this.pnlCard.Controls.Add(this.lblPass2);
        this.pnlCard.Controls.Add(this.txtNuevaPassword);
        this.pnlCard.Controls.Add(this.lblPass1);
        this.pnlCard.Controls.Add(this.txtUsuario);
        this.pnlCard.Controls.Add(this.lblUser);
        this.pnlCard.Location = new System.Drawing.Point(22, 86);
        this.pnlCard.Name = "pnlCard";
        this.pnlCard.Size = new System.Drawing.Size(400, 245);
        this.pnlCard.TabIndex = 1;
        // 
        // lblUser
        // 
        this.lblUser.AutoSize = true;
        this.lblUser.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
        this.lblUser.ForeColor = System.Drawing.Color.FromArgb(85, 85, 85);
        this.lblUser.Location = new System.Drawing.Point(20, 16);
        this.lblUser.Name = "lblUser";
        this.lblUser.Size = new System.Drawing.Size(156, 15);
        this.lblUser.TabIndex = 0;
        this.lblUser.Text = "CUENTA ADMINISTRADOR";
        // 
        // txtUsuario
        // 
        this.txtUsuario.BackColor = System.Drawing.Color.FromArgb(244, 246, 248);
        this.txtUsuario.Font = new System.Drawing.Font("Segoe UI", 9.5F);
        this.txtUsuario.Location = new System.Drawing.Point(20, 36);
        this.txtUsuario.Name = "txtUsuario";
        this.txtUsuario.ReadOnly = true;
        this.txtUsuario.Size = new System.Drawing.Size(356, 24);
        this.txtUsuario.TabIndex = 1;
        this.txtUsuario.Text = "Admin";
        // 
        // lblPass1
        // 
        this.lblPass1.AutoSize = true;
        this.lblPass1.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
        this.lblPass1.ForeColor = System.Drawing.Color.FromArgb(85, 85, 85);
        this.lblPass1.Location = new System.Drawing.Point(20, 74);
        this.lblPass1.Name = "lblPass1";
        this.lblPass1.Size = new System.Drawing.Size(134, 15);
        this.lblPass1.TabIndex = 2;
        this.lblPass1.Text = "NUEVA CONTRASEÑA *";
        // 
        // txtNuevaPassword
        // 
        this.txtNuevaPassword.Font = new System.Drawing.Font("Segoe UI", 9.5F);
        this.txtNuevaPassword.Location = new System.Drawing.Point(20, 94);
        this.txtNuevaPassword.Name = "txtNuevaPassword";
        this.txtNuevaPassword.Size = new System.Drawing.Size(356, 24);
        this.txtNuevaPassword.TabIndex = 3;
        this.txtNuevaPassword.UseSystemPasswordChar = true;
        // 
        // lblPass2
        // 
        this.lblPass2.AutoSize = true;
        this.lblPass2.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
        this.lblPass2.ForeColor = System.Drawing.Color.FromArgb(85, 85, 85);
        this.lblPass2.Location = new System.Drawing.Point(20, 132);
        this.lblPass2.Name = "lblPass2";
        this.lblPass2.Size = new System.Drawing.Size(198, 15);
        this.lblPass2.TabIndex = 4;
        this.lblPass2.Text = "CONFIRMAR NUEVA CONTRASEÑA *";
        // 
        // txtConfirmarPassword
        // 
        this.txtConfirmarPassword.Font = new System.Drawing.Font("Segoe UI", 9.5F);
        this.txtConfirmarPassword.Location = new System.Drawing.Point(20, 152);
        this.txtConfirmarPassword.Name = "txtConfirmarPassword";
        this.txtConfirmarPassword.Size = new System.Drawing.Size(356, 24);
        this.txtConfirmarPassword.TabIndex = 5;
        this.txtConfirmarPassword.UseSystemPasswordChar = true;
        // 
        // btnGuardar
        // 
        this.btnGuardar.BackColor = System.Drawing.Color.FromArgb(83, 6, 24);
        this.btnGuardar.Cursor = System.Windows.Forms.Cursors.Hand;
        this.btnGuardar.FlatAppearance.BorderSize = 0;
        this.btnGuardar.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        this.btnGuardar.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
        this.btnGuardar.ForeColor = System.Drawing.Color.White;
        this.btnGuardar.Location = new System.Drawing.Point(20, 190);
        this.btnGuardar.Name = "btnGuardar";
        this.btnGuardar.Size = new System.Drawing.Size(356, 40);
        this.btnGuardar.TabIndex = 6;
        this.btnGuardar.Text = "Guardar Nueva Contraseña";
        this.btnGuardar.UseVisualStyleBackColor = false;
        // 
        // FrmRestablecerPassword
        // 
        this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.BackColor = System.Drawing.Color.FromArgb(244, 246, 248);
        this.ClientSize = new System.Drawing.Size(444, 351);
        this.Controls.Add(this.pnlCard);
        this.Controls.Add(this.pnlHeader);
        this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
        this.MaximizeBox = false;
        this.MinimizeBox = false;
        this.Name = "FrmRestablecerPassword";
        this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
        this.Text = "Restablecer Contraseña Administrador";
        this.pnlHeader.ResumeLayout(false);
        this.pnlHeader.PerformLayout();
        this.pnlCard.ResumeLayout(false);
        this.pnlCard.PerformLayout();
        this.ResumeLayout(false);
    }
}
