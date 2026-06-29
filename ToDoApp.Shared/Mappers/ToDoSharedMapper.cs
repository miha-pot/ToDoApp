using ToDoApp.Shared.ToDoDTO.Commands;
using ToDoApp.Shared.ToDoDTO.Queries;

namespace ToDoApp.Shared.Mappers;

public static class ToDoSharedMapper
{
    public static ToDoUpdateRequest ToUpdateRequest(this ToDoResponse response)
    {
        return new ToDoUpdateRequest
        {
            Id = response.Id,
            Title = response.Title,
            Description = response.Description,
            IsCompleted = response.IsCompleted,
            DueDate = response.DueDate,
            Level = response.Level,
            ParentToDoId = response.ParentToDoId,
            UserId = response.UserId,
            Tags = response.Tags
        };
    }
}
