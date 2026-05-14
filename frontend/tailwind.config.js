/** @type {import('tailwindcss').Config} */
export default {
  content: ['./index.html', './src/**/*.{js,ts,jsx,tsx}'],
  theme: {
    extend: {
      fontFamily: {
        sans: ['DM Sans', 'system-ui', 'sans-serif'],
        display: ['Outfit', 'system-ui', 'sans-serif'],
      },
      colors: {
        surface: {
          DEFAULT: '#f8f7f4',
          card: '#ffffff',
          muted: '#eef0eb',
        },
        ink: {
          DEFAULT: '#1a1f16',
          muted: '#5c6354',
          faint: '#8a9180',
        },
        leaf: {
          DEFAULT: '#2d6a4f',
          hover: '#245a43',
          soft: '#d8f0e4',
        },
        accent: {
          DEFAULT: '#bc6c25',
          soft: '#f4e6d7',
        },
      },
      boxShadow: {
        card: '0 1px 2px rgba(26, 31, 22, 0.06), 0 4px 24px rgba(26, 31, 22, 0.06)',
      },
    },
  },
  plugins: [],
}
