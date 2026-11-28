import './globals.css';

const defaultOrigin = process.env.NEXT_PUBLIC_APP_ORIGIN
  || (process.env.VERCEL_PROJECT_PRODUCTION_URL ? `https://${process.env.VERCEL_PROJECT_PRODUCTION_URL}` : 'https://importaplus.com.ar');

export const metadata = {
  metadataBase: new URL(defaultOrigin),
  title: 'IMPORTA +',
  description: 'Calculá el costo total nacionalizado de tu mercadería con aranceles NCM, tributos aduaneros actualizados (Decreto 99/2019, RG 2281), puertos habilitados y forwarders matriculados.',
  keywords: 'calculadora de importacion, costos de importacion argentina, simulador aduanero, aranceles ncm, derechos de importacion die, despachante de aduana, tasa de estadistica aduana, flete internacional argentina, arca dga',
  authors: [{ name: 'Bianca Bonelli | IMPORTA+' }],
  icons: {
    icon: [
      { url: '/favicon.ico' },
      { url: '/favicon-32x32.png', sizes: '32x32', type: 'image/png' },
      { url: '/favicon-192x192.png', sizes: '192x192', type: 'image/png' }
    ],
    apple: '/apple-touch-icon.png'
  },
  openGraph: {
    title: 'IMPORTA+ | Simulador oficial de costos aduaneros y tributos ARCA / DGA',
    description: 'Simulador oficial de costos aduaneros y tributos ARCA / DGA para la República Argentina.',
    url: 'https://importaplus.com.ar',
    siteName: 'IMPORTA+',
    locale: 'es_AR',
    type: 'website',
    images: [
      {
        url: '/FONDO%20PANTALLA.jpg',
        width: 1600,
        height: 524,
        alt: 'IMPORTA+ - Simulador de Costos de Importación'
      }
    ]
  },
  twitter: {
    card: 'summary_large_image',
    title: 'IMPORTA+ | Simulador Oficial de Importación Argentina',
    description: 'Simulador oficial de costos aduaneros y tributos ARCA / DGA.',
    images: ['/FONDO%20PANTALLA.jpg']
  }
};

export default function RootLayout({ children }) {
  return (
    <html lang="es">
      <head>
        <link rel="preconnect" href="https://fonts.googleapis.com" />
        <link rel="preconnect" href="https://fonts.gstatic.com" crossOrigin="anonymous" />
        <link
          href="https://fonts.googleapis.com/css2?family=Inter:wght@300;400;500;600;700;800;900&display=swap"
          rel="stylesheet"
        />
        <link
          rel="stylesheet"
          href="https://unpkg.com/leaflet@1.9.4/dist/leaflet.css"
        />
        <script
          src="https://unpkg.com/leaflet@1.9.4/dist/leaflet.js"
          defer
        />
      </head>
      <body className="min-h-screen bg-[#F4F6F8] text-[#171717] antialiased selection:bg-[#530618] selection:text-white">
        {children}
      </body>
    </html>
  );
}
