/**
 * Capa GUI (Graphical User Interface)
 * Patrón de Diseño GoF: STATE + Integración con Patrón OBSERVER (Multi-idioma ES / EN)
 * + Apartado FORWARDERS (Grilla de 4 cuadrados con Foto, Nombre <= 120 chars, WhatsApp y Mail)
 */

class VistaState {
  render(ctrl) {
    throw new Error('Método render() debe ser implementado por el estado concreto');
  }
  bindEvents(ctrl) {}
  onEnter(ctrl) {}
}

function renderBrandBadge(sizeClass = 'h-6 md:h-7') {
  return `<img src="logo-importa.svg" alt="IMPORTA+" class="${sizeClass} w-auto object-contain select-none" />`;
}

function renderHamburgerMenu(ctrl) {
  return `
    <div id="hamburger-menu-wrap" class="relative print:hidden">
      <button
        type="button"
        onclick="window.app.toggleMenuHamburguesa(event)"
        title="Menú"
        aria-label="Abrir menú"
        class="inline-flex items-center justify-center w-9 h-9 bg-white/10 hover:bg-white/20 border border-white/25 text-white rounded-sm transition-colors cursor-pointer select-none"
      >
        <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.2" d="M4 6h16M4 12h16M4 18h16" />
        </svg>
      </button>

      <div
        id="dropdown-hamburguesa"
        class="hidden absolute right-0 mt-2 w-60 bg-white text-[#171717] rounded-sm shadow-2xl border border-[#F0D0D4] py-1.5 z-50"
      >
        <button
          type="button"
          onclick="window.app.abrirWhatsappTrabajaConNosotros()"
          class="w-full px-4 py-2.5 text-left text-xs font-bold text-[#171717] hover:bg-[#FFF8F8] hover:text-[#530618] flex items-center gap-2.5 transition-colors cursor-pointer"
        >
          <span class="inline-flex items-center justify-center w-7 h-7 rounded-full bg-emerald-600 text-white shrink-0">
            <svg class="w-4 h-4 fill-current" viewBox="0 0 24 24">
              <path d="M20.52 3.48A11.82 11.82 0 0012.06 0C5.5 0 .16 5.34.16 11.9c0 2.1.55 4.15 1.6 5.95L0 24l6.32-1.66a11.86 11.86 0 005.74 1.46h.01c6.55 0 11.89-5.34 11.89-11.9 0-3.18-1.24-6.17-3.44-8.42zM12.07 21.8h-.01a9.87 9.87 0 01-5.03-1.38l-.36-.21-3.75.98 1-3.65-.24-.38a9.85 9.85 0 01-1.51-5.26c0-5.45 4.44-9.89 9.9-9.89 2.64 0 5.13 1.03 7 2.9a9.83 9.83 0 012.9 7c0 5.45-4.44 9.89-9.9 9.89zm5.43-7.41c-.3-.15-1.76-.87-2.03-.97-.27-.1-.47-.15-.67.15-.2.3-.77.97-.94 1.16-.17.2-.35.22-.65.07-.3-.15-1.26-.46-2.4-1.48-.89-.79-1.49-1.77-1.66-2.07-.17-.3-.02-.46.13-.61.13-.13.3-.35.45-.52.15-.17.2-.3.3-.5.1-.2.05-.37-.02-.52-.08-.15-.67-1.61-.92-2.21-.24-.58-.49-.5-.67-.51h-.57c-.2 0-.52.07-.79.37-.27.3-1.04 1.02-1.04 2.48 0 1.46 1.07 2.88 1.22 3.08.15.2 2.1 3.2 5.08 4.49.71.31 1.26.49 1.69.63.71.23 1.36.2 1.87.12.57-.09 1.76-.72 2.01-1.41.25-.7.25-1.29.17-1.41-.07-.12-.27-.2-.57-.35z" />
            </svg>
          </span>
          <span>${ctrl.escapeHtml(ctrl.t('menuTrabajaConNosotros'))}</span>
        </button>
      </div>
    </div>
  `;
}

function renderFloatingLanguageSwitch(ctrl) {
  return `
    <div class="fixed bottom-5 right-5 z-50 print:hidden">
      <button
        type="button"
        onclick="window.app.alternarIdioma()"
        title="Cambiar idioma / Switch language"
        class="inline-flex items-center gap-2 bg-[#420213] hover:bg-[#530618] border border-white/30 text-white text-xs font-bold px-4 py-2.5 rounded-full shadow-xl transition-all cursor-pointer select-none"
      >
        <svg class="w-4 h-4 text-rose-200" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M21 12a9 9 0 01-9 9m9-9a9 9 0 00-9-9m9 9H3m9 9a9 9 0 01-9-9m9 9c1.657 0 3-4.03 3-9s-1.343-9-3-9m0 18c-1.657 0-3-4.03-3-9s1.343-9 3-9m-9 9a9 9 0 019-9" />
        </svg>
        <span>${ctrl.escapeHtml(ctrl.t('toggleBtn'))}</span>
      </button>
    </div>
  `;
}

/**
 * Renderiza la Grilla de 4 Cuadrados de Forwarders (Foto + Nombre <= 120 chars + Botón WhatsApp + Botón Mail abajo)
 */
