/**
 * Capa GUI (Graphical User Interface)
 * Patrón de Diseño GoF: STATE (Estado de Navegación SPA)
 * 
 * Cada pantalla de la SPA se modela como una clase de estado independiente
 * que encapsula su propio renderizado (HTML) y enlace de eventos (DOM),
 * desacoplándolos del controlador principal SimuladorController.
 */

class VistaState {
  render(ctrl) {
    throw new Error('Método render() debe ser implementado por el estado concreto');
  }

  bindEvents(ctrl) {}

  onEnter(ctrl) {}
}

/**
 * Componente de Encabezado Institucional IMPORTA+ (Logo Oficial)
 */
function renderBrandBadge(sizeClass = 'h-6 md:h-7') {
  return `
    <img src="logo-importa.png" alt="IMPORTA+" class="${sizeClass} w-auto object-contain select-none" />
  `;
}

/**
 * Estado 1: Portada Principal / Hero
 * - Logo principal central: logo-importa.png (Limpio, sin mapa/Ameri)
 * - Subtítulo: "Simulador oficial de costos aduaneros y tributos ARCA / DGA"
 * - Sin pie de página ("Comercio Exterior · Argentina · Calculadora Oficial PN1" eliminado)
 */
class HeroState extends VistaState {
  render(ctrl) {
    return `
      <div class="h-screen w-full flex flex-col justify-between overflow-hidden" style="background-color: #420213;">
        <header class="px-6 py-3 md:py-4 flex items-center justify-between shrink-0">
          <div class="flex items-center cursor-pointer" onclick="window.app.setView('hero')">
            ${renderBrandBadge('h-6 md:h-7')}
          </div>
        </header>

        <div class="flex-1 flex flex-col items-center justify-center text-center px-4 w-full min-h-0 py-1">
          <!-- Logo principal central IMPORTA+ (limpio, sin mapa) -->
          <div class="w-full flex items-center justify-center mb-6">
            <img 
              src="logo-importa.png" 
              alt="IMPORTA+" 
              class="w-auto h-16 sm:h-20 md:h-24 max-w-[85vw] object-contain mx-auto block select-none" 
            />
          </div>

          <p class="text-base sm:text-lg md:text-xl text-white/90 font-normal mb-7 max-w-3xl px-4 sm:whitespace-nowrap">
            Simulador oficial de costos aduaneros y tributos ARCA&nbsp;/&nbsp;DGA
          </p>

          <button
            id="btn-acceder-calculadora"
            onclick="window.app.setView('form')"
            class="bg-white text-[#420213] font-bold text-xs md:text-sm uppercase tracking-widest px-8 md:px-10 py-3.5 rounded-sm hover:bg-white/90 transition-all duration-200 shadow-xl hover:scale-[1.02] active:scale-100"
          >
            CALCULAR IMPORTACION
          </button>
        </div>

        <div class="py-2 shrink-0"></div>
      </div>
    `;
  }
}

/**
 * Estado 2: Formulario de Carga de Variables (Paso 1 de 2)
 * - Campo "Producto a Importar" en blanco, búsqueda únicamente por nombre (sin Código NCM)
 * - Campo "País de origen" con búsqueda incremental sobre los 195 países
 * - Cantidad (Unidades), Valor por unidad (USD FOB), Flete internacional (USD) y Seguro de transporte (USD) inicializados en 0
 * - Condición Impositiva del Importador sin aclaración:
 *   "Responsable Inscripto / Reventa" | "Consumo Particular / No categorizado"
 * - Liquidación Aduanera (PN1): Descripción del producto desplegable con botón
 */
