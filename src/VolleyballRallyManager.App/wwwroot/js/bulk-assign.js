// Bulk Team Assignment Auto-Save
(function() {
    'use strict';

    // Debounce function to prevent too many saves
    function debounce(func, wait) {
        let timeout;
        return function executedFunction(...args) {
            const later = () => {
                clearTimeout(timeout);
                func(...args);
            };
            clearTimeout(timeout);
            timeout = setTimeout(later, wait);
        };
    }

    // Save team assignment
    async function saveTeamAssignment(teamId, divisionId, seedNumber) {
        const row = document.querySelector(`tr[data-team-id="${teamId}"]`);
        const statusIndicator = row.querySelector('.status-indicator');
        const savingIndicator = row.querySelector('.saving-indicator');
        
        try {
            // Show saving indicator
            savingIndicator.classList.remove('d-none');
            
            const response = await fetch('/Admin/TournamentTeams/BulkAssignUpdate', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify({
                    teamId: teamId,
                    divisionId: divisionId || null,
                    seedNumber: parseInt(seedNumber) || 0
                })
            });

            const result = await response.json();
            
            // Hide saving indicator
            savingIndicator.classList.add('d-none');
            
            if (result.success) {
                // Update status badge
                const isAssigned = divisionId && divisionId !== '';
                statusIndicator.innerHTML = isAssigned 
                    ? '<span class="badge bg-success"><i class="bi bi-check-circle"></i> Assigned</span>'
                    : '<span class="badge bg-secondary"><i class="bi bi-dash-circle"></i> Not Assigned</span>';
                
                // Enable/disable seed input based on assignment
                const seedInput = row.querySelector('.seed-input');
                seedInput.disabled = !isAssigned;
                
                // Show success animation
                row.classList.add('table-success');
                setTimeout(() => {
                    row.classList.remove('table-success');
                }, 1000);
            } else {
                // Show error
                showError(row, result.message || 'Failed to save');
            }
        } catch (error) {
            console.error('Error saving team assignment:', error);
            savingIndicator.classList.add('d-none');
            showError(row, 'Network error occurred');
        }
    }

    function showError(row, message) {
        row.classList.add('table-danger');
        setTimeout(() => {
            row.classList.remove('table-danger');
        }, 2000);
        
        console.error('Save error:', message);
    }

    // Debounced save function (500ms delay)
    const debouncedSave = debounce(saveTeamAssignment, 500);

    // Initialize event listeners
    function initializeBulkAssign() {
        const table = document.getElementById('bulkAssignTable');
        if (!table) return;

        // Handle division radio button changes
        table.addEventListener('change', function(e) {
            if (e.target.classList.contains('division-radio')) {
                const row = e.target.closest('tr');
                const teamId = row.dataset.teamId;
                const divisionId = e.target.value;
                const seedInput = row.querySelector('.seed-input');
                const seedNumber = seedInput.value || 0;
                
                // Save immediately on division change
                saveTeamAssignment(teamId, divisionId, seedNumber);
            }
        });

        // Handle seed number input changes
        table.addEventListener('input', function(e) {
            if (e.target.classList.contains('seed-input')) {
                const row = e.target.closest('tr');
                const teamId = row.dataset.teamId;
                const seedNumber = e.target.value;
                const divisionRadio = row.querySelector('.division-radio:checked');
                const divisionId = divisionRadio ? divisionRadio.value : '';
                
                // Only save if a division is selected
                if (divisionId) {
                    // Use debounced save for seed number changes
                    debouncedSave(teamId, divisionId, seedNumber);
                }
            }
        });

        // Prevent form submission on Enter key
        table.addEventListener('keydown', function(e) {
            if (e.key === 'Enter' && e.target.classList.contains('seed-input')) {
                e.preventDefault();
                e.target.blur();
            }
        });
    }

    // Initialize when DOM is ready
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', initializeBulkAssign);
    } else {
        initializeBulkAssign();
    }
})();
