Volleyball Rally Manager
The following is a description of a c# .Net Solution. Check the current solution to ensure it meets the requirements.

Volleyball Rally Manager Description
Build a C# dot net MVC app project using the latest LTS version of dot net. The app must use entityframe work identity to allow user authentication and authorization. Once logged in users should be able access administative funciton.  The name of the project is "Volleyball Rally Manager". The project will be backed by an SQL Server database hosted on azure

The theme colour of the project should be a green and gold.

Use bootstrap, jquery and other relevant frameworks and libraries to improve the user interface and user experience.

My Environment uses powershell.

STRUCTURE
Create 3 projects. Do not use top level statements.
A lib project that handles business logic including the entities, models, database contexts, services and migrations.
A app project that handles the ui and admin
A blazor SPA web app project that hadles the ui descripted inteh section labeles public ui

PUBLIC UI
On the main page without a logged in user you should be able to see a schedule of matches showing match number, round, time, court location, players, score, and whether or  not the match is finished. On this same home page there should be a partial view section for announcements. The page should automatically reload every 2 minutes.
On this same page there should be a partial view showing a leader board showing the list of teams and total points.
On this same page there should be a partial view showing a update stream.
Use signalR to push realtime update to the app.


GENERAL OVERVIEW
THe competetion has a BOYS and a GIRLS division.
Each division has 3 rounds, and then a quarter, semi-finals and finals.
Team receive 3 points for a win, 1 point for a draw and no points for a loss.
Each team has a NAME, SCHOOL, COLOUR, DIVISION, URL to a image logo and should track points, point difference and total points for and against. As well as anyother property you need to compete the task.
Each match should have match number, round, time, court location, 2 teams, score for each team, and whether or  not the match is finished. It shoulds have both a scheduled time and an acutal start time. Store also the referee name, scorer name, a a flag for a dispute.
For each match store a record of the update including what was changed and by which user.
There should be a list of announcements with just text body and a priority (info, warning, danger, primary, secondary) and show/hide flag.
A list of updates (match score updates) and the time it was created.
Use signalR to push update to the app.

The web app manages a one day volleyball competetion titled "ST JAGO VOLLEYBALL RALLY"
On the home page before logged in user  implement all the logic descried in the PUBLIC UI section.

The rest of the app is access by logging in.
A logged in user should be able to add, edit, delete and view a list of teams. 
A logged in user should be able to add, edit, delete and view a list of matches.
A logged in user should be able to add, edit, delete and view a list of announcements. 
A logged in user should be able to add, edit, delete and view a list of match updates.


Create a VS Code workspace file and a Visual Studio solution file.
Create the relevant readme and gitignore files.
All primary key ids must use GUID.
use appsettings.json and not web.config. Generally prefer json settings files over XML
1. Generate a db context for all the entity models
3. include a markdown package which must be used to parse the updates and annoucements

1. add entity framework for SQL server database
2. add a reference to the lib project
3. add entity framework identity for use with google single sign on and microsoft sign on. Be sure to include the settings in the appsettings.json file.
USe SQL script to handle database creation and setup.

Also build a second project that use a blazor single page web app that only handles the public display sections described above.

Create a readme file that contains instructions on what i need to set up on Azure to make all this work.
Create a teraform file which can be used to provision the resources.
You may include a .NET ASpire