using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VolleyballRallyManager.Lib.Models
{
    public class TournamentTeamDivision : BaseEntity
    {
        [Required]
        public Guid TournamentId { get; set; }
        [ForeignKey("TournamentId")]
        public Tournament Tournament { get; set; } = null!;

        [Required]
        public Guid TeamId { get; set; }
        [ForeignKey("TeamId")]
        public Team Team { get; set; } = null!;

        [Required]
        public Guid DivisionId { get; set; }
        [ForeignKey("DivisionId")]
        [Display(Name = "Division")]
        public Division Division { get; set; } = null!;


        public string GroupName { get; set; } = null!;

        [Display(Name = "Entry Rating")] 
        //Rating points coming into competetion
        public int Rating { get; set; } = 0;

        [Display(Name = "Seed Number")]
        public int SeedNumber { get; set; } = 0;

        [Display(Name = "Final Rank")]
        public int Rank { get; set; } = 0;
        [Display(Name = "Ranking Points")]
        public int RankingPoints { get; set; } = 0;


        // Statistics
        [Display(Name = "Matches Played")]
        public int MatchesPlayed { get; set; } = 0;

        [Display(Name = "Wins")]
        public int Wins { get; set; } = 0;

        [Display(Name = "Draws")]
        public int Draws { get; set; } = 0;

        [Display(Name = "Losses")]
        public int Losses { get; set; } = 0;

        [Display(Name = "Total Points")]
        public int TotalPoints { get; set; } = 0; // 3/1/0 added u across rounds

       

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
        

        // Navigation properties
        // public virtual ICollection<Match> HomeMatches { get; set; } = new List<Match>();
        //  public virtual ICollection<Match> AwayMatches { get; set; } = new List<Match>();

    }
}
