using ToDoApp.Domain.Entities;

namespace ToDoApp.Domain.RepositoryContracts;

public interface ITodoItemRepository : IRepository<TodoItem>
{
    Task<List<TodoItem>> GetSubTasks(Guid parentId, CancellationToken cancellationToken);
}
