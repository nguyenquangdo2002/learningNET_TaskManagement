using Microsoft.EntityFrameworkCore;
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

    public TaskService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<TaskDto>> GetAllAsync(TaskFilter filter)
    {
        var query = _context.Tasks
            .Include(t => t.AssignedTo)
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
            .FirstOrDefaultAsync(t => t.Id == id);

        return task == null ? null : ToDto(task);
    }

    public async Task<TaskDto> CreateAsync(CreateTaskDto dto)
    {
        var task = new TaskItem
        {
            Title = dto.Title,
            Description = dto.Description,
            Priority = Enum.Parse<Priority>(dto.Priority),
            AssignedToId = dto.AssignedToId
        };

        _context.Tasks.Add(task);
        await _context.SaveChangesAsync();

        return await GetByIdAsync(task.Id) ?? ToDto(task);
    }

    public async Task<TaskDto?> UpdateAsync(Guid id, UpdateTaskDto dto)
    {
        var task = await _context.Tasks.FindAsync(id);
        if (task == null) return null;

        if (dto.Title != null) task.Title = dto.Title;
        if (dto.Description != null) task.Description = dto.Description;
        if (dto.Status != null) task.Status = Enum.Parse<Models.TaskStatus>(dto.Status);
        if (dto.Priority != null) task.Priority = Enum.Parse<Priority>(dto.Priority);
        if (dto.AssignedToId.HasValue) task.AssignedToId = dto.AssignedToId;

        await _context.SaveChangesAsync();
        return await GetByIdAsync(id);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var task = await _context.Tasks.FindAsync(id);
        if (task == null) return false;

        _context.Tasks.Remove(task);
        await _context.SaveChangesAsync();
        return true;
    }

    private static TaskDto ToDto(TaskItem t) => new()
    {
        Id = t.Id,
        Title = t.Title,
        Description = t.Description,
        Status = t.Status.ToString(),
        Priority = t.Priority.ToString(),
        AssignedToUsername = t.AssignedTo?.Username,
        CreatedAt = t.CreatedAt
    };
}