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
        public Division Division { get; set; }

        public string Group { get; set; }

        public int SeedNumber { get; set; }
    }
}
