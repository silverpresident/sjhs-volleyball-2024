# Announcer Feature Documentation

## Overview

The Announcer feature provides a real-time announcement queue management system for tournament events. It allows operators to manage, queue, and announce items in a controlled sequence with priority-based ordering and automatic re-queuing.

## Features

### 1. Announcement Management
- Create, edit, and delete announcements
- Set priority levels (Urgent, Info, Routine)
- Configure repeat counts for recurring announcements
- Hide/unhide announcements from the queue
- View announcement history and statistics

### 2. Priority-Based Queue System
- **Urgent**: Inserted immediately after the last urgent item
- **Info/Routine**: Added to the end of the queue
- Automatic sequencing and re-sequencing

### 3. Announcer Board (Operator Interface)
- Card-based visual queue display
- One-click "Call/Announce" functionality
- Defer announcements to end of queue
- Real-time updates via SignalR
- Keyboard shortcuts for quick operation

### 4. Real-Time Updates
- SignalR integration for live queue updates
- Automatic board refresh on changes
- Multi-user synchronization

## Database Schema

### Announcements Table
```sql
- Id (UNIQUEIDENTIFIER, PK)
- Title (NVARCHAR(200), Required)
- Content (NVARCHAR(MAX), Required)
- Priority (NVARCHAR(20), Required) - 'Urgent', 'Info', or 'Routine'
- SequencingNumber (INT, Required)
- FirstAnnouncementTime (DATETIME2, Nullable)
- LastAnnouncementTime (DATETIME2, Nullable)
- RemainingRepeatCount (INT, Required, Default: 1)
- AnnouncedCount (INT, Required, Default: 0)
- IsHidden (BIT, Required, Default: 0)
- CreatedAt, UpdatedAt, CreatedBy, UpdatedBy (Standard audit fields)
```

### AnnouncementHistoryLogs Table
```sql
- Id (UNIQUEIDENTIFIER, PK)
- AnnouncementId (UNIQUEIDENTIFIER, FK, Required)
- Timestamp (DATETIME2, Required)
- CreatedAt, UpdatedAt, CreatedBy, UpdatedBy (Standard audit fields)
```

## Queue Logic

### Creating Announcements

#### Urgent Priority
1. Find the last non-hidden Urgent item in the queue
2. Insert new item immediately after it
3. Re-sequence all subsequent items (increment their sequence numbers)
4. If no Urgent items exist, insert at position 1 and shift all items down

#### Non-Urgent Priority (Info/Routine)
1. Find the maximum sequence number in the queue
2. Add new item at `maxSequence + 1`

### Calling/Announcing
When an announcement is "Called":
1. Log to `AnnouncementHistoryLogs`
2. Set `FirstAnnouncementTime` (if not already set)
3. Update `LastAnnouncementTime` to current time
4. Increment `AnnouncedCount`
5. Decrement `RemainingRepeatCount`
6. **If repeats remain** (`RemainingRepeatCount > 0`):
   - Move to end of queue (assign highest sequence number + 1)
7. **If no repeats remain** (`RemainingRepeatCount = 0`):
   - Mark as hidden (`IsHidden = true`)
   - Remove from active queue

### Deferring
When an announcement is "Deferred":
1. Move to end of queue (assign highest sequence number + 1)
2. Do NOT modify repeat counts or announcement times

### Reannouncing
When an announcement is "Reannounced":
1. Reset `RemainingRepeatCount` to 1 (or original configured value)
2. Set `IsHidden = false`
3. Treat as new announcement for sequencing:
   - **Urgent**: Insert after last urgent item
   - **Non-Urgent**: Append to end

## User Interfaces

### 1. Admin: Manage Announcements
**Route**: `/Admin/Announcements`

**Features**:
- Table view of all announcements (including hidden)
- Create new announcements
- Edit existing announcements
- Delete announcements
- Toggle hide/show status
- Reannounce hidden items
- View announcement details and history

**Actions**:
- **Create**: Opens form to create new announcement
- **Edit**: Modify title, content, priority, repeat count
- **Delete**: Permanently remove announcement
- **Hide/Show**: Toggle visibility in queue
- **Reannounce**: Put hidden announcement back in queue
- **Details**: View full details and call history

### 2. Announcer Board
**Route**: `/Admin/AnnouncerBoard`

**Features**:
- Card-based display of queued announcements
- Ordered by `SequencingNumber`
- Visual priority indicators (color-coded borders)
- Real-time updates via SignalR

**Card Display**:
- Priority badge (Urgent=Red, Info=Blue, Routine=Gray)
- Sequence number
- Repeat count (if > 1)
- Title and content
- Content is markdown text
- Statistics (times called, last announcement time)

**Actions**:
- **Call/Announce**: Process the announcement
- **Defer**: Move to end of queue
- **Details**: View full details

**Keyboard Shortcuts**:
- `Space`: Call first announcement
- `D`: Defer first announcement

## SignalR Events

### Client-Side Events (Board Receives)
- `AnnouncementCreated`: New announcement added
- `AnnouncementUpdated`: Announcement modified
- `AnnouncementDeleted`: Announcement removed
- `AnnouncementCalled`: Announcement was called
- `AnnouncementQueueChanged`: Queue order changed

**Behavior**: All events trigger a page reload to show latest state

