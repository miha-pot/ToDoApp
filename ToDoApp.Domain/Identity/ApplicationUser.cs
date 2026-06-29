using Microsoft.AspNetCore.Identity;

namespace ToDoApp.Domain.Identity;

public class ApplicationUser : IdentityUser<Guid>
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? RefreshToken { get; set; }
    public DateTime RefreshTokenExpirationDateTime { get; set; }
}