class FormularioState extends VistaState {
  render(ctrl) {
    const f = ctrl.state.form;
    const cantNum = parseInt(f.cantidad) || 0;
    const precioNum = parseFloat(f.precioUnidad) || 0;
    const subtotalFOB = cantNum * precioNum;

    return `
      <div class="min-h-screen bg-[#F4F6F8]">
        <header class="bg-[#420213] px-6 py-4 flex items-center justify-between shadow-sm">
          <div class="flex items-center cursor-pointer" onclick="window.app.setView('hero')">
            ${renderBrandBadge('h-6 md:h-7')}
          </div>
          <span class="text-white/70 text-xs font-semibold uppercase tracking-widest">Paso 1 de 2</span>
        </header>

        <div class="max-w-2xl mx-auto px-4 py-10">
          <h2 class="text-2xl md:text-3xl font-bold text-[#171717] mb-1">Liquidación Aduanera</h2>
          <p class="text-[#555555] text-sm mb-8">Completá los datos para calcular el costo oficial de nacionalización.</p>

          <div class="bg-white rounded-sm border border-[#D8DDE3] divide-y divide-[#F4F6F8] overflow-hidden mb-6 shadow-sm">
            
            <!-- Bloque 1: Información General -->
            <div class="px-6 py-5">
              <div class="flex items-center gap-2 mb-4">
                <p class="text-xs font-bold uppercase tracking-widest text-[#530618]">Información general</p>
              </div>

              <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
                <!-- Buscador Predictivo exclusivamente por Nombre de Producto (En blanco al iniciar, sin código NCM) -->
                <div class="relative md:col-span-2">
                  <label class="block text-xs font-semibold uppercase tracking-wider text-[#555555] mb-1.5">
                    Producto a Importar
                  </label>
                  <input 
                    type="text" 
                    id="input-producto"
                    autocomplete="off"
                    class="w-full bg-white border border-[#D8DDE3] rounded-sm px-4 py-3 text-[#171717] text-sm focus:outline-none focus:border-[#530618] focus:ring-1 focus:ring-[#530618]"
                    placeholder="Escribí el nombre del producto (ej: Notebook, Remera, Router, Calzado)..."
                    value="${f.producto || ''}"
                  />
                  <div id="ncm-dropdown" class="absolute left-0 right-0 top-full mt-1 bg-white border border-[#D8DDE3] rounded-sm shadow-xl z-50 max-h-60 overflow-y-auto hidden"></div>

                  <div id="ficha-producto-seleccionado" class="${ctrl.state.posicionSeleccionada ? '' : 'hidden'} mt-2.5 p-3 bg-[#FFF8F8] border border-[#F0D0D4] rounded-sm text-xs">
                    ${ctrl.state.posicionSeleccionada ? `
                      <div class="flex justify-between items-center font-bold text-[#530618]">
                        <span>${ctrl.state.posicionSeleccionada.categoria || 'Producto Seleccionado'}</span>
                        <div class="flex items-center gap-2">
                          <span class="bg-[#530618] text-white px-2 py-0.5 rounded-sm text-[10px]">Arancel Base: ${ctrl.state.posicionSeleccionada.arancelExternoComun}%</span>
                          <button
                            type="button"
                            onclick="window.app.toggleDescripcionProducto()"
                            id="btn-texto-descripcion-producto"
                            class="text-[11px] font-bold text-[#530618] bg-white border border-[#F0D0D4] px-2.5 py-0.5 rounded-sm hover:bg-[#530618] hover:text-white transition-colors"
                          >
                            ${ctrl.state.mostrarDescripcionProducto ? 'Ocultar descripción del producto ▲' : 'Ver descripción del producto ▼'}
                          </button>
                        </div>
                      </div>
                      <div id="panel-descripcion-producto" class="${ctrl.state.mostrarDescripcionProducto ? '' : 'hidden'} mt-2 pt-2 border-t border-[#F0D0D4]">
                        <p class="text-[#555] leading-relaxed">${ctrl.state.posicionSeleccionada.descripcion}</p>
                        ${ctrl.state.posicionSeleccionada.poseeExcepcion ? `
                          <div class="mt-2 pt-2 border-t border-[#F0D0D4] flex items-center justify-between text-[#15803d]">
                            <span>Beneficio arancelario disponible:</span>
                            <span class="font-bold">Arancel reducido ${ctrl.state.posicionSeleccionada.arancelAplicableExcepcion}%</span>
                          </div>
                        ` : ''}
                      </div>
                    ` : ''}
                  </div>
                </div>

                <!-- País de Origen: Búsqueda Incremental con los 195 Países -->
                <div class="relative">
                  <label class="block text-xs font-semibold uppercase tracking-wider text-[#555555] mb-1.5">País de origen</label>
                  <input
                    type="text"
                    id="input-pais"
                    autocomplete="off"
                    class="w-full bg-white border border-[#D8DDE3] rounded-sm px-4 py-3 text-[#171717] text-sm focus:outline-none focus:border-[#530618] focus:ring-1 focus:ring-[#530618]"
                    placeholder="Buscar país de origen..."
                    value="${f.paisOrigen || ''}"
                  />
                  <div id="pais-dropdown" class="absolute left-0 right-0 top-full mt-1 bg-white border border-[#D8DDE3] rounded-sm shadow-xl z-50 max-h-60 overflow-y-auto hidden"></div>
                </div>

                <!-- Cantidad (Unidades) inicializado en 0 -->
                <div>
                  <label class="block text-xs font-semibold uppercase tracking-wider text-[#555555] mb-1.5">Cantidad (Unidades)</label>
                  <input
                    type="number"
                    id="input-cantidad"
                    min="0"
                    onfocus="if(this.value==='0') this.select()"
                    class="w-full bg-white border border-[#D8DDE3] rounded-sm px-4 py-3 text-[#171717] text-sm focus:outline-none focus:border-[#530618]"
                    value="${f.cantidad}"
                  >
                </div>

                <!-- Valor por unidad (USD FOB) inicializado en 0 -->
                <div class="md:col-span-2">
                  <label class="block text-xs font-semibold uppercase tracking-wider text-[#555555] mb-1.5">Valor por unidad (USD FOB)</label>
                  <div class="relative">
                    <span class="absolute left-3.5 top-1/2 -translate-y-1/2 text-[#999] text-sm font-medium">$</span>
                    <input
                      type="number"
                      step="0.01"
                      min="0"
                      id="input-precio"
                      onfocus="if(this.value==='0' || this.value==='0.00') this.select()"
                      class="w-full bg-white border border-[#D8DDE3] rounded-sm pl-8 pr-4 py-3 text-[#171717] text-sm focus:outline-none focus:border-[#530618]"
                      value="${f.precioUnidad}"
                    >
                  </div>
                </div>
              </div>

              <!-- Subtotal en vivo -->
              <div class="mt-4 bg-[#F4F6F8] border border-[#D8DDE3] rounded-sm px-4 py-3 flex items-center justify-between">
                <div>
                  <p class="text-xs font-bold uppercase tracking-wider text-[#555555]">Valor total de la mercadería (FOB Total)</p>
                  <p id="subtotal-fob-formula" class="text-[11px] text-[#530618] font-semibold mt-0.5">
                    ${cantNum} unid. × USD ${ctrl.formatNumber(precioNum)} = USD ${ctrl.formatNumber(subtotalFOB)}
                  </p>
                </div>
                <span id="subtotal-fob-monto" class="text-lg font-bold text-[#530618] tabular-nums">USD ${ctrl.formatNumber(subtotalFOB)}</span>
              </div>
            </div>

            <!-- Bloque 2: Logística Internacional (Flete y Seguro inicializados en 0) -->
            <div class="px-6 py-5">
              <div class="flex items-center justify-between mb-2">
                <div class="flex items-center gap-2">
                  <p class="text-xs font-bold uppercase tracking-widest text-[#530618]">Costos adicionales</p>
                  <span class="text-[10px] font-semibold border border-[#D8DDE3] text-[#555555] px-1.5 py-0.5 rounded-sm uppercase tracking-wide">Opcional</span>
                </div>
                <button type="button" onclick="window.app.setView('contact')" class="bg-[#530618] hover:bg-[#3D0411] text-white text-xs font-semibold px-3 py-1.5 rounded-sm transition-colors duration-200 shadow-sm cursor-pointer">
                  Contactar Fordweader
                </button>
              </div>

              <div class="grid grid-cols-1 md:grid-cols-2 gap-4 mt-3">
                <div>
                  <label class="block text-xs font-semibold uppercase tracking-wider text-[#555555] mb-1.5">Flete internacional (USD)</label>
                  <input
                    type="number"
                    min="0"
                    step="0.01"
                    id="input-flete"
                    onfocus="if(this.value==='0') this.select()"
                    class="w-full bg-white border border-[#D8DDE3] rounded-sm px-4 py-3 text-[#171717] text-sm focus:outline-none focus:border-[#530618]"
                    value="${f.flete}"
                  >
                </div>
                <div>
                  <label class="block text-xs font-semibold uppercase tracking-wider text-[#555555] mb-1.5">Seguro de transporte (USD)</label>
                  <input
                    type="number"
                    min="0"
                    step="0.01"
                    id="input-seguro"
                    onfocus="if(this.value==='0') this.select()"
                    class="w-full bg-white border border-[#D8DDE3] rounded-sm px-4 py-3 text-[#171717] text-sm focus:outline-none focus:border-[#530618]"
                    value="${f.seguro}"
                  >
                </div>
              </div>
            </div>

            <!-- Bloque 3: Condición Impositiva del Importador (Sin Aclaración) -->
            <div class="px-6 py-4 bg-[#fafbfc]">
              <label class="block text-xs font-semibold uppercase tracking-wider text-[#555555] mb-2">
                Condición Impositiva del Importador
              </label>
              <div class="flex flex-wrap gap-6 text-xs text-[#171717]">
                <label class="flex items-center gap-2 cursor-pointer font-medium">
                  <input type="radio" name="rad-ganancias" value="inscripto" ${f.condicionGanancias === 'inscripto' ? 'checked' : ''} onchange="window.app.state.form.condicionGanancias = 'inscripto'">
                  Responsable Inscripto / Reventa
                </label>
                <label class="flex items-center gap-2 cursor-pointer font-medium">
                  <input type="radio" name="rad-ganancias" value="no_inscripto" ${f.condicionGanancias === 'no_inscripto' ? 'checked' : ''} onchange="window.app.state.form.condicionGanancias = 'no_inscripto'">
                  Consumo Particular / No categorizado
                </label>
              </div>
            </div>

          </div>

          <!-- Botón Principal -->
          <button onclick="window.app.ejecutarLiquidacion()" class="w-full bg-[#530618] hover:bg-[#3D0411] text-white font-bold text-sm uppercase tracking-widest py-4 rounded-sm transition-colors duration-200 shadow-md">
            CALCULAR IMPORTACION
          </button>
        </div>
      </div>
    `;
  }

