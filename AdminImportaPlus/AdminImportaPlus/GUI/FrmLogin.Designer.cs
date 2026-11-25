namespace AdminImportaPlus.GUI;

#nullable enable
partial class FrmLogin
{
    private System.ComponentModel.IContainer? components = null;

    private AdminImportaPlus.GUI.AppleCardPanel pnlCardLogin = null!;
    private System.Windows.Forms.Label lblUsuario = null!;
    private System.Windows.Forms.TextBox txtUsuario = null!;
    private System.Windows.Forms.Label lblPassword = null!;
    private System.Windows.Forms.TextBox txtPassword = null!;
    private AdminImportaPlus.GUI.AppleButton btnIngresar = null!;

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
        this.pnlCardLogin = new AdminImportaPlus.GUI.AppleCardPanel();
        this.lblUsuario = new System.Windows.Forms.Label();
        this.txtUsuario = new System.Windows.Forms.TextBox();
        this.lblPassword = new System.Windows.Forms.Label();
        this.txtPassword = new System.Windows.Forms.TextBox();
        this.btnIngresar = new AdminImportaPlus.GUI.AppleButton();
        this.pnlCardLogin.SuspendLayout();
        this.SuspendLayout();
        // 
        // pnlCardLogin
        // 
        this.pnlCardLogin.BackColor = System.Drawing.Color.White;
        this.pnlCardLogin.BorderColor = System.Drawing.Color.FromArgb(229, 231, 235);
        this.pnlCardLogin.Controls.Add(this.btnIngresar);
        this.pnlCardLogin.Controls.Add(this.txtPassword);
        this.pnlCardLogin.Controls.Add(this.lblPassword);
        this.pnlCardLogin.Controls.Add(this.txtUsuario);
        this.pnlCardLogin.Controls.Add(this.lblUsuario);
        this.pnlCardLogin.CornerRadius = 18;
        this.pnlCardLogin.Location = new System.Drawing.Point(24, 24);
        this.pnlCardLogin.Name = "pnlCardLogin";
        this.pnlCardLogin.Size = new System.Drawing.Size(348, 224);
        this.pnlCardLogin.TabIndex = 0;
        // 
        // lblUsuario
        // 
        this.lblUsuario.AutoSize = true;
        this.lblUsuario.BackColor = System.Drawing.Color.White;
        this.lblUsuario.Font = new System.Drawing.Font("Segoe UI Semibold", 8.5F);
        this.lblUsuario.ForeColor = System.Drawing.Color.FromArgb(110, 110, 115);
        this.lblUsuario.Location = new System.Drawing.Point(28, 24);
        this.lblUsuario.Name = "lblUsuario";
        this.lblUsuario.Size = new System.Drawing.Size(47, 15);
        this.lblUsuario.TabIndex = 0;
        this.lblUsuario.Text = "Usuario";
        // 
        // txtUsuario
        // 
        this.txtUsuario.BackColor = System.Drawing.Color.FromArgb(248, 249, 251);
        this.txtUsuario.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
        this.txtUsuario.Font = new System.Drawing.Font("Segoe UI", 10.5F);
        this.txtUsuario.ForeColor = System.Drawing.Color.FromArgb(29, 29, 31);
        this.txtUsuario.Location = new System.Drawing.Point(28, 44);
        this.txtUsuario.Name = "txtUsuario";
        this.txtUsuario.Size = new System.Drawing.Size(292, 26);
        this.txtUsuario.TabIndex = 1;
        this.txtUsuario.Text = "Admin";
        // 
        // lblPassword
        // 
        this.lblPassword.AutoSize = true;
        this.lblPassword.BackColor = System.Drawing.Color.White;
        this.lblPassword.Font = new System.Drawing.Font("Segoe UI Semibold", 8.5F);
        this.lblPassword.ForeColor = System.Drawing.Color.FromArgb(110, 110, 115);
        this.lblPassword.Location = new System.Drawing.Point(28, 84);
        this.lblPassword.Name = "lblPassword";
        this.lblPassword.Size = new System.Drawing.Size(67, 15);
        this.lblPassword.TabIndex = 2;
        this.lblPassword.Text = "Contraseña";
        // 
        // txtPassword
        // 
        this.txtPassword.BackColor = System.Drawing.Color.FromArgb(248, 249, 251);
        this.txtPassword.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
        this.txtPassword.Font = new System.Drawing.Font("Segoe UI", 10.5F);
        this.txtPassword.ForeColor = System.Drawing.Color.FromArgb(29, 29, 31);
        this.txtPassword.Location = new System.Drawing.Point(28, 104);
        this.txtPassword.Name = "txtPassword";
        this.txtPassword.Size = new System.Drawing.Size(292, 26);
        this.txtPassword.TabIndex = 3;
        this.txtPassword.UseSystemPasswordChar = true;
        // 
        // btnIngresar
        // 
        this.btnIngresar.BackColor = System.Drawing.Color.FromArgb(83, 6, 24);
        this.btnIngresar.CornerRadius = 12;
        this.btnIngresar.Font = new System.Drawing.Font("Segoe UI Semibold", 10F);
        this.btnIngresar.ForeColor = System.Drawing.Color.White;
        this.btnIngresar.HoverBackColor = System.Drawing.Color.FromArgb(115, 16, 40);
        this.btnIngresar.Location = new System.Drawing.Point(28, 152);
        this.btnIngresar.Name = "btnIngresar";
        this.btnIngresar.Size = new System.Drawing.Size(292, 44);
        this.btnIngresar.TabIndex = 4;
        this.btnIngresar.Text = "Ingresar al Panel";
        // 
        // FrmLogin
        // 
        this.AcceptButton = this.btnIngresar;
        this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.BackColor = System.Drawing.Color.FromArgb(245, 245, 247);
        this.ClientSize = new System.Drawing.Size(396, 272);
        this.Controls.Add(this.pnlCardLogin);
        this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
        this.MaximizeBox = false;
        this.Name = "FrmLogin";
        this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
        this.Text = "Acceso Administrador";
        this.pnlCardLogin.ResumeLayout(false);
        this.pnlCardLogin.PerformLayout();
        this.ResumeLayout(false);
    }
}
