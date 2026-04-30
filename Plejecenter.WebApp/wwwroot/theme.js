// Simple theme helper for CSS variables in app.css:
// - Default is "dark" (no [data-theme] override)
// - Light mode uses: [data-theme="light"] { ... }

window.setTheme = function (theme) {
  try {
    if (theme === "light") {
      document.documentElement.setAttribute("data-theme", "light");
    } else {
      document.documentElement.removeAttribute("data-theme");
      theme = "dark";
    }
    localStorage.setItem("theme", theme);
  } catch {
    // ignore (private mode / storage blocked)
  }
};

window.getTheme = function () {
  try {
    return localStorage.getItem("theme") || "dark";
  } catch {
    return "dark";
  }
};

// Apply stored theme as early as possible
(function () {
  window.setTheme(window.getTheme());
})();