  bindEvents(ctrl) {
    const inputProd = document.getElementById('input-producto');
    const dropdownNcm = document.getElementById('ncm-dropdown');
    const inputPais = document.getElementById('input-pais');
    const dropdownPais = document.getElementById('pais-dropdown');
    const inputCant = document.getElementById('input-cantidad');
    const inputPrecio = document.getElementById('input-precio');
    const inputFlete = document.getElementById('input-flete');
    const inputSeguro = document.getElementById('input-seguro');

    // 1. Búsqueda incremental por Nombre de Producto (sin código NCM)
    const ejecutarBusquedaProducto = async (texto) => {
      if (!dropdownNcm) return;
      const cleanVal = String(texto || '').replace(/[0-9]/g, ' ').trim();
      if (cleanVal.length >= 2) {
        const res = await ctrl.posicionBLL.buscarPosiciones(cleanVal, 10);
        if (res && res.length > 0) {
          dropdownNcm.innerHTML = res.map((p, idx) => `
            <div class="p-3 border-b border-gray-100 hover:bg-[#FFF8F8] cursor-pointer text-xs" onmousedown="event.preventDefault(); window.app.seleccionarNCM(${idx})">
              <div class="flex justify-between font-bold text-[#530618]">
                <span>${p.categoria || 'Producto'}</span>
                <span class="text-gray-500 font-normal">Arancel: ${p.poseeExcepcion ? p.arancelAplicableExcepcion : p.arancelExternoComun}%</span>
              </div>
              <div class="text-gray-700 mt-0.5 font-medium">${p.descripcion}</div>
            </div>
          `).join('');
          dropdownNcm.classList.remove('hidden');
          window._ncmCache = res;
        } else {
          dropdownNcm.innerHTML = `
            <div class="p-3 text-xs text-gray-500">
              No se encontraron productos con ese nombre. Probá con otro término (ej: Notebook, Remera, Calzado, Teléfono).
            </div>
          `;
          dropdownNcm.classList.remove('hidden');
        }
      } else {
        dropdownNcm.classList.add('hidden');
      }
    };

    if (inputProd) {
      inputProd.addEventListener('input', (e) => {
        const val = e.target.value;
        ctrl.state.form.producto = val;
        ctrl.state.posicionSeleccionada = null;
        const fichaEl = document.getElementById('ficha-producto-seleccionado');
        if (fichaEl) fichaEl.classList.add('hidden');
        ejecutarBusquedaProducto(val);
      });
      inputProd.addEventListener('focus', (e) => {
        if (!ctrl.state.posicionSeleccionada && e.target.value.trim().length >= 2) {
          ejecutarBusquedaProducto(e.target.value);
        }
      });
      inputProd.addEventListener('click', (e) => {
        if (!ctrl.state.posicionSeleccionada && e.target.value.trim().length >= 2) {
          ejecutarBusquedaProducto(e.target.value);
        }
      });
      inputProd.addEventListener('blur', () => {
        if (dropdownNcm) dropdownNcm.classList.add('hidden');
      });
    }

    // 2. Búsqueda incremental de País de origen sobre los 195 países (SOLO LETRAS, abre al click/focus y cierra al desenfocar)
    const actualizarSugerenciasPais = async (texto) => {
      if (!dropdownPais) return;
      const res = await ctrl.paisBLL.buscarPaises(texto, 15);
      if (res && res.length > 0) {
        dropdownPais.innerHTML = res.map((p, idx) => `
          <div class="p-2.5 border-b border-gray-100 hover:bg-[#FFF8F8] cursor-pointer text-xs flex justify-between items-center" onmousedown="event.preventDefault(); window.app.seleccionarPais(${idx})">
            <span class="font-semibold text-[#171717]">${p.nombre}</span>
            <span class="text-[10px] px-2 py-0.5 rounded-sm ${p.esIntrazona ? 'bg-emerald-50 text-emerald-700 font-bold' : 'bg-gray-100 text-gray-600'}">
              ${p.esIntrazona ? 'Mercosur · 0% DIE' : 'Extrazona'}
            </span>
          </div>
        `).join('');
        dropdownPais.classList.remove('hidden');
        window._paisesCache = res;
      } else {
        dropdownPais.classList.add('hidden');
      }
    };

    if (inputPais) {
      inputPais.addEventListener('keydown', (e) => {
        if (e.ctrlKey || e.metaKey || e.altKey) return;
        const teclasPermitidas = ['Backspace', 'Delete', 'Tab', 'Escape', 'Enter', 'ArrowLeft', 'ArrowRight', 'ArrowUp', 'ArrowDown', 'Home', 'End', ' '];
        if (teclasPermitidas.includes(e.key)) {
          if (e.key === 'Escape' && dropdownPais) dropdownPais.classList.add('hidden');
          return;
        }
        if (!/^[a-zA-ZáéíóúÁÉÍÓÚñÑüÜ]$/.test(e.key)) {
          e.preventDefault();
        }
      });
      inputPais.addEventListener('input', (e) => {
        const soloLetras = e.target.value.replace(/[^a-zA-ZáéíóúÁÉÍÓÚñÑüÜ\s]/g, '');
        if (e.target.value !== soloLetras) {
          e.target.value = soloLetras;
        }
        ctrl.state.form.paisOrigen = soloLetras;
        ctrl.state.paisSeleccionado = null;
        actualizarSugerenciasPais(soloLetras);
      });
      inputPais.addEventListener('focus', (e) => {
        actualizarSugerenciasPais(e.target.value);
      });
      inputPais.addEventListener('click', (e) => {
        actualizarSugerenciasPais(e.target.value);
      });
      inputPais.addEventListener('blur', () => {
        if (dropdownPais) dropdownPais.classList.add('hidden');
      });
    }

    // 3. Actualización en vivo de Cantidad y Precio sin perder foco del cursor
    const actualizarSubtotalEnVivo = () => {
      const cantNum = parseInt(ctrl.state.form.cantidad) || 0;
      const precioNum = parseFloat(ctrl.state.form.precioUnidad) || 0;
      const subtotal = cantNum * precioNum;
      const elFormula = document.getElementById('subtotal-fob-formula');
      const elMonto = document.getElementById('subtotal-fob-monto');
      if (elFormula) {
        elFormula.textContent = `${cantNum} unid. × USD ${ctrl.formatNumber(precioNum)} = USD ${ctrl.formatNumber(subtotal)}`;
      }
      if (elMonto) {
        elMonto.textContent = `USD ${ctrl.formatNumber(subtotal)}`;
      }
    };

    if (inputCant) {
      inputCant.addEventListener('input', (e) => {
        ctrl.state.form.cantidad = e.target.value;
        actualizarSubtotalEnVivo();
      });
      inputCant.addEventListener('blur', (e) => {
        if (e.target.value === '') {
          e.target.value = '0';
          ctrl.state.form.cantidad = '0';
          actualizarSubtotalEnVivo();
        }
      });
    }

    if (inputPrecio) {
      inputPrecio.addEventListener('input', (e) => {
        ctrl.state.form.precioUnidad = e.target.value;
        actualizarSubtotalEnVivo();
      });
      inputPrecio.addEventListener('blur', (e) => {
        if (e.target.value === '') {
          e.target.value = '0';
          ctrl.state.form.precioUnidad = '0';
          actualizarSubtotalEnVivo();
        }
      });
    }

    if (inputFlete) {
      inputFlete.addEventListener('input', (e) => { ctrl.state.form.flete = e.target.value; });
      inputFlete.addEventListener('blur', (e) => {
        if (e.target.value === '') { e.target.value = '0'; ctrl.state.form.flete = '0'; }
      });
    }

    if (inputSeguro) {
      inputSeguro.addEventListener('input', (e) => { ctrl.state.form.seguro = e.target.value; });
      inputSeguro.addEventListener('blur', (e) => {
        if (e.target.value === '') { e.target.value = '0'; ctrl.state.form.seguro = '0'; }
      });
    }
  }
}

