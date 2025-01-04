using System;

namespace VolleyballRallyManager.Lib.Models
{
    public class Tournament : BaseEntity
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime TournamentDate { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string CreatedBy { get; set; }
        public string UpdatedBy { get; set; }
        public bool IsActive { get; set; }
    }
}
