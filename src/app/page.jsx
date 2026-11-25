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

function traducirConceptoGravamen(item, idioma, dicc) {
  if (idioma !== "en") return item.concepto;
  switch (item.id) {
    case "derechos_importacion":
      return `${dicc.itemDie} (${item.alicuota})`;
    case "tasa_estadistica":
      return `${dicc.itemEstadistica} (${item.alicuota})`;
    case "base_interior":
      return "Customs Domestic Taxable Base (CIF + Duty + Statistical Fee)";
    case "iva_aduanero":
      return `${dicc.itemIva} (${item.alicuota})`;
    case "percepcion_iva":
      return `${dicc.itemIvaAdic} (${item.alicuota})`;
    case "percepcion_ganancias":
      return `${dicc.itemGanancias} (${item.alicuota})`;
    case "arancel_sim":
      return dicc.itemSim;
    default:
      return item.concepto;
  }
}

function traducirAlicuota(alicuota, idioma) {
  if (idioma === "en" && alicuota === "Fijo") return "Fixed";
  return alicuota;
}

export default function ImportaPlusApp() {
  // --- PATRÓN OBSERVER: Estado reactivo suscrito a IdiomaSubject (ES / EN) ---
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

  // Buscador Predictivo de Producto (Debounce 300ms + AbortController + Caché sesión anti-cuota)
  const [busqueda, setBusqueda] = useState("");
  const [sugerenciasNcm, setSugerenciasNcm] = useState([]);
  const [busquedaSinResultados, setBusquedaSinResultados] = useState(false);
  const [productoSeleccionado, setProductoSeleccionado] = useState(null);
  const [cargandoBusqueda, setCargandoBusqueda] = useState(false);
  const [mostrarDescProducto, setMostrarDescProducto] = useState(false);

  // Buscador Incremental de País de Origen (195 países)
  const [paisesCatalogo, setPaisesCatalogo] = useState([]);
  const [busquedaPais, setBusquedaPais] = useState("");
  const [sugerenciasPais, setSugerenciasPais] = useState([]);
  const [paisSeleccionado, setPaisSeleccionado] = useState(null);

  // Variables Comerciales
  const [cantidad, setCantidad] = useState(0);
  const [fobUnitario, setFobUnitario] = useState(0);
  const [flete, setFlete] = useState(0);
  const [seguro, setSeguro] = useState(0);
  const [condicionImportador, setCondicionImportador] = useState("REVENTA_RI");
  const [usaExcepcion, setUsaExcepcion] = useState(true);
  const [mensajeError, setMensajeError] = useState("");

  // Resultados, Puertos y Grilla de Forwarders
  const [resultado, setResultado] = useState(null);
  const [mostrarDescResultado, setMostrarDescResultado] = useState(false);
  const [puertosSugeridos, setPuertosSugeridos] = useState([]);
  const [forwarders, setForwarders] = useState([]);
  const [calculando, setCalculando] = useState(false);
  const [mostrarPuertosYForwarders, setMostrarPuertosYForwarders] = useState(false);
  const [mailCopiadoId, setMailCopiadoId] = useState(null);

  const forwardersRef = useRef(null);

  // Búsqueda incremental con Debounce 300ms + AbortController anti-race-conditions + Caché de sesión
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
    }, 300);

    return () => {
      clearTimeout(timer);
      controller.abort();
    };
  }, [busqueda, productoSeleccionado]);

  // Carga inicial consolidada
  useEffect(() => {
    async function cargarDirectorio() {
      try {
        const res = await fetch("/api/v1/puertos");
        const data = await res.json();
        if (Array.isArray(data.forwarders)) setForwarders(data.forwarders);
        if (Array.isArray(data.puertos)) setPuertosSugeridos(data.puertos);
        if (Array.isArray(data.paises)) setPaisesCatalogo(data.paises);
      } catch (err) {
        console.warn("Error al cargar directorio:", err);
      }
    }
    cargarDirectorio();
  }, []);

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

  const abrirForwarders = () => {
    setTimeout(() => {
      if (forwardersRef.current) {
        forwardersRef.current.scrollIntoView({ behavior: "smooth", block: "start" });
      }
    }, 60);
  };

  const redirigirWhatsapp = (fwd) => {
    const digits = limpiarNumeroWhatsapp(fwd.whatsapp);
    if (!digits) return;
    const msg = encodeURIComponent(dicc.waMensajeDefault);
    window.open(`https://wa.me/${digits}?text=${msg}`, "_blank", "noopener,noreferrer");
  };

  const copiarEmailForwarder = async (fwd) => {
    if (!fwd.email) return;
    try {
      await navigator.clipboard.writeText(fwd.email);
    } catch {
      const temp = document.createElement("textarea");
      temp.value = fwd.email;
      document.body.appendChild(temp);
      temp.select();
      document.execCommand("copy");
      document.body.removeChild(temp);
    }
    setMailCopiadoId(fwd.id);
    setTimeout(() => {
      setMailCopiadoId((prev) => (prev === fwd.id ? null : prev));
    }, 2200);
  };

  const ejecutarCalculo = async (e) => {
    e.preventDefault();
    setMensajeError("");

    if (!productoSeleccionado) {
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
          codigoNcm: productoSeleccionado.codigo_ncm,
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
        setResultado(data.simulacion);
        setMostrarDescResultado(false);
        if (data.puertosSugeridos) {
          setPuertosSugeridos(data.puertosSugeridos);
        }
        setMostrarPuertosYForwarders(false);
      }
    } catch (err) {
      setMensajeError(err.message);
    } finally {
      setCalculando(false);
    }
  };

  const localeFormato = idioma === "en" ? "en-US" : "es-AR";
  const cantidadNum = Math.max(0, Number(cantidad) || 0);
  const fobUnitNum = Math.max(0, Number(fobUnitario) || 0);
  const fobTotalEnVivo = cantidadNum * fobUnitNum;
  const tieneExcepcionProducto = Boolean(
    productoSeleccionado &&
    productoSeleccionado.anexo_excepcion &&
    productoSeleccionado.arancel_excepcion !== null &&
    productoSeleccionado.arancel_excepcion !== undefined
  );

  return (
    <div className="min-h-screen bg-slate-50 text-slate-900 font-sans">
      {/* Header Institucional IMPORTA+ con Botón Alternador Multi-idioma (Observer) */}
      <header className="bg-[#420213] text-white py-4 px-6 shadow-md print:bg-white print:text-black print:border-b">
        <div className="max-w-5xl mx-auto flex flex-row justify-between items-center gap-3">
          <div className="flex items-center gap-4">
            <div className="cursor-pointer">
              <img src="/logo-importa.svg" alt="IMPORTA+" className="h-7 md:h-8 w-auto object-contain select-none" />
            </div>
            <p className="text-xs text-rose-100/90 sm:whitespace-nowrap hidden sm:block border-l border-white/20 pl-4">
              {dicc.heroSubtitle}
            </p>
          </div>

          <button
            type="button"
            onClick={() => idiomaSubject.alternarIdioma()}
            title="Cambiar idioma / Switch language"
            className="print:hidden inline-flex items-center gap-2 bg-white/10 hover:bg-white/20 border border-white/25 text-white text-xs font-bold px-3.5 py-1.5 rounded-full transition-colors cursor-pointer select-none"
          >
            <svg className="w-3.5 h-3.5 text-rose-200" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" d="M21 12a9 9 0 01-9 9m9-9a9 9 0 00-9-9m9 9H3m9 9a9 9 0 01-9-9m9 9c1.657 0 3-4.03 3-9s-1.343-9-3-9m0 18c-1.657 0-3-4.03-3-9s1.343-9 3-9m-9 9a9 9 0 019-9" />
            </svg>
            <span>{dicc.toggleBtn}</span>
          </button>
        </div>
      </header>

      <main className="max-w-5xl mx-auto p-4 sm:p-6 space-y-8">
        {/* PN1: Formulario de Liquidación Aduanera */}
        <section className="bg-white p-6 sm:p-8 rounded-xl shadow-sm border border-slate-200 print:hidden">
          <div className="flex items-center justify-between mb-4 border-b pb-3">
            <div>
              <span className="text-xs font-bold uppercase tracking-widest text-[#530618]">
                {dicc.paso1}
              </span>
              <h2 className="text-xl font-bold text-slate-800">
                {dicc.tituloLiquidacion}
              </h2>
            </div>
          </div>

          {mensajeError && (
            <div
              role="alert"
              className="mb-4 p-3.5 bg-rose-50 border border-rose-300 text-[#530618] rounded-lg text-xs font-semibold flex items-center justify-between gap-3"
            >
              <span>{mensajeError}</span>
              <button
                type="button"
                onClick={() => setMensajeError("")}
                className="text-rose-800 hover:text-[#530618] font-bold px-2 py-0.5 rounded"
                aria-label="Cerrar alerta"
              >
                ✕
              </button>
            </div>
          )}

          <form onSubmit={ejecutarCalculo} className="space-y-5">
            <div className="relative">
              <label className="block text-sm font-semibold text-slate-700 mb-1">
                {dicc.lblProducto} <span className="text-rose-600">*</span>
              </label>
              <input
                type="text"
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
                className="w-full p-3.5 border border-slate-300 rounded-lg focus:ring-2 focus:ring-[#530618] focus:border-[#530618] outline-none text-slate-900 shadow-sm"
              />

              {cargandoBusqueda && (
                <div className="absolute right-4 top-10 text-xs text-slate-400 animate-pulse">
                  {dicc.buscandoProducto}
                </div>
              )}

              {sugerenciasNcm.length > 0 && !productoSeleccionado && (
                <ul className="absolute z-20 w-full bg-white border border-slate-300 rounded-lg mt-1 shadow-2xl max-h-72 overflow-y-auto divide-y divide-slate-100">
                  {sugerenciasNcm.map((item) => (
                    <li
                      key={item.codigo_ncm}
                      onMouseDown={(e) => {
                        e.preventDefault();
                        setProductoSeleccionado(item);
                        setBusqueda(item.descripcion);
                        setUsaExcepcion(true);
                        setSugerenciasNcm([]);
                        setBusquedaSinResultados(false);
                        setMensajeError("");
                      }}
                      className="p-3.5 hover:bg-rose-50/60 cursor-pointer transition-colors text-sm"
                    >
                      <div className="flex justify-between items-start gap-2">
                        <div>
                          <span className="text-[11px] font-mono font-bold text-[#530618] mr-2">
                            {item.codigo_ncm}
                          </span>
                          <span className="font-semibold text-slate-900 leading-snug">
                            {item.descripcion}
                          </span>
                        </div>
                        <span className="ml-3 shrink-0 text-xs bg-slate-100 text-slate-700 px-2.5 py-1 rounded font-medium">
                          {dicc.arancel}: {item.anexo_excepcion && item.arancel_excepcion !== null ? item.arancel_excepcion : item.aec}%
                        </span>
                      </div>
                    </li>
                  ))}
                </ul>
              )}

              {busquedaSinResultados && !cargandoBusqueda && !productoSeleccionado && busqueda.trim().length >= 2 && (
                <div className="absolute z-20 w-full bg-white border border-slate-300 rounded-lg mt-1 shadow-lg p-3.5 text-xs text-slate-600">
                  {dicc.sinProductos}
                </div>
              )}

              {productoSeleccionado && (
                <div className="mt-3 p-3.5 bg-rose-50/50 border border-rose-200 rounded-lg text-xs space-y-2">
                  <div className="flex flex-wrap justify-between items-center gap-2">
                    <span className="font-bold text-[#530618]">
                      {dicc.prodSeleccionado} ({productoSeleccionado.codigo_ncm}) · {dicc.arancelBase}: {productoSeleccionado.aec}%
                    </span>
                    <div className="flex items-center gap-3">
                      <button
                        type="button"
                        onClick={() => setMostrarDescProducto(!mostrarDescProducto)}
                        className="font-bold text-[#530618] bg-white border border-rose-200 px-2.5 py-1 rounded hover:bg-[#530618] hover:text-white transition-colors"
                      >
                        {mostrarDescProducto ? dicc.ocultarDesc : dicc.verDesc}
                      </button>
                      <button
                        type="button"
                        onClick={() => {
                          setProductoSeleccionado(null);
                          setBusqueda("");
                          setMostrarDescProducto(false);
                        }}
                        className="text-rose-700 hover:underline font-semibold"
                      >
                        {dicc.cambiar}
                      </button>
                    </div>
                  </div>

                  {tieneExcepcionProducto && (
                    <label className="flex flex-wrap items-center justify-between gap-2 pt-2 border-t border-rose-200 text-emerald-800 cursor-pointer">
                      <span className="flex items-center gap-2 font-semibold">
                        <input
                          type="checkbox"
                          checked={usaExcepcion}
                          onChange={(e) => setUsaExcepcion(e.target.checked)}
                          className="accent-emerald-700 rounded"
                        />
                        {dicc.beneficioDisponible} {productoSeleccionado.anexo_excepcion}
                      </span>
                      <span className="font-bold bg-emerald-100 text-emerald-800 px-2 py-0.5 rounded">
                        {dicc.arancelReducido}: {productoSeleccionado.arancel_excepcion}%
                      </span>
                    </label>
                  )}

                  {mostrarDescProducto && (
                    <div className="pt-2 border-t border-rose-200 text-slate-700 leading-relaxed">
                      {productoSeleccionado.descripcion}
                    </div>
                  )}
                </div>
              )}
            </div>

            <div className="grid grid-cols-1 md:grid-cols-3 gap-4 pt-2">
              <div className="relative">
                <label className="block text-xs font-semibold text-slate-700 mb-1 uppercase tracking-wider">
                  {dicc.lblPais} *
                </label>
                <input
                  type="text"
                  value={busquedaPais}
                  onKeyDown={(e) => {
                    if (e.ctrlKey || e.metaKey || e.altKey) return;
                    const permitidas = ["Backspace", "Delete", "Tab", "Escape", "Enter", "ArrowLeft", "ArrowRight", "ArrowUp", "ArrowDown", "Home", "End", " "];
                    if (permitidas.includes(e.key)) {
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
                  className="w-full p-2.5 border border-slate-300 rounded-lg bg-white focus:ring-2 focus:ring-[#530618]"
                />
                {sugerenciasPais.length > 0 && (
                  <ul className="absolute z-20 w-full bg-white border border-slate-300 rounded-lg mt-1 shadow-xl max-h-60 overflow-y-auto divide-y divide-slate-100">
                    {sugerenciasPais.map((p) => (
                      <li
                        key={p.codigo_iso}
                        onMouseDown={(e) => {
                          e.preventDefault();
                          setPaisSeleccionado(p);
                          setBusquedaPais(p.nombre);
                          setSugerenciasPais([]);
                          setMensajeError("");
                        }}
                        className="p-2.5 hover:bg-rose-50 cursor-pointer text-xs flex justify-between items-center"
                      >
                        <span className="font-semibold text-slate-800">{p.nombre}</span>
                        <span className={`px-2 py-0.5 rounded text-[10px] ${p.es_mercosur ? "bg-emerald-50 text-emerald-700 font-bold" : "bg-slate-100 text-slate-600"}`}>
                          {p.es_mercosur ? dicc.mercosur : dicc.extrazona}
                        </span>
                      </li>
                    ))}
                  </ul>
                )}
              </div>

              <div>
                <label className="block text-xs font-semibold text-slate-700 mb-1 uppercase tracking-wider">
                  {dicc.lblCantidad} *
                </label>
                <input
                  type="number"
                  min="0"
                  required
                  value={cantidad}
                  onFocus={(e) => { if (Number(e.target.value) === 0) e.target.select(); }}
                  onChange={(e) => setCantidad(e.target.value === "" ? 0 : e.target.value)}
                  className="w-full p-2.5 border border-slate-300 rounded-lg focus:ring-2 focus:ring-[#530618]"
                />
              </div>

              <div>
                <label className="block text-xs font-semibold text-slate-700 mb-1 uppercase tracking-wider">
                  {dicc.lblFobUnitario} *
                </label>
                <input
                  type="number"
                  step="0.01"
                  min="0"
                  required
                  value={fobUnitario}
                  onFocus={(e) => { if (Number(e.target.value) === 0) e.target.select(); }}
                  onChange={(e) => setFobUnitario(e.target.value === "" ? 0 : e.target.value)}
                  className="w-full p-2.5 border border-slate-300 rounded-lg focus:ring-2 focus:ring-[#530618]"
                />
              </div>
            </div>

            {/* Barra de Subtotal FOB en vivo alineada con diseño institucional */}
            <div className="bg-slate-50 border border-slate-200 rounded-lg px-4 py-3 flex flex-wrap items-center justify-between gap-2">
              <div>
                <p className="text-xs font-bold uppercase tracking-wider text-slate-600">
                  {dicc.lblFobTotalVivo}
                </p>
                <p className="text-[11px] text-[#530618] font-semibold mt-0.5">
                  {cantidadNum} {dicc.unidAbrev} × USD {fobUnitNum.toLocaleString(localeFormato, { minimumFractionDigits: 2, maximumFractionDigits: 2 })} = USD {fobTotalEnVivo.toLocaleString(localeFormato, { minimumFractionDigits: 2, maximumFractionDigits: 2 })}
                </p>
              </div>
              <span className="text-lg font-bold text-[#530618] tabular-nums">
                USD {fobTotalEnVivo.toLocaleString(localeFormato, { minimumFractionDigits: 2, maximumFractionDigits: 2 })}
              </span>
            </div>

            <div>
              <div className="flex items-center justify-between mb-2">
                <div className="flex items-center gap-2">
                  <span className="text-xs font-bold uppercase tracking-widest text-[#530618]">{dicc.costosAdicionales}</span>
                  <span className="text-[10px] font-semibold border border-slate-300 text-slate-500 px-1.5 py-0.5 rounded uppercase">{dicc.opcional}</span>
                </div>
                <button
                  type="button"
                  onClick={abrirForwarders}
                  className="bg-[#530618] hover:bg-[#3D0411] text-white text-xs font-semibold px-3 py-1.5 rounded transition-colors shadow-sm"
                >
                  {dicc.btnContactarForwarder}
                </button>
              </div>
              <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                <div>
                  <label className="block text-xs font-semibold text-slate-700 mb-1 uppercase tracking-wider">
                    {dicc.lblFlete}
                  </label>
                  <input
                    type="number"
                    step="0.01"
                    min="0"
                    value={flete}
                    onFocus={(e) => { if (Number(e.target.value) === 0) e.target.select(); }}
                    onChange={(e) => setFlete(e.target.value === "" ? 0 : e.target.value)}
                    className="w-full p-2.5 border border-slate-300 rounded-lg text-sm"
                  />
                </div>

                <div>
                  <label className="block text-xs font-semibold text-slate-700 mb-1 uppercase tracking-wider">
                    {dicc.lblSeguro}
                  </label>
                  <input
                    type="number"
                    step="0.01"
                    min="0"
                    value={seguro}
                    onFocus={(e) => { if (Number(e.target.value) === 0) e.target.select(); }}
                    onChange={(e) => setSeguro(e.target.value === "" ? 0 : e.target.value)}
                    className="w-full p-2.5 border border-slate-300 rounded-lg text-sm"
                  />
                </div>
              </div>
            </div>

            <div className="p-4 bg-slate-50 border border-slate-200 rounded-lg">
              <label className="block text-xs font-bold uppercase tracking-wider text-slate-700 mb-2">
                {dicc.lblCondicionImpositiva}
              </label>
              <div className="grid grid-cols-1 sm:grid-cols-2 gap-3">
                <label className="flex items-center gap-2.5 p-2.5 bg-white border border-slate-200 rounded-lg cursor-pointer hover:border-[#530618] transition-colors">
                  <input
                    type="radio"
                    name="condicionImportador"
                    value="REVENTA_RI"
                    checked={condicionImportador === "REVENTA_RI"}
                    onChange={() => setCondicionImportador("REVENTA_RI")}
                    className="accent-[#530618]"
                  />
                  <span className="text-xs font-bold text-slate-800">
                    {dicc.condInscripto}
                  </span>
                </label>

                <label className="flex items-center gap-2.5 p-2.5 bg-white border border-slate-200 rounded-lg cursor-pointer hover:border-[#530618] transition-colors">
                  <input
                    type="radio"
                    name="condicionImportador"
                    value="USO_PROPIO_CONSUMO"
                    checked={condicionImportador === "USO_PROPIO_CONSUMO"}
                    onChange={() => setCondicionImportador("USO_PROPIO_CONSUMO")}
                    className="accent-[#530618]"
                  />
                  <span className="text-xs font-bold text-slate-800">
                    {dicc.condConsumo}
                  </span>
                </label>
              </div>
            </div>

            <button
              id="btn-acceder-calculadora"
              type="submit"
              disabled={calculando}
              className="w-full bg-[#530618] hover:bg-[#3D0411] text-white font-bold py-4 rounded-lg transition-all duration-200 shadow-md uppercase tracking-widest text-sm flex items-center justify-center gap-2 disabled:opacity-50"
            >
              {calculando ? dicc.calculando : dicc.btnCalcular}
            </button>
          </form>
        </section>

        {/* PN2: Comprobante de Liquidación Aduanera */}
        {resultado && (
          <section className="bg-white p-6 sm:p-8 rounded-xl shadow-sm border border-slate-200 space-y-6">
            <div className="flex flex-wrap justify-between items-center border-b pb-4 gap-3">
              <div>
                <div className="flex items-center gap-2">
                  <span className="text-xs font-bold uppercase tracking-widest text-[#530618]">
                    {dicc.tituloLiquidacion}
                  </span>
                  <span className="text-xs font-mono font-bold bg-slate-900 text-white px-2.5 py-1 rounded">
                    ID: {resultado.idCotizacion}
                  </span>
                </div>
                <p className="text-xs text-slate-500 mt-1">
                  {dicc.origen}: <strong>{resultado.fichaProducto.paisOrigen}</strong> ({resultado.fichaProducto.regimenAplicado})
                </p>
              </div>

              <div className="flex flex-wrap gap-2 print:hidden">
                <button
                  type="button"
                  onClick={() => setMostrarDescResultado(!mostrarDescResultado)}
                  className="px-3.5 py-2.5 text-xs font-bold border border-[#530618] text-[#530618] hover:bg-[#530618] hover:text-white rounded-lg transition-colors"
                >
                  {mostrarDescResultado ? dicc.ocultarDesc : dicc.verDesc}
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
                  className="px-4 py-2.5 text-xs font-bold bg-emerald-700 hover:bg-emerald-800 text-white rounded-lg transition-colors flex items-center gap-1.5"
                >
                  {dicc.btnExportarExcel}
                </button>
                <button
                  type="button"
                  onClick={() => window.print()}
                  className="px-4 py-2.5 text-xs font-bold bg-slate-800 hover:bg-slate-700 text-white rounded-lg transition-colors flex items-center gap-1.5"
                >
                  {dicc.btnImprimirPdf}
                </button>
              </div>
            </div>

            {mostrarDescResultado && (
              <div className="p-4 bg-rose-50/50 border border-rose-200 rounded-lg text-sm text-slate-800">
                <span className="text-xs font-bold uppercase tracking-wider text-[#530618] block mb-1">
                  {dicc.descProductoTitulo}
                </span>
                {resultado.fichaProducto.descripcion}
              </div>
            )}

            <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
              <div className="p-5 bg-slate-50 rounded-xl border border-slate-200">
                <p className="text-xs text-slate-500 uppercase tracking-wider font-semibold">
                  {dicc.card1Titulo}
                </p>
                <p className="text-2xl font-black text-slate-800 mt-1">
                  USD {resultado.parametrosComerciales.valorFobTotal.toLocaleString(localeFormato, { minimumFractionDigits: 2 })}
                </p>
                <p className="text-xs text-slate-500 mt-1">
                  {dicc.itemFlete}: USD {resultado.parametrosComerciales.flete.toLocaleString(localeFormato, { minimumFractionDigits: 2 })} | {dicc.itemSeguro}: USD {resultado.parametrosComerciales.seguro.toLocaleString(localeFormato, { minimumFractionDigits: 2 })}
                </p>
                <p className="text-xs font-semibold text-slate-600 mt-2 pt-2 border-t border-slate-200">
                  {dicc.card1Cif}: USD {resultado.parametrosComerciales.valorCIF.toLocaleString(localeFormato, { minimumFractionDigits: 2 })}
                </p>
              </div>

              <div className="p-5 bg-amber-50 rounded-xl border border-amber-200">
                <p className="text-xs text-amber-800 uppercase tracking-wider font-semibold">
                  {dicc.card2Titulo}
                </p>
                <p className="text-2xl font-black text-amber-700 mt-1">
                  + USD {resultado.resumenEconomico.totalGravamenesUSD.toLocaleString(localeFormato, { minimumFractionDigits: 2 })}
                </p>
                <p className="text-xs text-amber-800 mt-1">
                  {dicc.card2Incidencia} <strong>+{resultado.resumenEconomico.incidenciaFiscalPct}%</strong>
                </p>
              </div>

              <div className="p-5 bg-rose-50 rounded-xl border border-rose-200">
                <p className="text-xs text-[#530618] uppercase tracking-wider font-semibold">
                  {dicc.card3Titulo}
                </p>
                <p className="text-2xl font-black text-[#530618] mt-1">
                  USD {resultado.resumenEconomico.costoTotalNacionalizadoUSD.toLocaleString(localeFormato, { minimumFractionDigits: 2 })}
                </p>
                <p className="text-xs font-bold text-[#530618] mt-1">
                  {dicc.costoUnitarioNac} USD {resultado.resumenEconomico.costoUnitarioFinalUSD.toLocaleString(localeFormato, { minimumFractionDigits: 2 })} / {dicc.unidad}
                </p>
                <p className="text-xs text-rose-800 mt-2 pt-2 border-t border-rose-200">
                  ≈ ARS ${resultado.resumenEconomico.costoTotalEstimadoARS.toLocaleString(localeFormato, { minimumFractionDigits: 2 })} ({dicc.card3Tc}: ${resultado.resumenEconomico.tipoCambioBNA.toLocaleString(localeFormato, { minimumFractionDigits: 2 })})
                </p>
              </div>
            </div>

            <div className="border border-slate-200 rounded-lg overflow-hidden">
              <div className="bg-slate-100 px-4 py-2.5 font-bold text-xs uppercase tracking-wider text-slate-700">
                {dicc.tablaTitulo}
              </div>
              <div className="overflow-x-auto">
                <table className="w-full text-sm text-left border-collapse">
                  <thead>
                    <tr className="bg-slate-50 text-slate-600 border-b text-xs uppercase">
                      <th className="p-3">{dicc.thConcepto}</th>
                      <th className="p-3">{dicc.thAlicuota}</th>
                      <th className="p-3 text-right">{dicc.thImporte}</th>
                    </tr>
                  </thead>
                  <tbody className="divide-y divide-slate-100">
                    {resultado.desgloseTributario.map((item, idx) => (
                      <tr
                        key={item.id || idx}
                        className={item.esSubtotal ? "bg-slate-50 font-bold text-slate-900" : "hover:bg-slate-50/50"}
                      >
                        <td className="p-3">
                          <span className="block font-medium">{traducirConceptoGravamen(item, idioma, dicc)}</span>
                        </td>
                        <td className="p-3 font-mono text-xs">{traducirAlicuota(item.alicuota, idioma)}</td>
                        <td className="p-3 text-right font-mono font-semibold">
                          USD {item.montoUSD.toLocaleString(localeFormato, { minimumFractionDigits: 2 })}
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            </div>

            <div className="pt-4 border-t flex flex-wrap justify-between items-center gap-3 print:hidden">
              <p className="text-xs text-slate-600">
                {dicc.preguntaPuertos}
              </p>
              <button
                type="button"
                onClick={() => setMostrarPuertosYForwarders(!mostrarPuertosYForwarders)}
                className="bg-[#530618] hover:bg-[#3D0411] text-white px-6 py-3 rounded-lg text-sm font-bold transition shadow-sm flex items-center gap-2"
              >
                <span>{mostrarPuertosYForwarders ? dicc.ocultarPuertos : dicc.verPuertos}</span>
                <span>&rarr;</span>
              </button>
            </div>
          </section>
        )}

        {/* PN3: Módulo de Puertos y Aduanas */}
        {mostrarPuertosYForwarders && (
          <section className="space-y-6 pt-2 print:hidden">
            <div className="bg-white p-6 sm:p-8 rounded-xl shadow-sm border border-slate-200 space-y-4">
              <div className="flex items-center justify-between border-b pb-3">
                <div>
                  <span className="text-xs font-bold uppercase tracking-widest text-[#530618]">
                    {dicc.pn3Tag}
                  </span>
                  <h3 className="text-xl font-bold text-slate-800">
                    {dicc.puertosSugeridosPara} {resultado?.fichaProducto.paisOrigen || ""}
                  </h3>
                </div>
                <span className="text-xs bg-rose-50 text-[#530618] px-3 py-1 rounded-full font-semibold">
                  {dicc.puntosHabilitados}
                </span>
              </div>

              <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                {puertosSugeridos.map((puerto) => (
                  <div
                    key={puerto.id || puerto.nombre}
                    className="p-4 border border-slate-200 rounded-lg hover:border-[#530618] transition-colors bg-slate-50/50"
                  >
                    <div className="flex justify-between items-center mb-1">
                      <span className="text-xs font-bold text-[#530618] uppercase">
                        {puerto.via_ingreso}
                      </span>
                      <span className="text-xs font-mono text-slate-500">{puerto.codigo_sim}</span>
                    </div>
                    <h4 className="font-bold text-slate-900 text-base">{puerto.nombre}</h4>
                    <p className="text-xs text-slate-600 mt-1">{puerto.infraestructura}</p>
                    <div className="flex flex-wrap gap-1.5 mt-3">
                      {(puerto.tipos_carga || []).map((tipo, tIdx) => (
                        <span key={tIdx} className="text-[10px] bg-slate-200 text-slate-700 px-2 py-0.5 rounded font-medium">
                          {tipo}
                        </span>
                      ))}
                    </div>
                  </div>
                ))}
              </div>
            </div>
          </section>
        )}

        {/* PN4: APARTADO FORWARDERS - GRILLA DE 4 POR FILA */}
        {forwarders.length > 0 && (
          <section ref={forwardersRef} className="bg-white p-6 sm:p-8 rounded-xl shadow-sm border border-slate-200 space-y-6 print:hidden">
            <div className="border-b pb-3">
              <span className="text-xs font-bold uppercase tracking-widest text-[#530618]">
                {dicc.fwdTag}
              </span>
              <h3 className="text-xl font-bold text-slate-800 mt-0.5">
                {dicc.fwdTitulo}
              </h3>
              <p className="text-xs text-slate-500 mt-1">
                {dicc.fwdSubtitulo}
              </p>
            </div>

            <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-5">
              {forwarders.map((fwd) => {
                const nombreCortado = String(fwd.nombre || "").slice(0, 120);
                const copiado = mailCopiadoId === fwd.id;
                const imgSegura = esUrlImagenSeguraCliente(fwd.imagen_url) ? fwd.imagen_url : "/logo-importa.png";
                return (
                  <div
                    key={fwd.id}
                    className="bg-[#FFF8F8] border border-[#F0D0D4] rounded-xl overflow-hidden shadow-sm hover:shadow-md hover:border-[#530618] transition-all flex flex-col justify-between"
                  >
                    <div className="w-full aspect-square bg-slate-200 overflow-hidden relative">
                      <img
                        src={imgSegura}
                        alt={nombreCortado}
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
                        title={nombreCortado}
                      >
                        {nombreCortado}
                      </h4>

                      <div className="space-y-2">
                        <button
                          type="button"
                          onClick={() => redirigirWhatsapp(fwd)}
                          className="w-full flex items-center justify-center gap-2 bg-emerald-600 hover:bg-emerald-700 text-white font-bold text-xs py-2.5 px-3 rounded-lg transition-colors shadow-sm cursor-pointer"
                        >
                          <svg className="w-4 h-4 fill-current shrink-0" viewBox="0 0 24 24">
                            <path d="M20.52 3.48A11.82 11.82 0 0012.06 0C5.5 0 .16 5.34.16 11.9c0 2.1.55 4.15 1.6 5.95L0 24l6.32-1.66a11.86 11.86 0 005.74 1.46h.01c6.55 0 11.89-5.34 11.89-11.9 0-3.18-1.24-6.17-3.44-8.42zM12.07 21.8h-.01a9.87 9.87 0 01-5.03-1.38l-.36-.21-3.75.98 1-3.65-.24-.38a9.85 9.85 0 01-1.51-5.26c0-5.45 4.44-9.89 9.9-9.89 2.64 0 5.13 1.03 7 2.9a9.83 9.83 0 012.9 7c0 5.45-4.44 9.89-9.9 9.89zm5.43-7.41c-.3-.15-1.76-.87-2.03-.97-.27-.1-.47-.15-.67.15-.2.3-.77.97-.94 1.16-.17.2-.35.22-.65.07-.3-.15-1.26-.46-2.4-1.48-.89-.79-1.49-1.77-1.66-2.07-.17-.3-.02-.46.13-.61.13-.13.3-.35.45-.52.15-.17.2-.3.3-.5.1-.2.05-.37-.02-.52-.08-.15-.67-1.61-.92-2.21-.24-.58-.49-.5-.67-.51h-.57c-.2 0-.52.07-.79.37-.27.3-1.04 1.02-1.04 2.48 0 1.46 1.07 2.88 1.22 3.08.15.2 2.1 3.2 5.08 4.49.71.31 1.26.49 1.69.63.71.23 1.36.2 1.87.12.57-.09 1.76-.72 2.01-1.41.25-.7.25-1.29.17-1.41-.07-.12-.27-.2-.57-.35z" />
                          </svg>
                          <span>{dicc.btnWhatsapp}</span>
                        </button>

                        <button
                          type="button"
                          onClick={() => copiarEmailForwarder(fwd)}
                          className={`w-full flex items-center justify-center gap-2 font-bold text-xs py-2.5 px-3 rounded-lg transition-colors shadow-sm cursor-pointer ${
                            copiado
                              ? "bg-[#3D0411] text-rose-100 ring-2 ring-[#530618]"
                              : "bg-[#530618] hover:bg-[#3D0411] text-white"
                          }`}
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
          </section>
        )}
      </main>
    </div>
  );
}
