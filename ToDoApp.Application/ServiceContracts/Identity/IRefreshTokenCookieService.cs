namespace ToDoApp.Application.ServiceContracts.Identity;

public interface IRefreshTokenCookieService
{
    void Append(string refreshToken, DateTime expiration);
    bool TryGet(out string? refreshToken);
    void Delete();
}