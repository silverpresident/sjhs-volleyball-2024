using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using VolleyballRallyManager.Lib.Data;
using VolleyballRallyManager.Lib.Hubs;
using VolleyballRallyManager.Lib.Models;
using VolleyballRallyManager.Lib.Services;

namespace VolleyballRallyManager.Admin.Controllers;

[Authorize]
public class AnnouncementsController : ApiControllerBase
{
    private readonly IAnnouncementService _announcementService;

    public AnnouncementsController(
        ApplicationDbContext context,
        IHubContext<MatchHub> hubContext,
        IAnnouncementService announcementService,
        ILogger<AnnouncementsController> logger)
        : base(context, hubContext, logger)
    {
        _announcementService = announcementService;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<Announcement>>> GetAnnouncements([FromQuery] bool includeHidden = false)
    {
        try
        {
            var announcements = await _announcementService.GetAnnouncementsAsync(includeHidden);
            return Ok(announcements);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting announcements");
            return StatusCode(500, "An error occurred while retrieving announcements");
        }
    }

    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<ActionResult<Announcement>> GetAnnouncement(Guid id)
    {
        try
        {
            var announcement = await _announcementService.GetAnnouncementAsync(id);

            if (announcement == null)
            {
                return NotFound($"Announcement with ID {id} not found.");
            }

            return Ok(announcement);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting announcement {AnnouncementId}", id);
            return StatusCode(500, "An error occurred while retrieving the announcement");
        }
    }

    [HttpPost]
    public async Task<ActionResult<Announcement>> CreateAnnouncement(Announcement announcement)
    {
        try
        {
            var createdAnnouncement = await _announcementService.CreateAnnouncementAsync(announcement);
            await _hubContext.Clients.All.SendAsync("AnnouncementAdded", createdAnnouncement);

            return CreatedAtAction(
                nameof(GetAnnouncement), 
                new { id = createdAnnouncement.Id }, 
                createdAnnouncement);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating announcement");
            return StatusCode(500, "An error occurred while creating the announcement");
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<Announcement>> UpdateAnnouncement(Guid id, Announcement announcement)
    {
        if (id != announcement.Id)
        {
            return BadRequest("ID mismatch");
        }

        try
        {
            var updatedAnnouncement = await _announcementService.UpdateAnnouncementAsync(announcement);
            await _hubContext.Clients.All.SendAsync("AnnouncementUpdated", updatedAnnouncement);

            return Ok(updatedAnnouncement);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating announcement {AnnouncementId}", id);
            return StatusCode(500, "An error occurred while updating the announcement");
        }
    }

    [HttpPut("{id}/visibility")]
    public async Task<ActionResult<Announcement>> UpdateAnnouncementVisibility(Guid id, [FromBody] bool isVisible)
    {
        try
        {
            var updatedAnnouncement = await _announcementService.UpdateAnnouncementVisibilityAsync(id, isVisible);
            await _hubContext.Clients.All.SendAsync("AnnouncementUpdated", updatedAnnouncement);

            return Ok(updatedAnnouncement);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating announcement visibility {AnnouncementId}", id);
            return StatusCode(500, "An error occurred while updating the announcement visibility");
        }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<Announcement>> DeleteAnnouncement(Guid id)
    {
        try
        {
            var success = await _announcementService.DeleteAnnouncementAsync(id);
            if (!success)
            {
                return NotFound($"Announcement with ID {id} not found.");
            }

            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting announcement {AnnouncementId}", id);
            return StatusCode(500, "An error occurred while deleting the announcement");
        }
    }
}
