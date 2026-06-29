namespace ToDoApp.Domain.EntityContract;

public interface IUserOwnedEntity
{
    Guid UserId { get; set; }
}
