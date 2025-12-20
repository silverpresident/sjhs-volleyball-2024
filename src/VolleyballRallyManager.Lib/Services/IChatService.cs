using VolleyballRallyManager.Lib.Models;

namespace VolleyballRallyManager.Lib.Services
{
    /// <summary>
    /// Service interface for chat operations
    /// </summary>
    public interface IChatService
    {
        // Room Management
        Task<ChatRoom?> GetRoomByIdAsync(Guid roomId);
        Task<IEnumerable<ChatRoom>> GetUserRoomsAsync(string userId);
        Task<IEnumerable<ChatRoom>> GetPublicRoomsAsync();
        Task<ChatRoom> CreateRoomAsync(string name, string? description, ChatRoomType roomType, string? requiredRole, string ownerId, bool isSystemRoom = false);
        Task<bool> DeleteRoom(Guid roomId);
        Task<ChatRoom?> GetOrCreateDirectRoomAsync(string user1Id, string user2Id);

        // Membership Management
        Task<bool> AddUserToRoomAsync(Guid roomId, string userId, bool isRoomAdmin = false);
        Task<bool> RemoveUserFromRoomAsync(Guid roomId, string userId);
        Task<bool> ToggleMuteRoomAsync(Guid roomId, string userId);
        Task<bool> IsUserInRoomAsync(Guid roomId, string userId);
        Task<bool> CanUserAccessRoomAsync(Guid roomId, string userId, IEnumerable<string> userRoles);
        Task<IEnumerable<string>> GetRoomMemberIdsAsync(Guid roomId);
        Task<ChatRoomMembership?> GetMembershipAsync(Guid roomId, string userId);
        Task UpdateLastReadAtAsync(Guid roomId, string userId);
        Task<int> GetUnreadCountAsync(Guid roomId, string userId);

        // Message Management
        Task<ChatMessage> SendMessageAsync(Guid roomId, string senderId, string senderName, string content);
        Task<IEnumerable<ChatMessage>> GetRecentMessagesAsync(Guid roomId, int count = 50);
        Task<IEnumerable<ChatMessage>> GetMessagesPaginatedAsync(Guid roomId, DateTime beforeTimestamp, int count = 50);
        Task<ChatMessage?> GetMessageByIdAsync(Guid messageId);
    }
}
