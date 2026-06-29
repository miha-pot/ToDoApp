namespace ToDoApp.Domain.Entities;

public class TodoItemTag
{
    public Guid TodoItemId { get; set; }
    public TodoItem TodoItem { get; set; } = null!;

    public Guid TagId { get; set; }
    public Tag Tag { get; set; } = null!;
}
