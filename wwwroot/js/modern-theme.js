/**
 * Modern Theme JS - LINTU LMS
 * Thêm các hiệu ứng và tính năng tương tác cho giao diện hiện đại
 */

document.addEventListener('DOMContentLoaded', function() {
    // Thêm class animation cho các card khi trang tải xong
    const cards = document.querySelectorAll('.card');
    cards.forEach((card, index) => {
        setTimeout(() => {
            card.classList.add('animate-fade-in');
        }, index * 100);
    });

    // Thêm hiệu ứng ripple cho các button
    const buttons = document.querySelectorAll('.btn');
    buttons.forEach(button => {
        button.addEventListener('click', function(e) {
            const x = e.clientX - e.target.getBoundingClientRect().left;
            const y = e.clientY - e.target.getBoundingClientRect().top;
            
            const ripple = document.createElement('span');
            ripple.classList.add('ripple-effect');
            ripple.style.left = `${x}px`;
            ripple.style.top = `${y}px`;
            
            this.appendChild(ripple);
            
            setTimeout(() => {
                ripple.remove();
            }, 600);
        });
    });

    // Thêm hiệu ứng hover cho các menu item trong sidebar
    const menuItems = document.querySelectorAll('.pcoded-item li a');
    menuItems.forEach(item => {
        item.addEventListener('mouseenter', function() {
            const icon = this.querySelector('.pcoded-micon i');
            if (icon) {
                icon.classList.add('animate-pulse');
            }
        });
        
        item.addEventListener('mouseleave', function() {
            const icon = this.querySelector('.pcoded-micon i');
            if (icon) {
                icon.classList.remove('animate-pulse');
            }
        });
    });

    // Thêm hiệu ứng scroll cho các phần tử
    const animateOnScroll = function() {
        const elements = document.querySelectorAll('.scroll-animate');
        
        elements.forEach(element => {
            const elementPosition = element.getBoundingClientRect().top;
            const windowHeight = window.innerHeight;
            
            if (elementPosition < windowHeight - 50) {
                element.classList.add('animate-slide-in-up');
                element.classList.remove('scroll-animate');
            }
        });
    };
    
    window.addEventListener('scroll', animateOnScroll);
    animateOnScroll(); // Chạy một lần khi trang tải xong

    // Thêm hiệu ứng cho dropdown
    const dropdowns = document.querySelectorAll('.dropdown-toggle');
    dropdowns.forEach(dropdown => {
        dropdown.addEventListener('click', function() {
            const dropdownMenu = this.nextElementSibling;
            if (dropdownMenu && dropdownMenu.classList.contains('dropdown-menu')) {
                dropdownMenu.classList.add('animate-fade-in');
            }
        });
    });

    // Thêm hiệu ứng cho thông báo
    const notifications = document.querySelectorAll('#notificationList li:not(:first-child)');
    notifications.forEach((notification, index) => {
        setTimeout(() => {
            notification.classList.add('animate-slide-in-up');
        }, index * 100);
    });

    // Thêm hiệu ứng cho các card trên trang chủ
    const statsCards = document.querySelectorAll('.stats-card');
    statsCards.forEach((card, index) => {
        setTimeout(() => {
            card.classList.add('animate-slide-in-up');
        }, index * 150);
    });

    // Thêm hiệu ứng cho các course card
    const courseCards = document.querySelectorAll('.course-card');
    courseCards.forEach((card, index) => {
        setTimeout(() => {
            card.classList.add('animate-fade-in');
        }, index * 100);
    });
});