/**
 * Estado 3: Resumen de Resultados - Liquidación Aduanera (PN1)
 * - Incluye botón desplegable para ver/ocultar la descripción del producto
 */
class ResultadosState extends VistaState {
  render(ctrl) {
    const be = ctrl.state.calculadoraBE;
    if (!be) return '';

    const items = [
      { label: "Valor total del producto (FOB)", value: be.fobTotal },
      { label: "Flete internacional", value: be.flete },
      { label: "Seguro de transporte", value: be.seguro },
      { label: `Arancel DIE (${be.alicuotaDie}%)`, value: be.montoDie },
      { label: "Impuestos DGA", value: (be.montoIva + be.montoIvaAdicional + be.montoGanancias) },
      { label: "Cargos SIM + Tasa Estadística", value: (be.montoEstadistica + be.totalSimDigitalizacion) }
    ];

    return `
      <div class="min-h-screen bg-[#F4F6F8]">
        <header class="bg-[#420213] px-6 py-4 flex items-center justify-between shadow-sm">
          <div class="flex items-center cursor-pointer" onclick="window.app.setView('hero')">
            ${renderBrandBadge('h-6 md:h-7')}
          </div>
          <button onclick="window.app.setView('form')" class="text-white/70 text-xs font-medium hover:text-white transition-colors">
            ← Nueva consulta
          </button>
        </header>

        <div class="max-w-2xl mx-auto px-4 py-10">
          <div class="flex flex-wrap items-center justify-between gap-2 mb-2">
            <span class="text-xs font-bold uppercase tracking-widest text-[#530618]">Liquidación Aduanera</span>
            <button
              type="button"
              id="btn-texto-descripcion-producto"
              onclick="window.app.toggleDescripcionProducto()"
              class="text-xs font-bold text-[#530618] bg-white border border-[#F0D0D4] px-3 py-1 rounded-sm hover:bg-[#530618] hover:text-white transition-colors shadow-sm"
            >
              ${ctrl.state.mostrarDescripcionProducto ? 'Ocultar descripción del producto ▲' : 'Ver descripción del producto ▼'}
            </button>
          </div>

          <h2 class="text-2xl font-bold text-[#171717] mb-1">${be.posicion?.categoria || 'Mercadería Seleccionada'}</h2>
          <p class="text-[#555555] text-sm mb-3">Origen: ${be.pais?.nombre} · ${be.cantidad} unidades a USD ${ctrl.formatNumber(be.precioFobUnitario)}</p>

          <!-- Descripción del producto desplegable con botón -->
          <div id="panel-descripcion-producto" class="${ctrl.state.mostrarDescripcionProducto ? '' : 'hidden'} mb-5 p-4 bg-white border border-[#F0D0D4] rounded-sm text-xs text-[#555555] shadow-sm">
            <p class="font-bold text-[#530618] uppercase tracking-wider mb-1">Descripción del Producto</p>
            <p class="leading-relaxed text-[#171717]">${be.posicion?.descripcion || 'Mercadería General'}</p>
          </div>

          <!-- Tarjeta de Costo Total Vinotinto -->
          <div class="bg-[#530618] rounded-sm p-6 mb-4 relative overflow-hidden shadow-lg">
            <div class="absolute inset-0 opacity-10" style="background: radial-gradient(circle at 80% 20%, #fff 0%, transparent 60%);"></div>
            <p class="text-white/70 text-xs font-bold uppercase tracking-widest mb-1">Costo Total Estimado Puesto en Aduana</p>
            <p class="text-4xl md:text-5xl font-bold text-white tracking-tight">USD ${ctrl.formatNumber(be.costoTotalNacionalizado)}</p>
            <p class="text-white/80 text-xs mt-2 font-medium">≈ $ ${ctrl.formatNumber(be.costoTotalARS)} ARS <span class="opacity-60">(BNA: $${be.cotizacionBna})</span></p>
            <div class="mt-4 pt-3 border-t border-white/20 flex justify-between text-xs text-white/90">
              <span>Costo unitario nacionalizado:</span>
              <strong class="font-mono text-sm">USD ${ctrl.formatNumber(be.costoUnitarioNacionalizado)} / unidad</strong>
            </div>
          </div>

          <!-- Cuadro Comparativo FOB -> Landing Cost -->
          <div class="bg-white border border-[#D8DDE3] rounded-sm p-4 mb-4 shadow-sm">
            <div class="flex justify-between items-center text-xs font-bold text-[#555] mb-2 uppercase">
              <span>Incidencia Aduanera</span>
              <span class="text-[#530618]">+${be.ratioSobrecostoPct}% sobre FOB</span>
            </div>
            <div class="grid grid-cols-2 gap-3 text-center">
              <div class="p-3 bg-gray-50 rounded-sm border border-gray-100">
                <span class="text-[11px] text-gray-400 font-bold uppercase">Compra FOB</span>
                <p class="text-lg font-black text-gray-800">USD ${ctrl.formatNumber(be.fobTotal)}</p>
              </div>
              <div class="p-3 bg-[#FFF8F8] rounded-sm border border-[#F0D0D4]">
                <span class="text-[11px] text-[#530618] font-bold uppercase">Desembolso Final</span>
                <p class="text-lg font-black text-[#530618]">USD ${ctrl.formatNumber(be.costoTotalNacionalizado)}</p>
              </div>
            </div>
          </div>

          <!-- Tabla Resumen -->
          <div class="bg-white border border-[#D8DDE3] rounded-sm divide-y divide-[#F4F6F8] mb-4 shadow-sm">
            ${items.map(it => `
              <div class="flex items-center justify-between px-5 py-3.5">
                <span class="text-sm text-[#555555]">${it.label}</span>
                <span class="text-sm font-semibold text-[#171717] tabular-nums">USD ${ctrl.formatNumber(it.value)}</span>
              </div>
            `).join('')}
          </div>

          <button onclick="window.app.setView('breakdown')" class="w-full bg-[#530618] hover:bg-[#3D0411] text-white font-bold text-sm uppercase tracking-widest py-4 rounded-sm transition-colors duration-200 mb-3 shadow-md">
            VER DESGLOSE
          </button>
          <button onclick="window.ExcelDataLoaderDAL && window.ExcelDataLoaderDAL.exportarComprobanteExcel(window.app.state.calculadoraBE)" class="w-full bg-emerald-700 hover:bg-emerald-800 text-white font-bold text-xs uppercase tracking-widest py-3.5 rounded-sm transition-colors duration-200 mb-3 shadow-sm flex items-center justify-center">
            EXPORTAR PLANILLA EXCEL (.CSV / .XLSX)
          </button>
          <button onclick="window.app.setView('form')" class="w-full border border-[#530618] text-[#530618] hover:bg-[#530618] hover:text-white font-semibold text-sm py-3.5 rounded-sm transition-colors duration-200">
            Nueva consulta
          </button>
        </div>
      </div>
    `;
  }
}

