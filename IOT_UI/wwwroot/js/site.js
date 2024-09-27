document.querySelector('.toggle-btn').addEventListener('click', function () {
    const sidebar = document.getElementById('sidebar');
    sidebar.classList.toggle('expand');
    const mainContent = document.querySelector('.main');
    if (sidebar.classList.contains('expand')) {
        mainContent.style.marginLeft = '260px'; // Adjust margin for expanded sidebar
    } else {
        mainContent.style.marginLeft = '70px'; // Reset margin for collapsed sidebar
    }
});
