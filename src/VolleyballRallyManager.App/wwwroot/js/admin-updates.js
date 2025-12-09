// Admin Updates Page - Search and Filter Functionality
(function () {
    'use strict';

    document.addEventListener('DOMContentLoaded', function () {
        initializeFilters();
        initializeSearch();
    });

    /**
     * Initialize filter buttons for update types
     */
    function initializeFilters() {
        const filterButtons = document.querySelectorAll('[data-filter]');
        
        filterButtons.forEach(button => {
            button.addEventListener('click', function () {
                // Update active button state
                filterButtons.forEach(btn => btn.classList.remove('active'));
                this.classList.add('active');

                const filterValue = this.getAttribute('data-filter');
                filterUpdates(filterValue);
            });
        });
    }

    /**
     * Filter updates based on type
     */
    function filterUpdates(filterValue) {
        const tableRows = document.querySelectorAll('#updatesTable tbody tr');
        let visibleCount = 0;

        tableRows.forEach(row => {
            const updateType = row.getAttribute('data-update-type');
            
            if (filterValue === 'all') {
                row.style.display = '';
                visibleCount++;
            } else if (updateType === filterValue) {
                row.style.display = '';
                visibleCount++;
            } else {
                row.style.display = 'none';
            }
        });

        // Update display count
        updateDisplayCount(visibleCount);
    }

    /**
     * Initialize search functionality
     */
    function initializeSearch() {
        const searchInput = document.getElementById('searchInput');
        
        if (!searchInput) return;

        // Debounce search input to improve performance
        let searchTimeout;
        searchInput.addEventListener('input', function () {
            clearTimeout(searchTimeout);
            searchTimeout = setTimeout(() => {
                performSearch(this.value);
            }, 300);
        });
    }

    /**
     * Perform search on table rows
     */
    function performSearch(searchTerm) {
        const tableRows = document.querySelectorAll('#updatesTable tbody tr');
        const term = searchTerm.toLowerCase().trim();
        let visibleCount = 0;

        if (term === '') {
            // If search is empty, show all rows (respecting active filter)
            const activeFilter = document.querySelector('[data-filter].active');
            const filterValue = activeFilter ? activeFilter.getAttribute('data-filter') : 'all';
            filterUpdates(filterValue);
            return;
        }

        tableRows.forEach(row => {
            const searchContent = row.getAttribute('data-search-content').toLowerCase();
            
            if (searchContent.includes(term)) {
                row.style.display = '';
                visibleCount++;
            } else {
                row.style.display = 'none';
            }
        });

        // Update display count
        updateDisplayCount(visibleCount);
    }

    /**
     * Update the display count
     */
    function updateDisplayCount(count) {
        const displayCountElement = document.getElementById('displayCount');
        if (displayCountElement) {
            displayCountElement.textContent = count;
        }
    }

    /**
     * Add smooth scroll to top functionality
     */
    window.scrollToTop = function() {
        window.scrollTo({
            top: 0,
            behavior: 'smooth'
        });
    };

    /**
     * Confirm delete action
     */
    document.querySelectorAll('a[href*="Delete"]').forEach(link => {
        link.addEventListener('click', function(e) {
            if (!confirm('Are you sure you want to delete this update? This action cannot be undone.')) {
                e.preventDefault();
            }
        });
    });

    /**
     * Auto-dismiss alerts after 5 seconds
     */
    const alerts = document.querySelectorAll('.alert');
    alerts.forEach(alert => {
        setTimeout(() => {
            const bsAlert = new bootstrap.Alert(alert);
            bsAlert.close();
        }, 5000);
    });
})();