/**
 * Estado 4: Desglose Analítico Oficial
 * - Sin "Fórmulas Normativas Aplicadas (Ley 22.415 / Dec. 99/2019 / RG 2281)" visibles para el cliente
 * - Descripción del producto desplegable con botón
 */
class DesgloseState extends VistaState {
  render(ctrl) {
    const be = ctrl.state.calculadoraBE;
    if (!be) return '';

    const items = [
      { label: "Valor total del producto (FOB)", value: be.fobTotal, note: "Precio de compra en puerto de origen", highlight: false },
      { label: "Flete internacional", value: be.flete, note: "Transporte internacional de origen a aduana", highlight: false },
      { label: "Seguro de transporte", value: be.seguro, note: "Póliza de cobertura sobre la mercadería", highlight: false },
      { label: "Valor Base (CIF)", value: be.cifAduanero, note: "Base Imponible de Valoración DGA", highlight: true },
      { label: `Derecho de Importación DIE (${be.alicuotaDie}%)`, value: be.montoDie, note: be.tratamientoArancelario, highlight: false },
      { label: `Tasa de Estadística (${be.alicuotaEstadistica}%)`, value: be.montoEstadistica, note: (be.pais?.esIntrazona || be.posicion?.esBienDeCapital) ? "Exento Mercosur / Bien de Capital" : "Tasa estadística aduanera sobre valor CIF", highlight: false },
      { label: `IVA General (${be.alicuotaIva}%)`, value: be.montoIva, note: "Aplicado sobre Base Imponible Interior", highlight: false },
      { label: `Percepción IVA Adicional (${be.alicuotaIvaAdicional}%)`, value: be.montoIvaAdicional, note: "Percepción aduanera de IVA", highlight: false },
      { label: `Percepción Ganancias (${be.alicuotaGanancias}%)`, value: be.montoGanancias, note: be.esInscriptoGanancias ? "Responsable Inscripto / Reventa" : "Consumo Particular / No categorizado", highlight: false },
      { label: "Arancel SIM + Digitalización", value: be.totalSimDigitalizacion, note: "USD 28 Malvina + USD 10 Digitalización", highlight: false }
    ];

    return `
      <div class="min-h-screen bg-[#F4F6F8] print:bg-white print:min-h-0">
        <!-- Barra de Navegación (Oculta en Impresión) -->
        <header class="bg-[#420213] px-6 py-4 flex items-center justify-between shadow-sm print:hidden">
          <div class="flex items-center cursor-pointer" onclick="window.app.setView('hero')">
            ${renderBrandBadge('h-6 md:h-7')}
          </div>
          <div class="flex items-center gap-3">
            <button onclick="window.print()" class="flex items-center gap-1.5 bg-white/10 hover:bg-white/20 text-white text-xs font-semibold px-3 py-1.5 rounded-sm transition-colors border border-white/20 cursor-pointer">
              <svg xmlns="http://www.w3.org/2000/svg" class="w-3.5 h-3.5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M17 17h2a2 2 0 002-2v-4a2 2 0 00-2-2H5a2 2 0 00-2 2v4a2 2 0 002 2h2m2 4h6a2 2 0 002-2v-4a2 2 0 00-2-2H9a2 2 0 00-2 2v4a2 2 0 002 2zm8-12V5a2 2 0 00-2-2H9a2 2 0 00-2 2v4h10z" />
              </svg>
              Imprimir Desglose
            </button>
            <button onclick="window.app.setView('results')" class="text-white/70 text-xs font-medium hover:text-white transition-colors cursor-pointer">
              ← Volver a resultados
            </button>
          </div>
        </header>

        <div class="max-w-2xl mx-auto px-4 py-10 print:py-0 print:px-0 print:max-w-full">
          <!-- Encabezado oficial exclusivo para impresión / PDF -->
          <div class="hidden print:block mb-6 border-b-2 border-[#530618] pb-4">
            <div class="flex items-center justify-between">
              <div>
                <h1 class="text-2xl font-black text-[#530618] tracking-tight">IMPORTA<span class="text-[#171717]">+</span></h1>
                <p class="text-xs text-[#555555] font-semibold mt-0.5">Desglose Analítico Oficial Aduanero</p>
              </div>
              <div class="text-right text-xs text-[#555555]">
                <p class="font-semibold text-[#171717]">Emisión: ${new Date().toLocaleDateString('es-AR', { day: '2-digit', month: '2-digit', year: 'numeric', hour: '2-digit', minute: '2-digit' })}</p>
              </div>
            </div>
          </div>

          <!-- Título y Botón desplegable de Descripción del Producto -->
          <div class="flex flex-col sm:flex-row sm:items-start justify-between gap-4 mb-4">
            <div>
              <span class="text-xs font-bold uppercase tracking-widest text-[#530618]">Liquidación Aduanera · Desglose Oficial</span>
              <h2 class="text-2xl font-bold text-[#171717] mt-1">${be.posicion?.categoria || 'Mercadería Seleccionada'}</h2>
              <div class="flex flex-wrap gap-x-4 gap-y-1 text-xs text-[#555555] mt-1.5 font-medium">
                <span>Origen: <strong class="text-[#171717]">${be.pais?.nombre || 'Internacional'}</strong></span>
                <span>Condición: <strong class="text-[#171717]">${be.esInscriptoGanancias ? 'Responsable Inscripto / Reventa' : 'Consumo Particular / No categorizado'}</strong></span>
              </div>
            </div>
            <div class="flex flex-wrap gap-2 print:hidden shrink-0">
              <button
                type="button"
                id="btn-texto-descripcion-producto"
                onclick="window.app.toggleDescripcionProducto()"
                class="inline-flex items-center gap-1.5 bg-white border border-[#530618] text-[#530618] hover:bg-[#530618] hover:text-white px-3.5 py-2 rounded-sm text-xs font-bold transition-all cursor-pointer"
              >
                ${ctrl.state.mostrarDescripcionProducto ? 'Ocultar descripción del producto ▲' : 'Ver descripción del producto ▼'}
              </button>
            </div>
          </div>

          <!-- Panel Desplegable de Descripción del Producto -->
          <div id="panel-descripcion-producto" class="${ctrl.state.mostrarDescripcionProducto ? '' : 'hidden'} mb-5 p-4 bg-white border border-[#F0D0D4] rounded-sm text-xs text-[#555555] shadow-sm">
            <p class="font-bold text-[#530618] uppercase tracking-wider mb-1">Descripción del Producto</p>
            <p class="leading-relaxed text-[#171717]">${be.posicion?.descripcion || 'Tu importación'}</p>
          </div>

          <!-- Tabla de Desglose Analítico -->
          <div class="bg-white border border-[#D8DDE3] rounded-sm overflow-hidden mb-6 shadow-sm print:shadow-none print:border-gray-300">
            ${items.map(it => `
              <div class="flex items-center justify-between px-5 py-4 border-b border-[#F4F6F8] print:border-gray-200 last:border-0 ${it.highlight ? 'bg-[#FFF8F8] print:bg-gray-50' : ''}">
                <div>
                  <p class="text-sm text-[#171717] ${it.highlight ? 'font-semibold' : 'font-normal'}">${it.label}</p>
                  <p class="text-xs text-[#999] mt-0.5 print:text-gray-500">${it.note}</p>
                </div>
                <span class="text-sm font-bold tabular-nums ${it.highlight ? 'text-[#530618]' : 'text-[#171717]'}">
                  USD ${ctrl.formatNumber(it.value)}
                </span>
              </div>
            `).join('')}

            <div class="flex items-center justify-between px-5 py-5 bg-[#530618] print:bg-[#530618] text-white">
              <p class="text-white/80 text-xs uppercase tracking-wider font-semibold">Costo Total Nacionalizado</p>
              <span class="text-xl font-bold text-white tabular-nums">USD ${ctrl.formatNumber(be.costoTotalNacionalizado)}</span>
            </div>
          </div>

          <!-- Botones de Acción (Ocultos en Impresión) -->
          <div class="print:hidden space-y-3">
            <div class="grid grid-cols-1 sm:grid-cols-2 gap-3">
              <button onclick="window.print()" class="w-full bg-[#530618] hover:bg-[#3D0411] text-white font-bold text-xs uppercase tracking-widest py-3.5 rounded-sm transition-colors duration-200 shadow-md flex items-center justify-center gap-2 cursor-pointer">
                <svg xmlns="http://www.w3.org/2000/svg" class="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M17 17h2a2 2 0 002-2v-4a2 2 0 00-2-2H5a2 2 0 00-2 2v4a2 2 0 002 2h2m2 4h6a2 2 0 002-2v-4a2 2 0 00-2-2H9a2 2 0 00-2 2v4a2 2 0 002 2zm8-12V5a2 2 0 00-2-2H9a2 2 0 00-2 2v4h10z" />
                </svg>
                Imprimir / PDF Oficial
              </button>
              <button onclick="window.ExcelDataLoaderDAL && window.ExcelDataLoaderDAL.exportarComprobanteExcel(window.app.state.calculadoraBE)" class="w-full bg-emerald-700 hover:bg-emerald-800 text-white font-bold text-xs uppercase tracking-widest py-3.5 rounded-sm transition-colors duration-200 shadow-md flex items-center justify-center cursor-pointer">
                Exportar Planilla Excel
              </button>
            </div>
            <button onclick="window.app.setView('ports')" class="w-full border border-[#530618] text-[#530618] hover:bg-[#530618] hover:text-white font-bold text-sm uppercase tracking-widest py-3.5 rounded-sm transition-colors duration-200 cursor-pointer">
              VER PUERTOS Y ADUANAS
            </button>
            <button onclick="window.app.setView('results')" class="w-full border border-[#D8DDE3] text-[#555555] hover:border-[#530618] hover:text-[#530618] font-semibold text-sm py-3.5 rounded-sm transition-colors duration-200 cursor-pointer">
              ← Volver a resultados
            </button>
          </div>
        </div>
      </div>
    `;
  }
}

