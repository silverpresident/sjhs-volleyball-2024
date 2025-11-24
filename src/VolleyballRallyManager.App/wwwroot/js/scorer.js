// Capture data attributes from the script tag
const scriptTag = document.currentScript;
const matchId = scriptTag.getAttribute('data-match-id');
const homeTeamId = scriptTag.getAttribute('data-hometeam-id');
const awayTeamId = scriptTag.getAttribute('data-awayteam-id');
const initialCurrentSetNumber = parseInt(scriptTag.getAttribute('data-current-set-number'));
const initialSets = JSON.parse(scriptTag.getAttribute('data-sets'));
const initialIsFinished = scriptTag.getAttribute('data-is-finished') === 'true';

$(function () {
    let currentSetNumber = initialCurrentSetNumber;
    let scorerConnection;

    // Initialize match state
    let matchState = {
        homeScore: 0,
        awayScore: 0,
        homeSetsWon: 0,
        awaySetsWon: 0,
        sets: initialSets,
        isFinished: initialIsFinished,
        currentSetNumber: initialCurrentSetNumber
    };

    initializeSignalR();
    loadInitialState();

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

        // Receive feed updates
        scorerConnection.on("ReceiveFeedUpdate", function (update) {
            console.log("Feed update received:", update);
            addUpdateToFeed(update);
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
        const opcode = scriptTag.getAttribute('data-opcode');
        if (opcode == "handleQuickAction") {
            const action = scriptTag.getAttribute('data-action');
            handleQuickAction(action);
            return;
        }
        if (opcode == "handleSetAction") {
            const action = scriptTag.getAttribute('data-action');
            handleSetAction(action);
            return;
        }
        if (opcode == "updateScore") {
            const teamId = scriptTag.getAttribute('data-team-id');
            const scoreChange = scriptTag.getAttribute('data-score-change');
            updateScore(teamId, parseInt(scoreChange));
            return;
        }
    }

    async function handleSetAction(actionType) {
        try {
            await scorerConnection.invoke("SendSetStateChange", matchId, actionType);
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
        matchState.isFinished = data.isFinished;

        if (data.isFinished) {
            $('#btnStartMatch').prop('disabled', true);
            $('#btnEndMatch').prop('disabled', true);
            alert("Match has ended!");
        }

        updateButtonStates();
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

        // Disable score buttons if match finished
        if (matchState.isFinished) {
            $('.score-btn').prop('disabled', true);
            $('.set-control-btn').prop('disabled', true);
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
    $('body').on('click', '[data-opcode]', handeOpcodeClick);
});