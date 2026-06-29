using Microsoft.EntityFrameworkCore;
using ToDoApp.Domain.Entities;
using ToDoApp.Domain.RepositoryContracts;
using ToDoApp.Infrastructure.DatabaseContext;

namespace ToDoApp.Infrastructure.Repositories.EF;

public class TagRepository : BaseRepository<Tag>, ITagRepository
{
    public TagRepository(ApplicationDbContext db) : base(db)
    {
    }

    public async Task<List<Tag>> GetActiveTags(CancellationToken cancellationToken)
    {
        return await _db.Tags.Where(t => t.IsActive).ToListAsync(cancellationToken);
    }
}
