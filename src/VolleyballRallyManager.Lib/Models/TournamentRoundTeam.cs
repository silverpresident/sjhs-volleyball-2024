using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VolleyballRallyManager.Lib.Models;

/// <summary>
/// Represents a team's participation in a specific tournament round
/// Tracks seeding, final ranking, and round-specific statistics
/// </summary>
public class TournamentRoundTeam : BaseEntity
{
    // Primary Identifiers
    [Required]
    [Display(Name = "Tournament")]
    public Guid TournamentId { get; set; }
    
    [ForeignKey("TournamentId")]
    public Tournament Tournament { get; set; } = null!;

    [Required]
    [Display(Name = "Division")]
    public Guid DivisionId { get; set; }
    
    [ForeignKey("DivisionId")]
    public Division Division { get; set; } = null!;

    [Required]
    [Display(Name = "Round")]
    public Guid RoundTemplateId { get; set; }
    
    [ForeignKey("RoundTemplateId")]
    public RoundTemplate Round { get; set; } = null!;

    [Required]
    [Display(Name = "Team")]
    public Guid TeamId { get; set; }
    
    [ForeignKey("TeamId")]
    public Team Team { get; set; } = null!;

    [Required]
    [Display(Name = "Tournament Round")]
    public Guid TournamentRoundId { get; set; }
    
    [ForeignKey("TournamentRoundId")]
    public TournamentRound TournamentRound { get; set; } = null!;

    // Ranking Properties
    [Required]
    [Display(Name = "Seed Number")]
    public int SeedNumber { get; set; } = 0;

    [Display(Name = "Final Rank")]
    public int Rank { get; set; } = 0;
    [Display(Name = "Ranking Points")]
    public int RankingPoints { get; set; } = 0;

    // Statistics
    [Display(Name = "Points")]
    public int Points { get; set; } = 0;

    [Display(Name = "Matches Played")]
    public int MatchesPlayed { get; set; } = 0;

    [Display(Name = "Wins")]
    public int Wins { get; set; } = 0;

    [Display(Name = "Draws")]
    public int Draws { get; set; } = 0;

    [Display(Name = "Losses")]
    public int Losses { get; set; } = 0;

    [Display(Name = "Sets For")]
    public int SetsFor { get; set; } = 0;

    [Display(Name = "Sets Against")]
    public int SetsAgainst { get; set; } = 0;

    [Display(Name = "Sets Difference")]
    public int SetsDifference => SetsFor - SetsAgainst;

    [Display(Name = "Score For")]
    public int ScoreFor { get; set; } = 0;

    [Display(Name = "Score Against")]
    public int ScoreAgainst { get; set; } = 0;

    [Display(Name = "Score Difference")]
    public int ScoreDifference => ScoreFor - ScoreAgainst;

    // Additional properties from Team
    [Display(Name = "Group Name")]
    public string GroupName { get; set; } = string.Empty;
}
