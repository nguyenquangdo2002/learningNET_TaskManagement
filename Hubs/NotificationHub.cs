using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace TaskManagement.Hubs;

/// <summary>
/// SignalR Hub cho real-time notification.
/// Mỗi user được add vào 1 Group riêng (group name = "user_{userId}").
/// Khi cần gửi notification cho user cụ thể → gửi vào group đó.
/// </summary>
[Authorize]
public class NotificationHub : Hub
{
    /// <summary>
    /// Khi client connect → tự động add vào group theo userId từ JWT
    /// </summary>
    public override async Task OnConnectedAsync()
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId != null)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"user_{userId}");
        }
        await base.OnConnectedAsync();
    }

    /// <summary>
    /// Khi client disconnect → tự động remove khỏi group
    /// </summary>
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId != null)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"user_{userId}");
        }
        await base.OnDisconnectedAsync(exception);
    }
}
