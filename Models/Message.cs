namespace TaskManagement.Models;

public class Message : BaseEntity
{
    public string Content { get; set; } = string.Empty;
    
    public Guid SenderId { get; set; }
    public User Sender { get; set; } = null!;
    
    public Guid ReceiverId { get; set; }
    public User Receiver { get; set; } = null!;
    
    public bool IsRead { get; set; } = false;
}
