using ToDoApp.Shared.Common;
using ToDoApp.Shared.Common.Filtering;
using ToDoApp.Shared.TagDTO.Commands;
using ToDoApp.Shared.TagDTO.Queries;
using ToDoApp.SharedUI.ServiceContracts;

namespace ToDoApp.SharedUI.Services;

public class TagService : ITagService
{
    private readonly ApiService _apiService;
    private readonly string _endpoint = "tags";

    public TagService(ApiService apiService)
    {
        _apiService = apiService;
    }

    public async Task<List<TagResponse>> GetItems(CancellationToken token)
        => (await _apiService.GetAsync<List<TagResponse>>(_endpoint, token)).Value ?? [];

    public async Task<ApiResponse<TagResponse>> CreateAsync(TagAddRequest addRequest, CancellationToken token)
        => await _apiService.PostAsync<TagAddRequest, TagResponse>($"{_endpoint}/create", addRequest, token);

    public async Task<ApiResponse<TagResponse>> UpdateAsync(TagUpdateRequest updateRequest, CancellationToken token)
        => await _apiService.PutAsync<TagUpdateRequest, TagResponse>($"{_endpoint}/update", updateRequest, token);

    public async Task<ApiResponse<string>> DeleteAsync(Guid id, CancellationToken token)
        => await _apiService.DeleteAsync($"{_endpoint}/delete/{id}", token);

    public async Task<ApiResponse<TagResponse>> GetByIdAsync(Guid id, CancellationToken token)
      => (await _apiService.GetByIdAsync<TagResponse>($"{_endpoint}/details/{id}", token));

    public async Task<ApiResponse<PagedResult<TagResponse>>> GetItemsWithQuery(QueryRequest queryRequest, CancellationToken token)
   => (await _apiService.PostAsync<QueryRequest, PagedResult<TagResponse>>(_endpoint, queryRequest, token));

    public async Task<List<TagResponse>> GetActiveTags(CancellationToken token)
        => (await _apiService.GetAsync<List<TagResponse>>($"{_endpoint}/active", token)).Value ?? [];
}
