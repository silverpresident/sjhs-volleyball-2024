using System.ComponentModel.DataAnnotations;
using VolleyballRallyManager.Lib.Models;

namespace VolleyballRallyManager.App.Areas.Admin.Models;

/// <summary>
/// ViewModel for selecting qualifying teams from a completed tournament round
/// </summary>
public class SelectTeamsViewModel
{
    public Guid TournamentRoundId { get; set; }
    
    [Display(Name = "Round Name")]
    public string RoundName { get; set; } = string.Empty;
    
    [Display(Name = "Team Selection Strategy")]
    public TeamSelectionStrategy TeamSelectionStrategy { get; set; }
    
    [Display(Name = "Number of Qualifying Teams")]
    public int NumberOfQualifyingTeams { get; set; }
    
    [Display(Name = "Teams")]
    public List<TeamSelectionItem> Teams { get; set; } = new();
    
    /// <summary>
    /// Nested class representing a team that can be selected
    /// </summary>
    public class TeamSelectionItem
    {
        public Guid TeamId { get; set; }
        
        [Display(Name = "Team Name")]
        public string TeamName { get; set; } = string.Empty;
        
        [Display(Name = "Rank")]
        public int Rank { get; set; }
        
        [Display(Name = "Group")]
        public string GroupName { get; set; } = string.Empty;
        
        [Display(Name = "Sets Won")]
        public int SetsWon { get; set; }
        
        [Display(Name = "Sets Lost")]
        public int SetsLost { get; set; }
        
        [Display(Name = "Points")]
        public int Points { get; set; }
        
        [Display(Name = "Point Difference")]
        public int PointDifference { get; set; }
        
        [Display(Name = "Selected")]
        public bool IsSelected { get; set; }
    }
}
