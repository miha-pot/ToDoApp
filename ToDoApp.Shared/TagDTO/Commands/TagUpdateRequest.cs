namespace ToDoApp.Shared.TagDTO.Commands;

public class TagUpdateRequest
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public string? ColorHex { get; set; }
    public string? BgColorHex { get; set; }
    public bool IsActive { get; set; }
}
