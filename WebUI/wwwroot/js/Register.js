document.addEventListener("DOMContentLoaded", function () {
    const password1 = document.getElementById("regPassword");
    const toggle1 = document.getElementById("togglePassword1");
    const password2 = document.getElementById("confirmPassword");
    const toggle2 = document.getElementById("togglePassword2");

    toggle1.addEventListener("click", function () {
        const type = password1.getAttribute("type") === "password" ? "text" : "password";
        password1.setAttribute("type", type);
        this.classList.toggle("fa-eye-slash");
        this.classList.toggle("fa-eye");
    });

    toggle2.addEventListener("click", function () {
        const type = password2.getAttribute("type") === "password" ? "text" : "password";
        password2.setAttribute("type", type);
        this.classList.toggle("fa-eye-slash");
        this.classList.toggle("fa-eye");
    });
});
