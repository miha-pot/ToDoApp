using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using ToDoApp.Domain.Entities;
using ToDoApp.Domain.RepositoryContracts;
using ToDoApp.Infrastructure.DatabaseContext;
using ToDoApp.Infrastructure.Repositories;
using ToDoApp.Infrastructure.Repositories.EF;

namespace ToDoApp.Application.UnitTests.TagTests;

public class TagRepositoryTests
{
    private readonly IHttpContextAccessor _mockHttpContextAccessor;
    private readonly ICurrentUserRepository _mockCurrentUserRepository;

    public TagRepositoryTests()
    {
        _mockHttpContextAccessor = Substitute.For<HttpContextAccessor>();
        _mockCurrentUserRepository = Substitute.For<CurrentUserRepository>();
    }

    private ApplicationDbContext CreateInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<DbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new ApplicationDbContext(options, _mockHttpContextAccessor);
    }

    [Fact]
    public async Task AddAsync_Should_InsertTag_IntoDatabase()
    {
        using var context = CreateInMemoryDbContext();
        var repository = new TagRepository(context, _mockCurrentUserRepository);

        var tagId = Guid.NewGuid();
        var tag = new Tag
        {
            Id = tagId,
            Name = "Business",
            ColorHex = "#FF5733",
            BgColorHex = "#C70039",
        };

        var result = await repository.AddAsync(tag, CancellationToken.None);

        result.Should().BeTrue();

        var dbItem = await context.Tags.FindAsync(tagId);
        dbItem.Should().NotBeNull();
        dbItem.Name.Should().Be("Business");
    }

    [Fact]
    public async Task GetByIdAsync_Should_ReturnTag_WhenItExists()
    {
        using var context = CreateInMemoryDbContext();

        var tagId = Guid.NewGuid();
        var existingTag = new Tag { Id = tagId, Name = "Business", ColorHex = "#FF5733", BgColorHex = "#C70039" };

        await context.Tags.AddAsync(existingTag);
        await context.SaveChangesAsync();

        var repository = new TagRepository(context, _mockCurrentUserRepository);

        var result = await repository.GetByIdAsync(tagId, CancellationToken.None);

        result.Should().NotBeNull();
        result.Id.Should().Be(tagId);
        result.Name.Should().Be("Business");
    }

    [Fact]
    public async Task GetByIdAsync_Should_ReturnNull_WhenItemDoesNotExist()
    {
        using var context = CreateInMemoryDbContext();
        var repository = new TagRepository(context, _mockCurrentUserRepository);

        var result = await repository.GetByIdAsync(Guid.NewGuid(), CancellationToken.None);

        result.Should().BeNull();
    }

    [Fact]
    public async Task UpdateAsync_Should_ModifyExistingTag_InDatabase()
    {
        using var context = CreateInMemoryDbContext();
        var tagId = Guid.NewGuid();
        var originalTag = new Tag { Id = tagId, Name = "Stari naziv", ColorHex = "#FF5733", BgColorHex = "#C70039" };

        await context.Tags.AddAsync(originalTag);
        await context.SaveChangesAsync();

        var repository = new TagRepository(context, _mockCurrentUserRepository);
        var tagToUpdate = await context.Tags.FindAsync(tagId);

        tagToUpdate!.Name = "Novi naziv";

        var result = await repository.UpdateAsync(tagToUpdate, CancellationToken.None);

        result.Should().BeTrue();

        var updatedDbItem = await context.Tags.FindAsync(tagId);
        updatedDbItem.Should().NotBeNull();
        updatedDbItem!.Name.Should().Be("Novi naziv");
    }

    [Fact]
    public async Task DeleteAsync_Should_RemoveTag_FromDatabase()
    {
        using var context = CreateInMemoryDbContext();
        var tagId = Guid.NewGuid();
        var tagToDelete = new Tag { Id = tagId, Name = "Opravilo za brisanje", ColorHex = "#FF5733", BgColorHex = "#C70039" };

        await context.Tags.AddAsync(tagToDelete);
        await context.SaveChangesAsync();

        var repository = new TagRepository(context, _mockCurrentUserRepository);

        var result = await repository.DeleteAsync(tagId, CancellationToken.None);

        result.Should().BeTrue();

        var deletedDbItem = await context.Tags.FindAsync(tagId);
        deletedDbItem.Should().BeNull();
    }
}