using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TaskManagement.Data;
using TaskManagement.DTOs;
using TaskManagement.Models;

namespace TaskManagement.Services;

public interface ITaskService
{
    Task<IEnumerable<TaskDto>> GetAllAsync(TaskFilter filter);
    Task<TaskDto?> GetByIdAsync(Guid id);
    Task<TaskDto> CreateAsync(CreateTaskDto dto);
    Task<TaskDto?> UpdateAsync(Guid id, UpdateTaskDto dto);
    Task<bool> DeleteAsync(Guid id);
}

public class TaskService : ITaskService
{
    private readonly AppDbContext _context;
    private readonly IHttpContextAccessor _httpContext;
    private readonly INotificationService _notificationService;

    public TaskService(AppDbContext context, IHttpContextAccessor httpContext, INotificationService notificationService)
    {
        _context = context;
        _httpContext = httpContext;
        _notificationService = notificationService;
    }

    public async Task<IEnumerable<TaskDto>> GetAllAsync(TaskFilter filter)
    {
        var query = _context.Tasks
            .Include(t => t.AssignedTo)
            .Include(t => t.CreatedBy)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.Status))
            query = query.Where(t => t.Status.ToString() == filter.Status);

        if (filter.AssignedToId.HasValue)
            query = query.Where(t => t.AssignedToId == filter.AssignedToId);

        if (!string.IsNullOrWhiteSpace(filter.Search))
            query = query.Where(t => t.Title.Contains(filter.Search));

        return await query.Select(t => ToDto(t)).ToListAsync();
    }

    public async Task<TaskDto?> GetByIdAsync(Guid id)
    {
        var task = await _context.Tasks
            .Include(t => t.AssignedTo)
            .Include(t => t.CreatedBy)
            .FirstOrDefaultAsync(t => t.Id == id);

        return task == null ? null : ToDto(task);
    }

    public async Task<TaskDto> CreateAsync(CreateTaskDto dto)
    {
        var currentUserId = GetCurrentUserId();

        var task = new TaskItem
        {
            Title = dto.Title,
            Description = dto.Description,
            Priority = Enum.Parse<Priority>(dto.Priority),
            AssignedToId = dto.AssignedToId,
            CreatedById = currentUserId // Gán ownership cho người tạo
        };

        _context.Tasks.Add(task);
        await _context.SaveChangesAsync();

        if (task.AssignedToId.HasValue && task.AssignedToId != currentUserId)
        {
            var currentUser = await _context.Users.FindAsync(currentUserId);
            await _notificationService.SendAsync(
                task.AssignedToId.Value, 
                $"{currentUser?.Username} đã giao cho bạn một task mới: {task.Title}", 
                NotificationType.TaskAssigned, 
                task.Id);
        }

        return await GetByIdAsync(task.Id) ?? ToDto(task);
    }

    public async Task<TaskDto?> UpdateAsync(Guid id, UpdateTaskDto dto)
    {
        var task = await _context.Tasks.FindAsync(id);
        if (task == null) return null;

        // Ownership check: chỉ người tạo hoặc Admin mới được sửa
        EnsureOwnershipOrAdmin(task.CreatedById);

        var originalAssignedToId = task.AssignedToId;
        var originalStatus = task.Status.ToString();

        if (dto.Title != null) task.Title = dto.Title;
        if (dto.Description != null) task.Description = dto.Description;
        if (dto.Status != null) task.Status = Enum.Parse<Models.TaskStatus>(dto.Status);
        if (dto.Priority != null) task.Priority = Enum.Parse<Priority>(dto.Priority);
        if (dto.AssignedToId.HasValue) task.AssignedToId = dto.AssignedToId;

        await _context.SaveChangesAsync();
        
        var currentUserId = GetCurrentUserId();
        var currentUser = await _context.Users.FindAsync(currentUserId);

        if (task.AssignedToId.HasValue && task.AssignedToId != originalAssignedToId && task.AssignedToId != currentUserId)
        {
             await _notificationService.SendAsync(
                task.AssignedToId.Value, 
                $"{currentUser?.Username} đã giao cho bạn một task: {task.Title}", 
                NotificationType.TaskAssigned, 
                task.Id);
        }
        else if (task.AssignedToId.HasValue && task.AssignedToId != currentUserId && dto.Status != null && originalStatus != dto.Status)
        {
            await _notificationService.SendAsync(
                task.AssignedToId.Value, 
                $"{currentUser?.Username} đã cập nhật trạng thái task {task.Title} thành {dto.Status}", 
                NotificationType.TaskStatusChanged, 
                task.Id);
        }

        return await GetByIdAsync(id);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var task = await _context.Tasks.FindAsync(id);
        if (task == null) return false;

        // Ownership check: chỉ người tạo hoặc Admin mới được xóa
        EnsureOwnershipOrAdmin(task.CreatedById);

        _context.Tasks.Remove(task);
        await _context.SaveChangesAsync();
        return true;
    }

    // ============ HELPER METHODS ============

    /// <summary>
    /// Lấy UserId từ JWT Claims trong HttpContext
    /// </summary>
    private Guid GetCurrentUserId()
    {
        var claim = _httpContext.HttpContext?.User
            .FindFirst(ClaimTypes.NameIdentifier);

        if (claim == null)
            throw new UnauthorizedAccessException("Không tìm thấy thông tin user");

        return Guid.Parse(claim.Value);
    }

    /// <summary>
    /// Kiểm tra user hiện tại có phải Admin không
    /// </summary>
    private bool IsAdmin()
    {
        return _httpContext.HttpContext?.User.IsInRole("Admin") ?? false;
    }

    /// <summary>
    /// Kiểm tra ownership: chỉ người tạo hoặc Admin mới được thao tác.
    /// Task cũ không có CreatedById → chỉ Admin mới được sửa.
    /// </summary>
    private void EnsureOwnershipOrAdmin(Guid? createdById)
    {
        if (IsAdmin()) return;

        var currentUserId = GetCurrentUserId();

        if (createdById == null || createdById != currentUserId)
            throw new UnauthorizedAccessException("Bạn không có quyền thao tác task này");
    }

    private static TaskDto ToDto(TaskItem t) => new()
    {
        Id = t.Id,
        Title = t.Title,
        Description = t.Description,
        Status = t.Status.ToString(),
        Priority = t.Priority.ToString(),
        AssignedToUsername = t.AssignedTo?.Username,
        CreatedByUsername = t.CreatedBy?.Username,
        CreatedAt = t.CreatedAt
    };
}