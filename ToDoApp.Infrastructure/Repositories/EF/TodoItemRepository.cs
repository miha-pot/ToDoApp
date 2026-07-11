using Microsoft.EntityFrameworkCore;
using ToDoApp.Domain.Common;
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

    public async Task<List<TodoItem>> GetSubTasks(Guid parentId, CancellationToken cancellationToken)
    {
        return await _dbSet.Where(x => x.ParentTodoId == parentId).Include(x => x.TodoItemTags).ThenInclude(x => x.Tag).ToListAsync(cancellationToken);
    }

    public async Task<DatabaseResult> UpdateAsync(TodoItem entity, List<Guid> newTagIds, CancellationToken cancellationToken)
    {
        var existingTodo = await _db.TodoItems
            .AsTracking()
            .Include(t => t.TodoItemTags)
            .FirstOrDefaultAsync(t => t.Id == entity.Id, cancellationToken);

        if (existingTodo == null)
        {
            return DatabaseResult.Failed;
        }

        _db.Entry(existingTodo).CurrentValues.SetValues(entity);

        var currentTagIds = existingTodo.TodoItemTags.Select(x => x.TagId).ToList();

        var tagsToRemove = existingTodo.TodoItemTags
            .Where(x => !newTagIds.Contains(x.TagId))
            .ToList();

        foreach (var tagRelation in tagsToRemove)
        {
            existingTodo.TodoItemTags.Remove(tagRelation);
        }

        var tagIdsToAdd = newTagIds
            .Where(id => !currentTagIds.Contains(id))
            .ToList();

        foreach (var tagId in tagIdsToAdd)
        {
            existingTodo.TodoItemTags.Add(new TodoItemTag
            {
                TodoItemId = entity.Id,
                TagId = tagId
            });
        }

        var hasChanges = _db.ChangeTracker.HasChanges();
        if (!hasChanges)
        {
            return DatabaseResult.NoChanges;
        }

        bool isSaved = await _db.SaveChangesAsync(cancellationToken) > 0;

        return isSaved ? DatabaseResult.Success : DatabaseResult.Failed;
    }
}
