/**
 * Navigation JS - LINTU LMS
 * Xử lý các tương tác cho thanh điều hướng và sidebar
 * Tác giả: Augment AI
 */

document.addEventListener('DOMContentLoaded', function () {
    // Xử lý hiển thị menu trên thiết bị di động
    const mobileMenu = document.querySelector('.mobile-menu');
    if (mobileMenu) {
        mobileMenu.addEventListener('click', function () {
            const navbar = document.querySelector('.pcoded-navbar');
            navbar.classList.toggle('mob-open');
        });
    }

    // Xử lý đóng menu khi click bên ngoài trên thiết bị di động
    document.addEventListener('click', function (e) {
        const navbar = document.querySelector('.pcoded-navbar');
        const mobileMenu = document.querySelector('.mobile-menu');

        if (navbar && navbar.classList.contains('mob-open') &&
            !navbar.contains(e.target) &&
            mobileMenu && !mobileMenu.contains(e.target)) {
            navbar.classList.remove('mob-open');
        }
    });

    // Thêm hiệu ứng ripple cho các mục menu
    const menuItems = document.querySelectorAll('.pcoded-inner-navbar .pcoded-item > li > a');

    menuItems.forEach(item => {
        item.addEventListener('click', function (e) {
            const rect = this.getBoundingClientRect();
            const x = e.clientX - rect.left;
            const y = e.clientY - rect.top;

            const ripple = document.createElement('span');
            ripple.className = 'ripple';
            ripple.style.left = x + 'px';
            ripple.style.top = y + 'px';

            this.appendChild(ripple);

            setTimeout(() => {
                ripple.remove();
            }, 600);
        });
    });

    // Đảm bảo menu active được hiển thị đúng
    const activeMenuItem = document.querySelector('.pcoded-inner-navbar .pcoded-item > li.active');
    if (activeMenuItem) {
        activeMenuItem.scrollIntoView({ block: 'center', behavior: 'smooth' });
    }

    // Thêm hiệu ứng hover cho các mục menu
    menuItems.forEach(item => {
        item.addEventListener('mouseenter', function () {
            this.classList.add('hover');
        });

        item.addEventListener('mouseleave', function () {
            this.classList.remove('hover');
        });
    });
});
