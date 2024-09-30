document.querySelector('.toggle-btn').addEventListener('click', function () {
    const sidebar = document.getElementById('sidebar');
    sidebar.classList.toggle('expand');
    const mainContent = document.querySelector('.main');
});
