namespace AdminImportaPlus.GUI;

#nullable enable
partial class FrmPanelAdmin
{
    private System.ComponentModel.IContainer? components = null;

    // Sidebar Izquierdo Estilo Apple + Área Principal (7 secciones + Cambiar Contraseña)
    private System.Windows.Forms.Panel pnlSidebar = null!;
    private AdminImportaPlus.GUI.AppleButton btnNavForwarders = null!;
    private AdminImportaPlus.GUI.AppleButton btnNavDespachantes = null!;
    private AdminImportaPlus.GUI.AppleButton btnNavPuertos = null!;
    private AdminImportaPlus.GUI.AppleButton btnNavUsuarios = null!;
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

    // Sección 0: Forwarders (Ilimitados, orden único)
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

    // Sección 1: Despachantes de Aduana (Misma interfaz que Forwarders, catálogo independiente)
    private System.Windows.Forms.Panel pnlSecDespachantes = null!;
    private System.Windows.Forms.SplitContainer splitDespachantes = null!;
    private AdminImportaPlus.GUI.AppleCardPanel pnlDespGridWrap = null!;
    private System.Windows.Forms.Label lblDespGridTitle = null!;
    private System.Windows.Forms.DataGridView dgvDespachantes = null!;
    private AdminImportaPlus.GUI.AppleCardPanel pnlDespForm = null!;
    private System.Windows.Forms.Label lblDespEditorTitulo = null!;
    private System.Windows.Forms.PictureBox picDespachante = null!;
    private AdminImportaPlus.GUI.AppleButton btnSubirFotoDesp = null!;
    private System.Windows.Forms.Label lblDespUrl = null!;
    private System.Windows.Forms.TextBox txtDespImagenUrl = null!;
    private System.Windows.Forms.Label lblDespNombre = null!;
    private System.Windows.Forms.Label lblContadorNombreDesp = null!;
    private System.Windows.Forms.TextBox txtDespNombre = null!;
    private System.Windows.Forms.Label lblDespWa = null!;
    private System.Windows.Forms.TextBox txtDespWhatsapp = null!;
    private System.Windows.Forms.Label lblDespMail = null!;
    private System.Windows.Forms.TextBox txtDespEmail = null!;
    private System.Windows.Forms.Label lblDespOrden = null!;
    private System.Windows.Forms.NumericUpDown nudDespOrden = null!;
    private System.Windows.Forms.CheckBox chkDespActivo = null!;
    private AdminImportaPlus.GUI.AppleButton btnGuardarDesp = null!;
    private AdminImportaPlus.GUI.AppleButton btnNuevoDesp = null!;
    private AdminImportaPlus.GUI.AppleButton btnEliminarDesp = null!;

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
    private System.Windows.Forms.Label lblPtoMapsUrl = null!;
    private System.Windows.Forms.TextBox txtPuertoMapsUrl = null!;
    private AdminImportaPlus.GUI.AppleButton btnActualizarLinkMapsPto = null!;
    private AdminImportaPlus.GUI.AppleButton cmdProbarMapsPto = null!;
    private AdminImportaPlus.GUI.AppleButton btnGuardarPto = null!;
    private AdminImportaPlus.GUI.AppleButton btnNuevoPto = null!;
    private AdminImportaPlus.GUI.AppleButton btnBorrarPto = null!;

    // Sección 3: Gestión de Usuarios (Adaptado de ING-SOFTWARE-BritezG/UI/GestionUsuario)
    private System.Windows.Forms.Panel pnlSecUsuarios = null!;
    private AdminImportaPlus.GUI.AppleCardPanel pnlUsuariosAcciones = null!;
    private System.Windows.Forms.Label lblUsuAccionesTit = null!;
    private AdminImportaPlus.GUI.AppleButton btnCrearUsuario = null!;
    private AdminImportaPlus.GUI.AppleButton btnModificarUsuario = null!;
    private AdminImportaPlus.GUI.AppleButton btnDesbloquearUsuario = null!;
    private AdminImportaPlus.GUI.AppleButton btnActDesactUsuario = null!;
    private AdminImportaPlus.GUI.AppleButton btnResetPassUsuario = null!;
    private AdminImportaPlus.GUI.AppleButton cmdActualizarUsuarios = null!;
    private System.Windows.Forms.Panel pnlUsuariosRightWrap = null!;
    private AdminImportaPlus.GUI.AppleCardPanel pnlUsuariosGridCard = null!;
    private System.Windows.Forms.Panel pnlUsuariosTopFilter = null!;
    private System.Windows.Forms.Label lblUsuGridTit = null!;
    private System.Windows.Forms.RadioButton rbMostrarTodos = null!;
    private System.Windows.Forms.RadioButton rbMostrarActivos = null!;
    private System.Windows.Forms.RadioButton rbMostrarInactivos = null!;
    private System.Windows.Forms.DataGridView dgvUsuarios = null!;
    private System.Windows.Forms.Panel pnlUsuariosGap = null!;
    private AdminImportaPlus.GUI.AppleCardPanel pnlUsuariosEditorCard = null!;
    private System.Windows.Forms.Label lblUsuModoTitulo = null!;
    private System.Windows.Forms.Label lblUsuDni = null!;
    private System.Windows.Forms.TextBox txtUsuDni = null!;
    private System.Windows.Forms.Label lblUsuNombre = null!;
    private System.Windows.Forms.TextBox txtUsuNombre = null!;
    private System.Windows.Forms.Label lblUsuApellido = null!;
    private System.Windows.Forms.TextBox txtUsuApellido = null!;
    private System.Windows.Forms.Label lblUsuUsername = null!;
    private System.Windows.Forms.TextBox txtUsuUsername = null!;
    private System.Windows.Forms.Label lblUsuRol = null!;
    private System.Windows.Forms.ComboBox cmbUsuRol = null!;
    private System.Windows.Forms.CheckBox chkUsuActivar = null!;
    private System.Windows.Forms.CheckBox chkUsuDesactivar = null!;
    private AdminImportaPlus.GUI.AppleButton cmdAceptarUsuario = null!;
    private AdminImportaPlus.GUI.AppleButton cmdCancelarUsuario = null!;

    // Sección 4: Gestión de Perfiles (Adaptado de ING-SOFTWARE-BritezG/UI/Perfiles · 1 Permiso = 1 Botón)
    private System.Windows.Forms.Panel pnlSecRoles = null!;
    private System.Windows.Forms.ToolStrip TS_Gestion = null!;
    private System.Windows.Forms.ToolStripDropDownButton toolStripBtnAgregar = null!;
    private System.Windows.Forms.ToolStripMenuItem tsMenuAddFamiliaAPerfil = null!;
    private System.Windows.Forms.ToolStripMenuItem tsMenuAddPermisoAPerfil = null!;
    private System.Windows.Forms.ToolStripMenuItem tsMenuAddPermisoAFamilia = null!;
    private System.Windows.Forms.ToolStripSeparator tsSepAgregar = null!;
    private System.Windows.Forms.ToolStripMenuItem tsMenuAddFamilia = null!;
    private System.Windows.Forms.ToolStripMenuItem tsMenuAddPerfil = null!;
    private System.Windows.Forms.ToolStripMenuItem tsMenuAddPermiso = null!;
    private System.Windows.Forms.ToolStripDropDownButton toolStripBtnEliminar = null!;
    private System.Windows.Forms.ToolStripMenuItem tsMenuDelFamiliaDePerfil = null!;
    private System.Windows.Forms.ToolStripMenuItem tsMenuDelPermisoDePerfil = null!;
    private System.Windows.Forms.ToolStripMenuItem tsMenuDelPermisoDeFamilia = null!;
    private System.Windows.Forms.ToolStripSeparator tsSepEliminar = null!;
    private System.Windows.Forms.ToolStripMenuItem tsMenuDelPerfil = null!;
    private System.Windows.Forms.ToolStripMenuItem tsMenuDelFamilia = null!;
    private System.Windows.Forms.ToolStripMenuItem tsMenuDelPermiso = null!;
    private System.Windows.Forms.ToolStripButton tsBtnRelacionFamilia = null!;
    private System.Windows.Forms.ToolStripLabel tsLblReglaBotones = null!;
    private System.Windows.Forms.TableLayoutPanel tlpPerfilesColumnas = null!;
    private AdminImportaPlus.GUI.AppleCardPanel pnlCardPermisos = null!;
    private System.Windows.Forms.Panel pnlHdrPermisos = null!;
    private System.Windows.Forms.Label lblHdrPermisos = null!;
    private System.Windows.Forms.DataGridView dgvPermisos = null!;
    private AdminImportaPlus.GUI.AppleCardPanel pnlCardFamilias = null!;
    private System.Windows.Forms.Panel pnlHdrFamilias = null!;
    private System.Windows.Forms.Label lblHdrFamilias = null!;
    private System.Windows.Forms.DataGridView dgvFamilias = null!;
    private AdminImportaPlus.GUI.AppleCardPanel pnlCardPerfiles = null!;
    private System.Windows.Forms.Panel pnlHdrPerfiles = null!;
    private System.Windows.Forms.Label lblHdrPerfiles = null!;
    private System.Windows.Forms.DataGridView dgvPerfiles = null!;
    private AdminImportaPlus.GUI.AppleCardPanel pnlCardArbol = null!;
    private System.Windows.Forms.Panel pnlHdrArbol = null!;
    private System.Windows.Forms.Label lblHdrArbol = null!;
    private System.Windows.Forms.TreeView Vista_Familia = null!;

    // Sección 5: Dígito Verificador
    private System.Windows.Forms.Panel pnlSecDigitoVerificador = null!;
    private AdminImportaPlus.GUI.AppleCardPanel pnlDvCard = null!;
    private System.Windows.Forms.Label lblDvTit = null!;
    private AdminImportaPlus.GUI.AppleButton btnVerificarDv = null!;
    private AdminImportaPlus.GUI.AppleButton btnRecalcularDv = null!;
    private System.Windows.Forms.DataGridView dgvDvv = null!;
    private AdminImportaPlus.GUI.AppleCardPanel pnlDvReporte = null!;
    private System.Windows.Forms.Label lblDetalleIntegridadPanel = null!;

    // Sección 6: RestoreBackup & Supabase
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
        this.btnNavUsuarios = new AdminImportaPlus.GUI.AppleButton();
        this.btnNavPuertos = new AdminImportaPlus.GUI.AppleButton();
        this.btnNavDespachantes = new AdminImportaPlus.GUI.AppleButton();
        this.btnNavForwarders = new AdminImportaPlus.GUI.AppleButton();
        this.pnlMain = new System.Windows.Forms.Panel();
        this.pnlContenedorVistas = new System.Windows.Forms.Panel();

        // Forwarders
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

        // Despachantes de Aduana
        this.pnlSecDespachantes = new System.Windows.Forms.Panel();
        this.splitDespachantes = new System.Windows.Forms.SplitContainer();
        this.pnlDespGridWrap = new AdminImportaPlus.GUI.AppleCardPanel();
        this.dgvDespachantes = new System.Windows.Forms.DataGridView();
        this.lblDespGridTitle = new System.Windows.Forms.Label();
        this.pnlDespForm = new AdminImportaPlus.GUI.AppleCardPanel();
        this.btnEliminarDesp = new AdminImportaPlus.GUI.AppleButton();
        this.btnNuevoDesp = new AdminImportaPlus.GUI.AppleButton();
        this.btnGuardarDesp = new AdminImportaPlus.GUI.AppleButton();
        this.chkDespActivo = new System.Windows.Forms.CheckBox();
        this.nudDespOrden = new System.Windows.Forms.NumericUpDown();
        this.lblDespOrden = new System.Windows.Forms.Label();
        this.txtDespEmail = new System.Windows.Forms.TextBox();
        this.lblDespMail = new System.Windows.Forms.Label();
        this.txtDespWhatsapp = new System.Windows.Forms.TextBox();
        this.lblDespWa = new System.Windows.Forms.Label();
        this.txtDespNombre = new System.Windows.Forms.TextBox();
        this.lblContadorNombreDesp = new System.Windows.Forms.Label();
        this.lblDespNombre = new System.Windows.Forms.Label();
        this.txtDespImagenUrl = new System.Windows.Forms.TextBox();
        this.lblDespUrl = new System.Windows.Forms.Label();
        this.btnSubirFotoDesp = new AdminImportaPlus.GUI.AppleButton();
        this.picDespachante = new System.Windows.Forms.PictureBox();
        this.lblDespEditorTitulo = new System.Windows.Forms.Label();

