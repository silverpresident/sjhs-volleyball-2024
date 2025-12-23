Help me improve this prompt for cline ai bot being used vs code. It already knows the context of my project. 
Always use  paragraph type descriptions in place of tables
Here is what i have already written as  my prompt:


Let's work on the TournamentRounds
Create an action and view for the "Select Teams" option.
The view should present a form.
the form should list all the team in the round, their 
rank, group, sets, points and point diff, similar to the picture in a4. 
Beside each team there should be an option to CHECK the team to include it.
THe teams to be selected should be checked  by default based on  team selection strategy and number of qualifying teams.
If the team selection strategy of the TournamentRound is "Manual" the user can be allowed to change the check items.
When saved those teams are added to the round.


======
Let's introduce a new feature within the Admin area to handle the selection of qualifying teams from a completed tournament round. This will involve a new view model, a GET action to display the selection form, and a POST action to process the results and guide the user to the next step in the workflow.

Please start by creating a new view model named SelectTeamsViewModel in the src/VolleyballRallyManager.App/Areas/Admin/Models/ directory. This view model should serve as the data container for the new view. It needs to include properties for the TournamentRoundId, the RoundName, the TeamSelectionStrategy, and the NumberOfQualifyingTeams. The main property should be a List<TeamSelectionItem>, where TeamSelectionItem is a new nested class. This TeamSelectionItem class must contain properties for TeamId, TeamName, Rank, GroupName, SetsWon, SetsLost, Points, PointDifference, and a boolean IsSelected to bind to a form checkbox.

The view should present a form.
the form should list all the team in the round, their 
rank, group, sets, points and point diff, similar to the picture in a4. 

Next, in the TournamentRoundsController within the Admin area, create a new GET action named SelectTeams that accepts the id of the tournament round. This action should fetch the specified TournamentRound and a ranked list of its participating teams. Use this data to populate an instance of the SelectTeamsViewModel. The logic here should automatically set the IsSelected property to true for each team that qualifies based on the round's TeamSelectionStrategy and NumberOfQualifyingTeams.

Then, create the corresponding Razor view at Areas/Admin/Views/TournamentRounds/SelectTeams.cshtml. This view should render a form. At the top, display the RoundName as a clear heading. The body of the form should render a row for each team from the List<TeamSelectionItem>. Each row should display the team's rank and other statistics, and end with a checkbox bound to the IsSelected property. An important rule for the view is that if the TeamSelectionStrategy is anything other than Manual, the checkboxes should be rendered as disabled. This ensures users can see the automatic selection but cannot alter it.

Finally, implement the [HttpPost] action for SelectTeams, which will receive the SelectTeamsViewModel. When the form is submitted, this action should filter the list of teams to find all TeamIds where IsSelected is true. The action should then update the TournamentRound with the selected teams. After updating, redirect the user to the Details view for the current round, ensuring they see the updated information.