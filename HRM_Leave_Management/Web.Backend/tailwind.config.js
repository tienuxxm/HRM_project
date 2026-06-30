/** @type {import('tailwindcss').Config} */
module.exports = {
  content: [
    "./Views/**/*.cshtml",
    "./wwwroot/js/**/*.js",
    "./node_modules/flowbite/**/*.js"
  ],
  theme: {
    extend: {
      colors: {
        'wnz-yellow': '#f1a90a',
        'wnz-secondary': '#1e1e1e',
      },
    },
  },
  plugins: [
    require('flowbite/plugin')
  ],
}