        // Puertos
        this.pnlSecPuertos = new System.Windows.Forms.Panel();
        this.splitPuertos = new System.Windows.Forms.SplitContainer();
        this.pnlPtoGridWrap = new AdminImportaPlus.GUI.AppleCardPanel();
        this.dgvPuertos = new System.Windows.Forms.DataGridView();
        this.pnlPtoForm = new AdminImportaPlus.GUI.AppleCardPanel();
        this.btnBorrarPto = new AdminImportaPlus.GUI.AppleButton();
        this.btnNuevoPto = new AdminImportaPlus.GUI.AppleButton();
        this.btnGuardarPto = new AdminImportaPlus.GUI.AppleButton();
        this.btnActualizarLinkMapsPto = new AdminImportaPlus.GUI.AppleButton();
        this.cmdProbarMapsPto = new AdminImportaPlus.GUI.AppleButton();
        this.txtPuertoMapsUrl = new System.Windows.Forms.TextBox();
        this.lblPtoMapsUrl = new System.Windows.Forms.Label();
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

        // Gestión de Usuarios
        this.pnlSecUsuarios = new System.Windows.Forms.Panel();
        this.pnlUsuariosAcciones = new AdminImportaPlus.GUI.AppleCardPanel();
        this.lblUsuAccionesTit = new System.Windows.Forms.Label();
        this.btnCrearUsuario = new AdminImportaPlus.GUI.AppleButton();
        this.btnModificarUsuario = new AdminImportaPlus.GUI.AppleButton();
        this.btnDesbloquearUsuario = new AdminImportaPlus.GUI.AppleButton();
        this.btnActDesactUsuario = new AdminImportaPlus.GUI.AppleButton();
        this.btnResetPassUsuario = new AdminImportaPlus.GUI.AppleButton();
        this.cmdActualizarUsuarios = new AdminImportaPlus.GUI.AppleButton();
        this.pnlUsuariosRightWrap = new System.Windows.Forms.Panel();
        this.pnlUsuariosGridCard = new AdminImportaPlus.GUI.AppleCardPanel();
        this.pnlUsuariosTopFilter = new System.Windows.Forms.Panel();
        this.lblUsuGridTit = new System.Windows.Forms.Label();
        this.rbMostrarTodos = new System.Windows.Forms.RadioButton();
        this.rbMostrarActivos = new System.Windows.Forms.RadioButton();
        this.rbMostrarInactivos = new System.Windows.Forms.RadioButton();
        this.dgvUsuarios = new System.Windows.Forms.DataGridView();
        this.pnlUsuariosGap = new System.Windows.Forms.Panel();
        this.pnlUsuariosEditorCard = new AdminImportaPlus.GUI.AppleCardPanel();
        this.lblUsuModoTitulo = new System.Windows.Forms.Label();
        this.lblUsuDni = new System.Windows.Forms.Label();
        this.txtUsuDni = new System.Windows.Forms.TextBox();
        this.lblUsuNombre = new System.Windows.Forms.Label();
        this.txtUsuNombre = new System.Windows.Forms.TextBox();
        this.lblUsuApellido = new System.Windows.Forms.Label();
        this.txtUsuApellido = new System.Windows.Forms.TextBox();
        this.lblUsuUsername = new System.Windows.Forms.Label();
        this.txtUsuUsername = new System.Windows.Forms.TextBox();
        this.lblUsuRol = new System.Windows.Forms.Label();
        this.cmbUsuRol = new System.Windows.Forms.ComboBox();
        this.chkUsuActivar = new System.Windows.Forms.CheckBox();
        this.chkUsuDesactivar = new System.Windows.Forms.CheckBox();
        this.cmdAceptarUsuario = new AdminImportaPlus.GUI.AppleButton();
        this.cmdCancelarUsuario = new AdminImportaPlus.GUI.AppleButton();

        // Gestión de Perfiles (4 columnas + ToolStrip)
        this.pnlSecRoles = new System.Windows.Forms.Panel();
        this.TS_Gestion = new System.Windows.Forms.ToolStrip();
        this.toolStripBtnAgregar = new System.Windows.Forms.ToolStripDropDownButton();
        this.tsMenuAddFamiliaAPerfil = new System.Windows.Forms.ToolStripMenuItem();
        this.tsMenuAddPermisoAPerfil = new System.Windows.Forms.ToolStripMenuItem();
        this.tsMenuAddPermisoAFamilia = new System.Windows.Forms.ToolStripMenuItem();
        this.tsSepAgregar = new System.Windows.Forms.ToolStripSeparator();
        this.tsMenuAddFamilia = new System.Windows.Forms.ToolStripMenuItem();
        this.tsMenuAddPerfil = new System.Windows.Forms.ToolStripMenuItem();
        this.tsMenuAddPermiso = new System.Windows.Forms.ToolStripMenuItem();
        this.toolStripBtnEliminar = new System.Windows.Forms.ToolStripDropDownButton();
        this.tsMenuDelFamiliaDePerfil = new System.Windows.Forms.ToolStripMenuItem();
        this.tsMenuDelPermisoDePerfil = new System.Windows.Forms.ToolStripMenuItem();
        this.tsMenuDelPermisoDeFamilia = new System.Windows.Forms.ToolStripMenuItem();
        this.tsSepEliminar = new System.Windows.Forms.ToolStripSeparator();
        this.tsMenuDelPerfil = new System.Windows.Forms.ToolStripMenuItem();
        this.tsMenuDelFamilia = new System.Windows.Forms.ToolStripMenuItem();
        this.tsMenuDelPermiso = new System.Windows.Forms.ToolStripMenuItem();
        this.tsBtnRelacionFamilia = new System.Windows.Forms.ToolStripButton();
        this.tsLblReglaBotones = new System.Windows.Forms.ToolStripLabel();
        this.tlpPerfilesColumnas = new System.Windows.Forms.TableLayoutPanel();
        this.pnlCardPermisos = new AdminImportaPlus.GUI.AppleCardPanel();
        this.pnlHdrPermisos = new System.Windows.Forms.Panel();
        this.lblHdrPermisos = new System.Windows.Forms.Label();
        this.dgvPermisos = new System.Windows.Forms.DataGridView();
        this.pnlCardFamilias = new AdminImportaPlus.GUI.AppleCardPanel();
        this.pnlHdrFamilias = new System.Windows.Forms.Panel();
        this.lblHdrFamilias = new System.Windows.Forms.Label();
        this.dgvFamilias = new System.Windows.Forms.DataGridView();
        this.pnlCardPerfiles = new AdminImportaPlus.GUI.AppleCardPanel();
        this.pnlHdrPerfiles = new System.Windows.Forms.Panel();
        this.lblHdrPerfiles = new System.Windows.Forms.Label();
        this.dgvPerfiles = new System.Windows.Forms.DataGridView();
        this.pnlCardArbol = new AdminImportaPlus.GUI.AppleCardPanel();
        this.pnlHdrArbol = new System.Windows.Forms.Panel();
        this.lblHdrArbol = new System.Windows.Forms.Label();
        this.Vista_Familia = new System.Windows.Forms.TreeView();

        // Dígito Verificador
        this.pnlSecDigitoVerificador = new System.Windows.Forms.Panel();
        this.pnlDvCard = new AdminImportaPlus.GUI.AppleCardPanel();
        this.pnlDvReporte = new AdminImportaPlus.GUI.AppleCardPanel();
        this.lblDetalleIntegridadPanel = new System.Windows.Forms.Label();
        this.dgvDvv = new System.Windows.Forms.DataGridView();
        this.btnRecalcularDv = new AdminImportaPlus.GUI.AppleButton();
        this.btnVerificarDv = new AdminImportaPlus.GUI.AppleButton();
        this.lblDvTit = new System.Windows.Forms.Label();

