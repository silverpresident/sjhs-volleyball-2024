using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using VolleyballRallyManager.Lib.Models;

namespace VolleyballRallyManager.App.Models
{
    public class SelectDivisionsViewModel
    {
        [ValidateNever]
        public Tournament? Tournament { get; set; }
        [ValidateNever]
        public IEnumerable<Division> AvailableDivisions { get; set; } = new List<Division>();
        public List<Guid> SelectedDivisionIds { get; set; } = new List<Guid>();

    }
}
