# Chat Member Management Guide

## Overview
The chat feature includes comprehensive member management capabilities that allow room owners and administrators to control who can access private rooms and direct chats.

## Features

### 1. Room Member Management

#### Who Can Manage Members?
- **System Administrators**: Full access to manage all rooms
- **Room Owner**: Original creator of the room
- **Room Admins**: Users designated as admins for specific rooms

#### Access the Members Panel
1. Join a private room or direct chat
2. Click the **"Members"** button in the chat header
3. The "Manage Room Members" modal will open

### 2. View Room Members

The members modal displays:
- **Current Members List**: All users currently in the room
- **Remove Button**: Next to each member (except yourself)
- **Your Badge**: Shows "You" badge next to your name

### 3. Add Members to a Room

#### Steps to Add a Member:
1. Open the "Manage Room Members" modal
2. Select a user from the dropdown in the "Add Member" section
3. Click the **"Add"** button
4. The user will be immediately added to the room

#### What Happens:
- User is added to the database as a room member
- User receives a notification: "You were added to [Room Name]"
- Room appears in the user's room list
- If user is online, they can immediately start chatting

#### Backend Method:
```csharp
// ChatHub.cs
public async Task AddUserToRoom(string roomId, string targetUserId)
```

### 4. Remove Members from a Room

#### Steps to Remove a Member:
1. Open the "Manage Room Members" modal
2. Find the member you want to remove
3. Click the **"Remove"** button next to their name
4. Confirm the removal

#### What Happens:
- User is removed from the database membership
- User receives a notification: "You were removed from [Room Name]"
- Room disappears from the user's room list
- If user is currently viewing the room, it's closed automatically

#### Backend Method:
```csharp
// ChatHub.cs
public async Task RemoveUserFromRoom(string roomId, string targetUserId)
```

### 5. Member Management Permissions

#### Permission Matrix:

| Role/Status | View Members | Add Members | Remove Members |
|-------------|--------------|-------------|-----------------|
| System Administrator | ✅ Yes | ✅ Yes | ✅ Yes |
| Room Owner | ✅ Yes | ✅ Yes | ✅ Yes |
| Room Admin | ✅ Yes | ✅ Yes | ✅ Yes |
| Regular Member | ✅ Yes | ❌ No | ❌ No |

#### Special Cases:
- **Public Rooms**: No member management needed (all users auto-join)
- **Role-Based Rooms**: Members are determined by role, manual management not allowed
- **System Rooms**: Cannot be deleted or have members forcibly removed

### 6. UI Components

#### Members Button
Located in the chat header, visible only for:
- Private rooms
- Direct chats

```html
<button class="btn btn-sm btn-outline-primary" id="manage-members-btn">
    <i class="bi bi-people"></i> Members
</button>
```

#### Manage Members Modal
Contains two sections:
1. **Current Members** - List of all room members with remove buttons
2. **Add Member** - Dropdown to select users and add them

### 7. SignalR Events

#### Server→Client Events:
- **`UserAddedToRoom(roomId, userId)`**: Member was added to a room
- **`UserRemovedFromRoom(roomId, userId)`**: Member was removed from a room
- **`AddedToRoom(roomInfo)`**: You were added to a new room
- **`RemovedFromRoom(roomId, roomName)`**: You were removed from a room
- **`RoomMembers(roomId, memberIds)`**: List of current room members

#### Client→Server Methods:
- **`AddUserToRoom(roomId, targetUserId)`**: Add a member to the room
- **`RemoveUserFromRoom(roomId, targetUserId)`**: Remove a member from the room
- **`GetRoomMembers(roomId)`**: Request list of current members

### 8. JavaScript Implementation

#### Files:
- **[`chat.js`](../src/VolleyballRallyManager.App/wwwroot/js/chat.js)**: Core chat functionality
- **[`chat-members.js`](../src/VolleyballRallyManager.App/wwwroot/js/chat-members.js)**: Member management functionality

#### State Management:
The chat state is exposed globally as `window.chatState` to allow chat-members.js to access:
- `currentRoomId`
- `allUsers`
- `connection`

### 9. Direct (1-on-1) Chat Member Management

#### Starting a Direct Chat:
While not yet exposed in the UI with a dedicated button, the backend supports:

