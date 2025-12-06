namespace VolleyballRallyManager.Lib.Models
{
    /// <summary>
    /// ViewModel for division statistics within a tournament
    /// </summary>
    public class TournamentDivisionViewModel
    {
        public Guid DivisionId { get; set; }
        public string DivisionName { get; set; } = string.Empty;
        public List<string> GroupNames { get; set; } = new List<string>(); 
        public int RoundsCount { get; set; } 
        public int MatchCount { get; set; }
       
        public int TeamCount { get; set; }
        public int MatchesPlayed { get; set; }
        public Division Division { get; set; }
    }
}
