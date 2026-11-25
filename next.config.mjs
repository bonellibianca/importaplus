/** @type {import('next').NextConfig} */
const nextConfig = {
  reactStrictMode: true,
  poweredByHeader: false,
  images: {
    remotePatterns: [
      { protocol: 'https', hostname: '**.supabase.co' },
      { protocol: 'https', hostname: 'images.unsplash.com' }
    ]
  },
  headers: async () => [
    {
      source: '/api/:path*',
      headers: [
        // Corrección Bug CORS: no combinar Access-Control-Allow-Credentials:true con wildcard '*'
        { key: 'Access-Control-Allow-Origin', value: process.env.NEXT_PUBLIC_APP_ORIGIN || 'https://importaplus.com.ar' },
        { key: 'Access-Control-Allow-Methods', value: 'GET,POST,OPTIONS' },
        { key: 'Access-Control-Allow-Headers', value: 'X-CSRF-Token, X-Requested-With, Accept, Accept-Version, Content-Length, Content-MD5, Content-Type, Date, X-Api-Version' },
        { key: 'Vary', value: 'Origin' }
      ],
    },
  ],
};

export default nextConfig;
