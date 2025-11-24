using VolleyballRallyManager.Lib.Models;

namespace VolleyballRallyManager.App.Areas.Admin.Models;

public class MatchDetailsViewModel
{
    public Match Match { get; set; } = null!;
    public List<MatchSet> Sets { get; set; } = new List<MatchSet>();
    public List<MatchUpdate> Updates { get; set; } = new List<MatchUpdate>();
}
