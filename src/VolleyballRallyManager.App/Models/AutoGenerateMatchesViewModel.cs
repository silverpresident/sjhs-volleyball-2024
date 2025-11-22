using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;

namespace VolleyballRallyManager.App.Models
{
    public class AutoGenerateMatchesViewModel
    {
        public List<SelectListItem> Divisions { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> Groups { get; set; } = new List<SelectListItem>();
        public List<Guid> SelectedDivisionIds { get; set; } = new List<Guid>();
        public List<string> SelectedGroupNames { get; set; } = new List<string>();
        public bool RemoveExistingMatches { get; set; }
    }
}
