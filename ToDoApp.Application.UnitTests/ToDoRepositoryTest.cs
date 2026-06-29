using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using ToDoApp.Domain.Entities;
using ToDoApp.Infrastructure.DatabaseContext;
using ToDoApp.Infrastructure.Repositories.EF;

namespace ToDoApp.Application.UnitTests;

public class TodoRepositoryTests
{
    private readonly IHttpContextAccessor _mockHttpContextAccessor;

    public TodoRepositoryTests()
    {
        _mockHttpContextAccessor = Substitute.For<HttpContextAccessor>();
    }

    private ApplicationDbContext CreateInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<DbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()) // Vsak test svojo bazo
            .Options;
        return new ApplicationDbContext(options, _mockHttpContextAccessor);
    }

    [Fact]
    public async Task AddAsync_Should_InsertTodoItem_IntoDatabase()
    {
        // Arrange
        using var context = CreateInMemoryDbContext();
        var repository = new TodoItemRepository(context);

        var todoId = Guid.NewGuid();
        var todoItem = new TodoItem
        {
            Id = todoId,
            Title = "Napisati unit teste",
            Description = "Pokriti repozitorij z integracijskimi testi",
            IsCompleted = false
        };

        // Act
        var result = await repository.AddAsync(todoItem);

        // Assert
        result.Should().BeTrue();

        // Preverimo direktno v DbContextu, če zapis zares obstaja v tabeli
        var dbItem = await context.TodoItems.FindAsync(todoId);
        dbItem.Should().NotBeNull();
        dbItem!.Title.Should().Be("Napisati unit teste");
        dbItem.IsCompleted.Should().BeFalse();
    }

    [Fact]
    public async Task GetByIdAsync_Should_ReturnTodoItem_WhenItExists()
    {
        // Arrange
        using var context = CreateInMemoryDbContext();

        var todoId = Guid.NewGuid();
        var existingTodo = new TodoItem { Id = todoId, Title = "Obstoječe opravilo" };

        // Ročno "seed-amo" (vstavimo) podatek v bazo pred klicem repozitorija
        await context.TodoItems.AddAsync(existingTodo);
        await context.SaveChangesAsync();

        var repository = new TodoItemRepository(context);

        // Act
        var result = await repository.GetByIdAsync(todoId);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(todoId);
        result.Title.Should().Be("Obstoječe opravilo");
    }

    [Fact]
    public async Task GetByIdAsync_Should_ReturnNull_WhenItemDoesNotExist()
    {
        // Arrange
        using var context = CreateInMemoryDbContext();
        var repository = new TodoItemRepository(context);

        // Act
        var result = await repository.GetByIdAsync(Guid.NewGuid()); // Naključen neobstoječ ID

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task UpdateAsync_Should_ModifyExistingTodoItem_InDatabase()
    {
        // Arrange
        using var context = CreateInMemoryDbContext();
        var todoId = Guid.NewGuid();
        var originalTodo = new TodoItem { Id = todoId, Title = "Stari naslov", IsCompleted = false };

        await context.TodoItems.AddAsync(originalTodo);
        await context.SaveChangesAsync();

        // Ponovno naložimo za repozitorij (simuliramo nov request)
        var repository = new TodoItemRepository(context);
        var todoToUpdate = await context.TodoItems.FindAsync(todoId);

        // Spremenimo lastnosti entitete
        todoToUpdate!.Title = "Novi posodobljen naslov";
        todoToUpdate.IsCompleted = true;

        // Act
        var result = await repository.UpdateAsync(todoToUpdate);

        // Assert
        result.Should().BeTrue();

        // Preverimo neposredno v bazi, če so se podatki prepisali
        var updatedDbItem = await context.TodoItems.FindAsync(todoId);
        updatedDbItem.Should().NotBeNull();
        updatedDbItem!.Title.Should().Be("Novi posodobljen naslov");
        updatedDbItem.IsCompleted.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteAsync_Should_RemoveTodoItem_FromDatabase()
    {
        // Arrange
        using var context = CreateInMemoryDbContext();
        var todoId = Guid.NewGuid();
        var todoToDelete = new TodoItem { Id = todoId, Title = "Opravilo za brisanje" };

        await context.TodoItems.AddAsync(todoToDelete);
        await context.SaveChangesAsync();

        var repository = new TodoItemRepository(context);

        // Act
        var result = await repository.DeleteAsync(todoId);

        // Assert
        result.Should().BeTrue();

        // Preverimo, da zapisa ni več v bazi
        var deletedDbItem = await context.TodoItems.FindAsync(todoId);
        deletedDbItem.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdWithTagsAsync_Should_IncludeConnectedTags()
    {
        // Arrange
        using var context = CreateInMemoryDbContext();

        var todoId = Guid.NewGuid();
        var tagId = Guid.NewGuid();

        var todo = new TodoItem { Id = todoId, Title = "Todo s tagom" };
        var tag = new Tag { Id = tagId, Name = "Nujno", BgColorHex = "#000", ColorHex = "#fff" };
        var todoTag = new TodoItemTag { TodoItemId = todoId, TagId = tagId, Tag = tag };

        // Vstavimo celotno relacijsko strukturo v InMemory bazo
        await context.TodoItems.AddAsync(todo);
        await context.Tags.AddAsync(tag);
        await context.TodoItemTags.AddAsync(todoTag);
        await context.SaveChangesAsync();

        var repository = new TodoItemRepository(context);

        // Act
        // (Predpostavljamo, da tvoj repozitorij ponuja metodo z vključenimi tagi)
        var result = await repository.GetByIdAsync(todoId);

        // Assert
        result.Should().NotBeNull();
        // Če tvoja metoda v repozitoriju dela .Include(), bo naslednja trditev vrnila True:
        result!.TodoItemTags.Should().NotBeEmpty();
        result.TodoItemTags.First().Tag.Name.Should().Be("Nujno");
    }
}