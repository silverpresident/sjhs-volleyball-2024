// Announcer Board - SignalR Real-time Updates with AJAX
(function () {
    'use strict';

    // Initialize SignalR connection
    const announcerConnection = new signalR.HubConnectionBuilder()
        .withUrl("/TournamentHub")
        .withAutomaticReconnect()
        .build();

    // Get anti-forgery token
    function getAntiForgeryToken() {
        return document.querySelector('input[name="__RequestVerificationToken"]')?.value || '';
    }

    // Start connection
    announcerConnection.start()
        .then(() => {
            console.log('Announcer Board SignalR connected');

            announcerConnection.invoke("SubscribeToAnnouncer")
                .catch(function (err) {
                    console.error("Error subscribing to Announcer:", err);
                });
            announcerConnection.invoke("RequestForAnnouncements")
                .catch(function (err) {
                    console.error("Error requesting announcements:", err);
                });
        })
        .catch(err => {
            console.error('SignalR connection error:', err);
        });

    // Handle reconnection
    announcerConnection.onreconnecting(() => {
        console.log('Reconnecting to SignalR...');
    });

    announcerConnection.onreconnected(() => {
        console.log('Reconnected to SignalR');
        announcerConnection.invoke("RequestForAnnouncements")
            .catch(function (err) {
                console.error("Error requesting announcements:", err);
            });
    });

    announcerConnection.onclose(() => {
        console.log('SignalR connection closed');
    });

    // Helper function to get priority class
    function getPriorityClass(priority) {
        switch (priority) {
            case 0: // Urgent
                return 'border-danger';
            case 1: // Info
                return 'border-info';
            case 2: // Routine
                return 'border-secondary';
            default:
                return '';
        }
    }

    // Helper function to get priority text
    function getPriorityText(priority) {
        switch (priority) {
            case 0:
                return 'Urgent';
            case 1:
                return 'Info';
            case 2:
                return 'Routine';
            default:
                return 'Unknown';
        }
    }

    // Helper function to format time
    function formatTime(dateString) {
        if (!dateString) return '';
        const date = new Date(dateString);
        return date.toLocaleTimeString('en-US', { hour: '2-digit', minute: '2-digit', second: '2-digit' });
    }

    // Create announcement card HTML
    function createAnnouncementCard(announcement) {
        const priorityClass = getPriorityClass(announcement.priority);
        const priorityText = getPriorityText(announcement.priority);
        const lastAnnouncementTime = announcement.lastAnnouncementTime 
            ? formatTime(announcement.lastAnnouncementTime) 
            : '';

        const repeatBadge = announcement.remainingRepeatCount > 1 
            ? `<span class="badge bg-info"><i class="bi bi-arrow-repeat"></i> ${announcement.remainingRepeatCount}</span>`
            : '';

        const lastTimeInfo = lastAnnouncementTime 
            ? `<br /><i class="bi bi-clock"></i> Last: ${lastAnnouncementTime}`
            : '';

        const cardHtml = `
            <div class="col-md-6 col-lg-4 mb-4" data-announcement-id="${announcement.id}" style="order: ${announcement.sequencingNumber}; ${announcement.isHidden ? 'display: none;' : ''}">
                <div class="card h-100 announcement-card ${priorityClass}">
                    <div class="card-header d-flex justify-content-between align-items-center">
                        <div>
                            <span class="badge priority-badge">${priorityText}</span>
                            <span class="badge bg-secondary ms-2">#${announcement.sequencingNumber}</span>
                        </div>
                        <div>
                            ${repeatBadge}
                        </div>
                    </div>
                    <div class="card-body">
                        <h5 class="card-title">${escapeHtml(announcement.title)}</h5>
                        <p class="card-text">${escapeHtml(announcement.content)}</p>
                        
                        <div class="announcement-stats mb-3">
                            <small class="text-muted">
                                <i class="bi bi-megaphone"></i> Called: ${announcement.announcedCount} times
                                ${lastTimeInfo}
                            </small>
                        </div>
                    </div>
                    <div class="card-footer bg-transparent">
                        <div class="d-grid gap-2">
                            <button type="button" class="btn btn-success btn-lg w-100 btn-call-announcement" data-id="${announcement.id}">
                                <i class="bi bi-megaphone-fill"></i> Call/Announce
                            </button>
                            <div class="btn-group" role="group">
                                <button type="button" class="btn btn-warning w-100 btn-defer-announcement" data-id="${announcement.id}">
                                    <i class="bi bi-skip-forward"></i> Defer
                                </button>
                                <a href="/Admin/AnnouncerBoard/Details/${announcement.id}" class="btn btn-info">
                                    <i class="bi bi-eye"></i> Details
                                </a>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        `;
        
        return cardHtml;
    }

    // Escape HTML to prevent XSS
    function escapeHtml(text) {
        const div = document.createElement('div');
        div.textContent = text;
        return div.innerHTML;
    }

    // Update announcement display
    function updateAnnouncementDisplay(announcement) {
        const card = document.querySelector(`[data-announcement-id="${announcement.id}"]`);
        if (!card) {
            console.log('Card not found for update, creating new one:', announcement.id);
            renderAnnouncement(announcement);
            return;
        }

        // Update card classes
        const cardElement = card.querySelector('.announcement-card');
        cardElement.className = `card h-100 announcement-card ${getPriorityClass(announcement.priority)}`;

        // Update priority badge
        const priorityBadge = card.querySelector('.priority-badge');
        if (priorityBadge) {
            priorityBadge.textContent = getPriorityText(announcement.priority);
        }

        // Update sequence number
        const sequenceBadge = card.querySelector('.badge.bg-secondary');
        if (sequenceBadge) {
            sequenceBadge.textContent = `#${announcement.sequencingNumber}`;
        }

        // Update repeat count badge
        const badgeContainer = card.querySelector('.card-header > div:last-child');
        if (badgeContainer) {
            if (announcement.remainingRepeatCount > 1) {
                badgeContainer.innerHTML = `<span class="badge bg-info"><i class="bi bi-arrow-repeat"></i> ${announcement.remainingRepeatCount}</span>`;
            } else {
                badgeContainer.innerHTML = '';
            }
        }

        // Update title and content
        const titleElement = card.querySelector('.card-title');
        if (titleElement) {
            titleElement.textContent = announcement.title;
        }

        const contentElement = card.querySelector('.card-text');
        if (contentElement) {
            contentElement.textContent = announcement.content;
        }

        // Update stats
        const statsElement = card.querySelector('.announcement-stats small');
        if (statsElement) {
            const lastTimeInfo = announcement.lastAnnouncementTime 
                ? `<br /><i class="bi bi-clock"></i> Last: ${formatTime(announcement.lastAnnouncementTime)}`
                : '';
            statsElement.innerHTML = `<i class="bi bi-megaphone"></i> Called: ${announcement.announcedCount} times${lastTimeInfo}`;
        }

        // Update order
        card.style.order = announcement.sequencingNumber;

        // Update visibility
        if (announcement.isHidden) {
            card.style.display = 'none';
        } else {
            card.style.display = '';
        }
    }

    // Render new announcement
    function renderAnnouncement(announcement) {
        const board = document.getElementById('announcer-board');
        if (!board) return;

        // Check if already exists
        const existing = document.querySelector(`[data-announcement-id="${announcement.id}"]`);
        if (existing) {
            updateAnnouncementDisplay(announcement);
            return;
        }

        const cardHtml = createAnnouncementCard(announcement);
        board.insertAdjacentHTML('beforeend', cardHtml);
        
        // Attach event listeners to the new card
        attachCardEventListeners(announcement.id);
    }

    // Remove announcement card
    function removeAnnouncementCard(id) {
        const card = document.querySelector(`[data-announcement-id="${id}"]`);
        if (card) {
            card.style.transition = 'opacity 0.3s ease-out';
            card.style.opacity = '0';
            setTimeout(() => {
                card.remove();
                checkEmptyBoard();
            }, 300);
        }
    }

    // Hide announcement card
    function hideAnnouncementCard(id) {
        const card = document.querySelector(`[data-announcement-id="${id}"]`);
        if (card) {
            card.style.transition = 'opacity 0.3s ease-out';
            card.style.opacity = '0';
            setTimeout(() => {
                card.style.display = 'none';
                card.style.opacity = '1';
                checkEmptyBoard();
            }, 300);
        }
    }

    // Show announcement card
    function showAnnouncementCard(id) {
        const card = document.querySelector(`[data-announcement-id="${id}"]`);
        if (card) {
            card.style.display = '';
            card.style.opacity = '0';
            setTimeout(() => {
                card.style.transition = 'opacity 0.3s ease-in';
                card.style.opacity = '1';
            }, 10);
        }
    }

    // Set announcement order
    function setAnnouncementOrder(id, order) {
        const card = document.querySelector(`[data-announcement-id="${id}"]`);
        if (card) {
            card.style.order = order;
        }
    }

    // Check if board is empty and show message
    function checkEmptyBoard() {
        const board = document.getElementById('announcer-board');
        if (!board) return;

        const visibleCards = board.querySelectorAll('[data-announcement-id]:not([style*="display: none"])');
        
        if (visibleCards.length === 0) {
            // Show empty message if not already present
            if (!document.querySelector('.alert-info')) {
                const emptyMessage = `
                    <div class="alert alert-info text-center">
                        <i class="bi bi-inbox" style="font-size: 3rem;"></i>
                        <h4>No Announcements in Queue</h4>
                        <p>All announcements have been called or the queue is empty.</p>
                        <a href="/Admin/Announcements/Create" class="btn btn-success">
                            <i class="bi bi-plus-circle"></i> Create New Announcement
                        </a>
                    </div>
                `;
                board.insertAdjacentHTML('beforebegin', emptyMessage);
            }
        } else {
            // Remove empty message if present
            const emptyAlert = document.querySelector('.alert-info');
            if (emptyAlert) {
                emptyAlert.remove();
            }
        }
    }

    // Handle Call button click
    async function handleCallClick(button, announcementId) {
        button.disabled = true;
        button.innerHTML = '<i class="bi bi-hourglass-split"></i> Calling...';

        try {
            const response = await fetch(`/Admin/AnnouncerBoard/CallAjax/${announcementId}`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'RequestVerificationToken': getAntiForgeryToken()
                }
            });

            const result = await response.json();

            if (result.success) {
                // Hide the card immediately (SignalR will handle the full update)
                hideAnnouncementCard(announcementId);
            } else {
                // Re-enable button on error
                button.disabled = false;
                button.innerHTML = '<i class="bi bi-megaphone-fill"></i> Call/Announce';
                alert(result.message || 'Failed to call announcement');
            }
        } catch (error) {
            console.error('Error calling announcement:', error);
            button.disabled = false;
            button.innerHTML = '<i class="bi bi-megaphone-fill"></i> Call/Announce';
            alert('An error occurred while calling the announcement');
        }
    }

    // Handle Defer button click
    async function handleDeferClick(button, announcementId) {
        button.disabled = true;
        button.innerHTML = '<i class="bi bi-hourglass-split"></i> Deferring...';

        try {
            const response = await fetch(`/Admin/AnnouncerBoard/DeferAjax/${announcementId}`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'RequestVerificationToken': getAntiForgeryToken()
                }
            });

            const result = await response.json();

            if (result.success) {
                // Hide the card temporarily (SignalR will update with new position)
                const card = document.querySelector(`[data-announcement-id="${announcementId}"]`);
                if (card) {
                    card.style.opacity = '0.5';
                }
            } else {
                // Re-enable button on error
                button.disabled = false;
                button.innerHTML = '<i class="bi bi-skip-forward"></i> Defer';
                alert(result.message || 'Failed to defer announcement');
            }
        } catch (error) {
            console.error('Error deferring announcement:', error);
            button.disabled = false;
            button.innerHTML = '<i class="bi bi-skip-forward"></i> Defer';
            alert('An error occurred while deferring the announcement');
        }
    }

    // Attach event listeners to card buttons
    function attachCardEventListeners(announcementId) {
        const card = document.querySelector(`[data-announcement-id="${announcementId}"]`);
        if (!card) return;

        const callButton = card.querySelector('.btn-call-announcement');
        const deferButton = card.querySelector('.btn-defer-announcement');

        if (callButton) {
            callButton.addEventListener('click', function() {
                handleCallClick(this, announcementId);
            });
        }

        if (deferButton) {
            deferButton.addEventListener('click', function() {
                handleDeferClick(this, announcementId);
            });
        }
    }

    // SignalR Event Handlers

    // Listen for announcement created
    announcerConnection.on("AnnouncementCreated", function (announcement) {
        console.log('New announcement created:', announcement);
        renderAnnouncement(announcement);
    });

    // Listen for announcement called
    announcerConnection.on("AnnouncementCalled", function (announcement) {
        console.log('Announcement called:', announcement);
        updateAnnouncementDisplay(announcement);
    });

    // Listen for announcement updated
    announcerConnection.on("AnnouncementUpdated", function (announcement) {
        console.log('Announcement updated:', announcement);
        updateAnnouncementDisplay(announcement);
    });

    // Listen for announcement deleted
    announcerConnection.on("AnnouncementDeleted", function (announcementId) {
        console.log('Announcement deleted:', announcementId);
        removeAnnouncementCard(announcementId);
    });

    // Listen for announcement queue changes
    announcerConnection.on("AnnouncementQueueChanged", function (announcements) {
        console.log('Announcement queue changed:', announcements);
        
        const board = document.getElementById('announcer-board');
        if (!board) return;

        // Get all current announcement IDs on the board
        const currentCards = Array.from(board.querySelectorAll('[data-announcement-id]'));
        const currentIds = currentCards.map(card => card.getAttribute('data-announcement-id'));
        
        // Get all announcement IDs from the new list
        const newIds = announcements.map(a => a.id);

        // Remove cards that are not in the new list
        currentIds.forEach(id => {
            if (!newIds.includes(id)) {
                removeAnnouncementCard(id);
            }
        });

        // Update or create cards for announcements in the new list
        announcements.forEach(announcement => {
            const existingCard = document.querySelector(`[data-announcement-id="${announcement.id}"]`);
            if (existingCard) {
                updateAnnouncementDisplay(announcement);
            } else {
                renderAnnouncement(announcement);
            }
        });

        checkEmptyBoard();
    });

    // Listen for announcement property changes
    announcerConnection.on("AnnouncementPropertyChanged", function (announcementId, property, value) {
        console.log('Announcement property changed:', announcementId, property, value);
        
        if (property === 'hide') {
            hideAnnouncementCard(announcementId);
        } else if (property === 'show') {
            showAnnouncementCard(announcementId);
        } else if (property === 'order') {
            setAnnouncementOrder(announcementId, parseInt(value));
        }
    });

    // Keyboard shortcuts for announcer board
    document.addEventListener('keydown', function(e) {
        // Only enable shortcuts if we're on the announcer board
        if (!document.getElementById('announcer-board')) {
            return;
        }

        // Space bar - Call first announcement
        if (e.code === 'Space' && !e.target.matches('input, textarea, select')) {
            e.preventDefault();
            const firstCallButton = document.querySelector('.btn-call-announcement:not(:disabled)');
            if (firstCallButton) {
                firstCallButton.click();
            }
        }

        // D key - Defer first announcement
        if (e.code === 'KeyD' && !e.target.matches('input, textarea, select')) {
            e.preventDefault();
            const firstDeferButton = document.querySelector('.btn-defer-announcement:not(:disabled)');
            if (firstDeferButton) {
                firstDeferButton.click();
            }
        }
    });

    // Initialize event listeners on page load
    document.addEventListener('DOMContentLoaded', function() {
        // Attach event listeners to all existing cards
        const existingCards = document.querySelectorAll('[data-announcement-id]');
        existingCards.forEach(card => {
            const announcementId = card.getAttribute('data-announcement-id');
            attachCardEventListeners(announcementId);
        });

        checkEmptyBoard();
    });

})();
