using ToDoApp.Domain.EntityContract;

namespace ToDoApp.Domain.Entities;

public class Tag : BaseEntity, IUserOwnedEntity
{
    public string? Name { get; set; }
    public string? ColorHex { get; set; }
    public string? BgColorHex { get; set; }
    public Guid UserId { get; set; }
    public bool IsActive { get; set; }

    public ICollection<TodoItemTag> TodoItemTags { get; set; } = [];
}
