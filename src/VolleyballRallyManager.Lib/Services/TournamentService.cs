using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

        public async Task SetActiveTournament(Guid tournamentId)
        {
            // Implementation not provided
            await Task.CompletedTask;
        }


        public async Task<IEnumerable<Division>> GetTournamentDivisions(Guid tournamentId)
        {
            return await _context.TournamentDivisions
                .Where(td => td.TournamentId == tournamentId)
                .Select(td => td.Division)
                .ToListAsync();
        }


        public async Task<IEnumerable<Team>> GetTournamentTeams(Guid tournamentId)
        {
            return await _context.TournamentDivisions
                .Where(td => td.TournamentId == tournamentId)
                .Select(td => td.Division.Teams)
                .SelectMany(teams => teams)
                .ToListAsync();
        }

        public async Task<IEnumerable<Match>> GetTournamentMatches(Guid tournamentId)
        {
            return await _context.Matches
                .Where(m => m.TournamentId == tournamentId)
                .ToListAsync();
        }

        public async Task AddDivisionToTournament(Guid tournamentId, Guid divisionId)
        {
            var tournamentDivision = new TournamentDivision
            {
                TournamentId = tournamentId,
                DivisionId = divisionId
            };
            _context.TournamentDivisions.Add(tournamentDivision);
            await _context.SaveChangesAsync();
        }

        public async Task RemoveDivisionFromTournament(Guid tournamentId, Guid divisionId)
        {
            var tournamentDivision = await _context.TournamentDivisions
                .FirstOrDefaultAsync(td => td.TournamentId == tournamentId && td.DivisionId == divisionId);
            if (tournamentDivision != null)
            {
                _context.TournamentDivisions.Remove(tournamentDivision);
                await _context.SaveChangesAsync();
            }
        }
    }
}
