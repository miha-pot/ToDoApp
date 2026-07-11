using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using ToDoApp.Domain.RepositoryContracts;

namespace ToDoApp.Infrastructure.Repositories;

public class CurrentUserRepository : ICurrentUserRepository
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserRepository(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid GetUserId()
    {
        var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        return Guid.TryParse(userIdClaim, out var parsedGuid) ? parsedGuid : throw new NotImplementedException("User id was not received!");
    }
}
