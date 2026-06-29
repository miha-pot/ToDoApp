namespace ToDoApp.Shared.TagDTO.Commands;

public class TagAddRequest
{
    public string Name { get; set; } = string.Empty;
    public string ColorHex { get; set; } = "#000000";
    public string BgColorHex { get; set; } = "#FFFFFF";
    public bool IsActive { get; set; }
}