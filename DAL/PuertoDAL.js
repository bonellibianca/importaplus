/**
 * Capa DAL (Data Access Layer)
 * Repositorio / Acceso a Datos: PuertoDAL
 * 
 * Implementa el Patrón Repository para centralizar la persistencia y consulta
 * de puntos de ingreso aduanero (puertos, aduanas aeroportuarias, pasos fronterizos)
 * y ciudades de referencia argentinas, desacoplándolos de la capa visual GUI.
 */
class PuertoDAL {
  constructor() {
    this._puertos = [
      {
        id: "pba",
        name: "Puerto de Buenos Aires",
        type: "Marítimo principal",
        lat: -34.5985,
        lng: -58.3731,
        desc: "Principal puerto de importación del país. Maneja la mayor parte del comercio exterior argentino.",
        operador: "AGPSE (Administración General de Puertos)"
      },
      {
        id: "eze",
        name: "Aduana de Ezeiza",
        type: "Aéreo",
        lat: -34.8222,
        lng: -58.5358,
        desc: "Aduana del Aeropuerto Internacional Ministro Pistarini. Principal ingreso aéreo de mercadería.",
        operador: "ARCA-Aduana / AA2000"
      },
      {
        id: "ros",
        name: "Puerto de Rosario",
        type: "Fluvial",
        lat: -32.9468,
        lng: -60.6393,
        desc: "Importante terminal fluvial sobre el Río Paraná. Relevante para granos y productos industriales.",
        operador: "Ente Administrador Puerto Rosario"
      },
      {
        id: "mdp",
        name: "Puerto de Mar del Plata",
        type: "Marítimo",
        lat: -38.0055,
        lng: -57.5426,
        desc: "Puerto atlántico al sur de la provincia de Buenos Aires. Pesca e importaciones regionales.",
        operador: "Consorcio de Gestión del Puerto"
      },
      {
        id: "cr",
        name: "Paso Cristo Redentor",
        type: "Terrestre",
        lat: -32.8353,
        lng: -70.0649,
        desc: "Principal paso fronterizo con Chile en la Cordillera de los Andes. Alto volumen de camiones.",
        operador: "Aduana Argentina / DNV"
      }
    ];

    this._ciudades = [
      { name: "Buenos Aires", lat: -34.6037, lng: -58.3816 },
      { name: "Córdoba", lat: -31.4135, lng: -64.1811 },
      { name: "Rosario", lat: -32.9468, lng: -60.6393 },
      { name: "Mendoza", lat: -32.8908, lng: -68.8272 },
      { name: "Tucumán", lat: -26.8083, lng: -65.2176 },
      { name: "La Plata", lat: -34.9215, lng: -57.9545 },
      { name: "Mar del Plata", lat: -38.0023, lng: -57.5575 },
      { name: "Salta", lat: -24.7821, lng: -65.4232 },
      { name: "Santa Fe", lat: -31.6333, lng: -60.7 },
      { name: "San Juan", lat: -31.5375, lng: -68.5364 }
    ];
  }

  /**
   * Obtiene todos los puntos aduaneros mapeados a entidades PuertoBE.
   * @returns {Promise<PuertoBE[]>}
   */
  async obtenerTodos() {
    const PuertoClass = typeof window !== 'undefined' ? window.PuertoBE : null;
    return this._puertos.map(p => PuertoClass ? new PuertoClass(p) : p);
  }

  /**
   * Obtiene puertos filtrados por tipo de vía (Marítimo, Aéreo, Fluvial, Terrestre).
   * @param {string} tipo 
   * @returns {Promise<PuertoBE[]>}
   */
  async obtenerPorTipo(tipo) {
    const todos = await this.obtenerTodos();
    if (!tipo) return todos;
    return todos.filter(p => p.tipo.toLowerCase().includes(tipo.toLowerCase()));
  }

  /**
   * Obtiene la lista de coordenadas de ciudades de referencia para el cálculo de distancia.
   * @returns {Promise<Array<{name: string, lat: number, lng: number}>>}
   */
  async obtenerCiudadesReferencia() {
    return [...this._ciudades];
  }
}

if (typeof window !== 'undefined') {
  window.PuertoDAL = PuertoDAL;
}
