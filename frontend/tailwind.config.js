/** @type {import('tailwindcss').Config} */
module.exports = {
  content: [
    "./src/**/*.{js,jsx,ts,tsx}",
  ],
  theme: {
    extend: {
      fontFamily: {
        'pixel': ['visitor1', 'monospace'],
      },
      animation: {
        'float': 'float 6s ease-in-out infinite',
        'fly': 'fly 8s linear infinite',
        'rotate': 'rotate 20s linear infinite',
        'pulse-glow': 'pulse-glow 2s ease-in-out infinite alternate',
        'exhaust': 'exhaust 0.5s ease-in-out infinite alternate',
      },
      keyframes: {
        float: {
          '0%, 100%': { transform: 'translateY(0px)' },
          '50%': { transform: 'translateY(-20px)' },
        },
        fly: {
          '0%': { transform: 'translateX(100vw) translateY(0px)' },
          '50%': { transform: 'translateX(50vw) translateY(-10px)' },
          '100%': { transform: 'translateX(-100px) translateY(0px)' },
        },
        rotate: {
          '0%': { transform: 'rotate(0deg)' },
          '100%': { transform: 'rotate(360deg)' },
        },
        'pulse-glow': {
          '0%': { boxShadow: '0 0 5px rgba(59, 130, 246, 0.5)' },
          '100%': { boxShadow: '0 0 20px rgba(59, 130, 246, 0.8)' },
        },
        exhaust: {
          '0%': { opacity: '0.6', transform: 'scaleX(1)' },
          '100%': { opacity: '1', transform: 'scaleX(1.2)' },
        },
      },
    },
  },
  plugins: [],
}