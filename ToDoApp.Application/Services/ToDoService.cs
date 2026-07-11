using Microsoft.EntityFrameworkCore;
using System.Net;
using ToDoApp.Application.Mappers;
using ToDoApp.Application.RepositoryContracts;
using ToDoApp.Application.ServiceContracts;
using ToDoApp.Domain.Common;
using ToDoApp.Domain.Entities;
using ToDoApp.Domain.RepositoryContracts;
using ToDoApp.Shared.Common;
using ToDoApp.Shared.Common.Filtering;
using ToDoApp.Shared.Enums;
using ToDoApp.Shared.TagDTO.Queries;
using ToDoApp.Shared.ToDoDTO.Commands;
using ToDoApp.Shared.ToDoDTO.Queries;

namespace ToDoApp.Application.Services;

public class ToDoService : IToDoService
{
    private readonly ITodoItemRepository _repository;
    private readonly IQueryRepository _queryRepository;
    private readonly ICurrentUserRepository _currentUserRepository;

    public ToDoService(ITodoItemRepository toDoRepository,
                       IQueryRepository queryRepository,
                       ICurrentUserRepository currentUserRepository)
    {
        _repository = toDoRepository;
        _queryRepository = queryRepository;
        _currentUserRepository = currentUserRepository;
    }

    public async Task<ServiceResult<bool>> ChangeCompletionStatus(Guid? id, CancellationToken cancellationToken)
    {
        if (!id.HasValue)
        {
            return ServiceResult<bool>.Failure("Id is null!",
                                                 "Provided data is null!",
                                                 HttpStatusCode.BadRequest);
        }

        TodoItem? toDoItem = await _repository.GetByIdAsync(id.Value, cancellationToken);

        if (toDoItem is null)
        {
            return ServiceResult<bool>.Failure("ToDo item not found!",
                                               "ToDo item with provided id was not found!",
                                               HttpStatusCode.NotFound);
        }

        toDoItem.IsCompleted = !toDoItem.IsCompleted;
        DatabaseResult result = await _repository.UpdateAsync(toDoItem, cancellationToken);

        return result switch
        {
            DatabaseResult.Success => ServiceResult<bool>.Success(true),
            DatabaseResult.NoChanges => ServiceResult<bool>.Warning(false, "No changes were made!", "No changes were made to entity!"),
            DatabaseResult.Failed => ServiceResult<bool>.Failure("Academic year was not updated!", "Failure while updating academic year!"),
            _ => ServiceResult<bool>.Failure("Unexpected error!", "An unexpected database state occurred. Please contact support.")
        };
    }

    public async Task<ServiceResult<ToDoResponse>> CreateItem(ToDoAddRequest? addRequest, CancellationToken cancellationToken)
    {
        Guid toDoId = Guid.NewGuid();

        TodoItem toDoItem = addRequest!.ToEntity();
        toDoItem.Id = toDoId;
        toDoItem.UserId = _currentUserRepository.GetUserId();

        if (addRequest!.TagIds is not null && addRequest.TagIds.Any())
        {
            toDoItem.TodoItemTags = addRequest.TagIds.Select(x => new TodoItemTag()
            {
                TagId = x,
                TodoItemId = toDoId

            }).ToList();
        }

        DatabaseResult result = await _repository.AddAsync(toDoItem, cancellationToken);

        return result switch
        {
            DatabaseResult.Success => ServiceResult<ToDoResponse>.Success(toDoItem.ToResponse()),
            DatabaseResult.Failed => ServiceResult<ToDoResponse>.Failure("Item was not created!", "Failure while creating new item!"),
            _ => ServiceResult<ToDoResponse>.Failure("Unexpected error!", "An unexpected database state occurred. Please contact support.")
        };
    }

    public async Task<ServiceResult<string>> DeleteItem(Guid? itemId, CancellationToken cancellationToken)
    {
        if (itemId is null)
        {
            return ServiceResult<string>.Failure("Id is null!",
                                                 "Provided data is null!",
                                                 HttpStatusCode.BadRequest);
        }

        TodoItem? toDoItem = await _repository.GetByIdAsync(itemId.Value, cancellationToken);

        if (toDoItem is null)
        {
            return ServiceResult<string>.Failure("ToDo item not found!",
                                                 "ToDo item with provided id was not found!",
                                                 HttpStatusCode.NotFound);
        }

        DatabaseResult result = await _repository.DeleteAsync(itemId.Value, cancellationToken);

        return result switch
        {
            DatabaseResult.Success => ServiceResult<string>.Success("Item was deleted!"),
            DatabaseResult.Failed => ServiceResult<string>.Failure("Item was not deleted!", "Failure while deleting item!"),
            _ => ServiceResult<string>.Failure("Unexpected error!", "An unexpected database state occurred. Please contact support.")
        };
    }

