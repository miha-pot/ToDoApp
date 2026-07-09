using FluentAssertions;
using System.Net;
using System.Net.Http.Json;
using ToDoApp.Shared.AuthDTO.Register;

namespace ToDoApp.Infrastructure.IntegrationTests;

public class TodoApiTests : IClassFixture<ToDoApiFactory>
{
    private readonly HttpClient _client;

    public TodoApiTests(ToDoApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetAllTodos_ShouldReturnOk()
    {

        var request = new RegisterRequest()
        {
            FirstName = "Test",
            LastName = "User",
            Email = "test@user.com",
            Password = "Test1234!",
            ConfirmPassword = "Test1234!"
        };

        // Act: Klic na API endpoint 
        var response = await _client.PostAsJsonAsync("/api/v1/auth/register", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
