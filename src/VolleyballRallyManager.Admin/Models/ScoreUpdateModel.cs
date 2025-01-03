using System.ComponentModel.DataAnnotations;

namespace VolleyballRallyManager.App.Models;

public class ScoreUpdateModel
{
    [Required]
    public Guid MatchId { get; set; }

    [Required]
    [Range(0, 999, ErrorMessage = "Score must be between 0 and 999")]
    public int HomeTeamScore { get; set; }

    [Required]
    [Range(0, 999, ErrorMessage = "Score must be between 0 and 999")]
    public int AwayTeamScore { get; set; }

    public bool IsFinished { get; set; }

    public bool IsDisputed { get; set; }

    [StringLength(500, ErrorMessage = "Update notes cannot exceed 500 characters")]
    public string? Notes { get; set; }
}
