using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace VolleyballRallyManager.Lib.Models;

public class Team : BaseEntity
{
    [Display(Name = "Name")]
    public required string Name { get; set; }

    [Display(Name = "School")]
    public required string School { get; set; }

    [Display(Name = "Color")]
    public required string Color { get; set; }

    [Display(Name = "Division")]
    public required Division Division { get; set; }

    [Display(Name = "Logo Url")]
    public string? LogoUrl { get; set; }

    // Statistics
    [Display(Name = "Matches Played")]
    public int MatchesPlayed { get; set; }

    [Display(Name = "Wins")]
    public int Wins { get; set; }

    [Display(Name = "Draws")]
    public int Draws { get; set; }

    [Display(Name = "Losses")]
    public int Losses { get; set; }

    [Display(Name = "Total Points")]
    public int TotalPoints { get; set; } = 0; // 3/1/0 added

    [Display(Name = "Points Scored")]
    public int PointsScored { get; set; } = 0;

    [Display(Name = "Points Conceded")]
    public int PointsConceded { get; set; } = 0;

    [Display(Name = "Point Difference")]
    public int PointDifference => PointsScored - PointsConceded;
   
    // Navigation properties
    public virtual ICollection<Match> HomeMatches { get; set; } = new List<Match>();
    public virtual ICollection<Match> AwayMatches { get; set; } = new List<Match>();
    public virtual ICollection<TournamentTeamDivision> TournamentTeamDivisions { get; set; } = new List<TournamentTeamDivision>();
}
