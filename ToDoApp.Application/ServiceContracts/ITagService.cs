using ToDoApp.Shared.Common;
using ToDoApp.Shared.Common.Filtering;
using ToDoApp.Shared.TagDTO.Commands;
using ToDoApp.Shared.TagDTO.Queries;

namespace ToDoApp.Application.ServiceContracts;

public interface ITagService : ICommonService<TagAddRequest, TagResponse, TagUpdateRequest>
{
    Task<ServiceResult<PagedResult<TagResponse>>> GetItemsWithQuery(QueryRequest queryRequest, CancellationToken cancellationToken);
    Task<ServiceResult<List<TagResponse>>> GetActiveTags(CancellationToken cancellationToken);
}
