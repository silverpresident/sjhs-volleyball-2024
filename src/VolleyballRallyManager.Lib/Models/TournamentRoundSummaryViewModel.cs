using VolleyballRallyManager.Lib.Models;

namespace VolleyballRallyManager.Lib.Models
{
    /// <summary>
    /// ViewModel for round statistics within a tournament division
    /// ViewModel for displaying a tournament round in the index
    /// </summary>


    public class TournamentRoundSummaryViewModel: ITournamentRoundButtonState
    {
        public Guid TournamentId { get; set; }
        public Guid TournamentRoundId { get; set; }
        public Guid DivisionId { get; set; }
        public string TournamentName { get; set; } = string.Empty;
        public string DivisionName { get; set; } = string.Empty;
        public string RoundName { get; set; } = string.Empty;
        public int RoundNumber { get; set; }
        public int TeamCount { get; set; }
        public int CompletedMatches { get; set; }
        public int TotalMatches { get; set; }
        public bool IsPlayoff { get; set; }
        public bool IsFinished { get; set; }
        public bool IsLocked { get; set; }
        public TeamSelectionStrategy AdvancingTeamSelectionStrategy { get; set; }
        public MatchGenerationStrategy MatchGenerationStrategy { get; set; }
        public int AdvancingTeamsCount { get; set; }


        // Conditional button visibility flags
        public bool CanFinalize { get; set; }
        public bool CanSelectTeams { get; set; }
        public bool CanGenerateMatches { get; set; }
        public bool CanGenerateNextRound { get; set; }
        public bool ShowCreatePlayoffRound { get; set; }

        /// <summary>
        /// Calculates the completion percentage for this round
        /// </summary>
        public int CompletionPercentage => TotalMatches > 0
            ? (int)((double)CompletedMatches / TotalMatches * 100)
            : 0;
        /// <summary>
        /// Calculates the Pending Matches for this round
        /// </summary>
        public int PendingMatches => TotalMatches > 0
            ? TotalMatches - CompletedMatches
            : 0;
    }
}
