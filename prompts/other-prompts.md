You can look at the README.md to understand the design of this solution.
Remember that both the Admin and public project make use of the Lib project.
Please run the project and fix any errors.


All CRUD views should be nicely contained in a bootstrap card.

Check the DB models and uses for correctness

in the .App project build a TeamsController which lists all teams by division, group as well as results, and also the details of an single team.
Style the views using bootstrap containers and cards.
The details view should also show an automatically updating feed of match results.

===
Create a public UpdatesController in the .App project.

1. The View (Index.cshtml):

Fetch the 25 most recent MatchUpdates from the database (Order by CreatedDate Desc).

Render a container div for the list.

Import a lightweight Markdown library (like marked.js ) via CDN.

Date Formatting: "Format the timestamps as 'TimeAgo' (e.g., '2 mins ago') using a library like moment.js or C# logic."

2. The Logic (JavaScript):

Create a function renderUpdateCard(update) that takes the update object, converts the Markdown content to HTML, and returns a styled HTML card.

On page load, iterate through the server-side model data and append these cards.

3. Real-Time (SignalR):

Connect to the existing SignalR Hub.

Listen for the 'ReceiveMatchUpdate' event.

When a new update arrives:

Pass it to renderUpdateCard.

Prepend the new card to the top of the list with a subtle fade-in animation.

Remove the last item if the count exceeds 25."

===
Update the public Matches/Details.cshtml page
1. Improve the visual presentation of the page. Also display the match number and state (scheduled, started, inprogress,finished, disputed)
2. Display the details of each MatchSet
3. Ensure the updates are rendered. Remember the content of the update is markdown and should be rendered accordingly.

====
Update the public Teams/Details.cshtml page
1. Improve the visual presentation of the page. Also display the match number and state (scheduled, started, inprogress,finished, disputed)
2. Improve the display of the match results. Include the round, match number and other relevant details.
====
On the Home index page create a card above "Total Teams" to display the details (date and description) of the active tournament.
====
The Rounds index page in the Admin controller needs alot of work. Please fix the presentation and display relevant details.
====
Fix the admin area TournamentTeams/Details.cshtml page and action.
The page needs to display all relevant informaiton about the team in the active tournament.
====
In the admin area, in the TournamentTeams controller create a new action and view.
The view will list all the teams in the system
- Beside each team we should be able to select a division (radio) and a SeedNumber. Remember an option to "remove the team" by unselecting a division
-The purpose of this interface is a fast way to bulk select teams for the tournament.
-The input should save via javascript.
-Place a link to this view on the TournamentTeams Index page next to the "Add Team" button.
====
In the admin area, in the ActiveTournament controller 
On the index Add a management card above the "Teams" card.
Include management links for the active tournament.
Along with other you find in the project besure to include:
- Set Divisions
- Select Teams (bulk assign)
- Seed Teams (auto seed)
- Create First Round


In the  Rounds progress section
To the heading add a link to the TournamentRounds  index page  next to the matches page
 To each row in the tables in the Add a link to open the TournamentRounds details page
 

====
In the admin area, in the TournamentRounds  controller 
On the  details page add a button to
- Rank teams
-Generate next round

In the " Round Statistics" section display in highlighted fashion the number of disputed matches.
Implement the Rank Teams action.
-Generate next round action alread exists, just add the link to it.
====
Create a developer Controller and page in the admin area.
It should only be vailable in debug mode.

It should have actions which are limited to the Active tournament.
 Create views and actions for
- Generate Teams ( select number of teams and add a set of team with randomly generated name and schools)
- add an option to delete all matches  in the active tournament

====
In the open service file, AssignFirstRoundTeamsAsync method needs to be implemented .
Ensure the round is a first round, and assign teams based on the   tournamentRound.TeamsPerGroup  or tournamentRound.GroupsInRound  
Also consider that if mathes are already scheduled then the group assignment should not be changed but if there afre no matches then everything may be reassigned.

Look at CreateFirstRoundAsync to see how the teams are selected for the first round then assign and place them in groups.

====

Fix the AssignTeamsToGroupsByTeamsPerGroupAsync method and the AssignTeamsToGroupsByGroupCountAsync.
1. The top seeded teams should be distributed amond the available groups, then the next best seed and so on.
====
In the developer controller and views
Similar to the Delete all matches, do a "delete all rounds"
====
In the TournamentRounds/GenerateMatches View and action make the follwoing changes
- Remove "Default Court Location"
- Put an input for "Starting Court Number" (int) default 1 min 1
- Put an input for "Number of courts" (int) default 1 min 1 max 99
- Put an input for "Match time interval" (int) default 10, min 5 max 60
Pass these to the match generator.
Balance the court assignments.
 - Try to put all matches from same group on the same court
 - Matchs on different courts can start at the same time...factor that into the time assignment.
- Mark the "Start Time" input time only and precise upto the minute only (with property bound)
====
In the developer controller and views
Create an action and view called "Generate match results"
For each match that is not finished in the active tournament
- add 1 or 3 sets.
- add scores to sets and finalzie the sets
- finalize the match
====
Create an informational partial view which will be displayed on both the GenerateNextRound.cshtml and 
CreateFirstRound.cshtml.
This section will display useful information for how to set up the round.
 for example
- If the next round is Quarter finals you should  have 8 or less groups in this round and 8 teams advancing
- If the next round is semi finals you should  have 4 or less teams/groups in this round and 4 teams advancing 

Recommendations for Finals
Teams coming in should be WINNERS ONLY. 2 Teams.

Recommendations for Semi Finals
Teams coming in should be WINNERS ONLY. 4 Teams.

Recommendations for Quarter Finals
Teams coming in should be TOP FROM GROUP AND NEXT BEST or TOP BY POINTS, 8 Teams.
====
Look at the tournament service
Create a GetTournamentRoundDetails(TournamentRoundId)
This methods should return the details for a single TournamentRound
The GetTournamentDetailsAsync does something similar but it returns details for all the rounds int he tournament.
====

In the GenerateNextRound, for the " Teams Coming Into This Round" section those fields shouldbe editable by the user.
For the "Configuration Type" include an option for No Group 
Put the _RoundSetupRecommendations beside the main form

Also on the CreateFirstRound, Put the _RoundSetupRecommendations beside the main form
