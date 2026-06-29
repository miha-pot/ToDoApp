using ToDoApp.Shared.Common;
using ToDoApp.Shared.Common.Filtering;
using ToDoApp.Shared.ToDoDTO.Commands;
using ToDoApp.Shared.ToDoDTO.Queries;

namespace ToDoApp.Application.ServiceContracts;

public interface IToDoService : ICommonService<ToDoAddRequest, ToDoResponse, ToDoUpdateRequest>
{
    Task<PagedResult<ToDoResponse>> GetItemsWithQuery(QueryRequest queryRequest, CancellationToken cancellationToken);
    Task<ServiceResult<bool>> ChangeCompletionStatus(Guid? id, CancellationToken cancellationToken);
}
