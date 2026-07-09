using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using RealEstateApp.Core.Application.Features.Improvements.Commands.Delete;
using RealEstateApp.Core.Domain.Entities;
using RealEstateApp.Infrastructure.Persistence.Contexts;
using RealEstateApp.Infrastructure.Persistence.Repositories;

namespace RealEstateApp.Unit.Tests.Features.Improvements
{
    public class DeleteImprovementCommandHandlerTests
    {
        private readonly DbContextOptions<ApplicationDbContext> _dbOptions;

        public DeleteImprovementCommandHandlerTests()
        {
            _dbOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase($"TestDb_DeleteImprovement_{Guid.NewGuid()}")
                .Options;
        }

        [Fact]
        public async Task Handle_Should_Delete_Improvement_When_It_Exists()
        {
            // Arrange
            using var context = new ApplicationDbContext(_dbOptions);
            var loggerMoq = new Mock<ILogger<GenericRepository<Improvement>>>();

            var improvement = new Improvement { Id = 1, Name = "Gimnasio", Description = "Gimnasio equipado" };
            context.Improvements.Add(improvement);
            await context.SaveChangesAsync();

            var repository = new ImprovementRepository(context, loggerMoq.Object);
            var handler = new DeleteImprovementCommandHandler(repository);

            var command = new DeleteImprovementCommand { Id = 1 };

            // Act
            await handler.Handle(command, CancellationToken.None);

            // Assert
            var deleted = await context.Improvements.FindAsync(1);
            deleted.Should().BeNull();
        }

        [Fact]
        public async Task Handle_Should_Not_Throw_When_Improvement_Does_Not_Exist()
        {
            // Arrange
            using var context = new ApplicationDbContext(_dbOptions);
            var loggerMoq = new Mock<ILogger<GenericRepository<Improvement>>>();
            var repository = new ImprovementRepository(context, loggerMoq.Object);
            var handler = new DeleteImprovementCommandHandler(repository);

            var command = new DeleteImprovementCommand { Id = 999 };

            // Act
            var act = async () => await handler.Handle(command, CancellationToken.None);

            // Assert
            await act.Should().NotThrowAsync();
        }
    }
}