using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using VolleyballRallyManager.Lib.Common;
using VolleyballRallyManager.Lib.Data;
using VolleyballRallyManager.Lib.Models;

namespace VolleyballRallyManager.Lib.Services
{
    public class ActiveTournamentService : IActiveTournamentService
    {
        private readonly ApplicationDbContext _context;
        private readonly ITournamentService _tournamentService;
        private readonly ILogger<ActiveTournamentService> _logger;
        //TODO add logging to methods

        public ActiveTournamentService(ApplicationDbContext dbContext, ILogger<ActiveTournamentService> logger, ITournamentService tournamentService)
        {
            _context = dbContext;
            _logger = logger;
            _tournamentService = tournamentService;
        }

        public async Task<Tournament?> GetActiveTournamentAsync()
        {
            var model = await _context.Tournaments
                .FirstOrDefaultAsync(t => t.IsActive);
            if (model != null)
            {
                model.TournamentDivisions = await _context.TournamentDivisions
                    .Where(td => td.TournamentId == model.Id).ToListAsync();
            }
            return model;
        }
        public async Task<Division?> GetDivisionAsync(Guid divisionId)
        {
            return await _context.Divisions.FindAsync(divisionId);
        }
        public async Task<IEnumerable<Division>> GetAvailableDivisionsAsync()
        {
            return await _context.Divisions.ToListAsync();
        }
        public async Task<IEnumerable<Team>> GetAvailableTeamsAsync()
        {

            var activeTournament = await GetActiveTournamentAsync();
            if (activeTournament == null)
            {
                throw new Exception("No active tournament found.");
            }
            var ids = _context.TournamentTeamDivisions
                .Where(ttd => ttd.TournamentId == activeTournament.Id)
                .Select(ttd => ttd.TeamId)
                .ToArray();

            return await _context.Teams.Where(t => !ids.Contains(t.Id)).OrderBy(t => t.Name).ToListAsync();
        }

