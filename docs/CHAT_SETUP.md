# Chat Feature Setup Guide

This guide will help you set up and deploy the chat feature for the Volleyball Rally Manager.

## Prerequisites

- SQL Server database
- .NET 10.0 SDK
- Audio file for notifications (optional)

## Step 1: Database Migration

Run the chat feature migration script against your database:

```sql
sqlcmd -S your-server -d VolleyballRallyManager -i database/migration-chat-feature.sql
```

Or manually execute the script in SQL Server Management Studio or Azure Data Studio.

## Step 2: Add Notification Sound (Optional)

1. Create a directory: `src/VolleyballRallyManager.App/wwwroot/sounds/`
2. Add a notification sound file named `message.mp3` to this directory
3. You can use any short sound effect (recommend < 1 second)
4. Free sound resources:
   - https://freesound.org/
   - https://mixkit.co/free-soundeffects/
   - https://notificationsounds.com/

**Note**: If you don't add a sound file, the chat will still work but without audio notifications.

## Step 3: Verify Configuration

The following files should already be configured:

1. **Service Registration**: [`src/VolleyballRallyManager.Lib/Configuration/ServiceCollectionExtensions.cs`](../src/VolleyballRallyManager.Lib/Configuration/ServiceCollectionExtensions.cs)
   - ✅ `IChatService` is registered

2. **SignalR Hub**: [`src/VolleyballRallyManager.Lib/Configuration/SignalRConfiguration.cs`](../src/VolleyballRallyManager.Lib/Configuration/SignalRConfiguration.cs)
   - ✅ ChatHub is mapped to `/chathub`

3. **Database Initialization**: [`src/VolleyballRallyManager.Lib/Configuration/DatabaseInitialization.cs`](../src/VolleyballRallyManager.Lib/Configuration/DatabaseInitialization.cs)
   - ✅ Default chat rooms are seeded automatically

## Step 4: Build and Run

```bash
# Build the solution
dotnet build

# Run the Admin application
cd src/VolleyballRallyManager.App
dotnet run
```

## Step 5: Access the Chat

1. Navigate to your application (e.g., `https://localhost:5001`)
2. Log in with an authenticated account
3. Go to `/Admin/Chat` to access the chat interface

## Default System Rooms

After running the migration and initializing the database, you should see these default rooms:

1. **Lobby** - Public room for all users
2. **Management** - Private room for key organizers
3. **Judges and Referees** - Role-based (Referee role required)
4. **Scorers** - Role-based (Scorekeeper role required)
5. **Announcers** - Role-based (Announcer role required)
6. **Support** - Private room for support staff

## User Roles

Make sure users are assigned appropriate roles:

- **Administrator**: Full access to all rooms
- **Referee**: Access to Judges and Referees room
- **Scorekeeper**: Access to Scorers room
- **Announcer**: Access to Announcers room
- **Judge**: Traditional judging role

### Assigning Roles

Use the User Management interface in the Admin area:
`/Admin/UserManagement`

Or use ASP.NET Core Identity APIs:

```csharp
await userManager.AddToRoleAsync(user, "Referee");
```

## Testing the Chat

### Basic Functionality Test

1. **Connect to Chat**:
   - Navigate to `/Admin/Chat`
   - Verify the connection status message appears

2. **Join a Room**:
   - Click on "Lobby" in the sidebar
   - Verify the chat area loads

3. **Send a Message**:
   - Type a message in the input box
   - Press Enter or click Send
   - Verify the message appears in the chat area

4. **Test Markdown**:
   - Send: `**bold text**` → Should display as bold
   - Send: `*italic text*` → Should display as italic
   - Send: `` `code` `` → Should display as code
   - Send: `[Link](https://example.com)` → Should display as a clickable link

### Advanced Features Test

5. **Typing Indicators**:
   - Open chat in two browser windows with different users
   - Start typing in one window
   - Verify "User is typing..." appears in the other window

