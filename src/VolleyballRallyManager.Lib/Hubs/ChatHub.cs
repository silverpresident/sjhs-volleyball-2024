using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using VolleyballRallyManager.Lib.Models;
using VolleyballRallyManager.Lib.Services;

namespace VolleyballRallyManager.Lib.Hubs
{
    /// <summary>
    /// SignalR Hub for real-time chat functionality
    /// </summary>
    [Authorize]
    public class ChatHub : Hub
    {
        private readonly IChatService _chatService;
        private readonly ILogger<ChatHub> _logger;
        private static readonly Dictionary<string, string> _userConnections = new(); // userId -> connectionId
        private static readonly Dictionary<string, HashSet<string>> _roomConnections = new(); // roomId -> connectionIds

        public ChatHub(IChatService chatService, ILogger<ChatHub> logger)
        {
            _chatService = chatService;
            _logger = logger;
        }

        #region Connection Management

        public override async Task OnConnectedAsync()
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrEmpty(userId))
            {
                _userConnections[userId] = Context.ConnectionId;
                _logger.LogInformation("User {UserId} connected with ConnectionId {ConnectionId}", userId, Context.ConnectionId);

                // Notify others that this user is online
                await Clients.All.SendAsync("UserOnline", userId);

                // Join user's rooms
                var userRoles = Context.User?.Claims
                    .Where(c => c.Type == ClaimTypes.Role)
                    .Select(c => c.Value)
                    .ToList() ?? new List<string>();

                var userRooms = await _chatService.GetUserRoomsAsync(userId);
                foreach (var room in userRooms)
                {
                    await Groups.AddToGroupAsync(Context.ConnectionId, room.Id.ToString());

                    if (!_roomConnections.ContainsKey(room.Id.ToString()))
                    {
                        _roomConnections[room.Id.ToString()] = new HashSet<string>();
                    }
                    _roomConnections[room.Id.ToString()].Add(Context.ConnectionId);
                }

                // Auto-join public rooms
                var publicRooms = await _chatService.GetPublicRoomsAsync();
                foreach (var room in publicRooms)
                {
                    // Add membership if not already a member
                    if (!await _chatService.IsUserInRoomAsync(room.Id, userId))
                    {
                        await _chatService.AddUserToRoomAsync(room.Id, userId);
                    }
                    await Groups.AddToGroupAsync(Context.ConnectionId, room.Id.ToString());

                    if (!_roomConnections.ContainsKey(room.Id.ToString()))
                    {
                        _roomConnections[room.Id.ToString()] = new HashSet<string>();
                    }
                    _roomConnections[room.Id.ToString()].Add(Context.ConnectionId);
                }
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrEmpty(userId))
            {
                _userConnections.Remove(userId);
                _logger.LogInformation("User {UserId} disconnected", userId);

                // Notify others that this user is offline
                await Clients.All.SendAsync("UserOffline", userId);

                // Remove from room connections
                foreach (var roomConnections in _roomConnections.Values)
                {
                    roomConnections.Remove(Context.ConnectionId);
                }
            }

