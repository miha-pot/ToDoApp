using Microsoft.EntityFrameworkCore;
using ToDoApp.Domain.Entities;
using ToDoApp.Domain.RepositoryContracts;
using ToDoApp.Infrastructure.DatabaseContext;

namespace ToDoApp.Infrastructure.Repositories.EF;

public class TodoItemRepository : BaseRepository<TodoItem>, ITodoItemRepository
{
    public TodoItemRepository(ApplicationDbContext db) : base(db)
    {
    }

    public override async Task<TodoItem?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _dbSet.Include(x => x.TodoItemTags).ThenInclude(x => x.Tag).FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<List<TodoItem>>   GetSubTasks(Guid parentId, CancellationToken cancellationToken)
    {
        return await _dbSet.Where(x => x.ParentTodoId == parentId).Include(x => x.TodoItemTags).ThenInclude(x => x.Tag).ToListAsync(cancellationToken);
    }
}
