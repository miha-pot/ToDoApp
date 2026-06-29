using System.Net;
using ToDoApp.Application.Mappers;
using ToDoApp.Application.RepositoryContracts;
using ToDoApp.Application.ServiceContracts;
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

        bool result = await _repository.AddAsync(tag, cancellationToken);

        return result ?
            ServiceResult<TagResponse>.Success(tag.ToResponse()) :
            ServiceResult<TagResponse>.Failure("Error while inserting to database!",
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

        Tag? tag = await _repository.GetByIdAsync(itemId.Value, cancellationToken);

        if (tag is null)
        {
            return ServiceResult<string>.Failure("Tag not found!",
                                                 "Tag with provided id was not found!",
                                                 HttpStatusCode.BadRequest);
        }

        bool result = await _repository.DeleteAsync(itemId.Value, cancellationToken);

        return result ?
            ServiceResult<string>.Success("Tag with provided id was successfuly deleted") :
            ServiceResult<string>.Failure("Error while inserting to database!",
                                          "Check backend logs!",
                                          HttpStatusCode.BadRequest);
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

        bool result = await _repository.UpdateAsync(matchedTag, cancellationToken);

        return result ?
            ServiceResult<TagResponse>.Success(matchedTag.ToResponse()) :
            ServiceResult<TagResponse>.Failure("Error while updating item!",
                                               "Check backend logs!",
                                               HttpStatusCode.BadRequest);
    }

    public async Task<PagedResult<TagResponse>> GetItemsWithQuery(QueryRequest queryRequest, CancellationToken cancellationToken)
    {
        return await _queryRepository.GetListByQueryAsync<Tag, TagResponse>(queryRequest, x => x.ToResponse(), cancellationToken);
    }

    public async Task<List<TagResponse>> GetActiveTags(CancellationToken cancellationToken)
    {
        var tags = await _repository.GetActiveTags(cancellationToken);

        return tags.Select(x => x.ToResponse()).ToList();
    }
}
