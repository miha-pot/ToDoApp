using System.Security.Cryptography;

namespace ToDoApp.Infrastructure.Services;

public static class RefreshTokenGenerator
{
    public static string Generate()
    {
        var bytes = new byte[64];
        using var rng = RandomNumberGenerator.Create(); // was never disposed before
        rng.GetBytes(bytes);
        return Convert.ToBase64String(bytes);
    }
}
