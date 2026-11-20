# IMPORTA +
### Simulador Oficial de Costos Aduaneros y Tributos ARCA / DGA (República Argentina)

**IMPORTA+** es una solución integral de ingeniería de software para el cálculo preciso, oficial y parametrizado del costo total de nacionalización y despacho a plaza de mercaderías importadas a la República Argentina, basada en las regulaciones de la Dirección General de Aduanas (DGA / ARCA), Código Aduanero (Ley 22.415), Decreto 99/2019, Resoluciones Generales AFIP 2281 y 563/1999.

---

## 🏛️ Arquitectura del Sistema

El sistema implementa una arquitectura desacoplada en capas (**Layered Architecture**) y patrones de diseño GoF:

1. **BE (Business Entities):** Modelado de entidades del dominio aduanero (`CalculadoraArancelariaBE`, `PosicionArancelariaBE`, `PaisBE`, `PuertoBE`, `OperadorBE`, `SimulacionBE`).
2. **BLL (Business Logic Layer):**
   - **Patrón GoF Template Method:** `CalculadorAduaneroTemplate` coordina la secuencia de cálculo invariable (FOB -> Flete y Seguro -> CIF -> DIE -> Tasa de Estadística con escala de topes -> Base Imponible Interior -> IVA 21% / 10.5% -> Percepción IVA 20% / 10% -> Percepción Ganancias 6% / 11% -> Arancel SIM USD 28 + Digitalización USD 10).
   - Variantes del régimen: Intrazona (Mercosur 0% DIE / Exenta de Estadística), Extrazona General y Bienes con Excepción Arancelaria (ej: informática/telecomunicaciones).
3. **DAL (Data Access Layer - Persistencia en Excel):**
   - Persistencia **100% en Excel** sin dependencia obligatoria de motores de base de datos SQL.
   - Libro Maestro: `DAL/database/ImportaPlus_ExcelDB.xlsx`.
   - Tablas en hojas sincronizadas: `NCM_Catalogo`, `Paises` (196 países del mundo), `Puertos_Aduanas`, `Operadores_Logisticos`, `Parametros_Regulatorios`, `Simulaciones` y `Solicitudes_Contacto`.
   - Consulta en vivo de cotización oficial del Banco Nación (ARS/USD) vía API con respaldo automático (fallback) a la planilla de parámetros en caso de operar sin conexión.
4. **GUI (Graphical User Interface):**
   - **Patrón GoF State:** Máquina de estados desacoplada para navegación SPA fluida (`HeroState`, `FormularioState`, `ResultadosState`, `DesgloseState`, `PuertosState`, `ContactoState`, `PlanesState`).
   - Versión Dual: Funciona de forma autónoma abriendo `index.html` en cualquier navegador web o mediante el framework **Next.js (React)** con API REST serverless.

---

## 🚀 Puesta en Marcha

### Opción A: Modo Autónomo (SPA Liviana)
Abrir directamente el archivo `index.html` en cualquier navegador web moderno (Edge, Chrome, Firefox, Safari).

### Opción B: Modo Next.js (Full Stack)
```bash
npm install
npm run dev
```
La aplicación estará disponible en `http://localhost:3000`.

---

## 🧪 Pruebas Automatizadas
Para verificar la integridad matemática y aduanera de las liquidaciones y la persistencia en las hojas de cálculo Excel:
```bash
python test_calculo_pn1.py
```

---

## 📄 Licencia
Propiedad de IMPORTA+. Todos los derechos reservados.
