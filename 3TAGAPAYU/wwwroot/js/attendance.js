document.addEventListener('DOMContentLoaded', function () {
    const filterBtn = document.getElementById('filterBtn');
    const dateFilter = document.getElementById('dateFilter');
    const searchBox = document.getElementById('searchBox');
    const exportBtn = document.getElementById('exportBtn');
    const attendanceTable = document.querySelector('.attendance-table tbody');

    // Date filter
    if (filterBtn && dateFilter) {
        filterBtn.addEventListener('click', function () {
            const selectedDate = dateFilter.value;
            if (selectedDate) {
                const formattedDate = formatDate(new Date(selectedDate));
                console.log('Filter applied for date:', formattedDate);
                // In a real app, this would trigger a fetch to the server
                // window.location.href = `/Attendance?date=${selectedDate}`;
            }
        });
    }

    // Search functionality
    if (searchBox) {
        searchBox.addEventListener('input', function () {
            const searchTerm = this.value.toLowerCase();
            filterTable(searchTerm);
        });
    }

    // Export functionality
    if (exportBtn) {
        exportBtn.addEventListener('click', function () {
            exportTableToCSV('attendance_report.csv');
        });
    }

    function filterTable(searchTerm) {
        const rows = attendanceTable.querySelectorAll('tr');
        let visibleCount = 0;

        rows.forEach(row => {
            const employeeName = row.querySelector('td').textContent.toLowerCase();
            const isVisible = employeeName.includes(searchTerm);
            row.style.display = isVisible ? '' : 'none';
            if (isVisible) visibleCount++;
        });

        // Show no results message if needed
        if (visibleCount === 0) {
            const noResultsRow = document.querySelector('.no-results');
            if (!noResultsRow) {
                const tr = document.createElement('tr');
                tr.className = 'no-results';
                tr.innerHTML = '<td colspan="6" style="text-align: center; padding: 40px; color: #999;">No records found</td>';
                attendanceTable.appendChild(tr);
            }
        } else {
            const noResultsRow = document.querySelector('.no-results');
            if (noResultsRow) noResultsRow.remove();
        }
    }

    function exportTableToCSV(filename) {
        let csv = [];
        const headers = ['Employee Name', 'Department', 'Check-In', 'Check-Out', 'Working Hours', 'Status'];
        csv.push(headers.join(','));

        const rows = attendanceTable.querySelectorAll('tr');
        rows.forEach(row => {
            const cells = row.querySelectorAll('td');
            const rowData = [];
            cells.forEach(cell => {
                let text = cell.textContent.trim();
                text = text.replace(/"/g, '""');
                rowData.push(`"${text}"`);
            });
            csv.push(rowData.join(','));
        });

        downloadCSV(csv.join('\n'), filename);
    }

    function downloadCSV(csv, filename) {
        const csvFile = new Blob([csv], { type: 'text/csv;charset=utf-8;' });
        const link = document.createElement('a');
        const url = URL.createObjectURL(csvFile);
        link.setAttribute('href', url);
        link.setAttribute('download', filename);
        link.style.visibility = 'hidden';
        document.body.appendChild(link);
        link.click();
        document.body.removeChild(link);
    }

    function formatDate(date) {
        const year = date.getFullYear();
        const month = String(date.getMonth() + 1).padStart(2, '0');
        const day = String(date.getDate()).padStart(2, '0');
        return `${year}-${month}-${day}`;
    }

    // Initialize tooltips or other UI enhancements
    initializeUI();
});

function initializeUI() {
    // Add animation to stat cards
    const statCards = document.querySelectorAll('.stat-card');
    statCards.forEach((card, index) => {
        card.style.animationDelay = `${index * 0.1}s`;
    });
}