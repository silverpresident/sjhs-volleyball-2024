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
        int advancingTeamsCount,
        TeamSelectionStrategy advancingTeamSelectionStrategy,
        MatchGenerationStrategy matchGenerationStrategy,
        GroupGenerationStrategy groupingStrategy,
        int groupingSize,
        string userName)
    {

        var roundNumber = 1;
        if (_context.TournamentRounds.Any(tr => tr.TournamentId == tournamentId && tr.DivisionId == divisionId && tr.RoundNumber == roundNumber))
        {
            throw new InvalidOperationException($"Round {roundNumber} already exists");
        }
        try
        {
            _logger.LogInformation("Creating first round for tournament {TournamentId}, division {DivisionId}", 
                tournamentId, divisionId);

            var tournamentRound = new TournamentRound
            {
                TournamentId = tournamentId,
                DivisionId = divisionId,
                RoundId = roundId,
                RoundNumber = roundNumber,
                MatchGenerationStrategy = matchGenerationStrategy,
                PreviousTournamentRoundId = null,
                QualifyingTeamsCount = 0,
                QualifyingTeamSelectionStrategy = TeamSelectionStrategy.Manual,
                AdvancingTeamSelectionStrategy = advancingTeamSelectionStrategy,
                AdvancingTeamsCount = advancingTeamsCount,
                GroupingStrategy = groupingStrategy,
                IsFinished = false,
                IsLocked = false,
                CreatedBy = userName,
                UpdatedBy = userName,
                CreatedAt = DateTime.Now
            };
            // Save group configuration 
            tournamentRound.TeamsPerGroup = tournamentRound.GroupingStrategy == GroupGenerationStrategy.TeamsPerGroup ? groupingSize : null;
            tournamentRound.GroupsInRound = tournamentRound.GroupingStrategy == GroupGenerationStrategy.GroupsInRound ? groupingSize : null;
            if (tournamentRound.AdvancingTeamSelectionStrategy == TeamSelectionStrategy.WinnersOnly)
            {
                if (tournamentRound.AdvancingTeamsCount > 1)
                {
                    tournamentRound.AdvancingTeamSelectionStrategy = TeamSelectionStrategy.TopByPoints;
                    if (tournamentRound.GroupingStrategy != GroupGenerationStrategy.NoGroup)
                    {
                        tournamentRound.AdvancingTeamSelectionStrategy = TeamSelectionStrategy.TopFromGroupAndNextBest;
                    }
                }
            }

            _context.TournamentRounds.Add(tournamentRound);
            await _context.SaveChangesAsync();


            _logger.LogInformation("Created first round with");

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
        int qualifyingTeamsCount,
        TeamSelectionStrategy qualifyingTeamSelectionStrategy,
        int advancingTeamsCount,
        TeamSelectionStrategy advancingTeamSelectionStrategy,
        MatchGenerationStrategy matchGenerationStrategy,
        GroupGenerationStrategy groupingStrategy,
        int groupingSize,
        bool isPlayoff,
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
            var roundNumber = previousRound.RoundNumber + 1;
            if (isPlayoff == false)
            {
                if (_context.TournamentRounds.Any(tr => tr.TournamentId == tournamentId && tr.DivisionId == divisionId && tr.RoundNumber == roundNumber))
                {
                    throw new InvalidOperationException($"Round {roundNumber} already exists");
                }
            }
            /*if (_context.TournamentRounds.Any(tr => tr.TournamentId == tournamentId && tr.DivisionId == divisionId && tr.RoundNumber == roundNumber )){
                throw new InvalidOperationException($"CurrentRound {roundNumber} already exists");     
            }*/

            var tournamentRound = new TournamentRound
            {
                TournamentId = tournamentId,
                DivisionId = divisionId,
                RoundId = roundId,
                RoundNumber = roundNumber,
                QualifyingTeamsCount = qualifyingTeamsCount,
                QualifyingTeamSelectionStrategy = qualifyingTeamSelectionStrategy,
                AdvancingTeamsCount = advancingTeamsCount,
                AdvancingTeamSelectionStrategy = advancingTeamSelectionStrategy,
                MatchGenerationStrategy = matchGenerationStrategy,
                PreviousTournamentRoundId = previousTournamentRoundId,
                GroupingStrategy = groupingStrategy,
                IsPlayoff = isPlayoff,
                IsFinished = false,
                IsLocked = false,
                CreatedBy = userName,
                UpdatedBy = userName,
                CreatedAt = DateTime.Now
            };
            // Save group configuration 
            tournamentRound.TeamsPerGroup = tournamentRound.GroupingStrategy == GroupGenerationStrategy.TeamsPerGroup ? groupingSize : null;
            tournamentRound.GroupsInRound = tournamentRound.GroupingStrategy == GroupGenerationStrategy.GroupsInRound ? groupingSize : null;
  
            if (tournamentRound.AdvancingTeamSelectionStrategy == TeamSelectionStrategy.WinnersOnly)
            {
                if (tournamentRound.AdvancingTeamsCount > 1)
                {
                    tournamentRound.AdvancingTeamSelectionStrategy = TeamSelectionStrategy.TopByPoints;
                    if (tournamentRound.GroupingStrategy != GroupGenerationStrategy.NoGroup)
                    {
                        tournamentRound.AdvancingTeamSelectionStrategy = TeamSelectionStrategy.TopFromGroupAndNextBest;
                    }
                }
            }
            _context.TournamentRounds.Add(tournamentRound);
            await _context.SaveChangesAsync();

            if (isPlayoff == false)
            {
                previousRound.NextTournamentRoundId = tournamentRound.Id;
                await _context.SaveChangesAsync();
            }

            _logger.LogInformation("Created next round with round number {RoundNumber}", tournamentRound.RoundNumber);

            return tournamentRound;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating next round");
            throw;
        }
    }
    public async Task<List<TournamentRoundTeam>> AssignFirstRoundTeamsAsync(Guid tournamentRoundId, string userName)
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

            return roundTeams;
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

            if (tournamentRound.RoundNumber == 1){
                return await AssignFirstRoundTeamsAsync(tournamentRound.Id, userName);
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
            switch (tournamentRound.QualifyingTeamSelectionStrategy)
            {
                case TeamSelectionStrategy.WinnersOnly:
                    // Select teams who won at least one match, ordered by rank
                    qualifyingTeams = previousRoundTeams
                        .Where(t => t.Wins > 0)
                        .Take(tournamentRound.QualifyingTeamsCount)
                        .ToList();
                    break;

                case TeamSelectionStrategy.SeedTopHalf:
                    int halfCount = previousRoundTeams.Count / 2;
                    int teamsToSelect = Math.Min(halfCount, tournamentRound.QualifyingTeamsCount);
                    qualifyingTeams = previousRoundTeams
                        .Take(teamsToSelect)
                        .ToList();
                    break;

                case TeamSelectionStrategy.TopByPoints:
                    qualifyingTeams = previousRoundTeams
                        .Take(tournamentRound.QualifyingTeamsCount)
                        .ToList();
                    break;

                case TeamSelectionStrategy.TopFromGroupAndNextBest:
                    // Get top team from each group
                    var groupWinners = previousRoundTeams
                        .GroupBy(t => t.GroupName)
                        .Select(g => g.OrderBy(t => t.Rank).First())
                        .ToList();

                    // Get next best teams overall
                    var remainingTeams = previousRoundTeams
                        .Except(groupWinners)
                        .Take(tournamentRound.QualifyingTeamsCount - groupWinners.Count)
                        .ToList();

                    qualifyingTeams = groupWinners.Concat(remainingTeams).ToList();
                    break;

                case TeamSelectionStrategy.Manual:
                    throw new InvalidOperationException("Manual team selection must be done through UI");

                default:
                    throw new InvalidOperationException($"Unknown team selection method: {tournamentRound.AdvancingTeamSelectionStrategy}");
            }
            //Remove any existing teams for the round
            var existingTeams = await _context.TournamentRoundTeams
                .Where(trt => trt.TournamentRoundId == tournamentRound.Id)
                .ToListAsync();
            _context.TournamentRoundTeams.RemoveRange(existingTeams);
            await _context.SaveChangesAsync();

            // Create TournamentRoundTeam entries for qualifying teams
            int seedNumber = 1;
            foreach (var qualifyingTeam in qualifyingTeams.OrderBy(t => t.Rank))
            {
                var roundTeam = new TournamentRoundTeam
                {
                    TournamentId = tournamentRound.TournamentId,
                    DivisionId = tournamentRound.DivisionId,
                    RoundId = tournamentRound.RoundId,
                    TeamId = qualifyingTeam.TeamId,
                    TournamentRoundId = tournamentRound.Id,
                    SeedNumber = seedNumber++,
                    GroupName = string.Empty,
                    CreatedBy = userName,
                    UpdatedBy = userName,
                    CreatedAt = DateTime.Now
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
        int startingCourtNumber,
        int numberOfCourts,
        int matchTimeInterval,
        string userName)
    {
        try
        {
            _logger.LogInformation("Generating matches for round {TournamentRoundId} with {NumberOfCourts} courts starting at court {StartingCourtNumber}", 
                tournamentRoundId, numberOfCourts, startingCourtNumber);

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
            switch (tournamentRound.MatchGenerationStrategy)
            {
                case MatchGenerationStrategy.RoundRobin:
                    matches = GenerateRoundRobinMatches(tournamentRound, roundTeams, ref matchNumber, startTime, 
                        startingCourtNumber, numberOfCourts, matchTimeInterval, userName);
                    break;

                case MatchGenerationStrategy.SeededBracket:
                    matches = GenerateSeededBracketMatches(tournamentRound, roundTeams, ref matchNumber, startTime, 
                        startingCourtNumber, numberOfCourts, matchTimeInterval, userName);
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
        DateTime startTime,
        int startingCourtNumber,
        int numberOfCourts,
        int matchTimeInterval,
        string userName)
    {
        var matches = new List<Match>();
        var orderedTeams = teams.OrderBy(t => t.SeedNumber).ToList();

        // Group teams by their GroupName
        var groupedTeams = orderedTeams.GroupBy(t => string.IsNullOrEmpty(t.GroupName) ? "NoGroup" : t.GroupName).ToList();

        // Track next available time for each court
        var courtSchedule = new Dictionary<int, DateTime>();
        for (int i = 0; i < numberOfCourts; i++)
        {
            courtSchedule[startingCourtNumber + i] = startTime;
        }

        // Assign each group to a court (round-robin if more groups than courts)
        var groupToCourtMap = new Dictionary<string, int>();
        int courtIndex = 0;
        foreach (var group in groupedTeams)
        {
            int assignedCourt = startingCourtNumber + (courtIndex % numberOfCourts);
            groupToCourtMap[group.Key] = assignedCourt;
            courtIndex++;
        }

        // Generate matches for each group
        foreach (var group in groupedTeams)
        {
            var groupTeams = group.ToList();
            int assignedCourt = groupToCourtMap[group.Key];
            string groupName = group.Key == "NoGroup" ? string.Empty : group.Key;

            // Generate round-robin matches within the group
            for (int i = 0; i < groupTeams.Count; i++)
            {
                for (int j = i + 1; j < groupTeams.Count; j++)
                {
                    var match = new Match
                    {
                        TournamentId = tournamentRound.TournamentId,
                        DivisionId = tournamentRound.DivisionId,
                        RoundId = tournamentRound.RoundId,
                        MatchNumber = matchNumber++,
                        HomeTeamId = groupTeams[i].TeamId,
                        AwayTeamId = groupTeams[j].TeamId,
                        ScheduledTime = courtSchedule[assignedCourt],
                        CourtLocation = $"Court {assignedCourt}",
                        GroupName = groupName,
                        CreatedBy = userName,
                        UpdatedBy = userName
                    };

                    matches.Add(match);
                    
                    // Advance time for this court
                    courtSchedule[assignedCourt] = courtSchedule[assignedCourt].AddMinutes(matchTimeInterval);
                }
            }
        }

        _logger.LogInformation("Generated {MatchCount} round-robin matches across {CourtCount} courts", 
            matches.Count, numberOfCourts);

        return matches;
    }

    private List<Match> GenerateSeededBracketMatches(
        TournamentRound tournamentRound,
        List<TournamentRoundTeam> teams,
        ref int matchNumber,
        DateTime startTime,
        int startingCourtNumber,
        int numberOfCourts,
        int matchTimeInterval,
        string userName)
    {
        var matches = new List<Match>();
        var orderedTeams = teams.OrderBy(t => t.SeedNumber).ToList();

        // Track next available time for each court
        var courtSchedule = new Dictionary<int, DateTime>();
        for (int i = 0; i < numberOfCourts; i++)
        {
            courtSchedule[startingCourtNumber + i] = startTime;
        }

        // Standard tournament seeding: 1 vs Last, 2 vs Second-to-last, etc.
        int teamCount = orderedTeams.Count;
        int courtIndex = 0;

        for (int i = 0; i < teamCount / 2; i++)
        {
            // Assign to courts in round-robin fashion
            int assignedCourt = startingCourtNumber + (courtIndex % numberOfCourts);

            var match = new Match
            {
                TournamentId = tournamentRound.TournamentId,
                DivisionId = tournamentRound.DivisionId,
                RoundId = tournamentRound.RoundId,
                MatchNumber = matchNumber++,
                HomeTeamId = orderedTeams[i].TeamId,
                AwayTeamId = orderedTeams[teamCount - 1 - i].TeamId,
                ScheduledTime = courtSchedule[assignedCourt],
                CourtLocation = $"Court {assignedCourt}",
                GroupName = string.Empty,
                CreatedBy = userName,
                UpdatedBy = userName
            };

            matches.Add(match);
            
            // Advance time for this court
            courtSchedule[assignedCourt] = courtSchedule[assignedCourt].AddMinutes(matchTimeInterval);
            courtIndex++;
        }

        _logger.LogInformation("Generated {MatchCount} seeded bracket matches across {CourtCount} courts", 
            matches.Count, numberOfCourts);

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

            if (tournamentRound.IsLocked)
            {
                throw new InvalidOperationException("Round is locked");
            }

            // Verify all matches are complete
            if (!await AreAllMatchesCompleteAsync(tournamentRoundId))
            {
                throw new InvalidOperationException("All matches must be complete before finalizing round");
            }

            // Update team rankings
            await _ranksService.UpdateTeamRanksAsync(tournamentRoundId);
            // Mark round as finished but not locked
            tournamentRound.IsFinished = true;
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
    public async Task<TournamentRound> UnfinalizeRoundAsync(Guid tournamentRoundId, string userName)
    {
        try
        {
            _logger.LogInformation("Unlocking round {TournamentRoundId}", tournamentRoundId);

            var tournamentRound = await GetTournamentRoundByIdAsync(tournamentRoundId);
            if (tournamentRound == null)
            {
                throw new InvalidOperationException($"Tournament round {tournamentRoundId} not found");
            }
  
            // Mark round as finished and locked
            tournamentRound.IsFinished = false;
            tournamentRound.IsLocked = false;
            tournamentRound.UpdatedBy = userName;
            tournamentRound.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Unlocking round {TournamentRoundId}", tournamentRoundId);

            return tournamentRound;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error unlocking round {TournamentRoundId}", tournamentRoundId);
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

    public async Task<int> DeleteAllRoundsByTournamentAsync(Guid tournamentId)
    {
        try
        {
            _logger.LogInformation("Deleting all rounds for tournament {TournamentId}", tournamentId);

            // Get all tournament rounds for this tournament
            var tournamentRounds = await _context.TournamentRounds
                .Where(tr => tr.TournamentId == tournamentId)
                .ToListAsync();

            if (!tournamentRounds.Any())
            {
                _logger.LogInformation("No rounds found for tournament {TournamentId}", tournamentId);
                return 0;
            }

            var tournamentRoundIds = tournamentRounds.Select(tr => tr.Id).ToList();
            var roundIds = tournamentRounds.Select(tr => tr.RoundId).Distinct().ToList();

            // Delete tournament round teams
            var tournamentRoundTeams = await _context.TournamentRoundTeams
                .Where(trt => tournamentRoundIds.Contains(trt.TournamentRoundId))
                .ToListAsync();
            _context.TournamentRoundTeams.RemoveRange(tournamentRoundTeams);
            _logger.LogInformation("Deleted {Count} tournament round teams", tournamentRoundTeams.Count);

            // Get all matches for these rounds
            var matches = await _context.Matches
                .Where(m => roundIds.Contains(m.RoundId) && m.TournamentId == tournamentId)
                .ToListAsync();

            if (matches.Any())
            {
                var matchIds = matches.Select(m => m.Id).ToList();

                // Delete match sets
                var matchSets = await _context.MatchSets
                    .Where(ms => matchIds.Contains(ms.MatchId))
                    .ToListAsync();
                _context.MatchSets.RemoveRange(matchSets);
                _logger.LogInformation("Deleted {Count} match sets", matchSets.Count);

                // Delete match updates
                var matchUpdates = await _context.MatchUpdates
                    .Where(mu => matchIds.Contains(mu.MatchId))
                    .ToListAsync();
                _context.MatchUpdates.RemoveRange(matchUpdates);
                _logger.LogInformation("Deleted {Count} match updates", matchUpdates.Count);

                // Delete matches
                _context.Matches.RemoveRange(matches);
                _logger.LogInformation("Deleted {Count} matches", matches.Count);
            }

            // Delete tournament rounds
            _context.TournamentRounds.RemoveRange(tournamentRounds);
            _logger.LogInformation("Deleted {Count} tournament rounds", tournamentRounds.Count);

            /*// Delete the actual CurrentRound entities
            var rounds = await _context.Rounds
                .Where(r => roundIds.Contains(r.Id))
                .ToListAsync();
            _context.Rounds.RemoveRange(rounds);
            _logger.LogInformation("Deleted {Count} rounds", rounds.Count);
*/
            await _context.SaveChangesAsync();

            _logger.LogInformation("Successfully deleted {Count} rounds for tournament {TournamentId}", 
                tournamentRounds.Count, tournamentId);

            return tournamentRounds.Count;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting all rounds for tournament {TournamentId}", tournamentId);
            throw;
        }
    }

    public async Task<IEnumerable<TournamentRoundTeamSummaryViewModel>> GetPlayoffCandidateTeamsAsync(Guid previousRoundId, int numberOfTeamsToSelect)
    {
        try
        {
            _logger.LogInformation("Getting playoff candidate teams from round {PreviousRoundId}", previousRoundId);

            var previousRound = await GetTournamentRoundByIdAsync(previousRoundId);
            if (previousRound == null)
            {
                throw new InvalidOperationException($"Previous round {previousRoundId} not found");
            }

            if (!previousRound.IsFinished)
            {
                throw new InvalidOperationException("Previous round must be finished to get playoff candidates");
            }

            // Get all teams that participated in the previous round with their rankings
            var allTeamsInPreviousRound = await _ranksService.GetStandingsAsync(previousRoundId);

            // Identify teams that have already advanced to the next round
            var advancingTeamIds = new HashSet<Guid>();
            
            if (previousRound.NextTournamentRoundId.HasValue)
            {
                // Get teams that advanced to the next round
                var nextRoundTeams = await _context.TournamentRoundTeams
                    .Where(trt => trt.TournamentRoundId == previousRound.NextTournamentRoundId.Value)
                    .Select(trt => trt.TeamId)
                    .ToListAsync();
                
                advancingTeamIds = nextRoundTeams.ToHashSet();
            }
            else
            {
                // If there's no next round yet, identify which teams WOULD advance based on the selection strategy
                // This uses the same logic as SelectTeamsForRoundAsync
                switch (previousRound.AdvancingTeamSelectionStrategy)
                {
                    case TeamSelectionStrategy.WinnersOnly:
                        advancingTeamIds = allTeamsInPreviousRound
                            .Where(t => t.Wins > 0)
                            .Take(previousRound.AdvancingTeamsCount)
                            .Select(t => t.TeamId)
                            .ToHashSet();
                        break;

                    case TeamSelectionStrategy.SeedTopHalf:
                        int halfCount = allTeamsInPreviousRound.Count / 2;
                        int teamsToSelect = Math.Min(halfCount, previousRound.AdvancingTeamsCount);
                        advancingTeamIds = allTeamsInPreviousRound
                            .Take(teamsToSelect)
                            .Select(t => t.TeamId)
                            .ToHashSet();
                        break;

                    case TeamSelectionStrategy.TopByPoints:
                        advancingTeamIds = allTeamsInPreviousRound
                            .Take(previousRound.AdvancingTeamsCount)
                            .Select(t => t.TeamId)
                            .ToHashSet();
                        break;

                    case TeamSelectionStrategy.TopFromGroupAndNextBest:
                        // Get top team from each group
                        var groupWinners = allTeamsInPreviousRound
                            .GroupBy(t => t.GroupName)
                            .Select(g => g.OrderBy(t => t.Rank).First())
                            .ToList();

                        // Get next best teams overall
                        var remainingTeams = allTeamsInPreviousRound
                            .Where(t => !groupWinners.Any(gw => gw.TeamId == t.TeamId))
                            .Take(previousRound.AdvancingTeamsCount - groupWinners.Count)
                            .ToList();

                        advancingTeamIds = groupWinners.Concat(remainingTeams)
                            .Select(t => t.TeamId)
                            .ToHashSet();
                        break;

                    case TeamSelectionStrategy.Manual:
                        // For manual selection, we can't determine advancing teams automatically
                        // Return all teams as potential candidates
                        _logger.LogWarning("Manual team selection strategy - cannot determine advancing teams automatically");
                        break;

                    default:
                        throw new InvalidOperationException($"Unknown team selection strategy: {previousRound.AdvancingTeamSelectionStrategy}");
                }
            }

            // Filter out the advancing teams to get the "losers" (non-advancing teams)
            var loserTeams = allTeamsInPreviousRound
                .Where(t => !advancingTeamIds.Contains(t.TeamId))
                .ToList();

            // Rank the losers by their performance (already ranked by GetStandingsAsync)
            // Take the top numberOfTeamsToSelect from the losers
            var playoffCandidates = loserTeams
                .Take(numberOfTeamsToSelect)
                .Select(t => new TournamentRoundTeamSummaryViewModel
                {
                    TeamId = t.TeamId,
                    TeamName = t.Team?.Name ?? "Unknown",
                    SeedNumber = t.SeedNumber,
                    Rank = t.Rank,
                    RankingPoints = t.RankingPoints,
                    Points = t.Points,
                    MatchesPlayed = t.MatchesPlayed,
                    Wins = t.Wins,
                    Draws = t.Draws,
                    Losses = t.Losses,
                    SetsFor = t.SetsFor,
                    SetsAgainst = t.SetsAgainst,
                    SetsDifference = t.SetsDifference,
                    ScoreFor = t.ScoreFor,
                    ScoreAgainst = t.ScoreAgainst,
                    ScoreDifference = t.ScoreDifference,
                    GroupName = t.GroupName
                })
                .ToList();

            _logger.LogInformation("Found {CandidateCount} playoff candidate teams from {TotalLosers} non-advancing teams",
                playoffCandidates.Count, loserTeams.Count);

            return playoffCandidates;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting playoff candidate teams from round {PreviousRoundId}", previousRoundId);
            throw;
        }
    }
}
