using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TaskManagement.Services;

namespace TaskManagement.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class NotificationsController : ControllerBase
{
    private readonly INotificationService _notificationService;

    public NotificationsController(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    [HttpGet]
    public async Task<IActionResult> GetNotifications()
    {
        var userId = GetCurrentUserId();
        var notifications = await _notificationService.GetByUserAsync(userId);
        return Ok(notifications);
    }

    [HttpGet("unread-count")]
    public async Task<IActionResult> GetUnreadCount()
    {
        var userId = GetCurrentUserId();
        var count = await _notificationService.GetUnreadCountAsync(userId);
        return Ok(new { Count = count });
    }

    [HttpPut("{id:guid}/read")]
    public async Task<IActionResult> MarkAsRead(Guid id)
    {
        var userId = GetCurrentUserId();
        var result = await _notificationService.MarkAsReadAsync(id, userId);
        if (!result) return NotFound();
        return NoContent();
    }

    [HttpPut("read-all")]
    public async Task<IActionResult> MarkAllAsRead()
    {
        var userId = GetCurrentUserId();
        await _notificationService.MarkAllAsReadAsync(userId);
        return NoContent();
    }

    private Guid GetCurrentUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (claim == null) throw new UnauthorizedAccessException("Không tìm thấy thông tin user");
        return Guid.Parse(claim.Value);
    }
}
