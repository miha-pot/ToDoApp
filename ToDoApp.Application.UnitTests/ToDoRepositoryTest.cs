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
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new ApplicationDbContext(options, _mockHttpContextAccessor);
    }

    [Fact]
    public async Task AddAsync_Should_InsertTodoItem_IntoDatabase()
    {
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

        var result = await repository.AddAsync(todoItem, CancellationToken.None);

        result.Should().BeTrue();

        var dbItem = await context.TodoItems.FindAsync(todoId);
        dbItem.Should().NotBeNull();
        dbItem!.Title.Should().Be("Napisati unit teste");
        dbItem.IsCompleted.Should().BeFalse();
    }

    [Fact]
    public async Task GetByIdAsync_Should_ReturnTodoItem_WhenItExists()
    {
        using var context = CreateInMemoryDbContext();

        var todoId = Guid.NewGuid();
        var existingTodo = new TodoItem { Id = todoId, Title = "Obstoječe opravilo" };

        await context.TodoItems.AddAsync(existingTodo);
        await context.SaveChangesAsync();

        var repository = new TodoItemRepository(context);

        var result = await repository.GetByIdAsync(todoId, CancellationToken.None);

        result.Should().NotBeNull();
        result!.Id.Should().Be(todoId);
        result.Title.Should().Be("Obstoječe opravilo");
    }

    [Fact]
    public async Task GetByIdAsync_Should_ReturnNull_WhenItemDoesNotExist()
    {
        using var context = CreateInMemoryDbContext();
        var repository = new TodoItemRepository(context);

        var result = await repository.GetByIdAsync(Guid.NewGuid(), CancellationToken.None);

        result.Should().BeNull();
    }

    [Fact]
    public async Task UpdateAsync_Should_ModifyExistingTodoItem_InDatabase()
    {
        using var context = CreateInMemoryDbContext();
        var todoId = Guid.NewGuid();
        var originalTodo = new TodoItem { Id = todoId, Title = "Stari naslov", IsCompleted = false };

        await context.TodoItems.AddAsync(originalTodo);
        await context.SaveChangesAsync();

        var repository = new TodoItemRepository(context);
        var todoToUpdate = await context.TodoItems.FindAsync(todoId);

        todoToUpdate!.Title = "Novi posodobljen naslov";
        todoToUpdate.IsCompleted = true;

        var result = await repository.UpdateAsync(todoToUpdate, CancellationToken.None);

        result.Should().BeTrue();

        var updatedDbItem = await context.TodoItems.FindAsync(todoId);
        updatedDbItem.Should().NotBeNull();
        updatedDbItem!.Title.Should().Be("Novi posodobljen naslov");
        updatedDbItem.IsCompleted.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteAsync_Should_RemoveTodoItem_FromDatabase()
    {
        using var context = CreateInMemoryDbContext();
        var todoId = Guid.NewGuid();
        var todoToDelete = new TodoItem { Id = todoId, Title = "Opravilo za brisanje" };

        await context.TodoItems.AddAsync(todoToDelete);
        await context.SaveChangesAsync();

        var repository = new TodoItemRepository(context);

        var result = await repository.DeleteAsync(todoId, CancellationToken.None);

        result.Should().BeTrue();

        var deletedDbItem = await context.TodoItems.FindAsync(todoId);
        deletedDbItem.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdWithTagsAsync_Should_IncludeConnectedTags()
    {
        using var context = CreateInMemoryDbContext();

        var todoId = Guid.NewGuid();
        var tagId = Guid.NewGuid();

        var todo = new TodoItem { Id = todoId, Title = "Todo s tagom" };
        var tag = new Tag { Id = tagId, Name = "Nujno", BgColorHex = "#000", ColorHex = "#fff" };
        var todoTag = new TodoItemTag { TodoItemId = todoId, TagId = tagId, Tag = tag };

        await context.TodoItems.AddAsync(todo);
        await context.Tags.AddAsync(tag);
        await context.TodoItemTags.AddAsync(todoTag);
        await context.SaveChangesAsync();

        var repository = new TodoItemRepository(context);

        var result = await repository.GetByIdAsync(todoId, CancellationToken.None);

        result.Should().NotBeNull();

        result!.TodoItemTags.Should().NotBeEmpty();
        result.TodoItemTags.First().Tag.Name.Should().Be("Nujno");
    }
}