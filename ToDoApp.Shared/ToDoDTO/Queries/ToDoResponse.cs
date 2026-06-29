using ToDoApp.Shared.Enums;
using ToDoApp.Shared.TagDTO.Queries;

namespace ToDoApp.Shared.ToDoDTO.Queries;

public class ToDoResponse
{
    public Guid Id { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
    public bool IsCompleted { get; set; }
    public DateTime? DueDate { get; set; }
    public Priority Level { get; set; }
    public Guid UserId { get; set; }
    public Guid? ParentToDoId { get; set; }
    public List<TagResponse> Tags { get; set; } = [];
}
