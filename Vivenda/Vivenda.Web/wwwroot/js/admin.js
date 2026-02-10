// Admin Dashboard JavaScript

// Toggle sidebar on mobile
function toggleSidebar() {
    const sidebar = document.querySelector('.admin-sidebar');
    sidebar.classList.toggle('show');
}

// Bulk actions
function selectAll(checkbox) {
    const checkboxes = document.querySelectorAll('.item-checkbox');
    checkboxes.forEach(cb => cb.checked = checkbox.checked);
}

function getSelectedIds() {
    const checkboxes = document.querySelectorAll('.item-checkbox:checked');
    return Array.from(checkboxes).map(cb => cb.value);
}

function bulkDelete() {
    const ids = getSelectedIds();
    if (ids.length === 0) {
        alert('Please select at least one item.');
        return;
    }
    
    if (confirm(`Are you sure you want to delete ${ids.length} items?`)) {
        document.getElementById('bulkDeleteIds').value = ids.join(',');
        document.getElementById('bulkDeleteForm').submit();
    }
}

function bulkMarkAsRead() {
    const ids = getSelectedIds();
    if (ids.length === 0) {
        alert('Please select at least one item.');
        return;
    }
    
    document.getElementById('bulkMarkAsReadIds').value = ids.join(',');
    document.getElementById('bulkMarkAsReadForm').submit();
}

// Modal management
function showModal(modalId) {
    document.getElementById(modalId).classList.add('show');
}

function closeModal(modalId) {
    document.getElementById(modalId).classList.remove('show');
}

// Close modal when clicking outside
window.addEventListener('click', function(event) {
    if (event.target.classList.contains('modal')) {
        event.target.classList.remove('show');
    }
});

// Toggle status
function toggleStatus(id, url) {
    fetch(url, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
            'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]').value
        }
    })
    .then(response => response.json())
    .then(data => {
        if (data.success) {
            location.reload();
        } else {
            alert('Error updating status');
        }
    })
    .catch(error => {
        console.error('Error:', error);
        alert('Error updating status');
    });
}

// Search functionality
const searchInput = document.getElementById('tableSearch');
if (searchInput) {
    searchInput.addEventListener('keyup', debounce(function() {
        const filter = this.value.toLowerCase();
        const rows = document.querySelectorAll('.data-table tbody tr');
        
        rows.forEach(row => {
            const text = row.textContent.toLowerCase();
            row.style.display = text.includes(filter) ? '' : 'none';
        });
    }, 300));
}

// Initialize tooltips if using a tooltip library
document.addEventListener('DOMContentLoaded', function() {
    // Add any initialization code here
    
    // Highlight current menu item
    const currentPath = window.location.pathname;
    document.querySelectorAll('.sidebar-menu-link').forEach(link => {
        if (link.getAttribute('href') === currentPath) {
            link.classList.add('active');
        }
    });
});

// Chart colors
const chartColors = {
    blue: '#3b82f6',
    green: '#10b981',
    yellow: '#f59e0b',
    red: '#ef4444',
    purple: '#8b5cf6',
    pink: '#ec4899',
    indigo: '#6366f1',
    teal: '#14b8a6'
};

// Export table to CSV
function exportTableToCSV(filename) {
    const table = document.querySelector('.data-table');
    let csv = [];
    
    // Headers
    const headers = [];
    table.querySelectorAll('thead th').forEach(th => {
        headers.push(th.textContent.trim());
    });
    csv.push(headers.join(','));
    
    // Rows
    table.querySelectorAll('tbody tr').forEach(tr => {
        const row = [];
        tr.querySelectorAll('td').forEach(td => {
            row.push('"' + td.textContent.trim() + '"');
        });
        csv.push(row.join(','));
    });
    
    // Download
    const csvFile = new Blob([csv.join('\n')], { type: 'text/csv' });
    const downloadLink = document.createElement('a');
    downloadLink.download = filename || 'export.csv';
    downloadLink.href = window.URL.createObjectURL(csvFile);
    downloadLink.style.display = 'none';
    document.body.appendChild(downloadLink);
    downloadLink.click();
    document.body.removeChild(downloadLink);
}

// Print table
function printTable() {
    window.print();
}
