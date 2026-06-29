namespace ToDoApp.Shared.TagDTO.Queries;

public class TagResponse
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public string? ColorHex { get; set; }
    public string? BgColorHex { get; set; }
    public Guid UserId { get; set; }
    public bool IsActive { get; set; }

    public override bool Equals(object? obj)
    {
        if (obj is TagResponse other)
        {
            return Id == other.Id;
        }
        return false;
    }

    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }
}