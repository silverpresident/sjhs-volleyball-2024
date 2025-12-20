namespace VolleyballRallyManager.Lib.Models
{
    /// <summary>
    /// Defines the type of chat room
    /// </summary>
    public enum ChatRoomType
    {
        /// <summary>
        /// Public room - Open to all authenticated users
        /// </summary>
        Public,

        /// <summary>
        /// Private room - Only invited members can join
        /// </summary>
        Private,

        /// <summary>
        /// Role-based room - Only users with specific role can join
        /// </summary>
        RoleBased,

        /// <summary>
        /// Direct/One-on-one chat between two users
        /// </summary>
        Direct
    }
}
