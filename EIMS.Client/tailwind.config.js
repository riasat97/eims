/** @type {import('tailwindcss').Config} */
module.exports = {
  content: [
    "./Pages/**/*.{razor,html,cshtml}",
    "./Shared/**/*.{razor,html,cshtml}",
    "./wwwroot/index.html"
  ],
  theme: {
    extend: {},
  },
  plugins: [require("daisyui")],
  daisyui: {
    themes: ["light", "dark"],
  },
} 