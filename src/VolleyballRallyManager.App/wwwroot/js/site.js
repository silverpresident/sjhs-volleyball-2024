// SignalR connection
let connection = null;

// Initialize SignalR connection
function initializeSignalR() {
    connection = new signalR.HubConnectionBuilder()
        .withUrl("/TournamentHub")
        .withAutomaticReconnect()
        .build();

    connection.start().catch(err => console.error(err.toString()));

    // Handle connection events
    connection.onreconnecting(error => {
        console.log('Reconnecting to SignalR hub...', error);
        showToast('warning', 'Connection lost. Attempting to reconnect...');
    });

    connection.onreconnected(connectionId => {
        console.log('Reconnected to SignalR hub.');
        showToast('success', 'Connection restored!');
    });

    connection.onclose(error => {
        console.log('Disconnected from SignalR hub.', error);
        showToast('error', 'Connection lost. Please refresh the page.');
    });

    // Register handlers for various updates
    connection.on("MatchCreated", match => {
        showToast('success', `New match created: ${match.homeTeam.name} vs ${match.awayTeam.name}`);
        refreshCurrentPage();
    });

    connection.on("MatchUpdated", match => {
        showToast('info', `Match updated: ${match.homeTeam.name} vs ${match.awayTeam.name}`);
        refreshCurrentPage();
    });

    connection.on("MatchDisputed", match => {
        showToast('warning', `Match disputed: ${match.homeTeam.name} vs ${match.awayTeam.name}`);
        refreshCurrentPage();
    });

    connection.on("BulletinCreated", bulletin => {
        showToast('success', 'New bulletin created');
        refreshCurrentPage();
    });

    connection.on("BulletinUpdated", bulletin => {
        showToast('info', 'Bulletin updated');
        refreshCurrentPage();
    });

    connection.on("BulletinDeleted", id => {
        showToast('info', 'Bulletin deleted');
        refreshCurrentPage();
    });
}

// Toast notifications
function showToast(type, message) {
    // Create toast container if it doesn't exist
    let toastContainer = document.getElementById('toast-container');
    if (!toastContainer) {
        toastContainer = document.createElement('div');
        toastContainer.id = 'toast-container';
        toastContainer.className = 'position-fixed bottom-0 end-0 p-3';
        document.body.appendChild(toastContainer);
    }

    // Create toast element
    const toastId = 'toast-' + Date.now();
    const toast = document.createElement('div');
    toast.className = `toast align-items-center text-white bg-${type} border-0`;
    toast.id = toastId;
    toast.setAttribute('role', 'alert');
    toast.setAttribute('aria-live', 'assertive');
    toast.setAttribute('aria-atomic', 'true');

    toast.innerHTML = `
        <div class="d-flex">
            <div class="toast-body">
                ${message}
            </div>
            <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast" aria-label="Close"></button>
        </div>
    `;

    toastContainer.appendChild(toast);

    // Initialize and show the toast
    const bsToast = new bootstrap.Toast(toast, {
        autohide: true,
        delay: 5000
    });
    bsToast.show();

    // Remove toast element after it's hidden
    toast.addEventListener('hidden.bs.toast', () => {
        toast.remove();
    });
}

// Refresh current page
function refreshCurrentPage() {
    if (!document.body.hasAttribute('data-no-refresh')) {
        location.reload();
    }
}

// Format date and time
function formatDateTime(dateString) {
    const date = new Date(dateString);
    return date.toLocaleDateString('en-GB', {
        day: '2-digit',
        month: '2-digit',
        year: 'numeric',
        hour: '2-digit',
        minute: '2-digit'
    });
}

// Handle confirmation dialogs
function confirmAction(message, callback) {
    if (confirm(message)) {
        callback();
    }
}

// Initialize tooltips and popovers
function initializeBootstrapComponents() {
    var tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
    tooltipTriggerList.map(function (tooltipTriggerEl) {
        return new bootstrap.Tooltip(tooltipTriggerEl);
    });

    var popoverTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="popover"]'));
    popoverTriggerList.map(function (popoverTriggerEl) {
        return new bootstrap.Popover(popoverTriggerEl);
    });
}

// Document ready handler
document.addEventListener('DOMContentLoaded', function() {
    // Initialize SignalR if the script is included
    if (typeof signalR !== 'undefined') {
        initializeSignalR();
    }

    // Initialize Bootstrap components
    initializeBootstrapComponents();

    // Handle form validation styling
    document.querySelectorAll('.form-control').forEach(input => {
        input.addEventListener('invalid', function() {
            input.classList.add('is-invalid');
        });
        input.addEventListener('input', function() {
            if (input.validity.valid) {
                input.classList.remove('is-invalid');
            }
        });
    });
});
