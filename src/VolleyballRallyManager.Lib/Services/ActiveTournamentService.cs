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
            return await _dbContext.Tournaments
                .FirstOrDefaultAsync(t => t.IsActive);
        }

        public async Task<IEnumerable<TournamentDivision>> GetTournamentDivisionsAsync(Guid tournamentId)
        {
            return await _dbContext.TournamentDivisions
                .Where(td => td.TournamentId == tournamentId)
                .ToListAsync();
        }

        public async Task<IEnumerable<TournamentTeamDivision>> GetTournamentTeamsAsync(Guid tournamentId)
        {
            return await _dbContext.TournamentTeamDivisions
                .Where(ttd => ttd.TournamentId == tournamentId)
                .ToListAsync();
        }
    }
}
