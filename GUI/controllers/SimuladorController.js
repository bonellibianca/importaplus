/**
 * Capa GUI (Graphical User Interface)
 * Controlador Principal: SimuladorController
 * - Context del Patrón STATE (Navegación SPA)
 * - Observador Concreto del Patrón OBSERVER (Multi-idioma Español / Inglés)
 */
class SimuladorController {
  constructor() {
    this.paisBLL = new window.PaisBLL();
    this.posicionBLL = new window.PosicionArancelariaBLL();
    this.calculadoraBLL = new window.CalculadoraArancelariaBLL();
    this.puertoBLL = new window.PuertoBLL();

    this.states = {
      hero: new window.HeroState(),
      form: new window.FormularioState(),
      results: new window.ResultadosState(),
      breakdown: new window.DesgloseState(),
      ports: new window.PuertosState(),
      contact: new window.ContactoState(),
      despachantes: new window.DespachantesState(),
      planes: new window.PlanesState()
    };
    this.currentState = this.states['hero'];

    this.state = {
      view: 'hero',
      idioma: window.idiomaSubject ? window.idiomaSubject.obtenerIdioma() : 'es',
      form: {
        producto: '',
        paisOrigen: '',
        cantidad: '0',
        precioUnidad: '0',
        valorProducto: '0',
        flete: '0',
        seguro: '0',
        arancelPct: '0',
        impuestosPct: '21',
        otrosGastos: '38',
        condicionGanancias: 'inscripto'
      },
      posicionSeleccionada: null,
      paisSeleccionado: null,
      mostrarDescripcionProducto: false,
      aplicarExcepcion: true,
      ncmSugerencias: [],
      paisesCatalogo: [],
      puertosCatalogo: [],
      forwardersCatalogo: [],
      despachantesCatalogo: [],
      mailCopiadoId: null,
      calculadoraBE: null,
      selectedPlan: null,
      mapInstance: null
    };
  }

  /**
   * Método del Patrón GoF OBSERVER: invocado automáticamente por IdiomaSubject al alternar ES / EN
   */
  actualizarIdioma(nuevoIdioma) {
    this.state.idioma = nuevoIdioma;
    this.render();
    if (this.state.view === 'ports' && this.currentState.onEnter) {
      this.currentState.onEnter(this);
    }
  }

  t(clave) {
    if (window.idiomaSubject) {
      return window.idiomaSubject.t(clave);
    }
    return clave;
  }

  alternarIdioma() {
    if (window.idiomaSubject) {
      window.idiomaSubject.alternarIdioma();
    }
  }

  toggleMenuHamburguesa(e) {
    if (e && e.stopPropagation) e.stopPropagation();
    const drop = document.getElementById('dropdown-hamburguesa');
    if (drop) {
      drop.classList.toggle('hidden');
    }
  }

  abrirWhatsappTrabajaConNosotros() {
    const drop = document.getElementById('dropdown-hamburguesa');
    if (drop) drop.classList.add('hidden');

    const primerFwd = (this.state.forwardersCatalogo || [])[0];
    const rawWa = (primerFwd && primerFwd.whatsapp) ? primerFwd.whatsapp : '+5491143218800';
    const digits = String(rawWa).replace(/[^0-9]/g, '') || '5491143218800';
    const msg = encodeURIComponent(this.t('waMensajeTrabaja'));
    window.open(`https://wa.me/${digits}?text=${msg}`, '_blank', 'noopener,noreferrer');
  }

