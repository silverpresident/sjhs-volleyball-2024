Help me improve this prompt for cline ai bot being used vs code. My project is a vollyball tournament management app and chat feature is for organizers and workers to communicate during the tournament. Since you are a highly expericed developer, use your experience to impove or fill in areas not covered inthe prompt. It already knows the context of my project:

My project is a vollyball tournament management app and chat feature is for organizers and workers to communicate during the tournament.
CReate a comphrensive realtime chat feature in the admin area, powered by signalR.
There should be a lobby where everyone can chat.
The ability to create specific rooms with specific users.
The ability to create a direct chat with a user and later make that into a private room.
Rooms should be open (any authenticated user can join), private (only added users can join), role based (only users with specific roles can join)
There should be a default room for
- Lobby (any)
- Management (private)
- Scorers (role)
- Support (private)
-Apart from the lobby which is open to everyone, users should be able to join, silence and leave rooms. When joined the notifications arrive ont he device, when silenced the user is not notified. When a notification arrives the device should vibrate slighy, more vigerously if the message is from a direct chat. There should also be a message chime.


==========
That's a **strong foundation** for your real-time chat feature! As an experienced developer, I can see you've covered the core functional requirements. The goal now is to add **security, scalability, persistence, and enhanced UX details** that the AI can implement to make the feature production-ready.

Here is an improved and more comprehensive prompt, structured by feature area:

## 💬 Comprehensive Real-Time Chat Feature Prompt (SignalR)
The chat feature is for organizers and workers to communicate during the tournament.

### 1. Core Architecture and Technology (SignalR)

* **Technology Stack:** Implement the chat feature using **ASP.NET Core SignalR Hubs** for bi-directional, real-time communication.
* **Authentication & Authorization:**
    * Protect the SignalR Hub with the `[Authorize]` attribute.
    * Map the authenticated user's ID/claims to the SignalR connection for sending messages to a **specific user** (direct/private chats).
    * Use **ASP.NET Core Identity** for user and role management.
* **Data Persistence:**
    * **Implement a database schema** (Entity Framework Core) for storing all chat data: `Rooms`, `Messages`, and `UserRoomMemberships` (including role/access level).
    * Ensure **all messages** sent are persisted before being broadcasted by the Hub.
* **Scalability (Advanced):** Design the system to be **horizontally scalable**. 

---

### 2. Room and Group Management

* **Room Types and Access Control:**
    * **Lobby:** Open to all authenticated users. Default join upon connection.
    * **Private Rooms:** Only members explicitly added by a room owner/admin can join. Check membership before allowing messages.
    * **Role-Based Rooms:** Only users with a specific role (e.g., "Scorer", "Admin", "Ref") can join. Check the user's role claim upon join attempt and before message delivery.
    * Each room the can have 1 or more designated admins
    * A system 'Administrator' role can access all rooms and has admin access in all rooms
* **Default System Rooms:**
    * **Lobby:** (Open) - General communication.
    * **Management:** (Private, Admin-only) - For key organizers, requires explicit invitation.
    * **Judges and Referee:** (Role-Based: `Judge` role) - For official referree communications.
    * **Scorers:** (Role-Based: `Scorer` role) - For official score-keeping communications.
    * **Announcers:** (Role-Based: `Announcer` role) - For official Announcer communications.
    * **Support:** (Private) - For support staff to handle escalated issues.
* **User Actions:**
    * **Join/Leave:** Implement methods for authenticated users to join/leave non-private rooms.
    * **Mute/Silence:** Implement a client-side and server-side mechanism to **silence a room**. When silenced, the user receives the message but **does not trigger notifications** (sound/vibration). Store this preference in the database (`UserRoomMemberships`).
    * **Room Creation:** Allow organizers (users with `Administrator` or `Organizer` roles) to create **new custom private rooms** and invite/remove specific users.
    * Allow any user to initiate a 1 on 1 chat with another user. Allow any of the users to add other users (making it a group chat)

---

### 3. User Experience (UX) and Messaging Features

* **Message History:** Implement a feature to load the **last 50 messages** upon joining a room to provide context. Implement **pagination** to fetch older messages upon request (e.g., scrolling up).
* **User Presence:** Display the **online/offline status** of users, particularly in direct chats and private rooms.
* **Typing Indicators:** Implement a subtle **"User is typing..."** indicator that is broadcasted only to other users in the same room/direct chat.
* **Notifications (Enhanced):**
    * **In-App Alerts:** When a new message arrives in an active or joined-but-not-active room, show a **visual badge** or **unseen message count** next to the room name.
    * **Sound:** Play a **message chime** for all incoming messages, *except* when the room is silenced.
    * **Vibration Feedback (Client-Side):**
        * **Standard:** Slight vibration/haptic feedback on the device for messages from group rooms.
        * **Priority:** A more vigorous or sustained vibration/haptic pattern for messages from **Direct Chats** to signal high priority.
* **Message Metadata:** Each message entity must store:
    * `SenderId`
    * `RoomId`/`ConversationId`
    * `Content` (Text)
    * `Timestamp`
    * (Optional but recommended: `ReadStatus` for private chats).



***
 
