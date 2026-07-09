using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using RealEstateApp.Core.Application.Features.SaleTypes.Commands.Delete;
using RealEstateApp.Core.Domain.Entities;
using RealEstateApp.Infrastructure.Persistence.Contexts;
using RealEstateApp.Infrastructure.Persistence.Repositories;

namespace RealEstateApp.Unit.Tests.Features.SaleTypes
{
    public class DeleteSaleTypeCommandHandlerTests
    {
        private readonly DbContextOptions<ApplicationDbContext> _dbOptions;

        public DeleteSaleTypeCommandHandlerTests()
        {
            _dbOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase($"TestDb_DeleteSaleType_{Guid.NewGuid()}")
                .Options;
        }

        [Fact]
        public async Task Handle_Should_Delete_SaleType_When_It_Exists()
        {
            // Arrange
            using var context = new ApplicationDbContext(_dbOptions);
            var loggerMoq = new Mock<ILogger<GenericRepository<SaleType>>>();

            var saleType = new SaleType { Id = 1, Name = "Alquiler Amueblado", Description = "Con mobiliario incluido" };
            context.SaleTypes.Add(saleType);
            await context.SaveChangesAsync();

            var repository = new SaleTypeRepository(context, loggerMoq.Object);
            var handler = new DeleteSaleTypeCommandHandler(repository);

            // Act
            await handler.Handle(new DeleteSaleTypeCommand { Id = 1 }, CancellationToken.None);

            // Assert
            var deleted = await context.SaleTypes.FindAsync(1);
            deleted.Should().BeNull();
        }

        [Fact]
        public async Task Handle_Should_Not_Throw_When_SaleType_Does_Not_Exist()
        {
            // Arrange
            using var context = new ApplicationDbContext(_dbOptions);
            var loggerMoq = new Mock<ILogger<GenericRepository<SaleType>>>();
            var repository = new SaleTypeRepository(context, loggerMoq.Object);
            var handler = new DeleteSaleTypeCommandHandler(repository);

            // Act
            var act = async () => await handler.Handle(new DeleteSaleTypeCommand { Id = 999 }, CancellationToken.None);

            // Assert
            await act.Should().NotThrowAsync();
        }
    }
}