  /**
   * Sanitización anti-XSS para atributos y nodos HTML (corrige vulnerabilidad XSS en plantillas)
   */
  escapeHtml(valor) {
    return String(valor ?? '')
      .replace(/&/g, '&amp;')
      .replace(/</g, '&lt;')
      .replace(/>/g, '&gt;')
      .replace(/"/g, '&quot;')
      .replace(/'/g, '&#39;');
  }

  async inicializar() {
    // Suscribir este controlador como Observador Concreto del Sujeto de Idioma
    if (window.idiomaSubject) {
      window.idiomaSubject.suscribir(this);
    }

    if (!this._outsideMenuBound) {
      this._outsideMenuBound = true;
      document.addEventListener('click', (e) => {
        const wrap = document.getElementById('hamburger-menu-wrap');
        const drop = document.getElementById('dropdown-hamburguesa');
        if (drop && !drop.classList.contains('hidden') && wrap && !wrap.contains(e.target)) {
          drop.classList.add('hidden');
        }
      });
    }

    try {
      this.state.paisesCatalogo = await this.paisBLL.obtenerPaises();
      this.state.puertosCatalogo = await this.puertoBLL.obtenerPuertos();
      if (window.SupabaseClientDAL) {
        const [fwds, desps] = await Promise.all([
          window.SupabaseClientDAL.obtenerForwardersGrilla(),
          window.SupabaseClientDAL.obtenerDespachantesGrilla()
        ]);
        this.state.forwardersCatalogo = fwds;
        this.state.despachantesCatalogo = desps;
      }
      await this.posicionBLL.obtenerTodas();
      if (this.calculadoraBLL && this.calculadoraBLL.calculadoraDAL) {
        this.calculadoraBLL.calculadoraDAL.obtenerParametrosRegulatorios().catch(() => {});
      }
    } catch (e) {
      console.warn('Inicialización de catálogos diferida:', e);
    }
    this.render();
  }

  setView(newView) {
    this.state.view = newView;
    this.currentState = this.states[newView] || this.states['hero'];
    window.scrollTo({ top: 0, behavior: 'smooth' });
    this.render();
    if (this.currentState.onEnter) {
      this.currentState.onEnter(this);
    }
  }

  render() {
    const app = document.getElementById('app');
    if (!app || !this.currentState) return;
    const switchFlotante = typeof window.renderFloatingLanguageSwitch === 'function'
      ? window.renderFloatingLanguageSwitch(this)
      : '';
    app.innerHTML = this.currentState.render(this) + switchFlotante;
    if (this.currentState.bindEvents) {
      this.currentState.bindEvents(this);
    }
  }

  formatNumber(num) {
    const val = parseFloat(num) || 0;
    const locale = this.state.idioma === 'en' ? 'en-US' : 'es-AR';
    return val.toLocaleString(locale, { minimumFractionDigits: 2, maximumFractionDigits: 2 });
  }

  redirigirWhatsappForwarder(idx) {
    const fwd = (this.state.forwardersCatalogo || [])[idx];
    if (!fwd || !fwd.whatsapp) return;
    const digits = String(fwd.whatsapp).replace(/[^0-9]/g, '');
    if (!digits) return;
    const msg = encodeURIComponent(this.t('waMensajeDefault'));
    window.open(`https://wa.me/${digits}?text=${msg}`, '_blank', 'noopener,noreferrer');
  }

  async copiarMailForwarder(idx) {
    const fwd = (this.state.forwardersCatalogo || [])[idx];
    if (!fwd || !fwd.email) return;
    try {
      await navigator.clipboard.writeText(fwd.email);
    } catch {
      const ta = document.createElement('textarea');
      ta.value = fwd.email;
      document.body.appendChild(ta);
      ta.select();
      document.execCommand('copy');
      document.body.removeChild(ta);
    }
    this.state.mailCopiadoId = fwd.id;
    this.render();
    setTimeout(() => {
      if (this.state.mailCopiadoId === fwd.id) {
        this.state.mailCopiadoId = null;
        this.render();
      }
    }, 2200);
  }

  redirigirWhatsappDespachante(idx) {
    const desp = (this.state.despachantesCatalogo || [])[idx];
    if (!desp || !desp.whatsapp) return;
    const digits = String(desp.whatsapp).replace(/[^0-9]/g, '');
    if (!digits) return;
    const msg = encodeURIComponent(this.t('waMensajeDespachante'));
    window.open(`https://wa.me/${digits}?text=${msg}`, '_blank', 'noopener,noreferrer');
  }

  async copiarMailDespachante(idx) {
    const desp = (this.state.despachantesCatalogo || [])[idx];
    if (!desp || !desp.email) return;
    try {
      await navigator.clipboard.writeText(desp.email);
    } catch {
      const ta = document.createElement('textarea');
      ta.value = desp.email;
      document.body.appendChild(ta);
      ta.select();
      document.execCommand('copy');
      document.body.removeChild(ta);
    }
    this.state.mailCopiadoId = desp.id;
    this.render();
    setTimeout(() => {
      if (this.state.mailCopiadoId === desp.id) {
        this.state.mailCopiadoId = null;
        this.render();
      }
    }, 2200);
  }

  seleccionarNCM(idx) {
    if (window._ncmCache && window._ncmCache[idx]) {
      this.state.posicionSeleccionada = window._ncmCache[idx];
      this.state.form.producto = this.state.posicionSeleccionada.descripcion;
      this.state.mostrarDescripcionProducto = false;
      const dropdown = document.getElementById('ncm-dropdown');
      if (dropdown) dropdown.classList.add('hidden');
      this.render();
    }
  }

  seleccionarPais(idx) {
    if (window._paisesCache && window._paisesCache[idx]) {
      this.state.paisSeleccionado = window._paisesCache[idx];
      this.state.form.paisOrigen = this.state.paisSeleccionado.nombre;
      const dropdown = document.getElementById('pais-dropdown');
      if (dropdown) dropdown.classList.add('hidden');
      const inputPais = document.getElementById('input-pais');
      if (inputPais) {
        inputPais.value = this.state.paisSeleccionado.nombre;
        inputPais.blur();
      }
    }
  }

  toggleDescripcionProducto() {
    this.state.mostrarDescripcionProducto = !this.state.mostrarDescripcionProducto;
    const panel = document.getElementById('panel-descripcion-producto');
    const btnTexto = document.getElementById('btn-texto-descripcion-producto');
    if (panel) {
      panel.classList.toggle('hidden', !this.state.mostrarDescripcionProducto);
      if (btnTexto) {
        btnTexto.textContent = this.state.mostrarDescripcionProducto
          ? this.t('ocultarDesc')
          : this.t('verDesc');
      }
    } else {
      this.render();
    }
  }

  resolverPaisActual() {
    if (this.state.paisSeleccionado) return this.state.paisSeleccionado;
    const texto = (this.state.form.paisOrigen || '').trim().toLowerCase();
    if (!texto) return null;

    const normalizar = (s) =>
      String(s || '')
        .toLowerCase()
        .normalize('NFD')
        .replace(/[\u0300-\u036f]/g, '');

    const term = normalizar(texto);
    return (
      this.state.paisesCatalogo.find((p) => normalizar(p.nombre) === term) ||
      this.state.paisesCatalogo.find((p) => normalizar(p.nombre).includes(term)) ||
      null
    );
  }

  estimarLogistica() {
    const cant = parseInt(this.state.form.cantidad) || 0;
    const precio = parseFloat(this.state.form.precioUnidad) || 0;
    const fob = cant * precio;
    const pais = this.resolverPaisActual() || this.state.paisesCatalogo[0];
    const est = this.paisBLL.estimarFleteYSeguro(fob, pais);
    this.state.form.flete = est.flete.toString();
    this.state.form.seguro = est.seguro.toString();
    this.render();
  }

  async ejecutarLiquidacion() {
    try {
      const f = this.state.form;
      let pos = this.state.posicionSeleccionada;

      if (!pos && f.producto && f.producto.trim().length >= 2) {
        const sugerencias = await this.posicionBLL.buscarPosiciones(f.producto, 1);
        if (sugerencias && sugerencias.length > 0) {
          pos = sugerencias[0];
          this.state.posicionSeleccionada = pos;
        }
      }

      if (!pos) {
        alert(this.t('alertProducto'));
        return;
      }

      const pais = this.resolverPaisActual();
      if (!pais) {
        alert(this.t('alertPais'));
        return;
      }

      const cantidadVal = parseInt(f.cantidad) || 0;
      const precioVal = parseFloat(f.precioUnidad) || 0;

      if (cantidadVal <= 0) {
        alert(this.t('alertCantidad'));
        return;
      }
      if (precioVal <= 0) {
        alert(this.t('alertFob'));
        return;
      }

      const be = new window.CalculadoraArancelariaBE({
        posicion: pos,
        cantidad: cantidadVal,
        precioFobUnitario: precioVal,
        pais: pais,
        flete: parseFloat(f.flete) || 0,
        seguro: parseFloat(f.seguro) || 0,
        esInscriptoGanancias: f.condicionGanancias === 'inscripto',
        aplicarExcepcion: this.state.aplicarExcepcion
      });

      this.state.mostrarDescripcionProducto = false;
      this.state.calculadoraBE = await this.calculadoraBLL.liquidar(be);
      this.setView('results');
    } catch (err) {
      alert('Error: ' + err.message);
    }
  }

  initMap() {
    const mapEl = document.getElementById('map');
    if (!mapEl || typeof L === 'undefined') return;

    if (this.state.mapInstance) {
      try {
        this.state.mapInstance.remove();
      } catch (e) {}
      this.state.mapInstance = null;
    }

    const map = L.map('map', {
      center: [-34.6, -60.0],
      zoom: 5,
      scrollWheelZoom: false
    });
    this.state.mapInstance = map;

    const hl = this.state.idioma === 'en' ? 'en' : 'es';
    const googleLayer = L.tileLayer(`https://{s}.google.com/vt/lyrs=m&hl=${hl}&x={x}&y={y}&z={z}`, {
      subdomains: ['mt0', 'mt1', 'mt2', 'mt3'],
      attribution: '&copy; Google Maps',
      maxZoom: 20
    });

    const cartoFallbackLayer = L.tileLayer('https://{s}.basemaps.cartocdn.com/rastertiles/voyager/{z}/{x}/{y}{r}.png', {
      subdomains: 'abcd',
      attribution: '&copy; CARTO &copy; OpenStreetMap',
      maxZoom: 19
    });

    let fallbackActivated = false;
    googleLayer.on('tileerror', () => {
      if (!fallbackActivated) {
        fallbackActivated = true;
        if (map.hasLayer(googleLayer)) map.removeLayer(googleLayer);
        cartoFallbackLayer.addTo(map);
      }
    });

    googleLayer.addTo(map);

    setTimeout(() => {
      try {
        map.invalidateSize();
      } catch (e) {}
    }, 200);

    const puertos = this.state.puertosCatalogo || [];
    const bounds = [];

    puertos.forEach((p) => {
      const lat = parseFloat(p.lat);
      const lng = parseFloat(p.lng);
      if (isNaN(lat) || isNaN(lng)) return;

      bounds.push([lat, lng]);

      const tipoStr = (p.tipo || p.type || '').toLowerCase();
      let icono = '⚓';
      let colorBg = '#530618';
      if (tipoStr.includes('aér') || tipoStr.includes('aer') || tipoStr.includes('aéreo')) {
        icono = '✈️';
        colorBg = '#1E3A8A';
      } else if (tipoStr.includes('terr') || tipoStr.includes('paso')) {
        icono = '🚛';
        colorBg = '#065F46';
      } else if (tipoStr.includes('fluv')) {
        icono = '🚢';
        colorBg = '#0284C7';
      }

      const customIcon = L.divIcon({
        className: 'custom-port-marker',
        html: `
          <div style="
            background: ${colorBg};
            color: #FFFFFF;
            width: 32px;
            height: 32px;
            border-radius: 50%;
            display: flex;
            align-items: center;
            justify-content: center;
            font-size: 15px;
            box-shadow: 0 4px 10px rgba(0,0,0,0.35);
            border: 2px solid #FFFFFF;
            cursor: pointer;
          ">${icono}</div>
        `,
        iconSize: [32, 32],
        iconAnchor: [16, 16],
        popupAnchor: [0, -18]
      });

      const marker = L.marker([lat, lng], { icon: customIcon }).addTo(map);
      const popupHtml = `
        <div style="font-family: 'Inter', system-ui, sans-serif; padding: 2px; min-width: 200px;">
          <div style="display: flex; justify-content: space-between; align-items: center; margin-bottom: 4px;">
            <span style="font-size: 10px; font-weight: 700; color: ${colorBg}; text-transform: uppercase;">
              ${this.escapeHtml(p.tipo || p.type)}
            </span>
            <span style="font-size: 9px; font-weight: 600; background: #F1F5F9; color: #475569; padding: 2px 6px; border-radius: 4px;">
              ${this.escapeHtml(p.id ? p.id.toUpperCase() : 'ADUANA')}
            </span>
          </div>
          <h4 style="margin: 0 0 4px 0; font-size: 13px; font-weight: 800; color: #0F172A; line-height: 1.2;">
            ${this.escapeHtml(p.nombre || p.name)}
          </h4>
          <p style="margin: 0 0 8px 0; font-size: 11px; color: #64748B; line-height: 1.35;">
            ${this.escapeHtml(p.descripcion || p.desc)}
          </p>
          <a href="${this.escapeHtml(p.google_maps_url || 'https://www.google.com/maps')}" target="_blank" rel="noopener noreferrer" style="display: inline-flex; align-items: center; gap: 4px; font-size: 11px; font-weight: 700; color: #530618; text-decoration: none;">
            ${this.escapeHtml(this.t('verEnGoogleMaps'))} &#8599;
          </a>
        </div>
      `;
      marker.bindPopup(popupHtml);
    });

    if (bounds.length > 0) {
      setTimeout(() => {
        try {
          map.fitBounds(bounds, { padding: [40, 40], maxZoom: 7 });
          map.invalidateSize();
        } catch (e) {}
      }, 300);
    }
  }
}

if (typeof window !== 'undefined') {
  window.SimuladorController = SimuladorController;
}
