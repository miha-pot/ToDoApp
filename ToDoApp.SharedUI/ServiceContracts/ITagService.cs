using ToDoApp.Shared.Common;
using ToDoApp.Shared.Common.Filtering;
using ToDoApp.Shared.TagDTO.Commands;
using ToDoApp.Shared.TagDTO.Queries;

namespace ToDoApp.SharedUI.ServiceContracts;

public interface ITagService
{
    Task<ApiResponse<TagResponse>> GetByIdAsync(Guid id, CancellationToken token);
    Task<ApiResponse<TagResponse>> CreateAsync(TagAddRequest addRequest, CancellationToken token);
    Task<ApiResponse<string>> DeleteAsync(Guid id, CancellationToken token);
    Task<ApiResponse<PagedResult<TagResponse>>> GetItemsWithQuery(QueryRequest queryRequest, CancellationToken token);
    Task<ApiResponse<TagResponse>> UpdateAsync(TagUpdateRequest updateRequest, CancellationToken token);
    Task<List<TagResponse>> GetActiveTags(CancellationToken token);
}
