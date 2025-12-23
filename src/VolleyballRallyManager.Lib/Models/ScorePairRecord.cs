using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using VolleyballRallyManager.Lib.Common;

namespace VolleyballRallyManager.Lib.Models;

public class ScorePairRecord 
{
    
    [Display(Name = "Home Team Score")]
    public int HomeTeamScore { get; set; } = 0;

    [Display(Name = "Away Team Score")]
    public int AwayTeamScore { get; set; } = 0; 
}
