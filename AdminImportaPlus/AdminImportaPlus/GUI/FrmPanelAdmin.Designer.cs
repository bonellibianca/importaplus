namespace AdminImportaPlus.GUI;

#nullable enable
partial class FrmPanelAdmin
{
    private System.ComponentModel.IContainer? components = null;

    // Sidebar Izquierdo Estilo Apple + Área Principal
    private System.Windows.Forms.Panel pnlSidebar = null!;
    private AdminImportaPlus.GUI.AppleButton btnNavForwarders = null!;
    private AdminImportaPlus.GUI.AppleButton btnNavPuertos = null!;
    private AdminImportaPlus.GUI.AppleButton btnNavRoles = null!;
    private AdminImportaPlus.GUI.AppleButton btnNavDigitoVerificador = null!;
    private AdminImportaPlus.GUI.AppleButton btnNavRestoreSupabase = null!;
    private AdminImportaPlus.GUI.AppleButton btnCambiarPassword = null!;

    private System.Windows.Forms.Panel pnlMain = null!;
    private System.Windows.Forms.Panel pnlTopBar = null!;
    private System.Windows.Forms.Label lblUsuarioActual = null!;
    private System.Windows.Forms.Label lblBadgeEstadoHeader = null!;
    private AdminImportaPlus.GUI.AppleButton btnCerrarSesion = null!;
    private System.Windows.Forms.Panel pnlContenedorVistas = null!;

    // Sección 1: Forwarders (Ilimitados, orden único)
    private System.Windows.Forms.Panel pnlSecForwarders = null!;
    private System.Windows.Forms.SplitContainer splitForwarders = null!;
    private AdminImportaPlus.GUI.AppleCardPanel pnlFwdGridWrap = null!;
    private System.Windows.Forms.Label lblFwdGridTitle = null!;
    private System.Windows.Forms.DataGridView dgvForwarders = null!;
    private AdminImportaPlus.GUI.AppleCardPanel pnlFwdForm = null!;
    private System.Windows.Forms.Label lblFwdEditorTitulo = null!;
    private System.Windows.Forms.PictureBox picForwarder = null!;
    private AdminImportaPlus.GUI.AppleButton btnSubirFoto = null!;
    private System.Windows.Forms.Label lblFwdUrl = null!;
    private System.Windows.Forms.TextBox txtFwdImagenUrl = null!;
    private System.Windows.Forms.Label lblFwdNombre = null!;
    private System.Windows.Forms.Label lblContadorNombreFwd = null!;
    private System.Windows.Forms.TextBox txtFwdNombre = null!;
    private System.Windows.Forms.Label lblFwdWa = null!;
    private System.Windows.Forms.TextBox txtFwdWhatsapp = null!;
    private System.Windows.Forms.Label lblFwdMail = null!;
    private System.Windows.Forms.TextBox txtFwdEmail = null!;
    private System.Windows.Forms.Label lblFwdOrden = null!;
    private System.Windows.Forms.NumericUpDown nudFwdOrden = null!;
    private System.Windows.Forms.CheckBox chkFwdActivo = null!;
    private AdminImportaPlus.GUI.AppleButton btnGuardarFwd = null!;
    private AdminImportaPlus.GUI.AppleButton btnNuevoFwd = null!;
    private AdminImportaPlus.GUI.AppleButton btnEliminarFwd = null!;

    // Sección 2: Puertos y Aduanas
    private System.Windows.Forms.Panel pnlSecPuertos = null!;
    private System.Windows.Forms.SplitContainer splitPuertos = null!;
    private AdminImportaPlus.GUI.AppleCardPanel pnlPtoGridWrap = null!;
    private System.Windows.Forms.DataGridView dgvPuertos = null!;
    private AdminImportaPlus.GUI.AppleCardPanel pnlPtoForm = null!;
    private System.Windows.Forms.Label lblPtoTit = null!;
    private System.Windows.Forms.Label lblPtoNombre = null!;
    private System.Windows.Forms.TextBox txtPuertoNombre = null!;
    private System.Windows.Forms.Label lblPtoVia = null!;
    private System.Windows.Forms.ComboBox cmbPuertoVia = null!;
    private System.Windows.Forms.Label lblPtoProvincia = null!;
    private System.Windows.Forms.TextBox txtPuertoProvincia = null!;
    private System.Windows.Forms.Label lblPtoSim = null!;
    private System.Windows.Forms.TextBox txtPuertoSim = null!;
    private System.Windows.Forms.Label lblPtoOperador = null!;
    private System.Windows.Forms.TextBox txtPuertoOperador = null!;
    private System.Windows.Forms.Label lblPtoInfra = null!;
    private System.Windows.Forms.TextBox txtPuertoInfra = null!;
    private System.Windows.Forms.Label lblPtoLat = null!;
    private System.Windows.Forms.TextBox txtPuertoLat = null!;
    private System.Windows.Forms.Label lblPtoLng = null!;
    private System.Windows.Forms.TextBox txtPuertoLng = null!;
    private AdminImportaPlus.GUI.AppleButton btnGuardarPto = null!;
    private AdminImportaPlus.GUI.AppleButton btnNuevoPto = null!;
    private AdminImportaPlus.GUI.AppleButton btnBorrarPto = null!;

    // Sección 3: Roles y Permisos (Patrón Composite: Permisos | Familias | Perfiles + TreeView)
    private System.Windows.Forms.Panel pnlSecRoles = null!;
    private System.Windows.Forms.SplitContainer splitRoles = null!;
    private AdminImportaPlus.GUI.AppleCardPanel pnlRolesIzq = null!;
    private System.Windows.Forms.Label lblRolesTit = null!;
    private System.Windows.Forms.Label lblPermisoSec = null!;
    private System.Windows.Forms.TextBox txtPermisoCodigo = null!;
    private System.Windows.Forms.TextBox txtPermisoNombre = null!;
    private AdminImportaPlus.GUI.AppleButton btnCrearPermiso = null!;
    private System.Windows.Forms.Label lblFamiliaSec = null!;
    private System.Windows.Forms.TextBox txtFamiliaNombre = null!;
    private AdminImportaPlus.GUI.AppleButton btnCrearFamilia = null!;
    private System.Windows.Forms.ComboBox cmbFamilias = null!;
    private System.Windows.Forms.ComboBox cmbComponentesParaFamilia = null!;
    private AdminImportaPlus.GUI.AppleButton btnAgregarAFamilia = null!;
    private System.Windows.Forms.Label lblPerfilSec = null!;
    private System.Windows.Forms.TextBox txtPerfilNombre = null!;
    private AdminImportaPlus.GUI.AppleButton btnCrearPerfil = null!;
    private AdminImportaPlus.GUI.AppleCardPanel pnlRolesDer = null!;
    private System.Windows.Forms.Label lblTreeTit = null!;
    private System.Windows.Forms.ComboBox cmbPerfiles = null!;
    private System.Windows.Forms.ComboBox cmbComponentesParaPerfil = null!;
    private AdminImportaPlus.GUI.AppleButton btnAsignarAPerfil = null!;
    private AdminImportaPlus.GUI.AppleButton btnQuitarDePerfil = null!;
    private System.Windows.Forms.TreeView tvPermisosPerfil = null!;
    private System.Windows.Forms.Label lblReglaComposite = null!;

    // Sección 4: Dígito Verificador (Conteo real total Supabase sin cargar 10.439 NCM ni 196 países en grilla)
    private System.Windows.Forms.Panel pnlSecDigitoVerificador = null!;
    private AdminImportaPlus.GUI.AppleCardPanel pnlDvCard = null!;
    private System.Windows.Forms.Label lblDvTit = null!;
    private AdminImportaPlus.GUI.AppleButton btnVerificarDv = null!;
    private AdminImportaPlus.GUI.AppleButton btnRecalcularDv = null!;
    private System.Windows.Forms.DataGridView dgvDvv = null!;
    private AdminImportaPlus.GUI.AppleCardPanel pnlDvReporte = null!;
    private System.Windows.Forms.Label lblDetalleIntegridadPanel = null!;

    // Sección 5: RestoreBackup & Supabase
    private System.Windows.Forms.Panel pnlSecRestoreSupabase = null!;
    private System.Windows.Forms.SplitContainer splitRestoreSupa = null!;
    private AdminImportaPlus.GUI.AppleCardPanel pnlBackupCard = null!;
    private System.Windows.Forms.Label lblBkTag = null!;
    private System.Windows.Forms.Label lblBkTit = null!;
    private System.Windows.Forms.Label lblBkDesc = null!;
    private AdminImportaPlus.GUI.AppleButton btnCrearBackup = null!;
    private System.Windows.Forms.Label lblBkSel = null!;
    private System.Windows.Forms.TextBox txtRutaRestore = null!;
    private AdminImportaPlus.GUI.AppleButton btnExaminarBk = null!;
    private AdminImportaPlus.GUI.AppleCardPanel pnlPreviewBk = null!;
    private System.Windows.Forms.Label lblPreviewBackup = null!;
    private AdminImportaPlus.GUI.AppleButton btnEjecutarRestore = null!;
    private AdminImportaPlus.GUI.AppleCardPanel pnlSupaCard = null!;
    private System.Windows.Forms.Label lblSpTag = null!;
    private System.Windows.Forms.Label lblSpTit = null!;
    private System.Windows.Forms.Label lblSupaUrl = null!;
    private System.Windows.Forms.TextBox txtSupaUrl = null!;
    private System.Windows.Forms.Label lblSupaService = null!;
    private System.Windows.Forms.TextBox txtSupaServiceKey = null!;
    private System.Windows.Forms.Label lblSupaAnon = null!;
    private System.Windows.Forms.TextBox txtSupaAnonKey = null!;
    private AdminImportaPlus.GUI.AppleButton btnGuardarSupa = null!;
    private AdminImportaPlus.GUI.AppleButton btnEmpujarNube = null!;
    private System.Windows.Forms.Label lblResultadoSupa = null!;

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
        this.pnlSidebar = new System.Windows.Forms.Panel();
        this.btnCambiarPassword = new AdminImportaPlus.GUI.AppleButton();
        this.btnNavRestoreSupabase = new AdminImportaPlus.GUI.AppleButton();
        this.btnNavDigitoVerificador = new AdminImportaPlus.GUI.AppleButton();
        this.btnNavRoles = new AdminImportaPlus.GUI.AppleButton();
        this.btnNavPuertos = new AdminImportaPlus.GUI.AppleButton();
        this.btnNavForwarders = new AdminImportaPlus.GUI.AppleButton();
        this.pnlMain = new System.Windows.Forms.Panel();
        this.pnlContenedorVistas = new System.Windows.Forms.Panel();

