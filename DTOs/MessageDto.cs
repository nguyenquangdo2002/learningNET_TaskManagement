namespace TaskManagement.DTOs;

public class MessageDto
{
    public Guid Id { get; set; }
    public string Content { get; set; } = string.Empty;
    public Guid SenderId { get; set; }
    public string SenderUsername { get; set; } = string.Empty;
    public Guid ReceiverId { get; set; }
    public string ReceiverUsername { get; set; } = string.Empty;
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class SendMessageDto
{
    public Guid ReceiverId { get; set; }
    public string Content { get; set; } = string.Empty;
}
