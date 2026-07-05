using ToDoApp.Shared.Common;
using ToDoApp.Shared.Common.Filtering;
using ToDoApp.Shared.ToDoDTO.Commands;
using ToDoApp.Shared.ToDoDTO.Queries;
using ToDoApp.SharedUI.ServiceContracts;

namespace ToDoApp.SharedUI.Services;

public class ToDoService : IToDoService
{
    private readonly ApiService _apiService;
    private readonly string _endpoint = "todos";

    public ToDoService(ApiService apiService)
    {
        _apiService = apiService;
    }
    public async Task<ApiResponse<PagedResult<ToDoResponse>>> GetItemsWithQuery(QueryRequest queryRequest, CancellationToken token)
      => (await _apiService.PostAsync<QueryRequest, PagedResult<ToDoResponse>>(_endpoint, queryRequest, token));

    public async Task<ApiResponse<ToDoResponse>> GetByIdAsync(Guid id, CancellationToken token)
      => (await _apiService.GetByIdAsync<ToDoResponse>($"{_endpoint}/details/{id}", token));

    public async Task<ApiResponse<ToDoResponse>> CreateAsync(ToDoAddRequest addRequest, CancellationToken token)
        => await _apiService.PostAsync<ToDoAddRequest, ToDoResponse>($"{_endpoint}/create", addRequest, token);

    public async Task<ApiResponse<ToDoResponse>> UpdateAsync(ToDoUpdateRequest updateRequest, CancellationToken token)
        => await _apiService.PutAsync<ToDoUpdateRequest, ToDoResponse>($"{_endpoint}/update", updateRequest, token);

    public async Task<ApiResponse<string>> DeleteAsync(Guid id, CancellationToken token)
        => await _apiService.DeleteAsync($"{_endpoint}/delete/{id}", token);

    public async Task<ApiResponse<bool>> ChangeCompletionStatus(Guid id, CancellationToken token)
        => await _apiService.PatchAsync<bool>($"{_endpoint}/{id}/complete", token);

    public async Task<ApiResponse<List<ToDoResponse>>> GetSubTasks(Guid parentId, CancellationToken token)
        => await _apiService.GetAsync<List<ToDoResponse>>($"{_endpoint}/{parentId}/subtasks", token);
}