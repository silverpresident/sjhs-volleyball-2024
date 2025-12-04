using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using VolleyballRallyManager.Lib.Data;
using VolleyballRallyManager.Lib.Models;

namespace VolleyballRallyManager.Lib.Services;

/// <summary>
/// Service for managing tournament rounds lifecycle
/// </summary>
public class TournamentRoundService : ITournamentRoundService
{
    private readonly ApplicationDbContext _context;
    private readonly IRanksService _ranksService;
    private readonly ILogger<TournamentRoundService> _logger;

    public TournamentRoundService(
        ApplicationDbContext context, 
        IRanksService ranksService,
        ILogger<TournamentRoundService> logger)
    {
        _context = context;
        _ranksService = ranksService;
        _logger = logger;
    }

    public async Task<List<TournamentRound>> GetTournamentRoundsAsync(Guid tournamentId, Guid? divisionId)
    {
        try
        {
            var qry = _context.TournamentRounds
                .Include(tr => tr.Tournament)
                .Include(tr => tr.Division)
                .Include(tr => tr.Round)
                .Include(tr => tr.TournamentRoundTeams)
                    .ThenInclude(trt => trt.Team)
                .Where(tr => tr.TournamentId == tournamentId);
            if (divisionId.HasValue)
            {
                qry = qry.Where(tr => tr.DivisionId == divisionId.Value);
            }
            return await qry
                .OrderBy(tr => tr.RoundNumber)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting tournament rounds for tournament {TournamentId}, division {DivisionId}", 
                tournamentId, divisionId);
            throw;
        }
    }

    public async Task<TournamentRound?> GetTournamentRoundByIdAsync(Guid tournamentRoundId)
    {
        try
        {
            return await _context.TournamentRounds
                .Include(tr => tr.Tournament)
                .Include(tr => tr.Division)
                .Include(tr => tr.Round)
                .Include(tr => tr.TournamentRoundTeams)
                    .ThenInclude(trt => trt.Team)
                .FirstOrDefaultAsync(tr => tr.Id == tournamentRoundId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting tournament round {TournamentRoundId}", tournamentRoundId);
            throw;
        }
    }

    public async Task<TournamentRound> CreateFirstRoundAsync(
        Guid tournamentId,
        Guid divisionId,
        Guid roundId,
        TeamSelectionMethod teamSelectionMethod,
        MatchGenerationStrategy matchGenerationStrategy,
        int TeamsAdvancing,
        string userName)
    {
        try
        {
            _logger.LogInformation("Creating first round for tournament {TournamentId}, division {DivisionId}", 
                tournamentId, divisionId);

            var tournamentRound = new TournamentRound
            {
                TournamentId = tournamentId,
                DivisionId = divisionId,
                RoundId = roundId,
                RoundNumber = 1,
                TeamSelectionMethod = teamSelectionMethod,
                MatchGenerationStrategy = matchGenerationStrategy,
                PreviousTournamentRoundId = null,
                TeamsAdvancing = TeamsAdvancing,
                IsFinished = false,
                IsLocked = false,
                CreatedBy = userName,
                UpdatedBy = userName
            };

            _context.TournamentRounds.Add(tournamentRound);
            await _context.SaveChangesAsync();


            _logger.LogInformation("Created first round with {TeamCount} teams", divisionTeams.Count);

            return tournamentRound;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating first round");
            throw;
        }
    }

    public async Task<TournamentRound> CreateNextRoundAsync(
        Guid tournamentId,
        Guid divisionId,
        Guid roundId,
        Guid previousTournamentRoundId,
        TeamSelectionMethod teamSelectionMethod,
        MatchGenerationStrategy matchGenerationStrategy,
        int TeamsAdvancing,
        string userName)
    {
        try
        {
            _logger.LogInformation("Creating next round for tournament {TournamentId}, division {DivisionId}", 
                tournamentId, divisionId);

            var previousRound = await GetTournamentRoundByIdAsync(previousTournamentRoundId);
            if (previousRound == null)
            {
                throw new InvalidOperationException($"Previous round {previousTournamentRoundId} not found");
            }

            if (!previousRound.IsFinished)
            {
                throw new InvalidOperationException("Previous round must be finished before creating next round");
            }

            var tournamentRound = new TournamentRound
            {
                TournamentId = tournamentId,
                DivisionId = divisionId,
                RoundId = roundId,
                RoundNumber = previousRound.RoundNumber + 1,
                TeamSelectionMethod = teamSelectionMethod,
                MatchGenerationStrategy = matchGenerationStrategy,
                PreviousTournamentRoundId = previousTournamentRoundId,
                TeamsAdvancing = TeamsAdvancing,
                IsFinished = false,
                IsLocked = false,
                CreatedBy = userName,
                UpdatedBy = userName
            };

            _context.TournamentRounds.Add(tournamentRound);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Created next round with round number {RoundNumber}", tournamentRound.RoundNumber);

            return tournamentRound;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating next round");
            throw;
        }
    }
    public async Task<TournamentRound> AssignFirstRoundTeamsAsync(Guid tournamentRoundId, string userName)
    {
        try
        {
            _logger.LogInformation("Assigning teams to groups for round {TournamentRoundId}", tournamentRoundId);

            var tournamentRound = await GetTournamentRoundByIdAsync(tournamentRoundId);
            if (tournamentRound == null)
            {
                throw new InvalidOperationException($"Tournament round {tournamentRoundId} not found");
            }

            // Ensure this is a first round
            if (tournamentRound.RoundNumber != 1)
            {
                throw new InvalidOperationException("This method can only be used for first round team assignment");
            }

            if (tournamentRound.IsFinished || tournamentRound.IsLocked)
            {
                throw new InvalidOperationException("Cannot assign teams to a finished or locked round");
            }

            // Check if matches have already been generated
            var hasMatches = await HasMatchesGeneratedAsync(tournamentRoundId);
            if (hasMatches)
            {
                throw new InvalidOperationException("Cannot reassign teams to groups after matches have been generated. Delete matches first if you need to reassign groups.");
            }

            // Get all teams for this round
            var roundTeams = await GetRoundTeamsAsync(tournamentRoundId);
            if (!roundTeams.Any())
            {
                throw new InvalidOperationException("No teams found for this round");
            }
            
            //Set up first found teams

            // Get all teams from the division for initial seeding
            var divisionTeams = await _context.TournamentTeamDivisions
                .Include(ttd => ttd.Team)
                .Where(ttd => ttd.TournamentId == tournamentRound.TournamentId && ttd.DivisionId == tournamentRound.DivisionId)
                .OrderBy(ttd => ttd.SeedNumber)
                .ToListAsync();

            // Create TournamentRoundTeam entries with initial seeding
            foreach (var divisionTeam in divisionTeams)
            {
                if (roundTeams.Any(rt => rt.TeamId == divisionTeam.TeamId)){
                    continue;
                }
                var roundTeam = new TournamentRoundTeam
                {
                    TournamentId = tournamentRound.TournamentId,
                    DivisionId = tournamentRound.DivisionId,
                    RoundId = tournamentRound.RoundId,
                    TeamId = divisionTeam.TeamId,
                    TournamentRoundId = tournamentRound.Id,
                    SeedNumber = divisionTeam.SeedNumber,
                    GroupName = divisionTeam.GroupName,
                    CreatedBy = userName,
                    CreatedAt = DateTime.Now,
                    UpdatedBy = userName
                };

                _context.TournamentRoundTeams.Add(roundTeam);
                roundTeams.Add(roundTeam);
            }

            await _context.SaveChangesAsync();

            // Determine group assignment strategy
            if (tournamentRound.TeamsPerGroup.HasValue && tournamentRound.TeamsPerGroup.Value > 0)
            {
                // Assign based on TeamsPerGroup
                await AssignTeamsToGroupsByTeamsPerGroupAsync(roundTeams, tournamentRound.TeamsPerGroup.Value, userName);
            }
            else if (tournamentRound.GroupsInRound.HasValue && tournamentRound.GroupsInRound.Value > 0)
            {
                // Assign based on GroupsInRound
                await AssignTeamsToGroupsByGroupCountAsync(roundTeams, tournamentRound.GroupsInRound.Value, userName);
            }
            else
            {
                // No group configuration specified - assign all teams to a single default group
                _logger.LogInformation("No group configuration specified. Assigning all teams to default group.");
                foreach (var team in roundTeams)
                {
                    team.GroupName = string.Empty;
                    team.UpdatedBy = userName;
                    team.UpdatedAt = DateTime.Now;
                }
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation("Successfully assigned {TeamCount} teams to groups for round {TournamentRoundId}", 
                roundTeams.Count, tournamentRoundId);

            return tournamentRound;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assigning teams to groups for round {TournamentRoundId}", tournamentRoundId);
            throw;
        }
    }

    private async Task AssignTeamsToGroupsByTeamsPerGroupAsync(
        List<TournamentRoundTeam> roundTeams,
        int teamsPerGroup,
        string userName)
    {
        _logger.LogInformation("Assigning {TeamCount} teams to groups with {TeamsPerGroup} teams per group",
            roundTeams.Count, teamsPerGroup);

        // Order teams by seed number for consistent assignment
        var orderedTeams = roundTeams.OrderBy(t => t.SeedNumber).ToList();
        
        // Calculate number of groups needed
        int groupCount = (int)Math.Ceiling((double)orderedTeams.Count / teamsPerGroup);
        
        // Distribute teams round-robin style: top seeds distributed across groups first
        for (int teamIndex = 0; teamIndex < orderedTeams.Count; teamIndex++)
        {
            // Determine which group this team should be assigned to
            // Team 0 -> Group 0, Team 1 -> Group 1, ..., Team groupCount -> Group 0, etc.
            int groupIndex = teamIndex % groupCount;
            string groupName = $"{(char)('A' + groupIndex)}";
            
            orderedTeams[teamIndex].GroupName = groupName;
            orderedTeams[teamIndex].UpdatedBy = userName;
            orderedTeams[teamIndex].UpdatedAt = DateTime.Now;
        }

        _logger.LogInformation("Distributed teams across {GroupCount} groups with round-robin seeding", groupCount);

        await Task.CompletedTask;
    }

    private async Task AssignTeamsToGroupsByGroupCountAsync(
        List<TournamentRoundTeam> roundTeams,
        int groupCount,
        string userName)
    {
        _logger.LogInformation("Assigning {TeamCount} teams to {GroupCount} groups",
            roundTeams.Count, groupCount);

        // Order teams by seed number for consistent assignment
        var orderedTeams = roundTeams.OrderBy(t => t.SeedNumber).ToList();
        
        // Distribute teams round-robin style: top seeds distributed across groups first
        // Example with 12 teams and 3 groups:
        // Group A: Seeds 1, 4, 7, 10
        // Group B: Seeds 2, 5, 8, 11
        // Group C: Seeds 3, 6, 9, 12
        for (int teamIndex = 0; teamIndex < orderedTeams.Count; teamIndex++)
        {
            // Determine which group this team should be assigned to
            // Team 0 -> Group 0, Team 1 -> Group 1, ..., Team groupCount -> Group 0, etc.
            int groupIndex = teamIndex % groupCount;
            string groupName = $"{(char)('A' + groupIndex)}";
            
            orderedTeams[teamIndex].GroupName = groupName;
            orderedTeams[teamIndex].UpdatedBy = userName;
            orderedTeams[teamIndex].UpdatedAt = DateTime.Now;
        }

        _logger.LogInformation("Distributed teams across {GroupCount} groups with round-robin seeding", groupCount);

        await Task.CompletedTask;
    }
    public async Task<List<TournamentRoundTeam>> SelectTeamsForRoundAsync(Guid tournamentRoundId, string userName)
    {
        try
        {
            _logger.LogInformation("Selecting teams for round {TournamentRoundId}", tournamentRoundId);

            var tournamentRound = await GetTournamentRoundByIdAsync(tournamentRoundId);
            if (tournamentRound == null)
            {
                throw new InvalidOperationException($"Tournament round {tournamentRoundId} not found");
            }

            if (!tournamentRound.PreviousTournamentRoundId.HasValue)
            {
                throw new InvalidOperationException("Cannot select teams for first round - teams are auto-assigned");
            }

            var previousRound = await GetTournamentRoundByIdAsync(tournamentRound.PreviousTournamentRoundId.Value);
            if (previousRound == null || !previousRound.IsFinished)
            {
                throw new InvalidOperationException("Previous round must be finished before selecting teams");
            }

            // Get previous round teams with rankings
            var previousRoundTeams = await _ranksService.GetStandingsAsync(previousRound.Id);

            List<TournamentRoundTeam> qualifyingTeams = new List<TournamentRoundTeam>();

            // Apply selection logic based on method
            switch (tournamentRound.TeamSelectionMethod)
            {
                case TeamSelectionMethod.WinnersOnly:
                    qualifyingTeams = previousRoundTeams
                        .Where(t => t.FinalRank == 1)
                        .Take(tournamentRound.TeamsAdvancing)
                        .ToList();
                    break;

                case TeamSelectionMethod.SeedTopHalf:
                    int halfCount = previousRoundTeams.Count / 2;
                    int teamsToSelect = Math.Min(halfCount, tournamentRound.TeamsAdvancing);
                    qualifyingTeams = previousRoundTeams
                        .Take(teamsToSelect)
                        .ToList();
                    break;

                case TeamSelectionMethod.TopByPoints:
                    qualifyingTeams = previousRoundTeams
                        .Take(tournamentRound.TeamsAdvancing)
                        .ToList();
                    break;

                case TeamSelectionMethod.TopFromGroupAndNextBest:
                    // Get top team from each group
                    var groupWinners = previousRoundTeams
                        .GroupBy(t => t.GroupName)
                        .Select(g => g.OrderBy(t => t.FinalRank).First())
                        .ToList();

                    // Get next best teams overall
                    var remainingTeams = previousRoundTeams
                        .Except(groupWinners)
                        .Take(tournamentRound.TeamsAdvancing - groupWinners.Count)
                        .ToList();

                    qualifyingTeams = groupWinners.Concat(remainingTeams).ToList();
                    break;

                case TeamSelectionMethod.Manual:
                    throw new InvalidOperationException("Manual team selection must be done through UI");

                default:
                    throw new InvalidOperationException($"Unknown team selection method: {tournamentRound.TeamSelectionMethod}");
            }

            // Create TournamentRoundTeam entries for qualifying teams
            int seedNumber = 1;
            foreach (var qualifyingTeam in qualifyingTeams.OrderBy(t => t.FinalRank))
            {
                var roundTeam = new TournamentRoundTeam
                {
                    TournamentId = tournamentRound.TournamentId,
                    DivisionId = tournamentRound.DivisionId,
                    RoundId = tournamentRound.RoundId,
                    TeamId = qualifyingTeam.TeamId,
                    TournamentRoundId = tournamentRound.Id,
                    SeedNumber = seedNumber++,
                    GroupName = qualifyingTeam.GroupName,
                    CreatedBy = userName,
                    UpdatedBy = userName
                };

                _context.TournamentRoundTeams.Add(roundTeam);
            }

            // Lock the previous round
            previousRound.IsLocked = true;
            previousRound.UpdatedBy = userName;
            previousRound.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Selected {TeamCount} teams for round {TournamentRoundId}", 
                qualifyingTeams.Count, tournamentRoundId);

            return await GetRoundTeamsAsync(tournamentRoundId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error selecting teams for round {TournamentRoundId}", tournamentRoundId);
            throw;
        }
    }

    public async Task<List<Match>> GenerateMatchesForRoundAsync(
        Guid tournamentRoundId,
        DateTime startTime,
        string courtLocation,
        string userName)
    {
        try
        {
            _logger.LogInformation("Generating matches for round {TournamentRoundId}", tournamentRoundId);

            var tournamentRound = await GetTournamentRoundByIdAsync(tournamentRoundId);
            if (tournamentRound == null)
            {
                throw new InvalidOperationException($"Tournament round {tournamentRoundId} not found");
            }

            var roundTeams = await GetRoundTeamsAsync(tournamentRoundId);
            if (roundTeams.Count < 2)
            {
                throw new InvalidOperationException("Need at least 2 teams to generate matches");
            }

            // Get next match number
            var lastMatchNumber = await _context.Matches
                .Where(m => m.TournamentId == tournamentRound.TournamentId)
                .MaxAsync(m => (int?)m.MatchNumber) ?? 0;

            int matchNumber = lastMatchNumber + 1;
            var matches = new List<Match>();
            DateTime currentTime = startTime;

            switch (tournamentRound.MatchGenerationStrategy)
            {
                case MatchGenerationStrategy.RoundRobin:
                    matches = GenerateRoundRobinMatches(tournamentRound, roundTeams, ref matchNumber, ref currentTime, courtLocation, userName);
                    break;

                case MatchGenerationStrategy.SeededBracket:
                    matches = GenerateSeededBracketMatches(tournamentRound, roundTeams, ref matchNumber, ref currentTime, courtLocation, userName);
                    break;

                case MatchGenerationStrategy.Manual:
                    throw new InvalidOperationException("Manual match generation must be done through UI");

                default:
                    throw new InvalidOperationException($"Match generation strategy {tournamentRound.MatchGenerationStrategy} not implemented");
            }

            _context.Matches.AddRange(matches);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Generated {MatchCount} matches for round {TournamentRoundId}", 
                matches.Count, tournamentRoundId);

            return matches;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating matches for round {TournamentRoundId}", tournamentRoundId);
            throw;
        }
    }

    private List<Match> GenerateRoundRobinMatches(
        TournamentRound tournamentRound,
        List<TournamentRoundTeam> teams,
        ref int matchNumber,
        ref DateTime currentTime,
        string courtLocation,
        string userName)
    {
        var matches = new List<Match>();
        var orderedTeams = teams.OrderBy(t => t.SeedNumber).ToList();

        for (int i = 0; i < orderedTeams.Count; i++)
        {
            for (int j = i + 1; j < orderedTeams.Count; j++)
            {
                var match = new Match
                {
                    TournamentId = tournamentRound.TournamentId,
                    DivisionId = tournamentRound.DivisionId,
                    RoundId = tournamentRound.RoundId,
                    MatchNumber = matchNumber++,
                    HomeTeamId = orderedTeams[i].TeamId,
                    AwayTeamId = orderedTeams[j].TeamId,
                    ScheduledTime = currentTime,
                    CourtLocation = courtLocation,
                    GroupName = orderedTeams[i].GroupName,
                    CreatedBy = userName,
                    UpdatedBy = userName
                };

                matches.Add(match);
                currentTime = currentTime.AddMinutes(15); // 15 minutes between matches
            }
        }

        return matches;
    }

    private List<Match> GenerateSeededBracketMatches(
        TournamentRound tournamentRound,
        List<TournamentRoundTeam> teams,
        ref int matchNumber,
        ref DateTime currentTime,
        string courtLocation,
        string userName)
    {
        var matches = new List<Match>();
        var orderedTeams = teams.OrderBy(t => t.SeedNumber).ToList();

        // Standard tournament seeding: 1 vs Last, 2 vs Second-to-last, etc.
        int teamCount = orderedTeams.Count;
        for (int i = 0; i < teamCount / 2; i++)
        {
            var match = new Match
            {
                TournamentId = tournamentRound.TournamentId,
                DivisionId = tournamentRound.DivisionId,
                RoundId = tournamentRound.RoundId,
                MatchNumber = matchNumber++,
                HomeTeamId = orderedTeams[i].TeamId,
                AwayTeamId = orderedTeams[teamCount - 1 - i].TeamId,
                ScheduledTime = currentTime,
                CourtLocation = courtLocation,
                GroupName = string.Empty,
                CreatedBy = userName,
                UpdatedBy = userName
            };

            matches.Add(match);
            currentTime = currentTime.AddMinutes(15);
        }

        return matches;
    }

    public async Task<TournamentRound> FinalizeRoundAsync(Guid tournamentRoundId, string userName)
    {
        try
        {
            _logger.LogInformation("Finalizing round {TournamentRoundId}", tournamentRoundId);

            var tournamentRound = await GetTournamentRoundByIdAsync(tournamentRoundId);
            if (tournamentRound == null)
            {
                throw new InvalidOperationException($"Tournament round {tournamentRoundId} not found");
            }

            if (tournamentRound.IsFinished)
            {
                throw new InvalidOperationException("Round is already finalized");
            }

            // Verify all matches are complete
            if (!await AreAllMatchesCompleteAsync(tournamentRoundId))
            {
                throw new InvalidOperationException("All matches must be complete before finalizing round");
            }

            // Update team rankings
            await _ranksService.UpdateTeamRanksAsync(tournamentRoundId);

            // Mark round as finished and locked
            tournamentRound.IsFinished = true;
            tournamentRound.IsLocked = true;
            tournamentRound.UpdatedBy = userName;
            tournamentRound.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Finalized round {TournamentRoundId}", tournamentRoundId);

            return tournamentRound;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error finalizing round {TournamentRoundId}", tournamentRoundId);
            throw;
        }
    }

    public async Task<bool> AreAllMatchesCompleteAsync(Guid tournamentRoundId)
    {
        try
        {
            var tournamentRound = await GetTournamentRoundByIdAsync(tournamentRoundId);
            if (tournamentRound == null)
            {
                return false;
            }

            var matches = await GetRoundMatchesAsync(tournamentRoundId);
            return matches.Any() && matches.All(m => m.IsFinished);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if all matches are complete for round {TournamentRoundId}", tournamentRoundId);
            throw;
        }
    }

    public async Task<bool> HasTeamsAssignedAsync(Guid tournamentRoundId)
    {
        try
        {
            var teams = await GetRoundTeamsAsync(tournamentRoundId);
            return teams.Any();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if round has teams assigned {TournamentRoundId}", tournamentRoundId);
            throw;
        }
    }

    public async Task<bool> HasMatchesGeneratedAsync(Guid tournamentRoundId)
    {
        try
        { 
            var tournamentRound = await GetTournamentRoundByIdAsync(tournamentRoundId);
            if (tournamentRound == null)
            {
                return false;
            }

            return await _context.Matches
                .Where(m => m.TournamentId == tournamentRound.TournamentId
                    && m.DivisionId == tournamentRound.DivisionId
                    && m.RoundId == tournamentRound.RoundId)
                .AnyAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if round has matches generated {TournamentRoundId}", tournamentRoundId);
            throw;
        }
    }

    public async Task<List<TournamentRoundTeam>> GetRoundTeamsAsync(Guid tournamentRoundId)
    {
        try
        {
            return await _context.TournamentRoundTeams
                .Include(trt => trt.Team)
                .Where(trt => trt.TournamentRoundId == tournamentRoundId)
                .OrderBy(trt => trt.SeedNumber)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting teams for round {TournamentRoundId}", tournamentRoundId);
            throw;
        }
    }

    public async Task<List<Match>> GetRoundMatchesAsync(Guid tournamentRoundId)
    {
        try
        {
            var tournamentRound = await GetTournamentRoundByIdAsync(tournamentRoundId);
            if (tournamentRound == null)
            {
                return new List<Match>();
            }

            return await _context.Matches
                .Include(m => m.HomeTeam)
                .Include(m => m.AwayTeam)
                .Include(m => m.Sets)
                .Where(m => m.TournamentId == tournamentRound.TournamentId
                    && m.DivisionId == tournamentRound.DivisionId
                    && m.RoundId == tournamentRound.RoundId)
                .OrderBy(m => m.MatchNumber)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting matches for round {TournamentRoundId}", tournamentRoundId);
            throw;
        }
    }
}
