"use client";
import React, { useState, useEffect } from "react";

export default function ImportaPlusApp() {
  // Buscador Predictivo de Producto por Nombre (En blanco al iniciar, sin código NCM)
  const [busqueda, setBusqueda] = useState("");
  const [sugerenciasNcm, setSugerenciasNcm] = useState([]);
  const [productoSeleccionado, setProductoSeleccionado] = useState(null);
  const [cargandoBusqueda, setCargandoBusqueda] = useState(false);
  const [mostrarDescProducto, setMostrarDescProducto] = useState(false);

  // Buscador Incremental de País de Origen (195 países)
  const [paisesCatalogo, setPaisesCatalogo] = useState([]);
  const [busquedaPais, setBusquedaPais] = useState("");
  const [sugerenciasPais, setSugerenciasPais] = useState([]);
  const [paisSeleccionado, setPaisSeleccionado] = useState(null);

  // Variables Comerciales (Inicializadas en 0)
  const [cantidad, setCantidad] = useState(0);
  const [fobUnitario, setFobUnitario] = useState(0);
  const [flete, setFlete] = useState(0);
  const [seguro, setSeguro] = useState(0);
  const [condicionImportador, setCondicionImportador] = useState("REVENTA_RI");
  const [usaExcepcion, setUsaExcepcion] = useState(true);

  // Resultados y Flujo Post-Cálculo (PN1 -> PN2 -> PN3 -> PN4)
  const [resultado, setResultado] = useState(null);
  const [mostrarDescResultado, setMostrarDescResultado] = useState(false);
  const [puertosSugeridos, setPuertosSugeridos] = useState([]);
  const [operadores, setOperadores] = useState([]);
  const [calculando, setCalculando] = useState(false);
  const [mostrarPuertosYForwarders, setMostrarPuertosYForwarders] = useState(false);

  // Modal de Contacto Forwarder / Despachante
  const [modalOperador, setModalOperador] = useState(null);
  const [leadNombre, setLeadNombre] = useState("");
  const [leadEmpresa, setLeadEmpresa] = useState("");
  const [leadEmail, setLeadEmail] = useState("");
  const [leadTelefono, setLeadTelefono] = useState("");
  const [leadComentarios, setLeadComentarios] = useState("");
  const [enviandoLead, setEnviandoLead] = useState(false);
  const [leadEnviadoExito, setLeadEnviadoExito] = useState(false);

  // Búsqueda incremental por nombre de producto (< 250ms)
  useEffect(() => {
    if (busqueda.trim().length < 2 || productoSeleccionado) {
      setSugerenciasNcm([]);
      return;
    }

    const timer = setTimeout(async () => {
      setCargandoBusqueda(true);
      try {
        const res = await fetch(`/api/v1/ncm/search?q=${encodeURIComponent(busqueda)}`);
        const data = await res.json();
        setSugerenciasNcm(data.results || []);
      } catch (err) {
        console.error("Error al buscar productos:", err);
      } finally {
        setCargandoBusqueda(false);
      }
    }, 200);

    return () => clearTimeout(timer);
  }, [busqueda, productoSeleccionado]);

  // Carga inicial de los 195 países, operadores y puertos desde Excel
  useEffect(() => {
    async function cargarDirectorio() {
      try {
        const res = await fetch("/api/v1/puertos");
        const data = await res.json();
        if (data.operadores) setOperadores(data.operadores);
        if (data.puertos && puertosSugeridos.length === 0) setPuertosSugeridos(data.puertos);
        if (data.paises) setPaisesCatalogo(data.paises);
      } catch (err) {
        console.warn("Error al cargar directorio:", err);
      }
    }
    cargarDirectorio();
  }, []);

  // Filtrado incremental de países (exclusivamente por letras)
  const filtrarPaises = (texto) => {
    const normalizar = (s) =>
      String(s || "")
        .toLowerCase()
        .normalize("NFD")
        .replace(/[\u0300-\u036f]/g, "");

    const limpio = String(texto || "").replace(/[^a-zA-ZáéíóúÁÉÍÓÚñÑüÜ\s]/g, "").trim();
    const term = normalizar(limpio);
    if (!term) {
      setSugerenciasPais(paisesCatalogo.slice(0, 15));
      return;
    }

    const filtrados = paisesCatalogo.filter((p) =>
      normalizar(p.nombre).includes(term)
    );
    filtrados.sort((a, b) => {
      const aStarts = normalizar(a.nombre).startsWith(term);
      const bStarts = normalizar(b.nombre).startsWith(term);
      if (aStarts && !bStarts) return -1;
      if (!aStarts && bStarts) return 1;
      return normalizar(a.nombre).localeCompare(normalizar(b.nombre));
    });
    setSugerenciasPais(filtrados.slice(0, 15));
  };

  // Ejecución de la simulación aduanera
  const ejecutarCalculo = async (e) => {
    e.preventDefault();
    if (!productoSeleccionado) {
      alert("Por favor escribí y seleccioná un producto a importar.");
      return;
    }

    let paisFinal = paisSeleccionado;
    if (!paisFinal && busquedaPais.trim()) {
      const normalizar = (s) =>
        String(s || "")
          .toLowerCase()
          .normalize("NFD")
          .replace(/[\u0300-\u036f]/g, "");
      const t = normalizar(busquedaPais.trim());
      paisFinal =
        paisesCatalogo.find((p) => normalizar(p.nombre) === t) ||
        paisesCatalogo.find((p) => normalizar(p.nombre).includes(t)) ||
        null;
    }

    if (!paisFinal) {
      alert("Por favor escribí y seleccioná un país de origen.");
      return;
    }

    if (Number(cantidad) <= 0) {
      alert("La cantidad de unidades debe ser mayor a 0.");
      return;
    }

    if (Number(fobUnitario) <= 0) {
      alert("El valor por unidad (USD FOB) debe ser mayor a 0.");
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
      alert("Error: " + err.message);
    } finally {
      setCalculando(false);
    }
  };

  // Enviar solicitud de contacto al Despachante/Forwarder
  const enviarSolicitudLead = async (e) => {
    e.preventDefault();
    if (!modalOperador) return;

    setEnviandoLead(true);
    try {
      const res = await fetch("/api/v1/leads", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({
          operadorId: modalOperador.id,
          idCotizacion: resultado?.idCotizacion || null,
          nombreApellido: leadNombre,
          empresa: leadEmpresa || null,
          email: leadEmail,
          telefono: leadTelefono,
          comentarios: leadComentarios || null
        })
      });

      const data = await res.json();
      if (!res.ok) throw new Error(data.error || "Error al enviar solicitud");

      setLeadEnviadoExito(true);
      setTimeout(() => {
        setLeadEnviadoExito(false);
        setModalOperador(null);
        setLeadNombre("");
        setLeadEmpresa("");
        setLeadEmail("");
        setLeadTelefono("");
        setLeadComentarios("");
      }, 2500);
    } catch (err) {
      alert("Error: " + err.message);
    } finally {
      setEnviandoLead(false);
    }
  };

  return (
    <div className="min-h-screen bg-slate-50 text-slate-900 font-sans">
      {/* Header Institucional IMPORTA+ */}
      <header className="bg-[#420213] text-white py-4 px-6 shadow-md print:bg-white print:text-black print:border-b">
        <div className="max-w-5xl mx-auto flex flex-col md:flex-row justify-between items-start md:items-center gap-3">
          <div className="flex items-center gap-4">
            <div className="cursor-pointer">
              <img src="/logo-importa.png" alt="IMPORTA+" className="h-7 md:h-8 w-auto object-contain select-none" />
            </div>
            <p className="text-xs text-rose-100/90 sm:whitespace-nowrap hidden sm:block border-l border-white/20 pl-4">
              Simulador oficial de costos aduaneros y tributos ARCA&nbsp;/&nbsp;DGA
            </p>
          </div>
        </div>
      </header>

      <main className="max-w-5xl mx-auto p-4 sm:p-6 space-y-8">
        {/* PN1: Formulario de Liquidación Aduanera */}
        <section className="bg-white p-6 sm:p-8 rounded-xl shadow-sm border border-slate-200 print:hidden">
          <div className="flex items-center justify-between mb-4 border-b pb-3">
            <div>
              <span className="text-xs font-bold uppercase tracking-widest text-[#530618]">
                Paso 1 de 2 · Parámetros Comerciales
              </span>
              <h2 className="text-xl font-bold text-slate-800">
                Liquidación Aduanera
              </h2>
            </div>
          </div>

          <form onSubmit={ejecutarCalculo} className="space-y-5">
            {/* Buscador Incremental exclusivamente por Nombre de Producto (En blanco al iniciar, sin código NCM) */}
            <div className="relative">
              <label className="block text-sm font-semibold text-slate-700 mb-1">
                Producto a Importar <span className="text-rose-600">*</span>
              </label>
              <input
                type="text"
                value={busqueda}
                onChange={(e) => {
                  setBusqueda(e.target.value);
                  setProductoSeleccionado(null);
                  setMostrarDescProducto(false);
                }}
                onBlur={() => setSugerenciasNcm([])}
                placeholder="Escribí el nombre del producto (ej: Notebook, Remera, Router, Calzado)..."
                className="w-full p-3.5 border border-slate-300 rounded-lg focus:ring-2 focus:ring-[#530618] focus:border-[#530618] outline-none text-slate-900 shadow-sm"
              />

              {cargandoBusqueda && (
                <div className="absolute right-4 top-10 text-xs text-slate-400 animate-pulse">
                  Buscando producto...
                </div>
              )}

              {/* Menú Desplegable con Sugerencias por Nombre */}
              {sugerenciasNcm.length > 0 && !productoSeleccionado && (
                <ul className="absolute z-20 w-full bg-white border border-slate-300 rounded-lg mt-1 shadow-2xl max-h-72 overflow-y-auto divide-y divide-slate-100">
                  {sugerenciasNcm.map((item) => (
                    <li
                      key={item.codigo_ncm}
                      onMouseDown={(e) => {
                        e.preventDefault();
                        setProductoSeleccionado(item);
                        setBusqueda(item.descripcion);
                        setSugerenciasNcm([]);
                      }}
                      className="p-3.5 hover:bg-rose-50/60 cursor-pointer transition-colors text-sm"
                    >
                      <div className="flex justify-between items-start">
                        <span className="font-semibold text-slate-900 leading-snug">
                          {item.descripcion}
                        </span>
                        <span className="ml-3 shrink-0 text-xs bg-slate-100 text-slate-700 px-2.5 py-1 rounded font-medium">
                          Arancel: {item.anexo_excepcion ? item.arancel_excepcion : item.aec}%
                        </span>
                      </div>
                    </li>
                  ))}
                </ul>
              )}

              {/* Ficha del Producto Seleccionado con Descripción Desplegable por Botón */}
              {productoSeleccionado && (
                <div className="mt-3 p-3.5 bg-rose-50/50 border border-rose-200 rounded-lg text-xs">
                  <div className="flex flex-wrap justify-between items-center gap-2">
                    <span className="font-bold text-[#530618]">
                      Producto seleccionado · Arancel Base: {productoSeleccionado.aec}%
                    </span>
                    <div className="flex items-center gap-3">
                      <button
                        type="button"
                        onClick={() => setMostrarDescProducto(!mostrarDescProducto)}
                        className="font-bold text-[#530618] bg-white border border-rose-200 px-2.5 py-1 rounded hover:bg-[#530618] hover:text-white transition-colors"
                      >
                        {mostrarDescProducto ? "Ocultar descripción del producto ▲" : "Ver descripción del producto ▼"}
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
                        Cambiar
                      </button>
                    </div>
                  </div>
                  {mostrarDescProducto && (
                    <div className="mt-2 pt-2 border-t border-rose-200 text-slate-700 leading-relaxed">
                      {productoSeleccionado.descripcion}
                    </div>
                  )}
                </div>
              )}
            </div>

            {/* Variables Comerciales: País de origen (Búsqueda incremental 195 países), Cantidad (0) y Valor FOB (0) */}
            <div className="grid grid-cols-1 md:grid-cols-3 gap-4 pt-2">
              <div className="relative">
                <label className="block text-xs font-semibold text-slate-700 mb-1 uppercase tracking-wider">
                  País de origen *
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
                    filtrarPaises(soloLetras);
                  }}
                  onFocus={(e) => filtrarPaises(e.target.value)}
                  onClick={(e) => filtrarPaises(e.target.value)}
                  onBlur={() => setSugerenciasPais([])}
                  placeholder="Buscar país de origen..."
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
                        }}
                        className="p-2.5 hover:bg-rose-50 cursor-pointer text-xs flex justify-between items-center"
                      >
                        <span className="font-semibold text-slate-800">{p.nombre}</span>
                        <span className={`px-2 py-0.5 rounded text-[10px] ${p.es_mercosur ? "bg-emerald-50 text-emerald-700 font-bold" : "bg-slate-100 text-slate-600"}`}>
                          {p.es_mercosur ? "Mercosur" : "Extrazona"}
                        </span>
                      </li>
                    ))}
                  </ul>
                )}
              </div>

              <div>
                <label className="block text-xs font-semibold text-slate-700 mb-1 uppercase tracking-wider">
                  Cantidad (Unidades) *
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
                  Valor por unidad (USD FOB) *
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

            {/* Flete internacional (USD) y Seguro de transporte (USD) inicializados en 0 */}
            <div>
              <div className="flex items-center justify-between mb-2">
                <div className="flex items-center gap-2">
                  <span className="text-xs font-bold uppercase tracking-widest text-[#530618]">Costos adicionales</span>
                  <span className="text-[10px] font-semibold border border-slate-300 text-slate-500 px-1.5 py-0.5 rounded uppercase">Opcional</span>
                </div>
                <button
                  type="button"
                  onClick={() => setModalOperador(operadores.find(o => o.tipo_operador === 'FORWARDER') || operadores[0] || { id: 'default', razon_social: 'Forwarder Internacional', matricula: 'DGA-FWD-01' })}
                  className="bg-[#530618] hover:bg-[#3D0411] text-white text-xs font-semibold px-3 py-1.5 rounded transition-colors shadow-sm"
                >
                  Contactar Fordweader
                </button>
              </div>
              <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
              <div>
                <label className="block text-xs font-semibold text-slate-700 mb-1 uppercase tracking-wider">
                  Flete internacional (USD)
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
                  Seguro de transporte (USD)
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

            {/* Condición Impositiva del Importador (Sin Aclaración) */}
            <div className="p-4 bg-slate-50 border border-slate-200 rounded-lg">
              <label className="block text-xs font-bold uppercase tracking-wider text-slate-700 mb-2">
                Condición Impositiva del Importador
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
                    Responsable Inscripto / Reventa
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
                    Consumo Particular / No categorizado
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
              {calculando ? "Procesando liquidación..." : "CALCULAR IMPORTACION"}
            </button>
          </form>
        </section>

        {/* PN2: Liquidación Aduanera (PN1) y Comprobante con Descripción Desplegable */}
        {resultado && (
          <section className="bg-white p-6 sm:p-8 rounded-xl shadow-sm border border-slate-200 space-y-6">
            <div className="flex flex-wrap justify-between items-center border-b pb-4 gap-3">
              <div>
                <div className="flex items-center gap-2">
                  <span className="text-xs font-bold uppercase tracking-widest text-[#530618]">
                    Liquidación Aduanera
                  </span>
                  <span className="text-xs font-mono font-bold bg-slate-900 text-white px-2.5 py-1 rounded">
                    ID: {resultado.idCotizacion}
                  </span>
                </div>
                <p className="text-xs text-slate-500 mt-1">
                  Origen: <strong>{resultado.fichaProducto.paisOrigen}</strong> ({resultado.fichaProducto.regimenAplicado})
                </p>
              </div>

              {/* Botón de Descripción del Producto Desplegable + Exportación Excel / PDF */}
              <div className="flex flex-wrap gap-2 print:hidden">
                <button
                  type="button"
                  onClick={() => setMostrarDescResultado(!mostrarDescResultado)}
                  className="px-3.5 py-2.5 text-xs font-bold border border-[#530618] text-[#530618] hover:bg-[#530618] hover:text-white rounded-lg transition-colors"
                >
                  {mostrarDescResultado ? "Ocultar descripción del producto ▲" : "Ver descripción del producto ▼"}
                </button>
                <button
                  type="button"
                  onClick={() => {
                    const r = resultado;
                    const filas = [
                      ["IMPORTA+ | COMPROBANTE OFICIAL DE SIMULACIÓN ADUANERA (ARCA / DGA)", "", ""],
                      ["ID Comprobante", r.idCotizacion, ""],
                      ["Fecha Emisión", new Date(r.fechaEmision || Date.now()).toLocaleString("es-AR"), ""],
                      ["Descripción", `"${String(r.fichaProducto.descripcion || "").replace(/"/g, '""')}"`, ""],
                      ["País Origen", r.fichaProducto.paisOrigen, ""],
                      ["Régimen Aplicado", r.fichaProducto.regimenAplicado, ""],
                      ["Cantidad Unidades", r.parametrosComerciales.cantidad, ""],
                      ["", "", ""],
                      ["CONCEPTO / PARTIDA ADUANERA", "ALÍCUOTA", "IMPORTE (USD)"],
                      ["Valor FOB Unitario", "Origen", r.parametrosComerciales.valorFobUnitario.toFixed(2)],
                      ["Valor FOB Total", `${r.parametrosComerciales.cantidad} u.`, r.parametrosComerciales.valorFobTotal.toFixed(2)],
                      ["Flete Internacional", "Transporte", r.parametrosComerciales.flete.toFixed(2)],
                      ["Seguro Internacional", "Póliza", r.parametrosComerciales.seguro.toFixed(2)],
                      ["VALOR CIF ADUANERO", "Base Imponible", r.parametrosComerciales.valorCIF.toFixed(2)],
                      ...r.desgloseTributario.map((it) => [
                        `"${String(it.concepto || "").replace(/"/g, '""')}"`,
                        it.alicuota,
                        Number(it.montoUSD || 0).toFixed(2)
                      ]),
                      ["", "", ""],
                      ["TOTAL GRAVÁMENES ADUANEROS", `+${r.resumenEconomico.incidenciaFiscalPct}% s/FOB`, r.resumenEconomico.totalGravamenesUSD.toFixed(2)],
                      ["COSTO TOTAL NACIONALIZADO (USD)", "Puesto en Plaza", r.resumenEconomico.costoTotalNacionalizadoUSD.toFixed(2)],
                      ["COSTO UNITARIO FINAL (USD)", "Por Unidad", r.resumenEconomico.costoUnitarioFinalUSD.toFixed(2)],
                      ["Tipo de Cambio Oficial BNA", "ARS/USD", r.resumenEconomico.tipoCambioBNA.toFixed(2)],
                      ["COSTO TOTAL ESTIMADO (ARS)", "Pesos Argentinos", r.resumenEconomico.costoTotalEstimadoARS.toFixed(2)]
                    ];
                    const csv = "\uFEFF" + filas.map((f) => f.join(";")).join("\r\n");
                    const blob = new Blob([csv], { type: "text/csv;charset=utf-8;" });
                    const url = URL.createObjectURL(blob);
                    const a = document.createElement("a");
                    a.href = url;
                    a.download = `Comprobante_ImportaPlus_${r.idCotizacion}.csv`;
                    document.body.appendChild(a);
                    a.click();
                    document.body.removeChild(a);
                    URL.revokeObjectURL(url);
                  }}
                  className="px-4 py-2.5 text-xs font-bold bg-emerald-700 hover:bg-emerald-800 text-white rounded-lg transition-colors flex items-center gap-1.5"
                >
                  Exportar Planilla Excel
                </button>
                <button
                  onClick={() => window.print()}
                  className="px-4 py-2.5 text-xs font-bold bg-slate-800 hover:bg-slate-700 text-white rounded-lg transition-colors flex items-center gap-1.5"
                >
                  Imprimir / PDF
                </button>
              </div>
            </div>

            {/* Panel Desplegable de Descripción del Producto */}
            {mostrarDescResultado && (
              <div className="p-4 bg-rose-50/50 border border-rose-200 rounded-lg text-sm text-slate-800">
                <span className="text-xs font-bold uppercase tracking-wider text-[#530618] block mb-1">
                  Descripción del Producto
                </span>
                {resultado.fichaProducto.descripcion}
              </div>
            )}

            {/* Tarjetas Comparativas de Costo (FOB vs Gravámenes vs Desembolso Final) */}
            <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
              <div className="p-5 bg-slate-50 rounded-xl border border-slate-200">
                <p className="text-xs text-slate-500 uppercase tracking-wider font-semibold">
                  1. Valor Compra FOB Total
                </p>
                <p className="text-2xl font-black text-slate-800 mt-1">
                  USD {resultado.parametrosComerciales.valorFobTotal.toLocaleString("es-AR", { minimumFractionDigits: 2 })}
                </p>
                <p className="text-xs text-slate-500 mt-1">
                  Flete: USD {resultado.parametrosComerciales.flete.toLocaleString("es-AR", { minimumFractionDigits: 2 })} | Seguro: USD {resultado.parametrosComerciales.seguro.toLocaleString("es-AR", { minimumFractionDigits: 2 })}
                </p>
                <p className="text-xs font-semibold text-slate-600 mt-2 pt-2 border-t border-slate-200">
                  Valor CIF Aduanero: USD {resultado.parametrosComerciales.valorCIF.toLocaleString("es-AR", { minimumFractionDigits: 2 })}
                </p>
              </div>

              <div className="p-5 bg-amber-50 rounded-xl border border-amber-200">
                <p className="text-xs text-amber-800 uppercase tracking-wider font-semibold">
                  2. Impuestos & Tasas Aduaneras
                </p>
                <p className="text-2xl font-black text-amber-700 mt-1">
                  + USD {resultado.resumenEconomico.totalGravamenesUSD.toLocaleString("es-AR", { minimumFractionDigits: 2 })}
                </p>
                <p className="text-xs text-amber-800 mt-1">
                  Incidencia fiscal sobre compra FOB: <strong>+{resultado.resumenEconomico.incidenciaFiscalPct}%</strong>
                </p>
              </div>

              <div className="p-5 bg-rose-50 rounded-xl border border-rose-200">
                <p className="text-xs text-[#530618] uppercase tracking-wider font-semibold">
                  3. Costo Final Nacionalizado
                </p>
                <p className="text-2xl font-black text-[#530618] mt-1">
                  USD {resultado.resumenEconomico.costoTotalNacionalizadoUSD.toLocaleString("es-AR", { minimumFractionDigits: 2 })}
                </p>
                <p className="text-xs font-bold text-[#530618] mt-1">
                  Costo Unitario Final: USD {resultado.resumenEconomico.costoUnitarioFinalUSD.toLocaleString("es-AR", { minimumFractionDigits: 2 })} / unidad
                </p>
                <p className="text-xs text-rose-800 mt-2 pt-2 border-t border-rose-200">
                  ≈ ARS ${resultado.resumenEconomico.costoTotalEstimadoARS.toLocaleString("es-AR", { minimumFractionDigits: 2 })} (TC Oficial BNA)
                </p>
              </div>
            </div>

            {/* Tabla Analítica de Desglose Impositivo Oficial */}
            <div className="border border-slate-200 rounded-lg overflow-hidden">
              <div className="bg-slate-100 px-4 py-2.5 font-bold text-xs uppercase tracking-wider text-slate-700">
                Liquidación Analítica Detallada de Gravámenes Oficiales
              </div>
              <div className="overflow-x-auto">
                <table className="w-full text-sm text-left border-collapse">
                  <thead>
                    <tr className="bg-slate-50 text-slate-600 border-b text-xs uppercase">
                      <th className="p-3">Concepto Tributario</th>
                      <th className="p-3">Alícuota Aplicada</th>
                      <th className="p-3 text-right">Importe en USD</th>
                    </tr>
                  </thead>
                  <tbody className="divide-y divide-slate-100">
                    {resultado.desgloseTributario.map((item, idx) => (
                      <tr
                        key={idx}
                        className={item.esSubtotal ? "bg-slate-50 font-bold text-slate-900" : "hover:bg-slate-50/50"}
                      >
                        <td className="p-3">
                          <span className="block font-medium">{item.concepto}</span>
                        </td>
                        <td className="p-3 font-mono text-xs">{item.alicuota}</td>
                        <td className="p-3 text-right font-mono font-semibold">
                          USD {item.montoUSD.toLocaleString("es-AR", { minimumFractionDigits: 2 })}
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            </div>

            {/* Botón de Puertos y Aduanas (PN3) */}
            <div className="pt-4 border-t flex flex-wrap justify-between items-center gap-3 print:hidden">
              <p className="text-xs text-slate-600">
                ¿Querés consultar los puntos aduaneros habilitados para este origen?
              </p>
              <button
                onClick={() => setMostrarPuertosYForwarders(!mostrarPuertosYForwarders)}
                className="bg-[#530618] hover:bg-[#3D0411] text-white px-6 py-3 rounded-lg text-sm font-bold transition shadow-sm flex items-center gap-2"
              >
                <span>{mostrarPuertosYForwarders ? "Ocultar Puertos y Aduanas" : "Ver Puertos y Aduanas"}</span>
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
                    PN3 · Logística Aduanera
                  </span>
                  <h3 className="text-xl font-bold text-slate-800">
                    Puertos y Aduanas Sugeridos para {resultado?.fichaProducto.paisOrigen || "este Origen"}
                  </h3>
                </div>
                <span className="text-xs bg-rose-50 text-[#530618] px-3 py-1 rounded-full font-semibold">
                  Puntos Habilitados DGA
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

        {/* Modal Informativo: Sección en Desarrollo PN4 */}
        {modalOperador && (
          <div className="fixed inset-0 z-50 flex items-center justify-center p-4 bg-slate-900/60 backdrop-blur-sm">
            <div className="bg-white w-full max-w-md rounded-2xl p-8 shadow-2xl border border-slate-200 relative text-center">
              <button
                onClick={() => setModalOperador(null)}
                className="absolute top-4 right-4 text-slate-400 hover:text-slate-700 text-xl font-bold"
              >
                &times;
              </button>

              <div className="w-16 h-16 rounded-full bg-rose-50 text-[#530618] flex items-center justify-center mx-auto mb-5 shadow-inner">
                <svg className="w-8 h-8" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth="1.8" d="M19 21V5a2 2 0 00-2-2H7a2 2 0 00-2 2v16m14 0h2m-2 0h-5m-9 0H3m2 0h5M9 7h1m-1 4h1m4-4h1m-1 4h1m-5 10v-5a1 1 0 011-1h2a1 1 0 011 1v5m-4 0h4" />
                </svg>
              </div>

              <span className="text-xs font-bold uppercase tracking-widest text-[#530618] bg-rose-50 border border-rose-200 px-3 py-1 rounded-full inline-block mb-3">
                Próximamente
              </span>

              <h3 className="text-2xl font-bold text-slate-900 mb-2">
                Sección en Desarrollo PN4
              </h3>

              <p className="text-xs text-slate-500 mb-6 leading-relaxed">
                El módulo de conexión directa y cotización con despachantes de aduana y forwarders se encuentra actualmente en proceso de integración.
              </p>

              <button
                type="button"
                onClick={() => setModalOperador(null)}
                className="w-full bg-[#530618] hover:bg-[#3D0411] text-white font-bold py-3.5 rounded-lg text-xs uppercase tracking-widest transition-colors shadow"
              >
                Cerrar
              </button>
            </div>
          </div>
        )}
      </main>
    </div>
  );
}
