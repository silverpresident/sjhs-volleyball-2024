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
/// Background service that processes Tournament events from the TournamentChannel
/// </summary>
public class TournamentAutomationWorker : BackgroundService
{
    private readonly TournamentChannel _TournamentChannel;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger<TournamentAutomationWorker> _logger;

    public TournamentAutomationWorker(
        TournamentChannel TournamentChannel,
        IServiceScopeFactory serviceScopeFactory,
        ILogger<TournamentAutomationWorker> logger)
    {
        _TournamentChannel = TournamentChannel;
        _serviceScopeFactory = serviceScopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("TournamentAutomationWorker started");

        await foreach (var TournamentEvent in _TournamentChannel.Reader.ReadAllAsync(stoppingToken))
        {
            try
            {
                _logger.LogInformation(
                    "Processing Tournament event: {EventType} for Tournament {TournamentId} from {Source}",
                    TournamentEvent.EventType,
                    TournamentEvent.TournamentId,
                    TournamentEvent.Source);

                await ProcessEventAsync(TournamentEvent, stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error processing Tournament event: {EventType} for Tournament {TournamentId}",
                    TournamentEvent.EventType,
                    TournamentEvent.TournamentId);
            }
        }

        _logger.LogInformation("TournamentAutomationWorker stopped");
    }

    private async Task ProcessEventAsync(TournamentEvent TournamentEvent, CancellationToken cancellationToken)
    {
        // Create a new scope for each event to get scoped services
        using var scope = _serviceScopeFactory.CreateScope();
        var rankService = scope.ServiceProvider.GetRequiredService<IRanksService>();
        //var matchService = scope.ServiceProvider.GetRequiredService<IMatchService>();
        var bulletinService = scope.ServiceProvider.GetRequiredService<IBulletinService>();
        var announcerService = scope.ServiceProvider.GetRequiredService<IAnnouncementService>();
        var notificationService = scope.ServiceProvider.GetRequiredService<ISignalRNotificationService>();
        
        switch (TournamentEvent.EventType)
        {
            case TournamentEventType.UpdateDivisionRanks:
                await HandleUpdateDivisionRanks(TournamentEvent, rankService, notificationService, announcerService, bulletinService);
                break;
            /*
        case TournamentEventType.CallToSupport:
            await HandleCallToSupportAsync(TournamentEvent, matchService, notificationService, announcerService);
            break;

        case TournamentEventType.MatchStart:
            await HandleMatchStartAsync(TournamentEvent, matchService, notificationService, announcerService);
            break;

        case TournamentEventType.MatchSetStart:
            await HandleMatchSetStartAsync(TournamentEvent, matchService, notificationService, announcerService);
            break;

        case TournamentEventType.MatchSetEnd:
            await HandleMatchSetEndAsync(TournamentEvent, matchService, notificationService, announcerService);
            break;

        case TournamentEventType.MatchSetRevertToPrevious:
            await HandleMatchSetRevertToPreviousAsync(TournamentEvent, matchService, notificationService, announcerService);
            break;

        case TournamentEventType.MatchSetScoreChange:
            await HandleMatchSetScoreChangeAsync(TournamentEvent, matchService, notificationService, announcerService);
            break;

        case TournamentEventType.MatchEnd:
            await HandleMatchEndAsync(TournamentEvent, matchService, notificationService, announcerService);
            break;

        case TournamentEventType.MatchDisputed:
            await HandleMatchDisputedAsync(TournamentEvent, matchService, notificationService, announcerService);
            break;
    */
            default:
                _logger.LogWarning("Unknown Tournament event type: {EventType}", TournamentEvent.EventType);
                break;
        }
    }

