using Microsoft.EntityFrameworkCore;
using TaskManagement.Models;

namespace TaskManagement.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<TaskItem> Tasks => Set<TaskItem>();

    protected override void OnModelCreating(ModelBuilder mb)
    {
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
    }
}