// skeleton.js - Handle skeleton loading states

function showSkeleton(elementId, skeletonHtml) {
    var element = document.getElementById(elementId);
    if (!element) return;

    // Store original content if not already stored
    if (!element.getAttribute('data-original-content')) {
        element.setAttribute('data-original-content', element.innerHTML);
    }

    // Store skeleton HTML if provided
    if (skeletonHtml) {
        element.setAttribute('data-skeleton-html', skeletonHtml);
    }

    var skeletonContent = element.getAttribute('data-skeleton-html');
    if (skeletonContent) {
        element.innerHTML = skeletonContent;
    }
    element.classList.add('loading');
}

function hideSkeleton(elementId) {
    var element = document.getElementById(elementId);
    if (!element) return;

    var originalContent = element.getAttribute('data-original-content');
    if (originalContent) {
        element.innerHTML = originalContent;
    }
    element.classList.remove('loading');
}

// Auto-hide skeleton rows when data loads
document.addEventListener('DOMContentLoaded', function () {
    // Hide skeleton after data loads (small delay to allow rendering)
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