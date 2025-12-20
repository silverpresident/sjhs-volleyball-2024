using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using VolleyballRallyManager.Lib.Data;
using VolleyballRallyManager.Lib.Models;

namespace VolleyballRallyManager.Lib.Services
{
    /// <summary>
    /// Service for managing chat rooms, messages, and memberships
    /// </summary>
    public class ChatService : IChatService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ChatService> _logger;

        public ChatService(ApplicationDbContext context, ILogger<ChatService> logger)
        {
            _context = context;
            _logger = logger;
        }

        #region Room Management

        public async Task<ChatRoom?> GetRoomByIdAsync(Guid roomId)
        {
            try
            {
                return await _context.ChatRooms
                    .Include(r => r.Memberships)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(r => r.Id == roomId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting room by ID: {RoomId}", roomId);
                throw;
            }
        }

        public async Task<IEnumerable<ChatRoom>> GetUserRoomsAsync(string userId)
        {
            try
            {
                // Get rooms where user is a member or public rooms
                var memberRooms = await _context.ChatRoomMemberships
                    .Where(m => m.UserId == userId)
                    .Select(m => m.ChatRoom)
                    .AsNoTracking()
                    .ToListAsync();

                return memberRooms;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user rooms for user: {UserId}", userId);
                throw;
            }
        }

        public async Task<IEnumerable<ChatRoom>> GetPublicRoomsAsync()
        {
            try
            {
                return await _context.ChatRooms
                    .Where(r => r.RoomType == ChatRoomType.Public)
                    .AsNoTracking()
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting public rooms");
                throw;
            }
        }

        public async Task<ChatRoom> CreateRoomAsync(string name, string? description, ChatRoomType roomType, string? requiredRole, string ownerId, bool isSystemRoom = false)
        {
            try
            {
                var room = new ChatRoom
                {
                    Id = Guid.NewGuid(),
                    Name = name,
                    Description = description,
                    RoomType = roomType,
                    RequiredRole = requiredRole,
                    OwnerId = ownerId,
                    IsSystemRoom = isSystemRoom,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now,
                    CreatedBy = ownerId,
                    UpdatedBy = ownerId
                };

                _context.ChatRooms.Add(room);

                // Add owner as room admin
                var membership = new ChatRoomMembership
                {
                    Id = Guid.NewGuid(),
                    ChatRoomId = room.Id,
                    UserId = ownerId,
                    IsRoomAdmin = true,
                    JoinedAt = DateTime.Now,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now,
                    CreatedBy = ownerId,
                    UpdatedBy = ownerId
                };
                _context.ChatRoomMemberships.Add(membership);

                await _context.SaveChangesAsync();

                _logger.LogInformation("Created chat room: {RoomName} ({RoomId})", name, room.Id);
                return room;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating room: {RoomName}", name);
                throw;
            }
        }

        public async Task<bool> DeleteRoom(Guid roomId)
        {
            try
            {
                var room = await _context.ChatRooms.FindAsync(roomId);
                if (room == null || room.IsSystemRoom)
                {
                    return false;
                }

                _context.ChatRooms.Remove(room);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Deleted chat room: {RoomId}", roomId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting room: {RoomId}", roomId);
                throw;
            }
        }

        public async Task<ChatRoom?> GetOrCreateDirectRoomAsync(string user1Id, string user2Id)
        {
            try
            {
                // Check if a direct room already exists between these two users
                var existingRoom = await _context.ChatRooms
                    .Where(r => r.RoomType == ChatRoomType.Direct)
                    .Where(r => r.Memberships.Any(m => m.UserId == user1Id) && 
                                r.Memberships.Any(m => m.UserId == user2Id))
                    .FirstOrDefaultAsync();

                if (existingRoom != null)
                {
                    return existingRoom;
                }

                // Create a new direct room
                var room = new ChatRoom
                {
                    Id = Guid.NewGuid(),
                    Name = $"Direct: {user1Id} - {user2Id}",
                    RoomType = ChatRoomType.Direct,
                    OwnerId = user1Id,
                    IsSystemRoom = false,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now,
                    CreatedBy = user1Id,
                    UpdatedBy = user1Id
                };

                _context.ChatRooms.Add(room);
                await _context.SaveChangesAsync();

                // Add both users as members
                await AddUserToRoomAsync(room.Id, user1Id);
                await AddUserToRoomAsync(room.Id, user2Id);

                _logger.LogInformation("Created direct room between {User1} and {User2}", user1Id, user2Id);
                return room;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating direct room between {User1} and {User2}", user1Id, user2Id);
                throw;
            }
        }

