using System.Net.Http.Json;
using System.Text.Json;
using VolleyballRallyManager.Lib.Models;

namespace VolleyballRallyManager.Public.Services;

public class ApiService : IApiService
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonOptions;

    public ApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }

    public async Task<List<Match>> GetMatchesAsync()
    {
        try
        {
            var response = await _httpClient.GetFromJsonAsync<List<Match>>("api/matches", _jsonOptions);
            return response ?? new List<Match>();
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error fetching matches: {ex.Message}");
            return new List<Match>();
        }
    }

    public async Task<List<Team>> GetTeamsAsync()
    {
        try
        {
            var response = await _httpClient.GetFromJsonAsync<List<Team>>("api/teams", _jsonOptions);
            return response ?? new List<Team>();
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error fetching teams: {ex.Message}");
            return new List<Team>();
        }
    }

    public async Task<List<Announcement>> GetAnnouncementsAsync()
    {
        try
        {
            var response = await _httpClient.GetFromJsonAsync<List<Announcement>>("api/announcements", _jsonOptions);
            return response ?? new List<Announcement>();
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error fetching announcements: {ex.Message}");
            return new List<Announcement>();
        }
    }

    public async Task<List<MatchUpdate>> GetMatchUpdatesAsync()
    {
        try
        {
            var response = await _httpClient.GetFromJsonAsync<List<MatchUpdate>>("api/matchupdates", _jsonOptions);
            return response ?? new List<MatchUpdate>();
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error fetching match updates: {ex.Message}");
            return new List<MatchUpdate>();
        }
    }

    public async Task<List<Division>> GetDivisionsAsync()
    {
        try
        {
            var response = await _httpClient.GetFromJsonAsync<List<Division>>("api/divisions", _jsonOptions);
            return response ?? new List<Division>();
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error fetching divisions: {ex.Message}");
            return new List<Division>();
        }
    }
}
