using Microsoft.AspNetCore.Components;
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

        public async Task CreateTournamentAsync(Tournament tournament)
        {
            _context.Tournaments.Add(tournament);
            await _context.SaveChangesAsync();
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
            if (_context.TournamentDivisions.Any(td => td.TournamentId == tournamentId && td.DivisionId == divisionId))
            {
                return;
            }
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

        public Task GetDetailsAsync(Tournament tournament)
        {
            if (tournament == null)
            {
                return Task.CompletedTask;
            }
            tournament.TournamentDivisions = _context.TournamentDivisions.Where(td => td.TournamentId == tournament.Id).Include(td => td.Division).ToList();
            tournament.TournamentTeamDivisions = _context.TournamentTeamDivisions.Where(td => td.TournamentId == tournament.Id).Include(td => td.Division).Include(td => td.Team).ToList();
            return Task.CompletedTask;
        }

        public async Task<TournamentDetailsViewModel> GetTournamentDetailsAsync(Guid tournamentId)
        {
            var tournament = await GetTournamentByIdAsync(tournamentId);
            if (tournament == null)
            {
                throw new Exception("Tournament not found");
            }
            // Populate division statistics
            var divisionStats = await _context.TournamentTeamDivisions
                .Where(ttd => ttd.TournamentId == tournamentId)
                .Include(ttd => ttd.Division)
                .GroupBy(ttd => ttd.Division)
                .Select(g => new TournamentDivisionViewModel
                {
                    Division = g.Key,
                    DivisionId = g.Key.Id,
                    DivisionName = g.Key.Name,
                    GroupNames = g.Select(x => x.GroupName).Distinct().OrderBy(gn => gn).ToList(),
                    TeamCount = g.Count(),
                    RoundsCount = _context.TournamentRounds
                        .Count(m => m.TournamentId == tournamentId
                                 && m.DivisionId == g.Key.Id),
                    MatchCount = _context.Matches
                        .Count(m => m.TournamentId == tournamentId
                                 && m.DivisionId == g.Key.Id),
                    MatchesPlayed = _context.Matches
                        .Count(m => m.TournamentId == tournamentId
                                 && m.DivisionId == g.Key.Id
                                 && m.IsFinished)
                })
                .OrderBy(d => d.DivisionName)
                .ToListAsync();


            // Populate tournamentRound statistics grouped by division
            var roundStats = await _context.Matches
                .Where(m => m.TournamentId == tournamentId)
                .Include(m => m.Round)
                .GroupBy(m => new { m.RoundId, m.DivisionId, m.Round!.Name })
                .Select(g => new
                {
                    RoundId = g.Key.RoundId,
                    DivisionId = g.Key.DivisionId,
                    RoundName = g.Key.Name,
                    TeamIds = g.Select(m => m.HomeTeamId).Union(g.Select(m => m.AwayTeamId)).Distinct().ToList(),
                    MatchesScheduled = g.Count(),
                    MatchesPlayed = g.Count(m => m.IsFinished)
                })
                .ToListAsync();


            // Get division names for rounds
            var divisionNames = await _context.Divisions
                .Where(d => roundStats.Select(r => r.DivisionId).Contains(d.Id))
                .ToDictionaryAsync(d => d.Id, d => d.Name);

            var rounds = new List<TournamentRoundSummaryViewModel>();

            // Get all teams in tournament
            var teams = await _context.TournamentTeamDivisions
                .Where(ttd => ttd.TournamentId == tournamentId)
                .Include(ttd => ttd.Team)
                .Include(ttd => ttd.Division)
                .OrderBy(ttd => ttd.Division.Name)
                .ThenBy(ttd => ttd.GroupName)
                .ThenBy(ttd => ttd.Team.Name)
                .ToListAsync();

            // Get TournamentRounds
            var tournamentRounds = _context.TournamentRounds.Where(tr => tr.TournamentId == tournamentId).ToList();

            foreach (var tr in tournamentRounds)
            {
                var roundView = new TournamentRoundSummaryViewModel()
                {
                    TournamentRoundId = tr.Id,
                    TournamentId = tournament.Id,
                    RoundId = tr.RoundId,
                    DivisionId = tr.DivisionId,
                    IsFinished = tr.IsFinished,
                    IsLocked = tr.IsLocked,
                    RoundNumber = tr.RoundNumber,
                    AdvancingTeamsCount = tr.AdvancingTeamsCount,
                    AdvancingTeamSelectionStrategy = tr.AdvancingTeamSelectionStrategy,
                    MatchGenerationStrategy = tr.MatchGenerationStrategy,
                    TournamentName = tournament.Name,
                };
                var divisionStat = divisionStats.FirstOrDefault(rs => rs.DivisionId == roundView.DivisionId);
                if (divisionStat != null)
                {
                    roundView.DivisionName = divisionStat.DivisionName;
                }

                var round = await _context.Rounds.FindAsync(tr.RoundId);
                if (round != null)
                {
                    roundView.RoundName = round.Name;
                }

                var roundStat = roundStats.FirstOrDefault(rs => rs.DivisionId == roundView.DivisionId && rs.RoundId == roundView.RoundId);
                if (roundStat != null)
                {
                    roundView.MatchesScheduled = roundStat.MatchesScheduled;
                    roundView.MatchesPlayed = roundStat.MatchesPlayed;
                }
                roundView.TeamCount = await _context.TournamentRoundTeams.CountAsync(trt => trt.TournamentId == tr.TournamentId
                            && trt.DivisionId == roundView.DivisionId && trt.RoundId == roundView.RoundId);
                //var hasTeams = await _context.TournamentRoundTeams.AnyAsync(trt => trt.TournamentId == tournamentRound.TournamentId
                //           && trt.DivisionId == roundView.DivisionId && trt.RoundId == roundView.RoundId);
                var hasMatches = await _context.Matches.AnyAsync(m => m.TournamentId == roundView.TournamentId && m.DivisionId == roundView.DivisionId && m.RoundId == roundView.RoundId);

                var allMatchesComplete = !await _context.Matches.AnyAsync(m => m.TournamentId == roundView.TournamentId && m.DivisionId == roundView.DivisionId && m.RoundId == roundView.RoundId && m.IsFinished == false);
                /*
                                // Determine button visibility based on tournamentRound state
                                roundView.CanFinalize = !tr.IsFinished && hasMatches && allMatchesComplete;
                                roundView.CanGenerateNextRound = tr.IsFinished;
                                roundView.CanSelectTeams = !hasTeams && tr.PreviousTournamentRoundId.HasValue;
                                roundView.CanGenerateMatches = hasTeams && !hasMatches;

                                // Check if previous tournamentRound is finished for team selection
                                if (roundView.CanSelectTeams && tr.PreviousTournamentRoundId.HasValue)
                                {
                                    var previousRound = await _context.TournamentRounds.FindAsync(tr.PreviousTournamentRoundId.Value);
                                    roundView.CanSelectTeams = previousRound != null && previousRound.IsFinished;
                                }*/

                await FixButtonState(roundView, tr, hasMatches, allMatchesComplete);

                /// <summary>
                rounds.Add(roundView);
            }

            var viewModel = new TournamentDetailsViewModel
            {
                TournamentId = tournamentId,
                Tournament = tournament,
                Divisions = divisionStats,
                Rounds = rounds.OrderBy(r => r.DivisionName)
                        .ThenBy(r => r.RoundNumber).ToList(),
                Teams = teams,
                TeamsByDivision = teams.GroupBy(t => t.Division)
                    .ToDictionary(g => g.Key,
                                g => g.OrderBy(t => t.GroupName)
                                        .ThenBy(t => t.Team.Name).ToList())
            };

            return viewModel;
        }


        public async Task<TournamentRoundSummaryViewModel?> GetTournamentRoundSummaryAsync(Guid tournamentRoundId)
        {
            // Get the TournamentRound
            var tournamentRound = await _context.TournamentRounds.FindAsync(tournamentRoundId);
            if (tournamentRound == null)
            {
                return null;
            }

            // Get the tournament
            var tournament = await _context.Tournaments.FindAsync(tournamentRound.TournamentId);
            if (tournament == null)
            {
                return null;
            }

            // Get the tournamentRound name
            var round = await _context.Rounds.FindAsync(tournamentRound.RoundId);

            // Get the division name
            var division = await _context.Divisions.FindAsync(tournamentRound.DivisionId);

            // Create the view model
            var roundView = new TournamentRoundSummaryViewModel
            {
                TournamentRoundId = tournamentRound.Id,
                TournamentId = tournament.Id,
                RoundId = tournamentRound.RoundId,
                DivisionId = tournamentRound.DivisionId,
                IsFinished = tournamentRound.IsFinished,
                IsLocked = tournamentRound.IsLocked,
                RoundNumber = tournamentRound.RoundNumber,
                AdvancingTeamsCount = tournamentRound.AdvancingTeamsCount,
                AdvancingTeamSelectionStrategy = tournamentRound.AdvancingTeamSelectionStrategy,
                MatchGenerationStrategy = tournamentRound.MatchGenerationStrategy,
                TournamentName = tournament.Name,
                DivisionName = division?.Name ?? string.Empty,
                RoundName = round?.Name ?? string.Empty
            };

            // Get tournamentRound statistics (matches scheduled, matches played)
            var matchStats = await _context.Matches
                .Where(m => m.TournamentId == tournamentRound.TournamentId
                         && m.RoundId == tournamentRound.RoundId
                         && m.DivisionId == tournamentRound.DivisionId)
                .GroupBy(m => 1)
                .Select(g => new
                {
                    MatchesScheduled = g.Count(),
                    MatchesPlayed = g.Count(m => m.IsFinished)
                })
                .FirstOrDefaultAsync();

            if (matchStats != null)
            {
                roundView.MatchesScheduled = matchStats.MatchesScheduled;
                roundView.MatchesPlayed = matchStats.MatchesPlayed;
            }

            // Get team count
            roundView.TeamCount = await _context.TournamentRoundTeams
                .CountAsync(trt => trt.TournamentId == tournamentRound.TournamentId
                            && trt.DivisionId == tournamentRound.DivisionId
                                && trt.RoundId == tournamentRound.RoundId);

            // Determine button visibility based on tournamentRound state
            /*var hasTeams = await _context.TournamentRoundTeams
                .AnyAsync(trt => trt.TournamentId == tournamentRound.TournamentId
                            && trt.DivisionId == tournamentRound.DivisionId
                              && trt.RoundId == tournamentRound.RoundId);*/

            var hasMatches = await _context.Matches
                .AnyAsync(m => m.TournamentId == tournamentRound.TournamentId
                            && m.DivisionId == tournamentRound.DivisionId
                            && m.RoundId == tournamentRound.RoundId);

            var allMatchesComplete = !await _context.Matches
                .AnyAsync(m => m.TournamentId == tournamentRound.TournamentId
                            && m.DivisionId == tournamentRound.DivisionId
                            && m.RoundId == tournamentRound.RoundId
                            && m.IsFinished == false);

            /*roundView.CanFinalize = !tournamentRound.IsFinished && hasMatches && allMatchesComplete;
            roundView.CanGenerateNextRound = tournamentRound.IsFinished;
            roundView.CanSelectTeams = !hasTeams && tournamentRound.PreviousTournamentRoundId.HasValue;
            roundView.CanGenerateMatches = hasTeams && !hasMatches;

            // Check if previous tournamentRound is finished for team selection
            if (roundView.CanSelectTeams && tournamentRound.PreviousTournamentRoundId.HasValue)
            {
                var previousRound = await _context.TournamentRounds.FindAsync(tournamentRound.PreviousTournamentRoundId.Value);
                roundView.CanSelectTeams = previousRound != null && previousRound.IsFinished;
            }*/

            await FixButtonState(roundView, tournamentRound, hasMatches, allMatchesComplete);

            return roundView;
        }

        public async Task<TournamentDivisionDetailsViewModel> GetTournamentDivisionDetailsAsync(Guid tournamentId, Guid divisionId)
        {
            // Get the tournament
            var tournament = await GetTournamentByIdAsync(tournamentId);
            if (tournament == null)
            {
                throw new Exception("Tournament not found");
            }

            // Get the division
            var division = await _context.Divisions.FindAsync(divisionId);
            if (division == null)
            {
                throw new Exception("Division not found");
            }

            // Get division statistics for this specific division
            var divisionStats = await _context.TournamentTeamDivisions
                .Where(ttd => ttd.TournamentId == tournamentId && ttd.DivisionId == divisionId)
                .Include(ttd => ttd.Division)
                .GroupBy(ttd => ttd.Division)
                .Select(g => new TournamentDivisionViewModel
                {
                    Division = g.Key,
                    DivisionId = g.Key.Id,
                    DivisionName = g.Key.Name,
                    GroupNames = g.Select(x => x.GroupName).Distinct().OrderBy(gn => gn).ToList(),
                    TeamCount = g.Count(),
                    RoundsCount = _context.TournamentRounds
                        .Count(m => m.TournamentId == tournamentId
                                 && m.DivisionId == g.Key.Id),
                    MatchCount = _context.Matches
                        .Count(m => m.TournamentId == tournamentId
                                 && m.DivisionId == g.Key.Id),
                    MatchesPlayed = _context.Matches
                        .Count(m => m.TournamentId == tournamentId
                                 && m.DivisionId == g.Key.Id
                                 && m.IsFinished)
                })
                .FirstOrDefaultAsync();

            // Create a default division stats if no teams are assigned yet
            if (divisionStats == null)
            {
                divisionStats = new TournamentDivisionViewModel
                {
                    Division = division,
                    DivisionId = division.Id,
                    DivisionName = division.Name,
                    GroupNames = new List<string>(),
                    TeamCount = 0,
                    RoundsCount = await _context.TournamentRounds
                        .CountAsync(m => m.TournamentId == tournamentId && m.DivisionId == divisionId),
                    MatchCount = await _context.Matches
                        .CountAsync(m => m.TournamentId == tournamentId && m.DivisionId == divisionId),
                    MatchesPlayed = await _context.Matches
                        .CountAsync(m => m.TournamentId == tournamentId && m.DivisionId == divisionId && m.IsFinished)
                };
            }

            // Get tournamentRound statistics for this specific division
            var roundStats = await _context.Matches
                .Where(m => m.TournamentId == tournamentId && m.DivisionId == divisionId)
                .Include(m => m.Round)
                .GroupBy(m => new { m.RoundId, m.DivisionId, m.Round!.Name })
                .Select(g => new
                {
                    RoundId = g.Key.RoundId,
                    DivisionId = g.Key.DivisionId,
                    RoundName = g.Key.Name,
                    TeamIds = g.Select(m => m.HomeTeamId).Union(g.Select(m => m.AwayTeamId)).Distinct().ToList(),
                    MatchesScheduled = g.Count(),
                    MatchesPlayed = g.Count(m => m.IsFinished)
                })
                .ToListAsync();

            // Get all teams in this division
            var teams = await _context.TournamentTeamDivisions
                .Where(ttd => ttd.TournamentId == tournamentId && ttd.DivisionId == divisionId)
                .Include(ttd => ttd.Team)
                .Include(ttd => ttd.Division)
                .OrderBy(ttd => ttd.GroupName)
                .ThenBy(ttd => ttd.Team.Name)
                .ToListAsync();

            // Get TournamentRounds for this specific division
            var tournamentRounds = await _context.TournamentRounds
                .Where(tr => tr.TournamentId == tournamentId && tr.DivisionId == divisionId)
                .OrderBy(tr => tr.RoundNumber)
                .ToListAsync();

            var roundViewModels = new List<TournamentRoundSummaryViewModel>();

            foreach (var tr in tournamentRounds)
            {
                var roundView = new TournamentRoundSummaryViewModel()
                {
                    TournamentRoundId = tr.Id,
                    TournamentId = tournament.Id,
                    RoundId = tr.RoundId,
                    DivisionId = tr.DivisionId,
                    IsFinished = tr.IsFinished,
                    IsLocked = tr.IsLocked,
                    RoundNumber = tr.RoundNumber,
                    AdvancingTeamsCount = tr.AdvancingTeamsCount,
                    AdvancingTeamSelectionStrategy = tr.AdvancingTeamSelectionStrategy,
                    MatchGenerationStrategy = tr.MatchGenerationStrategy,
                    TournamentName = tournament?.Name ?? "Unknown Tournament",
                    DivisionName = division?.Name ?? "Unknown Division",
                };

                var round = await _context.Rounds.FindAsync(tr.RoundId);
                if (round != null)
                {
                    roundView.RoundName = round.Name;
                    if (round.Sequence == 2)
                    {
                        roundView.RoundName = $"Round {roundView.RoundNumber}";
                    }
                }

                var roundStat = roundStats.FirstOrDefault(rs => rs.RoundId == roundView.RoundId);
                if (roundStat != null)
                {
                    roundView.MatchesScheduled = roundStat.MatchesScheduled;
                    roundView.MatchesPlayed = roundStat.MatchesPlayed;
                }

                roundView.TeamCount = await _context.TournamentRoundTeams.CountAsync(trt => trt.TournamentId == roundView.TournamentId && trt.DivisionId == roundView.DivisionId && trt.RoundId == roundView.RoundId);
                //var hasTeams = await _context.TournamentRoundTeams.AnyAsync(trt => trt.DivisionId == roundView.DivisionId && trt.RoundId == roundView.RoundId);
                var hasMatches = await _context.Matches.AnyAsync(m => m.TournamentId == roundView.TournamentId && m.DivisionId == roundView.DivisionId && m.RoundId == roundView.RoundId);

                var allMatchesComplete = !await _context.Matches.AnyAsync(m => m.TournamentId == roundView.TournamentId && m.DivisionId == roundView.DivisionId && m.RoundId == roundView.RoundId && m.IsFinished == false);

                await FixButtonState(roundView, tr, hasMatches, allMatchesComplete);

                roundViewModels.Add(roundView);
            }

            var viewModel = new TournamentDivisionDetailsViewModel
            {
                TournamentId = tournamentId,
                DivisionId = divisionId,
                Tournament = tournament,
                Division = division,
                DivisionStats = divisionStats,
                Rounds = roundViewModels,
                Teams = teams
            };
            return viewModel;
        }



        private async Task FixButtonState(ITournamentRoundButtonState roundView, TournamentRound tr, bool hasMatches, bool allMatchesComplete)
        {
            var hasTeams = roundView.TeamCount > 0;
            // Determine button visibility based on tournamentRound state
            roundView.CanFinalize = !tr.IsFinished && hasMatches && allMatchesComplete;
            roundView.CanGenerateNextRound = tr.IsFinished;
            roundView.CanSelectTeams = !hasTeams && tr.PreviousTournamentRoundId.HasValue;
            roundView.CanGenerateMatches = hasTeams && !tr.IsFinished && !tr.IsLocked;

            // Check if previous tournamentRound is finished for team selection
            if (roundView.CanSelectTeams && tr.PreviousTournamentRoundId.HasValue)
            {
                var previousRound = await _context.TournamentRounds.FindAsync(tr.PreviousTournamentRoundId.Value);
                roundView.CanSelectTeams = previousRound != null && previousRound.IsFinished;
            }
            //TODO temp
            roundView.CanSelectTeams = true;

        }

        public async Task<TournamentRoundDetailsViewModel?> GetTournamentRoundDetailsAsync(Guid tournamentRoundId)
        {
            // Get the TournamentRound
            var tournamentRound = await _context.TournamentRounds.FindAsync(tournamentRoundId);
            if (tournamentRound == null)
            {
                throw new Exception("Round not found");
            }


            var teams = await _context.TournamentRoundTeams
                .Include(trt => trt.Team)
                .Where(trt => trt.TournamentRoundId == tournamentRound.Id)
                .OrderBy(trt => trt.SeedNumber)
                .ToListAsync();

            var matches = await _context.Matches
                .Include(m => m.HomeTeam)
                .Include(m => m.AwayTeam)
                .Include(m => m.Sets)
                .Where(m => m.TournamentId == tournamentRound.TournamentId
                    && m.DivisionId == tournamentRound.DivisionId
                    && m.RoundId == tournamentRound.RoundId)
                .OrderBy(m => m.MatchNumber)
                .ToListAsync();
            if (matches == null) {
                matches = new List<Match>();
            }
            var teamViewModels = teams.Select(t => new TournamentRoundTeamSummaryViewModel
            {
                TeamId = t.TeamId,
                TeamName = t.Team?.Name ?? "Unknown",
                SeedNumber = t.SeedNumber,
                FinalRank = t.Rank,
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
            }).ToList();

            var hasMatches = matches.Any();
            var allMatchesComplete = matches.Any() && (matches.Count(m => m.IsFinished) == matches.Count());

            var viewModel = new TournamentRoundDetailsViewModel
            {
                CurrentRound = tournamentRound,
                Teams = teamViewModels,
                Matches = matches,
                TeamCount = teams.Count
            };
            if (tournamentRound.PreviousTournamentRoundId.HasValue)
            {
                var previousRound = await _context.TournamentRounds.FindAsync(tournamentRound.PreviousTournamentRoundId.Value);
                viewModel.PreviousRound = previousRound!;
            }
            if (tournamentRound.GroupingStrategy == GroupGenerationStrategy.GroupsInRound)
            {
                viewModel.GroupingStrategyLabel = $"{tournamentRound.GroupsInRound} Groups In Round";
            }
            else if (tournamentRound.GroupingStrategy == GroupGenerationStrategy.TeamsPerGroup)
            {
                viewModel.GroupingStrategyLabel = $"{tournamentRound.TeamsPerGroup} Teams Per Group";
            }
            else
            {
                viewModel.GroupingStrategyLabel = "Knockout";
            }

            await FixButtonState(viewModel, tournamentRound, hasMatches, allMatchesComplete);
            viewModel.CanSelectTeams = true;
            return viewModel;
        }
    }
}