        this.pnlSecForwarders = new System.Windows.Forms.Panel();
        this.splitForwarders = new System.Windows.Forms.SplitContainer();
        this.pnlFwdGridWrap = new AdminImportaPlus.GUI.AppleCardPanel();
        this.dgvForwarders = new System.Windows.Forms.DataGridView();
        this.lblFwdGridTitle = new System.Windows.Forms.Label();
        this.pnlFwdForm = new AdminImportaPlus.GUI.AppleCardPanel();
        this.btnEliminarFwd = new AdminImportaPlus.GUI.AppleButton();
        this.btnNuevoFwd = new AdminImportaPlus.GUI.AppleButton();
        this.btnGuardarFwd = new AdminImportaPlus.GUI.AppleButton();
        this.chkFwdActivo = new System.Windows.Forms.CheckBox();
        this.nudFwdOrden = new System.Windows.Forms.NumericUpDown();
        this.lblFwdOrden = new System.Windows.Forms.Label();
        this.txtFwdEmail = new System.Windows.Forms.TextBox();
        this.lblFwdMail = new System.Windows.Forms.Label();
        this.txtFwdWhatsapp = new System.Windows.Forms.TextBox();
        this.lblFwdWa = new System.Windows.Forms.Label();
        this.txtFwdNombre = new System.Windows.Forms.TextBox();
        this.lblContadorNombreFwd = new System.Windows.Forms.Label();
        this.lblFwdNombre = new System.Windows.Forms.Label();
        this.txtFwdImagenUrl = new System.Windows.Forms.TextBox();
        this.lblFwdUrl = new System.Windows.Forms.Label();
        this.btnSubirFoto = new AdminImportaPlus.GUI.AppleButton();
        this.picForwarder = new System.Windows.Forms.PictureBox();
        this.lblFwdEditorTitulo = new System.Windows.Forms.Label();

        this.pnlSecPuertos = new System.Windows.Forms.Panel();
        this.splitPuertos = new System.Windows.Forms.SplitContainer();
        this.pnlPtoGridWrap = new AdminImportaPlus.GUI.AppleCardPanel();
        this.dgvPuertos = new System.Windows.Forms.DataGridView();
        this.pnlPtoForm = new AdminImportaPlus.GUI.AppleCardPanel();
        this.btnBorrarPto = new AdminImportaPlus.GUI.AppleButton();
        this.btnNuevoPto = new AdminImportaPlus.GUI.AppleButton();
        this.btnGuardarPto = new AdminImportaPlus.GUI.AppleButton();
        this.txtPuertoLng = new System.Windows.Forms.TextBox();
        this.lblPtoLng = new System.Windows.Forms.Label();
        this.txtPuertoLat = new System.Windows.Forms.TextBox();
        this.lblPtoLat = new System.Windows.Forms.Label();
        this.txtPuertoInfra = new System.Windows.Forms.TextBox();
        this.lblPtoInfra = new System.Windows.Forms.Label();
        this.txtPuertoOperador = new System.Windows.Forms.TextBox();
        this.lblPtoOperador = new System.Windows.Forms.Label();
        this.txtPuertoSim = new System.Windows.Forms.TextBox();
        this.lblPtoSim = new System.Windows.Forms.Label();
        this.txtPuertoProvincia = new System.Windows.Forms.TextBox();
        this.lblPtoProvincia = new System.Windows.Forms.Label();
        this.cmbPuertoVia = new System.Windows.Forms.ComboBox();
        this.lblPtoVia = new System.Windows.Forms.Label();
        this.txtPuertoNombre = new System.Windows.Forms.TextBox();
        this.lblPtoNombre = new System.Windows.Forms.Label();
        this.lblPtoTit = new System.Windows.Forms.Label();

        this.pnlSecRoles = new System.Windows.Forms.Panel();
        this.splitRoles = new System.Windows.Forms.SplitContainer();
        this.pnlRolesIzq = new AdminImportaPlus.GUI.AppleCardPanel();
        this.lblRolesTit = new System.Windows.Forms.Label();
        this.lblPermisoSec = new System.Windows.Forms.Label();
        this.txtPermisoCodigo = new System.Windows.Forms.TextBox();
        this.txtPermisoNombre = new System.Windows.Forms.TextBox();
        this.btnCrearPermiso = new AdminImportaPlus.GUI.AppleButton();
        this.lblFamiliaSec = new System.Windows.Forms.Label();
        this.txtFamiliaNombre = new System.Windows.Forms.TextBox();
        this.btnCrearFamilia = new AdminImportaPlus.GUI.AppleButton();
        this.cmbFamilias = new System.Windows.Forms.ComboBox();
        this.cmbComponentesParaFamilia = new System.Windows.Forms.ComboBox();
        this.btnAgregarAFamilia = new AdminImportaPlus.GUI.AppleButton();
        this.lblPerfilSec = new System.Windows.Forms.Label();
        this.txtPerfilNombre = new System.Windows.Forms.TextBox();
        this.btnCrearPerfil = new AdminImportaPlus.GUI.AppleButton();
        this.pnlRolesDer = new AdminImportaPlus.GUI.AppleCardPanel();
        this.lblTreeTit = new System.Windows.Forms.Label();
        this.cmbPerfiles = new System.Windows.Forms.ComboBox();
        this.cmbComponentesParaPerfil = new System.Windows.Forms.ComboBox();
        this.btnAsignarAPerfil = new AdminImportaPlus.GUI.AppleButton();
        this.btnQuitarDePerfil = new AdminImportaPlus.GUI.AppleButton();
        this.tvPermisosPerfil = new System.Windows.Forms.TreeView();
        this.lblReglaComposite = new System.Windows.Forms.Label();

        this.pnlSecDigitoVerificador = new System.Windows.Forms.Panel();
        this.pnlDvCard = new AdminImportaPlus.GUI.AppleCardPanel();
        this.pnlDvReporte = new AdminImportaPlus.GUI.AppleCardPanel();
        this.lblDetalleIntegridadPanel = new System.Windows.Forms.Label();
        this.dgvDvv = new System.Windows.Forms.DataGridView();
        this.btnRecalcularDv = new AdminImportaPlus.GUI.AppleButton();
        this.btnVerificarDv = new AdminImportaPlus.GUI.AppleButton();
        this.lblDvTit = new System.Windows.Forms.Label();

        this.pnlSecRestoreSupabase = new System.Windows.Forms.Panel();
        this.splitRestoreSupa = new System.Windows.Forms.SplitContainer();
        this.pnlBackupCard = new AdminImportaPlus.GUI.AppleCardPanel();
        this.btnEjecutarRestore = new AdminImportaPlus.GUI.AppleButton();
        this.pnlPreviewBk = new AdminImportaPlus.GUI.AppleCardPanel();
        this.lblPreviewBackup = new System.Windows.Forms.Label();
        this.btnExaminarBk = new AdminImportaPlus.GUI.AppleButton();
        this.txtRutaRestore = new System.Windows.Forms.TextBox();
        this.lblBkSel = new System.Windows.Forms.Label();
        this.btnCrearBackup = new AdminImportaPlus.GUI.AppleButton();
        this.lblBkDesc = new System.Windows.Forms.Label();
        this.lblBkTit = new System.Windows.Forms.Label();
        this.lblBkTag = new System.Windows.Forms.Label();
        this.pnlSupaCard = new AdminImportaPlus.GUI.AppleCardPanel();
        this.lblResultadoSupa = new System.Windows.Forms.Label();
        this.btnEmpujarNube = new AdminImportaPlus.GUI.AppleButton();
        this.btnGuardarSupa = new AdminImportaPlus.GUI.AppleButton();
        this.txtSupaAnonKey = new System.Windows.Forms.TextBox();
        this.lblSupaAnon = new System.Windows.Forms.Label();
        this.txtSupaServiceKey = new System.Windows.Forms.TextBox();
        this.lblSupaService = new System.Windows.Forms.Label();
        this.txtSupaUrl = new System.Windows.Forms.TextBox();
        this.lblSupaUrl = new System.Windows.Forms.Label();
        this.lblSpTit = new System.Windows.Forms.Label();
        this.lblSpTag = new System.Windows.Forms.Label();

        this.pnlTopBar = new System.Windows.Forms.Panel();
        this.btnCerrarSesion = new AdminImportaPlus.GUI.AppleButton();
        this.lblBadgeEstadoHeader = new System.Windows.Forms.Label();
        this.lblUsuarioActual = new System.Windows.Forms.Label();

