using ToDoApp.Shared.Common;
using ToDoApp.Shared.TagDTO.Commands;
using ToDoApp.Shared.TagDTO.Queries;
using ToDoApp.SharedUI.ServiceContracts;
using ToDoApp.SharedUI.Services.Common;

namespace ToDoApp.SharedUI.Services;

public class TagService : BaseService<TagAddRequest, TagResponse, TagUpdateRequest>, ITagService
{
    public TagService(ApiService apiService) : base(apiService)
    {
    }

    public override string Endpoint => "tags";

    public async Task<ApiResponse<List<TagResponse>>> GetActiveTags(CancellationToken token)
        => await _apiService.GetAsync<List<TagResponse>>($"{Endpoint}/active", token);
}
