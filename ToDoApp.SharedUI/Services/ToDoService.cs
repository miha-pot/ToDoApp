using ToDoApp.Shared.Common;
using ToDoApp.Shared.ToDoDTO.Commands;
using ToDoApp.Shared.ToDoDTO.Queries;
using ToDoApp.SharedUI.ServiceContracts;
using ToDoApp.SharedUI.Services.Common;

namespace ToDoApp.SharedUI.Services;

public class ToDoService : BaseService<ToDoAddRequest, ToDoResponse, ToDoUpdateRequest>, IToDoService
{
    public ToDoService(ApiService apiService) : base(apiService)
    {
    }

    public override string Endpoint => "todos";

    public async Task<ApiResponse<bool>> ChangeCompletionStatus(Guid id, CancellationToken token)
        => await _apiService.PatchAsync<bool>($"{Endpoint}/{id}/complete", token);

    public async Task<ApiResponse<List<ToDoResponse>>> GetSubTasks(Guid parentId, CancellationToken token)
        => await _apiService.GetAsync<List<ToDoResponse>>($"{Endpoint}/{parentId}/subtasks", token);
}