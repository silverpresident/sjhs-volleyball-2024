using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileSystemGlobbing;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;
using VolleyballRallyManager.Lib.Common;
using VolleyballRallyManager.Lib.Hubs;
using VolleyballRallyManager.Lib.Models;
using VolleyballRallyManager.Lib.Services;

namespace VolleyballRallyManager.Lib.Workers;

/// <summary>
/// Background service that processes scoring events from the ScoringChannel
/// </summary>
public class ScoringAutomationWorker : BackgroundService
{
    private readonly ScoringChannel _scoringChannel;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger<ScoringAutomationWorker> _logger;

    public ScoringAutomationWorker(
        ScoringChannel scoringChannel,
        IServiceScopeFactory serviceScopeFactory,
        ILogger<ScoringAutomationWorker> logger)
    {
        _scoringChannel = scoringChannel;
        _serviceScopeFactory = serviceScopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("ScoringAutomationWorker started");

        await foreach (var scoringEvent in _scoringChannel.Reader.ReadAllAsync(stoppingToken))
        {
            try
            {
                _logger.LogInformation(
                    "Processing scoring event: {EventType} for match {MatchId} from {Source}",
                    scoringEvent.EventType,
                    scoringEvent.MatchId,
                    scoringEvent.Source);

                await ProcessEventAsync(scoringEvent, stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error processing scoring event: {EventType} for match {MatchId}",
                    scoringEvent.EventType,
                    scoringEvent.MatchId);
            }
        }

        _logger.LogInformation("ScoringAutomationWorker stopped");
    }

    private async Task ProcessEventAsync(ScoringEvent scoringEvent, CancellationToken cancellationToken)
    {
        // Create a new scope for each event to get scoped services
        using var scope = _serviceScopeFactory.CreateScope();
        var matchService = scope.ServiceProvider.GetRequiredService<IMatchService>();
        var bulletinService = scope.ServiceProvider.GetRequiredService<IBulletinService>();
        var announcerService = scope.ServiceProvider.GetRequiredService<IAnnouncementService>();
        var notificationService = scope.ServiceProvider.GetRequiredService<ISignalRNotificationService>();

        switch (scoringEvent.EventType)
        {
            case ScoringEventType.CallToCourt:
                await HandleCallToCourtAsync(scoringEvent, matchService, notificationService, announcerService, bulletinService);
                break;

            case ScoringEventType.CallToSupport:
                await HandleCallToSupportAsync(scoringEvent, matchService, notificationService, announcerService);
                break;

            case ScoringEventType.MatchStart:
                await HandleMatchStartAsync(scoringEvent, matchService, notificationService, announcerService);
                break;

            case ScoringEventType.MatchSetStart:
                await HandleMatchSetStartAsync(scoringEvent, matchService, notificationService, announcerService);
                break;

            case ScoringEventType.MatchSetEnd:
                await HandleMatchSetEndAsync(scoringEvent, matchService, notificationService, announcerService);
                break;

            case ScoringEventType.MatchSetRevertToPrevious:
                await HandleMatchSetRevertToPreviousAsync(scoringEvent, matchService, notificationService, announcerService);
                break;

            case ScoringEventType.MatchSetScoreChange:
                await HandleMatchSetScoreChangeAsync(scoringEvent, matchService, notificationService, announcerService);
                break;

            case ScoringEventType.MatchEnd:
                await HandleMatchEndAsync(scoringEvent, matchService, notificationService, announcerService);
                break;

            case ScoringEventType.MatchDisputed:
                await HandleMatchDisputedAsync(scoringEvent, matchService, notificationService, announcerService);
                break;

            default:
                _logger.LogWarning("Unknown scoring event type: {EventType}", scoringEvent.EventType);
                break;
        }
    }

