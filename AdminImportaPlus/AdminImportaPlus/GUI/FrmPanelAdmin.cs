using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Forms;
using AdminImportaPlus.BE;
using AdminImportaPlus.BLL;

namespace AdminImportaPlus.GUI;

public partial class FrmPanelAdmin : Form
{
    public const int SeccionForwarders = 0;
    public const int SeccionDespachantes = 1;
    public const int SeccionPuertos = 2;
    public const int SeccionUsuarios = 3;
    public const int SeccionPerfiles = 4;
    public const int SeccionDigitoVerificador = 5;
    public const int SeccionRestoreSupabase = 6;

    private static readonly HttpClient ImgHttp = new() { Timeout = TimeSpan.FromSeconds(6) };

    private sealed class UsuarioFilaVista
    {
        public Guid Id { get; init; }
        public int Dni { get; init; }
        public string Nombre { get; init; } = string.Empty;
        public string Apellido { get; init; } = string.Empty;
        public string Username { get; init; } = string.Empty;
        public string Perfil { get; init; } = string.Empty;
        public string Bloqueado { get; init; } = string.Empty;
        public string Estado { get; init; } = string.Empty;
        public bool ActivoRaw { get; init; }
    }

    private sealed class ImportaToolStripColorTable : ProfessionalColorTable
    {
        public override Color ToolStripDropDownBackground => Color.White;
        public override Color MenuItemSelected => Color.FromArgb(255, 240, 243);
        public override Color MenuItemBorder => TemaImportaPlus.BrandAccent;
        public override Color MenuBorder => TemaImportaPlus.BrandPrimary;
        public override Color ImageMarginGradientBegin => Color.FromArgb(252, 248, 249);
        public override Color ImageMarginGradientMiddle => Color.FromArgb(252, 248, 249);
        public override Color ImageMarginGradientEnd => Color.FromArgb(252, 248, 249);
        public override Color ButtonSelectedHighlight => TemaImportaPlus.BrandHover;
        public override Color ButtonSelectedGradientBegin => TemaImportaPlus.BrandHover;
        public override Color ButtonSelectedGradientMiddle => TemaImportaPlus.BrandHover;
        public override Color ButtonSelectedGradientEnd => TemaImportaPlus.BrandHover;
        public override Color ButtonPressedGradientBegin => TemaImportaPlus.BrandPrimary;
        public override Color ButtonPressedGradientMiddle => TemaImportaPlus.BrandPrimary;
        public override Color ButtonPressedGradientEnd => TemaImportaPlus.BrandPrimary;
    }

    private readonly AdminGestorBLL _gestor;
    private readonly int _seccionInicialSolicitada;
    private BaseDatosCompletaBE _db = new();

    private int _seccionActual = SeccionForwarders;

    // Estado ABM Forwarders
    private Guid _fwdEditId = Guid.NewGuid();
    private string? _fwdRutaImagenLocalSeleccionada;

    // Estado ABM Despachantes de Aduana
    private Guid _despEditId = Guid.NewGuid();
    private string? _despRutaImagenLocalSeleccionada;

    // Estado ABM Puertos
    private Guid _puertoEditId = Guid.NewGuid();

    // Estado Gestión de Usuarios (ING-SOFTWARE-BritezG)
    private string _usuModoActual = string.Empty;
    private Guid? _usuIdEnEdicion;
    private bool _suprimirEventosArbolPerfiles;

    public FrmPanelAdmin()
    {
        InitializeComponent();
        _gestor = new AdminGestorBLL();
        _seccionInicialSolicitada = SeccionForwarders;
        ConfigurarEstilosYEventos();
    }

    public FrmPanelAdmin(AdminGestorBLL gestor, int seccionInicial = SeccionForwarders)
    {
        InitializeComponent();
        _gestor = gestor;
        _seccionInicialSolicitada = seccionInicial;
        ConfigurarEstilosYEventos();
        MostrarSeccion(seccionInicial);
    }

