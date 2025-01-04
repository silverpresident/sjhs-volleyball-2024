namespace VolleyballRallyManager.Lib.Models;

public class Team : BaseEntity
{
    public required string Name { get; set; }
    public required string School { get; set; }
    public required string Color { get; set; }
    public required Division Division { get; set; }
    public string? LogoUrl { get; set; }

    // Statistics
    public int MatchesPlayed { get; set; }
    public int Wins { get; set; }
    public int Draws { get; set; }
    public int Losses { get; set; }
    public int TotalPoints { get; set; } = 0; // 3/1/0 added
    public int PointsScored { get; set; } = 0;
    public int PointsConceded { get; set; } = 0;
    public int PointDifference => PointsScored - PointsConceded;
   
    // Navigation properties
    public virtual ICollection<Match> HomeMatches { get; set; } = new List<Match>();
    public virtual ICollection<Match> AwayMatches { get; set; } = new List<Match>();
    public virtual ICollection<TournamentTeamDivision> TournamentTeamDivisions { get; set; } = new List<TournamentTeamDivision>();
}
