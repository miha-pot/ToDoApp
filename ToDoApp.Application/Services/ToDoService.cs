using Microsoft.EntityFrameworkCore;
using System.Net;
using ToDoApp.Application.Mappers;
using ToDoApp.Application.RepositoryContracts;
using ToDoApp.Application.ServiceContracts;
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
        bool result = await _repository.UpdateAsync(toDoItem, cancellationToken);

        return result ?
            ServiceResult<bool>.Success(toDoItem.IsCompleted) :
            ServiceResult<bool>.Failure("Error while updating ToDo item!",
                                        "Check application logs!",
                                        HttpStatusCode.BadRequest);
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

        bool result = await _repository.AddAsync(toDoItem, cancellationToken);

        return result ?
            ServiceResult<ToDoResponse>.Success(toDoItem.ToResponse()) :
            ServiceResult<ToDoResponse>.Failure("Error while inserting to database!",
                                                "Check application logs!",
                                                HttpStatusCode.BadRequest);
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

        bool result = await _repository.DeleteAsync(itemId.Value, cancellationToken);

        return result ?
            ServiceResult<string>.Success("ToDo item with provided id was successfuly deleted") :
            ServiceResult<string>.Failure("Error while inserting to database!",
                                          "Check backend logs!",
                                          HttpStatusCode.BadRequest);
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

    public async Task<PagedResult<ToDoResponse>> GetItemsWithQuery(QueryRequest queryRequest, CancellationToken cancellationToken)
    {
        IQueryable<TodoItem> query = _queryRepository.BuildBaseQuery<TodoItem>(queryRequest);

        query = query.Include(t => t.TodoItemTags)
                     .ThenInclude(tt => tt.Tag);

        return await _queryRepository.ExecuteQueryAsync<TodoItem, ToDoResponse>(query, queryRequest, todo => new ToDoResponse()
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

        bool result = await _repository.UpdateAsync(matchedToDoItem, cancellationToken);

        return result ?
            ServiceResult<ToDoResponse>.Success(matchedToDoItem.ToResponse()) :
            ServiceResult<ToDoResponse>.Failure("Error while updating item!",
                                                "Check backend logs!",
                                                HttpStatusCode.BadRequest);
    }
}
