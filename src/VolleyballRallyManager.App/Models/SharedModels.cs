using System.ComponentModel.DataAnnotations;
using VolleyballRallyManager.Lib.Models;

namespace VolleyballRallyManager.App.Models;

public class MatchListItemViewModel
{
    public Guid Id { get; set; }
    public int MatchNumber { get; set; }
    public string RoundName { get; set; } = string.Empty;
    public DateTime ScheduledTime { get; set; }
    public DateTime? ActualStartTime { get; set; }
    public string CourtLocation { get; set; } = string.Empty;
    public string HomeTeamName { get; set; } = string.Empty;
    public string AwayTeamName { get; set; } = string.Empty;
    public int HomeTeamScore { get; set; }
    public int AwayTeamScore { get; set; }
    public bool IsFinished { get; set; }
    public bool IsDisputed { get; set; }
    public string? RefereeName { get; set; }
    public string? ScorerName { get; set; }
}

public class TeamListItemViewModel
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string School { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty;
    public string DivisionName { get; set; } = string.Empty;
    public string? LogoUrl { get; set; }
    public int MatchesPlayed { get; set; }
    public int Wins { get; set; }
    public int Draws { get; set; }
    public int Losses { get; set; }
    public int PointsScored { get; set; }
    public int PointsConceded { get; set; }
    public int TotalPoints { get; set; }
}

public class AnnouncementListItemViewModel
{
    public Guid Id { get; set; }
    public string Content { get; set; } = string.Empty;
    public string RenderedContent { get; set; } = string.Empty;
    public bool UseMarkdown { get; set; }
    public AnnouncementPriority Priority { get; set; }
    public bool IsVisible { get; set; }
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
}

public class MatchUpdateListItemViewModel
{
    public Guid Id { get; set; }
    public string Content { get; set; } = string.Empty;
    public UpdateType UpdateType { get; set; }
    public string? PreviousValue { get; set; }
    public string? NewValue { get; set; }
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
}

public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public T? Data { get; set; }
    public List<string>? Errors { get; set; }
}

public class PaginatedList<T>
{
    public List<T> Items { get; set; } = new();
    public int PageIndex { get; set; }
    public int TotalPages { get; set; }
    public int TotalCount { get; set; }
    public bool HasPreviousPage => PageIndex > 1;
    public bool HasNextPage => PageIndex < TotalPages;
}
