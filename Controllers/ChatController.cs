using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TaskManagement.DTOs;
using TaskManagement.Services;

namespace TaskManagement.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ChatController : ControllerBase
{
    private readonly IChatService _chatService;

    public ChatController(IChatService chatService)
    {
        _chatService = chatService;
    }

    [HttpPost("send")]
    public async Task<IActionResult> SendMessage([FromBody] SendMessageDto dto)
    {
        var senderId = GetCurrentUserId();
        var result = await _chatService.SendMessageAsync(senderId, dto);
        return Ok(result);
    }

    [HttpGet("history/{otherUserId:guid}")]
    public async Task<IActionResult> GetHistory(Guid otherUserId)
    {
        var userId = GetCurrentUserId();
        var history = await _chatService.GetChatHistoryAsync(userId, otherUserId);
        return Ok(history);
    }

    [HttpPut("read/{otherUserId:guid}")]
    public async Task<IActionResult> MarkAsRead(Guid otherUserId)
    {
        var userId = GetCurrentUserId();
        await _chatService.MarkAsReadAsync(userId, otherUserId);
        return NoContent();
    }
    [HttpGet("unread-counts")]
    public async Task<IActionResult> GetUnreadCounts()
    {
        var userId = GetCurrentUserId();
        var counts = await _chatService.GetUnreadCountsAsync(userId);
        return Ok(counts);
    }

    private Guid GetCurrentUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (claim == null) throw new UnauthorizedAccessException("User not found");
        return Guid.Parse(claim.Value);
    }
}
