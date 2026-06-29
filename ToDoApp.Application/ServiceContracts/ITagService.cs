using ToDoApp.Shared.Common.Filtering;
using ToDoApp.Shared.TagDTO.Commands;
using ToDoApp.Shared.TagDTO.Queries;

namespace ToDoApp.Application.ServiceContracts;

public interface ITagService : ICommonService<TagAddRequest, TagResponse, TagUpdateRequest>
{
    Task<List<TagResponse>> GetItems(CancellationToken cancellationToken);
    Task<PagedResult<TagResponse>> GetItemsWithQuery(QueryRequest queryRequest, CancellationToken cancellationToken);
    Task<List<TagResponse>> GetActiveTags(CancellationToken cancellationToken);
}
