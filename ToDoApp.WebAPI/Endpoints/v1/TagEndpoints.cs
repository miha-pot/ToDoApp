using ToDoApp.Application.ServiceContracts;
using ToDoApp.Shared.Common;
using ToDoApp.Shared.Common.Filtering;
using ToDoApp.Shared.TagDTO.Commands;
using ToDoApp.Shared.TagDTO.Queries;
using ToDoApp.WebAPI.Extensions;
using ToDoApp.WebAPI.Filters;

namespace ToDoApp.WebAPI.Endpoints.v1;

public static class TagEndpoints
{
    public static IEndpointRouteBuilder MapTagEndpoints(this IEndpointRouteBuilder app)
    {
        var tagGroup = app.MapGroup("/tags")
                          .MapToApiVersion(1, 0)
                          .RequireAuthorization()
                          .RequireRateLimiting("api-policy");

        tagGroup.MapPost("", GetItems);

        tagGroup.MapPost("/create", Create)
                .Produces<TagResponse>(200)
                .Produces<string>(400)
                .AddEndpointFilter<ValidationFilter<TagAddRequest>>();

        tagGroup.MapDelete("/delete/{tagId:guid}", Delete)
                .Produces<string>(400);

        tagGroup.MapPut("/update", Update)
                .Produces<TagResponse>(200)
                .Produces<string>(400)
                .AddEndpointFilter<ValidationFilter<TagUpdateRequest>>();

        tagGroup.MapGet("/details/{tagId:guid}", Details)
                .Produces<TagResponse>(200)
                .Produces<string>(400);

        tagGroup.MapGet("/active", GetActiveTags)
                .Produces<List<TagResponse>>(200)
                .Produces<string>(400);

        return app;
    }

    public static async Task<IResult> GetItems(QueryRequest queryRequest,
                                               ITagService tagService,
                                               CancellationToken cancellationToken)
    {
        ServiceResult<PagedResult<TagResponse>> toDoItems = await tagService.GetItemsWithQuery(queryRequest, cancellationToken);

        return toDoItems.ToHttpResult();
    }

    public static async Task<IResult> Create(TagAddRequest? addRequest,
                                             ITagService tagService,
                                             CancellationToken cancellationToken)
    {
        ServiceResult<TagResponse> result = await tagService.CreateItem(addRequest, cancellationToken);

        return result.ToHttpResult();
    }

    public static async Task<IResult> Delete(Guid? tagId,
                                             ITagService tagService,
                                             CancellationToken cancellationToken)
    {
        ServiceResult<string> result = await tagService.DeleteItem(tagId, cancellationToken);

        return result.ToHttpResult();
    }

    public static async Task<IResult> Details(Guid? tagId,
                                              ITagService tagService,
                                              CancellationToken cancellationToken)
    {
        ServiceResult<TagResponse?> result = await tagService.GetItemById(tagId, cancellationToken);

        return result.ToHttpResult();
    }

    public static async Task<IResult> Update(TagUpdateRequest? updateRequest,
                                             ITagService tagService,
                                             CancellationToken cancellationToken)
    {
        ServiceResult<TagResponse> result = await tagService.UpdateItem(updateRequest, cancellationToken);

        return result.ToHttpResult();
    }

    public static async Task<IResult> GetActiveTags(ITagService tagService,
                                                    CancellationToken cancellationToken)
    {
        ServiceResult<List<TagResponse>> result = await tagService.GetActiveTags(cancellationToken);

        return result.ToHttpResult();
    }
}
