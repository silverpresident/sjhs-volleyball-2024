namespace VolleyballRallyManager.Lib.Models
{
    /// <summary>
    /// Represents a message sent in a chat room
    /// </summary>
    public class ChatMessage : BaseEntity
    {
        /// <summary>
        /// ID of the chat room this message belongs to
        /// </summary>
        public Guid ChatRoomId { get; set; }

        /// <summary>
        /// Navigation property to the chat room
        /// </summary>
        public virtual ChatRoom ChatRoom { get; set; } = null!;

        /// <summary>
        /// User ID of the sender
        /// </summary>
        public string SenderId { get; set; } = string.Empty;

        /// <summary>
        /// Display name of the sender at the time of sending
        /// </summary>
        public string SenderName { get; set; } = string.Empty;

        /// <summary>
        /// Message content (supports markdown)
        /// </summary>
        public string Content { get; set; } = string.Empty;

        /// <summary>
        /// Timestamp when the message was sent
        /// </summary>
        public DateTime Timestamp { get; set; } = DateTime.Now;

        /// <summary>
        /// Indicates if the message has been read (for direct chats)
        /// </summary>
        public bool IsRead { get; set; }
    }
}
