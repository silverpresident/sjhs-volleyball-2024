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

    [Display(Name = "Logo Url")]
    public string? LogoUrl { get; set; }

    //public virtual ICollection<TournamentTeamDivision> TournamentTeamDivisions { get; set; } = new List<TournamentTeamDivision>();
}
