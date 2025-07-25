document.addEventListener('DOMContentLoaded', function () {
    const sidebar = document.getElementById('sidebar');
    const toggle = document.getElementById('sidebarToggle');
    function isMobile() {
        return window.innerWidth <= 600;
    }
    toggle.addEventListener('click', function () {
        if (isMobile()) {
            sidebar.classList.toggle('open');
        } else {
            sidebar.classList.toggle('collapsed');
        }
    });
    // Fecha o sidebar ao clicar fora em mobile
    document.addEventListener('click', function (e) {
        if (isMobile() && sidebar.classList.contains('open')) {
            if (!sidebar.contains(e.target) && e.target !== toggle) {
                sidebar.classList.remove('open');
            }
        }
    });
    // Remove classes ao redimensionar
    window.addEventListener('resize', function () {
        if (!isMobile()) {
            sidebar.classList.remove('open');
        }
    });
}); 