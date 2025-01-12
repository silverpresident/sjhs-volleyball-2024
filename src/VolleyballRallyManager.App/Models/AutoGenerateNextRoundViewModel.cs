using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace VolleyballRallyManager.App.Models
{
    public class AutoGenerateNextRoundViewModel
    {
        [Required]
        [Display(Name = "Division")]
        public Guid SelectedDivisionId { get; set; }
        public IEnumerable<SelectListItem> Divisions { get; set; }

        [Required]
        [Display(Name = "Teams to Advance")]
        public int TeamsToAdvance { get; set; }
        public List<SelectListItem> TeamsToAdvanceOptions { get; set; } = new List<SelectListItem>
        {
            new SelectListItem { Value = "2", Text = "2" },
            new SelectListItem { Value = "4", Text = "4" },
            new SelectListItem { Value = "8", Text = "8" },
            new SelectListItem { Value = "16", Text = "16" }
        };

        [Required]
        [Display(Name = "Selection Method")]
        public string SelectionMethod { get; set; }
        public List<SelectListItem> SelectionMethods { get; set; } = new List<SelectListItem>
        {
            new SelectListItem { Value = "TopFromEachGroup", Text = "Top from each group only" },
            new SelectListItem { Value = "TopFromGroupAndNextBest", Text = "Top from group and next best" },
            new SelectListItem { Value = "TopByPoints", Text = "Top by points" }
        };
    }
}
