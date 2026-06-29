using ToDoApp.Domain.Entities;

namespace ToDoApp.Domain.RepositoryContracts;

public interface ITagRepository : IRepository<Tag>
{
    Task<List<Tag>> GetActiveTags(CancellationToken cancellationToken);
}