/**
 * Estado 5: Puertos y Aduanas en Mapa Leaflet
 */
class PuertosState extends VistaState {
  render(ctrl) {
    const puertos = ctrl.state.puertosCatalogo || [];
    return `
      <div class="min-h-screen bg-[#F4F6F8]">
        <header class="bg-[#420213] px-6 py-4 flex items-center justify-between shadow-sm">
          <div class="flex items-center cursor-pointer" onclick="window.app.setView('hero')">
            ${renderBrandBadge('h-6 md:h-7')}
          </div>
          <button onclick="window.app.setView('breakdown')" class="text-white/70 text-xs font-medium hover:text-white transition-colors">
            ← Volver al desglose
          </button>
        </header>

        <div class="max-w-4xl mx-auto px-4 py-10">
          <span class="text-xs font-bold uppercase tracking-widest text-[#530618]">Logística aduanera argentina</span>
          <h2 class="text-2xl md:text-3xl font-bold text-[#171717] mt-1 mb-2">Puertos y Aduanas Principales</h2>
          <p class="text-[#555] text-sm mb-6">Puntos de ingreso habilitados para nacionalización de mercaderías.</p>

          <div id="map" class="h-96 w-full rounded-sm border border-[#D8DDE3] mb-6 shadow-sm z-10"></div>

          <div class="grid grid-cols-1 md:grid-cols-2 gap-4 mb-8">
            ${puertos.map(p => `
              <div class="bg-white border border-[#D8DDE3] rounded-sm p-4 shadow-sm hover:border-[#530618] transition-colors">
                <div class="flex justify-between items-center mb-1">
                  <span class="text-xs font-bold text-[#530618] uppercase">${p.tipo || p.type}</span>
                  <span class="text-[10px] text-gray-400 font-semibold">${p.operador}</span>
                </div>
                <h4 class="font-bold text-[#171717] text-base mb-1">${p.nombre || p.name}</h4>
                <p class="text-xs text-[#555]">${p.descripcion || p.desc}</p>
              </div>
            `).join('')}
          </div>
        </div>
      </div>
    `;
  }