6. **Room Muting**:
   - Click the "Mute" button
   - Send yourself a message from another window
   - Verify no sound plays

7. **Create Private Room**:
   - Click "Create Room"
   - Enter room details
   - Check "Make this room private"
   - Verify the room appears in the sidebar

8. **Role-Based Access**:
   - Log in as a user with "Referee" role
   - Verify "Judges and Referees" room is visible
   - Log in as a user without "Referee" role
   - Verify the room is NOT visible

## Troubleshooting

### Connection Issues

**Problem**: "Failed to connect to chat" message

**Solutions**:
1. Check that the application is running
2. Verify SignalR is configured in `Program.cs`:
   ```csharp
   app.UseVolleyballSignalR();
   ```
3. Check browser console for errors (F12)
4. Verify no firewall is blocking WebSocket connections

### Messages Not Sending

**Problem**: Messages don't appear after sending

**Solutions**:
1. Check browser console for errors
2. Verify database connection is working
3. Check that ChatService is registered in DI
4. Ensure user is authenticated

### Roles Not Working

**Problem**: Users can't access role-based rooms

**Solutions**:
1. Verify role is assigned to the user in the database
2. Check `AspNetUserRoles` table
3. Ensure the role name matches exactly (case-sensitive)
4. Log out and log back in to refresh claims

### No Sound Notifications

**Problem**: No sound plays when receiving messages

**Solutions**:
1. Check that `message.mp3` exists in `wwwroot/sounds/`
2. Verify browser allows audio autoplay
3. Check browser volume settings
4. Some browsers block autoplay - user interaction may be required first

## Production Deployment

### Azure App Service

1. **Application Settings**:
   - Add connection string to Azure SQL Database
   - Ensure SignalR Service is configured (optional for scale):
     ```csharp
     services.AddSignalR().AddAzureSignalR(connectionString);
     ```

2. **Enable WebSockets**:
   - In Azure Portal > App Service > Configuration
   - Set "Web sockets" to **On**
   - Save and restart

3. **CORS Configuration**:
   - Update CORS policy if needed for cross-origin requests
   - Located in `Program.cs`

### Database

- Run the migration script against your production database
- Verify all tables are created:
  - ChatRooms
  - ChatMessages
  - ChatRoomMemberships

### Performance Tuning

For high-traffic scenarios:

1. **Enable Redis Backplane**:
   ```csharp
   services.AddSignalR()
       .AddStackExchangeRedis(configuration["Redis:ConnectionString"]);
   ```

2. **Database Indexing**:
   - The migration already includes necessary indexes
   - Monitor query performance and add as needed

3. **Message Retention**:
   - Consider implementing message cleanup for old messages
   - Create a background job to archive messages older than X days

## Additional Features

### Custom Notification Sounds

Replace `wwwroot/sounds/message.mp3` with your own WAV or MP3 file.

### Disabling Vibration

Edit `chat.js` and remove/comment out the vibration calls:

```javascript
// Comment out these lines
// navigator.vibrate([100, 50, 100, 50, 100]); // Priority
// navigator.vibrate(100); // Standard
```

### Customizing Room Icons

Edit the icons in `Chat/Index.cshtml`:

```csharp
var icon = room.RoomType switch
{
    ChatRoomType.Public => "bi-globe",        // Change these
    ChatRoomType.Private => "bi-lock",
    ChatRoomType.RoleBased => "bi-people",
    ChatRoomType.Direct => "bi-person-circle",
    _ => "bi-chat"
};
```

## Support

For issues or questions:
- Review the main documentation: [`CHAT_FEATURE.md`](CHAT_FEATURE.md)
- Check project rules: [`.clinerules`](../.clinerules)
- Review SignalR logs in the browser console (F12)
- Check server logs in Application Insights or console output

## Next Steps

1. Run the migration script
2. Test the basic functionality
3. Assign roles to users
4. Add custom notification sound  (optional)
5. Deploy to production environment

The chat feature is now ready to use!
