using ToDoApp.Domain.Entities;
using ToDoApp.Shared.TagDTO.Commands;
using ToDoApp.Shared.TagDTO.Queries;

namespace ToDoApp.Application.Mappers;

public static class TagMapper
{
    public static Tag ToEntity(this TagAddRequest request)
    {
        return new Tag
        {
            Name = request.Name,
            ColorHex = request.ColorHex,
            BgColorHex = request.BgColorHex,
            IsActive = request.IsActive
        };
    }

    public static void UpdateEntity(this TagUpdateRequest request, Tag existingTag)
    {
        existingTag.Name = request.Name;
        existingTag.ColorHex = request.ColorHex;
        existingTag.BgColorHex = request.BgColorHex;
        existingTag.IsActive = request.IsActive;
    }

    public static TagResponse ToResponse(this Tag tag)
    {
        return new TagResponse
        {
            Id = tag.Id,
            Name = tag.Name,
            ColorHex = tag.ColorHex,
            BgColorHex = tag.BgColorHex,
            UserId = tag.UserId,
            IsActive = tag.IsActive
        };
    }
}
