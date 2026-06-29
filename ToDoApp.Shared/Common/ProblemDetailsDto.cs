namespace ToDoApp.Shared.Common;

public class ProblemDetailsDto
{
    public string? Title { get; set; }
    public string? Detail { get; set; }
    public int? Status { get; set; }
    public Dictionary<string, string[]>? Errors { get; set; } = [];
}