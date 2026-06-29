using ToDoApp.Domain.Entities;
using ToDoApp.Shared.Enums;
using ToDoApp.Shared.ToDoDTO.Commands;
using ToDoApp.Shared.ToDoDTO.Queries;

namespace ToDoApp.Application.Mappers;

public static class ToDoMapper
{
    public static TodoItem ToEntity(this ToDoAddRequest request)
    {
        return new TodoItem
        {
            Title = request.Title,
            Description = request.Description,
            UserId = request.UserId,
            DueDate = request.DueDate,
            IsCompleted = request.IsCompleted,
            Level = (int)request.Level,
            ParentTodoId = request.ParentToDoId,
        };
    }

    public static void UpdateEntity(this ToDoUpdateRequest request, TodoItem existingToDo)
    {
        existingToDo.Title = request.Title;
        existingToDo.Description = request.Description;
        existingToDo.DueDate = request.DueDate;
        existingToDo.IsCompleted = request.IsCompleted;
        existingToDo.Level = (int)request.Level;

        existingToDo.TodoItemTags.Clear();

        foreach (Guid tagId in request.TagIds)
        {
            existingToDo.TodoItemTags.Add(new()
            {
                TagId = tagId,
                TodoItemId = existingToDo.Id
            });
        }
    }

    public static ToDoResponse ToResponse(this TodoItem toDoItem)
    {
        return new ToDoResponse()
        {
            Id = toDoItem.Id,
            Title = toDoItem.Title!,
            Description = toDoItem.Description!,
            IsCompleted = toDoItem.IsCompleted,
            DueDate = toDoItem.DueDate,
            Level = (Priority)toDoItem.Level,
            UserId = toDoItem.UserId,
            ParentToDoId = toDoItem.ParentTodoId
        };
    }

    public static ToDoResponse ToResponseWithTags(this TodoItem toDoItem)
    {
        return new ToDoResponse()
        {
            Id = toDoItem.Id,
            Title = toDoItem.Title!,
            Description = toDoItem.Description!,
            IsCompleted = toDoItem.IsCompleted,
            DueDate = toDoItem.DueDate,
            Level = (Priority)toDoItem.Level,
            UserId = toDoItem.UserId,
            ParentToDoId = toDoItem.ParentTodoId,
            Tags = toDoItem.TodoItemTags.Select(x => x.Tag.ToResponse()).ToList()
        };
    }
}
