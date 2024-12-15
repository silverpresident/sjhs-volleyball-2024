using System;
using System.Collections.Generic;

namespace VolleyballRallyManager.Lib.Models
{
    public class Division : BaseEntity
    {
        public string Name { get; set; }
        public virtual ICollection<Team> Teams { get; set; }
    }
}
