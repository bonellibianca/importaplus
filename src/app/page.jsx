"use client";
import React, { useState, useEffect, useRef } from "react";
import { idiomaSubject, DICCIONARIO_IDIOMAS } from "@/i18n/IdiomaObserver";
import { exportarComprobanteXlsx } from "@/lib/excelExport";

function limpiarNumeroWhatsapp(raw) {
  return String(raw || "").replace(/[^0-9]/g, "");
}

function esUrlImagenSeguraCliente(rawUrl) {
  if (!rawUrl || typeof rawUrl !== "string") return false;
  const trimmed = rawUrl.trim();
  if (!trimmed || trimmed.length > 160000) return false;
  if (trimmed.startsWith("/") && !trimmed.startsWith("//")) return true;
  if (/^data:image\/(jpeg|jpg|png|webp);base64,[A-Za-z0-9+/=]+$/i.test(trimmed)) return true;
  try {
    return new URL(trimmed).protocol === "https:";
  } catch {
    return false;
  }
}

function escapeHtmlAttr(valor) {
  return String(valor ?? "")
    .replace(/&/g, "&amp;")
    .replace(/</g, "&lt;")
    .replace(/>/g, "&gt;")
    .replace(/"/g, "&quot;")
    .replace(/'/g, "&#39;");
}

function etiquetaTipoPuerto(puerto) {
  const nom = String(puerto?.nombre || "").toLowerCase();
  const via = String(puerto?.via_ingreso || "").toUpperCase();
  if (nom.includes("buenos aires")) return "Marítimo Principal";
  if (nom.includes("rosario")) return "Fluvial";
  if (via === "AEREA") return "Aéreo";
  if (via === "TERRESTRE") return "Terrestre";
  return "Marítimo";
}

const ALIAS_PAISES_EN = {
  USA: "united states america usa us",
  GBR: "united kingdom uk great britain england",
  DEU: "germany",
  ESP: "spain",
  BRA: "brazil",
  ITA: "italy",
  FRA: "france",
  JPN: "japan",
  KOR: "south korea",
  CHN: "china",
  CHE: "switzerland",
  NLD: "netherlands holland",
  SWE: "sweden",
  BEL: "belgium",
  CAN: "canada",
  MEX: "mexico"
};

const WHATSAPP_SVG_PATH =
  "M20.52 3.48A11.82 11.82 0 0012.06 0C5.5 0 .16 5.34.16 11.9c0 2.1.55 4.15 1.6 5.95L0 24l6.32-1.66a11.86 11.86 0 005.74 1.46h.01c6.55 0 11.89-5.34 11.89-11.9 0-3.18-1.24-6.17-3.44-8.42zM12.07 21.8h-.01a9.87 9.87 0 01-5.03-1.38l-.36-.21-3.75.98 1-3.65-.24-.38a9.85 9.85 0 01-1.51-5.26c0-5.45 4.44-9.89 9.9-9.89 2.64 0 5.13 1.03 7 2.9a9.83 9.83 0 012.9 7c0 5.45-4.44 9.89-9.9 9.89zm5.43-7.41c-.3-.15-1.76-.87-2.03-.97-.27-.1-.47-.15-.67.15-.2.3-.77.97-.94 1.16-.17.2-.35.22-.65.07-.3-.15-1.26-.46-2.4-1.48-.89-.79-1.49-1.77-1.66-2.07-.17-.3-.02-.46.13-.61.13-.13.3-.35.45-.52.15-.17.2-.3.3-.5.1-.2.05-.37-.02-.52-.08-.15-.67-1.61-.92-2.21-.24-.58-.49-.5-.67-.51h-.57c-.2 0-.52.07-.79.37-.27.3-1.04 1.02-1.04 2.48 0 1.46 1.07 2.88 1.22 3.08.15.2 2.1 3.2 5.08 4.49.71.31 1.26.49 1.69.63.71.23 1.36.2 1.87.12.57-.09 1.76-.72 2.01-1.41.25-.7.25-1.29.17-1.41-.07-.12-.27-.2-.57-.35z";

export default function ImportaPlusApp() {
  // --- Navegación multi-pantalla 100% sincronizada con GUI/states/SimuladorStates.js ---
  const [view, setViewState] = useState("hero");
  const [returnView, setReturnView] = useState("form");

  // --- PATRÓN OBSERVER: Idioma ES / EN ---
  const [idioma, setIdioma] = useState("es");
  const [dicc, setDicc] = useState(DICCIONARIO_IDIOMAS.es);

  useEffect(() => {
    setIdioma(idiomaSubject.obtenerIdioma());
    setDicc(idiomaSubject.obtenerDiccionario());

    const observadorReact = {
      actualizarIdioma(nuevoIdioma, nuevoDiccionario) {
        setIdioma(nuevoIdioma);
        setDicc(nuevoDiccionario);
      }
    };

    const desuscribir = idiomaSubject.suscribir(observadorReact);
    return () => desuscribir();
  }, []);

  useEffect(() => {
    const handleBeforePrint = () => { document.title = " "; };
    const handleAfterPrint = () => { document.title = "IMPORTA +"; };
    window.addEventListener("beforeprint", handleBeforePrint);
    window.addEventListener("afterprint", handleAfterPrint);
    return () => {
      window.removeEventListener("beforeprint", handleBeforePrint);
      window.removeEventListener("afterprint", handleAfterPrint);
    };
  }, []);

  // --- Menú Hamburguesa minimalista estilo Apple ---
  const [menuHamburguesaAbierto, setMenuHamburguesaAbierto] = useState(false);
  const menuHamburguesaRef = useRef(null);

  useEffect(() => {
    const handleClickOutside = (e) => {
      if (menuHamburguesaRef.current && !menuHamburguesaRef.current.contains(e.target)) {
        setMenuHamburguesaAbierto(false);
      }
    };
    document.addEventListener("mousedown", handleClickOutside);
    return () => document.removeEventListener("mousedown", handleClickOutside);
  }, []);

  const navigateTo = (targetView) => {
    if (targetView === "contact" || targetView === "despachantes") {
      if (view === "form" || view === "results" || view === "breakdown") {
        setReturnView(view);
      }
    }
    setMenuHamburguesaAbierto(false);
    setViewState(targetView);
    if (typeof window !== "undefined") {
      window.scrollTo({ top: 0, behavior: "smooth" });
    }
  };

  // --- Buscador Predictivo de Producto ---
  const [busqueda, setBusqueda] = useState("");
  const [sugerenciasNcm, setSugerenciasNcm] = useState([]);
  const [busquedaSinResultados, setBusquedaSinResultados] = useState(false);
  const [productoSeleccionado, setProductoSeleccionado] = useState(null);
  const [cargandoBusqueda, setCargandoBusqueda] = useState(false);
  const [mostrarDescProducto, setMostrarDescProducto] = useState(false);

  // --- Buscador Incremental de País ---
  const [paisesCatalogo, setPaisesCatalogo] = useState([]);
  const [busquedaPais, setBusquedaPais] = useState("");
  const [sugerenciasPais, setSugerenciasPais] = useState([]);
  const [paisSeleccionado, setPaisSeleccionado] = useState(null);

  // --- Variables Comerciales ---
  const [cantidad, setCantidad] = useState(0);
  const [fobUnitario, setFobUnitario] = useState(0);
  const [flete, setFlete] = useState(0);
  const [seguro, setSeguro] = useState(0);
  const [condicionImportador, setCondicionImportador] = useState("REVENTA_RI");
  const [usaExcepcion, setUsaExcepcion] = useState(true);
  const [mensajeError, setMensajeError] = useState("");
  const [calculando, setCalculando] = useState(false);

  // --- Resultados, Puertos y Directorios Oficiales ---
  const [resultado, setResultado] = useState(null);
  const [puertosCatalogo, setPuertosCatalogo] = useState([]);
  const [forwarders, setForwarders] = useState([]);
  const [despachantes, setDespachantes] = useState([]);
  const [mailCopiadoId, setMailCopiadoId] = useState(null);
  const mapInstanceRef = useRef(null);

  // Debounce búsqueda NCM
  useEffect(() => {
    const q = busqueda.trim();
    if (q.length < 2 || productoSeleccionado) {
      setSugerenciasNcm([]);
      setBusquedaSinResultados(false);
      setCargandoBusqueda(false);
      return;
    }

    const cacheKey = `ncm_q_${q.toLowerCase()}`;
    try {
      const cached = sessionStorage.getItem(cacheKey);
      if (cached) {
        const parsed = JSON.parse(cached);
        setSugerenciasNcm(parsed);
        setBusquedaSinResultados(parsed.length === 0);
        return;
      }
    } catch {}

    const controller = new AbortController();
    const timer = setTimeout(async () => {
      setCargandoBusqueda(true);
      try {
        const res = await fetch(`/api/v1/ncm/search?q=${encodeURIComponent(q)}`, {
          signal: controller.signal
        });
        const data = await res.json();
        if (controller.signal.aborted) return;
        const items = Array.isArray(data.results) ? data.results : [];
        setSugerenciasNcm(items);
        setBusquedaSinResultados(items.length === 0);
        try {
          sessionStorage.setItem(cacheKey, JSON.stringify(items));
        } catch {}
      } catch (err) {
        if (err.name !== "AbortError") {
          console.error("Error al buscar productos:", err);
        }
      } finally {
        if (!controller.signal.aborted) {
          setCargandoBusqueda(false);
        }
      }
    }, 280);

    return () => {
      clearTimeout(timer);
      controller.abort();
    };
  }, [busqueda, productoSeleccionado]);

  // Carga inicial de catálogos desde Supabase / API
  useEffect(() => {
    async function cargarDirectorios() {
      try {
        const res = await fetch("/api/v1/puertos", { cache: "no-store" });
        const data = await res.json();
        if (Array.isArray(data.forwarders)) setForwarders(data.forwarders);
        if (Array.isArray(data.despachantes)) setDespachantes(data.despachantes);
        if (Array.isArray(data.puertos)) setPuertosCatalogo(data.puertos);
        if (Array.isArray(data.paises)) setPaisesCatalogo(data.paises);
      } catch (err) {
        console.warn("Error al cargar catálogos:", err);
      }
    }
    cargarDirectorios();
  }, []);

  // Inicialización del Mapa Interactivo Leaflet en la vista 'ports'
  useEffect(() => {
    if (view !== "ports") {
      if (mapInstanceRef.current) {
        try {
          mapInstanceRef.current.remove();
        } catch {}
        mapInstanceRef.current = null;
      }
      return;
    }

    let cancelled = false;
    let attempts = 0;

    const tryInitMap = () => {
      if (cancelled) return;
      const mapEl = document.getElementById("map");
      const L = typeof window !== "undefined" ? window.L : undefined;
      if (!mapEl || !L) {
        if (attempts < 25) {
          attempts += 1;
          setTimeout(tryInitMap, 120);
        }
        return;
      }

      if (mapInstanceRef.current) {
        try {
          mapInstanceRef.current.remove();
        } catch {}
        mapInstanceRef.current = null;
      }

      const map = L.map("map", {
        center: [-34.6, -60.0],
        zoom: 5,
        scrollWheelZoom: false
      });
      mapInstanceRef.current = map;

      const hl = idioma === "en" ? "en" : "es";
      const googleLayer = L.tileLayer(`https://{s}.google.com/vt/lyrs=m&hl=${hl}&x={x}&y={y}&z={z}`, {
        subdomains: ["mt0", "mt1", "mt2", "mt3"],
        attribution: "&copy; Google Maps",
        maxZoom: 20
      });

      const cartoFallbackLayer = L.tileLayer("https://{s}.basemaps.cartocdn.com/rastertiles/voyager/{z}/{x}/{y}{r}.png", {
        subdomains: "abcd",
        attribution: "&copy; CARTO &copy; OpenStreetMap",
        maxZoom: 19
      });

      let fallbackActivated = false;
      googleLayer.on("tileerror", () => {
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
        } catch {}
      }, 200);

      const bounds = [];
      puertosCatalogo.forEach((p) => {
        const lat = parseFloat(p.latitud ?? p.lat);
        const lng = parseFloat(p.longitud ?? p.lng);
        if (Number.isNaN(lat) || Number.isNaN(lng) || (lat === 0 && lng === 0)) return;

        bounds.push([lat, lng]);

        const tipoLabel = etiquetaTipoPuerto(p);
        const tipoLower = tipoLabel.toLowerCase();
        let icono = "⚓";
        let colorBg = "#530618";
        if (tipoLower.includes("aér") || tipoLower.includes("aer")) {
          icono = "✈️";
          colorBg = "#1E3A8A";
        } else if (tipoLower.includes("terr") || tipoLower.includes("paso")) {
          icono = "🚛";
          colorBg = "#065F46";
        } else if (tipoLower.includes("fluv")) {
          icono = "🚢";
          colorBg = "#0284C7";
        }

        const customIcon = L.divIcon({
          className: "custom-port-marker",
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
                ${escapeHtmlAttr(tipoLabel)}
              </span>
              <span style="font-size: 9px; font-weight: 600; background: #F1F5F9; color: #475569; padding: 2px 6px; border-radius: 4px;">
                ${escapeHtmlAttr(p.codigo_sim || "ADUANA")}
              </span>
            </div>
            <h4 style="margin: 0 0 4px 0; font-size: 13px; font-weight: 800; color: #0F172A; line-height: 1.2;">
              ${escapeHtmlAttr(p.nombre)}
            </h4>
            <p style="margin: 0 0 8px 0; font-size: 11px; color: #64748B; line-height: 1.35;">
              ${escapeHtmlAttr(p.infraestructura || "")}
            </p>
            <a href="${escapeHtmlAttr(p.google_maps_url || "https://www.google.com/maps")}" target="_blank" rel="noopener noreferrer" style="display: inline-flex; align-items: center; gap: 4px; font-size: 11px; font-weight: 700; color: #530618; text-decoration: none;">
              ${escapeHtmlAttr(dicc.verEnGoogleMaps || "Abrir en Google Maps")} &#8599;
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
          } catch {}
        }, 300);
      }
    };

    const timer = setTimeout(tryInitMap, 80);
    return () => {
      cancelled = true;
      clearTimeout(timer);
    };
  }, [view, puertosCatalogo, idioma, dicc.verEnGoogleMaps]);

  const normalizarTextoPais = (s) =>
    String(s || "")
      .toLowerCase()
      .normalize("NFD")
      .replace(/[\u0300-\u036f]/g, "");

  const filtrarPaises = (texto) => {
    const limpio = String(texto || "").replace(/[^a-zA-ZáéíóúÁÉÍÓÚñÑüÜ\s]/g, "").trim();
    const term = normalizarTextoPais(limpio);
    if (!term) {
      setSugerenciasPais(paisesCatalogo.slice(0, 15));
      return;
    }

    const filtrados = paisesCatalogo.filter((p) => {
      const nomEs = normalizarTextoPais(p.nombre);
      const aliasEn = ALIAS_PAISES_EN[p.codigo_iso] || "";
      return nomEs.includes(term) || aliasEn.includes(term) || String(p.codigo_iso || "").toLowerCase() === term;
    });

    filtrados.sort((a, b) => {
      const aStarts = normalizarTextoPais(a.nombre).startsWith(term);
      const bStarts = normalizarTextoPais(b.nombre).startsWith(term);
      if (aStarts && !bStarts) return -1;
      if (!aStarts && bStarts) return 1;
      return normalizarTextoPais(a.nombre).localeCompare(normalizarTextoPais(b.nombre));
    });
    setSugerenciasPais(filtrados.slice(0, 15));
  };

  const abrirWhatsappTrabajaConNosotros = () => {
    setMenuHamburguesaAbierto(false);
    const rawWa = forwarders[0]?.whatsapp || "+5491143218800";
    const digits = limpiarNumeroWhatsapp(rawWa) || "5491143218800";
    const msg = encodeURIComponent(dicc.waMensajeTrabaja || "Hola IMPORTA+, me contacto porque quiero trabajar con ustedes.");
    window.open(`https://wa.me/${digits}?text=${msg}`, "_blank", "noopener,noreferrer");
  };

  const redirigirWhatsappOperador = (operador, esDespachante = false) => {
    const digits = limpiarNumeroWhatsapp(operador?.whatsapp);
    if (!digits) return;
    const msg = encodeURIComponent(esDespachante ? dicc.waMensajeDespachante : dicc.waMensajeDefault);
    window.open(`https://wa.me/${digits}?text=${msg}`, "_blank", "noopener,noreferrer");
  };

  const copiarEmailOperador = async (operador) => {
    if (!operador?.email) return;
    try {
      await navigator.clipboard.writeText(operador.email);
    } catch {
      const temp = document.createElement("textarea");
      temp.value = operador.email;
      document.body.appendChild(temp);
      temp.select();
      document.execCommand("copy");
      document.body.removeChild(temp);
    }
    setMailCopiadoId(operador.id);
    setTimeout(() => {
      setMailCopiadoId((prev) => (prev === operador.id ? null : prev));
    }, 2200);
  };

  const ejecutarCalculo = async (e) => {
    if (e && e.preventDefault) e.preventDefault();
    setMensajeError("");

    let prodFinal = productoSeleccionado;
    if (!prodFinal && busqueda.trim().length >= 2) {
      try {
        const resQ = await fetch(`/api/v1/ncm/search?q=${encodeURIComponent(busqueda.trim())}`);
        const dataQ = await resQ.json();
        if (Array.isArray(dataQ.results) && dataQ.results.length > 0) {
          prodFinal = dataQ.results[0];
          setProductoSeleccionado(prodFinal);
        }
      } catch {}
    }

    if (!prodFinal) {
      setMensajeError(dicc.alertProducto);
      return;
    }

    let paisFinal = paisSeleccionado;
    if (!paisFinal && busquedaPais.trim()) {
      const t = normalizarTextoPais(busquedaPais.trim());
      paisFinal =
        paisesCatalogo.find((p) => normalizarTextoPais(p.nombre) === t) ||
        paisesCatalogo.find((p) => (ALIAS_PAISES_EN[p.codigo_iso] || "").split(" ").includes(t)) ||
        paisesCatalogo.find((p) => normalizarTextoPais(p.nombre).includes(t)) ||
        null;
      if (paisFinal) setPaisSeleccionado(paisFinal);
    }

    if (!paisFinal) {
      setMensajeError(dicc.alertPais);
      return;
    }

    if (Number(cantidad) <= 0) {
      setMensajeError(dicc.alertCantidad);
      return;
    }

    if (Number(fobUnitario) <= 0) {
      setMensajeError(dicc.alertFob);
      return;
    }

    setCalculando(true);
    try {
      const res = await fetch("/api/v1/simular", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({
          codigoNcm: prodFinal.codigo_ncm,
          codigoPais: paisFinal.codigo_iso,
          cantidad: Number(cantidad),
          valorFobUnitario: Number(fobUnitario),
          flete: Number(flete) || 0,
          seguro: Number(seguro) || 0,
          condicionImportador,
          usaExcepcion
        })
      });

      const data = await res.json();
      if (!res.ok) {
        throw new Error(data.error || "No se pudo calcular la simulación");
      }

      if (data.simulacion) {
        setResultado({
          ...data.simulacion,
          categoriaProducto: prodFinal.categoria || prodFinal.codigo_ncm || dicc.prodSeleccionado,
          esMercosurPais: Boolean(paisFinal.es_mercosur),
          esBienCapital: prodFinal.tipo_bien === "BK"
        });
        setMostrarDescProducto(false);
        navigateTo("results");
      }
    } catch (err) {
      setMensajeError(err.message);
    } finally {
      setCalculando(false);
    }
  };

  const localeFormato = idioma === "en" ? "en-US" : "es-AR";
  const formatNumber = (num) => {
    const val = parseFloat(num) || 0;
    return val.toLocaleString(localeFormato, { minimumFractionDigits: 2, maximumFractionDigits: 2 });
  };

  const cantidadNum = Math.max(0, parseInt(cantidad, 10) || 0);
  const fobUnitNum = Math.max(0, parseFloat(fobUnitario) || 0);
  const subtotalFOB = cantidadNum * fobUnitNum;
  const tieneExcepcionProducto = Boolean(
    productoSeleccionado &&
    productoSeleccionado.anexo_excepcion &&
    productoSeleccionado.arancel_excepcion !== null &&
    productoSeleccionado.arancel_excepcion !== undefined
  );

  // --- Componente Reutilizable: Menú Hamburguesa Minimalista Estilo Apple ---
  const renderHamburgerMenu = () => (
    <div ref={menuHamburguesaRef} className="relative print:hidden">
      <button
        type="button"
        onClick={() => setMenuHamburguesaAbierto((prev) => !prev)}
        title="Menú"
        aria-label="Abrir menú"
        aria-expanded={menuHamburguesaAbierto}
        className="inline-flex items-center justify-center w-8 h-8 bg-white/[0.08] hover:bg-white/[0.15] border border-white/[0.16] text-white/90 hover:text-white rounded-[10px] backdrop-blur-md transition-all duration-200 active:scale-95 cursor-pointer select-none"
      >
        <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path strokeLinecap="round" strokeLinejoin="round" strokeWidth="1.65" d="M4.75 7.25h14.5M4.75 12h14.5M4.75 16.75h14.5" />
        </svg>
      </button>

      <div
        className={`apple-popover ${menuHamburguesaAbierto ? "is-open" : ""} absolute right-0 mt-2 min-w-[194px] rounded-[14px] p-1 z-50`}
      >
        <button
          type="button"
          onClick={abrirWhatsappTrabajaConNosotros}
          className="apple-popover-item group w-full px-2.5 py-2 rounded-[10px] text-left flex items-center justify-between gap-3 cursor-pointer select-none"
        >
          <span className="flex items-center gap-2.5 min-w-0">
            <span className="inline-flex items-center justify-center w-[22px] h-[22px] rounded-[7px] bg-[#1d1d1f]/[0.05] group-hover:bg-[#25D366]/[0.13] text-[#1d1d1f]/70 group-hover:text-[#179c49] transition-colors duration-200 shrink-0">
              <svg className="w-[13px] h-[13px] fill-current" viewBox="0 0 24 24">
                <path d={WHATSAPP_SVG_PATH} />
              </svg>
            </span>
            <span className="text-[12px] font-medium tracking-[-0.01em] text-[#1d1d1f]/90 group-hover:text-[#1d1d1f] whitespace-nowrap transition-colors">
              {dicc.menuTrabajaConNosotros || "Trabaja con nosotros"}
            </span>
          </span>
          <svg className="w-3 h-3 text-[#1d1d1f]/30 group-hover:text-[#1d1d1f]/60 group-hover:translate-x-0.5 group-hover:-translate-y-0.5 transition-all duration-200 shrink-0" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth="1.8" d="M7 17L17 7M9 7h8v8" />
          </svg>
        </button>
      </div>
    </div>
  );

  // --- Componente Reutilizable: Grilla Oficial de 4 Cuadrados (Forwarders o Despachantes) ---
  const renderDirectorioGrid = (esDespachantes) => {
    const lista = esDespachantes ? despachantes : forwarders;
    if (!Array.isArray(lista) || lista.length === 0) return null;

    return (
      <div className="bg-white border border-[#D8DDE3] rounded-sm p-6 shadow-sm">
        <div className="border-b border-[#F4F6F8] pb-3 mb-5 text-left">
          <span className="text-xs font-bold uppercase tracking-widest text-[#530618]">
            {esDespachantes ? dicc.despTag : dicc.fwdTag}
          </span>
          <h3 className="text-xl md:text-2xl font-bold text-[#171717] mt-0.5">
            {esDespachantes ? dicc.despTitulo : dicc.fwdTitulo}
          </h3>
          <p className="text-xs text-[#555555] mt-1">
            {esDespachantes ? dicc.despSubtitulo : dicc.fwdSubtitulo}
          </p>
        </div>

        <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-5 text-left">
          {lista.map((item) => {
            const nombreMax120 = String(item.nombre || "").slice(0, 120);
            const copiado = mailCopiadoId === item.id;
            const imgSegura = esUrlImagenSeguraCliente(item.imagen_url) ? item.imagen_url : "/logo-importa.png";
            return (
              <div
                key={item.id}
                className="bg-[#FFF8F8] border border-[#F0D0D4] rounded-sm overflow-hidden shadow-sm hover:border-[#530618] transition-all flex flex-col justify-between"
              >
                <div className="w-full aspect-square bg-gray-200 overflow-hidden">
                  <img
                    src={imgSegura}
                    alt={nombreMax120}
                    loading="lazy"
                    referrerPolicy="no-referrer"
                    onError={(e) => {
                      e.currentTarget.onerror = null;
                      e.currentTarget.src = "/logo-importa.png";
                    }}
                    className="w-full h-full object-cover"
                  />
                </div>

                <div className="p-4 flex flex-col flex-1 justify-between gap-3">
                  <h4
                    className="text-sm font-bold text-[#171717] text-center leading-snug break-words"
                    title={nombreMax120}
                  >
                    {nombreMax120}
                  </h4>

                  <div className="space-y-2">
                    <button
                      type="button"
                      onClick={() => redirigirWhatsappOperador(item, esDespachantes)}
                      className="w-full flex items-center justify-center gap-2 bg-emerald-600 hover:bg-emerald-700 text-white font-bold text-xs py-2.5 px-3 rounded-sm transition-colors shadow-sm cursor-pointer"
                    >
                      <svg className="w-4 h-4 fill-current shrink-0" viewBox="0 0 24 24">
                        <path d={WHATSAPP_SVG_PATH} />
                      </svg>
                      <span>{dicc.btnWhatsapp}</span>
                    </button>

                    <button
                      type="button"
                      onClick={() => copiarEmailOperador(item)}
                      className={`w-full flex items-center justify-center gap-2 ${
                        copiado
                          ? "bg-[#3D0411] text-rose-100 ring-2 ring-[#530618]"
                          : "bg-[#530618] hover:bg-[#3D0411] text-white"
                      } font-bold text-xs py-2.5 px-3 rounded-sm transition-colors shadow-sm cursor-pointer`}
                    >
                      <svg className="w-4 h-4 shrink-0" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" d="M3 8l7.89 5.26a2 2 0 002.22 0L21 8M5 19h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v10a2 2 0 002 2z" />
                      </svg>
                      <span>{copiado ? dicc.mailCopiado : dicc.btnCopiarMail}</span>
                    </button>
                  </div>
                </div>
              </div>
            );
          })}
        </div>
      </div>
    );
  };

  // --- Extracción de ítems calculados para vistas 'results' y 'breakdown' ---
  const findGravamen = (id) =>
    resultado?.desgloseTributario?.find((g) => g.id === id) || { alicuotaNum: 0, montoUSD: 0, alicuota: "0%" };

  const dieItem = findGravamen("derechos_importacion");
  const estItem = findGravamen("tasa_estadistica");
  const ivaItem = findGravamen("iva_aduanero");
  const percIvaItem = findGravamen("percepcion_iva");
  const percGanItem = findGravamen("percepcion_ganancias");
  const simItem = findGravamen("arancel_sim");

  return (
    <>
      {/* VISTA 1: HERO INSTITUCIONAL */}
      {view === "hero" && (
        <div className="h-screen w-full flex flex-col justify-between overflow-hidden" style={{ backgroundColor: "#420213" }}>
          <header className="px-6 py-3 md:py-4 flex items-center justify-between shrink-0">
            <div className="flex items-center cursor-pointer" onClick={() => navigateTo("hero")}>
              <img src="/logo-importa.svg" alt="IMPORTA+" className="h-6 md:h-7 w-auto object-contain select-none" />
            </div>
            {renderHamburgerMenu()}
          </header>

          <div className="flex-1 flex flex-col items-center justify-center text-center px-4 w-full min-h-0 py-1">
            <div className="w-full flex items-center justify-center mb-6">
              <img
                src="/logo-importa.svg"
                alt="IMPORTA+"
                className="w-auto h-16 sm:h-20 md:h-24 max-w-[85vw] object-contain mx-auto block select-none"
              />
            </div>

            <p className="text-base sm:text-lg md:text-xl text-white/90 font-normal mb-7 max-w-3xl px-4">
              {dicc.heroSubtitle}
            </p>

            <button
              type="button"
              id="btn-acceder-calculadora"
              onClick={() => navigateTo("form")}
              className="bg-white text-[#420213] font-bold text-xs md:text-sm uppercase tracking-widest px-8 md:px-10 py-3.5 rounded-sm hover:bg-white/90 transition-all duration-200 shadow-xl hover:scale-[1.02] active:scale-100 cursor-pointer"
            >
              {dicc.btnCalcular}
            </button>
          </div>

          <div className="py-2 shrink-0" />
        </div>
      )}

      {/* VISTA 2: FORMULARIO DE LIQUIDACIÓN ADUANERA (PASO 1 DE 2) */}
      {view === "form" && (
        <div className="min-h-screen bg-[#F4F6F8]">
          <header className="bg-[#420213] px-6 py-4 flex items-center justify-between shadow-sm">
            <div className="flex items-center cursor-pointer" onClick={() => navigateTo("hero")}>
              <img src="/logo-importa.svg" alt="IMPORTA+" className="h-6 md:h-7 w-auto object-contain select-none" />
            </div>
            <div className="flex items-center gap-3">
              <span className="text-white/70 text-xs font-semibold uppercase tracking-widest hidden sm:inline">
                {dicc.paso1Corto}
              </span>
              {renderHamburgerMenu()}
            </div>
          </header>

          <div className="max-w-2xl mx-auto px-4 py-10">
            <h2 className="text-2xl md:text-3xl font-bold text-[#171717] mb-1">{dicc.tituloLiquidacion}</h2>
            <p className="text-[#555555] text-sm mb-8">{dicc.subtituloForm}</p>

            {mensajeError && (
              <div
                role="alert"
                className="mb-5 p-3.5 bg-[#FFF8F8] border border-[#F0D0D4] text-[#530618] rounded-sm text-xs font-semibold flex items-center justify-between gap-3 shadow-sm"
              >
                <span>{mensajeError}</span>
                <button
                  type="button"
                  onClick={() => setMensajeError("")}
                  className="text-[#530618] font-bold px-2 py-0.5 cursor-pointer"
                  aria-label="Cerrar alerta"
                >
                  ✕
                </button>
              </div>
            )}

            <form onSubmit={ejecutarCalculo}>
              <div className="bg-white rounded-sm border border-[#D8DDE3] divide-y divide-[#F4F6F8] overflow-hidden mb-6 shadow-sm">
                <div className="px-6 py-5">
                  <div className="flex items-center gap-2 mb-4">
                    <p className="text-xs font-bold uppercase tracking-widest text-[#530618]">{dicc.infoGeneral}</p>
                  </div>

                  <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                    <div className="relative md:col-span-2">
                      <label className="block text-xs font-semibold uppercase tracking-wider text-[#555555] mb-1.5">
                        {dicc.lblProducto}
                      </label>
                      <input
                        type="text"
                        autoComplete="off"
                        value={busqueda}
                        onChange={(e) => {
                          setBusqueda(e.target.value);
                          setProductoSeleccionado(null);
                          setMostrarDescProducto(false);
                          if (mensajeError) setMensajeError("");
                        }}
                        onBlur={() => {
                          setSugerenciasNcm([]);
                          setBusquedaSinResultados(false);
                        }}
                        placeholder={dicc.phProducto}
                        className="w-full bg-white border border-[#D8DDE3] rounded-sm px-4 py-3 text-[#171717] text-sm focus:outline-none focus:border-[#530618] focus:ring-1 focus:ring-[#530618]"
                      />

                      {cargandoBusqueda && (
                        <div className="absolute right-4 top-10 text-xs text-gray-400 animate-pulse">
                          {dicc.buscandoProducto}
                        </div>
                      )}

                      {sugerenciasNcm.length > 0 && !productoSeleccionado && (
                        <div className="absolute left-0 right-0 top-full mt-1 bg-white border border-[#D8DDE3] rounded-sm shadow-xl z-50 max-h-60 overflow-y-auto">
                          {sugerenciasNcm.map((p) => (
                            <div
                              key={p.codigo_ncm}
                              onMouseDown={(e) => {
                                e.preventDefault();
                                setProductoSeleccionado(p);
                                setBusqueda(p.descripcion);
                                setUsaExcepcion(true);
                                setMostrarDescProducto(false);
                                setSugerenciasNcm([]);
                                setBusquedaSinResultados(false);
                                setMensajeError("");
                              }}
                              className="p-3 border-b border-gray-100 hover:bg-[#FFF8F8] cursor-pointer text-xs"
                            >
                              <div className="flex justify-between font-bold text-[#530618]">
                                <span>{p.categoria || dicc.prodSeleccionado}</span>
                                <span className="text-gray-500 font-normal">
                                  {dicc.arancel}: {p.anexo_excepcion && p.arancel_excepcion !== null ? p.arancel_excepcion : p.aec}%
                                </span>
                              </div>
                              <div className="text-gray-700 mt-0.5 font-medium">{p.descripcion}</div>
                            </div>
                          ))}
                        </div>
                      )}

                      {busquedaSinResultados && !cargandoBusqueda && !productoSeleccionado && busqueda.trim().length >= 2 && (
                        <div className="absolute left-0 right-0 top-full mt-1 bg-white border border-[#D8DDE3] rounded-sm shadow-xl z-50 p-3 text-xs text-gray-500">
                          {dicc.sinProductos}
                        </div>
                      )}

                      {productoSeleccionado && (
                        <div className="mt-2.5 p-3 bg-[#FFF8F8] border border-[#F0D0D4] rounded-sm text-xs">
                          <div className="flex flex-wrap justify-between items-center gap-2 font-bold text-[#530618]">
                            <span>{productoSeleccionado.categoria || dicc.prodSeleccionado}</span>
                            <div className="flex items-center gap-2">
                              <span className="bg-[#530618] text-white px-2 py-0.5 rounded-sm text-[10px]">
                                {dicc.arancelBase}: {productoSeleccionado.aec}%
                              </span>
                              <button
                                type="button"
                                onClick={() => setMostrarDescProducto((prev) => !prev)}
                                className="text-[11px] font-bold text-[#530618] bg-white border border-[#F0D0D4] px-2.5 py-0.5 rounded-sm hover:bg-[#530618] hover:text-white transition-colors cursor-pointer"
                              >
                                {mostrarDescProducto ? dicc.ocultarDesc : dicc.verDesc}
                              </button>
                            </div>
                          </div>

                          {mostrarDescProducto && (
                            <div className="mt-2 pt-2 border-t border-[#F0D0D4]">
                              <p className="text-[#555] leading-relaxed">{productoSeleccionado.descripcion}</p>
                              {tieneExcepcionProducto && (
                                <div className="mt-2 pt-2 border-t border-[#F0D0D4] flex items-center justify-between text-[#15803d]">
                                  <span>{dicc.beneficioDisponible}</span>
                                  <span className="font-bold">
                                    {dicc.arancelReducido} {productoSeleccionado.arancel_excepcion}%
                                  </span>
                                </div>
                              )}
                            </div>
                          )}
                        </div>
                      )}
                    </div>

                    <div className="relative">
                      <label className="block text-xs font-semibold uppercase tracking-wider text-[#555555] mb-1.5">
                        {dicc.lblPais}
                      </label>
                      <input
                        type="text"
                        autoComplete="off"
                        value={busquedaPais}
                        onKeyDown={(e) => {
                          if (e.ctrlKey || e.metaKey || e.altKey) return;
                          const teclasPermitidas = ["Backspace", "Delete", "Tab", "Escape", "Enter", "ArrowLeft", "ArrowRight", "ArrowUp", "ArrowDown", "Home", "End", " "];
                          if (teclasPermitidas.includes(e.key)) {
                            if (e.key === "Escape") setSugerenciasPais([]);
                            return;
                          }
                          if (!/^[a-zA-ZáéíóúÁÉÍÓÚñÑüÜ]$/.test(e.key)) {
                            e.preventDefault();
                          }
                        }}
                        onChange={(e) => {
                          const soloLetras = e.target.value.replace(/[^a-zA-ZáéíóúÁÉÍÓÚñÑüÜ\s]/g, "");
                          setBusquedaPais(soloLetras);
                          setPaisSeleccionado(null);
                          if (mensajeError) setMensajeError("");
                          filtrarPaises(soloLetras);
                        }}
                        onFocus={(e) => filtrarPaises(e.target.value)}
                        onClick={(e) => filtrarPaises(e.target.value)}
                        onBlur={() => setSugerenciasPais([])}
                        placeholder={dicc.phPais}
                        className="w-full bg-white border border-[#D8DDE3] rounded-sm px-4 py-3 text-[#171717] text-sm focus:outline-none focus:border-[#530618] focus:ring-1 focus:ring-[#530618]"
                      />

                      {sugerenciasPais.length > 0 && (
                        <div className="absolute left-0 right-0 top-full mt-1 bg-white border border-[#D8DDE3] rounded-sm shadow-xl z-50 max-h-60 overflow-y-auto">
                          {sugerenciasPais.map((p) => (
                            <div
                              key={p.codigo_iso}
                              onMouseDown={(e) => {
                                e.preventDefault();
                                setPaisSeleccionado(p);
                                setBusquedaPais(p.nombre);
                                setSugerenciasPais([]);
                                setMensajeError("");
                              }}
                              className="p-2.5 border-b border-gray-100 hover:bg-[#FFF8F8] cursor-pointer text-xs flex justify-between items-center"
                            >
                              <span className="font-semibold text-[#171717]">{p.nombre}</span>
                              <span
                                className={`text-[10px] px-2 py-0.5 rounded-sm ${
                                  p.es_mercosur ? "bg-emerald-50 text-emerald-700 font-bold" : "bg-gray-100 text-gray-600"
                                }`}
                              >
                                {p.es_mercosur ? dicc.mercosurCero : dicc.extrazona}
                              </span>
                            </div>
                          ))}
                        </div>
                      )}
                    </div>

                    <div>
                      <label className="block text-xs font-semibold uppercase tracking-wider text-[#555555] mb-1.5">
                        {dicc.lblCantidad}
                      </label>
                      <input
                        type="number"
                        min="0"
                        value={cantidad}
                        onFocus={(e) => { if (Number(e.target.value) === 0) e.target.select(); }}
                        onChange={(e) => setCantidad(e.target.value === "" ? 0 : e.target.value)}
                        className="w-full bg-white border border-[#D8DDE3] rounded-sm px-4 py-3 text-[#171717] text-sm focus:outline-none focus:border-[#530618]"
                      />
                    </div>

                    <div className="md:col-span-2">
                      <label className="block text-xs font-semibold uppercase tracking-wider text-[#555555] mb-1.5">
                        {dicc.lblFobUnitario}
                      </label>
                      <div className="relative">
                        <span className="absolute left-3.5 top-1/2 -translate-y-1/2 text-[#999] text-sm font-medium">$</span>
                        <input
                          type="number"
                          step="0.01"
                          min="0"
                          value={fobUnitario}
                          onFocus={(e) => { if (Number(e.target.value) === 0) e.target.select(); }}
                          onChange={(e) => setFobUnitario(e.target.value === "" ? 0 : e.target.value)}
                          className="w-full bg-white border border-[#D8DDE3] rounded-sm pl-8 pr-4 py-3 text-[#171717] text-sm focus:outline-none focus:border-[#530618]"
                        />
                      </div>
                    </div>
                  </div>

                  <div className="mt-4 bg-[#F4F6F8] border border-[#D8DDE3] rounded-sm px-4 py-3 flex items-center justify-between">
                    <div>
                      <p className="text-xs font-bold uppercase tracking-wider text-[#555555]">{dicc.lblFobTotalVivo}</p>
                      <p className="text-[11px] text-[#530618] font-semibold mt-0.5">
                        {cantidadNum} {dicc.unidAbrev} × USD {formatNumber(fobUnitNum)} = USD {formatNumber(subtotalFOB)}
                      </p>
                    </div>
                    <span className="text-lg font-bold text-[#530618] tabular-nums">USD {formatNumber(subtotalFOB)}</span>
                  </div>
                </div>

                <div className="px-6 py-5">
                  <div className="flex flex-wrap items-start justify-between gap-2 mb-2">
                    <div className="flex items-center gap-2 pt-1">
                      <p className="text-xs font-bold uppercase tracking-widest text-[#530618]">{dicc.costosAdicionales}</p>
                      <span className="text-[10px] font-semibold border border-[#D8DDE3] text-[#555555] px-1.5 py-0.5 rounded-sm uppercase tracking-wide">
                        {dicc.opcional}
                      </span>
                    </div>
                    <div className="flex flex-col items-stretch sm:items-end gap-1.5">
                      <button
                        type="button"
                        onClick={() => navigateTo("contact")}
                        className="bg-[#530618] hover:bg-[#3D0411] text-white text-xs font-semibold px-3 py-1.5 rounded-sm transition-colors duration-200 shadow-sm cursor-pointer"
                      >
                        {dicc.btnContactarForwarder}
                      </button>
                      <button
                        type="button"
                        onClick={() => navigateTo("despachantes")}
                        className="bg-[#530618] hover:bg-[#3D0411] text-white text-xs font-semibold px-3 py-1.5 rounded-sm transition-colors duration-200 shadow-sm cursor-pointer"
                      >
                        {dicc.btnContactarDespachante}
                      </button>
                    </div>
                  </div>

                  <div className="grid grid-cols-1 md:grid-cols-2 gap-4 mt-3">
                    <div>
                      <label className="block text-xs font-semibold uppercase tracking-wider text-[#555555] mb-1.5">
                        {dicc.lblFlete}
                      </label>
                      <input
                        type="number"
                        min="0"
                        step="0.01"
                        value={flete}
                        onFocus={(e) => { if (Number(e.target.value) === 0) e.target.select(); }}
                        onChange={(e) => setFlete(e.target.value === "" ? 0 : e.target.value)}
                        className="w-full bg-white border border-[#D8DDE3] rounded-sm px-4 py-3 text-[#171717] text-sm focus:outline-none focus:border-[#530618]"
                      />
                    </div>
                    <div>
                      <label className="block text-xs font-semibold uppercase tracking-wider text-[#555555] mb-1.5">
                        {dicc.lblSeguro}
                      </label>
                      <input
                        type="number"
                        min="0"
                        step="0.01"
                        value={seguro}
                        onFocus={(e) => { if (Number(e.target.value) === 0) e.target.select(); }}
                        onChange={(e) => setSeguro(e.target.value === "" ? 0 : e.target.value)}
                        className="w-full bg-white border border-[#D8DDE3] rounded-sm px-4 py-3 text-[#171717] text-sm focus:outline-none focus:border-[#530618]"
                      />
                    </div>
                  </div>
                </div>

                <div className="px-6 py-4 bg-[#fafbfc]">
                  <label className="block text-xs font-semibold uppercase tracking-wider text-[#555555] mb-2">
                    {dicc.lblCondicionImpositiva}
                  </label>
                  <div className="flex flex-wrap gap-6 text-xs text-[#171717]">
                    <label className="flex items-center gap-2 cursor-pointer font-medium">
                      <input
                        type="radio"
                        name="condicionImportador"
                        value="REVENTA_RI"
                        checked={condicionImportador === "REVENTA_RI"}
                        onChange={() => setCondicionImportador("REVENTA_RI")}
                        className="accent-[#530618]"
                      />
                      {dicc.condInscripto}
                    </label>
                    <label className="flex items-center gap-2 cursor-pointer font-medium">
                      <input
                        type="radio"
                        name="condicionImportador"
                        value="USO_PROPIO_CONSUMO"
                        checked={condicionImportador === "USO_PROPIO_CONSUMO"}
                        onChange={() => setCondicionImportador("USO_PROPIO_CONSUMO")}
                        className="accent-[#530618]"
                      />
                      {dicc.condConsumo}
                    </label>
                  </div>
                </div>
              </div>

              <button
                type="submit"
                disabled={calculando}
                className="w-full bg-[#530618] hover:bg-[#3D0411] text-white font-bold text-sm uppercase tracking-widest py-4 rounded-sm transition-colors duration-200 shadow-md disabled:opacity-50 cursor-pointer"
              >
                {calculando ? dicc.calculando : dicc.btnCalcular}
              </button>
            </form>
          </div>
        </div>
      )}

      {/* VISTA 3: RESULTADOS DE LIQUIDACIÓN */}
      {view === "results" && resultado && (
        <div className="min-h-screen bg-[#F4F6F8]">
          <header className="bg-[#420213] px-6 py-4 flex items-center justify-between shadow-sm print:hidden">
            <div className="flex items-center cursor-pointer" onClick={() => navigateTo("hero")}>
              <img src="/logo-importa.svg" alt="IMPORTA+" className="h-6 md:h-7 w-auto object-contain select-none" />
            </div>
            <div className="flex items-center gap-3">
              <button
                type="button"
                onClick={() => navigateTo("form")}
                className="text-white/70 text-xs font-medium hover:text-white transition-colors cursor-pointer"
              >
                {dicc.nuevaConsulta}
              </button>
              {renderHamburgerMenu()}
            </div>
          </header>

          <div className="max-w-2xl mx-auto px-4 py-10">
            <div className="flex flex-wrap items-center justify-between gap-2 mb-2">
              <span className="text-xs font-bold uppercase tracking-widest text-[#530618]">{dicc.tituloLiquidacion}</span>
              <button
                type="button"
                onClick={() => setMostrarDescProducto((prev) => !prev)}
                className="text-xs font-bold text-[#530618] bg-white border border-[#F0D0D4] px-3 py-1 rounded-sm hover:bg-[#530618] hover:text-white transition-colors shadow-sm print:hidden cursor-pointer"
              >
                {mostrarDescProducto ? dicc.ocultarDesc : dicc.verDesc}
              </button>
            </div>

            <h2 className="text-2xl font-bold text-[#171717] mb-1">
              {resultado.categoriaProducto || dicc.prodSeleccionado}
            </h2>
            <p className="text-[#555555] text-sm mb-3">
              {dicc.origen}: {resultado.fichaProducto.paisOrigen} · {resultado.parametrosComerciales.cantidad} {dicc.unidadesA} USD {formatNumber(resultado.parametrosComerciales.valorFobUnitario)}
            </p>

            {mostrarDescProducto && (
              <div className="mb-5 p-4 bg-white border border-[#F0D0D4] rounded-sm text-xs text-[#555555] shadow-sm">
                <p className="font-bold text-[#530618] uppercase tracking-wider mb-1">{dicc.descProductoTitulo}</p>
                <p className="leading-relaxed text-[#171717]">{resultado.fichaProducto.descripcion}</p>
              </div>
            )}

            <div className="bg-[#530618] rounded-sm p-6 mb-4 relative overflow-hidden shadow-lg">
              <p className="text-white/70 text-xs font-bold uppercase tracking-widest mb-1">{dicc.costoTotalAduana}</p>
              <p className="text-4xl md:text-5xl font-bold text-white tracking-tight">
                USD {formatNumber(resultado.resumenEconomico.costoTotalNacionalizadoUSD)}
              </p>
              <p className="text-white/80 text-xs mt-2 font-medium">
                ≈ $ {formatNumber(resultado.resumenEconomico.costoTotalEstimadoARS)} ARS{" "}
                <span className="opacity-60">(BNA: ${resultado.resumenEconomico.tipoCambioBNA})</span>
              </p>
              <div className="mt-4 pt-3 border-t border-white/20 flex justify-between text-xs text-white/90">
                <span>{dicc.costoUnitarioNac}</span>
                <strong className="font-mono text-sm">
                  USD {formatNumber(resultado.resumenEconomico.costoUnitarioFinalUSD)} / {dicc.unidad}
                </strong>
              </div>
            </div>

            <div className="bg-white border border-[#D8DDE3] rounded-sm p-4 mb-4 shadow-sm">
              <div className="flex justify-between items-center text-xs font-bold text-[#555] mb-2 uppercase">
                <span>{dicc.incidenciaAduanera}</span>
                <span className="text-[#530618]">
                  +{resultado.resumenEconomico.sobrecostoTotalPct}% {dicc.sobreFob}
                </span>
              </div>
              <div className="grid grid-cols-2 gap-3 text-center">
                <div className="p-3 bg-gray-50 rounded-sm border border-gray-100">
                  <span className="text-[11px] text-gray-400 font-bold uppercase">{dicc.compraFob}</span>
                  <p className="text-lg font-black text-gray-800">
                    USD {formatNumber(resultado.parametrosComerciales.valorFobTotal)}
                  </p>
                </div>
                <div className="p-3 bg-[#FFF8F8] rounded-sm border border-[#F0D0D4]">
                  <span className="text-[11px] text-[#530618] font-bold uppercase">{dicc.desembolsoFinal}</span>
                  <p className="text-lg font-black text-[#530618]">
                    USD {formatNumber(resultado.resumenEconomico.costoTotalNacionalizadoUSD)}
                  </p>
                </div>
              </div>
            </div>

            <div className="bg-white border border-[#D8DDE3] rounded-sm divide-y divide-[#F4F6F8] mb-4 shadow-sm">
              {[
                { label: dicc.itemFob, value: resultado.parametrosComerciales.valorFobTotal },
                { label: dicc.itemFlete, value: resultado.parametrosComerciales.flete },
                { label: dicc.itemSeguro, value: resultado.parametrosComerciales.seguro },
                { label: `${dicc.itemDie} (${dieItem.alicuotaNum}%)`, value: dieItem.montoUSD },
                { label: dicc.impuestosDga, value: ivaItem.montoUSD + percIvaItem.montoUSD + percGanItem.montoUSD },
                { label: dicc.cargosSimEst, value: estItem.montoUSD + simItem.montoUSD }
              ].map((it, idx) => (
                <div key={idx} className="flex items-center justify-between px-5 py-3.5">
                  <span className="text-sm text-[#555555]">{it.label}</span>
                  <span className="text-sm font-semibold text-[#171717] tabular-nums">USD {formatNumber(it.value)}</span>
                </div>
              ))}
            </div>

            <div className="print:hidden">
              <button
                type="button"
                onClick={() => navigateTo("breakdown")}
                className="w-full bg-[#530618] hover:bg-[#3D0411] text-white font-bold text-sm uppercase tracking-widest py-4 rounded-sm transition-colors duration-200 mb-3 shadow-md cursor-pointer"
              >
                {dicc.btnVerDesglose}
              </button>
              <button
                type="button"
                onClick={() => navigateTo("contact")}
                className="w-full bg-[#3D0411] hover:bg-[#530618] text-white font-bold text-xs uppercase tracking-widest py-3.5 rounded-sm transition-colors duration-200 mb-2 shadow-sm cursor-pointer"
              >
                {dicc.btnContactarForwarder}
              </button>
              <button
                type="button"
                onClick={() => navigateTo("despachantes")}
                className="w-full bg-[#3D0411] hover:bg-[#530618] text-white font-bold text-xs uppercase tracking-widest py-3.5 rounded-sm transition-colors duration-200 mb-3 shadow-sm cursor-pointer"
              >
                {dicc.btnContactarDespachante}
              </button>
              <div className="grid grid-cols-1 sm:grid-cols-2 gap-3 mb-3">
                <button
                  type="button"
                  onClick={() => window.print()}
                  className="w-full bg-[#530618] hover:bg-[#3D0411] text-white font-bold text-xs uppercase tracking-widest py-3.5 rounded-sm transition-colors duration-200 shadow-md flex items-center justify-center gap-2 cursor-pointer"
                >
                  {dicc.btnImprimirPdf}
                </button>
                <button
                  type="button"
                  onClick={() => {
                    const r = resultado;
                    exportarComprobanteXlsx({
                      idCotizacion: r.idCotizacion,
                      fechaEmision: r.fechaEmision,
                      descripcion: r.fichaProducto.descripcion,
                      pais: r.fichaProducto.paisOrigen,
                      regimen: r.fichaProducto.regimenAplicado,
                      cantidad: r.parametrosComerciales.cantidad,
                      fobUnit: r.parametrosComerciales.valorFobUnitario,
                      fobTotal: r.parametrosComerciales.valorFobTotal,
                      flete: r.parametrosComerciales.flete,
                      seguro: r.parametrosComerciales.seguro,
                      cif: r.parametrosComerciales.valorCIF,
                      desglose: r.desgloseTributario,
                      incidenciaPct: r.resumenEconomico.incidenciaFiscalPct,
                      totalGrav: r.resumenEconomico.totalGravamenesUSD,
                      costoNac: r.resumenEconomico.costoTotalNacionalizadoUSD,
                      costoUnit: r.resumenEconomico.costoUnitarioFinalUSD,
                      tcBna: r.resumenEconomico.tipoCambioBNA,
                      costoArs: r.resumenEconomico.costoTotalEstimadoARS
                    });
                  }}
                  className="w-full bg-emerald-700 hover:bg-emerald-800 text-white font-bold text-xs uppercase tracking-widest py-3.5 rounded-sm transition-colors duration-200 shadow-md flex items-center justify-center cursor-pointer"
                >
                  {dicc.btnExportarExcel}
                </button>
              </div>
              <button
                type="button"
                onClick={() => navigateTo("form")}
                className="w-full border border-[#530618] text-[#530618] hover:bg-[#530618] hover:text-white font-semibold text-sm py-3.5 rounded-sm transition-colors duration-200 cursor-pointer"
              >
                {dicc.nuevaConsulta}
              </button>
            </div>
          </div>
        </div>
      )}

      {/* VISTA 4: DESGLOSE OFICIAL DETALLADO */}
      {view === "breakdown" && resultado && (
        <div className="min-h-screen bg-[#F4F6F8] print:bg-white print:min-h-0">
          <header className="bg-[#420213] px-6 py-4 flex items-center justify-between shadow-sm print:hidden">
            <div className="flex items-center cursor-pointer" onClick={() => navigateTo("hero")}>
              <img src="/logo-importa.svg" alt="IMPORTA+" className="h-6 md:h-7 w-auto object-contain select-none" />
            </div>
            <div className="flex items-center gap-3">
              <button
                type="button"
                onClick={() => window.print()}
                className="flex items-center gap-1.5 bg-white/10 hover:bg-white/20 text-white text-xs font-semibold px-3 py-1.5 rounded-sm transition-colors border border-white/20 cursor-pointer"
              >
                {dicc.btnImprimirDesglose}
              </button>
              <button
                type="button"
                onClick={() => navigateTo("results")}
                className="text-white/70 text-xs font-medium hover:text-white transition-colors cursor-pointer"
              >
                {dicc.volverResultados}
              </button>
              {renderHamburgerMenu()}
            </div>
          </header>

          <div className="max-w-2xl mx-auto px-4 py-10 print:py-0 print:px-0 print:max-w-full">
            <div className="flex flex-col sm:flex-row sm:items-start justify-between gap-4 mb-4">
              <div>
                <span className="text-xs font-bold uppercase tracking-widest text-[#530618]">{dicc.desgloseSubtitulo}</span>
                <h2 className="text-2xl font-bold text-[#171717] mt-1">
                  {resultado.categoriaProducto || dicc.prodSeleccionado}
                </h2>
                <div className="flex flex-wrap gap-x-4 gap-y-1 text-xs text-[#555555] mt-1.5 font-medium">
                  <span>
                    {dicc.origen}: <strong className="text-[#171717]">{resultado.fichaProducto.paisOrigen}</strong>
                  </span>
                  <span>
                    {dicc.condicion}:{" "}
                    <strong className="text-[#171717]">
                      {resultado.fichaProducto.condicionImportador === "REVENTA_RI" ? dicc.condInscripto : dicc.condConsumo}
                    </strong>
                  </span>
                </div>
              </div>
              <div className="flex flex-wrap gap-2 print:hidden shrink-0">
                <button
                  type="button"
                  onClick={() => setMostrarDescProducto((prev) => !prev)}
                  className="inline-flex items-center gap-1.5 bg-white border border-[#530618] text-[#530618] hover:bg-[#530618] hover:text-white px-3.5 py-2 rounded-sm text-xs font-bold transition-all cursor-pointer"
                >
                  {mostrarDescProducto ? dicc.ocultarDesc : dicc.verDesc}
                </button>
              </div>
            </div>

            {mostrarDescProducto && (
              <div className="mb-5 p-4 bg-white border border-[#F0D0D4] rounded-sm text-xs text-[#555555] shadow-sm">
                <p className="font-bold text-[#530618] uppercase tracking-wider mb-1">{dicc.descProductoTitulo}</p>
                <p className="leading-relaxed text-[#171717]">{resultado.fichaProducto.descripcion}</p>
              </div>
            )}

            <div className="bg-white border border-[#D8DDE3] rounded-sm overflow-hidden mb-6 shadow-sm print:shadow-none print:border-gray-300">
              {[
                { label: dicc.itemFob, value: resultado.parametrosComerciales.valorFobTotal, note: dicc.noteFob, highlight: false },
                { label: dicc.itemFlete, value: resultado.parametrosComerciales.flete, note: dicc.noteFlete, highlight: false },
                { label: dicc.itemSeguro, value: resultado.parametrosComerciales.seguro, note: dicc.noteSeguro, highlight: false },
                { label: dicc.itemCif, value: resultado.parametrosComerciales.valorCIF, note: dicc.noteCif, highlight: true },
                { label: `${dicc.itemDie} (${dieItem.alicuotaNum}%)`, value: dieItem.montoUSD, note: resultado.regimenAplicado, highlight: false },
                {
                  label: `${dicc.itemEstadistica} (${estItem.alicuotaNum}%)`,
                  value: estItem.montoUSD,
                  note: resultado.esMercosurPais || resultado.esBienCapital ? dicc.noteEstadisticaExento : dicc.noteEstadisticaGral,
                  highlight: false
                },
                { label: `${dicc.itemIva} (${ivaItem.alicuotaNum}%)`, value: ivaItem.montoUSD, note: dicc.noteIva, highlight: false },
                { label: `${dicc.itemIvaAdic} (${percIvaItem.alicuotaNum}%)`, value: percIvaItem.montoUSD, note: dicc.noteIvaAdic, highlight: false },
                {
                  label: `${dicc.itemGanancias} (${percGanItem.alicuotaNum}%)`,
                  value: percGanItem.montoUSD,
                  note: resultado.fichaProducto.condicionImportador === "REVENTA_RI" ? dicc.condInscripto : dicc.condConsumo,
                  highlight: false
                },
                { label: dicc.itemSim, value: simItem.montoUSD, note: dicc.noteSim, highlight: false }
              ].map((it, idx) => (
                <div
                  key={idx}
                  className={`flex items-center justify-between px-5 py-4 border-b border-[#F4F6F8] print:border-gray-200 last:border-0 ${
                    it.highlight ? "bg-[#FFF8F8] print:bg-gray-50" : ""
                  }`}
                >
                  <div>
                    <p className={`text-sm text-[#171717] ${it.highlight ? "font-semibold" : "font-normal"}`}>{it.label}</p>
                    <p className="text-xs text-[#999] mt-0.5 print:text-gray-500">{it.note}</p>
                  </div>
                  <span className={`text-sm font-bold tabular-nums ${it.highlight ? "text-[#530618]" : "text-[#171717]"}`}>
                    USD {formatNumber(it.value)}
                  </span>
                </div>
              ))}

              <div className="flex items-center justify-between px-5 py-5 bg-[#530618] print:bg-[#530618] text-white">
                <p className="text-white/80 text-xs uppercase tracking-wider font-semibold">{dicc.costoTotalNac}</p>
                <span className="text-xl font-bold text-white tabular-nums">
                  USD {formatNumber(resultado.resumenEconomico.costoTotalNacionalizadoUSD)}
                </span>
              </div>
            </div>

            {/* ÚNICAMENTE LOS 2 BOTONES SOLICITADOS EN VER DESGLOSE */}
            <div className="print:hidden space-y-3">
              <button
                type="button"
                onClick={() => navigateTo("ports")}
                className="w-full border border-[#530618] text-[#530618] hover:bg-[#530618] hover:text-white font-bold text-sm uppercase tracking-widest py-3.5 rounded-sm transition-colors duration-200 cursor-pointer"
              >
                {dicc.verPuertos}
              </button>
              <button
                type="button"
                onClick={() => navigateTo("results")}
                className="w-full border border-[#D8DDE3] text-[#555555] hover:border-[#530618] hover:text-[#530618] font-semibold text-sm py-3.5 rounded-sm transition-colors duration-200 cursor-pointer"
              >
                {dicc.volverResultados}
              </button>
            </div>
          </div>
        </div>
      )}

      {/* VISTA 5: PUERTOS Y ADUANAS (MAPA INTERACTIVO + TARJETAS A GOOGLE MAPS EN PESTAÑA NUEVA, SIN DIRECTORIO DEBAJO) */}
      {view === "ports" && (
        <div className="min-h-screen bg-[#F4F6F8]">
          <header className="bg-[#420213] px-6 py-4 flex items-center justify-between shadow-sm">
            <div className="flex items-center cursor-pointer" onClick={() => navigateTo("hero")}>
              <img src="/logo-importa.svg" alt="IMPORTA+" className="h-6 md:h-7 w-auto object-contain select-none" />
            </div>
            <div className="flex items-center gap-3">
              <button
                type="button"
                onClick={() => navigateTo(resultado ? "breakdown" : "form")}
                className="text-white/70 text-xs font-medium hover:text-white transition-colors cursor-pointer"
              >
                {resultado ? dicc.volverDesglose : dicc.volver}
              </button>
              {renderHamburgerMenu()}
            </div>
          </header>

          <div className="max-w-5xl mx-auto px-4 py-10 space-y-8">
            <div>
              <span className="text-xs font-bold uppercase tracking-widest text-[#530618]">{dicc.pn3Tag}</span>
              <h2 className="text-2xl md:text-3xl font-bold text-[#171717] mt-1 mb-2">{dicc.puertosTitulo}</h2>
              <p className="text-[#555] text-sm mb-6">{dicc.puertosSubtitulo}</p>

              <div id="map" className="h-96 w-full rounded-sm border border-[#D8DDE3] mb-6 shadow-sm z-10" />

              <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                {puertosCatalogo.map((p) => (
                  <a
                    key={p.id || p.nombre}
                    href={p.google_maps_url || "https://www.google.com/maps"}
                    target="_blank"
                    rel="noopener noreferrer"
                    className="block bg-white border border-[#D8DDE3] rounded-sm p-4 shadow-sm hover:border-[#530618] hover:shadow-md transition-all cursor-pointer group"
                  >
                    <div className="flex justify-between items-center mb-1">
                      <span className="text-xs font-bold text-[#530618] uppercase">{etiquetaTipoPuerto(p)}</span>
                      <span className="text-[10px] text-gray-400 font-semibold">{p.operador}</span>
                    </div>
                    <div className="flex items-center justify-between gap-2 mb-1">
                      <h4 className="font-bold text-[#171717] group-hover:text-[#530618] text-base transition-colors">
                        {p.nombre}
                      </h4>
                      <span className="text-[11px] font-bold text-[#530618] shrink-0">Google Maps &#8599;</span>
                    </div>
                    <p className="text-xs text-[#555]">{p.infraestructura}</p>
                  </a>
                ))}
              </div>
            </div>
          </div>
        </div>
      )}

      {/* VISTA 6: DIRECTORIO OFICIAL DE FORWARDERS (PÁGINA PROPIA) */}
      {view === "contact" && (
        <div className="min-h-screen bg-[#F4F6F8]">
          <header className="bg-[#420213] px-6 py-4 flex items-center justify-between shadow-sm">
            <div className="flex items-center cursor-pointer" onClick={() => navigateTo("hero")}>
              <img src="/logo-importa.svg" alt="IMPORTA+" className="h-6 md:h-7 w-auto object-contain select-none" />
            </div>
            <div className="flex items-center gap-3">
              <button
                type="button"
                onClick={() => navigateTo(returnView || (resultado ? "results" : "form"))}
                className="text-white/70 text-xs font-medium hover:text-white transition-colors cursor-pointer"
              >
                {dicc.volver}
              </button>
              {renderHamburgerMenu()}
            </div>
          </header>

          <div className="max-w-5xl mx-auto px-4 py-10 space-y-6">
            {renderDirectorioGrid(false)}

            <div className="text-center pt-2">
              <button
                type="button"
                onClick={() => navigateTo(returnView || (resultado ? "results" : "form"))}
                className="inline-flex items-center justify-center bg-[#530618] hover:bg-[#3D0411] text-white font-bold text-xs uppercase tracking-widest px-8 py-3.5 rounded-sm transition-colors shadow cursor-pointer"
              >
                {dicc.volver}
              </button>
            </div>
          </div>
        </div>
      )}

      {/* VISTA 7: DIRECTORIO OFICIAL DE DESPACHANTES DE ADUANA (PÁGINA PROPIA) */}
      {view === "despachantes" && (
        <div className="min-h-screen bg-[#F4F6F8]">
          <header className="bg-[#420213] px-6 py-4 flex items-center justify-between shadow-sm">
            <div className="flex items-center cursor-pointer" onClick={() => navigateTo("hero")}>
              <img src="/logo-importa.svg" alt="IMPORTA+" className="h-6 md:h-7 w-auto object-contain select-none" />
            </div>
            <div className="flex items-center gap-3">
              <button
                type="button"
                onClick={() => navigateTo(returnView || (resultado ? "results" : "form"))}
                className="text-white/70 text-xs font-medium hover:text-white transition-colors cursor-pointer"
              >
                {dicc.volver}
              </button>
              {renderHamburgerMenu()}
            </div>
          </header>

          <div className="max-w-5xl mx-auto px-4 py-10 space-y-6">
            {renderDirectorioGrid(true)}

            <div className="text-center pt-2">
              <button
                type="button"
                onClick={() => navigateTo(returnView || (resultado ? "results" : "form"))}
                className="inline-flex items-center justify-center bg-[#530618] hover:bg-[#3D0411] text-white font-bold text-xs uppercase tracking-widest px-8 py-3.5 rounded-sm transition-colors shadow cursor-pointer"
              >
                {dicc.volver}
              </button>
            </div>
          </div>
        </div>
      )}

      {/* SWITCH DE IDIOMA FLOTANTE EN LA ESQUINA INFERIOR DERECHA */}
      <div className="fixed bottom-5 right-5 z-50 print:hidden">
        <button
          type="button"
          onClick={() => idiomaSubject.alternarIdioma()}
          title="Cambiar idioma / Switch language"
          className="inline-flex items-center gap-2 bg-[#420213] hover:bg-[#530618] border border-white/30 text-white text-xs font-bold px-4 py-2.5 rounded-full shadow-xl transition-all cursor-pointer select-none"
        >
          <svg className="w-4 h-4 text-rose-200" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" d="M21 12a9 9 0 01-9 9m9-9a9 9 0 00-9-9m9 9H3m9 9a9 9 0 01-9-9m9 9c1.657 0 3-4.03 3-9s-1.343-9-3-9m0 18c-1.657 0-3-4.03-3-9s1.343-9 3-9m-9 9a9 9 0 019-9" />
          </svg>
          <span>{dicc.toggleBtn}</span>
        </button>
      </div>
    </>
  );
}
