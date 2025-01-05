using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VolleyballRallyManager.Lib.Models
{
    public class TournamentTeamDivision : BaseEntity
    {
        [Required]
        public Guid TournamentId { get; set; }
        [ForeignKey("TournamentId")]
        public Tournament Tournament { get; set; }

        [Required]
        public Guid TeamId { get; set; }
        [ForeignKey("TeamId")]
        public Team Team { get; set; }

        [Required]
        public Guid DivisionId { get; set; }
        [ForeignKey("DivisionId")]
        [Display(Name = "Division")]
        public Division Division { get; set; }


        public string GroupName { get; set; }

        public int SeedNumber { get; set; } = 0;



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

    }
}
