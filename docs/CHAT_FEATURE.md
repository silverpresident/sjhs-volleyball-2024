# Real-Time Chat Feature Documentation

## Overview
The VolleyballRallyManager now includes a comprehensive real-time chat feature built using ASP.NET Core SignalR. This feature enables organizers and workers to communicate efficiently during tournaments through public, private, and role-based chat rooms.

## Architecture

### Technology Stack
- **Backend**: ASP.NET Core SignalR for real-time bi-directional communication
- **Data Layer**: Entity Framework Core with SQL Server
- **Authentication**: ASP.NET Core Identity with role-based access control
- **Frontend**: JavaScript SignalR client with Markdown support

### Core Components

#### 1. Data Models
Located in [`src/VolleyballRallyManager.Lib/Models/`](../src/VolleyballRallyManager.Lib/Models/)

- **[`ChatRoom`](../src/VolleyballRallyManager.Lib/Models/ChatRoom.cs)**: Represents a chat room
  - Supports Public, Private, Role-Based, and Direct chat types
  - System rooms cannot be deleted
  - Includes owner and role requirements

- **[`ChatMessage`](../src/VolleyballRallyManager.Lib/Models/ChatMessage.cs)**: Represents individual messages
  - Stores sender information, content, and timestamp
  - Supports markdown formatting
  - Includes read status for direct chats

- **[`ChatRoomMembership`](../src/VolleyballRallyManager.Lib/Models/ChatRoomMembership.cs)**: Manages user-room relationships
  - Tracks room admin status
  - Supports room muting (no notifications)
  - Tracks last read timestamp for unread counts

- **[`ChatRoomType`](../src/VolleyballRallyManager.Lib/Models/ChatRoomType.cs)**: Enum for room types

#### 2. Services
Located in [`src/VolleyballRallyManager.Lib/Services/`](../src/VolleyballRallyManager.Lib/Services/)

- **[`IChatService`](../src/VolleyballRallyManager.Lib/Services/IChatService.cs)** / **[`ChatService`](../src/VolleyballRallyManager.Lib/Services/ChatService.cs)**: Business logic layer
  - Room management (CRUD operations)
  - Membership management (join, leave, mute)
  - Message operations (send, retrieve, pagination)
  - Access control validation
  - Unread message tracking

#### 3. SignalR Hub
Located in [`src/VolleyballRallyManager.Lib/Hubs/`](../src/VolleyballRallyManager.Lib/Hubs/)

- **[`ChatHub`](../src/VolleyballRallyManager.Lib/Hubs/ChatHub.cs)**: Real-time communication hub
  - User presence tracking (online/offline)
  - Connection management
  - Message broadcasting
  - Typing indicators
  - Room join/leave operations
  - Designed for horizontal scalability (backplane-ready)

## Default System Rooms

The system automatically seeds six default rooms:

1. **Lobby** (Public)
   - Open to all authenticated users
   - General communication

2. **Management** (Private)
   - For key organizers
   - Requires explicit invitation

3. **Judges and Referees** (Role-Based: `Referee`)
   - For official referee communications
   - Auto-accessible to users with `Referee` role

4. **Scorers** (Role-Based: `Scorekeeper`)
   - For official score-keeping communications
   - Auto-accessible to users with `Scorekeeper` role

5. **Announcers** (Role-Based: `Announcer`)
   - For official announcer communications
   - Auto-accessible to users with `Announcer` role

6. **Support** (Private)
   - For support staff to handle escalated issues

## User Roles

The system now includes five roles:
- **Administrator**: Full system access, can access all rooms
- **Judge**: Traditional judging functions
- **Referee**: Access to Judges and Referees room
- **Scorekeeper**: Access to Scorers room, can update match scores
- **Announcer**: Access to Announcers room

## Features

### 1. Room Management
- Create custom private rooms
- Invite specific users to private rooms
- Role-based automatic access
- System administrators can access all rooms
- Room admins can manage membership

### 2. Real-Time Messaging
- Instant message delivery via SignalR
- Message persistence in SQL Server
- Markdown support for rich text formatting
- Message history with pagination (50 messages per page)
- Infinite scroll for older messages

### 3. User Presence
- Real-time online/offline status
- Connection tracking per user
- Automatic presence updates

### 4. Typing Indicators
- "User is typing..." notifications
- Broadcasted only to room members
- Automatic timeout handling (client-side)

### 5. Notifications
- **In-App Badges**: Unread message counts per room
- **Sound Alerts**: Configurable message chimes
- **Vibration Feedback**: 
  - Standard vibration for group messages
  - Enhanced vibration for direct messages (priority)
- **Mute Option**: Silence specific rooms while staying connected

### 6. Direct/One-on-One Chat
- Users can initiate 1-on-1 conversations
- Convertible to group chats by adding more users
- Dedicated notification style for direct messages

## Database Schema

### Tables Created
See [`database/migration-chat-feature.sql`](../database/migration-chat-feature.sql) for the complete schema.

- **ChatRooms**: Stores room information
- **ChatMessages**: Stores all messages
- **ChatRoomMemberships**: Manages user-room relationships

### Indexes
- `IX_ChatMessages_ChatRoomId_Timestamp`: Optimizes message queries
- `IX_ChatRoomMemberships_UserId_ChatRoomId`: Ensures unique membership and fast lookups