function renderForwardersGrid(ctrl) {
  const forwarders = ctrl.state.forwardersCatalogo || [];
  if (forwarders.length === 0) return '';

  return `
    <div class="bg-white border border-[#D8DDE3] rounded-sm p-6 shadow-sm">
      <div class="border-b border-[#F4F6F8] pb-3 mb-5 text-left">
        <span class="text-xs font-bold uppercase tracking-widest text-[#530618]">${ctrl.escapeHtml(ctrl.t('fwdTag'))}</span>
        <h3 class="text-xl md:text-2xl font-bold text-[#171717] mt-0.5">${ctrl.escapeHtml(ctrl.t('fwdTitulo'))}</h3>
        <p class="text-xs text-[#555555] mt-1">${ctrl.escapeHtml(ctrl.t('fwdSubtitulo'))}</p>
      </div>

      <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-5 text-left">
        ${forwarders.map((fwd, idx) => {
          const nombreMax120 = String(fwd.nombre || '').slice(0, 120);
          const copiado = ctrl.state.mailCopiadoId === fwd.id;
          return `
            <div class="bg-[#FFF8F8] border border-[#F0D0D4] rounded-sm overflow-hidden shadow-sm hover:border-[#530618] transition-all flex flex-col justify-between">
              <div class="w-full aspect-square bg-gray-200 overflow-hidden">
                <img
                  src="${ctrl.escapeHtml(fwd.imagen_url || 'logo-importa.png')}"
                  alt="${ctrl.escapeHtml(nombreMax120)}"
                  loading="lazy"
                  onerror="this.onerror=null;this.src='logo-importa.png';"
                  class="w-full h-full object-cover"
                />
              </div>

              <div class="p-4 flex flex-col flex-1 justify-between gap-3">
                <h4 class="text-sm font-bold text-[#171717] text-center leading-snug break-words" title="${ctrl.escapeHtml(nombreMax120)}">
                  ${ctrl.escapeHtml(nombreMax120)}
                </h4>

                <div class="space-y-2">
                  <button
                    type="button"
                    onclick="window.app.redirigirWhatsappForwarder(${idx})"
                    class="w-full flex items-center justify-center gap-2 bg-emerald-600 hover:bg-emerald-700 text-white font-bold text-xs py-2.5 px-3 rounded-sm transition-colors shadow-sm cursor-pointer"
                  >
                    <svg class="w-4 h-4 fill-current shrink-0" viewBox="0 0 24 24">
                      <path d="M20.52 3.48A11.82 11.82 0 0012.06 0C5.5 0 .16 5.34.16 11.9c0 2.1.55 4.15 1.6 5.95L0 24l6.32-1.66a11.86 11.86 0 005.74 1.46h.01c6.55 0 11.89-5.34 11.89-11.9 0-3.18-1.24-6.17-3.44-8.42zM12.07 21.8h-.01a9.87 9.87 0 01-5.03-1.38l-.36-.21-3.75.98 1-3.65-.24-.38a9.85 9.85 0 01-1.51-5.26c0-5.45 4.44-9.89 9.9-9.89 2.64 0 5.13 1.03 7 2.9a9.83 9.83 0 012.9 7c0 5.45-4.44 9.89-9.9 9.89zm5.43-7.41c-.3-.15-1.76-.87-2.03-.97-.27-.1-.47-.15-.67.15-.2.3-.77.97-.94 1.16-.17.2-.35.22-.65.07-.3-.15-1.26-.46-2.4-1.48-.89-.79-1.49-1.77-1.66-2.07-.17-.3-.02-.46.13-.61.13-.13.3-.35.45-.52.15-.17.2-.3.3-.5.1-.2.05-.37-.02-.52-.08-.15-.67-1.61-.92-2.21-.24-.58-.49-.5-.67-.51h-.57c-.2 0-.52.07-.79.37-.27.3-1.04 1.02-1.04 2.48 0 1.46 1.07 2.88 1.22 3.08.15.2 2.1 3.2 5.08 4.49.71.31 1.26.49 1.69.63.71.23 1.36.2 1.87.12.57-.09 1.76-.72 2.01-1.41.25-.7.25-1.29.17-1.41-.07-.12-.27-.2-.57-.35z" />
                    </svg>
                    <span>${ctrl.escapeHtml(ctrl.t('btnWhatsapp'))}</span>
                  </button>

                  <button
                    type="button"
                    onclick="window.app.copiarMailForwarder(${idx})"
                    class="w-full flex items-center justify-center gap-2 ${copiado ? 'bg-[#3D0411] text-rose-100 ring-2 ring-[#530618]' : 'bg-[#530618] hover:bg-[#3D0411] text-white'} font-bold text-xs py-2.5 px-3 rounded-sm transition-colors shadow-sm cursor-pointer"
                  >
                    <svg class="w-4 h-4 shrink-0" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M3 8l7.89 5.26a2 2 0 002.22 0L21 8M5 19h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v10a2 2 0 002 2z" />
                    </svg>
                    <span>${ctrl.escapeHtml(copiado ? ctrl.t('mailCopiado') : ctrl.t('btnCopiarMail'))}</span>
                  </button>
                </div>
              </div>
            </div>
          `;
        }).join('')}
      </div>
    </div>
  `;
}

/**
 * Renderiza la Grilla de 4 Cuadrados de Despachantes de Aduana (Misma interfaz que Forwarders, mostrando solo Despachantes)
 */
function renderDespachantesGrid(ctrl) {
  const despachantes = ctrl.state.despachantesCatalogo || [];
  if (despachantes.length === 0) return '';

  return `
    <div class="bg-white border border-[#D8DDE3] rounded-sm p-6 shadow-sm">
      <div class="border-b border-[#F4F6F8] pb-3 mb-5 text-left">
        <span class="text-xs font-bold uppercase tracking-widest text-[#530618]">${ctrl.escapeHtml(ctrl.t('despTag'))}</span>
        <h3 class="text-xl md:text-2xl font-bold text-[#171717] mt-0.5">${ctrl.escapeHtml(ctrl.t('despTitulo'))}</h3>
        <p class="text-xs text-[#555555] mt-1">${ctrl.escapeHtml(ctrl.t('despSubtitulo'))}</p>
      </div>

      <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-5 text-left">
        ${despachantes.map((desp, idx) => {
          const nombreMax120 = String(desp.nombre || '').slice(0, 120);
          const copiado = ctrl.state.mailCopiadoId === desp.id;
          return `
            <div class="bg-[#FFF8F8] border border-[#F0D0D4] rounded-sm overflow-hidden shadow-sm hover:border-[#530618] transition-all flex flex-col justify-between">
              <div class="w-full aspect-square bg-gray-200 overflow-hidden">
                <img
                  src="${ctrl.escapeHtml(desp.imagen_url || 'logo-importa.png')}"
                  alt="${ctrl.escapeHtml(nombreMax120)}"
                  loading="lazy"
                  onerror="this.onerror=null;this.src='logo-importa.png';"
                  class="w-full h-full object-cover"
                />
              </div>

              <div class="p-4 flex flex-col flex-1 justify-between gap-3">
                <h4 class="text-sm font-bold text-[#171717] text-center leading-snug break-words" title="${ctrl.escapeHtml(nombreMax120)}">
                  ${ctrl.escapeHtml(nombreMax120)}
                </h4>

                <div class="space-y-2">
                  <button
                    type="button"
                    onclick="window.app.redirigirWhatsappDespachante(${idx})"
                    class="w-full flex items-center justify-center gap-2 bg-emerald-600 hover:bg-emerald-700 text-white font-bold text-xs py-2.5 px-3 rounded-sm transition-colors shadow-sm cursor-pointer"
                  >
                    <svg class="w-4 h-4 fill-current shrink-0" viewBox="0 0 24 24">
                      <path d="M20.52 3.48A11.82 11.82 0 0012.06 0C5.5 0 .16 5.34.16 11.9c0 2.1.55 4.15 1.6 5.95L0 24l6.32-1.66a11.86 11.86 0 005.74 1.46h.01c6.55 0 11.89-5.34 11.89-11.9 0-3.18-1.24-6.17-3.44-8.42zM12.07 21.8h-.01a9.87 9.87 0 01-5.03-1.38l-.36-.21-3.75.98 1-3.65-.24-.38a9.85 9.85 0 01-1.51-5.26c0-5.45 4.44-9.89 9.9-9.89 2.64 0 5.13 1.03 7 2.9a9.83 9.83 0 012.9 7c0 5.45-4.44 9.89-9.9 9.89zm5.43-7.41c-.3-.15-1.76-.87-2.03-.97-.27-.1-.47-.15-.67.15-.2.3-.77.97-.94 1.16-.17.2-.35.22-.65.07-.3-.15-1.26-.46-2.4-1.48-.89-.79-1.49-1.77-1.66-2.07-.17-.3-.02-.46.13-.61.13-.13.3-.35.45-.52.15-.17.2-.3.3-.5.1-.2.05-.37-.02-.52-.08-.15-.67-1.61-.92-2.21-.24-.58-.49-.5-.67-.51h-.57c-.2 0-.52.07-.79.37-.27.3-1.04 1.02-1.04 2.48 0 1.46 1.07 2.88 1.22 3.08.15.2 2.1 3.2 5.08 4.49.71.31 1.26.49 1.69.63.71.23 1.36.2 1.87.12.57-.09 1.76-.72 2.01-1.41.25-.7.25-1.29.17-1.41-.07-.12-.27-.2-.57-.35z" />
                    </svg>
                    <span>${ctrl.escapeHtml(ctrl.t('btnWhatsapp'))}</span>
                  </button>

                  <button
                    type="button"
                    onclick="window.app.copiarMailDespachante(${idx})"
                    class="w-full flex items-center justify-center gap-2 ${copiado ? 'bg-[#3D0411] text-rose-100 ring-2 ring-[#530618]' : 'bg-[#530618] hover:bg-[#3D0411] text-white'} font-bold text-xs py-2.5 px-3 rounded-sm transition-colors shadow-sm cursor-pointer"
                  >
                    <svg class="w-4 h-4 shrink-0" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M3 8l7.89 5.26a2 2 0 002.22 0L21 8M5 19h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v10a2 2 0 002 2z" />
                    </svg>
                    <span>${ctrl.escapeHtml(copiado ? ctrl.t('mailCopiado') : ctrl.t('btnCopiarMail'))}</span>
                  </button>
                </div>
              </div>
            </div>
          `;
        }).join('')}
      </div>
    </div>
  `;
}

