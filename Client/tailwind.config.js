/** @type {import('tailwindcss').Config} */
module.exports = {
  content: [
    "./index.html",
    "./wwwroot/index.html",
    "./**/*.razor",
    "./**/*.razor.cs",
    "./**/*.cshtml"
  ],
  theme: {
    extend: {},
  },
  plugins: [],
}
