using ToDoApp.Shared.Common;
using ToDoApp.Shared.Common.Filtering;
using ToDoApp.SharedUI.ServiceContracts.Common;

namespace ToDoApp.SharedUI.Services.Common;

public abstract class BaseService<TAddRequest, TResponse, TUpdateRequest> : IBaseService<TAddRequest, TResponse, TUpdateRequest>
{
    protected readonly ApiService _apiService;

    public abstract string Endpoint { get; }

    public BaseService(ApiService apiService)
    {
        _apiService = apiService;
    }

    public async Task<ApiResponse<TResponse>> CreateAsync(TAddRequest addRequest, CancellationToken token)
        => await _apiService.PostAsync<TAddRequest, TResponse>($"{Endpoint}/create", addRequest, token);

    public async Task<ApiResponse<string>> DeleteAsync(Guid id, CancellationToken token)
        => await _apiService.DeleteAsync($"{Endpoint}/delete/{id}", token);

    public async Task<ApiResponse<TResponse>> GetByIdAsync(Guid id, CancellationToken token)
        => (await _apiService.GetAsync<TResponse>($"{Endpoint}/details/{id}", token));

    public async Task<ApiResponse<PagedResult<TResponse>>> GetItemsWithQuery(QueryRequest queryRequest, CancellationToken token)
        => (await _apiService.PostAsync<QueryRequest, PagedResult<TResponse>>(Endpoint, queryRequest, token));

    public async Task<ApiResponse<TResponse>> UpdateAsync(TUpdateRequest updateRequest, CancellationToken token)
        => await _apiService.PutAsync<TUpdateRequest, TResponse>($"{Endpoint}/update", updateRequest, token);
}
