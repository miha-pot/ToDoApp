using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using ToDoApp.Domain.Entities;
using ToDoApp.Domain.EntityContract;
using ToDoApp.Domain.Identity;
using ToDoApp.Infrastructure.FluentConfigs;

namespace ToDoApp.Infrastructure.DatabaseContext;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public virtual DbSet<TodoItem> TodoItems { get; set; }
    public virtual DbSet<Tag> Tags { get; set; }
    public virtual DbSet<TodoItemTag> TodoItemTags { get; set; }

    public ApplicationDbContext(DbContextOptions options,
                                IHttpContextAccessor httpContextAccessor) : base(options)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfiguration(new FluentTagConfig());
        modelBuilder.ApplyConfiguration(new FluentToDoItemConfig());

        modelBuilder.Entity<TodoItemTag>(x =>
        {
            x.HasKey(tt => new { tt.TodoItemId, tt.TagId });

            x.HasOne(tt => tt.TodoItem)
              .WithMany(todo => todo.TodoItemTags)
              .HasForeignKey(tt => tt.TodoItemId)
              .OnDelete(DeleteBehavior.Cascade);

            x.HasOne(tt => tt.Tag)
                  .WithMany(tag => tag.TodoItemTags)
                  .HasForeignKey(tt => tt.TagId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Pridobimo trenutno prijavljenega uporabnika iz HTTP konteksta (iz JWT žetona)
        var currentUserId = _httpContextAccessor.HttpContext?.User?
            .FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "System";

        var currentTime = DateTime.UtcNow;

        // Poiščemo vse entitete, ki implementirajo IAuditableEntity in so v stanju Added ali Modified
        var entries = ChangeTracker.Entries<IAuditableEntity>();

        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAtUtc = currentTime;
                entry.Entity.CreatedBy = currentUserId;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.LastModifiedAtUtc = currentTime;
                entry.Entity.LastModifiedBy = currentUserId;
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}
