using ToDoApp.Application.ServiceContracts;
using ToDoApp.Shared.Common;
using ToDoApp.Shared.Common.Filtering;
using ToDoApp.Shared.ToDoDTO.Commands;
using ToDoApp.Shared.ToDoDTO.Queries;
using ToDoApp.WebAPI.Extensions;
using ToDoApp.WebAPI.Filters;

namespace ToDoApp.WebAPI.Endpoints.v1;

public static class ToDoEndpoints
{
    public static IEndpointRouteBuilder MapToDoEndpoints(this IEndpointRouteBuilder app)
    {
        var tagGroup = app.MapGroup("/todos")
                          .MapToApiVersion(1, 0)
                          .RequireAuthorization()
                          .RequireRateLimiting("api-policy");

        tagGroup.MapPost("", GetItems);

        tagGroup.MapPost("/create", Create)
                .Produces<ToDoResponse>(200)
                .Produces<string>(400)
                .AddEndpointFilter<ValidationFilter<ToDoAddRequest>>();

        tagGroup.MapDelete("/delete/{toDoItemId:guid}", Delete)
                .Produces<string>(400);

        tagGroup.MapPut("/update", Update)
                .Produces<ToDoResponse>(200)
                .Produces<string>(400)
                .AddEndpointFilter<ValidationFilter<ToDoUpdateRequest>>();

        tagGroup.MapGet("/details/{toDoItemId:guid}", Details)
                .Produces<ToDoResponse>(200)
                .Produces<string>(400);

        tagGroup.MapPatch("/{toDoItemId:guid}/complete", ChangeCompletionStatus)
                .Produces<string>(200)
                .ProducesProblem(400);

        tagGroup.MapGet("/{parentId:guid}/subtasks", GetSubTasks)
                .Produces<List<ToDoResponse>>(200);

        return app;
    }

    public static async Task<IResult> GetItems(QueryRequest queryRequest,
                                               IToDoService toDoService,
                                               CancellationToken cancellationToken)
    {
        ServiceResult<PagedResult<ToDoResponse>> result = await toDoService.GetItemsWithQuery(queryRequest, cancellationToken);

        return result.ToHttpResult();
    }

    public static async Task<IResult> Create(ToDoAddRequest? addRequest,
                                             IToDoService toDoService,
                                             CancellationToken cancellationToken)
    {
        ServiceResult<ToDoResponse> result = await toDoService.CreateItem(addRequest, cancellationToken);

        return result.ToHttpResult();
    }

    public static async Task<IResult> Delete(Guid? toDoItemId,
                                             IToDoService toDoService,
                                             CancellationToken cancellationToken)
    {
        ServiceResult<string> result = await toDoService.DeleteItem(toDoItemId, cancellationToken);

        return result.ToHttpResult();
    }

    public static async Task<IResult> Details(Guid? toDoItemId,
                                              IToDoService toDoService,
                                              CancellationToken cancellationToken)
    {
        ServiceResult<ToDoResponse?> result = await toDoService.GetItemById(toDoItemId, cancellationToken);

        return result.ToHttpResult();
    }

    public static async Task<IResult> Update(ToDoUpdateRequest? updateRequest,
                                             IToDoService toDoService,
                                             CancellationToken cancellationToken)
    {
        ServiceResult<ToDoResponse> result = await toDoService.UpdateItem(updateRequest, cancellationToken);

        return result.ToHttpResult();
    }

    public static async Task<IResult> ChangeCompletionStatus(Guid? toDoItemId,
                                                             IToDoService toDoService,
                                                             CancellationToken cancellationToken)
    {
        ServiceResult<bool> result = await toDoService.ChangeCompletionStatus(toDoItemId, cancellationToken);

        return result.ToHttpResult();
    }

    public static async Task<IResult> GetSubTasks(Guid parentId,
                                                  IToDoService toDoService,
                                                  CancellationToken cancellationToken)
    {
        ServiceResult<List<ToDoResponse>> result = await toDoService.GetSubTasks(parentId, cancellationToken);

        return result.ToHttpResult();
    }
}
