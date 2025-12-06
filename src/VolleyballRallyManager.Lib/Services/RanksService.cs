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

            // Assign final ranks
            int rank = 1;
            foreach (var team in rankedTeams)
            {
                team.FinalRank = rank++;
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
