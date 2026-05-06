// skeleton.js - Handle skeleton loading states

document.addEventListener('DOMContentLoaded', function () {
    // Hide skeleton after data loads
    setTimeout(function () {
        var skeletonRows = document.querySelectorAll('.skeleton-row');
        if (skeletonRows.length > 0) {
            var realRows = document.querySelectorAll('#studentsBody tr.table-row:not(.skeleton-row), #teachersBody tr.table-row:not(.skeleton-row)');
            if (realRows.length > 0) {
                skeletonRows.forEach(function (row) { if (row && row.remove) row.remove(); });
            }
        }

        var skeletonCards = document.getElementById('skeletonCards');
        if (skeletonCards) {
            var realCards = document.querySelectorAll('#coursesGrid .course-card');
            if (realCards.length > 0) {
                skeletonCards.style.display = 'none';
            }
        }
    }, 500);
});