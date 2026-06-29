using ToDoApp.Shared.Common;

namespace ToDoApp.Application.ServiceContracts;

public interface ICommonService<TAdd, TResponse, TUpdate>
{
    Task<ServiceResult<TResponse>> CreateItem(TAdd? addRequest, CancellationToken cancellationToken);
    Task<ServiceResult<TResponse?>> GetItemById(Guid? itemId, CancellationToken cancellationToken);
    Task<ServiceResult<TResponse>> UpdateItem(TUpdate? updateRequest, CancellationToken cancellationToken);
    Task<ServiceResult<string>> DeleteItem(Guid? itemId, CancellationToken cancellationToken);
}
