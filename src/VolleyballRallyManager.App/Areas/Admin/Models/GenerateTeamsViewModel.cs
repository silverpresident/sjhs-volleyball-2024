using System.ComponentModel.DataAnnotations;

namespace VolleyballRallyManager.App.Areas.Admin.Models;

public class GenerateTeamsViewModel
{
    [Required]
    [Range(1, 50, ErrorMessage = "Number of teams must be between 1 and 50")]
    [Display(Name = "Number of Teams")]
    public int NumberOfTeams { get; set; } = 10;
}
