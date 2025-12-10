# Tournament Round Management - User Guide

## Overview

The Tournament Round Management feature allows you to organize multi-stage tournaments where teams progress through successive rounds based on their performance. This guide explains how to use the feature effectively.

## Accessing the Feature

Navigate to: **Admin** → **Tournament Rounds**

Or use the URL: `/Admin/TournamentRounds/Index`

## Key Concepts

### What is a Tournament Round?
A tournament round represents a stage in the competition where a group of teams compete against each other. After all matches in a round are completed and the round is finalized, the top-performing teams can advance to the next round.

### Important Terms
- **Seed Number**: The team's initial ranking entering a round (from previous round or division ranking)
- **Final Rank**: The team's final position after completing all matches in a round
- **Qualifying Teams Count**: Expected number of teams that will enter this round (from previous rounds or seeding)
- **Qualifying Team Selection Strategy**: How teams are selected to enter this round
- **Advancing Teams Count**: Number of teams that will advance to the next round
- **Advancing Team Selection Strategy**: How teams are selected to advance from this round to the next
- **Is Playoff**: Indicates whether this is a playoff round (affects ranking and progression)
- **Round Status**:
  - **In Progress**: Round is active, matches being played
  - **Finished**: All matches complete, rankings calculated
  - **Locked**: Round is closed, cannot be modified

## Workflow: Creating Your First Round

### Step 1: Create First Round

1. Click **"Create First Round"** button on the main page
2. Fill in the form:
   - **Round**: Select which round this is (e.g., "Group Stage", "Round 1")
   - **Teams Advancing**: How many teams will advance (e.g., 8)
   - **Advancing Team Selection Strategy**: How teams will be selected for next round
   - **Match Generation Strategy**: How matches will be created
   - **Is Playoff Round** (checkbox): Check if this is a playoff round

3. Click **"Create First Round"**

**What Happens:**
- All teams from the selected division are automatically included
- Teams are seeded based on their division seed numbers
- You're redirected to generate matches

### Step 2: Generate Matches

1. Set the **Start Time** (when first match begins)
2. Choose **Default Court Location**
3. Review match generation details
4. Click **"Generate Matches"**

**Match Generation Strategies:**

- **Round Robin**: Every team plays every other team once
  - Best for: Group stages, leagues
  - Example: 8 teams = 28 matches

- **Seeded Bracket**: Elimination bracket with seeding
  - Best for: Knockout rounds, finals
  - Example: 8 teams = 4 matches (quarterfinals)
  - Seeding: 1 vs 8, 2 vs 7, 3 vs 6, 4 vs 5

**What Happens:**
- Matches are created with 15-minute intervals
- You're redirected to the round details page
- Matches can be edited individually if needed

### Step 3: Record Match Results

1. Navigate to each match from the round details page
2. Record scores for each set
3. Mark matches as complete

**Tip:** You can record match results through the existing Match Management interface.

### Step 4: Finalize the Round

Once all matches are complete:

1. Click **"Finalize Round"** button
2. Confirm the action

**What Happens:**
- Team rankings are calculated using the strict tie-breaker system
- Round is marked as "Finished" and "Locked"
- Round cannot be modified after finalization
- You can now create the next round

## Creating Subsequent Rounds

### Step 5: Generate Next Round

1. Click **"Generate Next Round"** on a finished round
2. Fill in the form:
   - **Round**: Select the next round (e.g., "Quarter Finals")
   - **Qualifying Teams Count**: Expected number of teams entering this round
   - **Qualifying Team Selection Strategy**: How teams qualify for this round
   - **Advancing Teams Count**: How many teams advance from this round to the next
   - **Advancing Team Selection Strategy**: How teams are selected to advance
   - **Match Generation Strategy**: Match format
   - **Is Playoff Round** (checkbox): Check if this is a playoff round

3. Click **"Create Next Round"**

**What Happens:**
- New round is created
- Teams are automatically selected based on your selection method
- Previous round is locked
- You're redirected to generate matches for the new round

### Step 6: Repeat

