Let's work on the TournamentRounds model. 
- I wish to specify the QualifyingTeamsCount (this is the number of teams coming in to the round)
- add a QualifyingTeamSelectionStrategy (type: TeamSelectionStrategy) stored in db as string
- add a IsPlayoff bool property
- generate the required TSQL for the database change.

Views and actions
- Update the relevant views, actions and services (CreateFirstRound, GenerateNextRound, Edit) to include these values.

- Add a new column `QualifyingTeamsCount` to the `TournamentRounds` table with a default value of 0.
- Add a new column `QualifyingTeamSelectionStrategy` to the `TournamentRounds` table with a default value of 'Random'.
- Add a new column `IsPlayoff` to the `TournamentRounds` table with a default value of 0 (false).

```sql
-- Add new columns to the TournamentRounds table
ALTER TABLE TournamentRounds
ADD
    QualifyingTeamsCount INT NOT NULL DEFAULT 0,
    QualifyingTeamSelectionStrategy NVARCHAR(50) NOT NULL DEFAULT 'Random',
    IsPlayoff BIT NOT NULL DEFAULT 0;

-- Update the default values for the new columns
UPDATE TournamentRounds
SET
    QualifyingTeamsCount = 0,
    QualifyingTeamSelectionStrategy = 'Manual',
    IsPlayoff = 0;

-- Add a check constraint for QualifyingTeamsCount
ALTER TABLE TournamentRounds
ADD CONSTRAINT CK_TournamentRounds_QualifyingTeamsCount CHECK (QualifyingTeamsCount >= 0);

-- Add a check constraint for IsPlayoff
ALTER TABLE TournamentRounds
ADD CONSTRAINT CK_TournamentRounds_IsPlayoff CHECK (IsPlayoff IN (0, 1));

