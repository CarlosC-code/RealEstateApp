using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using RealEstateApp.Core.Application.Features.PropertyTypes.Commands.Delete;
using RealEstateApp.Core.Domain.Entities;
using RealEstateApp.Infrastructure.Persistence.Contexts;
using RealEstateApp.Infrastructure.Persistence.Repositories;

namespace RealEstateApp.Unit.Tests.Features.PropertyTypes
{
    public class DeletePropertyTypeCommandHandlerTests
    {
        private readonly DbContextOptions<ApplicationDbContext> _dbOptions;

        public DeletePropertyTypeCommandHandlerTests()
        {
            _dbOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase($"TestDb_DeletePropertyType_{Guid.NewGuid()}")
                .Options;
        }

        [Fact]
        public async Task Handle_Should_Delete_PropertyType_When_It_Exists()
        {
            // Arrange
            using var context = new ApplicationDbContext(_dbOptions);
            var loggerMoq = new Mock<ILogger<GenericRepository<PropertyType>>>();

            var propertyType = new PropertyType { Id = 1, Name = "Penthouse", Description = "Piso superior de lujo" };
            context.PropertyTypes.Add(propertyType);
            await context.SaveChangesAsync();

            var repository = new PropertyTypeRepository(context, loggerMoq.Object);
            var handler = new DeletePropertyTypeCommandHandler(repository);

            // Act
            await handler.Handle(new DeletePropertyTypeCommand { Id = 1 }, CancellationToken.None);

            // Assert
            var deleted = await context.PropertyTypes.FindAsync(1);
            deleted.Should().BeNull();
        }

        [Fact]
        public async Task Handle_Should_Not_Throw_When_PropertyType_Does_Not_Exist()
        {
            // Arrange
            using var context = new ApplicationDbContext(_dbOptions);
            var loggerMoq = new Mock<ILogger<GenericRepository<PropertyType>>>();
            var repository = new PropertyTypeRepository(context, loggerMoq.Object);
            var handler = new DeletePropertyTypeCommandHandler(repository);

            // Act
            var act = async () => await handler.Handle(new DeletePropertyTypeCommand { Id = 999 }, CancellationToken.None);

            // Assert
            await act.Should().NotThrowAsync();
        }
    }
}