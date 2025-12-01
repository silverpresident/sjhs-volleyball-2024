Refactor the "Match Scorer" page (scorer.cshtml) to separate concerns.

1. Extract CSS

Create a new file named scorer.css in the same directory (or specify wwwroot/css/ if applicable).

Move all styles from the cshtml file to this new file.

Reference this stylesheet in the scorer.cshtml.

2. Extract JavaScript

Create a new file named scorer.js in wwwroot/js.

Data Passing: In scorer.cshtml, add the script tag referencing scorer.js. Add data-match-id, data-hometeam-id, and data-awayteam-id attributes to this script tag, populating them with the corresponding Razor Model values.

Variable Initialization: At the very top of scorer.js, capture document.currentScript to read these data attributes into const variables.

Logic Migration: Move all remaining JS logic to scorer.js. Important: Ensure any previous Razor syntax (e.g., @Model.MatchId) used inside the logic is replaced by the new const variables created from the data attributes.

=====
The Matches controller and views in the Admin Area need some work
- The create action does not pass inthe rounds, the scheduled time should default to 15 minutes after the last scheduled match or the date of the active tournament. The view needs work on the visual presentation. I like how the edit view looks generally.
-The edit action does not pass in the rounds.
- The Assign Page should also display the division, round, referee, scorer and court.
- The Update Page should also display the division,  round, referee, scorer and court.
- Similar to the assign page create a Finalize Page (and put a link on the index and details page) which allows editing the following fields:
   - FInished
   - Disputed
   - Locked
   - THe score if no sets are present
   - the notes
- The details page has a Set history section. Each set in this section should have a Edit options which allow you to  edit the score, IsLocked, IsFinished state for that set
- the scheduled time input should go up to hours and minutes not seconds.
- the actual start time input should go up to hours and minutes not seconds.
- THe CourtLocation should have a data list which contacing "Court 1" to "Court 9"
- Check the bindings on the various post action