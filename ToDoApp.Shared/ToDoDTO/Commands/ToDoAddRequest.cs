using ToDoApp.Shared.Enums;

namespace ToDoApp.Shared.ToDoDTO.Commands;

public class ToDoAddRequest
{
    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public bool IsCompleted { get; set; } = false;

    public DateTime? DueDate { get; set; } = DateTime.Today.AddDays(7);

    public Priority Level { get; set; } = Priority.VeryLow;

    public Guid UserId { get; set; } = Guid.Empty;

    public Guid? ParentToDoId { get; set; }
    public List<Guid> TagIds { get; set; } = [];
}