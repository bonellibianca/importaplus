/**
 * Capa GUI (Graphical User Interface)
 * Controlador Principal: SimuladorController (Context del Patrón STATE)
 * 
 * Orquesta la aplicación respetando las 4 capas y patrones de diseño:
 * - Capa BE: CalculadoraArancelariaBE, PosicionArancelariaBE, PaisBE, PuertoBE, GravamenItemBE
 * - Capa BLL: CalculadoraArancelariaBLL (Template Method), PosicionArancelariaBLL, PaisBLL, PuertoBLL
 * - Capa DAL: CalculadoraArancelariaDAL, PosicionArancelariaDAL, PaisDAL, PuertoDAL, ExcelDataLoaderDAL
 * - Capa GUI: Patrón STATE (HeroState, FormularioState, ResultadosState, etc.)
 */
class SimuladorController {
  constructor() {
    // 1. Instanciación desacoplada de la Capa de Lógica de Negocio (BLL)
    this.paisBLL = new window.PaisBLL();
    this.posicionBLL = new window.PosicionArancelariaBLL();
    this.calculadoraBLL = new window.CalculadoraArancelariaBLL();
    this.puertoBLL = new window.PuertoBLL();

    // 2. Registro de Estados de Navegación SPA (Patrón STATE)
    this.states = {
      hero: new window.HeroState(),
      form: new window.FormularioState(),
      results: new window.ResultadosState(),
      breakdown: new window.DesgloseState(),
      ports: new window.PuertosState(),
      contact: new window.ContactoState(),
      planes: new window.PlanesState()
    };
    this.currentState = this.states['hero'];

    // 3. Estado reactivo de datos de la sesión (Inicializado en blanco y valores numéricos en 0)
    this.state = {
      view: 'hero',
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
      calculadoraBE: null,
      selectedPlan: null,
      mapInstance: null
    };
  }

  /**
   * Inicializa catálogos y datos persistentes desde la BLL / DAL (Excel)
   * Mantiene el campo Producto a Importar y País de origen en blanco al iniciar.
   */
  async inicializar() {
    try {
      this.state.paisesCatalogo = await this.paisBLL.obtenerPaises();
      this.state.puertosCatalogo = await this.puertoBLL.obtenerPuertos();
      await this.posicionBLL.obtenerTodas();
      if (this.calculadoraBLL && this.calculadoraBLL.calculadoraDAL) {
        this.calculadoraBLL.calculadoraDAL.obtenerParametrosRegulatorios().catch(() => {});
      }
    } catch (e) {
      console.warn('Inicialización de catálogos diferida:', e);
    }
    this.render();
  }

  /**
   * Transición de Estado (Patrón STATE)
   * @param {string} newView 
   */
  setView(newView) {
    this.state.view = newView;
    this.currentState = this.states[newView] || this.states['hero'];
    window.scrollTo({ top: 0, behavior: 'smooth' });
    this.render();
    if (this.currentState.onEnter) {
      this.currentState.onEnter(this);
    }
  }

  /**
   * Renderiza delegando en el objeto del Estado Actual (Patrón STATE)
   */
  render() {
    const app = document.getElementById('app');
    if (!app || !this.currentState) return;
    app.innerHTML = this.currentState.render(this);
    if (this.currentState.bindEvents) {
      this.currentState.bindEvents(this);
    }
  }

  /**
   * Utilidad de formato de moneda argentina
   */
  formatNumber(num) {
    const val = parseFloat(num) || 0;
    return val.toLocaleString('es-AR', { minimumFractionDigits: 2, maximumFractionDigits: 2 });
  }

  /**
   * Selección de producto por nombre (sin insertar código NCM en el campo de texto)
   */
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

  /**
   * Selección de país desde el buscador incremental de 195 países
   */
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

  /**
   * Alterna el desplegable de la descripción del producto con botón
   */
  toggleDescripcionProducto() {
    this.state.mostrarDescripcionProducto = !this.state.mostrarDescripcionProducto;
    const panel = document.getElementById('panel-descripcion-producto');
    const btnTexto = document.getElementById('btn-texto-descripcion-producto');
    if (panel) {
      panel.classList.toggle('hidden', !this.state.mostrarDescripcionProducto);
      if (btnTexto) {
        btnTexto.textContent = this.state.mostrarDescripcionProducto
          ? 'Ocultar descripción del producto ▲'
          : 'Ver descripción del producto ▼';
      }
    } else {
      this.render();
    }
  }

  /**
   * Resuelve el país seleccionado o escrito por el usuario entre los 195 países
   */
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

  /**
   * Cálculo paramétrico de logística internacional (Regla supletoria legal)
   */
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

  /**
   * Ejecuta la liquidación delegando en la BLL (Template Method)
   */
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
        alert('Por favor escribí y seleccioná un producto a importar.');
        return;
      }

      const pais = this.resolverPaisActual();
      if (!pais) {
        alert('Por favor escribí y seleccioná un país de origen.');
        return;
      }

      const cantidadVal = parseInt(f.cantidad) || 0;
      const precioVal = parseFloat(f.precioUnidad) || 0;

      if (cantidadVal <= 0) {
        alert('La cantidad de unidades debe ser mayor a 0.');
        return;
      }
      if (precioVal <= 0) {
        alert('El valor por unidad (USD FOB) debe ser mayor a 0.');
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
      alert('Error al calcular: ' + err.message);
    }
  }

  /**
   * Inicializa el mapa interactivo de puertos con Leaflet
   */
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

    // Capa de Google Maps (100% gratuita, sin bloqueo 403 en file:/// ni HTTP) con respaldo CartoDB
    const googleLayer = L.tileLayer('https://{s}.google.com/vt/lyrs=m&hl=es&x={x}&y={y}&z={z}', {
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

    puertos.forEach(p => {
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
            transition: transform 0.2s ease;
          " onmouseover="this.style.transform='scale(1.2)'" onmouseout="this.style.transform='scale(1)'">
            ${icono}
          </div>
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
              ${p.tipo || p.type}
            </span>
            <span style="font-size: 9px; font-weight: 600; background: #F1F5F9; color: #475569; padding: 2px 6px; border-radius: 4px;">
              ${p.id ? p.id.toUpperCase() : 'ADUANA'}
            </span>
          </div>
          <h4 style="margin: 0 0 4px 0; font-size: 13px; font-weight: 800; color: #0F172A; line-height: 1.2;">
            ${p.nombre || p.name}
          </h4>
          <p style="margin: 0 0 6px 0; font-size: 11px; color: #64748B; line-height: 1.35;">
            ${p.descripcion || p.desc}
          </p>
          <div style="font-size: 10px; color: #334155; border-top: 1px solid #E2E8F0; padding-top: 6px; display: flex; justify-content: space-between; align-items: center;">
            <span><b>Operador:</b> ${p.operador || 'ARCA - DGA'}</span>
            <a href="https://www.google.com/maps/search/?api=1&query=${lat},${lng}" target="_blank" rel="noopener noreferrer" style="color: #530618; font-weight: 700; text-decoration: none;">Google Maps ↗</a>
          </div>
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
