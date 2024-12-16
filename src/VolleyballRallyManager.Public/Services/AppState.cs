 using VolleyballRallyManager.Lib.Models;

namespace VolleyballRallyManager.Public.Services;

public class AppState
{
    private readonly ISignalRService _signalRService;
    private readonly IApiService _apiService;

    public List<Match> Matches { get; private set; } = new();
    public List<Team> Teams { get; private set; } = new();
    public List<Announcement> Announcements { get; private set; } = new();
    public List<MatchUpdate> MatchUpdates { get; private set; } = new();
    public List<Division> Divisions { get; private set; } = new();

    public event Action? OnChange;

    public AppState(ISignalRService signalRService, IApiService apiService)
    {
        _signalRService = signalRService;
        _apiService = apiService;

        // Register SignalR event handlers
        _signalRService.OnMatchUpdated += HandleMatchUpdated;
        _signalRService.OnTeamUpdated += HandleTeamUpdated;
        _signalRService.OnAnnouncementAdded += HandleAnnouncementAdded;
        _signalRService.OnAnnouncementUpdated += HandleAnnouncementUpdated;
        _signalRService.OnMatchUpdateAdded += HandleMatchUpdateAdded;
    }

    public async Task InitializeAsync()
    {
        await LoadDataAsync();
        await _signalRService.StartAsync();
    }

    public async Task LoadDataAsync()
    {
        var matchesTask = _apiService.GetMatchesAsync();
        var teamsTask = _apiService.GetTeamsAsync();
        var announcementsTask = _apiService.GetAnnouncementsAsync();
        var matchUpdatesTask = _apiService.GetMatchUpdatesAsync();
        var divisionsTask = _apiService.GetDivisionsAsync();

        await Task.WhenAll(matchesTask, teamsTask, announcementsTask, matchUpdatesTask, divisionsTask);

        Matches = (await matchesTask).OrderBy(m => m.ScheduledTime).ToList();
        Teams = (await teamsTask).OrderBy(t => t.Name).ToList();
        Announcements = (await announcementsTask).OrderByDescending(a => a.CreatedAt).ToList();
        MatchUpdates = (await matchUpdatesTask).OrderByDescending(u => u.CreatedAt).ToList();
        Divisions = (await divisionsTask).OrderBy(d => d.Name).ToList();

        NotifyStateChanged();
    }

    private void HandleMatchUpdated(Match match)
    {
        var index = Matches.FindIndex(m => m.Id == match.Id);
        if (index >= 0)
        {
            Matches[index] = match;
        }
        else
        {
            Matches.Add(match);
        }
        NotifyStateChanged();
    }

    private void HandleTeamUpdated(Team team)
    {
        var index = Teams.FindIndex(t => t.Id == team.Id);
        if (index >= 0)
        {
            Teams[index] = team;
        }
        else
        {
            Teams.Add(team);
        }
        NotifyStateChanged();
    }

    private void HandleAnnouncementAdded(Announcement announcement)
    {
        Announcements.Insert(0, announcement);
        NotifyStateChanged();
    }

    private void HandleAnnouncementUpdated(Announcement announcement)
    {
        var index = Announcements.FindIndex(a => a.Id == announcement.Id);
        if (index >= 0)
        {
            Announcements[index] = announcement;
            NotifyStateChanged();
        }
    }

    private void HandleMatchUpdateAdded(MatchUpdate update)
    {
        MatchUpdates.Insert(0, update);
        NotifyStateChanged();
    }

    private void NotifyStateChanged() => OnChange?.Invoke();
}
