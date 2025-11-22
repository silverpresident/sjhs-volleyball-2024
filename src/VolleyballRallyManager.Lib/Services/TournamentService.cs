using Microsoft.EntityFrameworkCore;
using VolleyballRallyManager.Lib.Data;
using VolleyballRallyManager.Lib.Models;

namespace VolleyballRallyManager.Lib.Services
{
    public class TournamentService : ITournamentService
    {
        private readonly ApplicationDbContext _context;

        public TournamentService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task SetActiveTournamentAsync(Guid tournamentId)
        {
            var tournaments = await _context.Tournaments.ToListAsync();
            foreach (var tournament in tournaments)
            {
                tournament.IsActive = false;
            }

            var activeTournament = await _context.Tournaments.FindAsync(tournamentId);
            if (activeTournament != null)
            {
                activeTournament.IsActive = true;
            }

            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<Division>> GetTournamentDivisionsAsync(Guid tournamentId)
        {
            return await _context.TournamentDivisions
                .Where(td => td.TournamentId == tournamentId)
                .Select(td => td.Division)
                .ToListAsync();
        }

        public async Task<IEnumerable<Team>> GetTournamentTeamsAsync(Guid tournamentId)
        {
            return await _context.TournamentTeamDivisions
                .Where(ttd => ttd.TournamentId == tournamentId)
                .Select(ttd => ttd.Team)
                .ToListAsync();
        }

        public async Task<IEnumerable<Match>> GetTournamentMatchesAsync(Guid tournamentId)
        {
            return await _context.Matches
                .Where(m => m.TournamentId == tournamentId)
                .ToListAsync();
        }

        public async Task AddDivisionToTournamentAsync(Guid tournamentId, Guid divisionId)
        {
            var tournament = await _context.Tournaments.FindAsync(tournamentId);
            var division = await _context.Divisions.FindAsync(divisionId);

            if (tournament != null && division != null)
            {
                var tournamentDivision = new TournamentDivision
                {
                    TournamentId = tournamentId,
                    Tournament = tournament,
                    DivisionId = divisionId,
                    Division = division
                };
                _context.TournamentDivisions.Add(tournamentDivision);
                await _context.SaveChangesAsync();
            }
        }

        public async Task RemoveDivisionFromTournamentAsync(Guid tournamentId, Guid divisionId)
        {
            var tournamentDivision = await _context.TournamentDivisions
                .FirstOrDefaultAsync(td => td.TournamentId == tournamentId && td.DivisionId == divisionId);
            if (tournamentDivision != null)
            {
                _context.TournamentDivisions.Remove(tournamentDivision);
                await _context.SaveChangesAsync();
            }
        }

        public async Task AddTeamToTournamentAsync(Guid tournamentId, Guid teamId, Guid divisionId, string groupName, int seedNumber)
        {
            if (!_context.TournamentTeamDivisions.Any(ttd => ttd.TournamentId == tournamentId && ttd.TeamId == teamId && ttd.DivisionId == divisionId))
            {
                var division = await _context.Divisions.FindAsync(divisionId);
                if (division != null)
                {
                    _context.TournamentTeamDivisions.Add(new TournamentTeamDivision { TournamentId = tournamentId, TeamId = teamId, DivisionId = divisionId, Division = division, GroupName = groupName, SeedNumber = seedNumber });
                    await _context.SaveChangesAsync();
                }
            }
        }

        public async Task RemoveTeamFromTournamentAsync(Guid tournamentId, Guid teamId)
        {
            var tournamentTeamDivision = await _context.TournamentTeamDivisions
                .FirstOrDefaultAsync(ttd => ttd.TournamentId == tournamentId && ttd.TeamId == teamId);
            if (tournamentTeamDivision != null)
            {
                _context.TournamentTeamDivisions.Remove(tournamentTeamDivision);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<Tournament?> GetActiveTournamentAsync()
        {
            return await _context.Tournaments.FirstOrDefaultAsync(t => t.IsActive);
        }

        public async Task<Tournament?> GetTournamentByIdAsync(Guid tournamentId)
        {
            return await _context.Tournaments.FindAsync(tournamentId);
        }

        public async Task<IEnumerable<Tournament>> GetAllTournamentsAsync()
        {
            return await _context.Tournaments.ToListAsync();
        }

        public async Task UpdateTournamentAsync(Tournament tournament)
        {
            _context.Tournaments.Update(tournament);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<Team>> GetTeamsByDivisionAsync(Division division)
        {
            return await _context.TournamentTeamDivisions
                .Include(ttd => ttd.Team)
                .Where(ttd => ttd.DivisionId == division.Id)
                .Select(ttd => ttd.Team)
                .ToListAsync();
        }

        public async Task<IEnumerable<Team>> GetLeaderboardAsync(Guid divisionId)
        {
            var query = _context.TournamentTeamDivisions
            .Include(ttd => ttd.Team)
            .Where(ttd => ttd.DivisionId == divisionId)
            .OrderByDescending(ttd => ttd.TotalPoints)
            .ThenByDescending(ttd => ttd.PointDifference)
            .ThenByDescending(ttd => ttd.PointsScored)
            .Select(ttd => ttd.Team);

            return await query.ToListAsync();
        }

        public async Task RecalculateTeamStatisticsAsync(Guid teamId)
        {
            var tournamentTeamDivisions = await _context.TournamentTeamDivisions
                .Where(ttd => ttd.TeamId == teamId)
                .ToListAsync();

            foreach (var ttd in tournamentTeamDivisions)
            {
                // Reset statistics for each tournament-team-division
                ttd.MatchesPlayed = 0;
                ttd.Wins = 0;
                ttd.Draws = 0;
                ttd.Losses = 0;
                ttd.PointsScored = 0;
                ttd.PointsConceded = 0;
                ttd.TotalPoints = 0;

                // Get all finished matches for this team in the specific tournament
                var matches = await _context.Matches
                    .Where(m => (m.HomeTeamId == teamId || m.AwayTeamId == teamId) && m.IsFinished && m.TournamentId == ttd.TournamentId)
                    .ToListAsync();

                foreach (var match in matches)
                {
                    ttd.MatchesPlayed++;

                    if (match.HomeTeamId == teamId)
                    {
                        ttd.PointsScored += match.HomeTeamScore;
                        ttd.PointsConceded += match.AwayTeamScore;

                        if (match.HomeTeamScore > match.AwayTeamScore)
                        {
                            ttd.Wins++;
                            ttd.TotalPoints += 3;
                        }
                        else if (match.HomeTeamScore < match.AwayTeamScore)
                        {
                            ttd.Losses++;
                        }
                        else
                        {
                            ttd.Draws++;
                            ttd.TotalPoints += 1;
                        }
                    }
                    else // Away team
                    {
                        ttd.PointsScored += match.AwayTeamScore;
                        ttd.PointsConceded += match.HomeTeamScore;

                        if (match.AwayTeamScore > match.HomeTeamScore)
                        {
                            ttd.Wins++;
                            ttd.TotalPoints += 3;
                        }
                        else if (match.AwayTeamScore < match.HomeTeamScore)
                        {
                            ttd.Losses++;
                        }
                        else
                        {
                            ttd.Draws++;
                            ttd.TotalPoints += 1;
                        }
                    }
                }
            }
            await _context.SaveChangesAsync();
        }
    }
}