    private async Task HandleCallToCourtAsync(ScoringEvent scoringEvent, IMatchService matchService, ISignalRNotificationService notificationService, IAnnouncementService announcerService, IBulletinService bulletinService)
    {
        var match = await matchService.GetMatchAsync(scoringEvent.MatchId);
        if (match == null)
        {
            _logger.LogWarning("Match not found for CallToCourt: {MatchId}", scoringEvent.MatchId);
            return;
        }

        if (await matchService.IsCalledToCourt(scoringEvent.MatchId))
        {
            _logger.LogInformation("Match already called to court: {MatchId}", scoringEvent.MatchId);
            return;
        }

        // Create match update
        var update = new MatchUpdate
        {
            MatchId = scoringEvent.MatchId,
            UpdateType = UpdateType.CalledToCourt,
            CreatedBy = scoringEvent.UserName,
            Content = $"{match.HomeTeam?.Name} vs {match.AwayTeam?.Name} called to court"
        };
        await matchService.AddMatchUpdateAsync(update);
        await notificationService.NotifyAddFeedAsync(update);

        // Create public bulletin
        var bulletin = new Bulletin
        {
            Content = $"**Teams Called to Court**\n\n{match.HomeTeam?.Name} vs {match.AwayTeam?.Name} - Please report to {match.CourtLocation ?? "court"}",
            Priority = BulletinPriority.Warning,
            UseMarkdown = true,
            IsVisible = true,
            CreatedBy = scoringEvent.UserName
        };
        await bulletinService.CreateBulletinAsync(bulletin);
        var announcement = new Announcement
        {
            Content = $"We are ready to begin Match # {match.MatchNumber} : Teams {match.HomeTeam?.Name} and {match.AwayTeam?.Name} are called to court at {match.CourtLocation ?? "court"}.",
            IsHidden = false,
            CreatedBy = scoringEvent.UserName
        };
        await announcerService.CreateAnnouncementAsync(announcement);
        await notificationService.NotifyAnnouncementCreatedAsync(announcement);
        _logger.LogInformation("Processed CallToCourt for match {MatchId}", scoringEvent.MatchId);
    }

    private async Task HandleCallToSupportAsync(ScoringEvent scoringEvent, IMatchService matchService, ISignalRNotificationService notificationService, IAnnouncementService announcerService)
    {
        var match = await matchService.GetMatchAsync(scoringEvent.MatchId);
        if (match == null)
        {
            _logger.LogWarning("Match not found for CallToSupport: {MatchId}", scoringEvent.MatchId);
            return;
        }

        // Create match update requesting support
        /*var update = new MatchUpdate
        {
            MatchId = scoringEvent.MatchId,
            UpdateType = UpdateType.Other,
            Content = $"Support requested for {match.HomeTeam?.Name} vs {match.AwayTeam?.Name} on {match.CourtLocation ?? "court"}"
        };
        await matchService.AddMatchUpdateAsync(update);*/
        //TODO this message is for admin chat only - implement when admin chat is available

        _logger.LogInformation("Processed CallToSupport for match {MatchId} - Admin notification should be implemented", scoringEvent.MatchId);
    }

    private async Task HandleMatchStartAsync(ScoringEvent scoringEvent, IMatchService matchService, ISignalRNotificationService notificationService, IAnnouncementService announcerService)
    {
        var match = await matchService.StartMatchAsync(scoringEvent.MatchId, scoringEvent.UserName);
        await notificationService.NotifyMatchStartedAsync(match);
        string title = $"Match Started #{match.MatchNumber}";
        if (await announcerService.TitleExistsAsync(title) == false)
        {
            var announcement = new Announcement
            {
                Title = title,
                Content = $"Match #{match.MatchNumber} between Teams {match.HomeTeam?.Name} and {match.AwayTeam?.Name} has started on {match.CourtLocation ?? "court"}.",
                IsHidden = false,
                CreatedBy = scoringEvent.UserName
            };
            await announcerService.CreateAnnouncementAsync(announcement);
        }
        _logger.LogInformation("Processed MatchStart for match {MatchId}", scoringEvent.MatchId);
    }

