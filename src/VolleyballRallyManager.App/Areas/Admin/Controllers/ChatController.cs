using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using VolleyballRallyManager.Lib.Services;

namespace VolleyballRallyManager.App.Areas.Admin.Controllers
{
    /// <summary>
    /// Controller for chat operations
    /// </summary>
    [Authorize]
    [Area("Admin")]
    public class ChatController : Controller
    {
        private readonly IChatService _chatService;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ILogger<ChatController> _logger;

        public ChatController(IChatService chatService, UserManager<IdentityUser> userManager, ILogger<ChatController> logger)
        {
            _chatService = chatService;
            _userManager = userManager;
            _logger = logger;
        }

        /// <summary>
        /// Display the main chat interface
        /// </summary>
        public async Task<IActionResult> Index()
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    _logger.LogWarning("User not found for chat");
                    return RedirectToAction("Login", "Account", new { area = "Identity" });
                }

                // Get user's rooms
                var userRooms = await _chatService.GetUserRoomsAsync(user.Id);

                // Get public rooms
                var publicRooms = await _chatService.GetPublicRoomsAsync();

                // Combine and deduplicate
                var allRooms = userRooms.Union(publicRooms, new RoomComparer()).OrderBy(r => r.Name);

                ViewBag.CurrentUserId = user.Id;
                ViewBag.CurrentUserName = user.UserName;
                ViewBag.UserRoles = await _userManager.GetRolesAsync(user);

                return View(allRooms);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading chat page");
                return View("Error");
            }
        }

        /// <summary>
        /// Get all users for inviting to private rooms
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetUsers()
        {
            try
            {
                var users = _userManager.Users
                    .Select(u => new { id = u.Id, name = u.UserName, email = u.Email })
                    .ToList();

                return Json(users);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting users");
                return BadRequest("Failed to get users");
            }
        }

        /// <summary>
        /// Custom comparer to deduplicate rooms
        /// </summary>
        private class RoomComparer : IEqualityComparer<VolleyballRallyManager.Lib.Models.ChatRoom>
        {
            public bool Equals(VolleyballRallyManager.Lib.Models.ChatRoom? x, VolleyballRallyManager.Lib.Models.ChatRoom? y)
            {
                if (x == null || y == null) return false;
                return x.Id == y.Id;
            }

            public int GetHashCode(VolleyballRallyManager.Lib.Models.ChatRoom obj)
            {
                return obj.Id.GetHashCode();
            }
        }
    }
}
