// Capture data attributes from the script tag
const scriptTag = document.currentScript;
const matchId = scriptTag.getAttribute('data-match-id');
const homeTeamId = scriptTag.getAttribute('data-hometeam-id');
const awayTeamId = scriptTag.getAttribute('data-awayteam-id');
const initialCurrentSetNumber = parseInt(scriptTag.getAttribute('data-current-set-number'));
const initialSets = JSON.parse(scriptTag.getAttribute('data-sets'));
const initialIsFinished = scriptTag.getAttribute('data-is-finished') === 'true';

$(function () {
    const scorerContainer = $('#scorerContainer');
    let currentSetNumber = initialCurrentSetNumber;
    let scorerConnection;

    // Initialize match state
    let matchState = {
        homeScore: 0,
        awayScore: 0,
        homeSetsWon: 0,
        awaySetsWon: 0,
        sets: initialSets,
        isDisputed: false,
        isFinished: initialIsFinished,
        currentSetNumber: initialCurrentSetNumber
    };

    function initializeSignalR() {
        scorerConnection = new signalR.HubConnectionBuilder()
            .withUrl("/scorerhub")
            .withAutomaticReconnect()
            .build();

        // Receive score updates
        scorerConnection.on("ReceiveScoreUpdate", function (data) {
            console.log("Score update received:", data);
            updateScoreDisplay(data.setNumber, data.homeScore, data.awayScore);
        });

        // Receive set state changes
        scorerConnection.on("ReceiveSetStateChange", function (data) {
            console.log("Set state change received:", data);
            handleSetStateUpdate(data);
        });

        // Receive match state changes
        scorerConnection.on("ReceiveMatchStateChange", function (data) {
            console.log("Match state change received:", data);
            handleMatchStateUpdate(data);
        });
        // Receive match state  (initial)
        scorerConnection.on("ReceiveMatchState", function (data) {
            console.log("MatchState received:", data);
            handleMatchStateUpdate(data);
        }); 


        // Receive feed updates
        scorerConnection.on("ReceiveFeedUpdate", function (update) {
            console.log("Feed update received:", update);
            addUpdateToFeed(update);
        });
        // Receive complete feed
        scorerConnection.on("ReceiveFeedList", function (updates) {
            console.log("Feed list received:", updates);
            updates.forEach(update => {
                addUpdateToFeed(update);
            });
        });
        scorerConnection.start()
            .then(() => {
                console.log("SignalR Connected");
                scorerConnection.invoke("JoinMatchGroup", matchId);
            })
            .catch(err => console.error("SignalR Error:", err));
    }

    function loadInitialState() {
        // Load current set scores
        const currentSet = matchState.sets.find(s => s.setNumber === matchState.currentSetNumber);
        if (currentSet) {
            matchState.homeScore = currentSet.homeTeamScore;
            matchState.awayScore = currentSet.awayTeamScore;
            updateScoreDisplay(matchState.currentSetNumber, matchState.homeScore, matchState.awayScore);
        }

        // Update sets won
        $('#homeSetsWon').text(matchState.homeSetsWon);
        $('#awaySetsWon').text(matchState.awaySetsWon);

        // Update set label
        $('#setLabel').text('SET ' + matchState.currentSetNumber);

        // Update set history
        updateSetHistory();

        // Update button states
        updateButtonStates();
    }

    async function updateScore(teamId, increment) {
        if (matchState.isFinished) {
            alert("Match is finished. Cannot update scores.");
            return;
        }

        try {
            if (teamId == homeTeamId) {
                matchState.homeScore += increment;
            }
            if (teamId == awayTeamId) {
                matchState.awayScore += increment;
            }
            updateScoreDisplay(matchState.currentSetNumber, matchState.homeScore, matchState.awayScore);
            await scorerConnection.invoke("SendScoreUpdate", matchId, matchState.currentSetNumber, teamId, increment);
        } catch (err) {
            console.error("Error sending score update:", err);
            alert("Failed to update score. Please try again.");
        }
    }

    function updateScoreDisplay(setNumber, homeScore, awayScore) {
        if (setNumber === matchState.currentSetNumber) {
            matchState.homeScore = homeScore;
            matchState.awayScore = awayScore;
            $('#homeScore').text(homeScore);
            $('#awayScore').text(awayScore);
        }
    }
    function handeOpcodeClick(event) {
        event.preventDefault();
        const opcode = this.getAttribute('data-opcode');
        console.log('Op: ', opcode);
        if (opcode == "handleQuickAction") {
            const action = this.getAttribute('data-action');
            handleQuickAction(action);
            return;
        }
        if (opcode == "handleSetAction") {
            const action = this.getAttribute('data-action');
            handleSetAction(action);
            return;
        }
        if (opcode == "updateScore") {
            const teamId = this.getAttribute('data-team-id');
            const scoreChange = this.getAttribute('data-score-change');
            updateScore(teamId, parseInt(scoreChange));
            return;
        }
    }

    async function handleSetAction(actionType) {
        try {
            await scorerConnection.invoke("SendSetStateChange", matchId, actionType, matchState.currentSetNumber);
        } catch (err) {
            console.error("Error sending set state change:", err);
            alert("Failed to perform action. Please try again.");
        }
    }

    function handleSetStateUpdate(data) {
        matchState.currentSetNumber = data.currentSetNumber;
        matchState.sets = data.sets;
        matchState.homeSetsWon = data.homeSetsWon;
        matchState.awaySetsWon = data.awaySetsWon;

        // Update display
        $('#setLabel').text('SET ' + data.currentSetNumber);
        $('#homeSetsWon').text(data.homeSetsWon);
        $('#awaySetsWon').text(data.awaySetsWon);

        // Reset current scores for new set
        const currentSet = matchState.sets.find(s => s.setNumber === matchState.currentSetNumber);
        if (currentSet) {
            matchState.homeScore = currentSet.homeTeamScore;
            matchState.awayScore = currentSet.awayTeamScore;
            $('#homeScore').text(matchState.homeScore);
            $('#awayScore').text(matchState.awayScore);
        } else {
            matchState.homeScore = 0;
            matchState.awayScore = 0;
            $('#homeScore').text(0);
            $('#awayScore').text(0);
        }

        updateSetHistory();
        updateButtonStates();
    }

    async function handleQuickAction(actionType) {
        try {
            await scorerConnection.invoke("SendQuickAction", matchId, actionType);
        } catch (err) {
            console.error("Error sending quick action:", err);
            alert("Failed to perform action. Please try again.");
        }
    }

    function handleMatchStateUpdate(data) {
        matchState.currentSetNumber = data.currentSetNumber;
        matchState.isFinished = data.isFinished;
        matchState.isDisputed = data.isDisputed;
        matchState.isLocked = data.isLocked;

        if (data.isFinished) {
            alert("Match has ended!");
        }
        updateDisplayStates();
    }

    function updateSetHistory() {
        const historyHtml = matchState.sets
            .filter(s => s.isFinished)
            .sort((a, b) => a.setNumber - b.setNumber)
            .map(s => `
            <div class="set-history-item">
                <strong>Set ${s.setNumber}:</strong> 
                ${s.homeTeamScore} v ${s.awayTeamScore}
                ${s.isLocked ? '<span class="badge bg-secondary ms-2">Locked</span>' : ''}
            </div>
        `).join('');

        $('#setHistoryList').html(historyHtml || '<p class="text-muted">No sets completed yet</p>');
    }
    function updateDisplayStates() {
        if (matchState.isFinished) {
            $('#btnStartMatch').prop('disabled', true);
            $('#btnEndMatch').prop('disabled', true);
        }
        updateButtonStates();
    }
        function updateButtonStates() {
        const currentSet = matchState.sets.find(s => s.setNumber === matchState.currentSetNumber);
        const isCurrentSetFinished = currentSet ? currentSet.isFinished : false;

        // Previous Set button: enabled only if current set is 0-0 AND not first set
        const canRevert = matchState.homeScore === 0 && matchState.awayScore === 0 && matchState.currentSetNumber > 1;
        $('#btnPreviousSet').prop('disabled', !canRevert);

        // End Set button: visible when set is in progress
        if (isCurrentSetFinished) {
            $('#btnEndSet').hide();
            $('#btnNewSet').show();
        } else {
            $('#btnEndSet').show();
            $('#btnNewSet').hide();
        }

        if (matchState.currentSetNumber == 0) {
            $('[data-score-change]').prop('disabled', true).hide();
            $('#btnEndSet').hide();
            $('#btnPreviousSet').hide();
            $('#btnNewSet').show().prop('disabled', false);
            $('[data-action="Disputed"]').hide();
            $('[data-action="MatchEnded"]').hide();
        } else {
            $('#setBtnDisplayArea').show();
            $('[data-score-change]').prop('disabled', false).show();
            $('#btnPreviousSet').show();
            $('[data-action="Disputed"]').show();
            $('[data-action="MatchEnded"]').show();
        }
        // Disable score buttons if match finished
        if (matchState.isFinished) {
            $('.score-btn').prop('disabled', true);
            $('.set-control-btn').prop('disabled', true);
            updateDisplayFinished();
            }
            if (matchState.isLocked) {
                $('[data-action="Disputed"]').hide();
            }
    }
    function updateDisplayFinished() {
        if (matchState.isFinished != true) {
            return;
        }
        let el = $('#setLabel');
        if (matchState.isDisputed) {
            el.text('DISPUTED');
            scorerContainer.addClass('disputed-match'); 
        } else {
            el.text('FINISHED');
            scorerContainer.addClass('finished-match'); 
        }
        updateScoreDisplay(matchState.currentSetNumber, matchState.homeSetsWon, matchState.awaySetsWon);
        $('[data-score-change]').prop('disabled', true).hide();
        $('#setBtnDisplayArea').hide();
        $('[data-action="CallToCourt"]').hide();
        $('[data-action="MatchStarted"]').hide();
        $('[data-action="MatchEnded"]').hide();
        if (matchState.homeSetsWon > matchState.awaySetsWon) {
            $('#homeTeamPanel').find('.bi').show();
        }
        if (matchState.awaySetsWon > matchState.homeSetsWon) {
            $('#awayTeamPanel').find('.bi').show();
        }
    }
    function addUpdateToFeed(update) {
        const updateHtml = `
        <div class="update-item">
            <div class="update-time">${new Date(update.timestamp).toLocaleTimeString()}</div>
            <div>${update.content}</div>
            ${update.updateType ? '<small class="badge bg-primary">' + update.updateType + '</small>' : ''}
        </div>
    `;

        $('#updateFeed').prepend(updateHtml);

        // Keep only last 15 updates
        $('.update-item').slice(15).remove();
    }
    initializeSignalR();
    loadInitialState();
    $('body').on('click', '[data-opcode]', handeOpcodeClick);
});