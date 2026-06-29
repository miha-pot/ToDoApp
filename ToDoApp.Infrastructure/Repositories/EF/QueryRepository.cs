using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using ToDoApp.Application.RepositoryContracts;
using ToDoApp.Domain.EntityContract;
using ToDoApp.Domain.RepositoryContracts;
using ToDoApp.Infrastructure.DatabaseContext;
using ToDoApp.Infrastructure.Extensions;
using ToDoApp.Shared.Common.Filtering;

namespace ToDoApp.Infrastructure.Repositories.EF;

public class QueryRepository : IQueryRepository
{
    private readonly ICurrentUserRepository _currentUserRepository;
    protected readonly ApplicationDbContext _db;

    public QueryRepository(ICurrentUserRepository currentUserRepository, ApplicationDbContext db)
    {
        _currentUserRepository = currentUserRepository;
        _db = db;
    }

    public virtual async Task<PagedResult<TDto>> GetListByQueryAsync<T, TDto>(QueryRequest request,
                                                                           Expression<Func<T, TDto>> projection,
                                                                           CancellationToken cancellationToken)
        where T : class
    {
        Guid currentUserId = _currentUserRepository.GetUserId();

        var query = _db.Set<T>().AsNoTracking();

        if (typeof(IUserOwnedEntity).IsAssignableFrom(typeof(T)))
        {
            query = query.Where(e => ((IUserOwnedEntity)e).UserId == currentUserId);
        }

        var baseQuery = query.ApplyFilters(request.Filters)
                             .ApplySort(request.SortBy, request.SortDescending);

        var total = await baseQuery.CountAsync(cancellationToken);

        var items = await baseQuery.ApplyPaging(request.Page, request.PageSize)
                                   .Select(projection)
                                   .ToListAsync(cancellationToken);

        return new()
        {
            Items = items,
            TotalCount = total,
            Page = request.Page,
            PageSize = request.PageSize
        };
    }

    public IQueryable<T> BuildBaseQuery<T>(QueryRequest request) where T : class
    {
        Guid currentUserId = _currentUserRepository.GetUserId();

        // Pomembno: Tukaj vzamemo navaden Set<T>, ker bomo .AsNoTracking() dali na koncu, 
        // ko bomo dejansko izvajali query, da Include-i delujejo pravilno.
        var query = _db.Set<T>().AsQueryable();

        if (typeof(IUserOwnedEntity).IsAssignableFrom(typeof(T)))
        {
            query = query.Where(e => ((IUserOwnedEntity)e).UserId == currentUserId);
        }

        return query.ApplyFilters(request.Filters)
                    .ApplySort(request.SortBy, request.SortDescending);
    }

    // 🟢 DEL B: Izvedba (Count + Paging + Projekcija + DEJANSKI KLIC NA BAZO)
    public async Task<PagedResult<TDto>> ExecuteQueryAsync<T, TDto>(IQueryable<T> baseQuery,
                                                                    QueryRequest request,
                                                                    Expression<Func<T, TDto>> projection,
                                                                    CancellationToken cancellationToken) where T : class
    {
        // Pred izvedbo dodamo AsNoTracking za boljšo hitrost branja
        var finalQuery = baseQuery.AsNoTracking();

        // 💥 Prvi klic na bazo (SELECT COUNT(*))
        var total = await finalQuery.CountAsync(cancellationToken);

        // 💥 Drugi klic na bazo (SELECT ... z vsemi JOIN-i)
        var items = await finalQuery.ApplyPaging(request.Page, request.PageSize)
                                    .Select(projection)
                                    .ToListAsync(cancellationToken);

        return new PagedResult<TDto>
        {
            Items = items,
            TotalCount = total,
            Page = request.Page,
            PageSize = request.PageSize
        };
    }
}
