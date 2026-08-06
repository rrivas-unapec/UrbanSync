(function () {
  "use strict";

  document.addEventListener("DOMContentLoaded", function () {
    var toggles = document.querySelectorAll(
      ".login-password-toggle"
    );

    toggles.forEach(function (toggle) {
      var field = toggle.closest(
        ".login-password-field"
      );

      var input = field
        ? field.querySelector("input")
        : null;

      if (!input) {
        return;
      }

      toggle.addEventListener("click", function () {
        var shouldShowPassword =
          input.type === "password";

        input.type = shouldShowPassword
          ? "text"
          : "password";

        var icon = toggle.querySelector("i");

        if (icon) {
          icon.className = shouldShowPassword
            ? "bi bi-eye-slash"
            : "bi bi-eye";
        }

        toggle.setAttribute(
          "aria-label",
          shouldShowPassword
            ? "Ocultar contraseña"
            : "Mostrar contraseña"
        );

        toggle.setAttribute(
          "aria-pressed",
          shouldShowPassword
            ? "true"
            : "false"
        );
      });
    });
  });
})();