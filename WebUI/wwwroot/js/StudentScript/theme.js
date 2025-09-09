document.addEventListener("DOMContentLoaded", () => {
    const btn = document.getElementById("themeBtn");
    if (!btn) return;
    const saved = localStorage.getItem("site-theme");
    if (saved === "dark") document.documentElement.classList.add("dark");
    btn.addEventListener("click", () => {
        document.documentElement.classList.toggle("dark");
        localStorage.setItem("site-theme", document.documentElement.classList.contains("dark") ? "dark" : "light");
    });
});
