using System.Linq.Expressions;
using ToDoApp.Shared.Common.Filtering;

namespace ToDoApp.Application.RepositoryContracts;

public interface IQueryRepository
{
    Task<PagedResult<TDto>> GetListByQueryAsync<T, TDto>(QueryRequest request, Expression<Func<T, TDto>> projection, CancellationToken cancellationToken) where T : class;
    IQueryable<T> BuildBaseQuery<T>(QueryRequest request) where T : class;
    Task<PagedResult<TDto>> ExecuteQueryAsync<T, TDto>(
        IQueryable<T> baseQuery,
        QueryRequest request,
        Expression<Func<T, TDto>> projection, CancellationToken cancellationToken) where T : class;

}
