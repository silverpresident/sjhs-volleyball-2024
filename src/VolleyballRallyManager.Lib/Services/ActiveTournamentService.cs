using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using VolleyballRallyManager.Lib.Data;
using VolleyballRallyManager.Lib.Models;

namespace VolleyballRallyManager.Lib.Services
{
    public class ActiveTournamentService : IActiveTournamentService
    {
        private readonly ApplicationDbContext _dbContext;

        public ActiveTournamentService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Tournament> GetActiveTournamentAsync()
        {
            var model = await _dbContext.Tournaments
                .FirstOrDefaultAsync(t => t.IsActive);
            model.TournamentDivisions = await _dbContext.TournamentDivisions
            .Where(td => td.TournamentId == model.Id).ToListAsync();
            return model;
        }

        public async Task<IEnumerable<Division>> GetAvailableDivisionsAsync()
        {
            return await _dbContext.Divisions.ToListAsync();
        }

        public async Task<IEnumerable<TournamentDivision>> GetTournamentDivisionsAsync()
        {
            var activeTournament = await GetActiveTournamentAsync();
            if (activeTournament == null)
            {
                throw new Exception("No active tournament found.");
            }
            var model = await _dbContext.TournamentDivisions
            //.Include(td => td.Division)
                .Where(td => td.TournamentId == activeTournament.Id)
                .ToListAsync();
            if (model.Count() > 0)
            {
                var ids = model.Select(td => td.DivisionId).ToArray();
                _dbContext.Divisions.Where(d => ids.Contains(d.Id)).Load();
            }
            return model;
        }

        public async Task<IEnumerable<TournamentTeamDivision>> GetTournamentTeamsAsync(Guid divisionId)
        {
            var activeTournament = await GetActiveTournamentAsync();
            if (activeTournament == null)
            {
                throw new Exception("No active tournament found.");
            }
            var qry = _dbContext.TournamentTeamDivisions
                .Where(ttd => ttd.TournamentId == activeTournament.Id);
            if (divisionId != Guid.Empty)
            {
                qry = qry.Where(ttd => ttd.DivisionId == divisionId);
            }
            return await qry.OrderBy(ttd => ttd.SeedNumber).ToListAsync();
            /*
            if (model == null)
            {
                return new List<TournamentTeamDivision>();
            }
            return model;*/
        }
        public async Task<int> GetTournamentTeamsCountAsync(Guid divisionId)
        {
            var activeTournament = await GetActiveTournamentAsync();
            if (activeTournament == null)
            {
                throw new Exception("No active tournament found.");
            }
            var qry = _dbContext.TournamentTeamDivisions
                .Where(ttd => ttd.TournamentId == activeTournament.Id);
            if (divisionId != Guid.Empty)
            {
                qry = qry.Where(ttd => ttd.DivisionId == divisionId);
            }
            return await qry.CountAsync();
        }

        public async Task<TournamentTeamDivision> GetTeamAsync(Guid teamId)
        {
            var activeTournament = await GetActiveTournamentAsync();
            if (activeTournament == null)
            {
                throw new Exception("No active tournament found.");
            }
            var rrd = await _dbContext.TournamentTeamDivisions.FirstOrDefaultAsync(t => t.TournamentId == activeTournament.Id && t.TeamId == teamId);
            /*if (rrd == null)
            {
                throw new Exception("Team is not in this Tournament");
            }*/
            return rrd;

        }
        public async Task RemoveTeamAsync(Guid teamId)
        {
            var activeTournament = await GetActiveTournamentAsync();
            if (activeTournament == null)
            {
                throw new Exception("No active tournament found.");
            }
            var tournamentTeamDivision = _dbContext.TournamentTeamDivisions.FirstOrDefault(t => t.TournamentId == activeTournament.Id && t.TeamId == teamId);
            if (tournamentTeamDivision != null)
            {
                _dbContext.TournamentTeamDivisions.Remove(tournamentTeamDivision);
                await _dbContext.SaveChangesAsync();
            }
        }

