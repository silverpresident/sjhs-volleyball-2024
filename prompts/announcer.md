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
