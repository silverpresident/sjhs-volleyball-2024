// Updates page - Real-time match updates feed
(function () {
    'use strict';

    let connection = null;
    const MAX_UPDATES = 25;

    // Initialize on page load
    document.addEventListener('DOMContentLoaded', function () {
        initializeUpdateCards();
        initializeSignalR();
        startTimestampUpdates();
    });

    /**
     * Render all existing update cards from server-side data
     */
    function initializeUpdateCards() {
        const updateCards = document.querySelectorAll('.update-card');
        updateCards.forEach(card => {
            const updateData = extractUpdateData(card);
            const renderedCard = renderUpdateCard(updateData);
            card.outerHTML = renderedCard;
        });
    }

    /**
     * Extract update data from data attributes
     */
    function extractUpdateData(element) {
        return {
            id: element.dataset.id,
            matchId: element.dataset.matchId,
            type: element.dataset.type,
            content: element.dataset.content,
            createdAt: element.dataset.created,
            homeTeam: element.dataset.homeTeam,
            awayTeam: element.dataset.awayTeam,
            court: element.dataset.court,
            round: element.dataset.round
        };
    }

    /**
     * Render an update card with markdown support and time ago
     */
    function renderUpdateCard(update) {
        const updateType = getUpdateTypeInfo(update.type);
        const renderedContent = update.content ? marked.parse(update.content) : '';
        const timeAgo = moment(update.createdAt).fromNow();
        const matchContext = getMatchContext(update);

        return `
            <div class="update-card mb-3" data-update-id="${update.id}" data-created="${update.createdAt}">
                <div class="card">
                    <div class="card-header d-flex justify-content-between align-items-center bg-${updateType.color}">
                        <div>
                             <a class="btn btn-link" href="/Matches/Details/${update.matchId}">
                                <i class="bi bi-info-circle"></i>
                             </a>
                            <span class="badge bg-${updateType.badgeColor} me-2">
                                <i class="${updateType.icon}"></i> ${updateType.label}
                            </span>
                        </div>
                        <div>
                            ${matchContext}
                        </div>
                        <small class="text-muted timestamp" data-time="${update.createdAt}">${timeAgo}</small>
                    </div>
                    <div class="card-body">
                        <div class="markdown-content">${renderedContent}</div>
                    </div>
                </div>
            </div>
        `;
    }

    /**
     * Get match context string for display
     */
    function getMatchContext(update) {
        if (!update.homeTeam || !update.awayTeam) {
            return '';
        }
        
        let context = `<strong>${update.homeTeam}</strong> vs <strong>${update.awayTeam}</strong>`;
        
        if (update.court) {
            context += ` <span class="text-muted">@ ${update.court}</span>`;
        }
        
        if (update.round) {
            context += ` <span class="badge bg-secondary">${update.round}</span>`;
        }
        
        return context;
    }

    /**
     * Get update type information for styling
     */
    function getUpdateTypeInfo(type) {
        const types = {
            'ScoreUpdate': { 
                label: 'Score Update', 
                icon: 'bi bi-trophy-fill', 
                color: 'success', 
                badgeColor: 'success' 
            },
            'MatchStarted': { 
                label: 'Match Started', 
                icon: 'bi bi-play-circle-fill', 
                color: 'primary', 
                badgeColor: 'primary' 
            },
            'MatchFinished': { 
                label: 'Match Finished', 
                icon: 'bi bi-check-circle-fill', 
                color: 'info', 
                badgeColor: 'info' 
            },
            'MatchSetStarted': { 
                label: 'Set Started', 
                icon: 'bi bi-play-fill', 
                color: 'primary', 
                badgeColor: 'primary' 
            },
            'MatchSetFinished': { 
                label: 'Set Finished', 
                icon: 'bi bi-check-circle', 
                color: 'success', 
                badgeColor: 'success' 
            },
            'DisputeRaised': { 
                label: 'Dispute Raised', 
                icon: 'bi bi-exclamation-triangle-fill', 
                color: 'warning', 
                badgeColor: 'warning' 
            },
            'RefereeAssigned': { 
                label: 'Referee Assigned', 
                icon: 'bi bi-person-badge-fill', 
                color: 'secondary', 
                badgeColor: 'secondary' 
            },
            'ScorerAssigned': { 
                label: 'Scorer Assigned', 
                icon: 'bi bi-pencil-square', 
                color: 'secondary', 
                badgeColor: 'secondary' 
            },
            'LocationChanged': { 
                label: 'Location Changed', 
                icon: 'bi bi-geo-alt-fill', 
                color: 'info', 
                badgeColor: 'info' 
            },
            'TimeChanged': { 
                label: 'Time Changed', 
                icon: 'bi bi-clock-fill', 
                color: 'info', 
                badgeColor: 'info' 
            },
            'CalledToCourt': { 
                label: 'Called to Court', 
                icon: 'bi bi-megaphone-fill', 
                color: 'warning', 
                badgeColor: 'warning' 
            },
            'Other': { 
                label: 'Update', 
                icon: 'bi bi-info-circle-fill', 
                color: 'light', 
                badgeColor: 'secondary' 
            }
        };

        return types[type] || types['Other'];
    }

    /**
     * Initialize SignalR connection for real-time updates
     */
    function initializeSignalR() {
        connection = new signalR.HubConnectionBuilder()
            .withUrl("/hubs/match")
            .withAutomaticReconnect()
            .build();

        connection.on("ReceiveMatchUpdate", function (update) {
            handleNewUpdate(update);
        });

        connection.start()
            .then(function () {
                console.log("SignalR connected");
                // Subscribe to updates group
                connection.invoke("SubscribeToUpdates")
                    .catch(function (err) {
                        console.error("Error subscribing to updates:", err);
                    });
            })
            .catch(function (err) {
                console.error("SignalR connection error:", err);
            });

        connection.onreconnected(function () {
            console.log("SignalR reconnected");
            connection.invoke("SubscribeToUpdates")
                .catch(function (err) {
                    console.error("Error re-subscribing to updates:", err);
                });
        });
    }

    /**
     * Handle new update from SignalR
     */
    function handleNewUpdate(update) {
        const container = document.getElementById('updates-container');
        if (!container) return;

        // Create update data object
        const updateData = {
            id: update.id,
            matchId: update.matchId,
            type: update.updateType || 'Other',
            content: update.content,
            createdAt: update.createdAt,
            homeTeam: update.match?.homeTeam?.name || '',
            awayTeam: update.match?.awayTeam?.name || '',
            court: update.match?.courtLocation || '',
            round: update.match?.round?.name || ''
        };

        // Render the new card
        const newCardHtml = renderUpdateCard(updateData);
        const tempDiv = document.createElement('div');
        tempDiv.innerHTML = newCardHtml;
        const newCard = tempDiv.firstElementChild;

        // Add fade-in class
        newCard.classList.add('update-fade-in');

        // Prepend to container
        container.insertBefore(newCard, container.firstChild);

        // Remove last card if we exceed MAX_UPDATES
        const allCards = container.querySelectorAll('.update-card');
        if (allCards.length > MAX_UPDATES) {
            const lastCard = allCards[allCards.length - 1];
            lastCard.classList.add('update-fade-out');
            setTimeout(() => {
                lastCard.remove();
            }, 300);
        }

        // Trigger animation
        setTimeout(() => {
            newCard.classList.remove('update-fade-in');
        }, 10);
    }

    /**
     * Update all timestamps every minute
     */
    function startTimestampUpdates() {
        updateTimestamps();
        setInterval(updateTimestamps, 60000); // Update every minute
    }

    /**
     * Update all timestamp displays
     */
    function updateTimestamps() {
        const timestamps = document.querySelectorAll('.timestamp[data-time]');
        timestamps.forEach(el => {
            const time = el.dataset.time;
            el.textContent = moment(time).fromNow();
        });
    }

    // Clean up on page unload
    window.addEventListener('beforeunload', function () {
        if (connection) {
            connection.stop();
        }
    });
})();