        this.pnlSidebar.SuspendLayout();
        this.pnlMain.SuspendLayout();
        this.pnlContenedorVistas.SuspendLayout();
        this.pnlSecForwarders.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)(this.splitForwarders)).BeginInit();
        this.splitForwarders.Panel1.SuspendLayout();
        this.splitForwarders.Panel2.SuspendLayout();
        this.splitForwarders.SuspendLayout();
        this.pnlFwdGridWrap.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)(this.dgvForwarders)).BeginInit();
        this.pnlFwdForm.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)(this.nudFwdOrden)).BeginInit();
        ((System.ComponentModel.ISupportInitialize)(this.picForwarder)).BeginInit();
        this.pnlSecPuertos.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)(this.splitPuertos)).BeginInit();
        this.splitPuertos.Panel1.SuspendLayout();
        this.splitPuertos.Panel2.SuspendLayout();
        this.splitPuertos.SuspendLayout();
        this.pnlPtoGridWrap.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)(this.dgvPuertos)).BeginInit();
        this.pnlPtoForm.SuspendLayout();
        this.pnlSecRoles.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)(this.splitRoles)).BeginInit();
        this.splitRoles.Panel1.SuspendLayout();
        this.splitRoles.Panel2.SuspendLayout();
        this.splitRoles.SuspendLayout();
        this.pnlRolesIzq.SuspendLayout();
        this.pnlRolesDer.SuspendLayout();
        this.pnlSecDigitoVerificador.SuspendLayout();
        this.pnlDvCard.SuspendLayout();
        this.pnlDvReporte.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)(this.dgvDvv)).BeginInit();
        this.pnlSecRestoreSupabase.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)(this.splitRestoreSupa)).BeginInit();
        this.splitRestoreSupa.Panel1.SuspendLayout();
        this.splitRestoreSupa.Panel2.SuspendLayout();
        this.splitRestoreSupa.SuspendLayout();
        this.pnlBackupCard.SuspendLayout();
        this.pnlPreviewBk.SuspendLayout();
        this.pnlSupaCard.SuspendLayout();
        this.pnlTopBar.SuspendLayout();
        this.SuspendLayout();

        // ====================================================================
        // SIDEBAR IZQUIERDO ESTILO APPLE / MACOS
        // ====================================================================
        this.pnlSidebar.BackColor = System.Drawing.Color.FromArgb(46, 5, 16);
        this.pnlSidebar.Controls.Add(this.btnCambiarPassword);
        this.pnlSidebar.Controls.Add(this.btnNavRestoreSupabase);
        this.pnlSidebar.Controls.Add(this.btnNavDigitoVerificador);
        this.pnlSidebar.Controls.Add(this.btnNavRoles);
        this.pnlSidebar.Controls.Add(this.btnNavPuertos);
        this.pnlSidebar.Controls.Add(this.btnNavForwarders);
        this.pnlSidebar.Dock = System.Windows.Forms.DockStyle.Left;
        this.pnlSidebar.Name = "pnlSidebar";
        this.pnlSidebar.Size = new System.Drawing.Size(230, 701);

        this.btnNavForwarders.BackColor = System.Drawing.Color.FromArgb(115, 16, 40);
        this.btnNavForwarders.CornerRadius = 12;
        this.btnNavForwarders.ForeColor = System.Drawing.Color.White;
        this.btnNavForwarders.HoverBackColor = System.Drawing.Color.FromArgb(135, 24, 52);
        this.btnNavForwarders.Location = new System.Drawing.Point(16, 26);
        this.btnNavForwarders.Name = "btnNavForwarders";
        this.btnNavForwarders.Padding = new System.Windows.Forms.Padding(12, 0, 0, 0);
        this.btnNavForwarders.Size = new System.Drawing.Size(198, 44);
        this.btnNavForwarders.Text = "Forwarders";
        this.btnNavForwarders.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;

        this.btnNavPuertos.BackColor = System.Drawing.Color.FromArgb(46, 5, 16);
        this.btnNavPuertos.CornerRadius = 12;
        this.btnNavPuertos.ForeColor = System.Drawing.Color.FromArgb(245, 215, 222);
        this.btnNavPuertos.HoverBackColor = System.Drawing.Color.FromArgb(74, 12, 29);
        this.btnNavPuertos.Location = new System.Drawing.Point(16, 80);
        this.btnNavPuertos.Name = "btnNavPuertos";
        this.btnNavPuertos.Padding = new System.Windows.Forms.Padding(12, 0, 0, 0);
        this.btnNavPuertos.Size = new System.Drawing.Size(198, 44);
        this.btnNavPuertos.Text = "Puertos y Aduanas";
        this.btnNavPuertos.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;

        this.btnNavRoles.BackColor = System.Drawing.Color.FromArgb(46, 5, 16);
        this.btnNavRoles.CornerRadius = 12;
        this.btnNavRoles.ForeColor = System.Drawing.Color.FromArgb(245, 215, 222);
        this.btnNavRoles.HoverBackColor = System.Drawing.Color.FromArgb(74, 12, 29);
        this.btnNavRoles.Location = new System.Drawing.Point(16, 134);
        this.btnNavRoles.Name = "btnNavRoles";
        this.btnNavRoles.Padding = new System.Windows.Forms.Padding(12, 0, 0, 0);
        this.btnNavRoles.Size = new System.Drawing.Size(198, 44);
        this.btnNavRoles.Text = "Roles y Permisos";
        this.btnNavRoles.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;

        this.btnNavDigitoVerificador.BackColor = System.Drawing.Color.FromArgb(46, 5, 16);
        this.btnNavDigitoVerificador.CornerRadius = 12;
        this.btnNavDigitoVerificador.ForeColor = System.Drawing.Color.FromArgb(245, 215, 222);
        this.btnNavDigitoVerificador.HoverBackColor = System.Drawing.Color.FromArgb(74, 12, 29);
        this.btnNavDigitoVerificador.Location = new System.Drawing.Point(16, 188);
        this.btnNavDigitoVerificador.Name = "btnNavDigitoVerificador";
        this.btnNavDigitoVerificador.Padding = new System.Windows.Forms.Padding(12, 0, 0, 0);
        this.btnNavDigitoVerificador.Size = new System.Drawing.Size(198, 44);
        this.btnNavDigitoVerificador.Text = "Dígito Verificador";
        this.btnNavDigitoVerificador.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;

        this.btnNavRestoreSupabase.BackColor = System.Drawing.Color.FromArgb(46, 5, 16);
        this.btnNavRestoreSupabase.CornerRadius = 12;
        this.btnNavRestoreSupabase.ForeColor = System.Drawing.Color.FromArgb(245, 215, 222);
        this.btnNavRestoreSupabase.HoverBackColor = System.Drawing.Color.FromArgb(74, 12, 29);
        this.btnNavRestoreSupabase.Location = new System.Drawing.Point(16, 242);
        this.btnNavRestoreSupabase.Name = "btnNavRestoreSupabase";
        this.btnNavRestoreSupabase.Padding = new System.Windows.Forms.Padding(12, 0, 0, 0);
        this.btnNavRestoreSupabase.Size = new System.Drawing.Size(198, 44);
        this.btnNavRestoreSupabase.Text = "RestoreBackup y Nube";
        this.btnNavRestoreSupabase.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;

        this.btnCambiarPassword.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
        this.btnCambiarPassword.BackColor = System.Drawing.Color.FromArgb(83, 6, 24);
        this.btnCambiarPassword.BorderColor = System.Drawing.Color.FromArgb(155, 48, 64);
        this.btnCambiarPassword.BorderThickness = 1;
        this.btnCambiarPassword.CornerRadius = 12;
        this.btnCambiarPassword.ForeColor = System.Drawing.Color.White;
        this.btnCambiarPassword.HoverBackColor = System.Drawing.Color.FromArgb(115, 16, 40);
        this.btnCambiarPassword.Location = new System.Drawing.Point(16, 636);
        this.btnCambiarPassword.Name = "btnCambiarPassword";
        this.btnCambiarPassword.Size = new System.Drawing.Size(198, 42);
        this.btnCambiarPassword.Text = "Cambiar Contraseña";

        // ====================================================================
        // ÁREA PRINCIPAL DERECHA
        // ====================================================================
        this.pnlMain.BackColor = System.Drawing.Color.FromArgb(245, 245, 247);
        this.pnlMain.Controls.Add(this.pnlContenedorVistas);
        this.pnlMain.Controls.Add(this.pnlTopBar);
        this.pnlMain.Dock = System.Windows.Forms.DockStyle.Fill;

        this.pnlTopBar.BackColor = System.Drawing.Color.White;
        this.pnlTopBar.Controls.Add(this.btnCerrarSesion);
        this.pnlTopBar.Controls.Add(this.lblBadgeEstadoHeader);
        this.pnlTopBar.Controls.Add(this.lblUsuarioActual);
        this.pnlTopBar.Dock = System.Windows.Forms.DockStyle.Top;
        this.pnlTopBar.Size = new System.Drawing.Size(974, 58);

        this.lblUsuarioActual.AutoSize = true;
        this.lblUsuarioActual.Font = new System.Drawing.Font("Segoe UI Semibold", 10.5F);
        this.lblUsuarioActual.ForeColor = System.Drawing.Color.FromArgb(29, 29, 31);
        this.lblUsuarioActual.Location = new System.Drawing.Point(22, 19);
        this.lblUsuarioActual.Text = "Usuario: Admin";

        this.lblBadgeEstadoHeader.AutoSize = true;
        this.lblBadgeEstadoHeader.Font = new System.Drawing.Font("Segoe UI Semibold", 9F);
        this.lblBadgeEstadoHeader.ForeColor = System.Drawing.Color.FromArgb(5, 150, 105);
        this.lblBadgeEstadoHeader.Location = new System.Drawing.Point(168, 21);
        this.lblBadgeEstadoHeader.Text = "Verificando integridad...";

        this.btnCerrarSesion.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
        this.btnCerrarSesion.BackColor = System.Drawing.Color.FromArgb(245, 245, 247);
        this.btnCerrarSesion.BorderColor = System.Drawing.Color.FromArgb(229, 231, 235);
        this.btnCerrarSesion.BorderThickness = 1;
        this.btnCerrarSesion.CornerRadius = 10;
        this.btnCerrarSesion.ForeColor = System.Drawing.Color.FromArgb(83, 6, 24);
        this.btnCerrarSesion.HoverBackColor = System.Drawing.Color.FromArgb(255, 240, 243);
        this.btnCerrarSesion.Location = new System.Drawing.Point(828, 12);
        this.btnCerrarSesion.Size = new System.Drawing.Size(126, 34);
        this.btnCerrarSesion.Text = "Cerrar Sesión";

        this.pnlContenedorVistas.BackColor = System.Drawing.Color.FromArgb(245, 245, 247);
        this.pnlContenedorVistas.Controls.Add(this.pnlSecRestoreSupabase);
        this.pnlContenedorVistas.Controls.Add(this.pnlSecDigitoVerificador);
        this.pnlContenedorVistas.Controls.Add(this.pnlSecRoles);
        this.pnlContenedorVistas.Controls.Add(this.pnlSecPuertos);
        this.pnlContenedorVistas.Controls.Add(this.pnlSecForwarders);
        this.pnlContenedorVistas.Dock = System.Windows.Forms.DockStyle.Fill;
        this.pnlContenedorVistas.Padding = new System.Windows.Forms.Padding(16);

        // ====================================================================
        // SECCIÓN 1: FORWARDERS (ILIMITADOS, 4 POR FILA EN WEB, ORDEN ÚNICO)
        // ====================================================================
        this.pnlSecForwarders.BackColor = System.Drawing.Color.FromArgb(245, 245, 247);
        this.pnlSecForwarders.Controls.Add(this.splitForwarders);
        this.pnlSecForwarders.Dock = System.Windows.Forms.DockStyle.Fill;

        this.splitForwarders.BackColor = System.Drawing.Color.FromArgb(245, 245, 247);
        this.splitForwarders.Dock = System.Windows.Forms.DockStyle.Fill;
        this.splitForwarders.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
        this.splitForwarders.Panel1.Controls.Add(this.pnlFwdGridWrap);
        this.splitForwarders.Panel2.Controls.Add(this.pnlFwdForm);
        this.splitForwarders.Size = new System.Drawing.Size(942, 611);
        this.splitForwarders.SplitterDistance = 505;
        this.splitForwarders.SplitterWidth = 12;

        this.pnlFwdGridWrap.Controls.Add(this.dgvForwarders);
        this.pnlFwdGridWrap.Controls.Add(this.lblFwdGridTitle);
        this.pnlFwdGridWrap.Dock = System.Windows.Forms.DockStyle.Fill;
        this.pnlFwdGridWrap.Padding = new System.Windows.Forms.Padding(16);

        this.lblFwdGridTitle.Dock = System.Windows.Forms.DockStyle.Top;
        this.lblFwdGridTitle.Font = new System.Drawing.Font("Segoe UI Semibold", 10F);
        this.lblFwdGridTitle.ForeColor = System.Drawing.Color.FromArgb(29, 29, 31);
        this.lblFwdGridTitle.Height = 30;
        this.lblFwdGridTitle.Text = "Forwarders Activos (Grilla Web de 4 por fila · Sin límite)";

        this.dgvForwarders.Dock = System.Windows.Forms.DockStyle.Fill;

        this.pnlFwdForm.AutoScroll = true;
        this.pnlFwdForm.Controls.Add(this.btnEliminarFwd);
        this.pnlFwdForm.Controls.Add(this.btnNuevoFwd);
        this.pnlFwdForm.Controls.Add(this.btnGuardarFwd);
        this.pnlFwdForm.Controls.Add(this.chkFwdActivo);
        this.pnlFwdForm.Controls.Add(this.nudFwdOrden);
        this.pnlFwdForm.Controls.Add(this.lblFwdOrden);
        this.pnlFwdForm.Controls.Add(this.txtFwdEmail);
        this.pnlFwdForm.Controls.Add(this.lblFwdMail);
        this.pnlFwdForm.Controls.Add(this.txtFwdWhatsapp);
        this.pnlFwdForm.Controls.Add(this.lblFwdWa);
        this.pnlFwdForm.Controls.Add(this.txtFwdNombre);
        this.pnlFwdForm.Controls.Add(this.lblContadorNombreFwd);
        this.pnlFwdForm.Controls.Add(this.lblFwdNombre);
        this.pnlFwdForm.Controls.Add(this.txtFwdImagenUrl);
        this.pnlFwdForm.Controls.Add(this.lblFwdUrl);
        this.pnlFwdForm.Controls.Add(this.btnSubirFoto);
        this.pnlFwdForm.Controls.Add(this.picForwarder);
        this.pnlFwdForm.Controls.Add(this.lblFwdEditorTitulo);
        this.pnlFwdForm.Dock = System.Windows.Forms.DockStyle.Fill;

        this.lblFwdEditorTitulo.AutoSize = true;
        this.lblFwdEditorTitulo.Font = new System.Drawing.Font("Segoe UI Semibold", 12F);
        this.lblFwdEditorTitulo.ForeColor = System.Drawing.Color.FromArgb(29, 29, 31);
        this.lblFwdEditorTitulo.Location = new System.Drawing.Point(20, 16);
        this.lblFwdEditorTitulo.Text = "ABM de Forwarder";

        this.picForwarder.BackColor = System.Drawing.Color.FromArgb(245, 245, 247);
        this.picForwarder.Location = new System.Drawing.Point(20, 50);
        this.picForwarder.Size = new System.Drawing.Size(104, 104);
        this.picForwarder.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;

        this.btnSubirFoto.BackColor = System.Drawing.Color.FromArgb(245, 245, 247);
        this.btnSubirFoto.BorderColor = System.Drawing.Color.FromArgb(229, 231, 235);
        this.btnSubirFoto.BorderThickness = 1;
        this.btnSubirFoto.CornerRadius = 10;
        this.btnSubirFoto.ForeColor = System.Drawing.Color.FromArgb(83, 6, 24);
        this.btnSubirFoto.HoverBackColor = System.Drawing.Color.FromArgb(255, 240, 243);
        this.btnSubirFoto.Location = new System.Drawing.Point(138, 50);
        this.btnSubirFoto.Size = new System.Drawing.Size(262, 36);
        this.btnSubirFoto.Text = "Seleccionar Imagen...";

        this.lblFwdUrl.AutoSize = true;
        this.lblFwdUrl.Font = new System.Drawing.Font("Segoe UI Semibold", 8F);
        this.lblFwdUrl.ForeColor = System.Drawing.Color.FromArgb(110, 110, 115);
        this.lblFwdUrl.Location = new System.Drawing.Point(138, 96);
        this.lblFwdUrl.Text = "URL DE IMAGEN";

        this.txtFwdImagenUrl.Location = new System.Drawing.Point(138, 115);
        this.txtFwdImagenUrl.Size = new System.Drawing.Size(262, 23);

        this.lblFwdNombre.AutoSize = true;
        this.lblFwdNombre.Font = new System.Drawing.Font("Segoe UI Semibold", 8F);
        this.lblFwdNombre.ForeColor = System.Drawing.Color.FromArgb(110, 110, 115);
        this.lblFwdNombre.Location = new System.Drawing.Point(20, 170);
        this.lblFwdNombre.Text = "NOMBRE DEL FORWARDER (MÁX. 120 CARACTERES)";

        this.lblContadorNombreFwd.AutoSize = true;
        this.lblContadorNombreFwd.Font = new System.Drawing.Font("Segoe UI Semibold", 8F);
        this.lblContadorNombreFwd.ForeColor = System.Drawing.Color.FromArgb(83, 6, 24);
        this.lblContadorNombreFwd.Location = new System.Drawing.Point(354, 170);
        this.lblContadorNombreFwd.Text = "0 / 120";

        this.txtFwdNombre.Location = new System.Drawing.Point(20, 190);
        this.txtFwdNombre.MaxLength = 120;
        this.txtFwdNombre.Multiline = true;
        this.txtFwdNombre.Size = new System.Drawing.Size(380, 48);

        this.lblFwdWa.AutoSize = true;
        this.lblFwdWa.Font = new System.Drawing.Font("Segoe UI Semibold", 8F);
        this.lblFwdWa.ForeColor = System.Drawing.Color.FromArgb(110, 110, 115);
        this.lblFwdWa.Location = new System.Drawing.Point(20, 250);
        this.lblFwdWa.Text = "WHATSAPP (REDIRECCIÓN DIRECTA)";

        this.txtFwdWhatsapp.Location = new System.Drawing.Point(20, 270);
        this.txtFwdWhatsapp.PlaceholderText = "+5491143218800";
        this.txtFwdWhatsapp.Size = new System.Drawing.Size(380, 23);

        this.lblFwdMail.AutoSize = true;
        this.lblFwdMail.Font = new System.Drawing.Font("Segoe UI Semibold", 8F);
        this.lblFwdMail.ForeColor = System.Drawing.Color.FromArgb(110, 110, 115);
        this.lblFwdMail.Location = new System.Drawing.Point(20, 306);
        this.lblFwdMail.Text = "MAIL OFICIAL (COPIADO EN WEB)";

        this.txtFwdEmail.Location = new System.Drawing.Point(20, 326);
        this.txtFwdEmail.PlaceholderText = "contacto@importaplus.com.ar";
        this.txtFwdEmail.Size = new System.Drawing.Size(380, 23);

        this.lblFwdOrden.AutoSize = true;
        this.lblFwdOrden.Font = new System.Drawing.Font("Segoe UI Semibold", 8F);
        this.lblFwdOrden.ForeColor = System.Drawing.Color.FromArgb(110, 110, 115);
        this.lblFwdOrden.Location = new System.Drawing.Point(20, 364);
        this.lblFwdOrden.Text = "ORDEN ÚNICO EN GRILLA:";

        this.nudFwdOrden.Location = new System.Drawing.Point(20, 384);
        this.nudFwdOrden.Maximum = new decimal(new int[] { 999, 0, 0, 0 });
        this.nudFwdOrden.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
        this.nudFwdOrden.Size = new System.Drawing.Size(110, 23);
        this.nudFwdOrden.Value = new decimal(new int[] { 1, 0, 0, 0 });

        this.chkFwdActivo.AutoSize = true;
        this.chkFwdActivo.Checked = true;
        this.chkFwdActivo.Font = new System.Drawing.Font("Segoe UI Semibold", 9F);
        this.chkFwdActivo.Location = new System.Drawing.Point(150, 386);
        this.chkFwdActivo.Text = "Mostrar activo en la Web";

        this.btnGuardarFwd.BackColor = System.Drawing.Color.FromArgb(83, 6, 24);
        this.btnGuardarFwd.CornerRadius = 12;
        this.btnGuardarFwd.ForeColor = System.Drawing.Color.White;
        this.btnGuardarFwd.HoverBackColor = System.Drawing.Color.FromArgb(115, 16, 40);
        this.btnGuardarFwd.Location = new System.Drawing.Point(20, 428);
        this.btnGuardarFwd.Size = new System.Drawing.Size(380, 42);
        this.btnGuardarFwd.Text = "Guardar Forwarder";

        this.btnNuevoFwd.BackColor = System.Drawing.Color.FromArgb(245, 245, 247);
        this.btnNuevoFwd.BorderColor = System.Drawing.Color.FromArgb(229, 231, 235);
        this.btnNuevoFwd.BorderThickness = 1;
        this.btnNuevoFwd.CornerRadius = 10;
        this.btnNuevoFwd.ForeColor = System.Drawing.Color.FromArgb(29, 29, 31);
        this.btnNuevoFwd.Location = new System.Drawing.Point(20, 482);
        this.btnNuevoFwd.Size = new System.Drawing.Size(184, 38);
        this.btnNuevoFwd.Text = "Nuevo / Limpiar";

        this.btnEliminarFwd.BackColor = System.Drawing.Color.FromArgb(255, 241, 242);
        this.btnEliminarFwd.BorderColor = System.Drawing.Color.FromArgb(254, 205, 211);
        this.btnEliminarFwd.BorderThickness = 1;
        this.btnEliminarFwd.CornerRadius = 10;
        this.btnEliminarFwd.ForeColor = System.Drawing.Color.FromArgb(159, 18, 57);
        this.btnEliminarFwd.Location = new System.Drawing.Point(216, 482);
        this.btnEliminarFwd.Size = new System.Drawing.Size(184, 38);
        this.btnEliminarFwd.Text = "Eliminar";

        // ====================================================================
        // SECCIÓN 2: PUERTOS Y ADUANAS
        // ====================================================================
        this.pnlSecPuertos.BackColor = System.Drawing.Color.FromArgb(245, 245, 247);
        this.pnlSecPuertos.Controls.Add(this.splitPuertos);
        this.pnlSecPuertos.Dock = System.Windows.Forms.DockStyle.Fill;

        this.splitPuertos.BackColor = System.Drawing.Color.FromArgb(245, 245, 247);
        this.splitPuertos.Dock = System.Windows.Forms.DockStyle.Fill;
        this.splitPuertos.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
        this.splitPuertos.Panel1.Controls.Add(this.pnlPtoGridWrap);
        this.splitPuertos.Panel2.Controls.Add(this.pnlPtoForm);
        this.splitPuertos.Size = new System.Drawing.Size(942, 611);
        this.splitPuertos.SplitterDistance = 515;
        this.splitPuertos.SplitterWidth = 12;

        this.pnlPtoGridWrap.Controls.Add(this.dgvPuertos);
        this.pnlPtoGridWrap.Dock = System.Windows.Forms.DockStyle.Fill;
        this.pnlPtoGridWrap.Padding = new System.Windows.Forms.Padding(16);
        this.dgvPuertos.Dock = System.Windows.Forms.DockStyle.Fill;

        this.pnlPtoForm.AutoScroll = true;
        this.pnlPtoForm.Controls.Add(this.btnBorrarPto);
        this.pnlPtoForm.Controls.Add(this.btnNuevoPto);
        this.pnlPtoForm.Controls.Add(this.btnGuardarPto);
        this.pnlPtoForm.Controls.Add(this.txtPuertoLng);
        this.pnlPtoForm.Controls.Add(this.lblPtoLng);
        this.pnlPtoForm.Controls.Add(this.txtPuertoLat);
        this.pnlPtoForm.Controls.Add(this.lblPtoLat);
        this.pnlPtoForm.Controls.Add(this.txtPuertoInfra);
        this.pnlPtoForm.Controls.Add(this.lblPtoInfra);
        this.pnlPtoForm.Controls.Add(this.txtPuertoOperador);
        this.pnlPtoForm.Controls.Add(this.lblPtoOperador);
        this.pnlPtoForm.Controls.Add(this.txtPuertoSim);
        this.pnlPtoForm.Controls.Add(this.lblPtoSim);
        this.pnlPtoForm.Controls.Add(this.txtPuertoProvincia);
        this.pnlPtoForm.Controls.Add(this.lblPtoProvincia);
        this.pnlPtoForm.Controls.Add(this.cmbPuertoVia);
        this.pnlPtoForm.Controls.Add(this.lblPtoVia);
        this.pnlPtoForm.Controls.Add(this.txtPuertoNombre);
        this.pnlPtoForm.Controls.Add(this.lblPtoNombre);
        this.pnlPtoForm.Controls.Add(this.lblPtoTit);
        this.pnlPtoForm.Dock = System.Windows.Forms.DockStyle.Fill;

        this.lblPtoTit.AutoSize = true; this.lblPtoTit.Font = new System.Drawing.Font("Segoe UI Semibold", 12F);
        this.lblPtoTit.Location = new System.Drawing.Point(20, 16); this.lblPtoTit.Text = "ABM Puerto / Aduana";
        this.lblPtoNombre.AutoSize = true; this.lblPtoNombre.Font = new System.Drawing.Font("Segoe UI Semibold", 8F);
        this.lblPtoNombre.Location = new System.Drawing.Point(20, 50); this.lblPtoNombre.Text = "NOMBRE *";
        this.txtPuertoNombre.Location = new System.Drawing.Point(20, 70); this.txtPuertoNombre.Size = new System.Drawing.Size(370, 23);
        this.lblPtoVia.AutoSize = true; this.lblPtoVia.Font = new System.Drawing.Font("Segoe UI Semibold", 8F);
        this.lblPtoVia.Location = new System.Drawing.Point(20, 104); this.lblPtoVia.Text = "VÍA DE INGRESO *";
        this.cmbPuertoVia.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
        this.cmbPuertoVia.Items.AddRange(new object[] { "MARITIMA_FLUVIAL", "AEREA", "TERRESTRE" });
        this.cmbPuertoVia.Location = new System.Drawing.Point(20, 124); this.cmbPuertoVia.Size = new System.Drawing.Size(370, 23);
        this.lblPtoProvincia.AutoSize = true; this.lblPtoProvincia.Font = new System.Drawing.Font("Segoe UI Semibold", 8F);
        this.lblPtoProvincia.Location = new System.Drawing.Point(20, 158); this.lblPtoProvincia.Text = "PROVINCIA *";
        this.txtPuertoProvincia.Location = new System.Drawing.Point(20, 178); this.txtPuertoProvincia.Size = new System.Drawing.Size(370, 23);
        this.lblPtoSim.AutoSize = true; this.lblPtoSim.Font = new System.Drawing.Font("Segoe UI Semibold", 8F);
        this.lblPtoSim.Location = new System.Drawing.Point(20, 212); this.lblPtoSim.Text = "CÓDIGO SIM";
        this.txtPuertoSim.Location = new System.Drawing.Point(20, 232); this.txtPuertoSim.Size = new System.Drawing.Size(175, 23);
        this.lblPtoOperador.AutoSize = true; this.lblPtoOperador.Font = new System.Drawing.Font("Segoe UI Semibold", 8F);
        this.lblPtoOperador.Location = new System.Drawing.Point(210, 212); this.lblPtoOperador.Text = "OPERADOR";
        this.txtPuertoOperador.Location = new System.Drawing.Point(210, 232); this.txtPuertoOperador.Size = new System.Drawing.Size(180, 23);
        this.lblPtoInfra.AutoSize = true; this.lblPtoInfra.Font = new System.Drawing.Font("Segoe UI Semibold", 8F);
        this.lblPtoInfra.Location = new System.Drawing.Point(20, 266); this.lblPtoInfra.Text = "INFRAESTRUCTURA";
        this.txtPuertoInfra.Location = new System.Drawing.Point(20, 286); this.txtPuertoInfra.Multiline = true; this.txtPuertoInfra.Size = new System.Drawing.Size(370, 52);
        this.lblPtoLat.AutoSize = true; this.lblPtoLat.Font = new System.Drawing.Font("Segoe UI Semibold", 8F);
        this.lblPtoLat.Location = new System.Drawing.Point(20, 348); this.lblPtoLat.Text = "LATITUD";
        this.txtPuertoLat.Location = new System.Drawing.Point(20, 368); this.txtPuertoLat.Size = new System.Drawing.Size(175, 23);
        this.lblPtoLng.AutoSize = true; this.lblPtoLng.Font = new System.Drawing.Font("Segoe UI Semibold", 8F);
        this.lblPtoLng.Location = new System.Drawing.Point(210, 348); this.lblPtoLng.Text = "LONGITUD";
        this.txtPuertoLng.Location = new System.Drawing.Point(210, 368); this.txtPuertoLng.Size = new System.Drawing.Size(180, 23);

        this.btnGuardarPto.BackColor = System.Drawing.Color.FromArgb(83, 6, 24);
        this.btnGuardarPto.CornerRadius = 12;
        this.btnGuardarPto.ForeColor = System.Drawing.Color.White;
        this.btnGuardarPto.Location = new System.Drawing.Point(20, 414); this.btnGuardarPto.Size = new System.Drawing.Size(370, 40);
        this.btnGuardarPto.Text = "Guardar Puerto / Aduana";

        this.btnNuevoPto.BackColor = System.Drawing.Color.FromArgb(245, 245, 247);
        this.btnNuevoPto.BorderColor = System.Drawing.Color.FromArgb(229, 231, 235);
        this.btnNuevoPto.BorderThickness = 1;
        this.btnNuevoPto.CornerRadius = 10;
        this.btnNuevoPto.ForeColor = System.Drawing.Color.FromArgb(29, 29, 31);
        this.btnNuevoPto.Location = new System.Drawing.Point(20, 466); this.btnNuevoPto.Size = new System.Drawing.Size(178, 36);
        this.btnNuevoPto.Text = "Nuevo Puerto";

        this.btnBorrarPto.BackColor = System.Drawing.Color.FromArgb(255, 241, 242);
        this.btnBorrarPto.BorderColor = System.Drawing.Color.FromArgb(254, 205, 211);
        this.btnBorrarPto.BorderThickness = 1;
        this.btnBorrarPto.CornerRadius = 10;
        this.btnBorrarPto.ForeColor = System.Drawing.Color.FromArgb(159, 18, 57);
        this.btnBorrarPto.Location = new System.Drawing.Point(212, 466); this.btnBorrarPto.Size = new System.Drawing.Size(178, 36);
        this.btnBorrarPto.Text = "Eliminar";

        // ====================================================================
        // SECCIÓN 3: ROLES Y PERMISOS (PATRÓN COMPOSITE + TREEVIEW)
        // ====================================================================
        this.pnlSecRoles.BackColor = System.Drawing.Color.FromArgb(245, 245, 247);
        this.pnlSecRoles.Controls.Add(this.splitRoles);
        this.pnlSecRoles.Dock = System.Windows.Forms.DockStyle.Fill;

        this.splitRoles.BackColor = System.Drawing.Color.FromArgb(245, 245, 247);
        this.splitRoles.Dock = System.Windows.Forms.DockStyle.Fill;
        this.splitRoles.Panel1.Controls.Add(this.pnlRolesIzq);
        this.splitRoles.Panel2.Controls.Add(this.pnlRolesDer);
        this.splitRoles.Size = new System.Drawing.Size(942, 611);
        this.splitRoles.SplitterDistance = 440;
        this.splitRoles.SplitterWidth = 12;

        this.pnlRolesIzq.AutoScroll = true;
        this.pnlRolesIzq.Controls.Add(this.btnCrearPerfil);
        this.pnlRolesIzq.Controls.Add(this.txtPerfilNombre);
        this.pnlRolesIzq.Controls.Add(this.lblPerfilSec);
        this.pnlRolesIzq.Controls.Add(this.btnAgregarAFamilia);
        this.pnlRolesIzq.Controls.Add(this.cmbComponentesParaFamilia);
        this.pnlRolesIzq.Controls.Add(this.cmbFamilias);
        this.pnlRolesIzq.Controls.Add(this.btnCrearFamilia);
        this.pnlRolesIzq.Controls.Add(this.txtFamiliaNombre);
        this.pnlRolesIzq.Controls.Add(this.lblFamiliaSec);
        this.pnlRolesIzq.Controls.Add(this.btnCrearPermiso);
        this.pnlRolesIzq.Controls.Add(this.txtPermisoNombre);
        this.pnlRolesIzq.Controls.Add(this.txtPermisoCodigo);
        this.pnlRolesIzq.Controls.Add(this.lblPermisoSec);
        this.pnlRolesIzq.Controls.Add(this.lblRolesTit);
        this.pnlRolesIzq.Dock = System.Windows.Forms.DockStyle.Fill;

        this.lblRolesTit.AutoSize = true;
        this.lblRolesTit.Font = new System.Drawing.Font("Segoe UI Semibold", 12F);
        this.lblRolesTit.ForeColor = System.Drawing.Color.FromArgb(29, 29, 31);
        this.lblRolesTit.Location = new System.Drawing.Point(20, 16);
        this.lblRolesTit.Text = "Roles Composite: Permisos · Familias · Perfiles";

        this.lblPermisoSec.AutoSize = true;
        this.lblPermisoSec.Font = new System.Drawing.Font("Segoe UI Semibold", 8.5F);
        this.lblPermisoSec.ForeColor = System.Drawing.Color.FromArgb(83, 6, 24);
        this.lblPermisoSec.Location = new System.Drawing.Point(20, 56);
        this.lblPermisoSec.Text = "1. CREAR PERMISO SIMPLE (HOJA)";

        this.txtPermisoCodigo.Location = new System.Drawing.Point(20, 78);
        this.txtPermisoCodigo.PlaceholderText = "Código (ej: REP_EXPORT)";
        this.txtPermisoCodigo.Size = new System.Drawing.Size(160, 23);

        this.txtPermisoNombre.Location = new System.Drawing.Point(190, 78);
        this.txtPermisoNombre.PlaceholderText = "Nombre del permiso";
        this.txtPermisoNombre.Size = new System.Drawing.Size(218, 23);

        this.btnCrearPermiso.BackColor = System.Drawing.Color.FromArgb(83, 6, 24);
        this.btnCrearPermiso.CornerRadius = 10;
        this.btnCrearPermiso.Location = new System.Drawing.Point(20, 110);
        this.btnCrearPermiso.Size = new System.Drawing.Size(388, 36);
        this.btnCrearPermiso.Text = "+ Crear Permiso Simple";

        this.lblFamiliaSec.AutoSize = true;
        this.lblFamiliaSec.Font = new System.Drawing.Font("Segoe UI Semibold", 8.5F);
        this.lblFamiliaSec.ForeColor = System.Drawing.Color.FromArgb(83, 6, 24);
        this.lblFamiliaSec.Location = new System.Drawing.Point(20, 170);
        this.lblFamiliaSec.Text = "2. GESTIÓN DE FAMILIAS DE PERMISOS (COMPOSITE)";

        this.txtFamiliaNombre.Location = new System.Drawing.Point(20, 192);
        this.txtFamiliaNombre.PlaceholderText = "Nombre de nueva familia...";
        this.txtFamiliaNombre.Size = new System.Drawing.Size(230, 23);

        this.btnCrearFamilia.BackColor = System.Drawing.Color.FromArgb(83, 6, 24);
        this.btnCrearFamilia.CornerRadius = 10;
        this.btnCrearFamilia.Location = new System.Drawing.Point(260, 188);
        this.btnCrearFamilia.Size = new System.Drawing.Size(148, 32);
        this.btnCrearFamilia.Text = "+ Crear Familia";

        this.cmbFamilias.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
        this.cmbFamilias.Location = new System.Drawing.Point(20, 234);
        this.cmbFamilias.Size = new System.Drawing.Size(388, 23);

        this.cmbComponentesParaFamilia.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
        this.cmbComponentesParaFamilia.Location = new System.Drawing.Point(20, 268);
        this.cmbComponentesParaFamilia.Size = new System.Drawing.Size(388, 23);

        this.btnAgregarAFamilia.BackColor = System.Drawing.Color.FromArgb(245, 245, 247);
        this.btnAgregarAFamilia.BorderColor = System.Drawing.Color.FromArgb(229, 231, 235);
        this.btnAgregarAFamilia.BorderThickness = 1;
        this.btnAgregarAFamilia.CornerRadius = 10;
        this.btnAgregarAFamilia.ForeColor = System.Drawing.Color.FromArgb(83, 6, 24);
        this.btnAgregarAFamilia.Location = new System.Drawing.Point(20, 302);
        this.btnAgregarAFamilia.Size = new System.Drawing.Size(388, 36);
        this.btnAgregarAFamilia.Text = "Agregar Permiso / Sub-Familia a la Familia";

        this.lblPerfilSec.AutoSize = true;
        this.lblPerfilSec.Font = new System.Drawing.Font("Segoe UI Semibold", 8.5F);
        this.lblPerfilSec.ForeColor = System.Drawing.Color.FromArgb(83, 6, 24);
        this.lblPerfilSec.Location = new System.Drawing.Point(20, 366);
        this.lblPerfilSec.Text = "3. CREAR PERFIL (ROL)";

        this.txtPerfilNombre.Location = new System.Drawing.Point(20, 388);
        this.txtPerfilNombre.PlaceholderText = "Nombre del nuevo perfil (ej: Auditor)...";
        this.txtPerfilNombre.Size = new System.Drawing.Size(230, 23);

        this.btnCrearPerfil.BackColor = System.Drawing.Color.FromArgb(83, 6, 24);
        this.btnCrearPerfil.CornerRadius = 10;
        this.btnCrearPerfil.Location = new System.Drawing.Point(260, 384);
        this.btnCrearPerfil.Size = new System.Drawing.Size(148, 32);
        this.btnCrearPerfil.Text = "+ Crear Perfil";

        // Panel Derecho Roles: Selección de Perfil + Asignación + TreeView
        this.pnlRolesDer.Controls.Add(this.lblReglaComposite);
        this.pnlRolesDer.Controls.Add(this.tvPermisosPerfil);
        this.pnlRolesDer.Controls.Add(this.btnQuitarDePerfil);
        this.pnlRolesDer.Controls.Add(this.btnAsignarAPerfil);
        this.pnlRolesDer.Controls.Add(this.cmbComponentesParaPerfil);
        this.pnlRolesDer.Controls.Add(this.cmbPerfiles);
        this.pnlRolesDer.Controls.Add(this.lblTreeTit);
        this.pnlRolesDer.Dock = System.Windows.Forms.DockStyle.Fill;

        this.lblTreeTit.AutoSize = true;
        this.lblTreeTit.Font = new System.Drawing.Font("Segoe UI Semibold", 12F);
        this.lblTreeTit.ForeColor = System.Drawing.Color.FromArgb(29, 29, 31);
        this.lblTreeTit.Location = new System.Drawing.Point(20, 16);
        this.lblTreeTit.Text = "Árbol de Permisos del Perfil (TreeView)";

        this.cmbPerfiles.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
        this.cmbPerfiles.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
        this.cmbPerfiles.Location = new System.Drawing.Point(20, 52);
        this.cmbPerfiles.Size = new System.Drawing.Size(450, 23);

        this.cmbComponentesParaPerfil.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
        this.cmbComponentesParaPerfil.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
        this.cmbComponentesParaPerfil.Location = new System.Drawing.Point(20, 86);
        this.cmbComponentesParaPerfil.Size = new System.Drawing.Size(450, 23);

        this.btnAsignarAPerfil.BackColor = System.Drawing.Color.FromArgb(83, 6, 24);
        this.btnAsignarAPerfil.CornerRadius = 10;
        this.btnAsignarAPerfil.Location = new System.Drawing.Point(20, 120);
        this.btnAsignarAPerfil.Size = new System.Drawing.Size(250, 36);
        this.btnAsignarAPerfil.Text = "Asignar al Perfil";

        this.btnQuitarDePerfil.BackColor = System.Drawing.Color.FromArgb(255, 241, 242);
        this.btnQuitarDePerfil.BorderColor = System.Drawing.Color.FromArgb(254, 205, 211);
        this.btnQuitarDePerfil.BorderThickness = 1;
        this.btnQuitarDePerfil.CornerRadius = 10;
        this.btnQuitarDePerfil.ForeColor = System.Drawing.Color.FromArgb(159, 18, 57);
        this.btnQuitarDePerfil.Location = new System.Drawing.Point(282, 120);
        this.btnQuitarDePerfil.Size = new System.Drawing.Size(188, 36);
        this.btnQuitarDePerfil.Text = "Quitar Nodo Raíz";

        this.tvPermisosPerfil.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
        this.tvPermisosPerfil.BackColor = System.Drawing.Color.FromArgb(250, 250, 252);
        this.tvPermisosPerfil.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
        this.tvPermisosPerfil.Font = new System.Drawing.Font("Segoe UI", 9.5F);
        this.tvPermisosPerfil.Location = new System.Drawing.Point(20, 170);
        this.tvPermisosPerfil.Name = "tvPermisosPerfil";
        this.tvPermisosPerfil.Size = new System.Drawing.Size(450, 385);

        this.lblReglaComposite.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
        this.lblReglaComposite.Font = new System.Drawing.Font("Segoe UI", 8.5F);
        this.lblReglaComposite.ForeColor = System.Drawing.Color.FromArgb(110, 110, 115);
        this.lblReglaComposite.Location = new System.Drawing.Point(20, 566);
        this.lblReglaComposite.Size = new System.Drawing.Size(450, 32);
        this.lblReglaComposite.Text = "Regla activa: un perfil no puede recibir el mismo permiso dos veces (ni directo ni heredado por familia).";

        // ====================================================================
        // SECCIÓN 4: DÍGITO VERIFICADOR (CONTEO REAL TOTAL SUPABASE)
        // ====================================================================
        this.pnlSecDigitoVerificador.BackColor = System.Drawing.Color.FromArgb(245, 245, 247);
        this.pnlSecDigitoVerificador.Controls.Add(this.pnlDvCard);
        this.pnlSecDigitoVerificador.Dock = System.Windows.Forms.DockStyle.Fill;

        this.pnlDvCard.Controls.Add(this.pnlDvReporte);
        this.pnlDvCard.Controls.Add(this.dgvDvv);
        this.pnlDvCard.Controls.Add(this.btnRecalcularDv);
        this.pnlDvCard.Controls.Add(this.btnVerificarDv);
        this.pnlDvCard.Controls.Add(this.lblDvTit);
        this.pnlDvCard.Dock = System.Windows.Forms.DockStyle.Fill;

        this.lblDvTit.AutoSize = true;
        this.lblDvTit.Font = new System.Drawing.Font("Segoe UI Semibold", 12F);
        this.lblDvTit.Location = new System.Drawing.Point(22, 18);
        this.lblDvTit.Text = "Auditoría de Integridad Horizontal (DVH) y Vertical (DVV)";

        this.btnVerificarDv.BackColor = System.Drawing.Color.FromArgb(245, 245, 247);
        this.btnVerificarDv.BorderColor = System.Drawing.Color.FromArgb(229, 231, 235);
        this.btnVerificarDv.BorderThickness = 1;
        this.btnVerificarDv.CornerRadius = 10;
        this.btnVerificarDv.ForeColor = System.Drawing.Color.FromArgb(83, 6, 24);
        this.btnVerificarDv.Location = new System.Drawing.Point(22, 54);
        this.btnVerificarDv.Size = new System.Drawing.Size(220, 40);
        this.btnVerificarDv.Text = "Verificar Integridad Ahora";

        this.btnRecalcularDv.BackColor = System.Drawing.Color.FromArgb(83, 6, 24);
        this.btnRecalcularDv.CornerRadius = 10;
        this.btnRecalcularDv.Location = new System.Drawing.Point(254, 54);
        this.btnRecalcularDv.Size = new System.Drawing.Size(250, 40);
        this.btnRecalcularDv.Text = "Recalcular Digito Verificador";

        this.dgvDvv.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
        this.dgvDvv.Location = new System.Drawing.Point(22, 110);
        this.dgvDvv.Size = new System.Drawing.Size(898, 235);

        this.pnlDvReporte.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
        this.pnlDvReporte.AutoScroll = true;
        this.pnlDvReporte.BackColor = System.Drawing.Color.FromArgb(255, 248, 249);
        this.pnlDvReporte.BorderColor = System.Drawing.Color.FromArgb(240, 212, 218);
        this.pnlDvReporte.Controls.Add(this.lblDetalleIntegridadPanel);
        this.pnlDvReporte.Location = new System.Drawing.Point(22, 362);
        this.pnlDvReporte.Size = new System.Drawing.Size(898, 225);

        this.lblDetalleIntegridadPanel.AutoSize = true;
        this.lblDetalleIntegridadPanel.Location = new System.Drawing.Point(16, 16);
        this.lblDetalleIntegridadPanel.MaximumSize = new System.Drawing.Size(850, 0);
        this.lblDetalleIntegridadPanel.Text = "Estado de integridad...";

        // ====================================================================
        // SECCIÓN 5: RESTOREBACKUP & SUPABASE
        // ====================================================================
        this.pnlSecRestoreSupabase.BackColor = System.Drawing.Color.FromArgb(245, 245, 247);
        this.pnlSecRestoreSupabase.Controls.Add(this.splitRestoreSupa);
        this.pnlSecRestoreSupabase.Dock = System.Windows.Forms.DockStyle.Fill;

        this.splitRestoreSupa.BackColor = System.Drawing.Color.FromArgb(245, 245, 247);
        this.splitRestoreSupa.Dock = System.Windows.Forms.DockStyle.Fill;
        this.splitRestoreSupa.Panel1.Controls.Add(this.pnlBackupCard);
        this.splitRestoreSupa.Panel2.Controls.Add(this.pnlSupaCard);
        this.splitRestoreSupa.Size = new System.Drawing.Size(942, 611);
        this.splitRestoreSupa.SplitterDistance = 465;
        this.splitRestoreSupa.SplitterWidth = 12;

        this.pnlBackupCard.Controls.Add(this.btnEjecutarRestore);
        this.pnlBackupCard.Controls.Add(this.pnlPreviewBk);
        this.pnlBackupCard.Controls.Add(this.btnExaminarBk);
        this.pnlBackupCard.Controls.Add(this.txtRutaRestore);
        this.pnlBackupCard.Controls.Add(this.lblBkSel);
        this.pnlBackupCard.Controls.Add(this.btnCrearBackup);
        this.pnlBackupCard.Controls.Add(this.lblBkDesc);
        this.pnlBackupCard.Controls.Add(this.lblBkTit);
        this.pnlBackupCard.Controls.Add(this.lblBkTag);
        this.pnlBackupCard.Dock = System.Windows.Forms.DockStyle.Fill;

        this.lblBkTag.AutoSize = true; this.lblBkTag.Font = new System.Drawing.Font("Segoe UI Semibold", 8F);
        this.lblBkTag.ForeColor = System.Drawing.Color.FromArgb(83, 6, 24);
        this.lblBkTag.Location = new System.Drawing.Point(20, 18); this.lblBkTag.Text = "RESPALDO Y RECUPERACIÓN";
        this.lblBkTit.AutoSize = true; this.lblBkTit.Font = new System.Drawing.Font("Segoe UI Semibold", 13F);
        this.lblBkTit.Location = new System.Drawing.Point(18, 38); this.lblBkTit.Text = "Sistema RestoreBackup";
        this.lblBkDesc.Location = new System.Drawing.Point(20, 72); this.lblBkDesc.Size = new System.Drawing.Size(420, 42);
        this.lblBkDesc.Text = "Generá copias firmadas con SHA-256 o cargá un backup para restaurar la base actual.";

        this.btnCrearBackup.BackColor = System.Drawing.Color.FromArgb(5, 150, 105);
        this.btnCrearBackup.CornerRadius = 12;
        this.btnCrearBackup.Location = new System.Drawing.Point(20, 124); this.btnCrearBackup.Size = new System.Drawing.Size(420, 42);
        this.btnCrearBackup.Text = "1. Generar Backup de Base Actual";

        this.lblBkSel.AutoSize = true; this.lblBkSel.Font = new System.Drawing.Font("Segoe UI Semibold", 8F);
        this.lblBkSel.Location = new System.Drawing.Point(20, 184); this.lblBkSel.Text = "ARCHIVO BACKUP PARA CARGAR:";
        this.txtRutaRestore.Location = new System.Drawing.Point(20, 206); this.txtRutaRestore.ReadOnly = true; this.txtRutaRestore.Size = new System.Drawing.Size(295, 23);

        this.btnExaminarBk.BackColor = System.Drawing.Color.FromArgb(245, 245, 247);
        this.btnExaminarBk.BorderColor = System.Drawing.Color.FromArgb(229, 231, 235);
        this.btnExaminarBk.BorderThickness = 1;
        this.btnExaminarBk.CornerRadius = 8;
        this.btnExaminarBk.ForeColor = System.Drawing.Color.FromArgb(83, 6, 24);
        this.btnExaminarBk.Location = new System.Drawing.Point(324, 202); this.btnExaminarBk.Size = new System.Drawing.Size(116, 30);
        this.btnExaminarBk.Text = "Examinar...";

        this.pnlPreviewBk.BackColor = System.Drawing.Color.FromArgb(255, 248, 249);
        this.pnlPreviewBk.BorderColor = System.Drawing.Color.FromArgb(240, 212, 218);
        this.pnlPreviewBk.Controls.Add(this.lblPreviewBackup);
        this.pnlPreviewBk.Location = new System.Drawing.Point(20, 246); this.pnlPreviewBk.Padding = new System.Windows.Forms.Padding(12); this.pnlPreviewBk.Size = new System.Drawing.Size(420, 140);
        this.lblPreviewBackup.Dock = System.Windows.Forms.DockStyle.Fill;
        this.lblPreviewBackup.Text = "Seleccioná un backup para inspeccionarlo antes de restaurar.";

        this.btnEjecutarRestore.BackColor = System.Drawing.Color.FromArgb(83, 6, 24);
        this.btnEjecutarRestore.CornerRadius = 12;
        this.btnEjecutarRestore.Location = new System.Drawing.Point(20, 404); this.btnEjecutarRestore.Size = new System.Drawing.Size(420, 44);
        this.btnEjecutarRestore.Text = "2. Cargar Base Actual (RestoreBackup)";

        this.pnlSupaCard.Controls.Add(this.lblResultadoSupa);
        this.pnlSupaCard.Controls.Add(this.btnEmpujarNube);
        this.pnlSupaCard.Controls.Add(this.btnGuardarSupa);
        this.pnlSupaCard.Controls.Add(this.txtSupaAnonKey);
        this.pnlSupaCard.Controls.Add(this.lblSupaAnon);
        this.pnlSupaCard.Controls.Add(this.txtSupaServiceKey);
        this.pnlSupaCard.Controls.Add(this.lblSupaService);
        this.pnlSupaCard.Controls.Add(this.txtSupaUrl);
        this.pnlSupaCard.Controls.Add(this.lblSupaUrl);
        this.pnlSupaCard.Controls.Add(this.lblSpTit);
        this.pnlSupaCard.Controls.Add(this.lblSpTag);
        this.pnlSupaCard.Dock = System.Windows.Forms.DockStyle.Fill;

        this.lblSpTag.AutoSize = true; this.lblSpTag.Font = new System.Drawing.Font("Segoe UI Semibold", 8F);
        this.lblSpTag.ForeColor = System.Drawing.Color.FromArgb(83, 6, 24);
        this.lblSpTag.Location = new System.Drawing.Point(20, 18); this.lblSpTag.Text = "CONECTIVIDAD SUPABASE CLOUD";
        this.lblSpTit.AutoSize = true; this.lblSpTit.Font = new System.Drawing.Font("Segoe UI Semibold", 13F);
        this.lblSpTit.Location = new System.Drawing.Point(18, 38); this.lblSpTit.Text = "Configuración Proyecto Supabase";
        this.lblSupaUrl.AutoSize = true; this.lblSupaUrl.Font = new System.Drawing.Font("Segoe UI Semibold", 8F);
        this.lblSupaUrl.Location = new System.Drawing.Point(20, 86); this.lblSupaUrl.Text = "PROJECT URL:";
        this.txtSupaUrl.Location = new System.Drawing.Point(20, 106); this.txtSupaUrl.Size = new System.Drawing.Size(420, 23);
        this.lblSupaService.AutoSize = true; this.lblSupaService.Font = new System.Drawing.Font("Segoe UI Semibold", 8F);
        this.lblSupaService.Location = new System.Drawing.Point(20, 146); this.lblSupaService.Text = "SERVICE_ROLE / SECRET KEY:";
        this.txtSupaServiceKey.Location = new System.Drawing.Point(20, 166); this.txtSupaServiceKey.Size = new System.Drawing.Size(420, 23); this.txtSupaServiceKey.UseSystemPasswordChar = true;
        this.lblSupaAnon.AutoSize = true; this.lblSupaAnon.Font = new System.Drawing.Font("Segoe UI Semibold", 8F);
        this.lblSupaAnon.Location = new System.Drawing.Point(20, 206); this.lblSupaAnon.Text = "PUBLISHABLE / ANON KEY:";
        this.txtSupaAnonKey.Location = new System.Drawing.Point(20, 226); this.txtSupaAnonKey.Size = new System.Drawing.Size(420, 23);

        this.btnGuardarSupa.BackColor = System.Drawing.Color.FromArgb(83, 6, 24);
        this.btnGuardarSupa.CornerRadius = 12;
        this.btnGuardarSupa.Location = new System.Drawing.Point(20, 272); this.btnGuardarSupa.Size = new System.Drawing.Size(420, 42);
        this.btnGuardarSupa.Text = "Guardar y Probar Conexión Supabase";

        this.btnEmpujarNube.BackColor = System.Drawing.Color.FromArgb(245, 245, 247);
        this.btnEmpujarNube.BorderColor = System.Drawing.Color.FromArgb(229, 231, 235);
        this.btnEmpujarNube.BorderThickness = 1;
        this.btnEmpujarNube.CornerRadius = 10;
        this.btnEmpujarNube.ForeColor = System.Drawing.Color.FromArgb(83, 6, 24);
        this.btnEmpujarNube.Location = new System.Drawing.Point(20, 326); this.btnEmpujarNube.Size = new System.Drawing.Size(420, 38);
        this.btnEmpujarNube.Text = "Sincronizar Datos hacia Supabase";

        this.lblResultadoSupa.Location = new System.Drawing.Point(20, 382);
        this.lblResultadoSupa.Size = new System.Drawing.Size(420, 90);
        this.lblResultadoSupa.Text = "Conectado a Supabase Cloud.";

        // ====================================================================
        // FORM PRINCIPAL
        // ====================================================================
        this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.BackColor = System.Drawing.Color.FromArgb(245, 245, 247);
        this.ClientSize = new System.Drawing.Size(1204, 701);
        this.Controls.Add(this.pnlMain);
        this.Controls.Add(this.pnlSidebar);
        this.MinimumSize = new System.Drawing.Size(1080, 680);
        this.Name = "FrmPanelAdmin";
        this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
        this.Text = "IMPORTA+ · Panel de Administración";

        this.pnlSidebar.ResumeLayout(false);
        this.pnlMain.ResumeLayout(false);
        this.pnlContenedorVistas.ResumeLayout(false);
        this.pnlSecForwarders.ResumeLayout(false);
        this.splitForwarders.Panel1.ResumeLayout(false);
        this.splitForwarders.Panel2.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)(this.splitForwarders)).EndInit();
        this.splitForwarders.ResumeLayout(false);
        this.pnlFwdGridWrap.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)(this.dgvForwarders)).EndInit();
        this.pnlFwdForm.ResumeLayout(false);
        this.pnlFwdForm.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)(this.nudFwdOrden)).EndInit();
        ((System.ComponentModel.ISupportInitialize)(this.picForwarder)).EndInit();
        this.pnlSecPuertos.ResumeLayout(false);
        this.splitPuertos.Panel1.ResumeLayout(false);
        this.splitPuertos.Panel2.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)(this.splitPuertos)).EndInit();
        this.splitPuertos.ResumeLayout(false);
        this.pnlPtoGridWrap.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)(this.dgvPuertos)).EndInit();
        this.pnlPtoForm.ResumeLayout(false);
        this.pnlPtoForm.PerformLayout();
        this.pnlSecRoles.ResumeLayout(false);
        this.splitRoles.Panel1.ResumeLayout(false);
        this.splitRoles.Panel2.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)(this.splitRoles)).EndInit();
        this.splitRoles.ResumeLayout(false);
        this.pnlRolesIzq.ResumeLayout(false);
        this.pnlRolesIzq.PerformLayout();
        this.pnlRolesDer.ResumeLayout(false);
        this.pnlRolesDer.PerformLayout();
        this.pnlSecDigitoVerificador.ResumeLayout(false);
        this.pnlDvCard.ResumeLayout(false);
        this.pnlDvCard.PerformLayout();
        this.pnlDvReporte.ResumeLayout(false);
        this.pnlDvReporte.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)(this.dgvDvv)).EndInit();
        this.pnlSecRestoreSupabase.ResumeLayout(false);
        this.splitRestoreSupa.Panel1.ResumeLayout(false);
        this.splitRestoreSupa.Panel2.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)(this.splitRestoreSupa)).EndInit();
        this.splitRestoreSupa.ResumeLayout(false);
        this.pnlBackupCard.ResumeLayout(false);
        this.pnlBackupCard.PerformLayout();
        this.pnlPreviewBk.ResumeLayout(false);
        this.pnlSupaCard.ResumeLayout(false);
        this.pnlSupaCard.PerformLayout();
        this.pnlTopBar.ResumeLayout(false);
        this.pnlTopBar.PerformLayout();
        this.ResumeLayout(false);
    }
}