    public async Task<ServiceResult<ToDoResponse?>> GetItemById(Guid? itemId, CancellationToken cancellationToken)
    {
        if (itemId is null)
        {
            return ServiceResult<ToDoResponse?>.Failure("Id is null!",
                                                        "Provided id is null!",
                                                        HttpStatusCode.BadRequest);
        }

        TodoItem? toDoItem = await _repository.GetByIdAsync(itemId.Value, cancellationToken);

        if (toDoItem is null)
        {
            return ServiceResult<ToDoResponse?>.Failure("ToDo item not found!",
                                                        "ToDo item with provided id was not found!",
                                                        HttpStatusCode.BadRequest);
        }

        return ServiceResult<ToDoResponse?>.Success(toDoItem.ToResponseWithTags());
    }

    public async Task<ServiceResult<PagedResult<ToDoResponse>>> GetItemsWithQuery(QueryRequest queryRequest, CancellationToken cancellationToken)
    {
        IQueryable<TodoItem> query = _queryRepository.BuildBaseQuery<TodoItem>(queryRequest);

        query = query.Include(t => t.TodoItemTags)
                     .ThenInclude(tt => tt.Tag);

        var result = await _queryRepository.ExecuteQueryAsync(query, queryRequest, todo => new ToDoResponse()
        {
            Id = todo.Id,
            Title = todo.Title,
            IsCompleted = todo.IsCompleted,
            DueDate = todo.DueDate,
            Level = (Priority)todo.Level,
            ParentToDoId = todo.ParentTodoId,
            UserId = todo.UserId,
            Description = todo.Description,

            Tags = todo.TodoItemTags.Select(tt => new TagResponse
            {
                Id = tt.TagId,
                Name = tt.Tag.Name,
                ColorHex = tt.Tag.ColorHex,
                BgColorHex = tt.Tag.BgColorHex
            }).ToList()
        }, cancellationToken);

        return result is not null ?
            ServiceResult<PagedResult<ToDoResponse>>.Success(result) :
            ServiceResult<PagedResult<ToDoResponse>>.Failure("Query result is null!", "Error while receiving paged response from server!");
    }

    public async Task<ServiceResult<List<ToDoResponse>>> GetSubTasks(Guid parentId, CancellationToken cancellationToken)
    {
        List<TodoItem> subTasks = await _repository.GetSubTasks(parentId, cancellationToken);

        List<ToDoResponse> response = subTasks.Select(x => x.ToResponseWithTags()).ToList();

        return ServiceResult<List<ToDoResponse>>.Success(response);
    }

    public async Task<ServiceResult<ToDoResponse>> UpdateItem(ToDoUpdateRequest? updateRequest, CancellationToken cancellationToken)
    {
        TodoItem? matchedToDoItem = await _repository.GetByIdAsync(updateRequest!.Id, cancellationToken);

        if (matchedToDoItem is null)
        {
            return ServiceResult<ToDoResponse>.Failure("ToDo item not found!",
                                                       "ToDo item with provided id was not found!",
                                                       HttpStatusCode.BadRequest);
        }

        updateRequest.UpdateEntity(matchedToDoItem);

        DatabaseResult result = await _repository.UpdateAsync(matchedToDoItem, cancellationToken);

        return result switch
        {
            DatabaseResult.Success => ServiceResult<ToDoResponse>.Success(matchedToDoItem.ToResponse()),
            DatabaseResult.NoChanges => ServiceResult<ToDoResponse>.Warning(matchedToDoItem.ToResponse(), "No changes were made!", "No changes were made to entity!"),
            DatabaseResult.Failed => ServiceResult<ToDoResponse>.Failure("Academic year was not updated!", "Failure while updating academic year!"),
            _ => ServiceResult<ToDoResponse>.Failure("Unexpected error!", "An unexpected database state occurred. Please contact support.")
        };
    }
}
