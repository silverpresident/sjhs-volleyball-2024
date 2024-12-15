using Microsoft.AspNetCore.Components;
using VolleyballRallyManager.Public.Services;
using VolleyballRallyManager.Lib.Models;

namespace VolleyballRallyManager.Public.Components;

public abstract class StateComponentBase : ComponentBase, IDisposable
{
    [Inject]
    protected AppState AppState { get; set; } = default!;

    [Inject]
    protected ISignalRService SignalRService { get; set; } = default!;

    [Inject]
    protected IApiService ApiService { get; set; } = default!;

    protected bool IsLoading { get; private set; }
    protected string? ErrorMessage { get; private set; }

    protected override void OnInitialized()
    {
        AppState.OnChange += StateHasChanged;
    }

    protected async Task ExecuteAsync(Func<Task> action)
    {
        try
        {
            IsLoading = true;
            ErrorMessage = null;
            await action();
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            Console.Error.WriteLine($"Error in {GetType().Name}: {ex}");
        }
        finally
        {
            IsLoading = false;
            StateHasChanged();
        }
    }

    protected string GetTeamName(Guid teamId)
    {
        return AppState.Teams.FirstOrDefault(t => t.Id == teamId)?.Name ?? "Unknown Team";
    }

    protected string GetMatchDescription(Guid matchId)
    {
        var match = AppState.Matches.FirstOrDefault(m => m.Id == matchId);
        if (match == null) return "Unknown Match";

        var homeTeam = GetTeamName(match.HomeTeamId);
        var awayTeam = GetTeamName(match.AwayTeamId);
        return $"{homeTeam} vs {awayTeam}";
    }

    protected string FormatDateTime(DateTime dateTime)
    {
        return dateTime.ToString("MMM dd, yyyy HH:mm");
    }

    protected string GetMatchStatus(Match match)
    {
        if (match.IsFinished) return "Finished";
        if (match.ActualStartTime.HasValue) return "In Progress";
        return "Scheduled";
    }

    protected string GetMatchScore(Match match)
    {
        if (!match.ActualStartTime.HasValue) return "-";
        return $"{match.HomeTeamScore} - {match.AwayTeamScore}";
    }

    public virtual void Dispose()
    {
        AppState.OnChange -= StateHasChanged;
    }
}