        public async Task<TournamentTeamDivision> AddTeamAsync(Guid teamId, Guid divisionId, string groupName, int seedNumber = 0)
        {
            var activeTournament = await GetActiveTournamentAsync();
            if (activeTournament == null)
            {
                throw new Exception("No active tournament found.");
            }
            var tournamentTeamDivision = await _dbContext.TournamentTeamDivisions.FirstOrDefaultAsync(t => t.TournamentId == activeTournament.Id && t.TeamId == teamId);
            if (tournamentTeamDivision == null)
            {
                tournamentTeamDivision = new TournamentTeamDivision();
                tournamentTeamDivision.TournamentId = activeTournament.Id;
                tournamentTeamDivision.TeamId = teamId;
                _dbContext.TournamentTeamDivisions.Add(tournamentTeamDivision);
            }
            if (seedNumber == 0)
            {
                if (tournamentTeamDivision.DivisionId != divisionId)
                {
                    tournamentTeamDivision.SeedNumber = await GenerateSeedNumberAsync(activeTournament.Id, divisionId);
                }
            }
            else
            {
                tournamentTeamDivision.SeedNumber = seedNumber;
            }
            tournamentTeamDivision.DivisionId = divisionId;
            tournamentTeamDivision.GroupName = groupName;
            await _dbContext.SaveChangesAsync();
            return tournamentTeamDivision;
        }
        public async Task<TournamentTeamDivision> SetTeamAsync(Guid teamId, Guid divisionId, string groupName, int seedNumber = 0)
        {
            var activeTournament = await GetActiveTournamentAsync();
            if (activeTournament == null)
            {
                throw new Exception("No active tournament found.");
            }
            var tournamentTeamDivision = await _dbContext.TournamentTeamDivisions.FirstOrDefaultAsync(t => t.TournamentId == activeTournament.Id && t.TeamId == teamId);
            if (tournamentTeamDivision == null)
            {
                throw new Exception("Team is not in this Tournament");
            }
            if (seedNumber == 0)
            {
                if (tournamentTeamDivision.DivisionId != divisionId)
                {
                    tournamentTeamDivision.SeedNumber = await GenerateSeedNumberAsync(activeTournament.Id, divisionId);
                }
            }
            else
            {
                tournamentTeamDivision.SeedNumber = seedNumber;
            }
            if (tournamentTeamDivision.DivisionId != divisionId)
            {
                tournamentTeamDivision.SeedNumber = await GenerateSeedNumberAsync(activeTournament.Id, divisionId);
            }
            tournamentTeamDivision.DivisionId = divisionId;
            tournamentTeamDivision.GroupName = groupName;
            await _dbContext.SaveChangesAsync();
            return tournamentTeamDivision;
        }
        public async Task<int> TeamCountAsync(Guid? divisionId = null)
        {

            var activeTournament = await GetActiveTournamentAsync();
            if (activeTournament == null)
            {
                return 0;
            }

            if (divisionId != null)
            {
                return await _dbContext.TournamentTeamDivisions.CountAsync(t => t.TournamentId == activeTournament.Id && t.DivisionId == divisionId);

            }
            return await _dbContext.TournamentTeamDivisions.CountAsync(t => t.TournamentId == activeTournament.Id);

        }
        public async Task<int> MatchCountAsync(Guid? divisionId = null)
        {
            var activeTournament = await GetActiveTournamentAsync();
            if (activeTournament == null)
            {
                return 0;
            }
            if (divisionId != null)
            {
                throw new NotImplementedException();
                //                return await _dbContext.Matches.CountAsync(m => m.TournamentId == activeTournament.Id && m.DivisionId == divisionId)
            }
            return await _dbContext.Matches.CountAsync(m => m.TournamentId == activeTournament.Id);
        }
        private async Task<int> GenerateSeedNumberAsync(Guid tournamentId, Guid divisionId)
        {
            int count = _dbContext.TournamentTeamDivisions.Count(t => t.TournamentId == tournamentId);
            return count;
        }

        public async Task UpdateTeamStatisticsAsync(Match match)
        {
            if (match == null || !match.IsFinished) return;

            var homeTeam = await GetTeamAsync(match.HomeTeamId);
            var awayTeam = await GetTeamAsync(match.AwayTeamId);

            if (homeTeam != null)
            {
                homeTeam.MatchesPlayed++;
                homeTeam.PointsScored += match.HomeTeamScore;
                homeTeam.PointsConceded += match.AwayTeamScore;

                if (match.HomeTeamScore > match.AwayTeamScore)
                    homeTeam.Wins++;
                else if (match.HomeTeamScore < match.AwayTeamScore)
                    homeTeam.Losses++;
                else
                    homeTeam.Draws++;
            }

            if (awayTeam != null)
            {
                awayTeam.MatchesPlayed++;
                awayTeam.PointsScored += match.AwayTeamScore;
                awayTeam.PointsConceded += match.HomeTeamScore;

                if (match.AwayTeamScore > match.HomeTeamScore)
                    awayTeam.Wins++;
                else if (match.AwayTeamScore < match.HomeTeamScore)
                    awayTeam.Losses++;
                else
                    awayTeam.Draws++;
            }

            await _dbContext.SaveChangesAsync();
        }

        public async Task UpdateTournamentDivisionsAsync(List<Guid> selectedDivisionIds)
        {
            var activeTournament = await GetActiveTournamentAsync();
            if (activeTournament == null)
            {
                throw new Exception("No active tournament found.");
            }
            // Remove existing divisions
            var existingDivisions = await _dbContext.TournamentDivisions
                .Where(td => td.TournamentId == activeTournament.Id)
                .ToListAsync();

            var defDivisions = existingDivisions.Where(d => !selectedDivisionIds.Contains(d.DivisionId)).ToList();
            if (defDivisions.Count > 0)
            {
                _dbContext.TournamentDivisions.RemoveRange(defDivisions);
            }

            // Add selected divisions
            var tournament = await GetActiveTournamentAsync();
            if (tournament != null)
            {
                foreach (var divisionId in selectedDivisionIds)
                {
                    var division = await _dbContext.Divisions.FindAsync(divisionId);
                    if (division != null)
                    {
                        _dbContext.TournamentDivisions.Add(new TournamentDivision { TournamentId = activeTournament.Id, DivisionId = divisionId, Tournament = activeTournament, Division = division });
                    }
                }

                await _dbContext.SaveChangesAsync();
            }
        }
    }
}
