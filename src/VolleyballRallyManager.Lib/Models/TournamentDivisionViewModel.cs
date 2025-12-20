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
        public int TotalMatches { get; set; }
       
        public int TeamCount { get; set; }
        public int CompletedMatches { get; set; }
        public required Division Division { get; set; }
    }
}
