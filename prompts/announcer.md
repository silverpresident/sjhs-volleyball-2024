I need to implement an 'Announcer' feature with real-time updates via SignalR. Please implement the following components based on the existing project context:

1. Data Model Update/Create the Announcement entity with these specific fields:

Standard fields (Id, Title, Content, etc.) inherit from the BaseEntity

Priority Enum (Urgent, Info, Routine) store as a string

SequencingNumber (int, used to determine display order on the Board)

FirstAnnouncementTime (Nullable DateTime)

LastAnnouncementTime (Nullable DateTime)

RemainingRepeatCount (int: Configured number of times to repeat)

AnnouncedCount (int: Actual times announced so far)

IsHidden (bool: False if currently in the queue)

Create a secondary entity AnnouncementHistoryLog to track every time an announcement is 'Called/Announced' (store Timestamp and AnnouncementId).

Write TSQL scripts to create the database tables

2. Queue Logic (Crucial) Implement a service method to handle SequencingNumber assignment using this logic:

New Urgent Item: Insert immediately after the last existing not hidden Urgent item (or at the top if none exist), shifting subsequent items down if necessary.

New Non-Urgent Item: Append to the very end of the queue.

Defer Action: Move the item to the very end of the queue (re-sequence).

Called Logic: Decrease the RemainingRepeatCount by 1

Repeat Logic: If an item is 'Called' and RemainingRepeatCount h> 0, move its SequencingNumber to the end of the queue.

Reannounce Action: Treat as a new queuing (Urgent goes to top section, others to bottom).

3. UI Requirements

View 1: Admin Index (Table)

Display all announcements.

Actions: Edit, Delete, Hide (toggle visibility), Reannounce (puts back in queue).

View 2: Announcer Board (Card List)

Order by SequencingNumber.

Action - 'Call/Announce':

Log to History.

Update LastAnnouncementTime (and First if null).

Increment AnnouncedCount.

If repeats remain: Move to end of queue.

If no repeats: Mark as complete/remove from board.

Action - 'Defer': Move to end of queue.

Action - 'Details': Navigate to details view.

4. Real-Time Ensure the SignalR Hub broadcasts updates to the Board view whenever the sequence changes or items are added/removed.
=====
We are working on the Announcer and AnnouncerBoard in the Admin area.
1. Look at the announcer-board.js which mainly manages signal r
- When the Call button is pressed it should post the Call action via ajax/fetch, disable the button and hide the announcement without reloading the page.
- Defer should send a post with the defer  without reloading the page and also hide the announcement without reloading the page.
- React properly to the various SignalR events as indicated below, do not reload the page

*AnnouncementCreated*
Render a display for the announcement

*AnnouncementCalled*
Update the display of the announcement

*AnnouncementUpdated*
Update the display of the announcement

*AnnouncementDeleted*
Remove the display of the announcement with matching Id

*AnnouncementQueueChanged*
Process the array of announcements. UPdating existing display, render new displays and delete any existing display that is not in the list.

*AnnouncementPropertyChanged*
Update the change property. property="hide" should hide, property="show" should show, property="order" should set the order value,

*Update the display*
UPdate the displayed values, also update the style and use the order property of the announcement to set the style.order value. Use the IsHidden property to hide/display the announcment

In the TournamentHub
receive the following events and respond accordingly
- RequestForAnnouncements   - send back all active announcements use the *AnnouncementQueueChanged*



