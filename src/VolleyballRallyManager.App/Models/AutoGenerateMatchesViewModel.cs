using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;

namespace VolleyballRallyManager.App.Models
{
    public class AutoGenerateMatchesViewModel
    {
        public List<SelectListItem> Divisions { get; set; }
        public List<SelectListItem> Groups { get; set; }
        public List<Guid> SelectedDivisionIds { get; set; } = new List<Guid>();
        public List<string> SelectedGroupNames { get; set; } = new List<string>();
        public bool RemoveExistingMatches { get; set; }
    }
}