        public async Task<IEnumerable<TournamentDivision>> GetTournamentDivisionsAsync()
        {
            var activeTournament = await GetActiveTournamentAsync();
            if (activeTournament == null)
            {
                throw new Exception("No active tournament found.");
            }
            var model = await _context.TournamentDivisions
            //.Include(td => td.Division)
                .Where(td => td.TournamentId == activeTournament.Id)
                                .ToListAsync();
            if (model.Count() > 0)
            {
                var ids = model.Select(td => td.DivisionId).ToArray();
                _context.Divisions.Where(d => ids.Contains(d.Id)).Load();
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
            var qry = _context.TournamentTeamDivisions
                .Where(ttd => ttd.TournamentId == activeTournament.Id);
            if (divisionId != Guid.Empty)
            {
                qry = qry.Where(ttd => ttd.DivisionId == divisionId);
            }
            var model = await qry.ToListAsync();
            if (model.Count() > 0)
            {
                var ids = model.Select(td => td.DivisionId).ToArray();
                _context.Divisions.Where(d => ids.Contains(d.Id)).Load();
                ids = model.Select(td => td.TeamId).ToArray();
                _context.Teams.Where(d => ids.Contains(d.Id)).Load();
                model = model.OrderBy(m => m.Division.Name).ThenBy(m => m.GroupName).ThenBy(m => m.Team.Name).ToList();
            }
            return model;
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
            var qry = _context.TournamentTeamDivisions
                .Where(ttd => ttd.TournamentId == activeTournament.Id);
            if (divisionId != Guid.Empty)
            {
                qry = qry.Where(ttd => ttd.DivisionId == divisionId);
            }
            return await qry.CountAsync();
        }

        public async Task<TournamentTeamDivision?> GetTeamAsync(Guid teamId)
        {
            var activeTournament = await GetActiveTournamentAsync();
            if (activeTournament == null)
            {
                throw new Exception("No active tournament found.");
            }
            var rrd = await _context.TournamentTeamDivisions.FirstOrDefaultAsync(t => t.TournamentId == activeTournament.Id && t.TeamId == teamId);
            /*if (rrd == null)
            {
                throw new Exception("Team is not in this Tournament");
            }*/
            if (rrd != null)
            {
                _context.Divisions.Where(d => d.Id == rrd.DivisionId).Load();
                _context.Teams.Where(d => d.Id == rrd.TeamId).Load();
            }
            return rrd;

        }
        public async Task RemoveTeamAsync(Guid teamId)
        {
            var activeTournament = await GetActiveTournamentAsync();
            if (activeTournament == null)
            {
                throw new Exception("No active tournament found.");
            }
            var tournamentTeamDivision = _context.TournamentTeamDivisions.FirstOrDefault(t => t.TournamentId == activeTournament.Id && t.TeamId == teamId);
            if (tournamentTeamDivision != null)
            {
                _context.TournamentTeamDivisions.Remove(tournamentTeamDivision);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<TournamentTeamDivision> AddTeamAsync(Guid teamId, Guid divisionId, string groupName, int seedNumber = 0, int rating = 0)
        {
            var activeTournament = await GetActiveTournamentAsync();
            if (activeTournament == null)
            {
                throw new Exception("No active tournament found.");
            }
            var tournamentTeamDivision = await _context.TournamentTeamDivisions.FirstOrDefaultAsync(t => t.TournamentId == activeTournament.Id && t.TeamId == teamId);
            if (tournamentTeamDivision == null)
            {
                tournamentTeamDivision = new TournamentTeamDivision();
                tournamentTeamDivision.TournamentId = activeTournament.Id;
                tournamentTeamDivision.TeamId = teamId;
                _context.TournamentTeamDivisions.Add(tournamentTeamDivision);
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
            tournamentTeamDivision.Rating = rating;
            tournamentTeamDivision.DivisionId = divisionId;
            tournamentTeamDivision.GroupName = groupName;
            tournamentTeamDivision.UpdatedAt = DateTime.Now;
            await _context.SaveChangesAsync();
            return tournamentTeamDivision;
        }
        public async Task<TournamentTeamDivision> SetTeamAsync(Guid teamId, Guid divisionId, string groupName, int seedNumber = 0, int rating = 0)
        {
            var activeTournament = await GetActiveTournamentAsync();
            if (activeTournament == null)
            {
                throw new Exception("No active tournament found.");
            }
            var tournamentTeamDivision = await _context.TournamentTeamDivisions.FirstOrDefaultAsync(t => t.TournamentId == activeTournament.Id && t.TeamId == teamId);
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
            tournamentTeamDivision.Rating = rating;
            tournamentTeamDivision.DivisionId = divisionId;
            tournamentTeamDivision.GroupName = groupName;
            tournamentTeamDivision.UpdatedAt = DateTime.Now;
            await _context.SaveChangesAsync();
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
                return await _context.TournamentTeamDivisions.CountAsync(t => t.TournamentId == activeTournament.Id && t.DivisionId == divisionId);

            }
            return await _context.TournamentTeamDivisions.CountAsync(t => t.TournamentId == activeTournament.Id);

        }
        public async Task<int> MatchCountAsync(MatchState matchState = MatchState.None,Guid? divisionId = null)
        {
            var activeTournament = await GetActiveTournamentAsync();
            if (activeTournament == null)
            {
                return 0;
            }
            var qry = _context.Matches.Where(m => m.TournamentId == activeTournament.Id);
            if (divisionId != null)
            {
                qry = qry.Where(m => m.DivisionId == divisionId);
            }
            if (matchState == MatchState.Finished)
            {
                qry = qry.Where(m => m.IsFinished);
            }
            if (matchState == MatchState.Disputed)
            {
                qry = qry.Where(m => m.IsDisputed);
            }
            if (matchState == MatchState.InProgress)
            {
                qry = qry.Where(m => m.IsFinished == false && m.CurrentSetNumber > 0);
            }
            return await qry.CountAsync();
        }
        private Task<int> GenerateSeedNumberAsync(Guid tournamentId, Guid divisionId)
        {
            int count = _context.TournamentTeamDivisions.Count(t => t.TournamentId == tournamentId);
            return Task.FromResult(count);
        }

        public async Task UpdateTournamentDivisionsAsync(List<Guid> selectedDivisionIds)
        {
            var activeTournament = await GetActiveTournamentAsync();
            if (activeTournament == null)
            {
                throw new Exception("No active tournament found.");
            }
            // Remove existing divisions
            var existingDivisions = await _context.TournamentDivisions
                .Where(td => td.TournamentId == activeTournament.Id)
                .ToListAsync();

            var defDivisions = existingDivisions.Where(d => !selectedDivisionIds.Contains(d.DivisionId)).ToList();
            if (defDivisions.Count > 0)
            {
                _context.TournamentDivisions.RemoveRange(defDivisions);
            }

            // Add selected divisions
            var tournament = await GetActiveTournamentAsync();
            if (tournament != null)
            {
                foreach (var divisionId in selectedDivisionIds)
                {
                    var division = await _context.Divisions.FindAsync(divisionId);
                    if (division != null)
                    {
                        _context.TournamentDivisions.Add(new TournamentDivision { TournamentId = activeTournament.Id, DivisionId = divisionId, Tournament = activeTournament, Division = division });
                    }
                }

                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<Match>> GetMatchesAsync(Guid? divisionId = null, Guid? roundId = null, string? groupName = null, Guid? teamId = null)
        {
            var activeTournament = await GetActiveTournamentAsync();
            if (activeTournament == null)
            {
                throw new Exception("No active tournament found.");
            }
            var qry = _context.Matches.Where(m => m.TournamentId == activeTournament.Id);
            if (divisionId != null)
            {
                qry = qry.Where(m => m.DivisionId == divisionId);
            }
            if (roundId != null)
            {
                qry = qry.Where(m => m.RoundTemplateId == roundId);
            }
            if (teamId != null)
            {
                qry = qry.Where(m => m.HomeTeamId == teamId || m.AwayTeamId == teamId);
            }
            if (string.IsNullOrEmpty(groupName) == false)
            {
                qry = qry.Where(m => m.GroupName == groupName);
            }
            var model = await qry.ToListAsync();
            if (model.Count() > 0)
            {
                var ids = model.Select(td => td.DivisionId).ToArray();
                _context.Divisions.Where(d => ids.Contains(d.Id)).Load();
                ids = model.Select(td => td.HomeTeamId).ToArray();
                ids = ids.Union(model.Select(td => td.AwayTeamId).ToArray()).ToArray();
                _context.Teams.Where(d => ids.Contains(d.Id)).Load();
                ids = model.Select(td => td.RoundTemplateId).ToArray();
                _context.RoundTemplates.Where(d => ids.Contains(d.Id)).Load();
            }
            return model.OrderBy(m => m?.Division?.Name).ThenBy(m => m?.Round?.Sequence).ThenBy(m => m.ScheduledTime).ToList();
        }

        public async Task<IEnumerable<Match>> RecentMatchesAsync(int howMany = 10)
        {
            var activeTournament = await GetActiveTournamentAsync();
            if (activeTournament == null)
            {
                throw new Exception("No active tournament found.");
            }
            if (howMany < 1)
            {
                howMany = 10;
            }
            return await _context.Matches
                .Include(m => m.HomeTeam)
                .Include(m => m.AwayTeam)
                .Include(m => m.Round)
                .Where(m => m.TournamentId == activeTournament.Id)
                .OrderByDescending(m => m.UpdatedAt)
                .ThenBy(m => m.ScheduledTime)
                .Take(howMany)
                .ToListAsync();
        }

        public async Task<DateTime> GetNextMatchStartTimeAsync()
        {
            var activeTournament = await GetActiveTournamentAsync();
            if (activeTournament != null)
            {
                var match = await _context.Matches
                    .Where(m => m.TournamentId == activeTournament.Id)
                .OrderByDescending(m => m.ScheduledTime)
                .FirstOrDefaultAsync();
                if (match == null)
                {
                    var dt1 = activeTournament.TournamentDate.Date;
                    dt1.AddHours(9);
                    return dt1;
                }
                var dt = match.ScheduledTime.AddMinutes(15);
                if (DateTime.Now > dt)
                {
                    dt = DateTime.Now.AddMinutes(30);
                }
                // Adding half the interval length before truncation helps with rounding to the nearest value
                var totalMinutes = (int)(dt.TimeOfDay.TotalMinutes + 15 / 2.0);

                // Perform integer division and multiplication to find the nearest multiple of 'minutes'
                var roundedMinutes = (totalMinutes - (totalMinutes % 15));
                dt = dt.Date;

                return dt.AddMinutes(roundedMinutes);
            }


            return DateTime.Now.AddMinutes(30);
        }
    }
}
