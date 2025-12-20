namespace VolleyballRallyManager.Lib.Models
{
    /// <summary>
    /// Represents a user's membership in a chat room
    /// </summary>
    public class ChatRoomMembership : BaseEntity
    {
        /// <summary>
        /// ID of the chat room
        /// </summary>
        public Guid ChatRoomId { get; set; }

        /// <summary>
        /// Navigation property to the chat room
        /// </summary>
        public virtual ChatRoom ChatRoom { get; set; } = null!;

        /// <summary>
        /// User ID of the member
        /// </summary>
        public string UserId { get; set; } = string.Empty;

        /// <summary>
        /// Indicates if the user is a room admin (can invite/remove others, for private rooms)
        /// </summary>
        public bool IsRoomAdmin { get; set; }

        /// <summary>
        /// Indicates if the room is muted for this user (no notifications)
        /// </summary>
        public bool IsMuted { get; set; }

        /// <summary>
        /// Timestamp when the user joined the room
        /// </summary>
        public DateTime JoinedAt { get; set; } = DateTime.Now;

        /// <summary>
        /// Last time the user read messages in this room (for unread count)
        /// </summary>
        public DateTime? LastReadAt { get; set; }
    }
}
