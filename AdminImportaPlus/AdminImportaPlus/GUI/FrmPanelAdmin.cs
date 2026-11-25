using System;
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
    private static readonly HttpClient ImgHttp = new() { Timeout = TimeSpan.FromSeconds(6) };

    private readonly AdminGestorBLL _gestor;
    private BaseDatosCompletaBE _db = new();

    private Guid _fwdEditId = Guid.NewGuid();
    private string? _fwdRutaImagenLocalSeleccionada;
    private Guid _puertoEditId = Guid.NewGuid();

    public FrmPanelAdmin()
    {
        InitializeComponent();
        _gestor = new AdminGestorBLL();
        ConfigurarEstilosYEventos();
    }

    public FrmPanelAdmin(AdminGestorBLL gestor, int seccionInicial = 0)
    {
        InitializeComponent();
        _gestor = gestor;
        ConfigurarEstilosYEventos();
        MostrarSeccion(seccionInicial);
    }

    private void ConfigurarEstilosYEventos()
    {
        TemaImportaPlus.EstilizarGrilla(dgvForwarders);
        TemaImportaPlus.EstilizarGrilla(dgvPuertos);
        TemaImportaPlus.EstilizarGrilla(dgvDvv);

        cmbPuertoVia.SelectedIndex = 0;

        // Navegación con los 5 botones apilados (Botón 3 de NCM/Países removido, reemplazado por Roles Composite)
        btnNavForwarders.Click += (_, _) => MostrarSeccion(0);
        btnNavPuertos.Click += (_, _) => MostrarSeccion(1);
        btnNavRoles.Click += (_, _) => MostrarSeccion(2);
        btnNavDigitoVerificador.Click += (_, _) => MostrarSeccion(3);
        btnNavRestoreSupabase.Click += (_, _) => MostrarSeccion(4);

        btnCambiarPassword.Click += (_, _) =>
        {
            using var frm = new FrmRestablecerPassword(_gestor, _gestor.AdminActual?.Username ?? "Admin");
            frm.ShowDialog(this);
            _ = RecargarDatosUIAsync();
        };
        btnCerrarSesion.Click += (_, _) => Close();

        // Eventos Forwarders
        dgvForwarders.SelectionChanged += (_, _) => CargarForwarderSeleccionadoEnEditor();
        btnSubirFoto.Click += (_, _) => SeleccionarImagenForwarderLocal();
        txtFwdImagenUrl.Leave += async (_, _) => await PrevisualizarImagenDesdeUrlAsync(txtFwdImagenUrl.Text);
        txtFwdNombre.TextChanged += (_, _) => lblContadorNombreFwd.Text = $"{txtFwdNombre.Text.Length} / 120";
        btnGuardarFwd.Click += async (_, _) => await GuardarForwarderDesdeUIAsync();
        btnNuevoFwd.Click += (_, _) => LimpiarEditorForwarder();
        btnEliminarFwd.Click += async (_, _) => await EliminarForwarderDesdeUIAsync();

        // Eventos Puertos
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
            decimal.TryParse(txtPuertoLat.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out var lat);
            decimal.TryParse(txtPuertoLng.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out var lng);
            var res = await _gestor.GuardarPuertoAsync(new PuertoAduanaBE
            {
                Id = _puertoEditId,
                Nombre = txtPuertoNombre.Text,
                ViaIngreso = cmbPuertoVia.SelectedItem?.ToString() ?? "MARITIMA_FLUVIAL",
                Provincia = txtPuertoProvincia.Text,
                CodigoSim = txtPuertoSim.Text,
                Operador = txtPuertoOperador.Text,
                Infraestructura = txtPuertoInfra.Text,
                Latitud = lat == 0 ? null : lat,
                Longitud = lng == 0 ? null : lng
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
                MessageBox.Show(res.Mensaje);
                await RecargarDatosUIAsync();
            }
        };

        // Eventos Roles Composite (Permisos | Familias | Perfiles + TreeView + Regla anti-duplicados)
        btnCrearPermiso.Click += (_, _) =>
        {
            var res = _gestor.Roles.CrearPermiso(txtPermisoCodigo.Text, txtPermisoNombre.Text);
            MessageBox.Show(res.Mensaje, "Permisos", MessageBoxButtons.OK, res.Ok ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
            if (res.Ok)
            {
                txtPermisoCodigo.Clear();
                txtPermisoNombre.Clear();
                RecargarRolesUI();
            }
        };
        btnCrearFamilia.Click += (_, _) =>
        {
            var res = _gestor.Roles.CrearFamilia(txtFamiliaNombre.Text);
            MessageBox.Show(res.Mensaje, "Familias de Permisos", MessageBoxButtons.OK, res.Ok ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
            if (res.Ok)
            {
                txtFamiliaNombre.Clear();
                RecargarRolesUI();
            }
        };
        btnAgregarAFamilia.Click += (_, _) =>
        {
            if (cmbFamilias.SelectedItem is not ComponentePermiso fam || cmbComponentesParaFamilia.SelectedItem is not ComponentePermiso comp)
                return;
            var res = _gestor.Roles.AgregarComponenteAFamilia(fam.Id, comp.Id);
            MessageBox.Show(res.Mensaje, "Familias (Composite)", MessageBoxButtons.OK, res.Ok ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
            if (res.Ok) RecargarRolesUI();
        };
        btnCrearPerfil.Click += (_, _) =>
        {
            var res = _gestor.Roles.CrearPerfil(txtPerfilNombre.Text);
            MessageBox.Show(res.Mensaje, "Perfiles", MessageBoxButtons.OK, res.Ok ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
            if (res.Ok)
            {
                txtPerfilNombre.Clear();
                RecargarRolesUI();
            }
        };
        cmbPerfiles.SelectedIndexChanged += (_, _) => ActualizarTreeViewPerfil();
        btnAsignarAPerfil.Click += (_, _) =>
        {
            if (cmbPerfiles.SelectedItem is not PerfilBE perfil || cmbComponentesParaPerfil.SelectedItem is not ComponentePermiso comp)
                return;
            var res = _gestor.Roles.AsignarComponenteAPerfil(perfil.Id, comp.Id);
            MessageBox.Show(res.Mensaje, "Asignación de Permisos", MessageBoxButtons.OK, res.Ok ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
            if (res.Ok) ActualizarTreeViewPerfil();
        };
        btnQuitarDePerfil.Click += (_, _) =>
        {
            if (cmbPerfiles.SelectedItem is not PerfilBE perfil || tvPermisosPerfil.SelectedNode?.Tag is not Guid compId)
            {
                MessageBox.Show("Seleccioná en el TreeView el permiso o familia asignado al perfil que deseás quitar.", "Quitar", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            var res = _gestor.Roles.QuitarComponenteDePerfil(perfil.Id, compId);
            MessageBox.Show(res.Mensaje, "Quitar del Perfil", MessageBoxButtons.OK, res.Ok ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
            if (res.Ok) ActualizarTreeViewPerfil();
        };

        // Eventos Dígito Verificador
        btnVerificarDv.Click += async (_, _) => await RecargarDatosUIAsync(mostrarMensajeIntegridad: true);
        btnRecalcularDv.Click += async (_, _) =>
        {
            var res = await _gestor.RecalcularDigitosVerificadoresAsync();
            MessageBox.Show(res.Mensaje, "Recalcular Dígito Verificador", MessageBoxButtons.OK, MessageBoxIcon.Information);
            await RecargarDatosUIAsync();
        };

        // Eventos RestoreBackup & Supabase
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
            await RecargarDatosUIAsync();
        };
        btnEmpujarNube.Click += async (_, _) =>
        {
            var guardado = await _gestor.Dal.GuardarBaseDatosCompletaAsync(_db, sincronizarNube: true);
            lblResultadoSupa.Text = guardado.Detalle;
            MessageBox.Show(guardado.Detalle, "Sincronización Supabase", MessageBoxButtons.OK, guardado.Ok ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
        };

        Shown += async (_, _) =>
        {
            RecargarRolesUI();
            await RecargarDatosUIAsync();
        };
        MostrarSeccion(0);
    }

    private void MostrarSeccion(int indice)
    {
        Panel[] paneles =
        {
            pnlSecForwarders,
            pnlSecPuertos,
            pnlSecRoles,
            pnlSecDigitoVerificador,
            pnlSecRestoreSupabase
        };

        AppleButton[] botones =
        {
            btnNavForwarders,
            btnNavPuertos,
            btnNavRoles,
            btnNavDigitoVerificador,
            btnNavRestoreSupabase
        };

        for (int i = 0; i < paneles.Length; i++)
        {
            bool activo = i == indice;
            paneles[i].Visible = activo;
            if (activo) paneles[i].BringToFront();
            botones[i].BackColor = activo ? Color.FromArgb(115, 16, 40) : TemaImportaPlus.HeaderBurgundy;
            botones[i].ForeColor = activo ? Color.White : Color.FromArgb(245, 215, 222);
        }
    }

    private void RecargarRolesUI()
    {
        var grafo = _gestor.Roles.ConstruirGrafoComposite();
        var familias = grafo.Values.Where(c => c.EsFamilia).OrderBy(c => c.Nombre).ToList();
        var todos = grafo.Values.OrderByDescending(c => c.EsFamilia).ThenBy(c => c.Nombre).ToList();

        Guid? perfilPrevio = (cmbPerfiles.SelectedItem as PerfilBE)?.Id;

        cmbFamilias.DataSource = null;
        cmbFamilias.DataSource = familias;

        cmbComponentesParaFamilia.DataSource = null;
        cmbComponentesParaFamilia.DataSource = todos.ToList();

        cmbComponentesParaPerfil.DataSource = null;
        cmbComponentesParaPerfil.DataSource = todos.ToList();

        cmbPerfiles.DataSource = null;
        cmbPerfiles.DataSource = _gestor.Roles.Store.Perfiles.ToList();

        if (perfilPrevio.HasValue)
        {
            int idx = _gestor.Roles.Store.Perfiles.FindIndex(p => p.Id == perfilPrevio.Value);
            if (idx >= 0) cmbPerfiles.SelectedIndex = idx;
        }

        ActualizarTreeViewPerfil();
    }

    private void ActualizarTreeViewPerfil()
    {
        tvPermisosPerfil.Nodes.Clear();
        if (cmbPerfiles.SelectedItem is not PerfilBE perfil) return;

        var nodoRaiz = _gestor.Roles.ConstruirArbolPerfil(perfil.Id);
        tvPermisosPerfil.Nodes.Add(nodoRaiz);
        tvPermisosPerfil.ExpandAll();
    }

    private async Task RecargarDatosUIAsync(bool mostrarMensajeIntegridad = false)
    {
        _db = await _gestor.Dal.ObtenerBaseDatosActualAsync();
        var verif = SeguridadYDigitoVerificadorBLL.VerificarBaseCompleta(_db);

        lblUsuarioActual.Text = $"Usuario: {_gestor.AdminActual?.Username ?? "Admin"}";
        string modo = _gestor.Dal.Config.EstaConfiguradoEnNube ? "Supabase Cloud" : "Base Local";
        lblBadgeEstadoHeader.Text = verif.SaltoDetectado
            ? $"● {modo}  ·  ⚠ Alerta DVH/DVV ({verif.Alteraciones.Count} cambio/s)"
            : $"● {modo}  ·  ✔ Integridad DVH/DVV Verificada";
        lblBadgeEstadoHeader.ForeColor = verif.SaltoDetectado ? TemaImportaPlus.BrandPrimary : TemaImportaPlus.EmeraldGreen;

        dgvForwarders.DataSource = _db.Forwarders.OrderBy(f => f.Orden).ToList();
        OcultarColumnasTecnicas(dgvForwarders, "Id", "Dvh", "ImagenUrl");

        dgvPuertos.DataSource = _db.PuertosAduanas.OrderBy(p => p.Nombre).ToList();
        OcultarColumnasTecnicas(dgvPuertos, "Id", "Dvh", "Infraestructura");

        // Mostrar tabla limpia de DV con el conteo TOTAL real de cada tabla (10.439 NCM, 196 países, etc.)
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
        _ = PrevisualizarImagenDesdeUrlAsync(f.ImagenUrl);
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

    private async Task PrevisualizarImagenDesdeUrlAsync(string? urlOrData)
    {
        if (string.IsNullOrWhiteSpace(urlOrData))
        {
            picForwarder.Image = null;
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
                    picForwarder.Image = new Bitmap(ms);
                }
            }
            else if (File.Exists(urlOrData))
            {
                using var img = Image.FromFile(urlOrData);
                picForwarder.Image = new Bitmap(img);
            }
            else if (urlOrData.StartsWith("http", StringComparison.OrdinalIgnoreCase))
            {
                byte[] bytes = await ImgHttp.GetByteArrayAsync(urlOrData);
                using var ms = new MemoryStream(bytes);
                picForwarder.Image = new Bitmap(ms);
            }
        }
        catch
        {
            picForwarder.Image = null;
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
        MessageBox.Show(res.Mensaje, "ABM Forwarders", MessageBoxButtons.OK, MessageBoxIcon.Information);
        LimpiarEditorForwarder();
        await RecargarDatosUIAsync();
    }
}
