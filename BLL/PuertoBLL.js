/**
 * Capa BLL (Business Logic Layer)
 * Lógica de Negocio: PuertoBLL
 * 
 * Gestiona las operaciones de negocio sobre puertos y aduanas argentinas,
 * incluyendo cálculo geoespacial (fórmula de Haversine) y filtrado de terminales.
 */
class PuertoBLL {
  constructor(puertoDAL = null) {
    const DALClass = typeof window !== 'undefined' ? window.PuertoDAL : null;
    this.puertoDAL = puertoDAL || (DALClass ? new DALClass() : null);
  }

  /**
   * Obtiene la lista oficial de puertos y aduanas.
   * @returns {Promise<PuertoBE[]>}
   */
  async obtenerPuertos() {
    if (!this.puertoDAL) return [];
    return await this.puertoDAL.obtenerTodos();
  }

  /**
   * Obtiene las ciudades argentinas de referencia.
   * @returns {Promise<Array<{name: string, lat: number, lng: number}>>}
   */
  async obtenerCiudades() {
    if (!this.puertoDAL) return [];
    return await this.puertoDAL.obtenerCiudadesReferencia();
  }

  /**
   * Calcula la distancia ortodrómica en kilómetros entre dos coordenadas geográficas
   * utilizando la fórmula de Haversine (Regla de negocio logística).
   * @param {number} lat1 
   * @param {number} lon1 
   * @param {number} lat2 
   * @param {number} lon2 
   * @returns {number} Distancia en kilómetros
   */
  calcularDistanciaKm(lat1, lon1, lat2, lon2) {
    const dLat = (lat2 - lat1) * Math.PI / 180;
    const dLon = (lon2 - lon1) * Math.PI / 180;
    const a = Math.sin(dLat / 2) ** 2 + Math.cos(lat1 * Math.PI / 180) * Math.cos(lat2 * Math.PI / 180) * Math.sin(dLon / 2) ** 2;
    return Math.round(6371 * 2 * Math.atan2(Math.sqrt(a), Math.sqrt(1 - a)));
  }
}

if (typeof window !== 'undefined') {
  window.PuertoBLL = PuertoBLL;
}