  onEnter(ctrl) {
    setTimeout(() => ctrl.initMap(), 150);
  }
}

/**
 * Estado 6: Contacto y Cotización con Despachante
 */
class ContactoState extends VistaState {
  render(ctrl) {
    return `
      <div class="min-h-screen bg-[#F4F6F8]">
        <header class="bg-[#420213] px-6 py-4 flex items-center justify-between shadow-sm">
          <div class="flex items-center cursor-pointer" onclick="window.app.setView('hero')">
            ${renderBrandBadge('h-6 md:h-7')}
          </div>
          <button onclick="window.app.state.calculadoraBE ? window.app.setView('ports') : window.app.setView('form')" class="text-white/70 text-xs font-medium hover:text-white transition-colors">
            ← Volver
          </button>
        </header>

        <div class="max-w-2xl mx-auto px-4 py-16 text-center">
          <div class="bg-white border border-[#D8DDE3] rounded-sm p-10 shadow-sm max-w-lg mx-auto">
            <div class="w-16 h-16 rounded-full bg-rose-50 text-[#530618] flex items-center justify-center mx-auto mb-5 shadow-inner">
              <svg class="w-8 h-8" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="1.8" d="M19 21V5a2 2 0 00-2-2H7a2 2 0 00-2 2v16m14 0h2m-2 0h-5m-9 0H3m2 0h5M9 7h1m-1 4h1m4-4h1m-1 4h1m-5 10v-5a1 1 0 011-1h2a1 1 0 011 1v5m-4 0h4" />
              </svg>
            </div>

            <span class="text-xs font-bold uppercase tracking-widest text-[#530618] bg-rose-50 border border-rose-200 px-3 py-1 rounded-full inline-block mb-3">
              Próximamente
            </span>

            <h2 class="text-2xl font-bold text-[#171717] mb-2">
              Sección en Desarrollo PN4
            </h2>

            <p class="text-[#555] text-sm mb-6 max-w-sm mx-auto leading-relaxed">
              El módulo de conexión directa y cotización con despachantes de aduana y forwarders se encuentra actualmente en proceso de integración.
            </p>

            <button
              onclick="window.app.state.calculadoraBE ? window.app.setView('ports') : window.app.setView('form')"
              class="inline-flex items-center justify-center bg-[#530618] hover:bg-[#3D0411] text-white font-bold text-xs uppercase tracking-widest px-8 py-3.5 rounded-sm transition-colors shadow"
            >
              ← Volver
            </button>
          </div>
        </div>
      </div>
    `;
  }
}