    private async Task HandleMatchSetStartAsync(ScoringEvent scoringEvent, IMatchService matchService, ISignalRNotificationService notificationService, IAnnouncementService announcerService)
    {
        if (!scoringEvent.SetNumber.HasValue)
        {
            _logger.LogWarning("SetNumber is required for MatchSetStart event");
            return;
        }

        var match = await matchService.GetMatchAsync(scoringEvent.MatchId);
        if (match == null)
        {
            _logger.LogWarning("Match not found for MatchSetStart: {MatchId}", scoringEvent.MatchId);
            return;
        }
        if (scoringEvent.SetNumber.Value == match.CurrentSetNumber) 
        {
            _logger.LogWarning("Set already current SetNumber for MatchSetStart: {MatchId}, set {SetNumber}", scoringEvent.MatchId, scoringEvent.SetNumber);
            return;
        }
        if (0 == match.CurrentSetNumber)
        {
            await HandleMatchStartAsync(scoringEvent, matchService, notificationService, announcerService);
            return;
        }

            // Finish current set if needed
        if (match.CurrentSetNumber > 0)
        {
            var currentSet = await matchService.GetMatchSetAsync(scoringEvent.MatchId, match.CurrentSetNumber);
            if (currentSet != null && !currentSet.IsFinished)
            {
                await matchService.FinishSetAsync(scoringEvent.MatchId, match.CurrentSetNumber, scoringEvent.UserName);
            }
        }

        // Start next set
        await matchService.StartNextSetAsync(scoringEvent.MatchId, scoringEvent.Source, scoringEvent.SetNumber.Value);
        _logger.LogInformation("Processed MatchSetStart for match {MatchId}, set {SetNumber}", scoringEvent.MatchId, scoringEvent.SetNumber);
    }

    private async Task HandleMatchSetEndAsync(ScoringEvent scoringEvent, IMatchService matchService, ISignalRNotificationService notificationService, IAnnouncementService announcerService)
    {
        if (!scoringEvent.SetNumber.HasValue)
        {
            _logger.LogWarning("SetNumber is required for MatchSetEnd event");
            return;
        }
        var match = await matchService.GetMatchAsync(scoringEvent.MatchId);
        if (match == null)
        {
            _logger.LogWarning("Match not found for MatchSetStart: {MatchId}", scoringEvent.MatchId);
            return;
        }

        var matchSet = await matchService.FinishSetAsync(scoringEvent.MatchId, scoringEvent.SetNumber.Value, scoringEvent.Source);
        //add announcer
        string title = $"Match #{match.MatchNumber} Set {matchSet.SetNumber} Finished ";
        if (await announcerService.TitleExistsAsync(title) == false)
        {
            string winner;
            if (matchSet.AwayTeamScore > matchSet.HomeTeamScore)
            {
                winner = match.AwayTeam?.Name ?? "Away Team";
            }
            else
            {
                winner = match.HomeTeam?.Name ?? "Home Team";
            }
            var announcement = new Announcement
            {
                Title = title,
                Content = $"Match #{match.MatchNumber} between Teams {match.HomeTeam?.Name} and {match.AwayTeam?.Name} finished set {matchSet.SetNumber} with a score of {matchSet.HomeTeamScore}:{matchSet.AwayTeamScore} in favour of {winner}.",
                IsHidden = false,
                CreatedBy = scoringEvent.UserName
            };
            await announcerService.CreateAnnouncementAsync(announcement);
        }
        _logger.LogInformation("Processed MatchSetEnd for match {MatchId}, set {SetNumber}", scoringEvent.MatchId, scoringEvent.SetNumber);
    }

    private async Task HandleMatchSetRevertToPreviousAsync(ScoringEvent scoringEvent, IMatchService matchService, ISignalRNotificationService notificationService, IAnnouncementService announcerService)
    {
        if (!scoringEvent.SetNumber.HasValue)
        {
            _logger.LogWarning("SetNumber is required for MatchSetRevertToPrevious event");
            return;
        }

        await matchService.RevertToPreviousSetAsync(scoringEvent.MatchId, scoringEvent.Source, scoringEvent.SetNumber.Value);
        _logger.LogInformation("Processed MatchSetRevertToPrevious for match {MatchId}, set {SetNumber}", scoringEvent.MatchId, scoringEvent.SetNumber);
    }

