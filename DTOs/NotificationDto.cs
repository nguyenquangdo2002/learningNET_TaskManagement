namespace TaskManagement.DTOs;

public class NotificationDto
{
    public Guid Id { get; set; }
    public string Message { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public bool IsRead { get; set; }
    public Guid? TaskItemId { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class UnreadCountDto
{
    public int Count { get; set; }
}
