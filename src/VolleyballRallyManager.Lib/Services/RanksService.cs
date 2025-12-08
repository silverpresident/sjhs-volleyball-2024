using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using VolleyballRallyManager.Lib.Data;
using VolleyballRallyManager.Lib.Models;

namespace VolleyballRallyManager.Lib.Services;

/// <summary>
/// Service for calculating and updating team rankings within tournament rounds
/// </summary>
public class RanksService : IRanksService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<RanksService> _logger;

    public RanksService(ApplicationDbContext context, ILogger<RanksService> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Updates team ranks for a specific tournament round based on match results
    /// Applies strict tie-breaker rules:
    /// 1. Highest Total Points (Win=3, Draw=1, Loss=0)
    /// 2. Highest Score Difference (ScoreDifference)
    /// 3. Highest Score For (ScoreFor)
    /// 4. Best Seed Number (lower seed number is better)
    /// </summary>
    public async Task<List<TournamentRoundTeam>> UpdateTeamRanksAsync(Guid tournamentRoundId)
    {
        try
        {
            _logger.LogInformation("Updating team ranks for tournament round {TournamentRoundId}", tournamentRoundId);

            // Get the tournament round
            var tournamentRound = await _context.TournamentRounds
                .Include(tr => tr.TournamentRoundTeams)
                .ThenInclude(trt => trt.Team)
                .FirstOrDefaultAsync(tr => tr.Id == tournamentRoundId);

            if (tournamentRound == null)
            {
                _logger.LogWarning("Tournament round {TournamentRoundId} not found", tournamentRoundId);
                return new List<TournamentRoundTeam>();
            }

            // Get all matches for this round
            var matches = await _context.Matches
                .Where(m => m.TournamentId == tournamentRound.TournamentId 
                    && m.DivisionId == tournamentRound.DivisionId 
                    && m.RoundId == tournamentRound.RoundId
                    && m.IsFinished)
                .Include(m => m.Sets)
                .ToListAsync();

            _logger.LogInformation("Found {MatchCount} finished matches for tournament round {TournamentRoundId}", 
                matches.Count, tournamentRoundId);

            // Calculate statistics for each team
            foreach (var roundTeam in tournamentRound.TournamentRoundTeams)
            {
                var teamMatches = matches.Where(m => m.HomeTeamId == roundTeam.TeamId || m.AwayTeamId == roundTeam.TeamId).ToList();

                roundTeam.MatchesPlayed = teamMatches.Count;
                roundTeam.Wins = 0;
                roundTeam.Draws = 0;
                roundTeam.Losses = 0;
                roundTeam.SetsFor = 0;
                roundTeam.SetsAgainst = 0;
                roundTeam.ScoreFor = 0;
                roundTeam.ScoreAgainst = 0;
                roundTeam.Points = 0;

                foreach (var match in teamMatches)
                {
                    bool isHomeTeam = match.HomeTeamId == roundTeam.TeamId;
                    int teamSets = isHomeTeam ? match.HomeTeamScore : match.AwayTeamScore;
                    int opponentSets = isHomeTeam ? match.AwayTeamScore : match.HomeTeamScore;

                    // Update set statistics
                    roundTeam.SetsFor += teamSets;
                    roundTeam.SetsAgainst += opponentSets;

                    // Calculate score statistics from individual sets
                    var matchSets = match.Sets.OrderBy(s => s.SetNumber).ToList();
                    foreach (var set in matchSets)
                    {
                        int teamScore = isHomeTeam ? set.HomeTeamScore : set.AwayTeamScore;
                        int opponentScore = isHomeTeam ? set.AwayTeamScore : set.HomeTeamScore;
                        
                        roundTeam.ScoreFor += teamScore;
                        roundTeam.ScoreAgainst += opponentScore;
                    }

                    // Determine win/draw/loss and calculate points
                    if (teamSets > opponentSets)
                    {
                        roundTeam.Wins++;
                        roundTeam.Points += 3; // Win = 3 points
                    }
                    else if (teamSets < opponentSets)
                    {
                        roundTeam.Losses++;
                        // Loss = 0 points
                    }
                    else
                    {
                        roundTeam.Draws++;
                        roundTeam.Points += 1; // Draw = 1 point
                    }
                }

                roundTeam.UpdatedAt = DateTime.Now;
            }

            // Apply ranking with tie-breaker rules
            var rankedTeams = tournamentRound.TournamentRoundTeams
                .OrderByDescending(t => t.Points)                    // 1. Highest Total Points
                .ThenByDescending(t => t.ScoreDifference)           // 2. Highest Score Difference
                .ThenByDescending(t => t.ScoreFor)                  // 3. Highest Score For
                .ThenBy(t => t.SeedNumber)                          // 4. Best Seed Number (lower is better)
                .ToList();

            // Assign final ranks and ranking points
            int rank = 1;
            int maxRankingPoints = tournamentRound.TournamentRoundTeams.Count;
            foreach (var team in rankedTeams)
            {
                team.Rank = rank++;
                team.RankingPoints = maxRankingPoints--;
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation("Successfully updated ranks for {TeamCount} teams in tournament round {TournamentRoundId}", 
                rankedTeams.Count, tournamentRoundId);

            return rankedTeams;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating team ranks for tournament round {TournamentRoundId}", tournamentRoundId);
            throw;
        }
    }
    public async Task<List<TournamentTeamDivision>> UpdateDivisionRanksAsync(Guid tournamentId, Guid divisionId)
    {
        var teams = await _context.TournamentTeamDivisions
            .Where(ttd => ttd.TournamentId == tournamentId && ttd.DivisionId == divisionId)
            .ToListAsync();
        var tournamanetRounds = await _context.TournamentRounds
            .Where(ttd => ttd.TournamentId == tournamentId && ttd.DivisionId == divisionId)
            .ToListAsync();
        var tournamanetRoundTeams = await _context.TournamentRoundTeams
            .Where(ttd => ttd.TournamentId == tournamentId && ttd.DivisionId == divisionId)
            .ToListAsync();

        foreach (var team in teams)
        {
            team.Rank = 0;
            team.ScoreAgainst = 0;
            team.ScoreFor = 0;
            team.RankingPoints = 0;
            team.TotalPoints = 0;
            team.MatchesPlayed = 0;
            team.Wins = 0;
            team.Draws = 0;
            team.Losses = 0;
            team.SetsFor = 0;
            team.SetsAgainst = 0; 
            var roundRanks = tournamanetRoundTeams.Where(trt => trt.TeamId == team.TeamId);
            foreach(var roundRank in roundRanks)
            {
                team.ScoreAgainst += roundRank.ScoreAgainst;
                team.ScoreFor += roundRank.ScoreFor;
                team.RankingPoints += roundRank.RankingPoints;
                team.TotalPoints += roundRank.Points;
                team.MatchesPlayed += roundRank.MatchesPlayed;
                team.Wins += roundRank.Wins;
                team.Draws += roundRank.Draws;
                team.Losses += roundRank.Losses;
                team.SetsFor += roundRank.SetsFor;
                team.SetsAgainst += roundRank.SetsAgainst;
            }


        }
        var rankedTeams = teams
                .OrderByDescending(t => t.RankingPoints)                    // 1. Highest Total Points
                .OrderByDescending(t => t.TotalPoints)                    // 1. Highest Total Points
                .ThenByDescending(t => t.ScoreDifference)           // 2. Highest Score Difference
                .ThenByDescending(t => t.ScoreFor)                  // 3. Highest Score For
                .ThenBy(t => t.SeedNumber)                          // 4. Best Seed Number (lower is better)
                .ToList();

        // Assign final ranks and ranking points
        int rank = 1;
        foreach (var team in rankedTeams)
        {
            team.Rank = rank++;
        }
        return rankedTeams;
    }

    /// <summary>
    /// Calculates the final rank for a specific team within a round without persisting changes
    /// </summary>
    public async Task<int> CalculateTeamRankAsync(Guid tournamentRoundId, Guid teamId)
    {
        try
        {
            var standings = await GetStandingsAsync(tournamentRoundId);
            var teamStanding = standings.FirstOrDefault(t => t.TeamId == teamId);
            
            if (teamStanding == null)
            {
                _logger.LogWarning("Team {TeamId} not found in tournament round {TournamentRoundId}", teamId, tournamentRoundId);
                return 0;
            }

            return standings.IndexOf(teamStanding) + 1;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating team rank for team {TeamId} in tournament round {TournamentRoundId}", 
                teamId, tournamentRoundId);
            throw;
        }
    }

    /// <summary>
    /// Gets the current standings for a tournament round
    /// </summary>
    public async Task<List<TournamentRoundTeam>> GetStandingsAsync(Guid tournamentRoundId)
    {
        try
        {
            var tournamentRound = await _context.TournamentRounds
                .Include(tr => tr.TournamentRoundTeams)
                .ThenInclude(trt => trt.Team)
                .FirstOrDefaultAsync(tr => tr.Id == tournamentRoundId);

            if (tournamentRound == null)
            {
                _logger.LogWarning("Tournament round {TournamentRoundId} not found", tournamentRoundId);
                return new List<TournamentRoundTeam>();
            }

            // Return teams ordered by ranking criteria
            return tournamentRound.TournamentRoundTeams
                .OrderByDescending(t => t.Points)                    // 1. Highest Total Points
                .ThenByDescending(t => t.ScoreDifference)           // 2. Highest Score Difference
                .ThenByDescending(t => t.ScoreFor)                  // 3. Highest Score For
                .ThenBy(t => t.SeedNumber)                          // 4. Best Seed Number (lower is better)
                .ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting standings for tournament round {TournamentRoundId}", tournamentRoundId);
            throw;
        }
    }
}
