namespace TaskManagement.Models;

public enum NotificationType
{
    TaskAssigned,
    TaskUpdated,
    TaskDeleted,
    TaskStatusChanged
}

public class Notification : BaseEntity
{
    public string Message { get; set; } = string.Empty;
    public NotificationType Type { get; set; }
    public bool IsRead { get; set; } = false;

    /// <summary>
    /// User nhận notification
    /// </summary>
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    /// <summary>
    /// Task liên quan (nullable vì task có thể bị xóa)
    /// </summary>
    public Guid? TaskItemId { get; set; }
    public TaskItem? TaskItem { get; set; }
}