        #endregion

        #region Membership Management

        public async Task<bool> AddUserToRoomAsync(Guid roomId, string userId, bool isRoomAdmin = false)
        {
            try
            {
                // Check if membership already exists
                var existingMembership = await _context.ChatRoomMemberships
                    .FirstOrDefaultAsync(m => m.ChatRoomId == roomId && m.UserId == userId);

                if (existingMembership != null)
                {
                    return true; // Already a member
                }

                var membership = new ChatRoomMembership
                {
                    Id = Guid.NewGuid(),
                    ChatRoomId = roomId,
                    UserId = userId,
                    IsRoomAdmin = isRoomAdmin,
                    JoinedAt = DateTime.Now,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now,
                    CreatedBy = userId,
                    UpdatedBy = userId
                };

                _context.ChatRoomMemberships.Add(membership);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Added user {UserId} to room {RoomId}", userId, roomId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding user {UserId} to room {RoomId}", userId, roomId);
                return false;
            }
        }

        public async Task<bool> RemoveUserFromRoomAsync(Guid roomId, string userId)
        {
            try
            {
                var membership = await _context.ChatRoomMemberships
                    .FirstOrDefaultAsync(m => m.ChatRoomId == roomId && m.UserId == userId);

                if (membership == null)
                {
                    return false;
                }

                _context.ChatRoomMemberships.Remove(membership);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Removed user {UserId} from room {RoomId}", userId, roomId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing user {UserId} from room {RoomId}", userId, roomId);
                return false;
            }
        }

        public async Task<bool> ToggleMuteRoomAsync(Guid roomId, string userId)
        {
            try
            {
                var membership = await _context.ChatRoomMemberships
                    .FirstOrDefaultAsync(m => m.ChatRoomId == roomId && m.UserId == userId);

                if (membership == null)
                {
                    return false;
                }

                membership.IsMuted = !membership.IsMuted;
                membership.UpdatedAt = DateTime.Now;
                membership.UpdatedBy = userId;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Toggled mute for user {UserId} in room {RoomId} to {IsMuted}", 
                    userId, roomId, membership.IsMuted);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling mute for user {UserId} in room {RoomId}", userId, roomId);
                return false;
            }
        }

        public async Task<bool> IsUserInRoomAsync(Guid roomId, string userId)
        {
            try
            {
                return await _context.ChatRoomMemberships
                    .AnyAsync(m => m.ChatRoomId == roomId && m.UserId == userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if user {UserId} is in room {RoomId}", userId, roomId);
                return false;
            }
        }

        public async Task<bool> CanUserAccessRoomAsync(Guid roomId, string userId, IEnumerable<string> userRoles)
        {
            try
            {
                var room = await GetRoomByIdAsync(roomId);
                if (room == null)
                {
                    return false;
                }

                // Public rooms and Lobby are accessible to all
                if (room.RoomType == ChatRoomType.Public)
                {
                    return true;
                }

                // Check if user is owner or Administrator
                if (room.OwnerId == userId || userRoles.Contains("Administrator"))
                {
                    return true;
                }

                // For role-based rooms, check if user has the required role
                if (room.RoomType == ChatRoomType.RoleBased && !string.IsNullOrEmpty(room.RequiredRole))
                {
                    return userRoles.Contains(room.RequiredRole);
                }

                // For private and direct rooms, check membership
                return await IsUserInRoomAsync(roomId, userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking access for user {UserId} to room {RoomId}", userId, roomId);
                return false;
            }
        }

        public async Task<IEnumerable<string>> GetRoomMemberIdsAsync(Guid roomId)
        {
            try
            {
                return await _context.ChatRoomMemberships
                    .Where(m => m.ChatRoomId == roomId)
                    .Select(m => m.UserId)
                    .AsNoTracking()
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting members for room {RoomId}", roomId);
                throw;
            }
        }

        public async Task<ChatRoomMembership?> GetMembershipAsync(Guid roomId, string userId)
        {
            try
            {
                return await _context.ChatRoomMemberships
                    .AsNoTracking()
                    .FirstOrDefaultAsync(m => m.ChatRoomId == roomId && m.UserId == userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting membership for user {UserId} in room {RoomId}", userId, roomId);
                throw;
            }
        }

        public async Task UpdateLastReadAtAsync(Guid roomId, string userId)
        {
            try
            {
                var membership = await _context.ChatRoomMemberships
                    .FirstOrDefaultAsync(m => m.ChatRoomId == roomId && m.UserId == userId);

                if (membership != null)
                {
                    membership.LastReadAt = DateTime.Now;
                    membership.UpdatedAt = DateTime.Now;
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating last read for user {UserId} in room {RoomId}", userId, roomId);
            }
        }

        public async Task<int> GetUnreadCountAsync(Guid roomId, string userId)
        {
            try
            {
                var membership = await GetMembershipAsync(roomId, userId);
                if (membership == null || !membership.LastReadAt.HasValue)
                {
                    return await _context.ChatMessages
                        .Where(m => m.ChatRoomId == roomId)
                        .CountAsync();
                }

                return await _context.ChatMessages
                    .Where(m => m.ChatRoomId == roomId && m.Timestamp > membership.LastReadAt.Value)
                    .CountAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting unread count for user {UserId} in room {RoomId}", userId, roomId);
                return 0;
            }
        }

        #endregion

        #region Message Management

        public async Task<ChatMessage> SendMessageAsync(Guid roomId, string senderId, string senderName, string content)
        {
            try
            {
                var message = new ChatMessage
                {
                    Id = Guid.NewGuid(),
                    ChatRoomId = roomId,
                    SenderId = senderId,
                    SenderName = senderName,
                    Content = content,
                    Timestamp = DateTime.Now,
                    IsRead = false,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now,
                    CreatedBy = senderId,
                    UpdatedBy = senderId
                };

                _context.ChatMessages.Add(message);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Message sent by {SenderId} in room {RoomId}", senderId, roomId);
                return message;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending message in room {RoomId}", roomId);
                throw;
            }
        }

        public async Task<IEnumerable<ChatMessage>> GetRecentMessagesAsync(Guid roomId, int count = 50)
        {
            try
            {
                return await _context.ChatMessages
                    .Where(m => m.ChatRoomId == roomId)
                    .OrderByDescending(m => m.Timestamp)
                    .Take(count)
                    .AsNoTracking()
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting recent messages for room {RoomId}", roomId);
                throw;
            }
        }

        public async Task<IEnumerable<ChatMessage>> GetMessagesPaginatedAsync(Guid roomId, DateTime beforeTimestamp, int count = 50)
        {
            try
            {
                return await _context.ChatMessages
                    .Where(m => m.ChatRoomId == roomId && m.Timestamp < beforeTimestamp)
                    .OrderByDescending(m => m.Timestamp)
                    .Take(count)
                    .AsNoTracking()
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting paginated messages for room {RoomId}", roomId);
                throw;
            }
        }

        public async Task<ChatMessage?> GetMessageByIdAsync(Guid messageId)
        {
            try
            {
                return await _context.ChatMessages
                    .AsNoTracking()
                    .FirstOrDefaultAsync(m => m.Id == messageId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting message by ID: {MessageId}", messageId);
                throw;
            }
        }

        #endregion
    }
}
