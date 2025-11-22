using System.ComponentModel.DataAnnotations;

namespace VolleyballRallyManager.Lib.Models
{
    public class Tournament : BaseEntity
    {
        [Display(Name = "Name")]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "Description")]
        public string Description { get; set; } = string.Empty;

        [Display(Name = "Tournament Date")]
        public DateTime TournamentDate { get; set; }

        [Display(Name = "Is Active")]
        public bool IsActive { get; set; } = false;

        public ICollection<TournamentDivision> TournamentDivisions { get; set; } = new List<TournamentDivision>();
        public ICollection<TournamentTeamDivision> TournamentTeamDivisions { get; set; } = new List<TournamentTeamDivision>();
    }
}
