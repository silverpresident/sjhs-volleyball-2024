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