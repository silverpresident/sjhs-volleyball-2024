namespace VolleyballRallyManager.Lib.Models;

/// <summary>
/// Defines the method for selecting teams to advance to the next round
/// </summary>
public enum TeamSelectionMethod
{
    /// <summary>
    /// Teams are manually selected by the tournament administrator
    /// </summary>
    Manual,
    
    /// <summary>
    /// Top half of teams by ranking advance (e.g., top 8 from 16 teams)
    /// </summary>
    SeedTopHalf,
    
    /// <summary>
    /// Only winners (teams with FinalRank = 1) from each group advance
    /// </summary>
    WinnersOnly,
    
    /// <summary>
    /// Top team from each group plus next best teams by points
    /// </summary>
    TopFromGroupAndNextBest,
    
    /// <summary>
    /// Top teams overall by total points, regardless of group
    /// </summary>
    TopByPoints
}
