using ToDoApp.Domain.EntityContract;

namespace ToDoApp.Domain.Entities;

public abstract class BaseEntity : IAuditableEntity
{
    public Guid Id { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? LastModifiedAtUtc { get; set; }
    public string? LastModifiedBy { get; set; }
}
