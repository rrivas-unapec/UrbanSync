(function () {
  var STORAGE_KEYS = {
    sidebarCollapsed: "adminPanel.sidebarCollapsed",
    highContrast: "adminPanel.highContrast",
    largeText: "adminPanel.largeText"
  };

  function readFlag(key) {
    return localStorage.getItem(key) === "true";
  }

  function applyState() {
    var sidebar = document.getElementById("adminSidebar");
    if (sidebar) {
      sidebar.classList.toggle("is-collapsed", readFlag(STORAGE_KEYS.sidebarCollapsed));
    }

    document.documentElement.classList.toggle("admin-high-contrast", readFlag(STORAGE_KEYS.highContrast));
    document.documentElement.classList.toggle("admin-large-text", readFlag(STORAGE_KEYS.largeText));

    var contrastBtn = document.getElementById("adminContrastToggle");
    if (contrastBtn) {
      contrastBtn.classList.toggle("active", readFlag(STORAGE_KEYS.highContrast));
      contrastBtn.setAttribute("aria-pressed", readFlag(STORAGE_KEYS.highContrast));
    }

    var textBtn = document.getElementById("adminTextSizeToggle");
    if (textBtn) {
      textBtn.classList.toggle("active", readFlag(STORAGE_KEYS.largeText));
      textBtn.setAttribute("aria-pressed", readFlag(STORAGE_KEYS.largeText));
    }
  }

  function toggleFlag(key) {
    localStorage.setItem(key, (!readFlag(key)).toString());
    applyState();
  }

  document.addEventListener("DOMContentLoaded", function () {
    applyState();

    var sidebarToggle = document.getElementById("adminSidebarToggle");
    if (sidebarToggle) {
      sidebarToggle.addEventListener("click", function () {
        toggleFlag(STORAGE_KEYS.sidebarCollapsed);
      });
    }

    var contrastBtn = document.getElementById("adminContrastToggle");
    if (contrastBtn) {
      contrastBtn.addEventListener("click", function () {
        toggleFlag(STORAGE_KEYS.highContrast);
      });
    }

    var textBtn = document.getElementById("adminTextSizeToggle");
    if (textBtn) {
      textBtn.addEventListener("click", function () {
        toggleFlag(STORAGE_KEYS.largeText);
      });
    }
  });
})();
