using Microsoft.Extensions.Logging;
using VolleyballRallyManager.Lib.Models;

namespace VolleyballRallyManager.Lib.Services;

public class TeamGenerationService : ITeamGenerationService
{
    private readonly ITeamService _teamService;
    private readonly ILogger<TeamGenerationService> _logger;

    private static readonly string[] TeamAdjectives = new[]
    {
        "Lightning", "Thunder", "Storm", "Phoenix", "Dragon", "Tiger", "Eagle",
        "Hawk", "Wolf", "Lion", "Panther", "Falcon", "Warrior", "Knight",
        "Champion", "Victor", "Titan", "Spartan", "Gladiator", "Ranger",
        "Viper", "Cobra", "Python", "Shark", "Barracuda", "Mustang", "Stallion",
        "Maverick", "Patriot", "Rebel", "Raider", "Crusader", "Pioneer", "Voyager"
    };

    private static readonly string[] TeamNouns = new[]
    {
        "Strikers", "Spikers", "Smashers", "Blasters", "Aces", "Stars", "Comets",
        "Meteors", "Hurricanes", "Cyclones", "Tornadoes", "Blazers", "Flames",
        "Inferno", "Rockets", "Jets", "Bolts", "Waves", "Tides", "Surfers",
        "Chargers", "Attackers", "Defenders", "Warriors", "Fighters", "Slammers",
        "Dominators", "Crushers", "Blockers", "Setters", "Diggers", "Servers"
    };

    private static readonly string[] Schools = new[]
    {
        "St. Andrew High School", "St. Catherine High School", "St. George's College",
        "Kingston College", "Jamaica College", "Wolmer's Boys' School", "Wolmer's Girls' School",
        "Campion College", "Immaculate Conception High School", "St. Hugh's High School",
        "Holy Childhood High School", "Ardenne High School", "Calabar High School",
        "Excelsior High School", "St. Mary High School", "Clarendon College",
        "Glenmuir High School", "Cornwall College", "Herbert Morrison Technical",
        "Manchester High School", "Munro College", "Vere Technical High School",
        "St. Elizabeth Technical High School", "Bustamante High School", "Rusea's High School",
        "William Knibb Memorial High School", "Hampton School", "Denbigh High School",
        "Central High School", "Spanish Town High School", "Penwood High School",
        "Vauxhall High School", "Meadowbrook High School", "Dunoon Technical High School"
    };

    private static readonly string[] Colors = new[]
    {
        "#1E3A8A", "#DC2626", "#059669", "#D97706", "#7C3AED", "#DB2777",
        "#0891B2", "#EA580C", "#65A30D", "#0D9488", "#4F46E5", "#BE123C",
        "#0284C7", "#C2410C", "#16A34A", "#9333EA", "#BE185D", "#0E7490"
    };

    public TeamGenerationService(
        ITeamService teamService,
        ILogger<TeamGenerationService> logger)
    {
        _teamService = teamService;
        _logger = logger;
    }

    public async Task<IEnumerable<Team>> GenerateRandomTeamsAsync(int count)
    {
        if (count <= 0 || count > 50)
        {
            throw new ArgumentException("Count must be between 1 and 50", nameof(count));
        }

        var random = new Random();
        var generatedTeams = new List<Team>();
        var usedNames = new HashSet<string>();

        _logger.LogInformation("Starting generation of {Count} random teams", count);

        for (int i = 0; i < count; i++)
        {
            string teamName;
            int attempts = 0;
            const int maxAttempts = 100;

            // Generate unique team name
            do
            {
                var adjective = TeamAdjectives[random.Next(TeamAdjectives.Length)];
                var noun = TeamNouns[random.Next(TeamNouns.Length)];
                teamName = $"{adjective} {noun}";
                attempts++;

                if (attempts >= maxAttempts)
                {
                    // If we can't find a unique name, append a number
                    teamName = $"{teamName} {i + 1}";
                    break;
                }
            } while (usedNames.Contains(teamName));

            usedNames.Add(teamName);

            var team = new Team
            {
                Name = teamName,
                School = Schools[random.Next(Schools.Length)],
                Color = Colors[random.Next(Colors.Length)]
            };

            try
            {
                var createdTeam = await _teamService.CreateTeamAsync(team);
                generatedTeams.Add(createdTeam);
                _logger.LogInformation("Created team: {TeamName} from {School}", team.Name, team.School);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create team: {TeamName}", team.Name);
                throw;
            }
        }

        _logger.LogInformation("Successfully generated {Count} teams", generatedTeams.Count);
        return generatedTeams;
    }
}