            await base.OnDisconnectedAsync(exception);
        }

        #endregion

        #region Room Operations

        /// <summary>
        /// Join a chat room
        /// </summary>
        public async Task JoinRoom(string roomId)
        {
            try
            {
                var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var userName = Context.User?.Identity?.Name ?? "Unknown";

                if (string.IsNullOrEmpty(userId))
                {
                    await Clients.Caller.SendAsync("Error", "User not authenticated");
                    return;
                }

                var roomGuid = Guid.Parse(roomId);
                var userRoles = Context.User?.Claims
                    .Where(c => c.Type == ClaimTypes.Role)
                    .Select(c => c.Value)
                    .ToList() ?? new List<string>();

                // Check if user can access this room
                if (!await _chatService.CanUserAccessRoomAsync(roomGuid, userId, userRoles))
                {
                    await Clients.Caller.SendAsync("Error", "You don't have access to this room");
                    return;
                }

                // Add user to room group
                await Groups.AddToGroupAsync(Context.ConnectionId, roomId);

                if (!_roomConnections.ContainsKey(roomId))
                {
                    _roomConnections[roomId] = new HashSet<string>();
                }
                _roomConnections[roomId].Add(Context.ConnectionId);

                // Add membership to database
                await _chatService.AddUserToRoomAsync(roomGuid, userId);

                // Notify room members
                await Clients.Group(roomId).SendAsync("UserJoinedRoom", roomId, userId, userName);

                _logger.LogInformation("User {UserId} joined room {RoomId}", userId, roomId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error joining room {RoomId}", roomId);
                await Clients.Caller.SendAsync("Error", "Failed to join room");
            }
        }

        /// <summary>
        /// Leave a chat room
        /// </summary>
        public async Task LeaveRoom(string roomId)
        {
            try
            {
                var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var userName = Context.User?.Identity?.Name ?? "Unknown";

                if (string.IsNullOrEmpty(userId))
                {
                    return;
                }

                await Groups.RemoveFromGroupAsync(Context.ConnectionId, roomId);

                if (_roomConnections.ContainsKey(roomId))
                {
                    _roomConnections[roomId].Remove(Context.ConnectionId);
                }

                // Remove membership from database (unless it's a system room)
                var room = await _chatService.GetRoomByIdAsync(Guid.Parse(roomId));
                if (room != null && !room.IsSystemRoom && room.RoomType != ChatRoomType.Public)
                {
                    await _chatService.RemoveUserFromRoomAsync(Guid.Parse(roomId), userId);
                }

                // Notify room members
                await Clients.Group(roomId).SendAsync("UserLeftRoom", roomId, userId, userName);

                _logger.LogInformation("User {UserId} left room {RoomId}", userId, roomId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error leaving room {RoomId}", roomId);
            }
        }

        #endregion

        #region Messaging

        /// <summary>
        /// Send a message to a room
        /// </summary>
        public async Task SendMessage(string roomId, string message)
        {
            try
            {
                var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var userName = Context.User?.Identity?.Name ?? "Unknown";

                if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(message))
                {
                    await Clients.Caller.SendAsync("Error", "Invalid message");
                    return;
                }

                var roomGuid = Guid.Parse(roomId);

                // Verify user is in the room
                if (!await _chatService.IsUserInRoomAsync(roomGuid, userId))
                {
                    await Clients.Caller.SendAsync("Error", "You are not a member of this room");
                    return;
                }

                // Save message to database
                var chatMessage = await _chatService.SendMessageAsync(roomGuid, userId, userName, message);

                // Broadcast message to room members
                await Clients.Group(roomId).SendAsync("ReceiveMessage", new
                {
                    messageId = chatMessage.Id,
                    roomId = roomId,
                    senderId = userId,
                    senderName = userName,
                    content = message,
                    timestamp = chatMessage.Timestamp
                });

                _logger.LogInformation("Message sent by {UserId} in room {RoomId}", userId, roomId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending message to room {RoomId}", roomId);
                await Clients.Caller.SendAsync("Error", "Failed to send message");
            }
        }

        /// <summary>
        /// Load message history with pagination
        /// </summary>
        public async Task LoadMessageHistory(string roomId, string? beforeTimestamp = null, int count = 50)
        {
            try
            {
                var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(userId))
                {
                    return;
                }

                var roomGuid = Guid.Parse(roomId);

                // Verify user is in the room
                if (!await _chatService.IsUserInRoomAsync(roomGuid, userId))
                {
                    await Clients.Caller.SendAsync("Error", "You are not a member of this room");
                    return;
                }

                IEnumerable<Models.ChatMessage> messages;

                if (string.IsNullOrEmpty(beforeTimestamp))
                {
                    messages = await _chatService.GetRecentMessagesAsync(roomGuid, count);
                }
                else
                {
                    var beforeDate = DateTime.Parse(beforeTimestamp);
                    messages = await _chatService.GetMessagesPaginatedAsync(roomGuid, beforeDate, count);
                }

                await Clients.Caller.SendAsync("MessageHistory", roomId, messages.Select(m => new
                {
                    messageId = m.Id,
                    roomId = m.ChatRoomId,
                    senderId = m.SenderId,
                    senderName = m.SenderName,
                    content = m.Content,
                    timestamp = m.Timestamp
                }).Reverse()); // Reverse to show oldest first

                // Mark as read
                await _chatService.UpdateLastReadAtAsync(roomGuid, userId);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading message history for room {RoomId}", roomId);
                await Clients.Caller.SendAsync("Error", "Failed to load message history");
            }
        }

        #endregion

        #region Typing Indicators

        /// <summary>
        /// Notify room members that user is typing
        /// </summary>
        public async Task UserTyping(string roomId)
        {
            try
            {
                var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var userName = Context.User?.Identity?.Name ?? "Unknown";

                if (string.IsNullOrEmpty(userId))
                {
                    return;
                }

                // Broadcast to others in the room (excluding caller)
               await Clients.OthersInGroup(roomId).SendAsync("UserTyping", roomId, userId, userName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error broadcasting typing indicator for room {RoomId}", roomId);
            }
        }

        /// <summary>
        /// Notify room members that user stopped typing
        /// </summary>
        public async Task UserStoppedTyping(string roomId)
        {
            try
            {
                var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(userId))
                {
                    return;
                }

                // Broadcast to others in the room (excluding caller)
                await Clients.OthersInGroup(roomId).SendAsync("UserStoppedTyping", roomId, userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error broadcasting stopped typing indicator for room {RoomId}", roomId);
            }
        }

        #endregion

        #region Room Management

        /// <summary>
        /// Create a new private room
        /// </summary>
        public async Task CreateRoom(string roomName, string description, bool isPrivate)
        {
            try
            {
                var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(userId))
                {
                    await Clients.Caller.SendAsync("Error", "User not authenticated");
                    return;
                }

                var roomType = isPrivate ? Models.ChatRoomType.Private : Models.ChatRoomType.Public;
                var room = await _chatService.CreateRoomAsync(roomName, description, roomType, null, userId);

                await Clients.Caller.SendAsync("RoomCreated", new
                {
                    roomId = room.Id,
                    name = room.Name,
                    description = room.Description,
                    roomType = room.RoomType.ToString()
                });

                _logger.LogInformation("User {UserId} created room {RoomName}", userId, roomName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating room");
                await Clients.Caller.SendAsync("Error", "Failed to create room");
            }
        }

        /// <summary>
        /// Toggle mute for a room
        /// </summary>
        public async Task ToggleMuteRoom(string roomId)
        {
            try
            {
                var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(userId))
                {
                    return;
                }

                var success = await _chatService.ToggleMuteRoomAsync(Guid.Parse(roomId), userId);

                if (success)
                {
                    var membership = await _chatService.GetMembershipAsync(Guid.Parse(roomId), userId);
                    await Clients.Caller.SendAsync("RoomMuteToggled", roomId, membership?.IsMuted ?? false);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling mute for room {RoomId}", roomId);
            }
        }

        public async Task GetUnreadCount(string roomId)
        {
            try
            {
                var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(userId))
                {
                    return;
                }

                var count = await _chatService.GetUnreadCountAsync(Guid.Parse(roomId), userId);
                await Clients.Caller.SendAsync("UnreadCount", roomId, count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting unread count for room {RoomId}", roomId);
            }
        }

        /// <summary>
        /// Add a user to a room (for private rooms or direct chats)
        /// </summary>
        public async Task AddUserToRoom(string roomId, string targetUserId)
        {
            try
            {
                var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(userId))
                {
                    await Clients.Caller.SendAsync("Error", "User not authenticated");
                    return;
                }

                var userRoles = Context.User?.Claims
                    .Where(c => c.Type == ClaimTypes.Role)
                    .Select(c => c.Value)
                    .ToList() ?? new List<string>();

                var roomGuid = Guid.Parse(roomId);
                var room = await _chatService.GetRoomByIdAsync(roomGuid);

                if (room == null)
                {
                    await Clients.Caller.SendAsync("Error", "Room not found");
                    return;
                }

                // Check if user has permission to add members
                var membership = await _chatService.GetMembershipAsync(roomGuid, userId);
                bool isAdmin = userRoles.Contains("Administrator");
                bool isRoomAdmin = membership?.IsRoomAdmin ?? false;
                bool isOwner = room.OwnerId == userId;

                if (!isAdmin && !isRoomAdmin && !isOwner)
                {
                    await Clients.Caller.SendAsync("Error", "You don't have permission to add members to this room");
                    return;
                }

                // Add the user to the room
                var success = await _chatService.AddUserToRoomAsync(roomGuid, targetUserId);

                if (success)
                {
                    await Clients.Caller.SendAsync("UserAddedToRoom", roomId, targetUserId);
                    
                    // Notify the added user if they're online
                    if (_userConnections.TryGetValue(targetUserId, out var targetConnectionId))
                    {
                        await Clients.Client(targetConnectionId).SendAsync("AddedToRoom", new {
                            roomId = room.Id,
                            name = room.Name,
                            description = room.Description,
                            roomType = room.RoomType.ToString()
                        });
                    }

                    _logger.LogInformation("User {UserId} added user {TargetUserId} to room {RoomId}",
                        userId, targetUserId, roomId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding user to room {RoomId}", roomId);
                await Clients.Caller.SendAsync("Error", "Failed to add user to room");
            }
        }

        /// <summary>
        /// Remove a user from a room
        /// </summary>
        public async Task RemoveUserFromRoom(string roomId, string targetUserId)
        {
            try
            {
                var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(userId))
                {
                    await Clients.Caller.SendAsync("Error", "User not authenticated");
                    return;
                }

                var userRoles = Context.User?.Claims
                    .Where(c => c.Type == ClaimTypes.Role)
                    .Select(c => c.Value)
                    .ToList() ?? new List<string>();

                var roomGuid = Guid.Parse(roomId);
                var room = await _chatService.GetRoomByIdAsync(roomGuid);

                if (room == null)
                {
                    await Clients.Caller.SendAsync("Error", "Room not found");
                    return;
                }

                // Check if user has permission to remove members
                var membership = await _chatService.GetMembershipAsync(roomGuid, userId);
                bool isAdmin = userRoles.Contains("Administrator");
                bool isRoomAdmin = membership?.IsRoomAdmin ?? false;
                bool isOwner = room.OwnerId == userId;

                if (!isAdmin && !isRoomAdmin && !isOwner)
                {
                    await Clients.Caller.SendAsync("Error", "You don't have permission to remove members from this room");
                    return;
                }

                // Remove the user from the room
                var success = await _chatService.RemoveUserFromRoomAsync(roomGuid, targetUserId);

                if (success)
                {
                    await Clients.Caller.SendAsync("UserRemovedFromRoom", roomId, targetUserId);
                    
                    // Notify the removed user if they're online
                    if (_userConnections.TryGetValue(targetUserId, out var targetConnectionId))
                    {
                        await Clients.Client(targetConnectionId).SendAsync("RemovedFromRoom", roomId, room.Name);
                    }

                    _logger.LogInformation("User {UserId} removed user {TargetUserId} from room {RoomId}",
                        userId, targetUserId, roomId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing user from room {RoomId}", roomId);
                await Clients.Caller.SendAsync("Error", "Failed to remove user from room");
            }
        }

        /// <summary>
        /// Get list of members in a room
        /// </summary>
        public async Task GetRoomMembers(string roomId)
        {
            try
            {
                var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(userId))
                {
                    return;
                }

                var roomGuid = Guid.Parse(roomId);
                
                // Verify user is in the room
                if (!await _chatService.IsUserInRoomAsync(roomGuid, userId))
                {
                    await Clients.Caller.SendAsync("Error", "You are not a member of this room");
                    return;
                }

                var memberIds = await _chatService.GetRoomMemberIdsAsync(roomGuid);
                await Clients.Caller.SendAsync("RoomMembers", roomId, memberIds);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting members for room {RoomId}", roomId);
                await Clients.Caller.SendAsync("Error", "Failed to get room members");
            }
        }

        #endregion
    }
}
