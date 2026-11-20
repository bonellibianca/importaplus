import './globals.css';

export const metadata = {
  title: 'IMPORTA +',
  description: 'Calculá el costo total nacionalizado de tu mercadería con aranceles NCM, tributos aduaneros actualizados (Decreto 99/2019, RG 2281), puertos habilitados y forwarders matriculados.',
  keywords: 'calculadora de importacion, costos de importacion argentina, simulador aduanero, aranceles ncm, derechos de importacion die, despachante de aduana, tasa de estadistica aduana, flete internacional argentina, arca dga',
  authors: [{ name: 'Bianca Bonelli | IMPORTA+' }],
  icons: {
    icon: '/favicon.ico',
    apple: '/apple-touch-icon.png'
  },
  openGraph: {
    title: 'IMPORTA+ | Simulador oficial de costos aduaneros y tributos ARCA / DGA',
    description: 'Simulador oficial de costos aduaneros y tributos ARCA / DGA para la República Argentina.',
    url: 'https://importaplus.com.ar',
    siteName: 'IMPORTA+',
    locale: 'es_AR',
    type: 'website'
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
      </head>
      <body className="min-h-screen bg-slate-50 text-slate-900 antialiased selection:bg-[#530618] selection:text-white">
        {children}
      </body>
    </html>
  );
}
