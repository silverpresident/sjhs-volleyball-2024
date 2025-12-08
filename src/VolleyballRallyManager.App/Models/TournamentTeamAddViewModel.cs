using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using VolleyballRallyManager.Lib.Models;

namespace VolleyballRallyManager.App.Models
{
    public class TournamentTeamAddViewModel
    {
        [ValidateNever]
        public Tournament? ActiveTournament { get; set; }
        [ValidateNever]
        public IEnumerable<Team> AvailableTeams { get; set; } = new List<Team>();
        [ValidateNever]
        public IEnumerable<Division> AvailableDivisions { get; set; } = new List<Division>();
        [Display(Name = "Team")]
        [Required]

        public Guid TeamId { get; set; }
        [Display(Name = "Division")]
        [Required]
        public Guid DivisionId { get; set; }
        public Guid TournamentId { get; set; }
        [MaxLength(10)]
        [Display(Name = "Group")]
        public string? GroupName { get; set; }
        [Display(Name = "Entry Rating")]
        [DefaultValue(0)]
        [Range(0, 9000)]
        public int Rating { get; set; } = 0;
        [Display(Name = "Seed Number")]
        [DefaultValue(0)]
        [Range(0, 100)]
        public int SeedNumber { get; set; } = 0;
        [Display(Name = "Team")]
        [ValidateNever]
        public string? TeamName { get; set; }
        

    }
}
