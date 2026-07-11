using FluentAssertions;
using NSubstitute;
using System.Net;
using ToDoApp.Application.RepositoryContracts;
using ToDoApp.Application.Services;
using ToDoApp.Domain.Common;
using ToDoApp.Domain.Entities;
using ToDoApp.Domain.RepositoryContracts;
using ToDoApp.Shared.ToDoDTO.Commands;

namespace ToDoApp.Application.UnitTests.TagTests
{
    public class ToDoServiceTests
    {
        private readonly ITodoItemRepository _mockRepository;
        private readonly IQueryRepository _mockQueryRepository;
        private readonly ICurrentUserRepository _mockCurrentUserRepository;

        private readonly ToDoService _service;

        public ToDoServiceTests()
        {
            _mockRepository = Substitute.For<ITodoItemRepository>();
            _mockQueryRepository = Substitute.For<IQueryRepository>();
            _mockCurrentUserRepository = Substitute.For<ICurrentUserRepository>();
            _service = new ToDoService(_mockRepository, _mockQueryRepository, _mockCurrentUserRepository);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task ChangeCompletionStatus_ShouldToggleStatus_WhenItemExists(bool initialStatus)
        {
            var id = Guid.NewGuid();
            var item = new TodoItem { Id = id, IsCompleted = initialStatus };
            _mockRepository.GetByIdAsync(id, CancellationToken.None).Returns(item);
            _mockRepository.UpdateAsync(Arg.Any<TodoItem>(), CancellationToken.None).Returns(DatabaseResult.Success);

            var result = await _service.ChangeCompletionStatus(id, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Value.Should().Be(true);
            await _mockRepository.Received(1).UpdateAsync(Arg.Is<TodoItem>(x => x.IsCompleted == !initialStatus), CancellationToken.None);
        }

        [Fact]
        public async Task ChangeCompletionStatus_ShouldReturnNotFound_WhenItemMissing()
        {
            _mockRepository.GetByIdAsync(Arg.Any<Guid>(), CancellationToken.None).Returns((TodoItem?)null);

            var result = await _service.ChangeCompletionStatus(Guid.NewGuid(), CancellationToken.None);

            result.IsSuccess.Should().BeFalse();
            result.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task CreateItem_ShouldReturnSuccess_WhenDataIsValid()
        {
            var request = new ToDoAddRequest { Title = "Test" };
            var userId = Guid.NewGuid();
            _mockCurrentUserRepository.GetUserId().Returns(userId);
            _mockRepository.AddAsync(Arg.Any<TodoItem>(), CancellationToken.None).Returns(DatabaseResult.Success);

            var result = await _service.CreateItem(request, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Value.Should().NotBeNull();
            await _mockRepository.Received(1).AddAsync(Arg.Is<TodoItem>(x => x.UserId == userId), CancellationToken.None);
        }

        [Fact]
        public async Task GetItemById_ShouldReturnSuccess_WhenItemExists()
        {
            var id = Guid.NewGuid();
            _mockRepository.GetByIdAsync(id, CancellationToken.None).Returns(new TodoItem { Id = id });

            var result = await _service.GetItemById(id, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Value.Should().NotBeNull();
        }

        [Fact]
        public async Task GetItemById_ShouldReturnBadRequest_WhenIdIsNull()
        {
            var result = await _service.GetItemById(null, CancellationToken.None);

            result.IsSuccess.Should().BeFalse();
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task UpdateItem_ShouldReturnFailure_WhenItemMissing()
        {
            _mockRepository.GetByIdAsync(Arg.Any<Guid>(), CancellationToken.None).Returns((TodoItem?)null);
            var request = new ToDoUpdateRequest { Id = Guid.NewGuid() };

            var result = await _service.UpdateItem(request, CancellationToken.None);

            result.IsSuccess.Should().BeFalse();
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }
    }
}
