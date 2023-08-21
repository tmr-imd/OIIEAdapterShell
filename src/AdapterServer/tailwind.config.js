/** @type {import('tailwindcss').Config} */
module.exports = {
  content: ['../**/*.{razor,html}'],
  theme: {
    extend: {
      animation: {
        'animate-ping': 'ping 1s cubic-bezier(0, 0, 0.2, 1) 2'
      },
      keyframes: {
        'ping': {
          '0%': { opacity: 100 },
          '75%, 100%': {
            transform: 'scale(2) translate(-25%, 0%)',
            opacity: 0
          }
        }
      }
    },
  },
//  plugins: [
//    require('@tailwindcss/forms'),
//  ],
}