/**
 * Estado 7: Planes y Precios
 */
class PlanesState extends VistaState {
  render(ctrl) {
    return `
      <div class="min-h-screen bg-[#F4F6F8]">
        <header class="bg-[#420213] px-6 py-4 flex items-center justify-between shadow-sm">
          <div class="flex items-center cursor-pointer" onclick="window.app.setView('hero')">
            ${renderBrandBadge('h-6 md:h-7')}
          </div>
          <button onclick="window.app.setView('hero')" class="text-white/70 text-xs font-medium hover:text-white transition-colors">
            ← Volver al inicio
          </button>
        </header>

        <div class="max-w-4xl mx-auto px-4 py-12 text-center">
          <h2 class="text-3xl font-black text-[#171717] mb-3">Planes y Precios</h2>
          <p class="text-[#555] text-sm mb-10 max-w-md mx-auto">Seleccioná la opción adecuada para tu volumen de importaciones.</p>

          <div class="grid grid-cols-1 md:grid-cols-3 gap-6 text-left">
            <div class="bg-white border border-[#D8DDE3] rounded-sm p-6 shadow-sm">
              <h3 class="font-bold text-[#171717] text-lg mb-1">GRATUITO</h3>
              <p class="text-3xl font-extrabold text-[#530618] mb-4">$0 <span class="text-xs text-gray-500 font-normal">/mes</span></p>
              <ul class="text-xs text-gray-600 space-y-2 mb-6">
                <li>3 cálculos por mes</li>
                <li>Cálculo de costo estimado</li>
                <li>Consulta básica de puertos</li>
              </ul>
              <button onclick="window.app.setView('form')" class="w-full border border-[#530618] text-[#530618] font-bold text-xs uppercase py-3 rounded-sm hover:bg-[#530618] hover:text-white transition-colors">
                COMENZAR GRATIS
              </button>
            </div>

            <div class="bg-white border-2 border-[#530618] rounded-sm p-6 shadow-md relative">
              <span class="absolute -top-3 right-4 bg-[#530618] text-white text-[10px] font-bold uppercase px-2 py-0.5 rounded-sm">Recomendado</span>
              <h3 class="font-bold text-[#171717] text-lg mb-1">PRO</h3>
              <p class="text-3xl font-extrabold text-[#530618] mb-4">USD 9,99 <span class="text-xs text-gray-500 font-normal">/mes</span></p>
              <ul class="text-xs text-gray-600 space-y-2 mb-6">
                <li>Cálculos ilimitados</li>
                <li>Desglose completo ítem por ítem</li>
                <li>Exportación a PDF y Excel</li>
                <li>Soporte con forwarders matriculados</li>
              </ul>
              <button onclick="window.app.setView('form')" class="w-full bg-[#530618] text-white font-bold text-xs uppercase py-3 rounded-sm hover:bg-[#3D0411] transition-colors">
                ELEGIR PLAN PRO
              </button>
            </div>

            <div class="bg-white border border-[#D8DDE3] rounded-sm p-6 shadow-sm">
              <h3 class="font-bold text-[#171717] text-lg mb-1">EMPRESA</h3>
              <p class="text-3xl font-extrabold text-[#171717] mb-4">Personalizado</p>
              <ul class="text-xs text-gray-600 space-y-2 mb-6">
                <li>Múltiples usuarios por cuenta</li>
                <li>Asesoramiento arancelario continuo</li>
                <li>Integración API con sistemas ERP</li>
              </ul>
              <button onclick="window.app.setView('contact')" class="w-full border border-gray-400 text-gray-700 font-bold text-xs uppercase py-3 rounded-sm hover:bg-gray-100 transition-colors">
                CONTACTAR VENTAS
              </button>
            </div>
          </div>
        </div>
      </div>
    `;
  }
}

if (typeof window !== 'undefined') {
  window.VistaState = VistaState;
  window.HeroState = HeroState;
  window.FormularioState = FormularioState;
  window.ResultadosState = ResultadosState;
  window.DesgloseState = DesgloseState;
  window.PuertosState = PuertosState;
  window.ContactoState = ContactoState;
  window.PlanesState = PlanesState;
}