    private async Task HandleMatchSetScoreChangeAsync(ScoringEvent scoringEvent, IMatchService matchService, ISignalRNotificationService notificationService, IAnnouncementService announcerService)
    {
        if (!scoringEvent.SetNumber.HasValue || !scoringEvent.TeamId.HasValue || !scoringEvent.ScoreChange.HasValue)
        {
            _logger.LogWarning("SetNumber, TeamId, and ScoreChange are required for MatchSetScoreChange event");
            return;
        }

        var match = await matchService.GetMatchAsync(scoringEvent.MatchId);
        if (match == null)
        {
            _logger.LogWarning("Match not found for MatchSetScoreChange: {MatchId}", scoringEvent.MatchId);
            return;
        }

        var currentSet = await matchService.GetOrCreateMatchSetAsync(
            scoringEvent.MatchId, 
            scoringEvent.SetNumber.Value, 
            scoringEvent.Source);

        if (currentSet == null || currentSet.SetNumber != scoringEvent.SetNumber.Value)
        {
            _logger.LogWarning("Set not found or not current for MatchSetScoreChange: {MatchId}, set {SetNumber}", 
                scoringEvent.MatchId, scoringEvent.SetNumber.Value);
            return;
        }

        if (currentSet.IsLocked)
        {
            //notifiy error
            _logger.LogWarning("Cannot update score - set is locked: {MatchId}, set {SetNumber}", 
                scoringEvent.MatchId, scoringEvent.SetNumber.Value);
            return;
        }

        int newHomeScore = currentSet.HomeTeamScore;
        int newAwayScore = currentSet.AwayTeamScore;

        if (scoringEvent.TeamId.Value == match.HomeTeamId)
        {
            newHomeScore += scoringEvent.ScoreChange.Value;
            if (newHomeScore < 0) newHomeScore = 0;
        }
        else if (scoringEvent.TeamId.Value == match.AwayTeamId)
        {
            newAwayScore += scoringEvent.ScoreChange.Value;
            if (newAwayScore < 0) newAwayScore = 0;
        }

        await matchService.UpdateSetScoreAsync(
            scoringEvent.MatchId, 
            scoringEvent.SetNumber.Value, 
            newHomeScore, 
            newAwayScore, 
            scoringEvent.Source);

        _logger.LogInformation(
            "Processed MatchSetScoreChange for match {MatchId}, set {SetNumber}: {HomeScore}-{AwayScore}", 
            scoringEvent.MatchId, 
            scoringEvent.SetNumber, 
            newHomeScore, 
            newAwayScore);
    }

    private async Task HandleMatchEndAsync(ScoringEvent scoringEvent, IMatchService matchService, ISignalRNotificationService notificationService, IAnnouncementService announcerService)
    {
        var match = await matchService.EndMatchAndLockSetsAsync(scoringEvent.MatchId, scoringEvent.Source);


        var update = new MatchUpdate
        {
            MatchId = match.Id,
            CreatedBy = scoringEvent.UserName,
            UpdateType = UpdateType.MatchFinished,
            Content = $"Match ended. Final score: {match.HomeTeamScore}-{match.AwayTeamScore} sets"
        };
        await matchService.AddMatchUpdateAsync(update);
        string title = $"Match Finished #{match.MatchNumber}";
        if (await announcerService.TitleExistsAsync(title) == false)
        {
            //TODO include score for each set in announcement
            var announcement = new Announcement
            {
                Title = title,
                Content = $"Match #{match.MatchNumber} between Teams {match.HomeTeam?.Name} and {match.AwayTeam?.Name} has ended. Final score: {match.HomeTeamScore}-{match.AwayTeamScore} sets.",
                IsHidden = false,
                CreatedBy = scoringEvent.UserName
            };
            await announcerService.CreateAnnouncementAsync(announcement);
        }
        await notificationService.NotifyMatchFinishedAsync(match);
        _logger.LogInformation("Processed MatchEnd for match {MatchId}", scoringEvent.MatchId);
    }

    private async Task HandleMatchDisputedAsync(ScoringEvent scoringEvent, IMatchService matchService, ISignalRNotificationService notificationService, IAnnouncementService announcerService)
    {
        var match = await matchService.GetMatchAsync(scoringEvent.MatchId);
        if (match == null)
        {
            _logger.LogWarning("Match not found for MatchDisputed: {MatchId}", scoringEvent.MatchId);
            return;
        }

        bool isDisputed = false;
        if (scoringEvent.AdditionalData?.ContainsKey("IsDisputed") == true)
        {
            isDisputed = (bool)scoringEvent.AdditionalData["IsDisputed"];
        }

        match.IsDisputed = isDisputed;
        await matchService.UpdateMatchAsync(match);

        var update = new MatchUpdate
        {
            MatchId = scoringEvent.MatchId,
            UpdateType = UpdateType.DisputeRaised,
            Content = isDisputed ? "Match marked as disputed" : "Dispute cleared"
        };
        await matchService.AddMatchUpdateAsync(update);
        await notificationService.NotifyMatchUpdatedAsync(match);
        _logger.LogInformation("Processed MatchDisputed for match {MatchId}, disputed: {IsDisputed}", 
            scoringEvent.MatchId, isDisputed);
    }
}
