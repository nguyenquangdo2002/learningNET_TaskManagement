namespace TaskManagement.Models;

public enum TaskStatus { Todo, InProgress, Done }
public enum Priority { Low, Medium, High }

public class TaskItem : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public TaskStatus Status { get; set; } = TaskStatus.Todo;
    public Priority Priority { get; set; } = Priority.Medium;
    public Guid? AssignedToId { get; set; }
    public User? AssignedTo { get; set; }

    public Guid? CreatedById { get; set; }
    public User? CreatedBy { get; set; }
}
