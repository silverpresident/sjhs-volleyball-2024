using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Configuration;
using VolleyballRallyManager.Lib.Models;
using Microsoft.AspNetCore.SignalR.Client;

namespace VolleyballRallyManager.Public.Services;

public interface ISignalRService
{
    event Action<Match>? OnMatchUpdated;
    event Action<Team>? OnTeamUpdated;
    event Action<Announcement>? OnAnnouncementAdded;
    event Action<Announcement>? OnAnnouncementUpdated;
    event Action<MatchUpdate>? OnMatchUpdateAdded;
    Task StartAsync();
    Task StopAsync();
    bool IsConnected { get; }
}

public class SignalRService : ISignalRService, IAsyncDisposable
{
    private readonly NavigationManager _navigationManager;
    private readonly IConfiguration _configuration;
    private HubConnection? _hubConnection;

    public event Action<Match>? OnMatchUpdated;
    public event Action<Team>? OnTeamUpdated;
    public event Action<Announcement>? OnAnnouncementAdded;
    public event Action<Announcement>? OnAnnouncementUpdated;
    public event Action<MatchUpdate>? OnMatchUpdateAdded;

    public bool IsConnected => _hubConnection?.State == HubConnectionState.Connected;

    public SignalRService(NavigationManager navigationManager, IConfiguration configuration)
    {
        _navigationManager = navigationManager;
        _configuration = configuration;
    }

    public async Task StartAsync()
    {
        if (_hubConnection is not null)
        {
            return;
        }

        var hubUrl = _configuration["AppSettings:SignalR:HubUrl"] ?? 
                    _navigationManager.ToAbsoluteUri("/matchHub").ToString();

        _hubConnection = new HubConnectionBuilder()
            .WithUrl(hubUrl)
            .WithAutomaticReconnect(new[] { 
                TimeSpan.Zero, 
                TimeSpan.FromSeconds(2), 
                TimeSpan.FromSeconds(5), 
                TimeSpan.FromSeconds(30) 
            })
            .Build();

        RegisterHandlers();

        try
        {
            await _hubConnection.StartAsync();
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error starting SignalR connection: {ex.Message}");
            throw;
        }
    }

    private void RegisterHandlers()
    {
        if (_hubConnection is null) return;

        _hubConnection.On<Match>("MatchUpdated", match =>
        {
            OnMatchUpdated?.Invoke(match);
        });

        _hubConnection.On<Team>("TeamUpdated", team =>
        {
            OnTeamUpdated?.Invoke(team);
        });

        _hubConnection.On<Announcement>("AnnouncementAdded", announcement =>
        {
            OnAnnouncementAdded?.Invoke(announcement);
        });

        _hubConnection.On<Announcement>("AnnouncementUpdated", announcement =>
        {
            OnAnnouncementUpdated?.Invoke(announcement);
        });

        _hubConnection.On<MatchUpdate>("MatchUpdateAdded", update =>
        {
            OnMatchUpdateAdded?.Invoke(update);
        });
    }

    public async Task StopAsync()
    {
        if (_hubConnection is not null)
        {
            try
            {
                await _hubConnection.StopAsync();
            }
            finally
            {
                await _hubConnection.DisposeAsync();
                _hubConnection = null;
            }
        }
    }

    public async ValueTask DisposeAsync()
    {
        await StopAsync();
    }
}