    private void ConfigurarEstilosYEventos()
    {
        TemaImportaPlus.EstilizarGrilla(dgvForwarders);
        TemaImportaPlus.EstilizarGrilla(dgvDespachantes);
        TemaImportaPlus.EstilizarGrilla(dgvPuertos);
        TemaImportaPlus.EstilizarGrilla(dgvUsuarios);
        TemaImportaPlus.EstilizarGrilla(dgvPermisos);
        TemaImportaPlus.EstilizarGrilla(dgvFamilias);
        TemaImportaPlus.EstilizarGrilla(dgvPerfiles);
        TemaImportaPlus.EstilizarGrilla(dgvDvv);

        TS_Gestion.Renderer = new ToolStripProfessionalRenderer(new ImportaToolStripColorTable())
        {
            RoundedEdges = false
        };

        cmbPuertoVia.SelectedIndex = 0;

        // Navegación Lateral (7 secciones)
        btnNavForwarders.Click += (_, _) => MostrarSeccion(SeccionForwarders);
        btnNavDespachantes.Click += (_, _) => MostrarSeccion(SeccionDespachantes);
        btnNavPuertos.Click += (_, _) => MostrarSeccion(SeccionPuertos);
        btnNavUsuarios.Click += (_, _) => MostrarSeccion(SeccionUsuarios);
        btnNavRoles.Click += (_, _) => MostrarSeccion(SeccionPerfiles);
        btnNavDigitoVerificador.Click += (_, _) => MostrarSeccion(SeccionDigitoVerificador);
        btnNavRestoreSupabase.Click += (_, _) => MostrarSeccion(SeccionRestoreSupabase);

        btnCambiarPassword.Click += (_, _) =>
        {
            using var frm = new FrmRestablecerPassword(_gestor, _gestor.AdminActual?.Username ?? "Admin");
            frm.ShowDialog(this);
            _ = RecargarDatosUIAsync();
        };
        btnCerrarSesion.Click += (_, _) => Close();

        // --- 1. Eventos ABM Forwarders ---
        dgvForwarders.SelectionChanged += (_, _) => CargarForwarderSeleccionadoEnEditor();
        btnSubirFoto.Click += (_, _) => SeleccionarImagenForwarderLocal();
        txtFwdImagenUrl.Leave += async (_, _) => await PrevisualizarImagenEnPictureBoxAsync(txtFwdImagenUrl.Text, picForwarder);
        txtFwdNombre.TextChanged += (_, _) => lblContadorNombreFwd.Text = $"{txtFwdNombre.Text.Length} / 120";
        btnGuardarFwd.Click += async (_, _) => await GuardarForwarderDesdeUIAsync();
        btnNuevoFwd.Click += (_, _) => LimpiarEditorForwarder();
        btnEliminarFwd.Click += async (_, _) => await EliminarForwarderDesdeUIAsync();

        // --- 2. Eventos ABM Despachantes de Aduana ---
        dgvDespachantes.SelectionChanged += (_, _) => CargarDespachanteSeleccionadoEnEditor();
        btnSubirFotoDesp.Click += (_, _) => SeleccionarImagenDespachanteLocal();
        txtDespImagenUrl.Leave += async (_, _) => await PrevisualizarImagenEnPictureBoxAsync(txtDespImagenUrl.Text, picDespachante);
        txtDespNombre.TextChanged += (_, _) => lblContadorNombreDesp.Text = $"{txtDespNombre.Text.Length} / 120";
        btnGuardarDesp.Click += async (_, _) => await GuardarDespachanteDesdeUIAsync();
        btnNuevoDesp.Click += (_, _) => LimpiarEditorDespachante();
        btnEliminarDesp.Click += async (_, _) => await EliminarDespachanteDesdeUIAsync();

        // --- 3. Eventos ABM Puertos y Aduanas ---
        dgvPuertos.SelectionChanged += (_, _) =>
        {
            if (dgvPuertos.CurrentRow?.DataBoundItem is PuertoAduanaBE p)
            {
                _puertoEditId = p.Id;
                txtPuertoNombre.Text = p.Nombre;
                cmbPuertoVia.SelectedItem = p.ViaIngreso;
                txtPuertoProvincia.Text = p.Provincia;
                txtPuertoSim.Text = p.CodigoSim;
                txtPuertoOperador.Text = p.Operador;
                txtPuertoInfra.Text = p.Infraestructura;
                txtPuertoLat.Text = p.Latitud?.ToString(CultureInfo.InvariantCulture) ?? "";
                txtPuertoLng.Text = p.Longitud?.ToString(CultureInfo.InvariantCulture) ?? "";
            }
        };
        btnGuardarPto.Click += async (_, _) =>
        {
            decimal? lat = null;
            decimal? lng = null;

            string rawLat = txtPuertoLat.Text.Trim().Replace(',', '.');
            if (!string.IsNullOrEmpty(rawLat))
            {
                if (!decimal.TryParse(rawLat, NumberStyles.Float, CultureInfo.InvariantCulture, out var parsedLat) || parsedLat < -90m || parsedLat > 90m)
                {
                    MessageBox.Show("Ingresá una latitud numérica válida entre -90 y 90 (ej: -34.5985).", "Validación de Coordenadas", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                lat = parsedLat;
            }

            string rawLng = txtPuertoLng.Text.Trim().Replace(',', '.');
            if (!string.IsNullOrEmpty(rawLng))
            {
                if (!decimal.TryParse(rawLng, NumberStyles.Float, CultureInfo.InvariantCulture, out var parsedLng) || parsedLng < -180m || parsedLng > 180m)
                {
                    MessageBox.Show("Ingresá una longitud numérica válida entre -180 y 180 (ej: -58.3731).", "Validación de Coordenadas", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                lng = parsedLng;
            }

            var res = await _gestor.GuardarPuertoAsync(new PuertoAduanaBE
            {
                Id = _puertoEditId,
                Nombre = txtPuertoNombre.Text,
                ViaIngreso = cmbPuertoVia.SelectedItem?.ToString() ?? "MARITIMA_FLUVIAL",
                Provincia = txtPuertoProvincia.Text,
                CodigoSim = txtPuertoSim.Text,
                Operador = txtPuertoOperador.Text,
                Infraestructura = txtPuertoInfra.Text,
                Latitud = lat,
                Longitud = lng
            });
            MessageBox.Show(res.Mensaje, "Puertos y Aduanas", MessageBoxButtons.OK, res.Ok ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
            if (res.Ok) await RecargarDatosUIAsync();
        };
        btnNuevoPto.Click += (_, _) =>
        {
            _puertoEditId = Guid.NewGuid();
            txtPuertoNombre.Clear(); txtPuertoProvincia.Clear(); txtPuertoSim.Clear();
            txtPuertoOperador.Clear(); txtPuertoInfra.Clear(); txtPuertoLat.Clear(); txtPuertoLng.Clear();
        };
        btnBorrarPto.Click += async (_, _) =>
        {
            if (MessageBox.Show("¿Eliminar el puerto seleccionado?", "Confirmar", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                var res = await _gestor.EliminarPuertoAsync(_puertoEditId);
                MessageBox.Show(res.Mensaje, "Puertos y Aduanas", MessageBoxButtons.OK, res.Ok ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
                await RecargarDatosUIAsync();
            }
        };

        // --- 4. Eventos Gestión de Usuarios (ING-SOFTWARE-BritezG) ---
        rbMostrarTodos.CheckedChanged += (_, _) => { if (rbMostrarTodos.Checked) RefrescarGrillaUsuarios(); };
        rbMostrarActivos.CheckedChanged += (_, _) => { if (rbMostrarActivos.Checked) RefrescarGrillaUsuarios(); };
        rbMostrarInactivos.CheckedChanged += (_, _) => { if (rbMostrarInactivos.Checked) RefrescarGrillaUsuarios(); };
        dgvUsuarios.SelectionChanged += (_, _) => CargarUsuarioSeleccionadoEnFicha();
        cmdActualizarUsuarios.Click += async (_, _) => await RecargarDatosUIAsync();

        btnCrearUsuario.Click += (_, _) => HabilitarModoCrearUsuario();
        btnModificarUsuario.Click += (_, _) => HabilitarModoModificarUsuario();
        cmdCancelarUsuario.Click += (_, _) => CancelarOperacionUsuario();
        cmdAceptarUsuario.Click += async (_, _) => await ConfirmarOperacionUsuarioAsync();
        btnDesbloquearUsuario.Click += async (_, _) => await DesbloquearUsuarioSeleccionadoAsync();
        btnActDesactUsuario.Click += async (_, _) => await CambiarEstadoUsuarioSeleccionadoAsync();
        btnResetPassUsuario.Click += async (_, _) => await CambiarClaveUsuarioSeleccionadoAsync();

        // --- 5. Eventos Gestión de Perfiles (ING-SOFTWARE-BritezG · 1 Permiso = 1 Botón) ---
        dgvFamilias.SelectionChanged += (_, _) =>
        {
            if (_suprimirEventosArbolPerfiles) return;
            if (dgvFamilias.CurrentRow?.DataBoundItem is ItemGrillaRbacBE fam)
            {
                MostrarArbolFamilia(fam.Id);
            }
        };
        dgvPerfiles.SelectionChanged += (_, _) =>
        {
            if (_suprimirEventosArbolPerfiles) return;
            if (dgvPerfiles.CurrentRow?.DataBoundItem is ItemGrillaRbacBE perf)
            {
                MostrarArbolPerfil(perf.Id);
            }
        };

        tsMenuAddFamiliaAPerfil.Click += (_, _) => AccionAgregarFamiliaAPerfil();
        tsMenuAddPermisoAPerfil.Click += (_, _) => AccionAgregarPermisoBotonAPerfil();
        tsMenuAddPermisoAFamilia.Click += (_, _) => AccionAgregarPermisoBotonAFamilia();
        tsMenuAddFamilia.Click += (_, _) => AccionCrearNuevaFamilia();
        tsMenuAddPerfil.Click += (_, _) => AccionCrearNuevoPerfil();
        tsMenuAddPermiso.Click += (_, _) => AccionRegistrarPermisoBoton();

        tsMenuDelFamiliaDePerfil.Click += (_, _) => AccionDesvincularFamiliaDePerfil();
        tsMenuDelPermisoDePerfil.Click += (_, _) => AccionDesvincularPermisoBotonDePerfil();
        tsMenuDelPermisoDeFamilia.Click += (_, _) => AccionDesvincularPermisoBotonDeFamilia();
        tsMenuDelPerfil.Click += (_, _) => AccionEliminarPerfil();
        tsMenuDelFamilia.Click += (_, _) => AccionEliminarFamilia();
        tsMenuDelPermiso.Click += (_, _) => AccionEliminarPermisoBoton();

        tsBtnRelacionFamilia.Click += (_, _) => AccionRelacionFamiliaAFamilia();

        // --- 6. Eventos Dígito Verificador ---
        btnVerificarDv.Click += async (_, _) => await RecargarDatosUIAsync(mostrarMensajeIntegridad: true);
        btnRecalcularDv.Click += async (_, _) =>
        {
            var res = await _gestor.RecalcularDigitosVerificadoresAsync();
            MessageBox.Show(res.Mensaje, "Recalcular Dígito Verificador", MessageBoxButtons.OK, res.Ok ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
            await RecargarDatosUIAsync();
        };

        // --- 7. Eventos RestoreBackup & Supabase ---
        btnCrearBackup.Click += async (_, _) =>
        {
            using var sfd = new SaveFileDialog
            {
                Title = "Guardar Backup de la Base de Datos IMPORTA+",
                InitialDirectory = _gestor.Dal.BackupsFolderPath,
                Filter = "Backup IMPORTA+ (*.ipbak.json)|*.ipbak.json|JSON (*.json)|*.json",
                FileName = $"ImportaPlus_Backup_{DateTime.Now:yyyyMMdd_HHmmss}.ipbak.json"
            };
            if (sfd.ShowDialog(this) == DialogResult.OK)
            {
                var res = await _gestor.CrearBackupBaseActualAsync(sfd.FileName);
                if (!res.Ok)
                {
                    MessageBox.Show(res.Mensaje, "Backup", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                txtRutaRestore.Text = res.RutaArchivo;
                var insp = await _gestor.InspeccionarArchivoBackupAsync(res.RutaArchivo);
                lblPreviewBackup.Text = insp.Resumen;
                MessageBox.Show($"{res.Mensaje}\r\nRuta: {res.RutaArchivo}", "Backup Creado", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        };
        btnExaminarBk.Click += async (_, _) =>
        {
            using var ofd = new OpenFileDialog
            {
                Title = "Seleccionar Backup para Cargar en la Base Actual",
                InitialDirectory = _gestor.Dal.BackupsFolderPath,
                Filter = "Backup IMPORTA+ (*.ipbak.json;*.json)|*.ipbak.json;*.json"
            };
            if (ofd.ShowDialog(this) == DialogResult.OK)
            {
                txtRutaRestore.Text = ofd.FileName;
                var insp = await _gestor.InspeccionarArchivoBackupAsync(ofd.FileName);
                lblPreviewBackup.Text = insp.Resumen;
            }
        };
        btnEjecutarRestore.Click += async (_, _) =>
        {
            if (string.IsNullOrWhiteSpace(txtRutaRestore.Text))
            {
                MessageBox.Show("Primero seleccioná un archivo .ipbak.json con el botón 'Examinar...'.", "RestoreBackup", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (MessageBox.Show("¿Confirmás cargar este Backup sobre la base de datos actual?", "Confirmar RestoreBackup", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
                return;

            var res = await _gestor.RestaurarBaseDesdeBackupAsync(txtRutaRestore.Text);
            MessageBox.Show(res.Mensaje, "RestoreBackup", MessageBoxButtons.OK, res.Ok ? MessageBoxIcon.Information : MessageBoxIcon.Error);
            if (res.Ok) await RecargarDatosUIAsync();
        };

        var cfg = _gestor.Dal.Config;
        txtSupaUrl.Text = cfg.SupabaseUrl;
        txtSupaServiceKey.Text = cfg.ServiceRoleKey;
        txtSupaAnonKey.Text = cfg.AnonKey;
        ActualizarEtiquetaEstadoSupabaseInicial(cfg);

        btnGuardarSupa.Click += async (_, _) =>
        {
            _gestor.Dal.GuardarConfiguracion(new SupabaseConfigBE
            {
                SupabaseUrl = txtSupaUrl.Text,
                ServiceRoleKey = txtSupaServiceKey.Text,
                AnonKey = txtSupaAnonKey.Text,
                WebWorkspacePath = _gestor.Dal.Config.WebWorkspacePath
            });
            var prueba = await _gestor.Dal.ProbarConexionSupabaseAsync();
            lblResultadoSupa.Text = prueba.Mensaje;
            lblResultadoSupa.ForeColor = prueba.Ok ? TemaImportaPlus.EmeraldGreen : TemaImportaPlus.BrandPrimary;
            MessageBox.Show(prueba.Mensaje, "Conexión Supabase Cloud", MessageBoxButtons.OK, prueba.Ok ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
            await RecargarDatosUIAsync();
        };
        btnEmpujarNube.Click += async (_, _) =>
        {
            var guardado = await _gestor.Dal.GuardarBaseDatosCompletaAsync(_db, sincronizarNube: true, rolesBll: _gestor.Roles);
            lblResultadoSupa.Text = guardado.Detalle;
            lblResultadoSupa.ForeColor = guardado.Ok && _gestor.Dal.Config.EstaConfiguradoEnNube
                ? TemaImportaPlus.EmeraldGreen
                : TemaImportaPlus.BrandPrimary;
            MessageBox.Show(guardado.Detalle, "Sincronización Supabase", MessageBoxButtons.OK, guardado.Ok ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
        };

        Shown += async (_, _) =>
        {
            RecargarGrillasPerfiles();
            AplicarPermisosUI();
            MostrarSeccion(_seccionInicialSolicitada);
            await RecargarDatosUIAsync();
        };
    }

    private void ActualizarEtiquetaEstadoSupabaseInicial(SupabaseConfigBE cfg)
    {
        lblResultadoSupa.Text = $"✔ Conectado a Supabase Cloud ({cfg.SupabaseUrl}). Cualquier usuario autenticado con permiso aplica los cambios directamente en la nube sin requerir Secret Key.";
        lblResultadoSupa.ForeColor = TemaImportaPlus.EmeraldGreen;
    }

    // =========================================================================
    // MOTOR DINÁMICO DE PERMISOS POR BOTÓN (1 PERMISO = 1 BOTÓN)
    // =========================================================================

    private Dictionary<string, AppleButton> ObtenerMapaBotonesGestionados() => new(StringComparer.OrdinalIgnoreCase)
    {
        ["btnNavForwarders"] = btnNavForwarders,
        ["btnNavDespachantes"] = btnNavDespachantes,
        ["btnNavPuertos"] = btnNavPuertos,
        ["btnNavUsuarios"] = btnNavUsuarios,
        ["btnNavRoles"] = btnNavRoles,
        ["btnNavDigitoVerificador"] = btnNavDigitoVerificador,
        ["btnNavRestoreSupabase"] = btnNavRestoreSupabase,
        ["btnCambiarPassword"] = btnCambiarPassword,
        ["btnGuardarFwd"] = btnGuardarFwd,
        ["btnNuevoFwd"] = btnNuevoFwd,
        ["btnEliminarFwd"] = btnEliminarFwd,
        ["btnSubirFoto"] = btnSubirFoto,
        ["btnGuardarDesp"] = btnGuardarDesp,
        ["btnNuevoDesp"] = btnNuevoDesp,
        ["btnEliminarDesp"] = btnEliminarDesp,
        ["btnSubirFotoDesp"] = btnSubirFotoDesp,
        ["btnGuardarPto"] = btnGuardarPto,
        ["btnNuevoPto"] = btnNuevoPto,
        ["btnBorrarPto"] = btnBorrarPto,
        ["btnCrearUsuario"] = btnCrearUsuario,
        ["btnModificarUsuario"] = btnModificarUsuario,
        ["btnDesbloquearUsuario"] = btnDesbloquearUsuario,
        ["btnActDesactUsuario"] = btnActDesactUsuario,
        ["btnResetPassUsuario"] = btnResetPassUsuario,
        ["btnVerificarDv"] = btnVerificarDv,
        ["btnRecalcularDv"] = btnRecalcularDv,
        ["btnCrearBackup"] = btnCrearBackup,
        ["btnEjecutarRestore"] = btnEjecutarRestore,
        ["btnGuardarSupa"] = btnGuardarSupa,
        ["btnEmpujarNube"] = btnEmpujarNube
    };

    private void AplicarPermisosUI()
    {
        bool modoRescateAdmin = _seccionInicialSolicitada == SeccionRestoreSupabase && _gestor.EsAdministradorActual();

        foreach (var (nombreBoton, boton) in ObtenerMapaBotonesGestionados())
        {
            bool tieneBoton = _gestor.AdminActual == null || _gestor.UsuarioActualTieneBoton(nombreBoton);

            if (modoRescateAdmin &&
                (nombreBoton is "btnNavRestoreSupabase" or "btnEjecutarRestore" or "btnNavDigitoVerificador" or "btnRecalcularDv"))
            {
                tieneBoton = true;
            }

            boton.Visible = tieneBoton;
            boton.Enabled = tieneBoton;
        }

        btnExaminarBk.Visible = btnEjecutarRestore.Visible;
        btnExaminarBk.Enabled = btnEjecutarRestore.Enabled;

        // Si el usuario estaba en modo edición de usuario, respetar el bloqueo temporal de botones laterales
        if (!string.IsNullOrEmpty(_usuModoActual))
        {
            SincronizarEstadoBotonesModoUsuario();
        }

        ReorganizarBotonesSidebar();
        ActualizarCabeceraUsuarioYPerfil();

        // Si la sección actualmente abierta perdió visibilidad de su botón de menú, saltar a la primera permitida
        AppleButton[] navBotones = ObtenerBotonesNavegacion();
        if (_seccionActual >= 0 && _seccionActual < navBotones.Length && !navBotones[_seccionActual].Visible)
        {
            MostrarSeccion(_seccionActual);
        }
    }

    private AppleButton[] ObtenerBotonesNavegacion() => new[]
    {
        btnNavForwarders,
        btnNavDespachantes,
        btnNavPuertos,
        btnNavUsuarios,
        btnNavRoles,
        btnNavDigitoVerificador,
        btnNavRestoreSupabase
    };

    private Panel[] ObtenerPanelesSecciones() => new[]
    {
        pnlSecForwarders,
        pnlSecDespachantes,
        pnlSecPuertos,
        pnlSecUsuarios,
        pnlSecRoles,
        pnlSecDigitoVerificador,
        pnlSecRestoreSupabase
    };

    private void ReorganizarBotonesSidebar()
    {
        int currentY = 20;
        const int espaciado = 8;

        foreach (var btn in ObtenerBotonesNavegacion())
        {
            if (!btn.Visible) continue;
            btn.Location = new Point(16, currentY);
            currentY += btn.Height + espaciado;
        }
    }

    private void MostrarSeccion(int indice)
    {
        Panel[] paneles = ObtenerPanelesSecciones();
        AppleButton[] botones = ObtenerBotonesNavegacion();

        if (indice < 0 || indice >= paneles.Length) indice = 0;

        if (!botones[indice].Visible)
        {
            int primeroVisible = Array.FindIndex(botones, b => b.Visible);
            if (primeroVisible < 0)
            {
                foreach (var p in paneles) p.Visible = false;
                return;
            }
            indice = primeroVisible;
        }

        _seccionActual = indice;

        for (int i = 0; i < paneles.Length; i++)
        {
            bool activo = i == indice;
            paneles[i].Visible = activo;
            if (activo) paneles[i].BringToFront();
            botones[i].BackColor = activo ? Color.FromArgb(115, 16, 40) : TemaImportaPlus.HeaderBurgundy;
            botones[i].ForeColor = activo ? Color.White : Color.FromArgb(245, 215, 222);
        }

        if (indice == SeccionUsuarios)
        {
            CargarComboPerfilesUsuario();
            RefrescarGrillaUsuarios();
        }
        else if (indice == SeccionPerfiles)
        {
            RecargarGrillasPerfiles();
        }
    }

    private void ActualizarCabeceraUsuarioYPerfil()
    {
        string user = _gestor.AdminActual?.Username ?? "Admin";
        var perfil = _gestor.Roles.ObtenerPerfilDeUsuario(user, _gestor.AdminActual?.PerfilId);
        string nombrePerfil = perfil?.Nombre ?? "Sin Perfil";
        lblUsuarioActual.Text = $"Usuario: {user}  ·  Perfil: {nombrePerfil}";
        lblBadgeEstadoHeader.Location = new Point(Math.Max(290, lblUsuarioActual.Right + 24), 21);
    }

    // =========================================================================
    // RECARGA GENERAL DE DATOS Y AUDITORÍA DVH / DVV
    // =========================================================================

    private async Task RecargarDatosUIAsync(bool mostrarMensajeIntegridad = false)
    {
        _db = await _gestor.Dal.ObtenerBaseDatosActualAsync(_gestor.Roles);
        if (_gestor.AdminActual != null)
        {
            var actualizado = _db.AdminUsuarios.FirstOrDefault(u => u.Id == _gestor.AdminActual.Id);
            if (actualizado != null)
            {
                _gestor.AdminActual.PerfilId = actualizado.PerfilId;
            }
        }

        var verif = SeguridadYDigitoVerificadorBLL.VerificarBaseCompleta(_db);

        ActualizarCabeceraUsuarioYPerfil();
        string modo = _gestor.Dal.ConectadoDirectoNube ? "Supabase Cloud" : "Base Local + Espejo Web";
        lblBadgeEstadoHeader.Text = verif.SaltoDetectado
            ? $"● {modo}  ·  ⚠ Alerta DVH/DVV ({verif.Alteraciones.Count} cambio/s)"
            : $"● {modo}  ·  ✔ Integridad DVH/DVV Verificada";
        lblBadgeEstadoHeader.ForeColor = verif.SaltoDetectado ? TemaImportaPlus.BrandPrimary : TemaImportaPlus.EmeraldGreen;

        dgvForwarders.DataSource = _db.Forwarders.OrderBy(f => f.Orden).ToList();
        OcultarColumnasTecnicas(dgvForwarders, "Id", "Dvh", "ImagenUrl");

        dgvDespachantes.DataSource = _db.Despachantes.OrderBy(d => d.Orden).ToList();
        OcultarColumnasTecnicas(dgvDespachantes, "Id", "Dvh", "ImagenUrl");

        dgvPuertos.DataSource = _db.PuertosAduanas.OrderBy(p => p.Nombre).ToList();
        OcultarColumnasTecnicas(dgvPuertos, "Id", "Dvh", "Infraestructura");

        CargarComboPerfilesUsuario();
        RefrescarGrillaUsuarios();

        dgvDvv.DataSource = _db.DigitosVerificadores
            .OrderBy(d => d.TablaNombre)
            .Select(d => new
            {
                Tabla = d.TablaNombre,
                TotalRegistros = d.TablaNombre == "ncm_catalogo" && _db.TotalNcmEnNube.HasValue
                    ? _db.TotalNcmEnNube.Value
                    : (d.TablaNombre == "paises" && _db.TotalPaisesEnNube.HasValue ? _db.TotalPaisesEnNube.Value : d.CantidadRegistros),
                DvvRegistrado = d.Dvv.Length > 20 ? d.Dvv[..20] + "..." : d.Dvv,
                ActualizadoPor = d.UpdatedBy,
                UltimaActualizacion = d.UpdatedAt.LocalDateTime.ToString("dd/MM/yyyy HH:mm:ss")
            })
            .ToList();

        lblDetalleIntegridadPanel.Text = verif.ObtenerTextoLabelCambios();
        lblDetalleIntegridadPanel.ForeColor = verif.SaltoDetectado ? TemaImportaPlus.BrandPrimary : TemaImportaPlus.EmeraldGreen;

        if (string.IsNullOrWhiteSpace(txtRutaRestore.Text))
        {
            var primerBk = Directory.Exists(_gestor.Dal.BackupsFolderPath)
                ? Directory.GetFiles(_gestor.Dal.BackupsFolderPath, "*.ipbak.json").OrderByDescending(f => f).FirstOrDefault()
                : null;
            if (primerBk != null)
            {
                txtRutaRestore.Text = primerBk;
                var insp = await _gestor.InspeccionarArchivoBackupAsync(primerBk);
                lblPreviewBackup.Text = insp.Resumen;
            }
        }

        AplicarPermisosUI();

        if (mostrarMensajeIntegridad)
        {
            MessageBox.Show(
                verif.ObtenerTextoLabelCambios(),
                verif.SaltoDetectado ? "Alerta Dígito Verificador" : "Integridad Verificada",
                MessageBoxButtons.OK,
                verif.SaltoDetectado ? MessageBoxIcon.Warning : MessageBoxIcon.Information);
        }
    }

    private static void OcultarColumnasTecnicas(DataGridView dgv, params string[] columnas)
    {
        foreach (string col in columnas)
        {
            if (dgv.Columns.Contains(col) && dgv.Columns[col] is DataGridViewColumn c)
                c.Visible = false;
        }
    }

    // =========================================================================
    // SECCIÓN 0: ABM FORWARDERS
    // =========================================================================

    private void CargarForwarderSeleccionadoEnEditor()
    {
        if (dgvForwarders.CurrentRow?.DataBoundItem is not ForwarderBE f) return;
        _fwdEditId = f.Id;
        _fwdRutaImagenLocalSeleccionada = null;
        txtFwdNombre.Text = f.Nombre;
        txtFwdImagenUrl.Text = f.ImagenUrl;
        txtFwdWhatsapp.Text = f.Whatsapp;
        txtFwdEmail.Text = f.Email;
        nudFwdOrden.Value = Math.Clamp(f.Orden, 1, 999);
        chkFwdActivo.Checked = f.Activo;
        _ = PrevisualizarImagenEnPictureBoxAsync(f.ImagenUrl, picForwarder);
    }

    private void LimpiarEditorForwarder()
    {
        _fwdEditId = Guid.NewGuid();
        _fwdRutaImagenLocalSeleccionada = null;
        txtFwdNombre.Clear();
        txtFwdImagenUrl.Clear();
        txtFwdWhatsapp.Clear();
        txtFwdEmail.Clear();
        nudFwdOrden.Value = Math.Clamp(AdminGestorBLL.ObtenerSiguienteOrdenDisponible(_db), 1, 999);
        chkFwdActivo.Checked = true;
        picForwarder.Image = null;
    }

    private void SeleccionarImagenForwarderLocal()
    {
        using var ofd = new OpenFileDialog
        {
            Title = "Seleccionar Foto del Forwarder",
            Filter = "Imágenes (*.jpg;*.jpeg;*.png;*.webp;*.bmp)|*.jpg;*.jpeg;*.png;*.webp;*.bmp"
        };
        if (ofd.ShowDialog(this) == DialogResult.OK)
        {
            _fwdRutaImagenLocalSeleccionada = ofd.FileName;
            txtFwdImagenUrl.Text = ofd.FileName;
            try
            {
                using var temp = Image.FromFile(ofd.FileName);
                picForwarder.Image = new Bitmap(temp);
            }
            catch { }
        }
    }

    private async Task GuardarForwarderDesdeUIAsync()
    {
        var item = new ForwarderBE
        {
            Id = _fwdEditId,
            Nombre = txtFwdNombre.Text,
            ImagenUrl = txtFwdImagenUrl.Text,
            Whatsapp = txtFwdWhatsapp.Text,
            Email = txtFwdEmail.Text,
            Orden = (int)nudFwdOrden.Value,
            Activo = chkFwdActivo.Checked
        };

        var res = await _gestor.GuardarForwarderAsync(item, _fwdRutaImagenLocalSeleccionada);
        MessageBox.Show(res.Mensaje, "ABM Forwarders", MessageBoxButtons.OK, res.Ok ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
        if (res.Ok)
        {
            _fwdRutaImagenLocalSeleccionada = null;
            await RecargarDatosUIAsync();
        }
    }

    private async Task EliminarForwarderDesdeUIAsync()
    {
        if (dgvForwarders.CurrentRow?.DataBoundItem is not ForwarderBE f) return;
        if (MessageBox.Show($"¿Confirmás eliminar al Forwarder '{f.Nombre}'?", "Confirmar Baja", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            return;

        var res = await _gestor.EliminarForwarderAsync(f.Id);
        MessageBox.Show(res.Mensaje, "ABM Forwarders", MessageBoxButtons.OK, res.Ok ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
        if (res.Ok)
        {
            LimpiarEditorForwarder();
            await RecargarDatosUIAsync();
        }
    }

    // =========================================================================
    // SECCIÓN 1: ABM DESPACHANTES DE ADUANA
    // =========================================================================

    private void CargarDespachanteSeleccionadoEnEditor()
    {
        if (dgvDespachantes.CurrentRow?.DataBoundItem is not DespachanteBE d) return;
        _despEditId = d.Id;
        _despRutaImagenLocalSeleccionada = null;
        txtDespNombre.Text = d.Nombre;
        txtDespImagenUrl.Text = d.ImagenUrl;
        txtDespWhatsapp.Text = d.Whatsapp;
        txtDespEmail.Text = d.Email;
        nudDespOrden.Value = Math.Clamp(d.Orden, 1, 999);
        chkDespActivo.Checked = d.Activo;
        _ = PrevisualizarImagenEnPictureBoxAsync(d.ImagenUrl, picDespachante);
    }

    private void LimpiarEditorDespachante()
    {
        _despEditId = Guid.NewGuid();
        _despRutaImagenLocalSeleccionada = null;
        txtDespNombre.Clear();
        txtDespImagenUrl.Clear();
        txtDespWhatsapp.Clear();
        txtDespEmail.Clear();
        nudDespOrden.Value = Math.Clamp(AdminGestorBLL.ObtenerSiguienteOrdenDespachante(_db), 1, 999);
        chkDespActivo.Checked = true;
        picDespachante.Image = null;
    }

    private void SeleccionarImagenDespachanteLocal()
    {
        using var ofd = new OpenFileDialog
        {
            Title = "Seleccionar Foto del Despachante de Aduana",
            Filter = "Imágenes (*.jpg;*.jpeg;*.png;*.webp;*.bmp)|*.jpg;*.jpeg;*.png;*.webp;*.bmp"
        };
        if (ofd.ShowDialog(this) == DialogResult.OK)
        {
            _despRutaImagenLocalSeleccionada = ofd.FileName;
            txtDespImagenUrl.Text = ofd.FileName;
            try
            {
                using var temp = Image.FromFile(ofd.FileName);
                picDespachante.Image = new Bitmap(temp);
            }
            catch { }
        }
    }

    private async Task GuardarDespachanteDesdeUIAsync()
    {
        var item = new DespachanteBE
        {
            Id = _despEditId,
            Nombre = txtDespNombre.Text,
            ImagenUrl = txtDespImagenUrl.Text,
            Whatsapp = txtDespWhatsapp.Text,
            Email = txtDespEmail.Text,
            Orden = (int)nudDespOrden.Value,
            Activo = chkDespActivo.Checked
        };

        var res = await _gestor.GuardarDespachanteAsync(item, _despRutaImagenLocalSeleccionada);
        MessageBox.Show(res.Mensaje, "ABM Despachantes de Aduana", MessageBoxButtons.OK, res.Ok ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
        if (res.Ok)
        {
            _despRutaImagenLocalSeleccionada = null;
            await RecargarDatosUIAsync();
        }
    }

    private async Task EliminarDespachanteDesdeUIAsync()
    {
        if (dgvDespachantes.CurrentRow?.DataBoundItem is not DespachanteBE d) return;
        if (MessageBox.Show($"¿Confirmás eliminar al Despachante de Aduana '{d.Nombre}'?", "Confirmar Baja", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            return;

        var res = await _gestor.EliminarDespachanteAsync(d.Id);
        MessageBox.Show(res.Mensaje, "ABM Despachantes de Aduana", MessageBoxButtons.OK, res.Ok ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
        if (res.Ok)
        {
            LimpiarEditorDespachante();
            await RecargarDatosUIAsync();
        }
    }

    private static async Task PrevisualizarImagenEnPictureBoxAsync(string? urlOrData, PictureBox destino)
    {
        if (string.IsNullOrWhiteSpace(urlOrData))
        {
            destino.Image = null;
            return;
        }

        try
        {
            if (urlOrData.StartsWith("data:image/", StringComparison.OrdinalIgnoreCase))
            {
                int comma = urlOrData.IndexOf(',');
                if (comma > 0)
                {
                    byte[] bytes = Convert.FromBase64String(urlOrData[(comma + 1)..]);
                    using var ms = new MemoryStream(bytes);
                    destino.Image = new Bitmap(ms);
                }
            }
            else if (File.Exists(urlOrData))
            {
                using var img = Image.FromFile(urlOrData);
                destino.Image = new Bitmap(img);
            }
            else if (SeguridadYDigitoVerificadorBLL.EsUrlImagenSegura(urlOrData))
            {
                const int maxBytes = 2 * 1024 * 1024;
                using var resp = await ImgHttp.GetAsync(urlOrData, HttpCompletionOption.ResponseHeadersRead);
                resp.EnsureSuccessStatusCode();
                if (resp.Content.Headers.ContentLength.HasValue && resp.Content.Headers.ContentLength.Value > maxBytes)
                {
                    destino.Image = null;
                    return;
                }

                await using var netStream = await resp.Content.ReadAsStreamAsync();
                using var ms = new MemoryStream();
                byte[] buffer = new byte[8192];
                int totalRead = 0;
                int read;
                while ((read = await netStream.ReadAsync(buffer)) > 0)
                {
                    totalRead += read;
                    if (totalRead > maxBytes)
                    {
                        destino.Image = null;
                        return;
                    }
                    ms.Write(buffer, 0, read);
                }

                ms.Position = 0;
                destino.Image = new Bitmap(ms);
            }
        }
        catch
        {
            destino.Image = null;
        }
    }

    // =========================================================================
    // SECCIÓN 3: GESTIÓN DE USUARIOS (COPIADO Y ADAPTADO DE ING-SOFTWARE-BritezG)
    // =========================================================================

    private void CargarComboPerfilesUsuario()
    {
        Guid? seleccionadoPrevio = (cmbUsuRol.SelectedItem as PerfilBE)?.Id;
        var perfiles = _gestor.Roles.Store.Perfiles.OrderBy(p => p.Nombre).ToList();

        cmbUsuRol.DataSource = null;
        cmbUsuRol.DisplayMember = nameof(PerfilBE.Nombre);
        cmbUsuRol.ValueMember = nameof(PerfilBE.Id);
        cmbUsuRol.DataSource = perfiles;

        if (seleccionadoPrevio.HasValue)
        {
            int idx = perfiles.FindIndex(p => p.Id == seleccionadoPrevio.Value);
            cmbUsuRol.SelectedIndex = idx;
        }
        else if (string.IsNullOrEmpty(_usuModoActual))
        {
            CargarUsuarioSeleccionadoEnFicha();
        }
    }

    private void RefrescarGrillaUsuarios()
    {
        IEnumerable<AdminUsuarioBE> consulta = _db.AdminUsuarios.OrderBy(u => u.Username);
        if (rbMostrarActivos.Checked)
            consulta = consulta.Where(u => u.Activo);
        else if (rbMostrarInactivos.Checked)
            consulta = consulta.Where(u => !u.Activo);

        var filas = consulta.Select(u =>
        {
            var perf = _gestor.Roles.ObtenerPerfilDeUsuario(u.Username, u.PerfilId);
            return new UsuarioFilaVista
            {
                Id = u.Id,
                Dni = u.Dni,
                Nombre = u.Nombre,
                Apellido = u.Apellido,
                Username = u.Username,
                Perfil = perf?.Nombre ?? "Sin Perfil",
                Bloqueado = u.Bloqueado ? "Bloqueado" : "No",
                Estado = u.Activo ? "Activo" : "Inactivo",
                ActivoRaw = u.Activo
            };
        }).ToList();

        dgvUsuarios.DataSource = null;
        dgvUsuarios.DataSource = filas;
        OcultarColumnasTecnicas(dgvUsuarios, nameof(UsuarioFilaVista.Id), nameof(UsuarioFilaVista.ActivoRaw));

        if (dgvUsuarios.Columns.Contains(nameof(UsuarioFilaVista.Dni)))
            dgvUsuarios.Columns[nameof(UsuarioFilaVista.Dni)]!.HeaderText = "DNI";
        if (dgvUsuarios.Columns.Contains(nameof(UsuarioFilaVista.Username)))
            dgvUsuarios.Columns[nameof(UsuarioFilaVista.Username)]!.HeaderText = "Nombre de Usuario";
        if (dgvUsuarios.Columns.Contains(nameof(UsuarioFilaVista.Perfil)))
            dgvUsuarios.Columns[nameof(UsuarioFilaVista.Perfil)]!.HeaderText = "Perfil (Rol)";
    }

    private AdminUsuarioBE? ObtenerUsuarioSeleccionadoEnGrilla()
    {
        if (dgvUsuarios.CurrentRow?.DataBoundItem is not UsuarioFilaVista fila) return null;
        return _db.AdminUsuarios.FirstOrDefault(u => u.Id == fila.Id);
    }

    private void CargarUsuarioSeleccionadoEnFicha()
    {
        if (!string.IsNullOrEmpty(_usuModoActual)) return;
        var u = ObtenerUsuarioSeleccionadoEnGrilla();
        if (u == null) return;

        txtUsuDni.Text = u.Dni.ToString(CultureInfo.InvariantCulture);
        txtUsuNombre.Text = u.Nombre;
        txtUsuApellido.Text = u.Apellido;
        txtUsuUsername.Text = u.Username;
        chkUsuActivar.Checked = u.Activo;
        chkUsuDesactivar.Checked = !u.Activo;

        var perf = _gestor.Roles.ObtenerPerfilDeUsuario(u.Username, u.PerfilId);
        if (perf != null && cmbUsuRol.DataSource is List<PerfilBE> lista)
        {
            int idx = lista.FindIndex(p => p.Id == perf.Id);
            cmbUsuRol.SelectedIndex = idx;
        }
        else
        {
            cmbUsuRol.SelectedIndex = -1;
        }
    }

    private void HabilitarModoCrearUsuario()
    {
        _usuModoActual = "Crear";
        _usuIdEnEdicion = null;
        dgvUsuarios.ClearSelection();

        txtUsuDni.Clear();
        txtUsuNombre.Clear();
        txtUsuApellido.Clear();
        txtUsuUsername.Clear();
        cmbUsuRol.SelectedIndex = cmbUsuRol.Items.Count > 0 ? 0 : -1;
        chkUsuActivar.Checked = true;
        chkUsuDesactivar.Checked = false;

        lblUsuModoTitulo.Text = "Modo CREAR USUARIO · Contraseña inicial automática = Nombre + DNI";
        SincronizarEstadoBotonesModoUsuario();
        txtUsuDni.Focus();
    }

    private void HabilitarModoModificarUsuario()
    {
        var u = ObtenerUsuarioSeleccionadoEnGrilla();
        if (u == null)
        {
            MessageBox.Show("Seleccioná una fila en la tabla de usuarios para Modificar.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        CargarUsuarioSeleccionadoEnFicha();
        _usuModoActual = "Modificar";
        _usuIdEnEdicion = u.Id;
        lblUsuModoTitulo.Text = $"Modo MODIFICAR USUARIO · Editando '{u.Username}'";
        SincronizarEstadoBotonesModoUsuario();
        txtUsuNombre.Focus();
    }

    private void SincronizarEstadoBotonesModoUsuario()
    {
        bool editando = !string.IsNullOrEmpty(_usuModoActual);
        bool esCrear = _usuModoActual == "Crear";

        txtUsuDni.Enabled = editando && esCrear;
        txtUsuNombre.Enabled = editando;
        txtUsuApellido.Enabled = editando;
        txtUsuUsername.Enabled = editando;
        cmbUsuRol.Enabled = editando;
        chkUsuActivar.Enabled = false;
        chkUsuDesactivar.Enabled = false;

        cmdAceptarUsuario.Visible = editando;
        cmdAceptarUsuario.Enabled = editando;
        cmdCancelarUsuario.Visible = editando;
        cmdCancelarUsuario.Enabled = editando;

        btnCrearUsuario.Enabled = !editando && btnCrearUsuario.Visible;
        btnModificarUsuario.Enabled = !editando && btnModificarUsuario.Visible;
        btnDesbloquearUsuario.Enabled = !editando && btnDesbloquearUsuario.Visible;
        btnActDesactUsuario.Enabled = !editando && btnActDesactUsuario.Visible;
        btnResetPassUsuario.Enabled = !editando && btnResetPassUsuario.Visible;
    }

    private void CancelarOperacionUsuario()
    {
        _usuModoActual = string.Empty;
        _usuIdEnEdicion = null;
        lblUsuModoTitulo.Text = "Ficha del Usuario (Seleccioná 'Crear' o 'Modificar' a la izquierda)";
        SincronizarEstadoBotonesModoUsuario();
        CargarUsuarioSeleccionadoEnFicha();
    }

    private async Task ConfirmarOperacionUsuarioAsync()
    {
        if (cmbUsuRol.SelectedItem is not PerfilBE perfilElegido)
        {
            MessageBox.Show("Seleccioná un Perfil (Rol) válido para el usuario.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (_usuModoActual == "Crear")
        {
            if (!int.TryParse(txtUsuDni.Text.Trim(), out int dni))
            {
                MessageBox.Show("Ingresá un DNI numérico válido de 8 dígitos.", "DNI Inválido", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtUsuDni.Focus();
                return;
            }

            var res = await _gestor.CrearUsuarioAsync(
                dni,
                txtUsuNombre.Text,
                txtUsuApellido.Text,
                txtUsuUsername.Text,
                perfilElegido.Id);

            MessageBox.Show(res.Mensaje, "Crear Usuario", MessageBoxButtons.OK, res.Ok ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
            if (res.Ok)
            {
                CancelarOperacionUsuario();
                await RecargarDatosUIAsync();
            }
        }
        else if (_usuModoActual == "Modificar" && _usuIdEnEdicion.HasValue)
        {
            var res = await _gestor.ModificarUsuarioAsync(
                _usuIdEnEdicion.Value,
                txtUsuNombre.Text,
                txtUsuApellido.Text,
                txtUsuUsername.Text,
                perfilElegido.Id);

            MessageBox.Show(res.Mensaje, "Modificar Usuario", MessageBoxButtons.OK, res.Ok ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
            if (res.Ok)
            {
                CancelarOperacionUsuario();
                await RecargarDatosUIAsync();
            }
        }
    }

    private async Task DesbloquearUsuarioSeleccionadoAsync()
    {
        var u = ObtenerUsuarioSeleccionadoEnGrilla();
        if (u == null)
        {
            MessageBox.Show("Seleccioná un usuario en la tabla para desbloquear.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var res = await _gestor.DesbloquearUsuarioAsync(u.Id);
        MessageBox.Show(res.Mensaje, "Desbloquear Usuario", MessageBoxButtons.OK, res.Ok ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
        if (res.Ok) await RecargarDatosUIAsync();
    }

    private async Task CambiarEstadoUsuarioSeleccionadoAsync()
    {
        var u = ObtenerUsuarioSeleccionadoEnGrilla();
        if (u == null)
        {
            MessageBox.Show("Seleccioná un usuario en la tabla para Activar / Desactivar.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        string accion = u.Activo ? "DESACTIVAR" : "ACTIVAR";
        if (MessageBox.Show($"¿Confirmás {accion} la cuenta del usuario '{u.Username}'?", "Confirmar Estado", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            return;

        var res = await _gestor.CambiarEstadoUsuarioAsync(u.Id);
        MessageBox.Show(res.Mensaje, "Estado de Usuario", MessageBoxButtons.OK, res.Ok ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
        if (res.Ok) await RecargarDatosUIAsync();
    }

    private async Task CambiarClaveUsuarioSeleccionadoAsync()
    {
        var u = ObtenerUsuarioSeleccionadoEnGrilla();
        if (u == null)
        {
            MessageBox.Show("Seleccioná un usuario en la tabla para cambiar su contraseña.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        string? nuevaPass = SolicitarNuevaClaveModal(u.Username);
        if (string.IsNullOrWhiteSpace(nuevaPass)) return;

        var res = await _gestor.CambiarClaveUsuarioDesdeGestionAsync(u.Id, nuevaPass);
        MessageBox.Show(res.Mensaje, "Cambiar Clave", MessageBoxButtons.OK, res.Ok ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
        if (res.Ok) await RecargarDatosUIAsync();
    }

    private string? SolicitarNuevaClaveModal(string username)
    {
        using var dlg = new Form
        {
            Text = $"Cambiar Contraseña · {username}",
            StartPosition = FormStartPosition.CenterParent,
            FormBorderStyle = FormBorderStyle.FixedDialog,
            MaximizeBox = false,
            MinimizeBox = false,
            BackColor = TemaImportaPlus.SurfaceBg,
            ClientSize = new Size(390, 190),
            Font = TemaImportaPlus.FontNormal
        };

        var lbl = new Label
        {
            AutoSize = true,
            Font = TemaImportaPlus.FontEtiqueta,
            ForeColor = TemaImportaPlus.TextBody,
            Location = new Point(20, 22),
            Text = $"NUEVA CONTRASEÑA PARA '{username.ToUpperInvariant()}' (MÍN. 4 CARACTERES):"
        };

        var txt = new TextBox
        {
            Location = new Point(20, 48),
            Size = new Size(350, 26),
            UseSystemPasswordChar = true
        };

        var chkVer = new CheckBox
        {
            AutoSize = true,
            Location = new Point(20, 82),
            Text = "Mostrar caracteres"
        };
        chkVer.CheckedChanged += (_, _) => txt.UseSystemPasswordChar = !chkVer.Checked;

        var btnOk = new AppleButton
        {
            BackColor = TemaImportaPlus.BrandPrimary,
            CornerRadius = 10,
            ForeColor = Color.White,
            Location = new Point(260, 126),
            Size = new Size(110, 36),
            Text = "Guardar"
        };
        btnOk.Click += (_, _) =>
        {
            if (!SeguridadYDigitoVerificadorBLL.ValidarFortalezaPassword(txt.Text, out string error))
            {
                MessageBox.Show(error, "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            dlg.DialogResult = DialogResult.OK;
            dlg.Close();
        };

        var btnCancel = new AppleButton
        {
            BackColor = Color.White,
            BorderColor = TemaImportaPlus.BorderNeutral,
            BorderThickness = 1,
            CornerRadius = 10,
            ForeColor = TemaImportaPlus.TextDark,
            Location = new Point(142, 126),
            Size = new Size(108, 36),
            Text = "Cancelar"
        };
        btnCancel.Click += (_, _) => { dlg.DialogResult = DialogResult.Cancel; dlg.Close(); };

        dlg.Controls.Add(lbl);
        dlg.Controls.Add(txt);
        dlg.Controls.Add(chkVer);
        dlg.Controls.Add(btnCancel);
        dlg.Controls.Add(btnOk);
        dlg.AcceptButton = btnOk;
        dlg.CancelButton = btnCancel;

        return dlg.ShowDialog(this) == DialogResult.OK ? txt.Text : null;
    }

    // =========================================================================
    // SECCIÓN 4: GESTIÓN DE PERFILES Y BOTONES (COPIADO DE ING-SOFTWARE-BritezG)
    // =========================================================================

    private void RecargarGrillasPerfiles(Guid? seleccionarPerfilId = null, Guid? seleccionarFamiliaId = null)
    {
        Guid? perfilPrevio = seleccionarPerfilId ?? (dgvPerfiles.CurrentRow?.DataBoundItem as ItemGrillaRbacBE)?.Id;
        Guid? familiaPrevia = seleccionarFamiliaId ?? (dgvFamilias.CurrentRow?.DataBoundItem as ItemGrillaRbacBE)?.Id;
        Guid? permisoPrevio = (dgvPermisos.CurrentRow?.DataBoundItem as ItemGrillaRbacBE)?.Id;

        _suprimirEventosArbolPerfiles = true;
        try
        {
            var listaPermisos = _gestor.Roles.ObtenerPermisosBotonesGrilla();
            dgvPermisos.DataSource = null;
            dgvPermisos.DataSource = listaPermisos;
            OcultarColumnasTecnicas(dgvPermisos, nameof(ItemGrillaRbacBE.Id), nameof(ItemGrillaRbacBE.Codigo));
            if (dgvPermisos.Columns.Contains(nameof(ItemGrillaRbacBE.Nombre)))
                dgvPermisos.Columns[nameof(ItemGrillaRbacBE.Nombre)]!.HeaderText = "Botón Habilitable";

            var listaFamilias = _gestor.Roles.ObtenerFamiliasGrilla();
            dgvFamilias.DataSource = null;
            dgvFamilias.DataSource = listaFamilias;
            OcultarColumnasTecnicas(dgvFamilias, nameof(ItemGrillaRbacBE.Id), nameof(ItemGrillaRbacBE.Codigo));
            if (dgvFamilias.Columns.Contains(nameof(ItemGrillaRbacBE.Nombre)))
                dgvFamilias.Columns[nameof(ItemGrillaRbacBE.Nombre)]!.HeaderText = "Familia de Botones";

            var listaPerfiles = _gestor.Roles.ObtenerPerfilesGrilla();
            dgvPerfiles.DataSource = null;
            dgvPerfiles.DataSource = listaPerfiles;
            OcultarColumnasTecnicas(dgvPerfiles, nameof(ItemGrillaRbacBE.Id), nameof(ItemGrillaRbacBE.Codigo));
            if (dgvPerfiles.Columns.Contains(nameof(ItemGrillaRbacBE.Nombre)))
                dgvPerfiles.Columns[nameof(ItemGrillaRbacBE.Nombre)]!.HeaderText = "Perfil (Rol)";

            RestaurarSeleccionGrillaRbac(dgvPermisos, permisoPrevio);
            RestaurarSeleccionGrillaRbac(dgvFamilias, familiaPrevia);
            RestaurarSeleccionGrillaRbac(dgvPerfiles, perfilPrevio);
        }
        finally
        {
            _suprimirEventosArbolPerfiles = false;
        }

        if (seleccionarFamiliaId.HasValue)
        {
            MostrarArbolFamilia(seleccionarFamiliaId.Value);
        }
        else if (dgvPerfiles.CurrentRow?.DataBoundItem is ItemGrillaRbacBE perfActual)
        {
            MostrarArbolPerfil(perfActual.Id);
        }
        else if (dgvFamilias.CurrentRow?.DataBoundItem is ItemGrillaRbacBE famActual)
        {
            MostrarArbolFamilia(famActual.Id);
        }
        else
        {
            Vista_Familia.Nodes.Clear();
        }

        CargarComboPerfilesUsuario();
        RefrescarGrillaUsuarios();
        AplicarPermisosUI();
    }

    private static void RestaurarSeleccionGrillaRbac(DataGridView dgv, Guid? idObjetivo)
    {
        if (!idObjetivo.HasValue || dgv.Rows.Count == 0) return;
        foreach (DataGridViewRow row in dgv.Rows)
        {
            if (row.DataBoundItem is ItemGrillaRbacBE item && item.Id == idObjetivo.Value)
            {
                dgv.ClearSelection();
                row.Selected = true;
                if (dgv.Columns.GetFirstColumn(DataGridViewElementStates.Visible) is DataGridViewColumn col)
                {
                    dgv.CurrentCell = row.Cells[col.Index];
                }
                break;
            }
        }
    }

    private void MostrarArbolPerfil(Guid idPerfil)
    {
        Vista_Familia.Nodes.Clear();
        Vista_Familia.Nodes.Add(_gestor.Roles.ConstruirArbolPerfil(idPerfil));
        Vista_Familia.ExpandAll();
    }

    private void MostrarArbolFamilia(Guid idFamilia)
    {
        Vista_Familia.Nodes.Clear();
        Vista_Familia.Nodes.Add(_gestor.Roles.ConstruirArbolFamilia(idFamilia));
        Vista_Familia.ExpandAll();
    }

    // --- Operaciones AGREGAR de Perfiles ---

    private void AccionAgregarFamiliaAPerfil()
    {
        if (dgvPerfiles.CurrentRow?.DataBoundItem is not ItemGrillaRbacBE perfil)
        {
            MessageBox.Show("Seleccioná un Perfil en la columna 'Perfiles Existentes'.", "Selección Requerida", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        if (dgvFamilias.CurrentRow?.DataBoundItem is not ItemGrillaRbacBE familia)
        {
            MessageBox.Show("Seleccioná una Familia en la columna 'Familias Existentes'.", "Selección Requerida", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var res = _gestor.Roles.AsignarComponenteAPerfil(perfil.Id, familia.Id);
        MessageBox.Show(res.Mensaje, "Asignar Familia a Perfil", MessageBoxButtons.OK, res.Ok ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
        if (res.Ok) RecargarGrillasPerfiles(seleccionarPerfilId: perfil.Id);
    }

    private void AccionAgregarPermisoBotonAPerfil()
    {
        if (dgvPerfiles.CurrentRow?.DataBoundItem is not ItemGrillaRbacBE perfil)
        {
            MessageBox.Show("Seleccioná un Perfil en la columna 'Perfiles Existentes'.", "Selección Requerida", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        if (dgvPermisos.CurrentRow?.DataBoundItem is not ItemGrillaRbacBE boton)
        {
            MessageBox.Show("Seleccioná un Botón en la columna 'Permisos (Solo Botones)'.", "Selección Requerida", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var res = _gestor.Roles.AsignarComponenteAPerfil(perfil.Id, boton.Id);
        MessageBox.Show(res.Mensaje, "Asignar Botón a Perfil", MessageBoxButtons.OK, res.Ok ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
        if (res.Ok) RecargarGrillasPerfiles(seleccionarPerfilId: perfil.Id);
    }

    private void AccionAgregarPermisoBotonAFamilia()
    {
        if (dgvFamilias.CurrentRow?.DataBoundItem is not ItemGrillaRbacBE familia)
        {
            MessageBox.Show("Seleccioná una Familia en la columna 'Familias Existentes'.", "Selección Requerida", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        if (dgvPermisos.CurrentRow?.DataBoundItem is not ItemGrillaRbacBE boton)
        {
            MessageBox.Show("Seleccioná un Botón en la columna 'Permisos (Solo Botones)'.", "Selección Requerida", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var res = _gestor.Roles.AgregarComponenteAFamilia(familia.Id, boton.Id);
        MessageBox.Show(res.Mensaje, "Agregar Botón a Familia", MessageBoxButtons.OK, res.Ok ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
        if (res.Ok) RecargarGrillasPerfiles(seleccionarFamiliaId: familia.Id);
    }

    private void AccionCrearNuevaFamilia()
    {
        var botonActual = dgvPermisos.CurrentRow?.DataBoundItem as ItemGrillaRbacBE;
        using var frm = new FrmCrearPermisoModal(FrmCrearPermisoModal.ModoFormulario.Familia, _gestor.Roles, botonActual?.Nombre);
        if (frm.ShowDialog(this) != DialogResult.OK) return;

        var res = _gestor.Roles.CrearFamilia(frm.NombrePermiso);
        if (!res.Ok)
        {
            MessageBox.Show(res.Mensaje, "Crear Familia", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (frm.IncluirBotonSeleccionadoEnFamilia && botonActual != null)
        {
            _gestor.Roles.AgregarComponenteAFamilia(res.FamiliaId, botonActual.Id);
        }

        MessageBox.Show(res.Mensaje, "Crear Familia", MessageBoxButtons.OK, MessageBoxIcon.Information);
        RecargarGrillasPerfiles(seleccionarFamiliaId: res.FamiliaId);
    }

    private void AccionCrearNuevoPerfil()
    {
        using var frm = new FrmCrearPermisoModal(FrmCrearPermisoModal.ModoFormulario.Perfil, _gestor.Roles);
        if (frm.ShowDialog(this) != DialogResult.OK) return;

        var res = _gestor.Roles.CrearPerfil(frm.NombrePermiso);
        MessageBox.Show(res.Mensaje, "Crear Perfil", MessageBoxButtons.OK, res.Ok ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
        if (res.Ok)
        {
            var nuevoId = _gestor.Roles.Store.Perfiles.FirstOrDefault(p => string.Equals(p.Nombre, frm.NombrePermiso, StringComparison.OrdinalIgnoreCase))?.Id;
            RecargarGrillasPerfiles(seleccionarPerfilId: nuevoId);
        }
    }

    private void AccionRegistrarPermisoBoton()
    {
        using var frm = new FrmCrearPermisoModal(FrmCrearPermisoModal.ModoFormulario.PermisoBoton, _gestor.Roles);
        if (frm.ShowDialog(this) != DialogResult.OK) return;

        var res = _gestor.Roles.RegistrarPermisoBoton(frm.NombreFormulario, frm.NombreBoton, frm.NombrePermiso);
        MessageBox.Show(res.Mensaje, "Registrar Botón como Permiso", MessageBoxButtons.OK, res.Ok ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
        if (res.Ok) RecargarGrillasPerfiles();
    }

    // --- Operaciones ELIMINAR / DESVINCULAR de Perfiles ---

    private void AccionDesvincularFamiliaDePerfil()
    {
        if (dgvPerfiles.CurrentRow?.DataBoundItem is not ItemGrillaRbacBE perfil ||
            dgvFamilias.CurrentRow?.DataBoundItem is not ItemGrillaRbacBE familia)
        {
            MessageBox.Show("Seleccioná el Perfil y la Familia que deseás desvincular.", "Selección Requerida", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (MessageBox.Show($"¿Confirmás quitar la familia '{familia.Nombre}' del perfil '{perfil.Nombre}'?", "Confirmar Desvinculación", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            return;

        var res = _gestor.Roles.DesvincularComponenteDePerfil(perfil.Id, familia.Id);
        MessageBox.Show(res.Mensaje, "Desvincular Familia de Perfil", MessageBoxButtons.OK, res.Ok ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
        if (res.Ok) RecargarGrillasPerfiles(seleccionarPerfilId: perfil.Id);
    }

    private void AccionDesvincularPermisoBotonDePerfil()
    {
        if (dgvPerfiles.CurrentRow?.DataBoundItem is not ItemGrillaRbacBE perfil ||
            dgvPermisos.CurrentRow?.DataBoundItem is not ItemGrillaRbacBE boton)
        {
            MessageBox.Show("Seleccioná el Perfil y el Botón que deseás quitar del perfil.", "Selección Requerida", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (MessageBox.Show($"¿Confirmás quitar el botón '{boton.Nombre}' del perfil '{perfil.Nombre}'?", "Confirmar Desvinculación", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            return;

        var res = _gestor.Roles.DesvincularComponenteDePerfil(perfil.Id, boton.Id);
        MessageBox.Show(res.Mensaje, "Quitar Botón del Perfil", MessageBoxButtons.OK, res.Ok ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
        if (res.Ok) RecargarGrillasPerfiles(seleccionarPerfilId: perfil.Id);
    }

    private void AccionDesvincularPermisoBotonDeFamilia()
    {
        if (dgvFamilias.CurrentRow?.DataBoundItem is not ItemGrillaRbacBE familia ||
            dgvPermisos.CurrentRow?.DataBoundItem is not ItemGrillaRbacBE boton)
        {
            MessageBox.Show("Seleccioná la Familia y el Botón que deseás quitar de esa familia.", "Selección Requerida", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (MessageBox.Show($"¿Confirmás quitar el botón '{boton.Nombre}' de la familia '{familia.Nombre}'?", "Confirmar Desvinculación", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            return;

        var res = _gestor.Roles.DesvincularComponenteDeFamilia(familia.Id, boton.Id);
        MessageBox.Show(res.Mensaje, "Quitar Botón de Familia", MessageBoxButtons.OK, res.Ok ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
        if (res.Ok) RecargarGrillasPerfiles(seleccionarFamiliaId: familia.Id);
    }

    private void AccionEliminarPerfil()
    {
        if (dgvPerfiles.CurrentRow?.DataBoundItem is not ItemGrillaRbacBE perfil)
        {
            MessageBox.Show("Seleccioná el Perfil que deseás eliminar.", "Selección Requerida", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (MessageBox.Show($"¿Confirmás eliminar el perfil '{perfil.Nombre}' del sistema?", "Confirmar Baja de Perfil", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
            return;

        var res = _gestor.Roles.EliminarPerfil(perfil.Id, _db.AdminUsuarios);
        MessageBox.Show(res.Mensaje, "Eliminar Perfil", MessageBoxButtons.OK, res.Ok ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
        if (res.Ok) RecargarGrillasPerfiles();
    }

    private void AccionEliminarFamilia()
    {
        if (dgvFamilias.CurrentRow?.DataBoundItem is not ItemGrillaRbacBE familia)
        {
            MessageBox.Show("Seleccioná la Familia que deseás eliminar.", "Selección Requerida", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (MessageBox.Show($"¿Confirmás eliminar la familia '{familia.Nombre}' y desvincularla en cascada de los perfiles donde esté asignada?", "Confirmar Baja de Familia", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
            return;

        var res = _gestor.Roles.EliminarFamilia(familia.Id);
        MessageBox.Show(res.Mensaje, "Eliminar Familia", MessageBoxButtons.OK, res.Ok ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
        if (res.Ok) RecargarGrillasPerfiles();
    }

    private void AccionEliminarPermisoBoton()
    {
        if (dgvPermisos.CurrentRow?.DataBoundItem is not ItemGrillaRbacBE boton)
        {
            MessageBox.Show("Seleccioná el Botón que deseás quitar de la lista de permisos.", "Selección Requerida", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (MessageBox.Show($"¿Confirmás eliminar el permiso del botón '{boton.Nombre}' en cascada?\n\n(Podrás volver a incorporarlo cuando quieras desde '➕ Agregar -> Permiso (Botón)').", "Confirmar Baja de Permiso", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
            return;

        var res = _gestor.Roles.EliminarPermisoBoton(boton.Id);
        MessageBox.Show(res.Mensaje, "Eliminar Permiso (Botón)", MessageBoxButtons.OK, res.Ok ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
        if (res.Ok) RecargarGrillasPerfiles();
    }

    private void AccionRelacionFamiliaAFamilia()
    {
        using var frm = new FrmSeleccionarFamiliaModal(_gestor.Roles);
        if (frm.ShowDialog(this) != DialogResult.OK) return;

        var res = frm.EsVinculacion
            ? _gestor.Roles.AgregarComponenteAFamilia(frm.IdFamiliaDestino, frm.IdFamiliaOrigen)
            : _gestor.Roles.DesvincularComponenteDeFamilia(frm.IdFamiliaDestino, frm.IdFamiliaOrigen);

        MessageBox.Show(res.Mensaje, "Relación entre Familias", MessageBoxButtons.OK, res.Ok ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
        if (res.Ok) RecargarGrillasPerfiles(seleccionarFamiliaId: frm.IdFamiliaDestino);
    }
}
