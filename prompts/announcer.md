//note to self the current announcemnt should be called notices

CReate a set up for announcer
1. Index display a table of accouncement for announcer.
 each with actions to Edit/Delete/Reannounce/hide

2. a page board which display the accnouncement in a list of cards.
THe announcer can
 - mark as announced/called (which records the time and removes it from list)
 - defer an announcement (put it at the end of the list) 
 - press a details button to go to the details page.

THe pages use signalr hub to update in real time

THe accouncement entity has the usual and necessary fields but also
- a priority (urgent, info, routine)
- a sequencingnumber (used to detrimn th eorder in which announcements are displayed to the announcer on the board)
- FirstAnnouncementTime
- LastAnnouncementTIme
- RepeatCount (set the number of times the announcement should be put back in the queue when it is marked as announced)
- AnnouncedCount

Maintain and display a history log for each time an announcement is called out.

when an announcement is created it is given a sequencingnumber.
- at the top of the queue (below all other URGENT announcements) if it is urgent
- at the bottom of the queue if it is not urgent.
The same is done when Reannounce is clicked.


If an announcement with Repeats remaining (> 0) is Called then it is put back at the end of the sequencingnumber
