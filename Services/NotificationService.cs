using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using TaskManagement.Data;
using TaskManagement.DTOs;
using TaskManagement.Hubs;
using TaskManagement.Models;

namespace TaskManagement.Services;

public interface INotificationService
{
    /// <summary>
    /// Tạo notification, lưu DB, và push real-time qua SignalR
    /// </summary>
    Task SendAsync(Guid recipientUserId, string message, NotificationType type, Guid? taskItemId = null);

    /// <summary>
    /// Lấy danh sách notification của user (mới nhất trước)
    /// </summary>
    Task<IEnumerable<NotificationDto>> GetByUserAsync(Guid userId);

    /// <summary>
    /// Đếm số notification chưa đọc
    /// </summary>
    Task<int> GetUnreadCountAsync(Guid userId);

    /// <summary>
    /// Đánh dấu 1 notification đã đọc
    /// </summary>
    Task<bool> MarkAsReadAsync(Guid notificationId, Guid userId);

    /// <summary>
    /// Đánh dấu tất cả notification của user đã đọc
    /// </summary>
    Task MarkAllAsReadAsync(Guid userId);
}

public class NotificationService : INotificationService
{
    private readonly AppDbContext _context;
    private readonly IHubContext<NotificationHub> _hubContext;

    public NotificationService(AppDbContext context, IHubContext<NotificationHub> hubContext)
    {
        _context = context;
        _hubContext = hubContext;
    }

    public async Task SendAsync(Guid recipientUserId, string message, NotificationType type, Guid? taskItemId = null)
    {
        // 1. Lưu notification vào DB (persist)
        var notification = new Notification
        {
            Message = message,
            Type = type,
            UserId = recipientUserId,
            TaskItemId = taskItemId
        };

        _context.Notifications.Add(notification);
        await _context.SaveChangesAsync();

        // 2. Push real-time qua SignalR → gửi đến group của user
        var dto = ToDto(notification);
        await _hubContext.Clients
            .Group($"user_{recipientUserId}")
            .SendAsync("ReceiveNotification", dto);
    }

    public async Task<IEnumerable<NotificationDto>> GetByUserAsync(Guid userId)
    {
        return await _context.Notifications
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAt)
            .Take(50)  // Giới hạn 50 notification gần nhất
            .Select(n => ToDto(n))
            .ToListAsync();
    }

    public async Task<int> GetUnreadCountAsync(Guid userId)
    {
        return await _context.Notifications
            .Where(n => n.UserId == userId && !n.IsRead)
            .CountAsync();
    }

    public async Task<bool> MarkAsReadAsync(Guid notificationId, Guid userId)
    {
        var notification = await _context.Notifications
            .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId);

        if (notification == null) return false;

        notification.IsRead = true;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task MarkAllAsReadAsync(Guid userId)
    {
        await _context.Notifications
            .Where(n => n.UserId == userId && !n.IsRead)
            .ExecuteUpdateAsync(n => n.SetProperty(x => x.IsRead, true));
    }

    private static NotificationDto ToDto(Notification n) => new()
    {
        Id = n.Id,
        Message = n.Message,
        Type = n.Type.ToString(),
        IsRead = n.IsRead,
        TaskItemId = n.TaskItemId,
        CreatedAt = n.CreatedAt
    };
}