    private async Task HandleUpdateDivisionRanks(TournamentEvent tournamentEvent, IRanksService rankService, ISignalRNotificationService notificationService, IAnnouncementService announcerService, IBulletinService bulletinService)
    {
        await rankService.UpdateDivisionRanksAsync(tournamentEvent.TournamentId, tournamentEvent.DivisionId ?? Guid.Empty);
        _logger.LogInformation("Processed HandleUpdateDivisionRanks for Tournament {TournamentId}", tournamentEvent.TournamentId);
    }
    /*
private async Task HandleCallToCourtAsync(TournamentEvent TournamentEvent, IMatchService matchService, ISignalRNotificationService notificationService, IAnnouncementService announcerService, IBulletinService bulletinService)
{
   var match = await matchService.GetMatchAsync(TournamentEvent.MatchId);
   if (match == null)
   {
       _logger.LogWarning("Match not found for CallToCourt: {MatchId}", TournamentEvent.MatchId);
       return;
   }

   if (await matchService.IsCalledToCourt(TournamentEvent.MatchId))
   {
       _logger.LogInformation("Match already called to court: {MatchId}", TournamentEvent.MatchId);
       return;
   }

   // Create match update
   var update = new MatchUpdate
   {
       MatchId = TournamentEvent.MatchId,
       UpdateType = UpdateType.CalledToCourt,
       CreatedBy = TournamentEvent.UserName,
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
       CreatedBy = TournamentEvent.UserName
   };
   await bulletinService.CreateBulletinAsync(bulletin);
   var announcement = new Announcement
   {
       Content = $"We are ready to begin Match # {match.MatchNumber} : Teams {match.HomeTeam?.Name} and {match.AwayTeam?.Name} are called to court at {match.CourtLocation ?? "court"}.",
       IsHidden = false,
       CreatedBy = TournamentEvent.UserName
   };
   await announcerService.CreateAnnouncementAsync(announcement);
   await notificationService.NotifyAnnouncementCreatedAsync(announcement);
   _logger.LogInformation("Processed CallToCourt for match {MatchId}", TournamentEvent.MatchId);
}

private async Task HandleMatchStartAsync(TournamentEvent TournamentEvent, IMatchService matchService, ISignalRNotificationService notificationService, IAnnouncementService announcerService)
{
   var match = await matchService.StartMatchAsync(TournamentEvent.MatchId, TournamentEvent.UserName);
   await notificationService.NotifyMatchStartedAsync(match);
   string title = $"Match Started #{match.MatchNumber}";
   if (await announcerService.TitleExistsAsync(title) == false)
   {
       var announcement = new Announcement
       {
           Title = title,
           Content = $"Match #{match.MatchNumber} between Teams {match.HomeTeam?.Name} and {match.AwayTeam?.Name} has started on {match.CourtLocation ?? "court"}.",
           IsHidden = false,
           CreatedBy = TournamentEvent.UserName
       };
       await announcerService.CreateAnnouncementAsync(announcement);
   }
   _logger.LogInformation("Processed MatchStart for match {MatchId}", TournamentEvent.MatchId);
}

private async Task HandleMatchSetStartAsync(TournamentEvent TournamentEvent, IMatchService matchService, ISignalRNotificationService notificationService, IAnnouncementService announcerService)
{
   if (!TournamentEvent.SetNumber.HasValue)
   {
       _logger.LogWarning("SetNumber is required for MatchSetStart event");
       return;
   }

   var match = await matchService.GetMatchAsync(TournamentEvent.MatchId);
   if (match == null)
   {
       _logger.LogWarning("Match not found for MatchSetStart: {MatchId}", TournamentEvent.MatchId);
       return;
   }
   if (TournamentEvent.SetNumber.Value == match.CurrentSetNumber) 
   {
       _logger.LogWarning("Set already current SetNumber for MatchSetStart: {MatchId}, set {SetNumber}", TournamentEvent.MatchId, TournamentEvent.SetNumber);
       return;
   }
   if (0 == match.CurrentSetNumber)
   {
       await HandleMatchStartAsync(TournamentEvent, matchService, notificationService, announcerService);
       return;
   }

       // Finish current set if needed
   if (match.CurrentSetNumber > 0)
   {
       var currentSet = await matchService.GetMatchSetAsync(TournamentEvent.MatchId, match.CurrentSetNumber);
       if (currentSet != null && !currentSet.IsFinished)
       {
           await matchService.FinishSetAsync(TournamentEvent.MatchId, match.CurrentSetNumber, TournamentEvent.UserName);
       }
   }

   // Start next set
   await matchService.StartNextSetAsync(TournamentEvent.MatchId, TournamentEvent.Source, TournamentEvent.SetNumber.Value);
   _logger.LogInformation("Processed MatchSetStart for match {MatchId}, set {SetNumber}", TournamentEvent.MatchId, TournamentEvent.SetNumber);
}

private async Task HandleMatchSetEndAsync(TournamentEvent TournamentEvent, IMatchService matchService, ISignalRNotificationService notificationService, IAnnouncementService announcerService)
{
   if (!TournamentEvent.SetNumber.HasValue)
   {
       _logger.LogWarning("SetNumber is required for MatchSetEnd event");
       return;
   }
   var match = await matchService.GetMatchAsync(TournamentEvent.MatchId);
   if (match == null)
   {
       _logger.LogWarning("Match not found for MatchSetStart: {MatchId}", TournamentEvent.MatchId);
       return;
   }

   var matchSet = await matchService.FinishSetAsync(TournamentEvent.MatchId, TournamentEvent.SetNumber.Value, TournamentEvent.Source);
   //add announcer
   string title = $"Match #{match.MatchNumber} Set {matchSet.SetNumber} Finished ";
   if (await announcerService.TitleExistsAsync(title) == false)
   {
       string winner;
       if (matchSet.AwayTeamScore > matchSet.HomeTeamScore)
       {
           winner = match.AwayTeam?.Name;
       }
       else
       {
           winner = match.HomeTeam?.Name;
       }
       var announcement = new Announcement
       {
           Title = title,
           Content = $"Match #{match.MatchNumber} between Teams {match.HomeTeam?.Name} and {match.AwayTeam?.Name} finished set {matchSet.SetNumber} with a score of {matchSet.HomeTeamScore}:{matchSet.AwayTeamScore} in favour of {winner}.",
           IsHidden = false,
           CreatedBy = TournamentEvent.UserName
       };
       await announcerService.CreateAnnouncementAsync(announcement);
   }
   _logger.LogInformation("Processed MatchSetEnd for match {MatchId}, set {SetNumber}", TournamentEvent.MatchId, TournamentEvent.SetNumber);
}

private async Task HandleMatchSetRevertToPreviousAsync(TournamentEvent TournamentEvent, IMatchService matchService, ISignalRNotificationService notificationService, IAnnouncementService announcerService)
{
   if (!TournamentEvent.SetNumber.HasValue)
   {
       _logger.LogWarning("SetNumber is required for MatchSetRevertToPrevious event");
       return;
   }

   await matchService.RevertToPreviousSetAsync(TournamentEvent.MatchId, TournamentEvent.Source, TournamentEvent.SetNumber.Value);
   _logger.LogInformation("Processed MatchSetRevertToPrevious for match {MatchId}, set {SetNumber}", TournamentEvent.MatchId, TournamentEvent.SetNumber);
}

private async Task HandleMatchSetScoreChangeAsync(TournamentEvent TournamentEvent, IMatchService matchService, ISignalRNotificationService notificationService, IAnnouncementService announcerService)
{
   if (!TournamentEvent.SetNumber.HasValue || !TournamentEvent.TeamId.HasValue || !TournamentEvent.ScoreChange.HasValue)
   {
       _logger.LogWarning("SetNumber, TeamId, and ScoreChange are required for MatchSetScoreChange event");
       return;
   }

   var match = await matchService.GetMatchAsync(TournamentEvent.MatchId);
   if (match == null)
   {
       _logger.LogWarning("Match not found for MatchSetScoreChange: {MatchId}", TournamentEvent.MatchId);
       return;
   }

   var currentSet = await matchService.GetOrCreateMatchSetAsync(
       TournamentEvent.MatchId, 
       TournamentEvent.SetNumber.Value, 
       TournamentEvent.Source);

   if (currentSet == null || currentSet.SetNumber != TournamentEvent.SetNumber.Value)
   {
       _logger.LogWarning("Set not found or not current for MatchSetScoreChange: {MatchId}, set {SetNumber}", 
           TournamentEvent.MatchId, TournamentEvent.SetNumber.Value);
       return;
   }

   if (currentSet.IsLocked)
   {
       //notifiy error
       _logger.LogWarning("Cannot update score - set is locked: {MatchId}, set {SetNumber}", 
           TournamentEvent.MatchId, TournamentEvent.SetNumber.Value);
       return;
   }

   int newHomeScore = currentSet.HomeTeamScore;
   int newAwayScore = currentSet.AwayTeamScore;

   if (TournamentEvent.TeamId.Value == match.HomeTeamId)
   {
       newHomeScore += TournamentEvent.ScoreChange.Value;
       if (newHomeScore < 0) newHomeScore = 0;
   }
   else if (TournamentEvent.TeamId.Value == match.AwayTeamId)
   {
       newAwayScore += TournamentEvent.ScoreChange.Value;
       if (newAwayScore < 0) newAwayScore = 0;
   }

   await matchService.UpdateSetScoreAsync(
       TournamentEvent.MatchId, 
       TournamentEvent.SetNumber.Value, 
       newHomeScore, 
       newAwayScore, 
       TournamentEvent.Source);

   _logger.LogInformation(
       "Processed MatchSetScoreChange for match {MatchId}, set {SetNumber}: {HomeScore}-{AwayScore}", 
       TournamentEvent.MatchId, 
       TournamentEvent.SetNumber, 
       newHomeScore, 
       newAwayScore);
}

private async Task HandleMatchEndAsync(TournamentEvent TournamentEvent, IMatchService matchService, ISignalRNotificationService notificationService, IAnnouncementService announcerService)
{
   var match = await matchService.EndMatchAndLockSetsAsync(TournamentEvent.MatchId, TournamentEvent.Source);


   var update = new MatchUpdate
   {
       MatchId = match.Id,
       CreatedBy = TournamentEvent.UserName,
       UpdateType = UpdateType.MatchFinished,
       Content = $"Match ended. Final score: {match.HomeTeamScore}-{match.AwayTeamScore} sets"
   };
   await matchService.AddMatchUpdateAsync(update);
   string title = $"Match Finished #{match.MatchNumber}";
   if (await announcerService.TitleExistsAsync(title) == false)
   {
       // Get all sets for the match to include individual set scores
       var sets = await matchService.GetMatchSetsAsync(TournamentEvent.MatchId);
       var setScores = string.Join(", ", sets.Select(s => $"{s.HomeTeamScore}-{s.AwayTeamScore}"));
       
       var announcement = new Announcement
       {
           Title = title,
           Content = $"Match #{match.MatchNumber} between Teams {match.HomeTeam?.Name} and {match.AwayTeam?.Name} has ended. Final score: {match.HomeTeamScore}-{match.AwayTeamScore} sets ({setScores}).",
           IsHidden = false,
           CreatedBy = TournamentEvent.UserName
       };
       await announcerService.CreateAnnouncementAsync(announcement);
   }
   await notificationService.NotifyMatchFinishedAsync(match);
   _logger.LogInformation("Processed MatchEnd for match {MatchId}", TournamentEvent.MatchId);
}

private async Task HandleMatchDisputedAsync(TournamentEvent TournamentEvent, IMatchService matchService, ISignalRNotificationService notificationService, IAnnouncementService announcerService)
{
   var match = await matchService.GetMatchAsync(TournamentEvent.MatchId);
   if (match == null)
   {
       _logger.LogWarning("Match not found for MatchDisputed: {MatchId}", TournamentEvent.MatchId);
       return;
   }

   bool isDisputed = false;
   if (TournamentEvent.AdditionalData?.ContainsKey("IsDisputed") == true)
   {
       isDisputed = (bool)TournamentEvent.AdditionalData["IsDisputed"];
   }

   match.IsDisputed = isDisputed;
   await matchService.UpdateMatchAsync(match);

   var update = new MatchUpdate
   {
       MatchId = TournamentEvent.MatchId,
       UpdateType = UpdateType.DisputeRaised,
       Content = isDisputed ? "Match marked as disputed" : "Dispute cleared"
   };
   await matchService.AddMatchUpdateAsync(update);
   await notificationService.NotifyMatchUpdatedAsync(match);
   _logger.LogInformation("Processed MatchDisputed for match {MatchId}, disputed: {IsDisputed}", 
       TournamentEvent.MatchId, isDisputed);
}
*/
}
