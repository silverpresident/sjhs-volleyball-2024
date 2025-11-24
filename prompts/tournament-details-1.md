Implement changes for "Tournament Details" 
---

### 1. Create a view model

Create a view model Tournament Details View Model
It should contina a tournamentId
a Tournament property
a Divisions property which is a list of Tournament Division View Model
a Rounds property which is a list of Tournament Rounds View Model
a Teams property which is a list of TournamentTeamDivions

The  Tournament Division View Model
Should contain the properties of  Tournament Division as well as
- Group Name 
- Team Count
- Matches Played

The Tournament Rounds View Model
Should contain the properties for
- Division
- Round
- Team Count
- Matches Scheduled
- Matches Played


---

### 2. Update the views of TOurnaments  
 UPdate the Details view of TOurnaments and the Index view of ActiveTournament to use this view model.
THe division section should display the relevant details from Tournament Division View Model
Add a round seciton to display relevant informaiton from the Tournament Rounds View Model