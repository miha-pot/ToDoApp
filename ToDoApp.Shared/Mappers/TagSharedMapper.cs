using ToDoApp.Shared.TagDTO.Commands;
using ToDoApp.Shared.TagDTO.Queries;

namespace ToDoApp.Shared.Mappers;

public static class TagSharedMapper
{
    public static TagUpdateRequest ToUpdateRequest(this TagResponse response)
    {
        return new TagUpdateRequest
        {
            Id = response.Id,
            Name = response.Name,
            ColorHex = response.ColorHex,
            BgColorHex = response.BgColorHex,
            IsActive = response.IsActive
        };
    }
}