class HeroState extends VistaState {
  render(ctrl) {
    return `
      <div class="h-screen w-full flex flex-col justify-between overflow-hidden" style="background-color: #420213;">
        <header class="px-6 py-3 md:py-4 flex items-center justify-between shrink-0">
          <div class="flex items-center cursor-pointer" onclick="window.app.setView('hero')">
            ${renderBrandBadge('h-6 md:h-7')}
          </div>
          ${renderHamburgerMenu(ctrl)}
        </header>

        <div class="flex-1 flex flex-col items-center justify-center text-center px-4 w-full min-h-0 py-1">
          <div class="w-full flex items-center justify-center mb-6">
            <img src="logo-importa.svg" alt="IMPORTA+" class="w-auto h-16 sm:h-20 md:h-24 max-w-[85vw] object-contain mx-auto block select-none" />
          </div>

          <p class="text-base sm:text-lg md:text-xl text-white/90 font-normal mb-7 max-w-3xl px-4">
            ${ctrl.escapeHtml(ctrl.t('heroSubtitle'))}
          </p>

          <button
            id="btn-acceder-calculadora"
            onclick="window.app.setView('form')"
            class="bg-white text-[#420213] font-bold text-xs md:text-sm uppercase tracking-widest px-8 md:px-10 py-3.5 rounded-sm hover:bg-white/90 transition-all duration-200 shadow-xl hover:scale-[1.02] active:scale-100"
          >
            ${ctrl.escapeHtml(ctrl.t('btnCalcular'))}
          </button>
        </div>

        <div class="py-2 shrink-0"></div>
      </div>
    `;
  }
}

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
          <div class="flex items-center gap-3">
            <span class="text-white/70 text-xs font-semibold uppercase tracking-widest hidden sm:inline">${ctrl.escapeHtml(ctrl.t('paso1Corto'))}</span>
            ${renderHamburgerMenu(ctrl)}
          </div>
        </header>

        <div class="max-w-2xl mx-auto px-4 py-10">
          <h2 class="text-2xl md:text-3xl font-bold text-[#171717] mb-1">${ctrl.escapeHtml(ctrl.t('tituloLiquidacion'))}</h2>
          <p class="text-[#555555] text-sm mb-8">${ctrl.escapeHtml(ctrl.t('subtituloForm'))}</p>

          <div class="bg-white rounded-sm border border-[#D8DDE3] divide-y divide-[#F4F6F8] overflow-hidden mb-6 shadow-sm">
            <div class="px-6 py-5">
              <div class="flex items-center gap-2 mb-4">
                <p class="text-xs font-bold uppercase tracking-widest text-[#530618]">${ctrl.escapeHtml(ctrl.t('infoGeneral'))}</p>
              </div>

              <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
                <div class="relative md:col-span-2">
                  <label class="block text-xs font-semibold uppercase tracking-wider text-[#555555] mb-1.5">
                    ${ctrl.escapeHtml(ctrl.t('lblProducto'))}
                  </label>
                  <input
                    type="text"
                    id="input-producto"
                    autocomplete="off"
                    class="w-full bg-white border border-[#D8DDE3] rounded-sm px-4 py-3 text-[#171717] text-sm focus:outline-none focus:border-[#530618] focus:ring-1 focus:ring-[#530618]"
                    placeholder="${ctrl.escapeHtml(ctrl.t('phProducto'))}"
                    value="${ctrl.escapeHtml(f.producto || '')}"
                  />
                  <div id="ncm-dropdown" class="absolute left-0 right-0 top-full mt-1 bg-white border border-[#D8DDE3] rounded-sm shadow-xl z-50 max-h-60 overflow-y-auto hidden"></div>

                  <div id="ficha-producto-seleccionado" class="${ctrl.state.posicionSeleccionada ? '' : 'hidden'} mt-2.5 p-3 bg-[#FFF8F8] border border-[#F0D0D4] rounded-sm text-xs">
                    ${ctrl.state.posicionSeleccionada ? `
                      <div class="flex flex-wrap justify-between items-center gap-2 font-bold text-[#530618]">
                        <span>${ctrl.escapeHtml(ctrl.state.posicionSeleccionada.categoria || ctrl.t('prodSeleccionado'))}</span>
                        <div class="flex items-center gap-2">
                          <span class="bg-[#530618] text-white px-2 py-0.5 rounded-sm text-[10px]">${ctrl.escapeHtml(ctrl.t('arancelBase'))}: ${ctrl.escapeHtml(ctrl.state.posicionSeleccionada.arancelExternoComun)}%</span>
                          <button
                            type="button"
                            onclick="window.app.toggleDescripcionProducto()"
                            id="btn-texto-descripcion-producto"
                            class="text-[11px] font-bold text-[#530618] bg-white border border-[#F0D0D4] px-2.5 py-0.5 rounded-sm hover:bg-[#530618] hover:text-white transition-colors"
                          >
                            ${ctrl.escapeHtml(ctrl.state.mostrarDescripcionProducto ? ctrl.t('ocultarDesc') : ctrl.t('verDesc'))}
                          </button>
                        </div>
                      </div>
                      <div id="panel-descripcion-producto" class="${ctrl.state.mostrarDescripcionProducto ? '' : 'hidden'} mt-2 pt-2 border-t border-[#F0D0D4]">
                        <p class="text-[#555] leading-relaxed">${ctrl.escapeHtml(ctrl.state.posicionSeleccionada.descripcion)}</p>
                        ${ctrl.state.posicionSeleccionada.poseeExcepcion ? `
                          <div class="mt-2 pt-2 border-t border-[#F0D0D4] flex items-center justify-between text-[#15803d]">
                            <span>${ctrl.escapeHtml(ctrl.t('beneficioDisponible'))}</span>
                            <span class="font-bold">${ctrl.escapeHtml(ctrl.t('arancelReducido'))} ${ctrl.escapeHtml(ctrl.state.posicionSeleccionada.arancelAplicableExcepcion)}%</span>
                          </div>
                        ` : ''}
                      </div>
                    ` : ''}
                  </div>
                </div>

                <div class="relative">
                  <label class="block text-xs font-semibold uppercase tracking-wider text-[#555555] mb-1.5">${ctrl.escapeHtml(ctrl.t('lblPais'))}</label>
                  <input
                    type="text"
                    id="input-pais"
                    autocomplete="off"
                    class="w-full bg-white border border-[#D8DDE3] rounded-sm px-4 py-3 text-[#171717] text-sm focus:outline-none focus:border-[#530618] focus:ring-1 focus:ring-[#530618]"
                    placeholder="${ctrl.escapeHtml(ctrl.t('phPais'))}"
                    value="${ctrl.escapeHtml(f.paisOrigen || '')}"
                  />
                  <div id="pais-dropdown" class="absolute left-0 right-0 top-full mt-1 bg-white border border-[#D8DDE3] rounded-sm shadow-xl z-50 max-h-60 overflow-y-auto hidden"></div>
                </div>

                <div>
                  <label class="block text-xs font-semibold uppercase tracking-wider text-[#555555] mb-1.5">${ctrl.escapeHtml(ctrl.t('lblCantidad'))}</label>
                  <input
                    type="number"
                    id="input-cantidad"
                    min="0"
                    onfocus="if(this.value==='0') this.select()"
                    class="w-full bg-white border border-[#D8DDE3] rounded-sm px-4 py-3 text-[#171717] text-sm focus:outline-none focus:border-[#530618]"
                    value="${ctrl.escapeHtml(f.cantidad)}"
                  >
                </div>

                <div class="md:col-span-2">
                  <label class="block text-xs font-semibold uppercase tracking-wider text-[#555555] mb-1.5">${ctrl.escapeHtml(ctrl.t('lblFobUnitario'))}</label>
                  <div class="relative">
                    <span class="absolute left-3.5 top-1/2 -translate-y-1/2 text-[#999] text-sm font-medium">$</span>
                    <input
                      type="number"
                      step="0.01"
                      min="0"
                      id="input-precio"
                      onfocus="if(this.value==='0' || this.value==='0.00') this.select()"
                      class="w-full bg-white border border-[#D8DDE3] rounded-sm pl-8 pr-4 py-3 text-[#171717] text-sm focus:outline-none focus:border-[#530618]"
                      value="${ctrl.escapeHtml(f.precioUnidad)}"
                    >
                  </div>
                </div>
              </div>

              <div class="mt-4 bg-[#F4F6F8] border border-[#D8DDE3] rounded-sm px-4 py-3 flex items-center justify-between">
                <div>
                  <p class="text-xs font-bold uppercase tracking-wider text-[#555555]">${ctrl.escapeHtml(ctrl.t('lblFobTotalVivo'))}</p>
                  <p id="subtotal-fob-formula" class="text-[11px] text-[#530618] font-semibold mt-0.5">
                    ${cantNum} ${ctrl.escapeHtml(ctrl.t('unidAbrev'))} × USD ${ctrl.formatNumber(precioNum)} = USD ${ctrl.formatNumber(subtotalFOB)}
                  </p>
                </div>
                <span id="subtotal-fob-monto" class="text-lg font-bold text-[#530618] tabular-nums">USD ${ctrl.formatNumber(subtotalFOB)}</span>
              </div>
            </div>

            <div class="px-6 py-5">
              <div class="flex flex-wrap items-start justify-between gap-2 mb-2">
                <div class="flex items-center gap-2 pt-1">
                  <p class="text-xs font-bold uppercase tracking-widest text-[#530618]">${ctrl.escapeHtml(ctrl.t('costosAdicionales'))}</p>
                  <span class="text-[10px] font-semibold border border-[#D8DDE3] text-[#555555] px-1.5 py-0.5 rounded-sm uppercase tracking-wide">${ctrl.escapeHtml(ctrl.t('opcional'))}</span>
                </div>
                <div class="flex flex-col items-stretch sm:items-end gap-1.5">
                  <button type="button" onclick="window.app.setView('contact')" class="bg-[#530618] hover:bg-[#3D0411] text-white text-xs font-semibold px-3 py-1.5 rounded-sm transition-colors duration-200 shadow-sm cursor-pointer">
                    ${ctrl.escapeHtml(ctrl.t('btnContactarForwarder'))}
                  </button>
                  <button type="button" onclick="window.app.setView('despachantes')" class="bg-[#530618] hover:bg-[#3D0411] text-white text-xs font-semibold px-3 py-1.5 rounded-sm transition-colors duration-200 shadow-sm cursor-pointer">
                    ${ctrl.escapeHtml(ctrl.t('btnContactarDespachante'))}
                  </button>
                </div>
              </div>

              <div class="grid grid-cols-1 md:grid-cols-2 gap-4 mt-3">
                <div>
                  <label class="block text-xs font-semibold uppercase tracking-wider text-[#555555] mb-1.5">${ctrl.escapeHtml(ctrl.t('lblFlete'))}</label>
                  <input
                    type="number"
                    min="0"
                    step="0.01"
                    id="input-flete"
                    onfocus="if(this.value==='0') this.select()"
                    class="w-full bg-white border border-[#D8DDE3] rounded-sm px-4 py-3 text-[#171717] text-sm focus:outline-none focus:border-[#530618]"
                    value="${ctrl.escapeHtml(f.flete)}"
                  >
                </div>
                <div>
                  <label class="block text-xs font-semibold uppercase tracking-wider text-[#555555] mb-1.5">${ctrl.escapeHtml(ctrl.t('lblSeguro'))}</label>
                  <input
                    type="number"
                    min="0"
                    step="0.01"
                    id="input-seguro"
                    onfocus="if(this.value==='0') this.select()"
                    class="w-full bg-white border border-[#D8DDE3] rounded-sm px-4 py-3 text-[#171717] text-sm focus:outline-none focus:border-[#530618]"
                    value="${ctrl.escapeHtml(f.seguro)}"
                  >
                </div>
              </div>
            </div>

            <div class="px-6 py-4 bg-[#fafbfc]">
              <label class="block text-xs font-semibold uppercase tracking-wider text-[#555555] mb-2">
                ${ctrl.escapeHtml(ctrl.t('lblCondicionImpositiva'))}
              </label>
              <div class="flex flex-wrap gap-6 text-xs text-[#171717]">
                <label class="flex items-center gap-2 cursor-pointer font-medium">
                  <input type="radio" name="rad-ganancias" value="inscripto" ${f.condicionGanancias === 'inscripto' ? 'checked' : ''} onchange="window.app.state.form.condicionGanancias = 'inscripto'">
                  ${ctrl.escapeHtml(ctrl.t('condInscripto'))}
                </label>
                <label class="flex items-center gap-2 cursor-pointer font-medium">
                  <input type="radio" name="rad-ganancias" value="no_inscripto" ${f.condicionGanancias === 'no_inscripto' ? 'checked' : ''} onchange="window.app.state.form.condicionGanancias = 'no_inscripto'">
                  ${ctrl.escapeHtml(ctrl.t('condConsumo'))}
                </label>
              </div>
            </div>
          </div>

          <button onclick="window.app.ejecutarLiquidacion()" class="w-full bg-[#530618] hover:bg-[#3D0411] text-white font-bold text-sm uppercase tracking-widest py-4 rounded-sm transition-colors duration-200 shadow-md">
            ${ctrl.escapeHtml(ctrl.t('btnCalcular'))}
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

    const ejecutarBusquedaProducto = async (texto) => {
      if (!dropdownNcm) return;
      const cleanVal = String(texto || '').trim();
      if (cleanVal.length >= 2) {
        const res = await ctrl.posicionBLL.buscarPosiciones(cleanVal, 10);
        if (res && res.length > 0) {
          dropdownNcm.innerHTML = res.map((p, idx) => `
            <div class="p-3 border-b border-gray-100 hover:bg-[#FFF8F8] cursor-pointer text-xs" onmousedown="event.preventDefault(); window.app.seleccionarNCM(${idx})">
              <div class="flex justify-between font-bold text-[#530618]">
                <span>${ctrl.escapeHtml(p.categoria || ctrl.t('prodSeleccionado'))}</span>
                <span class="text-gray-500 font-normal">${ctrl.escapeHtml(ctrl.t('arancel'))}: ${ctrl.escapeHtml(p.poseeExcepcion ? p.arancelAplicableExcepcion : p.arancelExternoComun)}%</span>
              </div>
              <div class="text-gray-700 mt-0.5 font-medium">${ctrl.escapeHtml(p.descripcion)}</div>
            </div>
          `).join('');
          dropdownNcm.classList.remove('hidden');
          window._ncmCache = res;
        } else {
          dropdownNcm.innerHTML = `
            <div class="p-3 text-xs text-gray-500">
              ${ctrl.escapeHtml(ctrl.t('sinProductos'))}
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

    const actualizarSugerenciasPais = async (texto) => {
      if (!dropdownPais) return;
      const res = await ctrl.paisBLL.buscarPaises(texto, 15);
      if (res && res.length > 0) {
        dropdownPais.innerHTML = res.map((p, idx) => `
          <div class="p-2.5 border-b border-gray-100 hover:bg-[#FFF8F8] cursor-pointer text-xs flex justify-between items-center" onmousedown="event.preventDefault(); window.app.seleccionarPais(${idx})">
            <span class="font-semibold text-[#171717]">${ctrl.escapeHtml(p.nombre)}</span>
            <span class="text-[10px] px-2 py-0.5 rounded-sm ${p.esIntrazona ? 'bg-emerald-50 text-emerald-700 font-bold' : 'bg-gray-100 text-gray-600'}">
              ${ctrl.escapeHtml(p.esIntrazona ? ctrl.t('mercosurCero') : ctrl.t('extrazona'))}
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
      inputPais.addEventListener('focus', (e) => actualizarSugerenciasPais(e.target.value));
      inputPais.addEventListener('click', (e) => actualizarSugerenciasPais(e.target.value));
      inputPais.addEventListener('blur', () => {
        if (dropdownPais) dropdownPais.classList.add('hidden');
      });
    }

    const actualizarSubtotalEnVivo = () => {
      const cantNum = parseInt(ctrl.state.form.cantidad) || 0;
      const precioNum = parseFloat(ctrl.state.form.precioUnidad) || 0;
      const subtotal = cantNum * precioNum;
      const elFormula = document.getElementById('subtotal-fob-formula');
      const elMonto = document.getElementById('subtotal-fob-monto');
      if (elFormula) {
        elFormula.textContent = `${cantNum} ${ctrl.t('unidAbrev')} × USD ${ctrl.formatNumber(precioNum)} = USD ${ctrl.formatNumber(subtotal)}`;
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

class ResultadosState extends VistaState {
  render(ctrl) {
    const be = ctrl.state.calculadoraBE;
    if (!be) return '';

    const items = [
      { label: ctrl.t('itemFob'), value: be.fobTotal },
      { label: ctrl.t('itemFlete'), value: be.flete },
      { label: ctrl.t('itemSeguro'), value: be.seguro },
      { label: `${ctrl.t('itemDie')} (${be.alicuotaDie}%)`, value: be.montoDie },
      { label: ctrl.t('impuestosDga'), value: (be.montoIva + be.montoIvaAdicional + be.montoGanancias) },
      { label: ctrl.t('cargosSimEst'), value: (be.montoEstadistica + be.totalSimDigitalizacion) }
    ];

    return `
      <div class="min-h-screen bg-[#F4F6F8]">
        <header class="bg-[#420213] px-6 py-4 flex items-center justify-between shadow-sm">
          <div class="flex items-center cursor-pointer" onclick="window.app.setView('hero')">
            ${renderBrandBadge('h-6 md:h-7')}
          </div>
          <div class="flex items-center gap-3">
            <button onclick="window.app.setView('form')" class="text-white/70 text-xs font-medium hover:text-white transition-colors">
              ${ctrl.escapeHtml(ctrl.t('nuevaConsulta'))}
            </button>
            ${renderHamburgerMenu(ctrl)}
          </div>
        </header>

        <div class="max-w-2xl mx-auto px-4 py-10">
          <div class="flex flex-wrap items-center justify-between gap-2 mb-2">
            <span class="text-xs font-bold uppercase tracking-widest text-[#530618]">${ctrl.escapeHtml(ctrl.t('tituloLiquidacion'))}</span>
            <button
              type="button"
              id="btn-texto-descripcion-producto"
              onclick="window.app.toggleDescripcionProducto()"
              class="text-xs font-bold text-[#530618] bg-white border border-[#F0D0D4] px-3 py-1 rounded-sm hover:bg-[#530618] hover:text-white transition-colors shadow-sm"
            >
              ${ctrl.escapeHtml(ctrl.state.mostrarDescripcionProducto ? ctrl.t('ocultarDesc') : ctrl.t('verDesc'))}
            </button>
          </div>

          <h2 class="text-2xl font-bold text-[#171717] mb-1">${ctrl.escapeHtml(be.posicion?.categoria || ctrl.t('prodSeleccionado'))}</h2>
          <p class="text-[#555555] text-sm mb-3">${ctrl.escapeHtml(ctrl.t('origen'))}: ${ctrl.escapeHtml(be.pais?.nombre)} · ${be.cantidad} ${ctrl.escapeHtml(ctrl.t('unidadesA'))} USD ${ctrl.formatNumber(be.precioFobUnitario)}</p>

          <div id="panel-descripcion-producto" class="${ctrl.state.mostrarDescripcionProducto ? '' : 'hidden'} mb-5 p-4 bg-white border border-[#F0D0D4] rounded-sm text-xs text-[#555555] shadow-sm">
            <p class="font-bold text-[#530618] uppercase tracking-wider mb-1">${ctrl.escapeHtml(ctrl.t('descProductoTitulo'))}</p>
            <p class="leading-relaxed text-[#171717]">${ctrl.escapeHtml(be.posicion?.descripcion || '')}</p>
          </div>

          <div class="bg-[#530618] rounded-sm p-6 mb-4 relative overflow-hidden shadow-lg">
            <p class="text-white/70 text-xs font-bold uppercase tracking-widest mb-1">${ctrl.escapeHtml(ctrl.t('costoTotalAduana'))}</p>
            <p class="text-4xl md:text-5xl font-bold text-white tracking-tight">USD ${ctrl.formatNumber(be.costoTotalNacionalizado)}</p>
            <p class="text-white/80 text-xs mt-2 font-medium">≈ $ ${ctrl.formatNumber(be.costoTotalARS)} ARS <span class="opacity-60">(BNA: $${be.cotizacionBna})</span></p>
            <div class="mt-4 pt-3 border-t border-white/20 flex justify-between text-xs text-white/90">
              <span>${ctrl.escapeHtml(ctrl.t('costoUnitarioNac'))}</span>
              <strong class="font-mono text-sm">USD ${ctrl.formatNumber(be.costoUnitarioNacionalizado)} / ${ctrl.escapeHtml(ctrl.t('unidad'))}</strong>
            </div>
          </div>

          <div class="bg-white border border-[#D8DDE3] rounded-sm p-4 mb-4 shadow-sm">
            <div class="flex justify-between items-center text-xs font-bold text-[#555] mb-2 uppercase">
              <span>${ctrl.escapeHtml(ctrl.t('incidenciaAduanera'))}</span>
              <span class="text-[#530618]">+${be.ratioSobrecostoPct}% ${ctrl.escapeHtml(ctrl.t('sobreFob'))}</span>
            </div>
            <div class="grid grid-cols-2 gap-3 text-center">
              <div class="p-3 bg-gray-50 rounded-sm border border-gray-100">
                <span class="text-[11px] text-gray-400 font-bold uppercase">${ctrl.escapeHtml(ctrl.t('compraFob'))}</span>
                <p class="text-lg font-black text-gray-800">USD ${ctrl.formatNumber(be.fobTotal)}</p>
              </div>
              <div class="p-3 bg-[#FFF8F8] rounded-sm border border-[#F0D0D4]">
                <span class="text-[11px] text-[#530618] font-bold uppercase">${ctrl.escapeHtml(ctrl.t('desembolsoFinal'))}</span>
                <p class="text-lg font-black text-[#530618]">USD ${ctrl.formatNumber(be.costoTotalNacionalizado)}</p>
              </div>
            </div>
          </div>

          <div class="bg-white border border-[#D8DDE3] rounded-sm divide-y divide-[#F4F6F8] mb-4 shadow-sm">
            ${items.map((it) => `
              <div class="flex items-center justify-between px-5 py-3.5">
                <span class="text-sm text-[#555555]">${ctrl.escapeHtml(it.label)}</span>
                <span class="text-sm font-semibold text-[#171717] tabular-nums">USD ${ctrl.formatNumber(it.value)}</span>
              </div>
            `).join('')}
          </div>

          <div class="print:hidden">
            <button onclick="window.app.setView('breakdown')" class="w-full bg-[#530618] hover:bg-[#3D0411] text-white font-bold text-sm uppercase tracking-widest py-4 rounded-sm transition-colors duration-200 mb-3 shadow-md cursor-pointer">
              ${ctrl.escapeHtml(ctrl.t('btnVerDesglose'))}
            </button>
            <button onclick="window.app.setView('contact')" class="w-full bg-[#3D0411] hover:bg-[#530618] text-white font-bold text-xs uppercase tracking-widest py-3.5 rounded-sm transition-colors duration-200 mb-2 shadow-sm cursor-pointer">
              ${ctrl.escapeHtml(ctrl.t('btnContactarForwarder'))}
            </button>
            <button onclick="window.app.setView('despachantes')" class="w-full bg-[#3D0411] hover:bg-[#530618] text-white font-bold text-xs uppercase tracking-widest py-3.5 rounded-sm transition-colors duration-200 mb-3 shadow-sm cursor-pointer">
              ${ctrl.escapeHtml(ctrl.t('btnContactarDespachante'))}
            </button>
            <div class="grid grid-cols-1 sm:grid-cols-2 gap-3 mb-3">
              <button onclick="window.print()" class="w-full bg-[#530618] hover:bg-[#3D0411] text-white font-bold text-xs uppercase tracking-widest py-3.5 rounded-sm transition-colors duration-200 shadow-md flex items-center justify-center gap-2 cursor-pointer">
                ${ctrl.escapeHtml(ctrl.t('btnImprimirPdf'))}
              </button>
              <button onclick="window.ExcelDataLoaderDAL && window.ExcelDataLoaderDAL.exportarComprobanteExcel(window.app.state.calculadoraBE)" class="w-full bg-emerald-700 hover:bg-emerald-800 text-white font-bold text-xs uppercase tracking-widest py-3.5 rounded-sm transition-colors duration-200 shadow-md flex items-center justify-center cursor-pointer">
                ${ctrl.escapeHtml(ctrl.t('btnExportarExcel'))}
              </button>
            </div>
            <button onclick="window.app.setView('form')" class="w-full border border-[#530618] text-[#530618] hover:bg-[#530618] hover:text-white font-semibold text-sm py-3.5 rounded-sm transition-colors duration-200 cursor-pointer">
              ${ctrl.escapeHtml(ctrl.t('nuevaConsulta'))}
            </button>
          </div>
        </div>
      </div>
    `;
  }
}

class DesgloseState extends VistaState {
  render(ctrl) {
    const be = ctrl.state.calculadoraBE;
    if (!be) return '';

    const items = [
      { label: ctrl.t('itemFob'), value: be.fobTotal, note: ctrl.t('noteFob'), highlight: false },
      { label: ctrl.t('itemFlete'), value: be.flete, note: ctrl.t('noteFlete'), highlight: false },
      { label: ctrl.t('itemSeguro'), value: be.seguro, note: ctrl.t('noteSeguro'), highlight: false },
      { label: ctrl.t('itemCif'), value: be.cifAduanero, note: ctrl.t('noteCif'), highlight: true },
      { label: `${ctrl.t('itemDie')} (${be.alicuotaDie}%)`, value: be.montoDie, note: be.tratamientoArancelario, highlight: false },
      { label: `${ctrl.t('itemEstadistica')} (${be.alicuotaEstadistica}%)`, value: be.montoEstadistica, note: (be.pais?.esIntrazona || be.posicion?.esBienDeCapital) ? ctrl.t('noteEstadisticaExento') : ctrl.t('noteEstadisticaGral'), highlight: false },
      { label: `${ctrl.t('itemIva')} (${be.alicuotaIva}%)`, value: be.montoIva, note: ctrl.t('noteIva'), highlight: false },
      { label: `${ctrl.t('itemIvaAdic')} (${be.alicuotaIvaAdicional}%)`, value: be.montoIvaAdicional, note: ctrl.t('noteIvaAdic'), highlight: false },
      { label: `${ctrl.t('itemGanancias')} (${be.alicuotaGanancias}%)`, value: be.montoGanancias, note: be.esInscriptoGanancias ? ctrl.t('condInscripto') : ctrl.t('condConsumo'), highlight: false },
      { label: ctrl.t('itemSim'), value: be.totalSimDigitalizacion, note: ctrl.t('noteSim'), highlight: false }
    ];

    return `
      <div class="min-h-screen bg-[#F4F6F8] print:bg-white print:min-h-0">
        <header class="bg-[#420213] px-6 py-4 flex items-center justify-between shadow-sm print:hidden">
          <div class="flex items-center cursor-pointer" onclick="window.app.setView('hero')">
            ${renderBrandBadge('h-6 md:h-7')}
          </div>
          <div class="flex items-center gap-3">
            <button onclick="window.print()" class="flex items-center gap-1.5 bg-white/10 hover:bg-white/20 text-white text-xs font-semibold px-3 py-1.5 rounded-sm transition-colors border border-white/20 cursor-pointer">
              ${ctrl.escapeHtml(ctrl.t('btnImprimirDesglose'))}
            </button>
            <button onclick="window.app.setView('results')" class="text-white/70 text-xs font-medium hover:text-white transition-colors cursor-pointer">
              ${ctrl.escapeHtml(ctrl.t('volverResultados'))}
            </button>
            ${renderHamburgerMenu(ctrl)}
          </div>
        </header>

        <div class="max-w-2xl mx-auto px-4 py-10 print:py-0 print:px-0 print:max-w-full">
          <div class="flex flex-col sm:flex-row sm:items-start justify-between gap-4 mb-4">
            <div>
              <span class="text-xs font-bold uppercase tracking-widest text-[#530618]">${ctrl.escapeHtml(ctrl.t('desgloseSubtitulo'))}</span>
              <h2 class="text-2xl font-bold text-[#171717] mt-1">${ctrl.escapeHtml(be.posicion?.categoria || ctrl.t('prodSeleccionado'))}</h2>
              <div class="flex flex-wrap gap-x-4 gap-y-1 text-xs text-[#555555] mt-1.5 font-medium">
                <span>${ctrl.escapeHtml(ctrl.t('origen'))}: <strong class="text-[#171717]">${ctrl.escapeHtml(be.pais?.nombre || '')}</strong></span>
                <span>${ctrl.escapeHtml(ctrl.t('condicion'))}: <strong class="text-[#171717]">${ctrl.escapeHtml(be.esInscriptoGanancias ? ctrl.t('condInscripto') : ctrl.t('condConsumo'))}</strong></span>
              </div>
            </div>
            <div class="flex flex-wrap gap-2 print:hidden shrink-0">
              <button
                type="button"
                id="btn-texto-descripcion-producto"
                onclick="window.app.toggleDescripcionProducto()"
                class="inline-flex items-center gap-1.5 bg-white border border-[#530618] text-[#530618] hover:bg-[#530618] hover:text-white px-3.5 py-2 rounded-sm text-xs font-bold transition-all cursor-pointer"
              >
                ${ctrl.escapeHtml(ctrl.state.mostrarDescripcionProducto ? ctrl.t('ocultarDesc') : ctrl.t('verDesc'))}
              </button>
            </div>
          </div>

          <div id="panel-descripcion-producto" class="${ctrl.state.mostrarDescripcionProducto ? '' : 'hidden'} mb-5 p-4 bg-white border border-[#F0D0D4] rounded-sm text-xs text-[#555555] shadow-sm">
            <p class="font-bold text-[#530618] uppercase tracking-wider mb-1">${ctrl.escapeHtml(ctrl.t('descProductoTitulo'))}</p>
            <p class="leading-relaxed text-[#171717]">${ctrl.escapeHtml(be.posicion?.descripcion || '')}</p>
          </div>

          <div class="bg-white border border-[#D8DDE3] rounded-sm overflow-hidden mb-6 shadow-sm print:shadow-none print:border-gray-300">
            ${items.map((it) => `
              <div class="flex items-center justify-between px-5 py-4 border-b border-[#F4F6F8] print:border-gray-200 last:border-0 ${it.highlight ? 'bg-[#FFF8F8] print:bg-gray-50' : ''}">
                <div>
                  <p class="text-sm text-[#171717] ${it.highlight ? 'font-semibold' : 'font-normal'}">${ctrl.escapeHtml(it.label)}</p>
                  <p class="text-xs text-[#999] mt-0.5 print:text-gray-500">${ctrl.escapeHtml(it.note)}</p>
                </div>
                <span class="text-sm font-bold tabular-nums ${it.highlight ? 'text-[#530618]' : 'text-[#171717]'}">
                  USD ${ctrl.formatNumber(it.value)}
                </span>
              </div>
            `).join('')}

            <div class="flex items-center justify-between px-5 py-5 bg-[#530618] print:bg-[#530618] text-white">
              <p class="text-white/80 text-xs uppercase tracking-wider font-semibold">${ctrl.escapeHtml(ctrl.t('costoTotalNac'))}</p>
              <span class="text-xl font-bold text-white tabular-nums">USD ${ctrl.formatNumber(be.costoTotalNacionalizado)}</span>
            </div>
          </div>

          <div class="print:hidden space-y-3">
            <button onclick="window.app.setView('ports')" class="w-full border border-[#530618] text-[#530618] hover:bg-[#530618] hover:text-white font-bold text-sm uppercase tracking-widest py-3.5 rounded-sm transition-colors duration-200 cursor-pointer">
              ${ctrl.escapeHtml(ctrl.t('verPuertos'))}
            </button>
            <button onclick="window.app.setView('results')" class="w-full border border-[#D8DDE3] text-[#555555] hover:border-[#530618] hover:text-[#530618] font-semibold text-sm py-3.5 rounded-sm transition-colors duration-200 cursor-pointer">
              ${ctrl.escapeHtml(ctrl.t('volverResultados'))}
            </button>
          </div>
        </div>
      </div>
    `;
  }
}

class PuertosState extends VistaState {
  render(ctrl) {
    const puertos = ctrl.state.puertosCatalogo || [];
    return `
      <div class="min-h-screen bg-[#F4F6F8]">
        <header class="bg-[#420213] px-6 py-4 flex items-center justify-between shadow-sm">
          <div class="flex items-center cursor-pointer" onclick="window.app.setView('hero')">
            ${renderBrandBadge('h-6 md:h-7')}
          </div>
          <div class="flex items-center gap-3">
            <button onclick="window.app.setView('breakdown')" class="text-white/70 text-xs font-medium hover:text-white transition-colors">
              ${ctrl.escapeHtml(ctrl.t('volverDesglose'))}
            </button>
            ${renderHamburgerMenu(ctrl)}
          </div>
        </header>

        <div class="max-w-5xl mx-auto px-4 py-10 space-y-8">
          <div>
            <span class="text-xs font-bold uppercase tracking-widest text-[#530618]">${ctrl.escapeHtml(ctrl.t('pn3Tag'))}</span>
            <h2 class="text-2xl md:text-3xl font-bold text-[#171717] mt-1 mb-2">${ctrl.escapeHtml(ctrl.t('puertosTitulo'))}</h2>
            <p class="text-[#555] text-sm mb-6">${ctrl.escapeHtml(ctrl.t('puertosSubtitulo'))}</p>

            <div id="map" class="h-96 w-full rounded-sm border border-[#D8DDE3] mb-6 shadow-sm z-10"></div>

            <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
              ${puertos.map((p) => `
                <a
                  href="${ctrl.escapeHtml(p.google_maps_url || 'https://www.google.com/maps')}"
                  target="_blank"
                  rel="noopener noreferrer"
                  class="block bg-white border border-[#D8DDE3] rounded-sm p-4 shadow-sm hover:border-[#530618] hover:shadow-md transition-all cursor-pointer group"
                >
                  <div class="flex justify-between items-center mb-1">
                    <span class="text-xs font-bold text-[#530618] uppercase">${ctrl.escapeHtml(p.tipo || p.type)}</span>
                    <span class="text-[10px] text-gray-400 font-semibold">${ctrl.escapeHtml(p.operador)}</span>
                  </div>
                  <div class="flex items-center justify-between gap-2 mb-1">
                    <h4 class="font-bold text-[#171717] group-hover:text-[#530618] text-base transition-colors">${ctrl.escapeHtml(p.nombre || p.name)}</h4>
                    <span class="text-[11px] font-bold text-[#530618] shrink-0">Google Maps &#8599;</span>
                  </div>
                  <p class="text-xs text-[#555]">${ctrl.escapeHtml(p.descripcion || p.desc)}</p>
                </a>
              `).join('')}
            </div>
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
 * Estado 6: Apartado FORWARDERS - 4 cuadrados estilo grilla (Foto + Nombre <= 120 chars + WhatsApp + Mail abajo)
 */
class ContactoState extends VistaState {
  render(ctrl) {
    return `
      <div class="min-h-screen bg-[#F4F6F8]">
        <header class="bg-[#420213] px-6 py-4 flex items-center justify-between shadow-sm">
          <div class="flex items-center cursor-pointer" onclick="window.app.setView('hero')">
            ${renderBrandBadge('h-6 md:h-7')}
          </div>
          <div class="flex items-center gap-3">
            <button onclick="window.app.state.calculadoraBE ? window.app.setView('results') : window.app.setView('form')" class="text-white/70 text-xs font-medium hover:text-white transition-colors">
              ${ctrl.escapeHtml(ctrl.t('volver'))}
            </button>
            ${renderHamburgerMenu(ctrl)}
          </div>
        </header>

        <div class="max-w-5xl mx-auto px-4 py-10 space-y-6">
          ${renderForwardersGrid(ctrl)}

          <div class="text-center pt-2">
            <button
              onclick="window.app.state.calculadoraBE ? window.app.setView('results') : window.app.setView('form')"
              class="inline-flex items-center justify-center bg-[#530618] hover:bg-[#3D0411] text-white font-bold text-xs uppercase tracking-widest px-8 py-3.5 rounded-sm transition-colors shadow cursor-pointer"
            >
              ${ctrl.escapeHtml(ctrl.t('volver'))}
            </button>
          </div>
        </div>
      </div>
    `;
  }
}

/**
 * Estado Despachantes de Aduana - Misma interfaz de 4 cuadrados estilo grilla mostrando únicamente los Despachantes de Aduana
 */
class DespachantesState extends VistaState {
  render(ctrl) {
    return `
      <div class="min-h-screen bg-[#F4F6F8]">
        <header class="bg-[#420213] px-6 py-4 flex items-center justify-between shadow-sm">
          <div class="flex items-center cursor-pointer" onclick="window.app.setView('hero')">
            ${renderBrandBadge('h-6 md:h-7')}
          </div>
          <div class="flex items-center gap-3">
            <button onclick="window.app.state.calculadoraBE ? window.app.setView('results') : window.app.setView('form')" class="text-white/70 text-xs font-medium hover:text-white transition-colors">
              ${ctrl.escapeHtml(ctrl.t('volver'))}
            </button>
            ${renderHamburgerMenu(ctrl)}
          </div>
        </header>

        <div class="max-w-5xl mx-auto px-4 py-10 space-y-6">
          ${renderDespachantesGrid(ctrl)}

          <div class="text-center pt-2">
            <button
              onclick="window.app.state.calculadoraBE ? window.app.setView('results') : window.app.setView('form')"
              class="inline-flex items-center justify-center bg-[#530618] hover:bg-[#3D0411] text-white font-bold text-xs uppercase tracking-widest px-8 py-3.5 rounded-sm transition-colors shadow cursor-pointer"
            >
              ${ctrl.escapeHtml(ctrl.t('volver'))}
            </button>
          </div>
        </div>
      </div>
    `;
  }
}

class PlanesState extends VistaState {
  render(ctrl) {
    return `
      <div class="min-h-screen bg-[#F4F6F8]">
        <header class="bg-[#420213] px-6 py-4 flex items-center justify-between shadow-sm">
          <div class="flex items-center cursor-pointer" onclick="window.app.setView('hero')">
            ${renderBrandBadge('h-6 md:h-7')}
          </div>
          <div class="flex items-center gap-3">
            <button onclick="window.app.setView('hero')" class="text-white/70 text-xs font-medium hover:text-white transition-colors">
              ${ctrl.escapeHtml(ctrl.t('volver'))}
            </button>
            ${renderHamburgerMenu(ctrl)}
          </div>
        </header>
        <div class="max-w-4xl mx-auto px-4 py-12 text-center">
          <button onclick="window.app.setView('form')" class="bg-[#530618] text-white font-bold text-xs uppercase px-8 py-3.5 rounded-sm hover:bg-[#3D0411] transition-colors">
            ${ctrl.escapeHtml(ctrl.t('btnCalcular'))}
          </button>
        </div>
      </div>
    `;
  }
}

if (typeof window !== 'undefined') {
  window.renderFloatingLanguageSwitch = renderFloatingLanguageSwitch;
  window.VistaState = VistaState;
  window.HeroState = HeroState;
  window.FormularioState = FormularioState;
  window.ResultadosState = ResultadosState;
  window.DesgloseState = DesgloseState;
  window.PuertosState = PuertosState;
  window.ContactoState = ContactoState;
  window.DespachantesState = DespachantesState;
  window.PlanesState = PlanesState;
}
