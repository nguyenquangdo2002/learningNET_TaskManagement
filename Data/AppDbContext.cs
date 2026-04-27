using Microsoft.EntityFrameworkCore;
using TaskManagement.Models;

namespace TaskManagement.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<TaskItem> Tasks => Set<TaskItem>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<Message> Messages => Set<Message>();

    protected override void OnModelCreating(ModelBuilder mb)
    {
        // ... (existing configs) ...
        mb.Entity<User>()
            .HasIndex(u => u.Username).IsUnique();

        mb.Entity<TaskItem>()
            .HasIndex(t => t.Status);

        mb.Entity<TaskItem>()
            .HasIndex(t => t.AssignedToId);

        mb.Entity<TaskItem>()
            .HasOne(t => t.AssignedTo)
            .WithMany()
            .HasForeignKey(t => t.AssignedToId)
            .OnDelete(DeleteBehavior.SetNull);

        // Task ownership — ai tạo task
        mb.Entity<TaskItem>()
            .HasOne(t => t.CreatedBy)
            .WithMany()
            .HasForeignKey(t => t.CreatedById)
            .OnDelete(DeleteBehavior.Restrict);

        mb.Entity<TaskItem>()
            .HasIndex(t => t.CreatedById);

        // Refresh Token config
        mb.Entity<RefreshToken>()
            .HasOne(rt => rt.User)
            .WithMany()
            .HasForeignKey(rt => rt.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        mb.Entity<RefreshToken>()
            .HasIndex(rt => rt.Token).IsUnique();
            
        // Notification config
        mb.Entity<Notification>()
            .HasOne(n => n.User)
            .WithMany()
            .HasForeignKey(n => n.UserId)
            .OnDelete(DeleteBehavior.Cascade);
            
        mb.Entity<Notification>()
            .HasOne(n => n.TaskItem)
            .WithMany()
            .HasForeignKey(n => n.TaskItemId)
            .OnDelete(DeleteBehavior.SetNull);
            
        mb.Entity<Notification>()
            .HasIndex(n => n.UserId);
            
        mb.Entity<Notification>()
            .HasIndex(n => n.IsRead);

        // Message config
        mb.Entity<Message>()
            .HasOne(m => m.Sender)
            .WithMany()
            .HasForeignKey(m => m.SenderId)
            .OnDelete(DeleteBehavior.Restrict);

        mb.Entity<Message>()
            .HasOne(m => m.Receiver)
            .WithMany()
            .HasForeignKey(m => m.ReceiverId)
            .OnDelete(DeleteBehavior.Restrict);

        mb.Entity<Message>()
            .HasIndex(m => m.SenderId);

        mb.Entity<Message>()
            .HasIndex(m => m.ReceiverId);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = DateTime.UtcNow;
            }
        }

        return await base.SaveChangesAsync(cancellationToken);
    }
}