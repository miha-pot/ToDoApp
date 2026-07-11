using System.Net;
using ToDoApp.Application.Mappers;
using ToDoApp.Application.RepositoryContracts;
using ToDoApp.Application.ServiceContracts;
using ToDoApp.Domain.Common;
using ToDoApp.Domain.Entities;
using ToDoApp.Domain.RepositoryContracts;
using ToDoApp.Shared.Common;
using ToDoApp.Shared.Common.Filtering;
using ToDoApp.Shared.TagDTO.Commands;
using ToDoApp.Shared.TagDTO.Queries;

namespace ToDoApp.Application.Services;

public class TagService : ITagService
{
    private readonly ITagRepository _repository;
    private readonly ICurrentUserRepository _currentUserRepository;
    private readonly IQueryRepository _queryRepository;

    public TagService(ITagRepository tagRepository, ICurrentUserRepository currentUserRepository, IQueryRepository queryRepository)
    {
        _repository = tagRepository;
        _currentUserRepository = currentUserRepository;
        _queryRepository = queryRepository;
    }

    public async Task<ServiceResult<TagResponse>> CreateItem(TagAddRequest? addRequest, CancellationToken cancellationToken)
    {
        Tag tag = addRequest!.ToEntity();
        tag.UserId = _currentUserRepository.GetUserId();

        DatabaseResult result = await _repository.AddAsync(tag, cancellationToken);

        return result switch
        {
            DatabaseResult.Success => ServiceResult<TagResponse>.Success(tag.ToResponse()),
            DatabaseResult.Failed => ServiceResult<TagResponse>.Failure("Item was not created!", "Failure while creating new item!"),
            _ => ServiceResult<TagResponse>.Failure("Unexpected error!", "An unexpected database state occurred. Please contact support.")
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

        Tag? tag = await _repository.GetByIdAsync(itemId.Value, cancellationToken);

        if (tag is null)
        {
            return ServiceResult<string>.Failure("Tag not found!",
                                                 "Tag with provided id was not found!",
                                                 HttpStatusCode.BadRequest);
        }

        DatabaseResult result = await _repository.DeleteAsync(itemId.Value, cancellationToken);

        return result switch
        {
            DatabaseResult.Success => ServiceResult<string>.Success("Item was deleted!"),
            DatabaseResult.Failed => ServiceResult<string>.Failure("Item was not deleted!", "Failure while deleting item!"),
            _ => ServiceResult<string>.Failure("Unexpected error!", "An unexpected database state occurred. Please contact support.")
        };
    }

    public async Task<ServiceResult<TagResponse?>> GetItemById(Guid? itemId, CancellationToken cancellationToken)
    {
        if (itemId is null)
        {
            return ServiceResult<TagResponse?>.Failure("Id is null!",
                                                       "Provided id is null!",
                                                       HttpStatusCode.BadRequest);
        }

        Tag? tag = await _repository.GetByIdAsync(itemId.Value, cancellationToken);

        if (tag is null)
        {
            return ServiceResult<TagResponse?>.Failure("Tag not found!",
                                                       "Tag with provided id was not found!",
                                                       HttpStatusCode.BadRequest);
        }

        return ServiceResult<TagResponse?>.Success(tag.ToResponse());

    }

    public async Task<List<TagResponse>> GetItems(CancellationToken cancellationToken)
    {
        List<Tag> tags = await _repository.GetAllAsync(cancellationToken);

        return tags.Select(x => x.ToResponse()).ToList();
    }

    public async Task<ServiceResult<TagResponse>> UpdateItem(TagUpdateRequest? updateRequest, CancellationToken cancellationToken)
    {
        Tag? matchedTag = await _repository.GetByIdAsync(updateRequest!.Id, cancellationToken);

        if (matchedTag is null)
        {
            return ServiceResult<TagResponse>.Failure("Tag not found!",
                                                      "Tag with provided id was not found!",
                                                      HttpStatusCode.BadRequest);
        }

        updateRequest.UpdateEntity(matchedTag);

        DatabaseResult result = await _repository.UpdateAsync(matchedTag, cancellationToken);

        return result switch
        {
            DatabaseResult.Success => ServiceResult<TagResponse>.Success(matchedTag.ToResponse()),
            DatabaseResult.NoChanges => ServiceResult<TagResponse>.Warning(matchedTag.ToResponse(), "No changes were made!", "No changes were made to entity!"),
            DatabaseResult.Failed => ServiceResult<TagResponse>.Failure("To do item was not updated!", "Failure while updating academic year!"),
            _ => ServiceResult<TagResponse>.Failure("Unexpected error!", "An unexpected database state occurred. Please contact support.")
        };
    }

    public async Task<ServiceResult<PagedResult<TagResponse>>> GetItemsWithQuery(QueryRequest queryRequest, CancellationToken cancellationToken)
    {
        var result = await _queryRepository.GetListByQueryAsync<Tag, TagResponse>(queryRequest, x => x.ToResponse(), cancellationToken);

        return result is not null ?
            ServiceResult<PagedResult<TagResponse>>.Success(result) :
            ServiceResult<PagedResult<TagResponse>>.Failure("Query result is null!", "Error while receiving paged response from server!");
    }

    public async Task<ServiceResult<List<TagResponse>>> GetActiveTags(CancellationToken cancellationToken)
    {
        var result = await _repository.GetActiveTags(cancellationToken);

        return result is not null ?
            ServiceResult<List<TagResponse>>.Success(result.Select(x => x.ToResponse()).ToList()) :
            ServiceResult<List<TagResponse>>.Failure("List is null!", "Error while receiving list of items from server!");
    }
}