        // RestoreBackup & Supabase
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
        this.pnlSecDespachantes.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)(this.splitDespachantes)).BeginInit();
        this.splitDespachantes.Panel1.SuspendLayout();
        this.splitDespachantes.Panel2.SuspendLayout();
        this.splitDespachantes.SuspendLayout();
        this.pnlDespGridWrap.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)(this.dgvDespachantes)).BeginInit();
        this.pnlDespForm.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)(this.nudDespOrden)).BeginInit();
        ((System.ComponentModel.ISupportInitialize)(this.picDespachante)).BeginInit();
        this.pnlSecPuertos.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)(this.splitPuertos)).BeginInit();
        this.splitPuertos.Panel1.SuspendLayout();
        this.splitPuertos.Panel2.SuspendLayout();
        this.splitPuertos.SuspendLayout();
        this.pnlPtoGridWrap.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)(this.dgvPuertos)).BeginInit();
        this.pnlPtoForm.SuspendLayout();
        this.pnlSecUsuarios.SuspendLayout();
        this.pnlUsuariosAcciones.SuspendLayout();
        this.pnlUsuariosRightWrap.SuspendLayout();
        this.pnlUsuariosGridCard.SuspendLayout();
        this.pnlUsuariosTopFilter.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)(this.dgvUsuarios)).BeginInit();
        this.pnlUsuariosEditorCard.SuspendLayout();
        this.pnlSecRoles.SuspendLayout();
        this.TS_Gestion.SuspendLayout();
        this.tlpPerfilesColumnas.SuspendLayout();
        this.pnlCardPermisos.SuspendLayout();
        this.pnlHdrPermisos.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)(this.dgvPermisos)).BeginInit();
        this.pnlCardFamilias.SuspendLayout();
        this.pnlHdrFamilias.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)(this.dgvFamilias)).BeginInit();
        this.pnlCardPerfiles.SuspendLayout();
        this.pnlHdrPerfiles.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)(this.dgvPerfiles)).BeginInit();
        this.pnlCardArbol.SuspendLayout();
        this.pnlHdrArbol.SuspendLayout();
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
        // SIDEBAR IZQUIERDO ESTILO APPLE / MACOS (7 BOTONES APILADOS + CLAVE)
        // ====================================================================
        this.pnlSidebar.BackColor = System.Drawing.Color.FromArgb(46, 5, 16);
        this.pnlSidebar.Controls.Add(this.btnCambiarPassword);
        this.pnlSidebar.Controls.Add(this.btnNavRestoreSupabase);
        this.pnlSidebar.Controls.Add(this.btnNavDigitoVerificador);
        this.pnlSidebar.Controls.Add(this.btnNavRoles);
        this.pnlSidebar.Controls.Add(this.btnNavUsuarios);
        this.pnlSidebar.Controls.Add(this.btnNavPuertos);
        this.pnlSidebar.Controls.Add(this.btnNavDespachantes);
        this.pnlSidebar.Controls.Add(this.btnNavForwarders);
        this.pnlSidebar.Dock = System.Windows.Forms.DockStyle.Left;
        this.pnlSidebar.Name = "pnlSidebar";
        this.pnlSidebar.Size = new System.Drawing.Size(236, 720);

        this.btnNavForwarders.BackColor = System.Drawing.Color.FromArgb(115, 16, 40);
        this.btnNavForwarders.CornerRadius = 12;
        this.btnNavForwarders.ForeColor = System.Drawing.Color.White;
        this.btnNavForwarders.HoverBackColor = System.Drawing.Color.FromArgb(135, 24, 52);
        this.btnNavForwarders.Location = new System.Drawing.Point(16, 20);
        this.btnNavForwarders.Name = "btnNavForwarders";
        this.btnNavForwarders.Padding = new System.Windows.Forms.Padding(12, 0, 0, 0);
        this.btnNavForwarders.Size = new System.Drawing.Size(204, 42);
        this.btnNavForwarders.Text = "Forwarders";
        this.btnNavForwarders.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;

        this.btnNavDespachantes.BackColor = System.Drawing.Color.FromArgb(46, 5, 16);
        this.btnNavDespachantes.CornerRadius = 12;
        this.btnNavDespachantes.ForeColor = System.Drawing.Color.FromArgb(245, 215, 222);
        this.btnNavDespachantes.HoverBackColor = System.Drawing.Color.FromArgb(74, 12, 29);
        this.btnNavDespachantes.Location = new System.Drawing.Point(16, 70);
        this.btnNavDespachantes.Name = "btnNavDespachantes";
        this.btnNavDespachantes.Padding = new System.Windows.Forms.Padding(12, 0, 0, 0);
        this.btnNavDespachantes.Size = new System.Drawing.Size(204, 42);
        this.btnNavDespachantes.Text = "Despachantes de Aduana";
        this.btnNavDespachantes.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;

        this.btnNavPuertos.BackColor = System.Drawing.Color.FromArgb(46, 5, 16);
        this.btnNavPuertos.CornerRadius = 12;
        this.btnNavPuertos.ForeColor = System.Drawing.Color.FromArgb(245, 215, 222);
        this.btnNavPuertos.HoverBackColor = System.Drawing.Color.FromArgb(74, 12, 29);
        this.btnNavPuertos.Location = new System.Drawing.Point(16, 120);
        this.btnNavPuertos.Name = "btnNavPuertos";
        this.btnNavPuertos.Padding = new System.Windows.Forms.Padding(12, 0, 0, 0);
        this.btnNavPuertos.Size = new System.Drawing.Size(204, 42);
        this.btnNavPuertos.Text = "Puertos y Aduanas";
        this.btnNavPuertos.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;

        this.btnNavUsuarios.BackColor = System.Drawing.Color.FromArgb(46, 5, 16);
        this.btnNavUsuarios.CornerRadius = 12;
        this.btnNavUsuarios.ForeColor = System.Drawing.Color.FromArgb(245, 215, 222);
        this.btnNavUsuarios.HoverBackColor = System.Drawing.Color.FromArgb(74, 12, 29);
        this.btnNavUsuarios.Location = new System.Drawing.Point(16, 170);
        this.btnNavUsuarios.Name = "btnNavUsuarios";
        this.btnNavUsuarios.Padding = new System.Windows.Forms.Padding(12, 0, 0, 0);
        this.btnNavUsuarios.Size = new System.Drawing.Size(204, 42);
        this.btnNavUsuarios.Text = "Gestión de Usuarios";
        this.btnNavUsuarios.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;

        this.btnNavRoles.BackColor = System.Drawing.Color.FromArgb(46, 5, 16);
        this.btnNavRoles.CornerRadius = 12;
        this.btnNavRoles.ForeColor = System.Drawing.Color.FromArgb(245, 215, 222);
        this.btnNavRoles.HoverBackColor = System.Drawing.Color.FromArgb(74, 12, 29);
        this.btnNavRoles.Location = new System.Drawing.Point(16, 220);
        this.btnNavRoles.Name = "btnNavRoles";
        this.btnNavRoles.Padding = new System.Windows.Forms.Padding(12, 0, 0, 0);
        this.btnNavRoles.Size = new System.Drawing.Size(204, 42);
        this.btnNavRoles.Text = "Gestión de Perfiles";
        this.btnNavRoles.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;

        this.btnNavDigitoVerificador.BackColor = System.Drawing.Color.FromArgb(46, 5, 16);
        this.btnNavDigitoVerificador.CornerRadius = 12;
        this.btnNavDigitoVerificador.ForeColor = System.Drawing.Color.FromArgb(245, 215, 222);
        this.btnNavDigitoVerificador.HoverBackColor = System.Drawing.Color.FromArgb(74, 12, 29);
        this.btnNavDigitoVerificador.Location = new System.Drawing.Point(16, 270);
        this.btnNavDigitoVerificador.Name = "btnNavDigitoVerificador";
        this.btnNavDigitoVerificador.Padding = new System.Windows.Forms.Padding(12, 0, 0, 0);
        this.btnNavDigitoVerificador.Size = new System.Drawing.Size(204, 42);
        this.btnNavDigitoVerificador.Text = "Dígito Verificador";
        this.btnNavDigitoVerificador.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;

        this.btnNavRestoreSupabase.BackColor = System.Drawing.Color.FromArgb(46, 5, 16);
        this.btnNavRestoreSupabase.CornerRadius = 12;
        this.btnNavRestoreSupabase.ForeColor = System.Drawing.Color.FromArgb(245, 215, 222);
        this.btnNavRestoreSupabase.HoverBackColor = System.Drawing.Color.FromArgb(74, 12, 29);
        this.btnNavRestoreSupabase.Location = new System.Drawing.Point(16, 320);
        this.btnNavRestoreSupabase.Name = "btnNavRestoreSupabase";
        this.btnNavRestoreSupabase.Padding = new System.Windows.Forms.Padding(12, 0, 0, 0);
        this.btnNavRestoreSupabase.Size = new System.Drawing.Size(204, 42);
        this.btnNavRestoreSupabase.Text = "RestoreBackup y Nube";
        this.btnNavRestoreSupabase.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;

        this.btnCambiarPassword.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
        this.btnCambiarPassword.BackColor = System.Drawing.Color.FromArgb(83, 6, 24);
        this.btnCambiarPassword.BorderColor = System.Drawing.Color.FromArgb(155, 48, 64);
        this.btnCambiarPassword.BorderThickness = 1;
        this.btnCambiarPassword.CornerRadius = 12;
        this.btnCambiarPassword.ForeColor = System.Drawing.Color.White;
        this.btnCambiarPassword.HoverBackColor = System.Drawing.Color.FromArgb(115, 16, 40);
        this.btnCambiarPassword.Location = new System.Drawing.Point(16, 656);
        this.btnCambiarPassword.Name = "btnCambiarPassword";
        this.btnCambiarPassword.Size = new System.Drawing.Size(204, 42);
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
        this.pnlTopBar.Size = new System.Drawing.Size(1024, 58);

        this.lblUsuarioActual.AutoSize = true;
        this.lblUsuarioActual.Font = new System.Drawing.Font("Segoe UI Semibold", 10F);
        this.lblUsuarioActual.ForeColor = System.Drawing.Color.FromArgb(29, 29, 31);
        this.lblUsuarioActual.Location = new System.Drawing.Point(22, 19);
        this.lblUsuarioActual.Text = "Usuario: Admin";

        this.lblBadgeEstadoHeader.AutoSize = true;
        this.lblBadgeEstadoHeader.Font = new System.Drawing.Font("Segoe UI Semibold", 9F);
        this.lblBadgeEstadoHeader.ForeColor = System.Drawing.Color.FromArgb(5, 150, 105);
        this.lblBadgeEstadoHeader.Location = new System.Drawing.Point(310, 21);
        this.lblBadgeEstadoHeader.Text = "Verificando integridad...";

        this.btnCerrarSesion.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
        this.btnCerrarSesion.BackColor = System.Drawing.Color.FromArgb(245, 245, 247);
        this.btnCerrarSesion.BorderColor = System.Drawing.Color.FromArgb(229, 231, 235);
        this.btnCerrarSesion.BorderThickness = 1;
        this.btnCerrarSesion.CornerRadius = 10;
        this.btnCerrarSesion.ForeColor = System.Drawing.Color.FromArgb(83, 6, 24);
        this.btnCerrarSesion.HoverBackColor = System.Drawing.Color.FromArgb(255, 240, 243);
        this.btnCerrarSesion.Location = new System.Drawing.Point(878, 12);
        this.btnCerrarSesion.Name = "btnCerrarSesion";
        this.btnCerrarSesion.Size = new System.Drawing.Size(126, 34);
        this.btnCerrarSesion.Text = "Cerrar Sesión";

        this.pnlContenedorVistas.BackColor = System.Drawing.Color.FromArgb(245, 245, 247);
        this.pnlContenedorVistas.Controls.Add(this.pnlSecRestoreSupabase);
        this.pnlContenedorVistas.Controls.Add(this.pnlSecDigitoVerificador);
        this.pnlContenedorVistas.Controls.Add(this.pnlSecRoles);
        this.pnlContenedorVistas.Controls.Add(this.pnlSecUsuarios);
        this.pnlContenedorVistas.Controls.Add(this.pnlSecPuertos);
        this.pnlContenedorVistas.Controls.Add(this.pnlSecDespachantes);
        this.pnlContenedorVistas.Controls.Add(this.pnlSecForwarders);
        this.pnlContenedorVistas.Dock = System.Windows.Forms.DockStyle.Fill;
        this.pnlContenedorVistas.Padding = new System.Windows.Forms.Padding(16);

        // ====================================================================
        // SECCIÓN 0: FORWARDERS
        // ====================================================================
        this.pnlSecForwarders.BackColor = System.Drawing.Color.FromArgb(245, 245, 247);
        this.pnlSecForwarders.Controls.Add(this.splitForwarders);
        this.pnlSecForwarders.Dock = System.Windows.Forms.DockStyle.Fill;

        this.splitForwarders.BackColor = System.Drawing.Color.FromArgb(245, 245, 247);
        this.splitForwarders.Dock = System.Windows.Forms.DockStyle.Fill;
        this.splitForwarders.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
        this.splitForwarders.Panel1.Controls.Add(this.pnlFwdGridWrap);
        this.splitForwarders.Panel2.Controls.Add(this.pnlFwdForm);
        this.splitForwarders.Size = new System.Drawing.Size(992, 630);
        this.splitForwarders.SplitterDistance = 555;
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
        this.btnSubirFoto.Name = "btnSubirFoto";
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
        this.btnGuardarFwd.Name = "btnGuardarFwd";
        this.btnGuardarFwd.Size = new System.Drawing.Size(380, 42);
        this.btnGuardarFwd.Text = "Guardar Forwarder";

        this.btnNuevoFwd.BackColor = System.Drawing.Color.FromArgb(245, 245, 247);
        this.btnNuevoFwd.BorderColor = System.Drawing.Color.FromArgb(229, 231, 235);
        this.btnNuevoFwd.BorderThickness = 1;
        this.btnNuevoFwd.CornerRadius = 10;
        this.btnNuevoFwd.ForeColor = System.Drawing.Color.FromArgb(29, 29, 31);
        this.btnNuevoFwd.Location = new System.Drawing.Point(20, 482);
        this.btnNuevoFwd.Name = "btnNuevoFwd";
        this.btnNuevoFwd.Size = new System.Drawing.Size(184, 38);
        this.btnNuevoFwd.Text = "Nuevo / Limpiar";

        this.btnEliminarFwd.BackColor = System.Drawing.Color.FromArgb(255, 241, 242);
        this.btnEliminarFwd.BorderColor = System.Drawing.Color.FromArgb(254, 205, 211);
        this.btnEliminarFwd.BorderThickness = 1;
        this.btnEliminarFwd.CornerRadius = 10;
        this.btnEliminarFwd.ForeColor = System.Drawing.Color.FromArgb(159, 18, 57);
        this.btnEliminarFwd.Location = new System.Drawing.Point(216, 482);
        this.btnEliminarFwd.Name = "btnEliminarFwd";
        this.btnEliminarFwd.Size = new System.Drawing.Size(184, 38);
        this.btnEliminarFwd.Text = "Eliminar";

        // ====================================================================
        // SECCIÓN 1: DESPACHANTES DE ADUANA (ABM IDÉNTICO A FORWARDERS)
        // ====================================================================
        this.pnlSecDespachantes.BackColor = System.Drawing.Color.FromArgb(245, 245, 247);
        this.pnlSecDespachantes.Controls.Add(this.splitDespachantes);
        this.pnlSecDespachantes.Dock = System.Windows.Forms.DockStyle.Fill;

        this.splitDespachantes.BackColor = System.Drawing.Color.FromArgb(245, 245, 247);
        this.splitDespachantes.Dock = System.Windows.Forms.DockStyle.Fill;
        this.splitDespachantes.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
        this.splitDespachantes.Panel1.Controls.Add(this.pnlDespGridWrap);
        this.splitDespachantes.Panel2.Controls.Add(this.pnlDespForm);
        this.splitDespachantes.Size = new System.Drawing.Size(992, 630);
        this.splitDespachantes.SplitterDistance = 555;
        this.splitDespachantes.SplitterWidth = 12;

        this.pnlDespGridWrap.Controls.Add(this.dgvDespachantes);
        this.pnlDespGridWrap.Controls.Add(this.lblDespGridTitle);
        this.pnlDespGridWrap.Dock = System.Windows.Forms.DockStyle.Fill;
        this.pnlDespGridWrap.Padding = new System.Windows.Forms.Padding(16);

        this.lblDespGridTitle.Dock = System.Windows.Forms.DockStyle.Top;
        this.lblDespGridTitle.Font = new System.Drawing.Font("Segoe UI Semibold", 10F);
        this.lblDespGridTitle.ForeColor = System.Drawing.Color.FromArgb(29, 29, 31);
        this.lblDespGridTitle.Height = 30;
        this.lblDespGridTitle.Text = "Despachantes de Aduana Activos (Grilla Web de 4 por fila · Sin límite)";

        this.dgvDespachantes.Dock = System.Windows.Forms.DockStyle.Fill;

        this.pnlDespForm.AutoScroll = true;
        this.pnlDespForm.Controls.Add(this.btnEliminarDesp);
        this.pnlDespForm.Controls.Add(this.btnNuevoDesp);
        this.pnlDespForm.Controls.Add(this.btnGuardarDesp);
        this.pnlDespForm.Controls.Add(this.chkDespActivo);
        this.pnlDespForm.Controls.Add(this.nudDespOrden);
        this.pnlDespForm.Controls.Add(this.lblDespOrden);
        this.pnlDespForm.Controls.Add(this.txtDespEmail);
        this.pnlDespForm.Controls.Add(this.lblDespMail);
        this.pnlDespForm.Controls.Add(this.txtDespWhatsapp);
        this.pnlDespForm.Controls.Add(this.lblDespWa);
        this.pnlDespForm.Controls.Add(this.txtDespNombre);
        this.pnlDespForm.Controls.Add(this.lblContadorNombreDesp);
        this.pnlDespForm.Controls.Add(this.lblDespNombre);
        this.pnlDespForm.Controls.Add(this.txtDespImagenUrl);
        this.pnlDespForm.Controls.Add(this.lblDespUrl);
        this.pnlDespForm.Controls.Add(this.btnSubirFotoDesp);
        this.pnlDespForm.Controls.Add(this.picDespachante);
        this.pnlDespForm.Controls.Add(this.lblDespEditorTitulo);
        this.pnlDespForm.Dock = System.Windows.Forms.DockStyle.Fill;

        this.lblDespEditorTitulo.AutoSize = true;
        this.lblDespEditorTitulo.Font = new System.Drawing.Font("Segoe UI Semibold", 12F);
        this.lblDespEditorTitulo.ForeColor = System.Drawing.Color.FromArgb(29, 29, 31);
        this.lblDespEditorTitulo.Location = new System.Drawing.Point(20, 16);
        this.lblDespEditorTitulo.Text = "ABM de Despachante de Aduana";

        this.picDespachante.BackColor = System.Drawing.Color.FromArgb(245, 245, 247);
        this.picDespachante.Location = new System.Drawing.Point(20, 50);
        this.picDespachante.Size = new System.Drawing.Size(104, 104);
        this.picDespachante.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;

        this.btnSubirFotoDesp.BackColor = System.Drawing.Color.FromArgb(245, 245, 247);
        this.btnSubirFotoDesp.BorderColor = System.Drawing.Color.FromArgb(229, 231, 235);
        this.btnSubirFotoDesp.BorderThickness = 1;
        this.btnSubirFotoDesp.CornerRadius = 10;
        this.btnSubirFotoDesp.ForeColor = System.Drawing.Color.FromArgb(83, 6, 24);
        this.btnSubirFotoDesp.HoverBackColor = System.Drawing.Color.FromArgb(255, 240, 243);
        this.btnSubirFotoDesp.Location = new System.Drawing.Point(138, 50);
        this.btnSubirFotoDesp.Name = "btnSubirFotoDesp";
        this.btnSubirFotoDesp.Size = new System.Drawing.Size(262, 36);
        this.btnSubirFotoDesp.Text = "Seleccionar Imagen...";

        this.lblDespUrl.AutoSize = true;
        this.lblDespUrl.Font = new System.Drawing.Font("Segoe UI Semibold", 8F);
        this.lblDespUrl.ForeColor = System.Drawing.Color.FromArgb(110, 110, 115);
        this.lblDespUrl.Location = new System.Drawing.Point(138, 96);
        this.lblDespUrl.Text = "URL DE IMAGEN";

        this.txtDespImagenUrl.Location = new System.Drawing.Point(138, 115);
        this.txtDespImagenUrl.Size = new System.Drawing.Size(262, 23);

        this.lblDespNombre.AutoSize = true;
        this.lblDespNombre.Font = new System.Drawing.Font("Segoe UI Semibold", 8F);
        this.lblDespNombre.ForeColor = System.Drawing.Color.FromArgb(110, 110, 115);
        this.lblDespNombre.Location = new System.Drawing.Point(20, 170);
        this.lblDespNombre.Text = "NOMBRE DEL DESPACHANTE (MÁX. 120 CARACTERES)";

        this.lblContadorNombreDesp.AutoSize = true;
        this.lblContadorNombreDesp.Font = new System.Drawing.Font("Segoe UI Semibold", 8F);
        this.lblContadorNombreDesp.ForeColor = System.Drawing.Color.FromArgb(83, 6, 24);
        this.lblContadorNombreDesp.Location = new System.Drawing.Point(354, 170);
        this.lblContadorNombreDesp.Text = "0 / 120";

        this.txtDespNombre.Location = new System.Drawing.Point(20, 190);
        this.txtDespNombre.MaxLength = 120;
        this.txtDespNombre.Multiline = true;
        this.txtDespNombre.Size = new System.Drawing.Size(380, 48);

        this.lblDespWa.AutoSize = true;
        this.lblDespWa.Font = new System.Drawing.Font("Segoe UI Semibold", 8F);
        this.lblDespWa.ForeColor = System.Drawing.Color.FromArgb(110, 110, 115);
        this.lblDespWa.Location = new System.Drawing.Point(20, 250);
        this.lblDespWa.Text = "WHATSAPP (REDIRECCIÓN DIRECTA)";

        this.txtDespWhatsapp.Location = new System.Drawing.Point(20, 270);
        this.txtDespWhatsapp.PlaceholderText = "+5491143218800";
        this.txtDespWhatsapp.Size = new System.Drawing.Size(380, 23);

        this.lblDespMail.AutoSize = true;
        this.lblDespMail.Font = new System.Drawing.Font("Segoe UI Semibold", 8F);
        this.lblDespMail.ForeColor = System.Drawing.Color.FromArgb(110, 110, 115);
        this.lblDespMail.Location = new System.Drawing.Point(20, 306);
        this.lblDespMail.Text = "MAIL OFICIAL (COPIADO EN WEB)";

        this.txtDespEmail.Location = new System.Drawing.Point(20, 326);
        this.txtDespEmail.PlaceholderText = "despachos@importaplus.com.ar";
        this.txtDespEmail.Size = new System.Drawing.Size(380, 23);

        this.lblDespOrden.AutoSize = true;
        this.lblDespOrden.Font = new System.Drawing.Font("Segoe UI Semibold", 8F);
        this.lblDespOrden.ForeColor = System.Drawing.Color.FromArgb(110, 110, 115);
        this.lblDespOrden.Location = new System.Drawing.Point(20, 364);
        this.lblDespOrden.Text = "ORDEN ÚNICO EN GRILLA:";

        this.nudDespOrden.Location = new System.Drawing.Point(20, 384);
        this.nudDespOrden.Maximum = new decimal(new int[] { 999, 0, 0, 0 });
        this.nudDespOrden.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
        this.nudDespOrden.Size = new System.Drawing.Size(110, 23);
        this.nudDespOrden.Value = new decimal(new int[] { 1, 0, 0, 0 });

        this.chkDespActivo.AutoSize = true;
        this.chkDespActivo.Checked = true;
        this.chkDespActivo.Font = new System.Drawing.Font("Segoe UI Semibold", 9F);
        this.chkDespActivo.Location = new System.Drawing.Point(150, 386);
        this.chkDespActivo.Text = "Mostrar activo en la Web";

        this.btnGuardarDesp.BackColor = System.Drawing.Color.FromArgb(83, 6, 24);
        this.btnGuardarDesp.CornerRadius = 12;
        this.btnGuardarDesp.ForeColor = System.Drawing.Color.White;
        this.btnGuardarDesp.HoverBackColor = System.Drawing.Color.FromArgb(115, 16, 40);
        this.btnGuardarDesp.Location = new System.Drawing.Point(20, 428);
        this.btnGuardarDesp.Name = "btnGuardarDesp";
        this.btnGuardarDesp.Size = new System.Drawing.Size(380, 42);
        this.btnGuardarDesp.Text = "Guardar Despachante";

        this.btnNuevoDesp.BackColor = System.Drawing.Color.FromArgb(245, 245, 247);
        this.btnNuevoDesp.BorderColor = System.Drawing.Color.FromArgb(229, 231, 235);
        this.btnNuevoDesp.BorderThickness = 1;
        this.btnNuevoDesp.CornerRadius = 10;
        this.btnNuevoDesp.ForeColor = System.Drawing.Color.FromArgb(29, 29, 31);
        this.btnNuevoDesp.Location = new System.Drawing.Point(20, 482);
        this.btnNuevoDesp.Name = "btnNuevoDesp";
        this.btnNuevoDesp.Size = new System.Drawing.Size(184, 38);
        this.btnNuevoDesp.Text = "Nuevo / Limpiar";

        this.btnEliminarDesp.BackColor = System.Drawing.Color.FromArgb(255, 241, 242);
        this.btnEliminarDesp.BorderColor = System.Drawing.Color.FromArgb(254, 205, 211);
        this.btnEliminarDesp.BorderThickness = 1;
        this.btnEliminarDesp.CornerRadius = 10;
        this.btnEliminarDesp.ForeColor = System.Drawing.Color.FromArgb(159, 18, 57);
        this.btnEliminarDesp.Location = new System.Drawing.Point(216, 482);
        this.btnEliminarDesp.Name = "btnEliminarDesp";
        this.btnEliminarDesp.Size = new System.Drawing.Size(184, 38);
        this.btnEliminarDesp.Text = "Eliminar";

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
        this.splitPuertos.Size = new System.Drawing.Size(992, 630);
        this.splitPuertos.SplitterDistance = 565;
        this.splitPuertos.SplitterWidth = 12;

        this.pnlPtoGridWrap.Controls.Add(this.dgvPuertos);
        this.pnlPtoGridWrap.Dock = System.Windows.Forms.DockStyle.Fill;
        this.pnlPtoGridWrap.Padding = new System.Windows.Forms.Padding(16);
        this.dgvPuertos.Dock = System.Windows.Forms.DockStyle.Fill;

        this.pnlPtoForm.AutoScroll = true;
        this.pnlPtoForm.Controls.Add(this.btnBorrarPto);
        this.pnlPtoForm.Controls.Add(this.btnNuevoPto);
        this.pnlPtoForm.Controls.Add(this.btnGuardarPto);
        this.pnlPtoForm.Controls.Add(this.cmdProbarMapsPto);
        this.pnlPtoForm.Controls.Add(this.btnActualizarLinkMapsPto);
        this.pnlPtoForm.Controls.Add(this.txtPuertoMapsUrl);
        this.pnlPtoForm.Controls.Add(this.lblPtoMapsUrl);
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

        this.lblPtoMapsUrl.AutoSize = true; this.lblPtoMapsUrl.Font = new System.Drawing.Font("Segoe UI Semibold", 8F);
        this.lblPtoMapsUrl.ForeColor = System.Drawing.Color.FromArgb(83, 6, 24);
        this.lblPtoMapsUrl.Location = new System.Drawing.Point(20, 402); this.lblPtoMapsUrl.Text = "LINK DE GOOGLE MAPS (REDIRECCIÓN EN PESTAÑA NUEVA) *";
        this.txtPuertoMapsUrl.Location = new System.Drawing.Point(20, 422);
        this.txtPuertoMapsUrl.PlaceholderText = "https://www.google.com/maps/place/...";
        this.txtPuertoMapsUrl.Size = new System.Drawing.Size(370, 23);

        this.btnActualizarLinkMapsPto.BackColor = System.Drawing.Color.FromArgb(5, 150, 105);
        this.btnActualizarLinkMapsPto.CornerRadius = 10;
        this.btnActualizarLinkMapsPto.ForeColor = System.Drawing.Color.White;
        this.btnActualizarLinkMapsPto.HoverBackColor = System.Drawing.Color.FromArgb(4, 120, 87);
        this.btnActualizarLinkMapsPto.Location = new System.Drawing.Point(20, 454);
        this.btnActualizarLinkMapsPto.Name = "btnActualizarLinkMapsPto";
        this.btnActualizarLinkMapsPto.Size = new System.Drawing.Size(252, 36);
        this.btnActualizarLinkMapsPto.Text = "Actualizar Link Google Maps";

        this.cmdProbarMapsPto.BackColor = System.Drawing.Color.FromArgb(245, 245, 247);
        this.cmdProbarMapsPto.BorderColor = System.Drawing.Color.FromArgb(229, 231, 235);
        this.cmdProbarMapsPto.BorderThickness = 1;
        this.cmdProbarMapsPto.CornerRadius = 10;
        this.cmdProbarMapsPto.ForeColor = System.Drawing.Color.FromArgb(83, 6, 24);
        this.cmdProbarMapsPto.HoverBackColor = System.Drawing.Color.FromArgb(255, 240, 243);
        this.cmdProbarMapsPto.Location = new System.Drawing.Point(280, 454);
        this.cmdProbarMapsPto.Name = "cmdProbarMapsPto";
        this.cmdProbarMapsPto.Size = new System.Drawing.Size(110, 36);
        this.cmdProbarMapsPto.Text = "↗ Probar Link";

        this.btnGuardarPto.BackColor = System.Drawing.Color.FromArgb(83, 6, 24);
        this.btnGuardarPto.CornerRadius = 12;
        this.btnGuardarPto.ForeColor = System.Drawing.Color.White;
        this.btnGuardarPto.Location = new System.Drawing.Point(20, 500);
        this.btnGuardarPto.Name = "btnGuardarPto";
        this.btnGuardarPto.Size = new System.Drawing.Size(370, 40);
        this.btnGuardarPto.Text = "Guardar Puerto / Aduana";

        this.btnNuevoPto.BackColor = System.Drawing.Color.FromArgb(245, 245, 247);
        this.btnNuevoPto.BorderColor = System.Drawing.Color.FromArgb(229, 231, 235);
        this.btnNuevoPto.BorderThickness = 1;
        this.btnNuevoPto.CornerRadius = 10;
        this.btnNuevoPto.ForeColor = System.Drawing.Color.FromArgb(29, 29, 31);
        this.btnNuevoPto.Location = new System.Drawing.Point(20, 550);
        this.btnNuevoPto.Name = "btnNuevoPto";
        this.btnNuevoPto.Size = new System.Drawing.Size(178, 36);
        this.btnNuevoPto.Text = "Nuevo Puerto";

        this.btnBorrarPto.BackColor = System.Drawing.Color.FromArgb(255, 241, 242);
        this.btnBorrarPto.BorderColor = System.Drawing.Color.FromArgb(254, 205, 211);
        this.btnBorrarPto.BorderThickness = 1;
        this.btnBorrarPto.CornerRadius = 10;
        this.btnBorrarPto.ForeColor = System.Drawing.Color.FromArgb(159, 18, 57);
        this.btnBorrarPto.Location = new System.Drawing.Point(212, 550);
        this.btnBorrarPto.Name = "btnBorrarPto";
        this.btnBorrarPto.Size = new System.Drawing.Size(178, 36);
        this.btnBorrarPto.Text = "Eliminar";

        // ====================================================================
        // SECCIÓN 3: GESTIÓN DE USUARIOS (ING-SOFTWARE-BritezG)
        // ====================================================================
        this.pnlSecUsuarios.BackColor = System.Drawing.Color.FromArgb(245, 245, 247);
        this.pnlSecUsuarios.Controls.Add(this.pnlUsuariosRightWrap);
        this.pnlSecUsuarios.Controls.Add(this.pnlUsuariosAcciones);
        this.pnlSecUsuarios.Dock = System.Windows.Forms.DockStyle.Fill;

        this.pnlUsuariosAcciones.Controls.Add(this.cmdActualizarUsuarios);
        this.pnlUsuariosAcciones.Controls.Add(this.btnResetPassUsuario);
        this.pnlUsuariosAcciones.Controls.Add(this.btnActDesactUsuario);
        this.pnlUsuariosAcciones.Controls.Add(this.btnDesbloquearUsuario);
        this.pnlUsuariosAcciones.Controls.Add(this.btnModificarUsuario);
        this.pnlUsuariosAcciones.Controls.Add(this.btnCrearUsuario);
        this.pnlUsuariosAcciones.Controls.Add(this.lblUsuAccionesTit);
        this.pnlUsuariosAcciones.Dock = System.Windows.Forms.DockStyle.Left;
        this.pnlUsuariosAcciones.Size = new System.Drawing.Size(192, 630);

        this.lblUsuAccionesTit.AutoSize = true;
        this.lblUsuAccionesTit.Font = new System.Drawing.Font("Segoe UI Semibold", 11F);
        this.lblUsuAccionesTit.ForeColor = System.Drawing.Color.FromArgb(29, 29, 31);
        this.lblUsuAccionesTit.Location = new System.Drawing.Point(18, 18);
        this.lblUsuAccionesTit.Text = "Gestión de Usuarios";

        this.btnCrearUsuario.BackColor = System.Drawing.Color.FromArgb(83, 6, 24);
        this.btnCrearUsuario.CornerRadius = 10;
        this.btnCrearUsuario.ForeColor = System.Drawing.Color.White;
        this.btnCrearUsuario.HoverBackColor = System.Drawing.Color.FromArgb(115, 16, 40);
        this.btnCrearUsuario.Location = new System.Drawing.Point(16, 56);
        this.btnCrearUsuario.Name = "btnCrearUsuario";
        this.btnCrearUsuario.Size = new System.Drawing.Size(160, 40);
        this.btnCrearUsuario.Text = "Crear";

        this.btnModificarUsuario.BackColor = System.Drawing.Color.FromArgb(245, 245, 247);
        this.btnModificarUsuario.BorderColor = System.Drawing.Color.FromArgb(229, 231, 235);
        this.btnModificarUsuario.BorderThickness = 1;
        this.btnModificarUsuario.CornerRadius = 10;
        this.btnModificarUsuario.ForeColor = System.Drawing.Color.FromArgb(83, 6, 24);
        this.btnModificarUsuario.HoverBackColor = System.Drawing.Color.FromArgb(255, 240, 243);
        this.btnModificarUsuario.Location = new System.Drawing.Point(16, 106);
        this.btnModificarUsuario.Name = "btnModificarUsuario";
        this.btnModificarUsuario.Size = new System.Drawing.Size(160, 40);
        this.btnModificarUsuario.Text = "Modificar";

        this.btnDesbloquearUsuario.BackColor = System.Drawing.Color.FromArgb(245, 245, 247);
        this.btnDesbloquearUsuario.BorderColor = System.Drawing.Color.FromArgb(229, 231, 235);
        this.btnDesbloquearUsuario.BorderThickness = 1;
        this.btnDesbloquearUsuario.CornerRadius = 10;
        this.btnDesbloquearUsuario.ForeColor = System.Drawing.Color.FromArgb(5, 150, 105);
        this.btnDesbloquearUsuario.HoverBackColor = System.Drawing.Color.FromArgb(236, 253, 245);
        this.btnDesbloquearUsuario.Location = new System.Drawing.Point(16, 156);
        this.btnDesbloquearUsuario.Name = "btnDesbloquearUsuario";
        this.btnDesbloquearUsuario.Size = new System.Drawing.Size(160, 40);
        this.btnDesbloquearUsuario.Text = "Desbloquear";

        this.btnActDesactUsuario.BackColor = System.Drawing.Color.FromArgb(255, 241, 242);
        this.btnActDesactUsuario.BorderColor = System.Drawing.Color.FromArgb(254, 205, 211);
        this.btnActDesactUsuario.BorderThickness = 1;
        this.btnActDesactUsuario.CornerRadius = 10;
        this.btnActDesactUsuario.ForeColor = System.Drawing.Color.FromArgb(159, 18, 57);
        this.btnActDesactUsuario.HoverBackColor = System.Drawing.Color.FromArgb(254, 226, 226);
        this.btnActDesactUsuario.Location = new System.Drawing.Point(16, 206);
        this.btnActDesactUsuario.Name = "btnActDesactUsuario";
        this.btnActDesactUsuario.Size = new System.Drawing.Size(160, 40);
        this.btnActDesactUsuario.Text = "Act / Desact";

        this.btnResetPassUsuario.BackColor = System.Drawing.Color.FromArgb(245, 245, 247);
        this.btnResetPassUsuario.BorderColor = System.Drawing.Color.FromArgb(229, 231, 235);
        this.btnResetPassUsuario.BorderThickness = 1;
        this.btnResetPassUsuario.CornerRadius = 10;
        this.btnResetPassUsuario.ForeColor = System.Drawing.Color.FromArgb(29, 29, 31);
        this.btnResetPassUsuario.HoverBackColor = System.Drawing.Color.FromArgb(238, 238, 242);
        this.btnResetPassUsuario.Location = new System.Drawing.Point(16, 256);
        this.btnResetPassUsuario.Name = "btnResetPassUsuario";
        this.btnResetPassUsuario.Size = new System.Drawing.Size(160, 40);
        this.btnResetPassUsuario.Text = "Cambiar Clave";

        this.cmdActualizarUsuarios.BackColor = System.Drawing.Color.White;
        this.cmdActualizarUsuarios.BorderColor = System.Drawing.Color.FromArgb(229, 231, 235);
        this.cmdActualizarUsuarios.BorderThickness = 1;
        this.cmdActualizarUsuarios.CornerRadius = 10;
        this.cmdActualizarUsuarios.ForeColor = System.Drawing.Color.FromArgb(110, 110, 115);
        this.cmdActualizarUsuarios.HoverBackColor = System.Drawing.Color.FromArgb(245, 245, 247);
        this.cmdActualizarUsuarios.Location = new System.Drawing.Point(16, 316);
        this.cmdActualizarUsuarios.Name = "cmdActualizarUsuarios";
        this.cmdActualizarUsuarios.Size = new System.Drawing.Size(160, 36);
        this.cmdActualizarUsuarios.Text = "↻ Actualizar";

        this.pnlUsuariosRightWrap.BackColor = System.Drawing.Color.FromArgb(245, 245, 247);
        this.pnlUsuariosRightWrap.Controls.Add(this.pnlUsuariosGridCard);
        this.pnlUsuariosRightWrap.Controls.Add(this.pnlUsuariosGap);
        this.pnlUsuariosRightWrap.Controls.Add(this.pnlUsuariosEditorCard);
        this.pnlUsuariosRightWrap.Dock = System.Windows.Forms.DockStyle.Fill;
        this.pnlUsuariosRightWrap.Padding = new System.Windows.Forms.Padding(12, 0, 0, 0);

        this.pnlUsuariosGridCard.Controls.Add(this.dgvUsuarios);
        this.pnlUsuariosGridCard.Controls.Add(this.pnlUsuariosTopFilter);
        this.pnlUsuariosGridCard.Dock = System.Windows.Forms.DockStyle.Fill;
        this.pnlUsuariosGridCard.Padding = new System.Windows.Forms.Padding(16, 12, 16, 12);

        this.pnlUsuariosTopFilter.BackColor = System.Drawing.Color.White;
        this.pnlUsuariosTopFilter.Controls.Add(this.rbMostrarInactivos);
        this.pnlUsuariosTopFilter.Controls.Add(this.rbMostrarActivos);
        this.pnlUsuariosTopFilter.Controls.Add(this.rbMostrarTodos);
        this.pnlUsuariosTopFilter.Controls.Add(this.lblUsuGridTit);
        this.pnlUsuariosTopFilter.Dock = System.Windows.Forms.DockStyle.Top;
        this.pnlUsuariosTopFilter.Height = 36;

        this.lblUsuGridTit.AutoSize = true;
        this.lblUsuGridTit.Font = new System.Drawing.Font("Segoe UI Semibold", 10.5F);
        this.lblUsuGridTit.ForeColor = System.Drawing.Color.FromArgb(29, 29, 31);
        this.lblUsuGridTit.Location = new System.Drawing.Point(0, 6);
        this.lblUsuGridTit.Text = "Usuarios Registrados y Perfiles Asignados";

        this.rbMostrarTodos.AutoSize = true;
        this.rbMostrarTodos.Checked = true;
        this.rbMostrarTodos.Font = new System.Drawing.Font("Segoe UI Semibold", 9F);
        this.rbMostrarTodos.Location = new System.Drawing.Point(450, 7);
        this.rbMostrarTodos.Text = "Todos";

        this.rbMostrarActivos.AutoSize = true;
        this.rbMostrarActivos.Font = new System.Drawing.Font("Segoe UI Semibold", 9F);
        this.rbMostrarActivos.Location = new System.Drawing.Point(530, 7);
        this.rbMostrarActivos.Text = "Activos";

        this.rbMostrarInactivos.AutoSize = true;
        this.rbMostrarInactivos.Font = new System.Drawing.Font("Segoe UI Semibold", 9F);
        this.rbMostrarInactivos.Location = new System.Drawing.Point(618, 7);
        this.rbMostrarInactivos.Text = "Inactivos";

        this.dgvUsuarios.Dock = System.Windows.Forms.DockStyle.Fill;

        this.pnlUsuariosGap.BackColor = System.Drawing.Color.FromArgb(245, 245, 247);
        this.pnlUsuariosGap.Dock = System.Windows.Forms.DockStyle.Bottom;
        this.pnlUsuariosGap.Height = 12;

        this.pnlUsuariosEditorCard.Controls.Add(this.cmdCancelarUsuario);
        this.pnlUsuariosEditorCard.Controls.Add(this.cmdAceptarUsuario);
        this.pnlUsuariosEditorCard.Controls.Add(this.chkUsuDesactivar);
        this.pnlUsuariosEditorCard.Controls.Add(this.chkUsuActivar);
        this.pnlUsuariosEditorCard.Controls.Add(this.cmbUsuRol);
        this.pnlUsuariosEditorCard.Controls.Add(this.lblUsuRol);
        this.pnlUsuariosEditorCard.Controls.Add(this.txtUsuUsername);
        this.pnlUsuariosEditorCard.Controls.Add(this.lblUsuUsername);
        this.pnlUsuariosEditorCard.Controls.Add(this.txtUsuApellido);
        this.pnlUsuariosEditorCard.Controls.Add(this.lblUsuApellido);
        this.pnlUsuariosEditorCard.Controls.Add(this.txtUsuNombre);
        this.pnlUsuariosEditorCard.Controls.Add(this.lblUsuNombre);
        this.pnlUsuariosEditorCard.Controls.Add(this.txtUsuDni);
        this.pnlUsuariosEditorCard.Controls.Add(this.lblUsuDni);
        this.pnlUsuariosEditorCard.Controls.Add(this.lblUsuModoTitulo);
        this.pnlUsuariosEditorCard.Dock = System.Windows.Forms.DockStyle.Bottom;
        this.pnlUsuariosEditorCard.Height = 196;

        this.lblUsuModoTitulo.AutoSize = true;
        this.lblUsuModoTitulo.Font = new System.Drawing.Font("Segoe UI Semibold", 10F);
        this.lblUsuModoTitulo.ForeColor = System.Drawing.Color.FromArgb(83, 6, 24);
        this.lblUsuModoTitulo.Location = new System.Drawing.Point(20, 14);
        this.lblUsuModoTitulo.Text = "Ficha del Usuario (Seleccioná 'Crear' o 'Modificar' a la izquierda)";

        this.lblUsuDni.AutoSize = true;
        this.lblUsuDni.Font = new System.Drawing.Font("Segoe UI Semibold", 8F);
        this.lblUsuDni.ForeColor = System.Drawing.Color.FromArgb(110, 110, 115);
        this.lblUsuDni.Location = new System.Drawing.Point(20, 46);
        this.lblUsuDni.Text = "DNI (8 DÍGITOS)";

        this.txtUsuDni.Enabled = false;
        this.txtUsuDni.Location = new System.Drawing.Point(20, 66);
        this.txtUsuDni.MaxLength = 8;
        this.txtUsuDni.Size = new System.Drawing.Size(145, 25);

        this.lblUsuNombre.AutoSize = true;
        this.lblUsuNombre.Font = new System.Drawing.Font("Segoe UI Semibold", 8F);
        this.lblUsuNombre.ForeColor = System.Drawing.Color.FromArgb(110, 110, 115);
        this.lblUsuNombre.Location = new System.Drawing.Point(182, 46);
        this.lblUsuNombre.Text = "NOMBRE";

        this.txtUsuNombre.Enabled = false;
        this.txtUsuNombre.Location = new System.Drawing.Point(182, 66);
        this.txtUsuNombre.Size = new System.Drawing.Size(185, 25);

        this.lblUsuApellido.AutoSize = true;
        this.lblUsuApellido.Font = new System.Drawing.Font("Segoe UI Semibold", 8F);
        this.lblUsuApellido.ForeColor = System.Drawing.Color.FromArgb(110, 110, 115);
        this.lblUsuApellido.Location = new System.Drawing.Point(384, 46);
        this.lblUsuApellido.Text = "APELLIDO";

        this.txtUsuApellido.Enabled = false;
        this.txtUsuApellido.Location = new System.Drawing.Point(384, 66);
        this.txtUsuApellido.Size = new System.Drawing.Size(185, 25);

        this.lblUsuUsername.AutoSize = true;
        this.lblUsuUsername.Font = new System.Drawing.Font("Segoe UI Semibold", 8F);
        this.lblUsuUsername.ForeColor = System.Drawing.Color.FromArgb(110, 110, 115);
        this.lblUsuUsername.Location = new System.Drawing.Point(586, 46);
        this.lblUsuUsername.Text = "NOMBRE DE USUARIO";

        this.txtUsuUsername.Enabled = false;
        this.txtUsuUsername.Location = new System.Drawing.Point(586, 66);
        this.txtUsuUsername.Size = new System.Drawing.Size(175, 25);

        this.lblUsuRol.AutoSize = true;
        this.lblUsuRol.Font = new System.Drawing.Font("Segoe UI Semibold", 8F);
        this.lblUsuRol.ForeColor = System.Drawing.Color.FromArgb(110, 110, 115);
        this.lblUsuRol.Location = new System.Drawing.Point(20, 108);
        this.lblUsuRol.Text = "ROL / PERFIL DE BOTONES ASIGNADO";

        this.cmbUsuRol.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
        this.cmbUsuRol.Enabled = false;
        this.cmbUsuRol.Location = new System.Drawing.Point(20, 128);
        this.cmbUsuRol.Size = new System.Drawing.Size(270, 25);

        this.chkUsuActivar.AutoSize = true;
        this.chkUsuActivar.Enabled = false;
        this.chkUsuActivar.Font = new System.Drawing.Font("Segoe UI Semibold", 9F);
        this.chkUsuActivar.Location = new System.Drawing.Point(312, 130);
        this.chkUsuActivar.Text = "Activo";

        this.chkUsuDesactivar.AutoSize = true;
        this.chkUsuDesactivar.Enabled = false;
        this.chkUsuDesactivar.Font = new System.Drawing.Font("Segoe UI Semibold", 9F);
        this.chkUsuDesactivar.Location = new System.Drawing.Point(388, 130);
        this.chkUsuDesactivar.Text = "Inactivo";

        this.cmdAceptarUsuario.BackColor = System.Drawing.Color.FromArgb(5, 150, 105);
        this.cmdAceptarUsuario.CornerRadius = 10;
        this.cmdAceptarUsuario.Enabled = false;
        this.cmdAceptarUsuario.ForeColor = System.Drawing.Color.White;
        this.cmdAceptarUsuario.HoverBackColor = System.Drawing.Color.FromArgb(4, 120, 87);
        this.cmdAceptarUsuario.Location = new System.Drawing.Point(515, 122);
        this.cmdAceptarUsuario.Name = "cmdAceptarUsuario";
        this.cmdAceptarUsuario.Size = new System.Drawing.Size(120, 38);
        this.cmdAceptarUsuario.Text = "Aceptar";
        this.cmdAceptarUsuario.Visible = false;

        this.cmdCancelarUsuario.BackColor = System.Drawing.Color.FromArgb(245, 245, 247);
        this.cmdCancelarUsuario.BorderColor = System.Drawing.Color.FromArgb(229, 231, 235);
        this.cmdCancelarUsuario.BorderThickness = 1;
        this.cmdCancelarUsuario.CornerRadius = 10;
        this.cmdCancelarUsuario.Enabled = false;
        this.cmdCancelarUsuario.ForeColor = System.Drawing.Color.FromArgb(29, 29, 31);
        this.cmdCancelarUsuario.Location = new System.Drawing.Point(645, 122);
        this.cmdCancelarUsuario.Name = "cmdCancelarUsuario";
        this.cmdCancelarUsuario.Size = new System.Drawing.Size(116, 38);
        this.cmdCancelarUsuario.Text = "Cancelar";
        this.cmdCancelarUsuario.Visible = false;

        // ====================================================================
        // SECCIÓN 4: GESTIÓN DE PERFILES (ING-SOFTWARE-BritezG · SOLO BOTONES)
        // ====================================================================
        this.pnlSecRoles.BackColor = System.Drawing.Color.FromArgb(245, 245, 247);
        this.pnlSecRoles.Controls.Add(this.tlpPerfilesColumnas);
        this.pnlSecRoles.Controls.Add(this.TS_Gestion);
        this.pnlSecRoles.Dock = System.Windows.Forms.DockStyle.Fill;

        this.TS_Gestion.AutoSize = false;
        this.TS_Gestion.BackColor = System.Drawing.Color.FromArgb(46, 5, 16);
        this.TS_Gestion.Dock = System.Windows.Forms.DockStyle.Top;
        this.TS_Gestion.Font = new System.Drawing.Font("Segoe UI Semibold", 9.5F);
        this.TS_Gestion.ForeColor = System.Drawing.Color.White;
        this.TS_Gestion.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
        this.TS_Gestion.Padding = new System.Windows.Forms.Padding(10, 5, 10, 5);
        this.TS_Gestion.Size = new System.Drawing.Size(992, 44);
        this.TS_Gestion.Items.AddRange(new System.Windows.Forms.ToolStripItem[]
        {
            this.toolStripBtnAgregar,
            this.toolStripBtnEliminar,
            this.tsBtnRelacionFamilia,
            this.tsLblReglaBotones
        });

        this.toolStripBtnAgregar.BackColor = System.Drawing.Color.FromArgb(83, 6, 24);
        this.toolStripBtnAgregar.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
        this.toolStripBtnAgregar.ForeColor = System.Drawing.Color.White;
        this.toolStripBtnAgregar.Margin = new System.Windows.Forms.Padding(2, 2, 8, 2);
        this.toolStripBtnAgregar.Padding = new System.Windows.Forms.Padding(10, 4, 10, 4);
        this.toolStripBtnAgregar.Text = "➕ Agregar";
        this.toolStripBtnAgregar.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[]
        {
            this.tsMenuAddFamiliaAPerfil,
            this.tsMenuAddPermisoAPerfil,
            this.tsMenuAddPermisoAFamilia,
            this.tsSepAgregar,
            this.tsMenuAddFamilia,
            this.tsMenuAddPerfil,
            this.tsMenuAddPermiso
        });

        this.tsMenuAddFamiliaAPerfil.Text = "Familia a Perfil";
        this.tsMenuAddPermisoAPerfil.Text = "Permiso (Botón) a Perfil";
        this.tsMenuAddPermisoAFamilia.Text = "Permiso (Botón) a Familia";
        this.tsMenuAddFamilia.Text = "Nueva Familia";
        this.tsMenuAddPerfil.Text = "Nuevo Perfil";
        this.tsMenuAddPermiso.Text = "Permiso (Botón)";

        this.toolStripBtnEliminar.BackColor = System.Drawing.Color.FromArgb(135, 24, 52);
        this.toolStripBtnEliminar.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
        this.toolStripBtnEliminar.ForeColor = System.Drawing.Color.White;
        this.toolStripBtnEliminar.Margin = new System.Windows.Forms.Padding(2, 2, 8, 2);
        this.toolStripBtnEliminar.Padding = new System.Windows.Forms.Padding(10, 4, 10, 4);
        this.toolStripBtnEliminar.Text = "🗑 Eliminar";
        this.toolStripBtnEliminar.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[]
        {
            this.tsMenuDelFamiliaDePerfil,
            this.tsMenuDelPermisoDePerfil,
            this.tsMenuDelPermisoDeFamilia,
            this.tsSepEliminar,
            this.tsMenuDelPerfil,
            this.tsMenuDelFamilia,
            this.tsMenuDelPermiso
        });

        this.tsMenuDelFamiliaDePerfil.Text = "Familia de Perfil";
        this.tsMenuDelPermisoDePerfil.Text = "Permiso (Botón) de Perfil";
        this.tsMenuDelPermisoDeFamilia.Text = "Permiso (Botón) de Familia";
        this.tsMenuDelPerfil.Text = "Eliminar Perfil";
        this.tsMenuDelFamilia.Text = "Eliminar Familia";
        this.tsMenuDelPermiso.Text = "Eliminar Permiso (Botón)";

        this.tsBtnRelacionFamilia.BackColor = System.Drawing.Color.FromArgb(83, 6, 24);
        this.tsBtnRelacionFamilia.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
        this.tsBtnRelacionFamilia.ForeColor = System.Drawing.Color.White;
        this.tsBtnRelacionFamilia.Margin = new System.Windows.Forms.Padding(2, 2, 8, 2);
        this.tsBtnRelacionFamilia.Padding = new System.Windows.Forms.Padding(10, 4, 10, 4);
        this.tsBtnRelacionFamilia.Text = "🔗 Relación Familia";

        this.tsLblReglaBotones.Font = new System.Drawing.Font("Segoe UI", 8.8F);
        this.tsLblReglaBotones.ForeColor = System.Drawing.Color.FromArgb(245, 215, 222);
        this.tsLblReglaBotones.Text = "   ·   Regla activa: 1 Permiso = 1 Botón (si el perfil tiene el permiso, verá ese botón)";

        this.tlpPerfilesColumnas.ColumnCount = 4;
        this.tlpPerfilesColumnas.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 27F));
        this.tlpPerfilesColumnas.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 21F));
        this.tlpPerfilesColumnas.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 21F));
        this.tlpPerfilesColumnas.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 31F));
        this.tlpPerfilesColumnas.RowCount = 1;
        this.tlpPerfilesColumnas.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
        this.tlpPerfilesColumnas.Dock = System.Windows.Forms.DockStyle.Fill;
        this.tlpPerfilesColumnas.Padding = new System.Windows.Forms.Padding(0, 12, 0, 0);
        this.tlpPerfilesColumnas.Controls.Add(this.pnlCardPermisos, 0, 0);
        this.tlpPerfilesColumnas.Controls.Add(this.pnlCardFamilias, 1, 0);
        this.tlpPerfilesColumnas.Controls.Add(this.pnlCardPerfiles, 2, 0);
        this.tlpPerfilesColumnas.Controls.Add(this.pnlCardArbol, 3, 0);

        // Columna 1: Permisos (Botones)
        this.pnlCardPermisos.Controls.Add(this.dgvPermisos);
        this.pnlCardPermisos.Controls.Add(this.pnlHdrPermisos);
        this.pnlCardPermisos.Dock = System.Windows.Forms.DockStyle.Fill;
        this.pnlCardPermisos.Margin = new System.Windows.Forms.Padding(0, 0, 8, 0);
        this.pnlCardPermisos.Padding = new System.Windows.Forms.Padding(8);

        this.pnlHdrPermisos.BackColor = System.Drawing.Color.FromArgb(83, 6, 24);
        this.pnlHdrPermisos.Controls.Add(this.lblHdrPermisos);
        this.pnlHdrPermisos.Dock = System.Windows.Forms.DockStyle.Top;
        this.pnlHdrPermisos.Height = 36;
        this.lblHdrPermisos.Dock = System.Windows.Forms.DockStyle.Fill;
        this.lblHdrPermisos.Font = new System.Drawing.Font("Segoe UI Semibold", 9.8F);
        this.lblHdrPermisos.ForeColor = System.Drawing.Color.White;
        this.lblHdrPermisos.Text = "🔘 Permisos (Solo Botones)";
        this.lblHdrPermisos.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
        this.dgvPermisos.Dock = System.Windows.Forms.DockStyle.Fill;

        // Columna 2: Familias Existentes
        this.pnlCardFamilias.Controls.Add(this.dgvFamilias);
        this.pnlCardFamilias.Controls.Add(this.pnlHdrFamilias);
        this.pnlCardFamilias.Dock = System.Windows.Forms.DockStyle.Fill;
        this.pnlCardFamilias.Margin = new System.Windows.Forms.Padding(4, 0, 8, 0);
        this.pnlCardFamilias.Padding = new System.Windows.Forms.Padding(8);

        this.pnlHdrFamilias.BackColor = System.Drawing.Color.FromArgb(83, 6, 24);
        this.pnlHdrFamilias.Controls.Add(this.lblHdrFamilias);
        this.pnlHdrFamilias.Dock = System.Windows.Forms.DockStyle.Top;
        this.pnlHdrFamilias.Height = 36;
        this.lblHdrFamilias.Dock = System.Windows.Forms.DockStyle.Fill;
        this.lblHdrFamilias.Font = new System.Drawing.Font("Segoe UI Semibold", 9.8F);
        this.lblHdrFamilias.ForeColor = System.Drawing.Color.White;
        this.lblHdrFamilias.Text = "📦 Familias Existentes";
        this.lblHdrFamilias.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
        this.dgvFamilias.Dock = System.Windows.Forms.DockStyle.Fill;

        // Columna 3: Perfiles Existentes
        this.pnlCardPerfiles.Controls.Add(this.dgvPerfiles);
        this.pnlCardPerfiles.Controls.Add(this.pnlHdrPerfiles);
        this.pnlCardPerfiles.Dock = System.Windows.Forms.DockStyle.Fill;
        this.pnlCardPerfiles.Margin = new System.Windows.Forms.Padding(4, 0, 8, 0);
        this.pnlCardPerfiles.Padding = new System.Windows.Forms.Padding(8);

        this.pnlHdrPerfiles.BackColor = System.Drawing.Color.FromArgb(83, 6, 24);
        this.pnlHdrPerfiles.Controls.Add(this.lblHdrPerfiles);
        this.pnlHdrPerfiles.Dock = System.Windows.Forms.DockStyle.Top;
        this.pnlHdrPerfiles.Height = 36;
        this.lblHdrPerfiles.Dock = System.Windows.Forms.DockStyle.Fill;
        this.lblHdrPerfiles.Font = new System.Drawing.Font("Segoe UI Semibold", 9.8F);
        this.lblHdrPerfiles.ForeColor = System.Drawing.Color.White;
        this.lblHdrPerfiles.Text = "👤 Perfiles Existentes";
        this.lblHdrPerfiles.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
        this.dgvPerfiles.Dock = System.Windows.Forms.DockStyle.Fill;

        // Columna 4: Árbol Jerarquía Composite
        this.pnlCardArbol.Controls.Add(this.Vista_Familia);
        this.pnlCardArbol.Controls.Add(this.pnlHdrArbol);
        this.pnlCardArbol.Dock = System.Windows.Forms.DockStyle.Fill;
        this.pnlCardArbol.Margin = new System.Windows.Forms.Padding(4, 0, 0, 0);
        this.pnlCardArbol.Padding = new System.Windows.Forms.Padding(8);

        this.pnlHdrArbol.BackColor = System.Drawing.Color.FromArgb(46, 5, 16);
        this.pnlHdrArbol.Controls.Add(this.lblHdrArbol);
        this.pnlHdrArbol.Dock = System.Windows.Forms.DockStyle.Top;
        this.pnlHdrArbol.Height = 36;
        this.lblHdrArbol.Dock = System.Windows.Forms.DockStyle.Fill;
        this.lblHdrArbol.Font = new System.Drawing.Font("Segoe UI Semibold", 9.8F);
        this.lblHdrArbol.ForeColor = System.Drawing.Color.White;
        this.lblHdrArbol.Text = "🌳 Jerarquía Composite";
        this.lblHdrArbol.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;

        this.Vista_Familia.BackColor = System.Drawing.Color.FromArgb(252, 252, 253);
        this.Vista_Familia.BorderStyle = System.Windows.Forms.BorderStyle.None;
        this.Vista_Familia.Dock = System.Windows.Forms.DockStyle.Fill;
        this.Vista_Familia.Font = new System.Drawing.Font("Segoe UI", 9.2F);
        this.Vista_Familia.ItemHeight = 24;

        // ====================================================================
        // SECCIÓN 5: DÍGITO VERIFICADOR
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
        this.btnVerificarDv.Name = "btnVerificarDv";
        this.btnVerificarDv.Size = new System.Drawing.Size(220, 40);
        this.btnVerificarDv.Text = "Verificar Integridad Ahora";

        this.btnRecalcularDv.BackColor = System.Drawing.Color.FromArgb(83, 6, 24);
        this.btnRecalcularDv.CornerRadius = 10;
        this.btnRecalcularDv.ForeColor = System.Drawing.Color.White;
        this.btnRecalcularDv.Location = new System.Drawing.Point(254, 54);
        this.btnRecalcularDv.Name = "btnRecalcularDv";
        this.btnRecalcularDv.Size = new System.Drawing.Size(250, 40);
        this.btnRecalcularDv.Text = "Recalcular Digito Verificador";

        this.dgvDvv.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
        this.dgvDvv.Location = new System.Drawing.Point(22, 110);
        this.dgvDvv.Size = new System.Drawing.Size(948, 250);

        this.pnlDvReporte.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
        this.pnlDvReporte.AutoScroll = true;
        this.pnlDvReporte.BackColor = System.Drawing.Color.FromArgb(255, 248, 249);
        this.pnlDvReporte.BorderColor = System.Drawing.Color.FromArgb(240, 212, 218);
        this.pnlDvReporte.Controls.Add(this.lblDetalleIntegridadPanel);
        this.pnlDvReporte.Location = new System.Drawing.Point(22, 376);
        this.pnlDvReporte.Size = new System.Drawing.Size(948, 230);

        this.lblDetalleIntegridadPanel.AutoSize = true;
        this.lblDetalleIntegridadPanel.Location = new System.Drawing.Point(16, 16);
        this.lblDetalleIntegridadPanel.MaximumSize = new System.Drawing.Size(900, 0);
        this.lblDetalleIntegridadPanel.Text = "Estado de integridad...";

        // ====================================================================
        // SECCIÓN 6: RESTOREBACKUP & SUPABASE
        // ====================================================================
        this.pnlSecRestoreSupabase.BackColor = System.Drawing.Color.FromArgb(245, 245, 247);
        this.pnlSecRestoreSupabase.Controls.Add(this.splitRestoreSupa);
        this.pnlSecRestoreSupabase.Dock = System.Windows.Forms.DockStyle.Fill;

        this.splitRestoreSupa.BackColor = System.Drawing.Color.FromArgb(245, 245, 247);
        this.splitRestoreSupa.Dock = System.Windows.Forms.DockStyle.Fill;
        this.splitRestoreSupa.Panel1.Controls.Add(this.pnlBackupCard);
        this.splitRestoreSupa.Panel2.Controls.Add(this.pnlSupaCard);
        this.splitRestoreSupa.Size = new System.Drawing.Size(992, 630);
        this.splitRestoreSupa.SplitterDistance = 490;
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
        this.lblBkDesc.Location = new System.Drawing.Point(20, 72); this.lblBkDesc.Size = new System.Drawing.Size(435, 42);
        this.lblBkDesc.Text = "Generá copias firmadas con SHA-256 o cargá un backup para restaurar la base actual.";

        this.btnCrearBackup.BackColor = System.Drawing.Color.FromArgb(5, 150, 105);
        this.btnCrearBackup.CornerRadius = 12;
        this.btnCrearBackup.ForeColor = System.Drawing.Color.White;
        this.btnCrearBackup.Location = new System.Drawing.Point(20, 124);
        this.btnCrearBackup.Name = "btnCrearBackup";
        this.btnCrearBackup.Size = new System.Drawing.Size(435, 42);
        this.btnCrearBackup.Text = "1. Generar Backup de Base Actual";

        this.lblBkSel.AutoSize = true; this.lblBkSel.Font = new System.Drawing.Font("Segoe UI Semibold", 8F);
        this.lblBkSel.Location = new System.Drawing.Point(20, 184); this.lblBkSel.Text = "ARCHIVO BACKUP PARA CARGAR:";
        this.txtRutaRestore.Location = new System.Drawing.Point(20, 206); this.txtRutaRestore.ReadOnly = true; this.txtRutaRestore.Size = new System.Drawing.Size(310, 23);

        this.btnExaminarBk.BackColor = System.Drawing.Color.FromArgb(245, 245, 247);
        this.btnExaminarBk.BorderColor = System.Drawing.Color.FromArgb(229, 231, 235);
        this.btnExaminarBk.BorderThickness = 1;
        this.btnExaminarBk.CornerRadius = 8;
        this.btnExaminarBk.ForeColor = System.Drawing.Color.FromArgb(83, 6, 24);
        this.btnExaminarBk.Location = new System.Drawing.Point(339, 202);
        this.btnExaminarBk.Name = "btnExaminarBk";
        this.btnExaminarBk.Size = new System.Drawing.Size(116, 30);
        this.btnExaminarBk.Text = "Examinar...";

        this.pnlPreviewBk.BackColor = System.Drawing.Color.FromArgb(255, 248, 249);
        this.pnlPreviewBk.BorderColor = System.Drawing.Color.FromArgb(240, 212, 218);
        this.pnlPreviewBk.Controls.Add(this.lblPreviewBackup);
        this.pnlPreviewBk.Location = new System.Drawing.Point(20, 246); this.pnlPreviewBk.Padding = new System.Windows.Forms.Padding(12); this.pnlPreviewBk.Size = new System.Drawing.Size(435, 145);
        this.lblPreviewBackup.Dock = System.Windows.Forms.DockStyle.Fill;
        this.lblPreviewBackup.Text = "Seleccioná un backup para inspeccionarlo antes de restaurar.";

        this.btnEjecutarRestore.BackColor = System.Drawing.Color.FromArgb(83, 6, 24);
        this.btnEjecutarRestore.CornerRadius = 12;
        this.btnEjecutarRestore.ForeColor = System.Drawing.Color.White;
        this.btnEjecutarRestore.Location = new System.Drawing.Point(20, 408);
        this.btnEjecutarRestore.Name = "btnEjecutarRestore";
        this.btnEjecutarRestore.Size = new System.Drawing.Size(435, 44);
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
        this.txtSupaUrl.Location = new System.Drawing.Point(20, 106); this.txtSupaUrl.Size = new System.Drawing.Size(435, 23);
        this.lblSupaService.AutoSize = true; this.lblSupaService.Font = new System.Drawing.Font("Segoe UI Semibold", 8F);
        this.lblSupaService.Location = new System.Drawing.Point(20, 146); this.lblSupaService.Text = "SERVICE_ROLE / SECRET KEY (sb_secret_...):";
        this.txtSupaServiceKey.Location = new System.Drawing.Point(20, 166); this.txtSupaServiceKey.Size = new System.Drawing.Size(435, 23); this.txtSupaServiceKey.UseSystemPasswordChar = true;
        this.lblSupaAnon.AutoSize = true; this.lblSupaAnon.Font = new System.Drawing.Font("Segoe UI Semibold", 8F);
        this.lblSupaAnon.Location = new System.Drawing.Point(20, 206); this.lblSupaAnon.Text = "PUBLISHABLE / ANON KEY (sb_publishable_...):";
        this.txtSupaAnonKey.Location = new System.Drawing.Point(20, 226); this.txtSupaAnonKey.Size = new System.Drawing.Size(435, 23);

        this.btnGuardarSupa.BackColor = System.Drawing.Color.FromArgb(83, 6, 24);
        this.btnGuardarSupa.CornerRadius = 12;
        this.btnGuardarSupa.ForeColor = System.Drawing.Color.White;
        this.btnGuardarSupa.Location = new System.Drawing.Point(20, 272);
        this.btnGuardarSupa.Name = "btnGuardarSupa";
        this.btnGuardarSupa.Size = new System.Drawing.Size(435, 42);
        this.btnGuardarSupa.Text = "Guardar y Probar Conexión Supabase";

        this.btnEmpujarNube.BackColor = System.Drawing.Color.FromArgb(245, 245, 247);
        this.btnEmpujarNube.BorderColor = System.Drawing.Color.FromArgb(229, 231, 235);
        this.btnEmpujarNube.BorderThickness = 1;
        this.btnEmpujarNube.CornerRadius = 10;
        this.btnEmpujarNube.ForeColor = System.Drawing.Color.FromArgb(83, 6, 24);
        this.btnEmpujarNube.Location = new System.Drawing.Point(20, 326);
        this.btnEmpujarNube.Name = "btnEmpujarNube";
        this.btnEmpujarNube.Size = new System.Drawing.Size(435, 38);
        this.btnEmpujarNube.Text = "Sincronizar Datos hacia Supabase";

        this.lblResultadoSupa.Location = new System.Drawing.Point(20, 382);
        this.lblResultadoSupa.Size = new System.Drawing.Size(435, 130);
        this.lblResultadoSupa.Text = "Verificando estado de credenciales Supabase...";

        // ====================================================================
        // FORM PRINCIPAL
        // ====================================================================
        this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.BackColor = System.Drawing.Color.FromArgb(245, 245, 247);
        this.ClientSize = new System.Drawing.Size(1260, 720);
        this.Controls.Add(this.pnlMain);
        this.Controls.Add(this.pnlSidebar);
        this.MinimumSize = new System.Drawing.Size(1140, 690);
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
        this.pnlSecDespachantes.ResumeLayout(false);
        this.splitDespachantes.Panel1.ResumeLayout(false);
        this.splitDespachantes.Panel2.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)(this.splitDespachantes)).EndInit();
        this.splitDespachantes.ResumeLayout(false);
        this.pnlDespGridWrap.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)(this.dgvDespachantes)).EndInit();
        this.pnlDespForm.ResumeLayout(false);
        this.pnlDespForm.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)(this.nudDespOrden)).EndInit();
        ((System.ComponentModel.ISupportInitialize)(this.picDespachante)).EndInit();
        this.pnlSecPuertos.ResumeLayout(false);
        this.splitPuertos.Panel1.ResumeLayout(false);
        this.splitPuertos.Panel2.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)(this.splitPuertos)).EndInit();
        this.splitPuertos.ResumeLayout(false);
        this.pnlPtoGridWrap.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)(this.dgvPuertos)).EndInit();
        this.pnlPtoForm.ResumeLayout(false);
        this.pnlPtoForm.PerformLayout();
        this.pnlSecUsuarios.ResumeLayout(false);
        this.pnlUsuariosAcciones.ResumeLayout(false);
        this.pnlUsuariosAcciones.PerformLayout();
        this.pnlUsuariosRightWrap.ResumeLayout(false);
        this.pnlUsuariosGridCard.ResumeLayout(false);
        this.pnlUsuariosTopFilter.ResumeLayout(false);
        this.pnlUsuariosTopFilter.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)(this.dgvUsuarios)).EndInit();
        this.pnlUsuariosEditorCard.ResumeLayout(false);
        this.pnlUsuariosEditorCard.PerformLayout();
        this.pnlSecRoles.ResumeLayout(false);
        this.TS_Gestion.ResumeLayout(false);
        this.TS_Gestion.PerformLayout();
        this.tlpPerfilesColumnas.ResumeLayout(false);
        this.pnlCardPermisos.ResumeLayout(false);
        this.pnlHdrPermisos.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)(this.dgvPermisos)).EndInit();
        this.pnlCardFamilias.ResumeLayout(false);
        this.pnlHdrFamilias.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)(this.dgvFamilias)).EndInit();
        this.pnlCardPerfiles.ResumeLayout(false);
        this.pnlHdrPerfiles.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)(this.dgvPerfiles)).EndInit();
        this.pnlCardArbol.ResumeLayout(false);
        this.pnlHdrArbol.ResumeLayout(false);
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
