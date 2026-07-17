/** @type {import('tailwindcss').Config} */
module.exports = {
  content: [
    "./Views/**/*.cshtml",
    "./test.html"
  ],
  theme: {
    extend: {
      colors: {
        'wnz-yellow': '#f1a90a',
        'wnz-secondary': '#1e1e1e',
        'swiss-light': '#FAF9F9',
        'swiss-border': '#D1D1D1',
        'swiss-red': '#bb0015',
        'swiss-accent-red': '#E62429',
        'primary': '#000000',
        'primary-foreground': '#FFFFFF',
        'danger': '#bb0015',
      },
      spacing: {
        '260': '260px',
      },
      width: {
        '260': '260px',
      }
    },
  },
  plugins: [
    require('flowbite/plugin')
  ],
}