## Configuration

### Service Registration
Located in [`src/VolleyballRallyManager.Lib/Configuration/ServiceCollectionExtensions.cs`](../src/VolleyballRallyManager.Lib/Configuration/ServiceCollectionExtensions.cs:52)

```csharp
services.AddScoped<IChatService, ChatService>();
```

### SignalR Hub Registration
Located in [`src/VolleyballRallyManager.Lib/Configuration/SignalRConfiguration.cs`](../src/VolleyballRallyManager.Lib/Configuration/SignalRConfiguration.cs:49)

```csharp
endpoints.MapHub<ChatHub>("/chathub");
```

### Database Initialization
Located in [`src/VolleyballRallyManager.Lib/Configuration/DatabaseInitialization.cs`](../src/VolleyballRallyManager.Lib/Configuration/DatabaseInitialization.cs:280)

System rooms are automatically seeded during database initialization.

## API Endpoints (SignalR Hub Methods)

### Connection Management
- `OnConnectedAsync()`: Auto-joins user to their rooms
- `OnDisconnectedAsync()`: Cleanup and offline notification

### Room Operations
- `JoinRoom(string roomId)`: Join a specific room
- `LeaveRoom(string roomId)`: Leave a room
- `CreateRoom(string name, string description, bool isPrivate)`: Create new room
- `ToggleMuteRoom(string roomId)`: Mute/unmute room notifications

### Messaging
- `SendMessage(string roomId, string message)`: Send a message
- `LoadMessageHistory(string roomId, string? beforeTimestamp, int count)`: Load message history

### Presence & Typing
- `UserTyping(string roomId)`: Broadcast typing indicator
- `UserStoppedTyping(string roomId)`: Stop typing indicator
- `GetUnreadCount(string roomId)`: Get unread message count

## Client Events (Received from Server)

### Connection
- `UserOnline(userId)`: User came online
- `UserOffline(userId)`: User went offline

### Room Events
- `UserJoinedRoom(roomId, userId, userName)`: User joined room
- `UserLeftRoom(roomId, userId, userName)`: User left room
- `RoomCreated(roomInfo)`: New room created
- `RoomMuteToggled(roomId, isMuted)`: Mute status changed

### Messaging
- `ReceiveMessage(messageData)`: New message received
- `MessageHistory(roomId, messages)`: Historical messages loaded
- `UnreadCount(roomId, count)`: Unread count updated

### Presence
- `UserTyping(roomId, userId, userName)`: User started typing
- `UserStoppedTyping(roomId, userId)`: User stopped typing

### Errors
- `Error(message)`: Error occurred

## Access Control

### Room Access Rules
1. **Public Rooms**: All authenticated users
2. **Private Rooms**: Only invited members or room admins
3. **Role-Based Rooms**: Users with the specified role
4. **Direct Rooms**: Only the participants
5. **Administrators**: Full access to all rooms

### Validation
- User authentication is required for all operations
- Room membership is verified before message sending
- Role claims are checked for role-based rooms
- System rooms cannot be deleted

## Scalability

The chat system is designed to support horizontal scaling:
- SignalR Groups are used for room-based broadcasting
- User-to-ConnectionId mapping supports multiple connections
- Ready for Redis backplane integration
- Stateless design (all state in database or backplane)

## Security

- **Authentication**: `[Authorize]` attribute on ChatHub
- **Authorization**: Role-based and membership-based access control
- **Anti-Forgery**: POST actions are protected with anti-forgery tokens
- **SQL Injection**: Entity Framework parameterized queries
- **XSS Protection**: Markdown sanitization (client-side)

## Next Steps for Implementation

### Frontend Components Needed
1. **Chat Controller**: MVC controller in Admin area
2. **Chat Index View**: Main chat UI
3. **JavaScript Client**: SignalR connection and event handling
4. **Markdown Renderer**: marked.js integration
5. **Notification System**: Sound and vibration implementation
6. **Room Management UI**: Create/invite/leave rooms

### Recommended Libraries
- **marked.js**: Markdown parsing
- **DOMPurify**: XSS sanitization for markdown
- **Howler.js** (optional): Advanced audio playback
- **Bootstrap 5**: UI components

## Future Enhancements

1. **File Sharing**: Attach files to messages
2. **Message Reactions**: Emoji reactions
3. **Message Editing/Deletion**: Modify or remove messages
4. **Search**: Full-text search across messages
5. **Push Notifications**: Browser push notifications
6. **Message Threading**: Reply threads for organized discussions
7. **User Mentions**: @ mentions with autocomplete
8. **Read Receipts**: More detailed read status tracking

## Logging

All operations are logged using ASP.NET Core's logging framework:
- Connection events
- Message sends
- Room operations
- Errors and exceptions

## Testing Considerations

- Test each room type (Public, Private, Role-Based, Direct)
- Verify all roles can access appropriate rooms
- Test with multiple concurrent users
- Verify message persistence and retrieval
- Test typing indicators and presence
- Verify notification muting functionality
- Test pagination with large message histories

## Support

For issues or questions, refer to:
- Project README: [`README.md`](../README.md)
- CLinerules: [`.clinerules`](../.clinerules)
- Other documentation: [`docs/`](../docs/)
