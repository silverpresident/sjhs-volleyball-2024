using Microsoft.AspNetCore.Identity;
using System;

namespace VolleyballRallyManager.Lib.Models;

public class ApplicationUser : IdentityUser
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiryTime { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastLoginAt { get; set; }
    
    // Navigation properties
    public virtual ICollection<MatchUpdate> Updates { get; set; } = new List<MatchUpdate>();
    public virtual ICollection<Announcement> Announcements { get; set; } = new List<Announcement>();
}