Repeat steps 2-5 for each subsequent round until you crown a champion!

## Team Selection Methods Explained

### 1. Top By Points
Selects teams with the highest point totals from the previous round.

**Example:** If you set "Teams Advancing" to 4, the top 4 teams by points advance.

**Best for:** Fair progression based on overall performance

### 2. Seed Top Half
Selects the top 50% of teams, capped by "Teams Advancing".

**Example:** 16 teams → Top 8 qualify (if Teams Advancing ≥ 8)

**Best for:** Reducing field size consistently

### 3. Winners Only
Only teams that achieved **Rank 1** in their group qualify.

**Example:** 4 groups → 4 group winners advance (max = Teams Advancing)

**Best for:** Group stage to knockout progression

### 4. Top From Group And Next Best
Takes the top team from each group, plus the best remaining teams overall.

**Example:** 4 groups + 4 best runners-up = 8 teams qualify

**Best for:** Fair representation from all groups

### 5. Manual
You manually select which teams advance through the UI.

**Best for:** Special circumstances, non-standard progressions

## Ranking System

### How Teams Are Ranked

Teams in each round are ranked using this strict hierarchy:

1. **Points** (highest first)
   - Win = 3 points
   - Draw = 1 point
   - Loss = 0 points

2. **Score Difference** (highest first)
   - ScoreFor - ScoreAgainst

3. **Score For** (highest first)
   - Total points scored

4. **Seed Number** (lowest first)
   - Original seeding breaks ties

### Statistics Tracked Per Round

For each team in a round:
- Matches Played
- Wins, Draws, Losses
- Points
- Sets For / Sets Against / Set Difference
- Score For / Score Against / Score Difference
- Final Rank (calculated when round is finalized)

## Understanding the Round Details Page

### Round Information Card
- Tournament and Division
- Round Number
- Qualifying Team Selection Strategy (how teams enter this round)
- Advancing Team Selection Strategy (how teams advance from this round)
- Match Generation Strategy
- Qualifying Teams Count
- Teams Advancing
- Is Playoff Round indicator
- Current Status

### Round Statistics Card
- Total Teams
- Total Matches
- Completed/Pending Matches
- Completion Percentage (visual progress bar)

### Team Rankings Table
Shows all teams with:
- Seed Number (entry rank)
- Final Rank (calculated rank)
- Complete statistics
- Color coding:
  - 🥇 Gold badge: Rank 1
  - 🥈 Blue badge: Ranks 2-3
  - Gray badge: Other ranks

### Matches List
Shows all matches with:
- Match number
- Teams
- Score (if finished)
- Court and time
- Status (Complete/Pending)
- Link to match details

## Action Buttons Explained

The system shows different action buttons based on round state:

### Details Button
- **Always visible**
- Shows complete round information, team rankings, and matches

### Select Teams Button
- **Visible when:** No teams assigned AND previous round is finished
- Automatically selects and seeds teams for the round

### Generate Matches Button
- **Visible when:** Teams assigned but no matches yet
- Creates match fixtures based on the round strategy

### Finalize Round Button
- **Visible when:** All matches complete but round not finalized
- Calculates final rankings and locks the round

### Generate Next Round Button
- **Visible when:** Round is finished
- Creates the subsequent round in the tournament

## Tips & Best Practices

### Planning Your Tournament

1. **Decide Round Structure First**
   - How many rounds?
   - What format for each round?
   - How many teams advance each time?

2. **Choose Appropriate Strategies**
   - Group stage: Round Robin
   - Knockout: Seeded Bracket
   - Mix strategies as needed

3. **Set Realistic Teams Advancing**
   - Must divide evenly for bracket formats
   - Common: 16 → 8 → 4 → 2 → 1 (champion)

### During the Tournament

1. **Complete Rounds Sequentially**
   - Finish all matches before finalizing
   - Finalize before creating next round

2. **Check Rankings Before Finalizing**
   - Review team standings
   - Ensure all scores are correct
   - Rankings are permanent after finalization

3. **Document Special Decisions**
   - Note any manual team selections
   - Keep track of tie-breaker outcomes

