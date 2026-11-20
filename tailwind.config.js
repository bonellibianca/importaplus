/** @type {import('tailwindcss').Config} */
module.exports = {
  content: [
    "./src/pages/**/*.{js,ts,jsx,tsx,mdx}",
    "./src/components/**/*.{js,ts,jsx,tsx,mdx}",
    "./src/app/**/*.{js,ts,jsx,tsx,mdx}",
    "./GUI/**/*.{js,ts,jsx,tsx,html}",
    "./index.html"
  ],
  theme: {
    extend: {
      colors: {
        brand: {
          DEFAULT: '#530618',
          hover: '#3D0411',
          dark: '#3D0411',
          gradient: '#9B3040',
          tint: '#FFF0F2',
          bgLight: '#FFF8F8',
          borderLight: '#F0D0D4'
        }
      },
      fontFamily: {
        sans: ['Inter', 'system-ui', 'sans-serif'],
        mono: ['ui-monospace', 'SFMono-Regular', 'Menlo', 'Monaco', 'Consolas', 'monospace']
      }
    },
  },
  plugins: [],
};
