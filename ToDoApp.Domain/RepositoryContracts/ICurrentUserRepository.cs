namespace ToDoApp.Domain.RepositoryContracts;

public interface ICurrentUserRepository
{
    Guid GetUserId();
}
