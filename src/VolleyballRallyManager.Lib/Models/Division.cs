using System.ComponentModel.DataAnnotations;

namespace VolleyballRallyManager.Lib.Models
{
    public class Division : BaseEntity
    {
        [Display(Name = "Name")]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "Tournament Team Divisions")]
        public virtual ICollection<TournamentTeamDivision> TournamentTeamDivisions { get; set; } = new List<TournamentTeamDivision>();
    }
}
