In the lib project, in th eWorkers folder
create a ScoringChannel to accept and queue background tasks for scorers.
It should allow a queuing of the following events:
- CallToCourt
- CallToSupport
- MatchStart
- MatchSetStart
- MatchSetEnd
- MatchSetRevertToPrevious
- MatchSetScoreChange
- MatchEnd
- MatchDisputed

Look at the existing scorer code (scorer.js, ScorerHub.cs) for context.
The purpose of using a queue is for better concurrent updates and sequencing of actions to avoid race conditions.

Create a ScoringAutomationWorker to process the events and trigger the necessary functions.

Add the automation workers to the extension method.