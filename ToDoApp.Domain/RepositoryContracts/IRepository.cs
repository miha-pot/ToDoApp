using ToDoApp.Domain.Common;

namespace ToDoApp.Domain.RepositoryContracts;

public interface IRepository<T> where T : class
{
    Task<List<T>> GetAllAsync(CancellationToken cancellationToken);
    Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<DatabaseResult> AddAsync(T entity, CancellationToken cancellationToken);
    Task<DatabaseResult> UpdateAsync(T entity, CancellationToken cancellationToken);
    Task<DatabaseResult> DeleteAsync(Guid id, CancellationToken cancellationToken);
}
