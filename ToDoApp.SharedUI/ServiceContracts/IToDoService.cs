using ToDoApp.Shared.Common;
using ToDoApp.Shared.Common.Filtering;
using ToDoApp.Shared.ToDoDTO.Commands;
using ToDoApp.Shared.ToDoDTO.Queries;

namespace ToDoApp.SharedUI.ServiceContracts;

public interface IToDoService
{
    Task<ApiResponse<ToDoResponse>> GetByIdAsync(Guid id, CancellationToken token);
    Task<ApiResponse<ToDoResponse>> CreateAsync(ToDoAddRequest addRequest, CancellationToken token);
    Task<ApiResponse<string>> DeleteAsync(Guid id, CancellationToken token);
    Task<ApiResponse<PagedResult<ToDoResponse>>> GetItemsWithQuery(QueryRequest queryRequest, CancellationToken token);
    Task<ApiResponse<ToDoResponse>> UpdateAsync(ToDoUpdateRequest updateRequest, CancellationToken token);
    Task<ApiResponse<bool>> ChangeCompletionStatus(Guid id, CancellationToken token);
    Task<ApiResponse<List<ToDoResponse>>> GetSubTasks(Guid parentId, CancellationToken token);

}