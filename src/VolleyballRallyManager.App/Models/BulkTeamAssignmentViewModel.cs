using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using VolleyballRallyManager.Lib.Models;

namespace VolleyballRallyManager.App.Models
{
    public class BulkTeamAssignmentViewModel
    {
        [ValidateNever]
        public Tournament? ActiveTournament { get; set; }
        
        [ValidateNever]
        public IEnumerable<Division> AvailableDivisions { get; set; } = new List<Division>();
        
        [ValidateNever]
        public IEnumerable<TeamAssignmentItem> Teams { get; set; } = new List<TeamAssignmentItem>();
    }

    public class TeamAssignmentItem
    {
        public Guid TeamId { get; set; }
        public string TeamName { get; set; } = string.Empty;
        public string School { get; set; } = string.Empty;
        public Guid? CurrentDivisionId { get; set; }
        public string? CurrentDivisionName { get; set; }
        public int CurrentSeedNumber { get; set; }
        public bool IsAssigned { get; set; }
    }

    public class BulkTeamAssignmentUpdateModel
    {
        public Guid TeamId { get; set; }
        public Guid? DivisionId { get; set; }
        public int SeedNumber { get; set; }
    }
}
