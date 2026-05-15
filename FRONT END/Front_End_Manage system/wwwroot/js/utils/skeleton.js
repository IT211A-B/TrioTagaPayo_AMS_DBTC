// skeleton.js - Handle skeleton loading states

document.addEventListener('DOMContentLoaded', function () {
    showSkeletons();
    setTimeout(hideSkeletons, 800);
});

function showSkeletons() {
    // Add skeleton class to containers
    const containers = [
        { selector: '.courses-grid', skeletonClass: 'skeleton-courses' },
        { selector: '.stats-row', skeletonClass: 'skeleton-stats' },
        { selector: '.table-wrapper', skeletonClass: 'skeleton-table' },
        { selector: '.attendance-list', skeletonClass: 'skeleton-list' }
    ];

    containers.forEach(container => {
        const element = document.querySelector(container.selector);
        if (element && element.children.length === 0) {
            element.classList.add(container.skeletonClass);
        }
    });
}

function hideSkeletons() {
    const skeletonClasses = ['skeleton-courses', 'skeleton-stats', 'skeleton-table', 'skeleton-list'];
    skeletonClasses.forEach(cls => {
        const elements = document.querySelectorAll('.' + cls);
        elements.forEach(el => el.classList.remove(cls));
    });
}