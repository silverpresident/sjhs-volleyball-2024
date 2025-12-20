namespace VolleyballRallyManager.Lib.Models
{
    /// <summary>
    /// Represents a chat room where users can communicate
    /// </summary>
    public class ChatRoom : BaseEntity
    {
        /// <summary>
        /// Name of the chat room
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Description of the chat room
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Type of the room (Public, Private, RoleBased, Direct)
        /// </summary>
        public ChatRoomType RoomType { get; set; }

        /// <summary>
        /// If RoomType is RoleBased, this specifies the required role
        /// </summary>
        public string? RequiredRole { get; set; }

        /// <summary>
        /// Indicates whether this is a system-created room (cannot be deleted by users)
        /// </summary>
        public bool IsSystemRoom { get; set; }

        /// <summary>
        /// User ID of the room owner/creator (for private rooms)
        /// </summary>
        public string? OwnerId { get; set; }

        /// <summary>
        /// Navigation property for room memberships
        /// </summary>
        public virtual ICollection<ChatRoomMembership> Memberships { get; set; } = new List<ChatRoomMembership>();

        /// <summary>
        /// Navigation property for messages in this room
        /// </summary>
        public virtual ICollection<ChatMessage> Messages { get; set; } = new List<ChatMessage>();
    }
}
