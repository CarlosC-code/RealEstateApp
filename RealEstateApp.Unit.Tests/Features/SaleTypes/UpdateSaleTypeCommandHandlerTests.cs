using AutoMapper;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using RealEstateApp.Core.Application.Features.SaleTypes.Commands.Update;
using RealEstateApp.Core.Application.Mappings.EntitiesAndDtos;
using RealEstateApp.Core.Domain.Entities;
using RealEstateApp.Infrastructure.Persistence.Contexts;
using RealEstateApp.Infrastructure.Persistence.Repositories;

namespace RealEstateApp.Unit.Tests.Features.SaleTypes
{
    public class UpdateSaleTypeCommandHandlerTests
    {
        private readonly DbContextOptions<ApplicationDbContext> _dbOptions;
        private readonly IMapper _mapper;

        public UpdateSaleTypeCommandHandlerTests()
        {
            _dbOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase($"TestDb_UpdateSaleType_{Guid.NewGuid()}")
                .Options;

            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<SaleTypeMappingProfile>();
            });

            _mapper = config.CreateMapper();
        }

        [Fact]
        public async Task Handle_Should_Return_Updated_SaleTypeDto_When_SaleType_Exists()
        {
            // Arrange
            using var context = new ApplicationDbContext(_dbOptions);
            var loggerMoq = new Mock<ILogger<GenericRepository<SaleType>>>();

            var saleType = new SaleType { Id = 1, Name = "Venta", Description = "Descripción original" };
            context.SaleTypes.Add(saleType);
            await context.SaveChangesAsync();

            var repository = new SaleTypeRepository(context, loggerMoq.Object);
            var handler = new UpdateSaleTypeCommandHandler(repository, _mapper);

            var command = new UpdateSaleTypeCommand
            {
                Id = 1,
                Name = "Venta Exclusiva",
                Description = "Descripción actualizada"
            };

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result!.Name.Should().Be(command.Name);
            result.Description.Should().Be(command.Description);

            var updated = await context.SaleTypes.FindAsync(1);
            updated!.Name.Should().Be(command.Name);
        }

        [Fact]
        public async Task Handle_Should_Return_Null_When_SaleType_Does_Not_Exist()
        {
            // Arrange
            using var context = new ApplicationDbContext(_dbOptions);
            var loggerMoq = new Mock<ILogger<GenericRepository<SaleType>>>();
            var repository = new SaleTypeRepository(context, loggerMoq.Object);
            var handler = new UpdateSaleTypeCommandHandler(repository, _mapper);

            var command = new UpdateSaleTypeCommand
            {
                Id = 999,
                Name = "No existe",
                Description = "Descripción"
            };

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().BeNull();
        }
    }
}