## API Endpoints (Controllers)

### AnnouncementsController (Admin)
```
GET    /Admin/Announcements              - List all
GET    /Admin/Announcements/Create       - Create form
POST   /Admin/Announcements/Create       - Save new
GET    /Admin/Announcements/Edit/{id}    - Edit form
POST   /Admin/Announcements/Edit         - Save changes
GET    /Admin/Announcements/Delete/{id}  - Delete confirmation
POST   /Admin/Announcements/Delete       - Confirm delete
GET    /Admin/Announcements/Details/{id} - View details
POST   /Admin/Announcements/ToggleHide   - Hide/Show
POST   /Admin/Announcements/Reannounce   - Requeue item
```

### AnnouncerBoardController
```
GET    /Admin/AnnouncerBoard              - Board view
GET    /Admin/AnnouncerBoard/Details/{id} - View details
POST   /Admin/AnnouncerBoard/Call         - Call announcement
POST   /Admin/AnnouncerBoard/Defer        - Defer to end
```

## Service Layer

### IAnnouncementService
```csharp
Task<IEnumerable<Announcement>> GetAllAnnouncementsAsync(bool includeHidden)
Task<Announcement?> GetAnnouncementByIdAsync(Guid id)
Task<IEnumerable<Announcement>> GetQueuedAnnouncementsAsync()
Task<Announcement> CreateAnnouncementAsync(Announcement announcement)
Task<Announcement> UpdateAnnouncementAsync(Announcement announcement)
Task<bool> DeleteAnnouncementAsync(Guid id)
Task<Announcement> HideAnnouncementAsync(Guid id)
Task<Announcement> UnhideAnnouncementAsync(Guid id)
Task<Announcement> CallAnnouncementAsync(Guid id)
Task<Announcement> DeferAnnouncementAsync(Guid id)
Task<Announcement> ReannounceAsync(Guid id)
Task<IEnumerable<AnnouncementHistoryLog>> GetHistoryForAnnouncementAsync(Guid id)
```

## Setup Instructions

### 1. Database Setup
```bash
# Run the SQL script to create tables
sqlcmd -S your-server -d stjago-volleyball-demo -i database/announcements-setup.sql
```

Or use SQL Server Management Studio to execute `database/announcements-setup.sql`

### 2. Entity Framework Migration (Optional)
If EF tools are installed:
```bash
cd src/VolleyballRallyManager.Lib
dotnet ef migrations add AddAnnouncementsTables --startup-project ../VolleyballRallyManager.App
dotnet ef database update --startup-project ../VolleyballRallyManager.App
```

### 3. Verify Service Registration
The `AnnouncementService` should already be registered in `ServiceCollectionExtensions.cs`:
```csharp
services.AddScoped<IAnnouncementService, AnnouncementService>();
```

### 4. Access the Feature
- Navigate to **Announcer → Announcer Board** in the navigation menu
- Or go to **Announcer → Manage Announcements** to create announcements

## Usage Workflow

### Creating an Announcement
1. Navigate to **Announcer → Manage Announcements**
2. Click **Create New**
3. Fill in:
   - Title 
   - Content (required)
   - Priority (Urgent/Info/Routine)
   - Repeat Count (how many times to announce)
4. Click **Create**

### Operating the Announcer Board
1. Navigate to **Announcer → Announcer Board**
2. Announcements are displayed as cards, ordered by sequence
3. To announce:
   - Click **Call/Announce** on the first card (or press Space)
   - The announcement is logged and moved/removed based on repeat count
4. To defer:
   - Click **Defer** (or press D for first card)
   - The announcement moves to the end of the queue

### Managing Queue
- **Hide**: Temporarily remove from queue (can be reannounced later)
- **Delete**: Permanently remove announcement
- **Reannounce**: Put a hidden/completed announcement back in the queue
- **Edit**: Modify announcement details (won't affect queue position)

## Styling & Theming

The Announcer Board uses the project's green and gold theme:
- **Green** (`#2d5016`): Primary action buttons, project color
- **Gold** (`#d4af37`): Warning/defer buttons, accents
- **Red**: Urgent priority indicators with pulsing animation
- **Blue**: Info priority indicators
- **Gray**: Routine priority indicators

## Troubleshooting

### Queue Not Updating
- Check SignalR connection in browser console
- Verify `MatchHub` is properly configured in `Program.cs`
- Ensure JavaScript file `/js/announcer-board.js` is loading

### Sequence Numbers Out of Order
- This shouldn't happen with proper service logic
- If it does, manually edit announcements to fix sequence
- Or delete and recreate problematic announcements

### Real-Time Not Working
- Verify SignalR service is registered
- Check browser console for connection errors
- Ensure firewall allows WebSocket connections

## Future Enhancements

Potential improvements:
1. Audio cues when announcement is called
2. Text-to-speech integration
3. Export announcement history to CSV
4. Scheduled announcements (time-based)
5. Templates for common announcements
6. Drag-and-drop queue reordering
7. Multi-language support
8. Integration with public display screens

## Related Documentation
- [User Management Guide](USER_MANAGEMENT_GUIDE.md)
- [Scoring Channel](SCORING_CHANNEL.md)
- [Authentication Setup](AUTHENTICATION_SETUP.md)
