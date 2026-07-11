using Microsoft.EntityFrameworkCore;
using ToDoApp.Domain.Common;
using ToDoApp.Domain.RepositoryContracts;
using ToDoApp.Infrastructure.DatabaseContext;

namespace ToDoApp.Infrastructure.Repositories.EF;

public abstract class BaseRepository<T> : IRepository<T>
    where T : class
{
    protected readonly ApplicationDbContext _db;
    protected readonly DbSet<T> _dbSet;

    public BaseRepository(ApplicationDbContext db)
    {
        _db = db;
        _dbSet = _db.Set<T>();
    }

    public virtual async Task<List<T>> GetAllAsync(CancellationToken cancellationToken)
    {
        return await _dbSet.AsNoTracking().ToListAsync(cancellationToken);
    }

    public virtual async Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _dbSet.FindAsync([id], cancellationToken);
    }

    public virtual async Task<DatabaseResult> AddAsync(T entity, CancellationToken cancellationToken)
    {
        await _dbSet.AddAsync(entity, cancellationToken);

        bool isSaved = await _db.SaveChangesAsync(cancellationToken) > 0;

        return isSaved ? DatabaseResult.Success : DatabaseResult.Failed;
    }

    public virtual async Task<DatabaseResult> UpdateAsync(T entity, CancellationToken cancellationToken)
    {
        var entry = _db.Entry(entity);

        if (entry.State == EntityState.Detached)
        {
            _dbSet.Update(entity);
        }

        if (entry.State == EntityState.Unchanged)
        {
            return DatabaseResult.NoChanges;
        }

        bool isSaved = await _db.SaveChangesAsync(cancellationToken) > 0;

        return isSaved ? DatabaseResult.Success : DatabaseResult.Failed;
    }

    public virtual async Task<DatabaseResult> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var entity = await _dbSet.FindAsync([id], cancellationToken);
        if (entity is null)
        {
            return DatabaseResult.NotFound;
        }

        _dbSet.Remove(entity);

        bool isDeleted = await _db.SaveChangesAsync(cancellationToken) > 0;

        return isDeleted ? DatabaseResult.Success : DatabaseResult.Failed;
    }
}
