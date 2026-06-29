using ToDoApp.Domain.EntityContract;

namespace ToDoApp.Domain.Entities;

public class TodoItem : BaseEntity, IUserOwnedEntity
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public bool IsCompleted { get; set; }
    public DateTime? DueDate { get; set; }
    public int Level { get; set; } = 1;
    public Guid UserId { get; set; }
    public Guid? ParentTodoId { get; set; }
    public TodoItem? ParentTodo { get; set; }
    public ICollection<TodoItem> SubTasks { get; set; } = [];

    // Relationships
    public ICollection<TodoItemTag> TodoItemTags { get; set; } = [];
}
