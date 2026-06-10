import type { Config } from 'tailwindcss'

export default {
  darkMode: ['class'],
  content: ['./src/**/*.{ts,tsx}'],
  theme: {
    extend: {
      colors: {
        'landing-bg':         '#0D1117',
        'landing-surface':    '#161B22',
        'landing-border':     '#21262D',
        'landing-gold':       '#C9A96E',
        'landing-gold-hover': '#D4B483',
        'landing-text':       '#E6EDF3',
        'landing-muted':      '#8B949E',
        primary: { DEFAULT: '#0f172a' },
        accent:  { DEFAULT: '#f59e0b' },
      },
      fontFamily: {
        sans:    ['Inter', 'sans-serif'],
        display: ['Inter', 'sans-serif'],
      },
      animation: {
        'fade-in-up': 'fadeInUp 0.6s ease-out forwards',
        'fade-in':    'fadeIn 0.4s ease-out forwards',
      },
      keyframes: {
        fadeInUp: {
          '0%':   { opacity: '0', transform: 'translateY(20px)' },
          '100%': { opacity: '1', transform: 'translateY(0)' },
        },
        fadeIn: {
          '0%':   { opacity: '0' },
          '100%': { opacity: '1' },
        },
      },
    },
  },
  plugins: [require('tailwindcss-animate')],
} satisfies Config
