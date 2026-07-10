using Microsoft.EntityFrameworkCore;
using ToDoApp.Domain.Entities;
using ToDoApp.Domain.RepositoryContracts;
using ToDoApp.Infrastructure.DatabaseContext;

namespace ToDoApp.Infrastructure.Repositories.EF;

public class TagRepository : BaseRepository<Tag>, ITagRepository
{
    private readonly ICurrentUserRepository _currentUserRepository;

    public TagRepository(ApplicationDbContext db, ICurrentUserRepository currentUserRepository) : base(db)
    {
        _currentUserRepository = currentUserRepository;
    }

    public async Task<List<Tag>> GetActiveTags(CancellationToken cancellationToken)
    {
        Guid userId = _currentUserRepository.GetUserId();

        return await _db.Tags.Where(t => t.IsActive && t.UserId == userId).ToListAsync(cancellationToken);
    }
}
