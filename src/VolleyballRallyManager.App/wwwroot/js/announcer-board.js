// Announcer Board - SignalR Real-time Updates
(function () {
    'use strict';

    // Initialize SignalR connection
    const announcerConnection = new signalR.HubConnectionBuilder()
        .withUrl("/TournamentHub")
        .withAutomaticReconnect()
        .build();

    // Start connection
    announcerConnection.start()
        .then(() => {
            console.log('Announcer Board SignalR connected');

            announcerConnection.invoke("SubscribeToAnnouncer")
                .catch(function (err) {
                    console.error("Error subscribing to Announcer:", err);
                });
            announcerConnection.invoke("RequestForAnnouncements");
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
        announcerConnection.invoke("RequestForAnnouncements");
    });

    announcerConnection.onclose(() => {
        console.log('SignalR connection closed');
    });

    // Listen for announcement queue changes
    announcerConnection.on("AnnouncementQueueChanged", function (announcements) {
        console.log('Announcement queue changed - reloading board');
        // Reload the page to show updated queue
        location.reload();
    });

    // Listen for announcement created
    announcerConnection.on("AnnouncementCreated", function (announcement) {
        console.log('New announcement created:', announcement);
        // Reload the page to show new announcement
        //location.reload();
    });

    // Listen for announcement called
    announcerConnection.on("AnnouncementCalled", function (announcement) {
        console.log('Announcement called:', announcement);
        // Reload the page to show updated queue
        location.reload();
    });

    // Listen for announcement updated
    announcerConnection.on("AnnouncementUpdated", function (announcement) {
        console.log('Announcement updated:', announcement);
        // Reload the page to show changes
        location.reload();
    });

    // Listen for announcement deleted
    announcerConnection.on("AnnouncementDeleted", function (announcementId) {
        console.log('Announcement deleted:', announcementId);
        // Reload the page to remove deleted announcement
        location.reload();
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
            const firstCallButton = document.querySelector('.btn-success[type="submit"]');
            if (firstCallButton) {
                firstCallButton.click();
            }
        }

        // D key - Defer first announcement
        if (e.code === 'KeyD' && !e.target.matches('input, textarea, select')) {
            e.preventDefault();
            const firstDeferButton = document.querySelector('.btn-warning[type="submit"]');
            if (firstDeferButton) {
                firstDeferButton.click();
            }
        }
    });

    // Add visual feedback for button clicks
    document.addEventListener('DOMContentLoaded', function() {
        const callButtons = document.querySelectorAll('.btn-success[type="submit"]');
        callButtons.forEach(button => {
            button.addEventListener('click', function() {
                button.disabled = true;
                button.innerHTML = '<i class="bi bi-hourglass-split"></i> Calling...';
            });
        });

        const deferButtons = document.querySelectorAll('.btn-warning[type="submit"]');
        deferButtons.forEach(button => {
            button.addEventListener('click', function() {
                button.disabled = true;
                button.innerHTML = '<i class="bi bi-hourglass-split"></i> Deferring...';
            });
        });
    });

})();