### Troubleshooting

**Q: Can't finalize round?**
- A: Ensure ALL matches are marked complete

**Q: Select Teams button not showing?**
- A: Check that previous round is finalized

**Q: Wrong teams selected?**
- A: You cannot undo team selection. Create a new round.

**Q: Need to change match results?**
- A: Edit the match before finalizing the round

**Q: Round finalized by mistake?**
- A: Rounds cannot be un-finalized. Contact system administrator.

## Understanding Playoff Rounds

### What is a Playoff Round?
A playoff round is a special type of round where teams compete in an elimination or high-stakes format. Marking a round as "Playoff" helps:
- Identify knockout stages in your tournament structure
- Apply special ranking considerations
- Organize tournament reporting and statistics

### When to Mark as Playoff
- **Quarterfinals**, **Semifinals**, **Finals**: Always mark as playoff
- **Third Place Match**: Mark as playoff
- **Elimination Brackets**: Mark as playoff
- **Group Stages**: Do NOT mark as playoff

### Qualifying vs. Advancing Teams

Understanding the difference is crucial for complex round structures:

**Qualifying Team Selection Strategy**:
- Controls how teams ENTER this round
- Used when creating the round to select teams from previous rounds
- Example: "Top 8 by points from Round 1 qualify for Round 2"

**Advancing Team Selection Strategy**:
- Controls how teams LEAVE this round for the next one
- Used when creating the NEXT round
- Example: "Winners of Round 2 matches advance to Round 3"

## Common Tournament Formats

### Format 1: Group Stage + Knockout

1. **Round 1**: Group Stage (Round Robin)
   - All teams play in groups
   - Top 2 from each group advance
   - Is Playoff: **No**
   - Advancing Strategy: TopFromGroupAndNextBest

2. **Round 2**: Quarter Finals (Seeded Bracket)
   - Qualifying Count: 8 teams
   - Qualifying Strategy: TopFromGroupAndNextBest (from Round 1)
   - 8 teams → 4 winners
   - Seed by group standings
   - Is Playoff: **Yes**
   - Advancing Strategy: WinnersOnly

3. **Round 3**: Semi Finals (Seeded Bracket)
   - Qualifying Count: 4 teams
   - Qualifying Strategy: WinnersOnly (from Round 2)
   - 4 teams → 2 winners
   - Is Playoff: **Yes**
   - Advancing Strategy: WinnersOnly

4. **Round 4**: Finals (Seeded Bracket)
   - Qualifying Count: 2 teams
   - Qualifying Strategy: WinnersOnly (from Round 3)
   - 2 teams → 1 champion
   - Is Playoff: **Yes**

### Format 2: Progressive Elimination

1. **Round 1**: Round Robin (16 teams)
   - Top 8 by points qualify

2. **Round 2**: Round Robin (8 teams)
   - Top 4 by points qualify

3. **Round 3**: Semi Finals
   - 1 vs 4, 2 vs 3

4. **Round 4**: Finals
   - Winners compete for championship

### Format 3: Swiss System

1. Multiple rounds of Swiss pairings
2. Team selection: Top By Points
3. Match strategy: Manual (or custom implementation)

## Keyboard Shortcuts

- **Alt + N**: Create first round (when on index page)
- **Alt + D**: View round details (when round selected)
- **Alt + F**: Finalize round (when button visible)

## Getting Help

If you encounter issues:

1. Check this user guide
2. Review `TOURNAMENT_ROUNDS_IMPLEMENTATION.md` for technical details
3. Contact system administrator
4. Report bugs through the proper channels

## Summary

The Tournament Round Management feature provides a powerful, flexible system for organizing multi-stage tournaments. By following this guide, you can:

- Create and manage rounds efficiently
- Track team performance accurately
- Ensure fair progression through the tournament
- Maintain complete statistics for each stage

Remember: **Finalization is permanent!** Always double-check results before finalizing rounds.

---

**Version:** 1.0  
**Last Updated:** December 2025  
**For Support:** Contact your system administrator
