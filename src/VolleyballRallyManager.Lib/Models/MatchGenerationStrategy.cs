namespace VolleyballRallyManager.Lib.Models;

/// <summary>
/// Defines the strategy for generating matches within a round
/// </summary>
public enum MatchGenerationStrategy
{
    /// <summary>
    /// Matches are created manually by the tournament administrator
    /// </summary>
    Manual,
    
    /// <summary>
    /// CurrentRound-robin format where each team plays every other team once
    /// </summary>
    RoundRobin,
    
    /// <summary>
    /// Seeded single-elimination bracket (1 vs Last, 2 vs Second-to-last, etc.)
    /// </summary>
    SeededBracket,
    
    /// <summary>
    /// Swiss system tournament format
    /// </summary>
    Swiss,
    
    /// <summary>
    /// Group stage followed by knockout rounds
    /// </summary>
    GroupStageKnockout
}
