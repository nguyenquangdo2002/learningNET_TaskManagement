using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using TaskManagement.Data;
using TaskManagement.DTOs;
using TaskManagement.Hubs;
using TaskManagement.Models;

namespace TaskManagement.Services;

public interface IChatService
{
    Task<MessageDto> SendMessageAsync(Guid senderId, SendMessageDto dto);
    Task<IEnumerable<MessageDto>> GetChatHistoryAsync(Guid userId, Guid otherUserId);
    Task MarkAsReadAsync(Guid userId, Guid otherUserId);
    Task<IDictionary<Guid, int>> GetUnreadCountsAsync(Guid userId);
}

public class ChatService : IChatService
{
    private readonly AppDbContext _context;
    private readonly IHubContext<ChatHub> _hubContext;

    public ChatService(AppDbContext context, IHubContext<ChatHub> hubContext)
    {
        _context = context;
        _hubContext = hubContext;
    }

    public async Task<MessageDto> SendMessageAsync(Guid senderId, SendMessageDto dto)
    {
        var message = new Message
        {
            Content = dto.Content,
            SenderId = senderId,
            ReceiverId = dto.ReceiverId
        };

        _context.Messages.Add(message);
        await _context.SaveChangesAsync();

        var response = await _context.Messages
            .Include(m => m.Sender)
            .Include(m => m.Receiver)
            .Select(m => new MessageDto
            {
                Id = m.Id,
                Content = m.Content,
                SenderId = m.SenderId,
                SenderUsername = m.Sender.Username,
                ReceiverId = m.ReceiverId,
                ReceiverUsername = m.Receiver.Username,
                IsRead = m.IsRead,
                CreatedAt = m.CreatedAt
            })
            .FirstOrDefaultAsync(m => m.Id == message.Id);

        if (response != null)
        {
            await _hubContext.Clients.Group($"user_{dto.ReceiverId}")
                .SendAsync("ReceiveMessage", response);
        }

        return response!;
    }

    public async Task<IEnumerable<MessageDto>> GetChatHistoryAsync(Guid userId, Guid otherUserId)
    {
        return await _context.Messages
            .Include(m => m.Sender)
            .Include(m => m.Receiver)
            .Where(m => (m.SenderId == userId && m.ReceiverId == otherUserId) || 
                        (m.SenderId == otherUserId && m.ReceiverId == userId))
            .OrderBy(m => m.CreatedAt)
            .Select(m => new MessageDto
            {
                Id = m.Id,
                Content = m.Content,
                SenderId = m.SenderId,
                SenderUsername = m.Sender.Username,
                ReceiverId = m.ReceiverId,
                ReceiverUsername = m.Receiver.Username,
                IsRead = m.IsRead,
                CreatedAt = m.CreatedAt
            })
            .ToListAsync();
    }

    public async Task MarkAsReadAsync(Guid userId, Guid otherUserId)
    {
        await _context.Messages
            .Where(m => m.SenderId == otherUserId && m.ReceiverId == userId && !m.IsRead)
            .ExecuteUpdateAsync(s => s.SetProperty(m => m.IsRead, true));
    }
    public async Task<IDictionary<Guid, int>> GetUnreadCountsAsync(Guid userId)
    {
        return await _context.Messages
            .Where(m => m.ReceiverId == userId && !m.IsRead)
            .GroupBy(m => m.SenderId)
            .Select(g => new { SenderId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.SenderId, x => x.Count) as IDictionary<Guid, int> 
            ?? new Dictionary<Guid, int>();
    }
}
