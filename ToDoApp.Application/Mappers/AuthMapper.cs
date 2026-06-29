using ToDoApp.Domain.Identity;
using ToDoApp.Shared.AuthDTO.Register;

namespace ToDoApp.Application.Mappers;

public static class AuthMapper
{
    public static ApplicationUser ToApplicationUser(this RegisterRequest request)
    {
        return new()
        {
            Email = request.Email,
            UserName = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName,
        };
    }
}