```javascript
// Example: Start a direct chat with another user
connection.invoke('CreateDirectChat', 'target-user-id')
    .then(roomInfo => {
        console.log('Direct chat created:', roomInfo);
    });
}
```

#### Converting to Group Chat:
Once in a direct chat:
1. Click "Members" button
2. Add additional users using the "Add Member" section
3. Room automatically converts to a group chat

### 10. Backend Service Methods

The [`ChatService`](../src/VolleyballRallyManager.Lib/Services/ChatService.cs) provides:

```csharp
// Add user to room
Task<bool> AddUserToRoomAsync(Guid roomId, string userId, bool isRoomAdmin = false);

// Remove user from room
Task<bool> RemoveUserFromRoomAsync(Guid roomId, string userId);

// Get all member IDs
Task<IEnumerable<string>> GetRoomMemberIdsAsync(Guid roomId);

// Check if user is in room
Task<bool> IsUserInRoomAsync(Guid roomId, string userId);

// Get or create direct room between two users
Task<ChatRoom?> GetOrCreateDirectRoomAsync(string user1Id, string user2Id);
```

### 11. Security Considerations

#### Access Control:
- Permission checks are enforced server-side in [`ChatHub`](../src/VolleyballRallyManager.Lib/Hubs/ChatHub.cs)
- Users cannot add/remove members unless they have appropriate permissions
- System rooms are protected from unauthorized changes
- User authentication is required for all operations

#### Validation:
- Room existence is verified
- User existence is verified via Identity
- Membership status is checked before operations
- Duplicate memberships are prevented by database constraint

### 12. Troubleshooting

#### Members Modal Won't Open
- **Check**: Is the "Members" button visible?
- **Solution**: Only appears for Private and Direct rooms

#### Can't Add Members
- **Check**: Do you have permission (Owner/Admin/Administrator)?
- **Check**: Browser console for error messages
- **Solution**: Contact room owner to make you an admin

#### User Not Appearing in Dropdown
- **Check**: User must have an account in the system
- **Check**: Browser console - users loaded successfully?
- **Solution**: Refresh the page or check `/Admin/UserManagement`

#### Member Removal Failed
- **Check**: You cannot remove yourself
- **Check**: Do you have permission?
- **Solution**: Verify you're a room owner or admin

### 13. Future Enhancements

Potential improvements for member management:
- **Promote to Admin**: Allow room owners to promote members to admin status
- **Transfer Ownership**: Transfer room ownership to another user
- **Bulk Actions**: Add/remove multiple users at once
- **User Search**: Search/filter users when adding members
- **Member Roles**: Assign specific roles within rooms (moderator, guest, etc.)
- **Member Limits**: Set maximum member count for rooms
- **Invite Links**: Generate shareable invite links
- **Join Requests**: Allow users to request to join private rooms

### 14. Examples

#### Example: Create Private Room and Add Members
1. Click "Create Room"
2. Enter room name: "Project Team"
3. Check "Make this room private"
4. Click "Create Room"
5. Click "Members" on the new room
6. Select team members from dropdown
7. Click "Add" for each member

#### Example: Remove Inactive Member
1. Open the room
2. Click "Members"
3. Find the inactive user in the list
4. Click "Remove" next to their name
5. Confirm removal

## API Reference

### Hub Methods

```csharp
// Add a user to a room
await connection.InvokeAsync("AddUserToRoom", roomId, userId);

// Remove a user from a room
await connection.InvokeAsync("RemoveUserFromRoom", roomId, userId);

// Get room members
await connection.InvokeAsync("GetRoomMembers", roomId);
```

### JavaScript Functions

```javascript
// Open the members modal
openMembersModal();

// Add a member (called by Add button)
addMemberToRoom();

// Remove a member (called by Remove button)
removeMember(userId, userName);

// Load and display members
loadRoomMembers();
displayRoomMembers(memberIds);
```

## Related Documentation

- **Main Chat Documentation**: [`CHAT_FEATURE.md`](CHAT_FEATURE.md)
- **Setup Guide**: [`CHAT_SETUP.md`](CHAT_SETUP.md)
- **User Management**: [`USER_MANAGEMENT_GUIDE.md`](USER_MANAGEMENT_GUIDE.md)
