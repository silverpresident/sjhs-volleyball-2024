Implement changes for "Match Scorer" (scorer.cshtml) page  to separate CSS and JS from CSHTML
---

### 1. Move the style sheet

Create a separate stylesheet.
Move all the CSS style in the page to this stylesheet
Add a link to this style
---

### 2. MOve the inline javascript
Create a separate javascript file.
Add a script tag to the file, on the script tag add a data attribute for the matchid, hometeamid and awayteamid.
at the start of the script file read these values into const from the currentScript element
move the remaining javascript to this file.