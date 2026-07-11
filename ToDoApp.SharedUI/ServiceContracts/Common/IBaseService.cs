using ToDoApp.Shared.Common;
using ToDoApp.Shared.Common.Filtering;

namespace ToDoApp.SharedUI.ServiceContracts.Common;

public interface IBaseService<TAddRequest, TResponse, TUpdateRequest>
{
    Task<ApiResponse<TResponse>> CreateAsync(TAddRequest addRequest, CancellationToken token);
    Task<ApiResponse<TResponse>> GetByIdAsync(Guid id, CancellationToken token);
    Task<ApiResponse<PagedResult<TResponse>>> GetItemsWithQuery(QueryRequest queryRequest, CancellationToken token);
    Task<ApiResponse<TResponse>> UpdateAsync(TUpdateRequest updateRequest, CancellationToken token);
    Task<ApiResponse<string>> DeleteAsync(Guid id, CancellationToken token);
}
