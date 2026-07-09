using AutoMapper;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using RealEstateApp.Core.Application.Features.SaleTypes.Commands.Create;
using RealEstateApp.Core.Application.Mappings.EntitiesAndDtos;
using RealEstateApp.Infrastructure.Persistence.Contexts;
using RealEstateApp.Infrastructure.Persistence.Repositories;

namespace RealEstateApp.Unit.Tests.Features.SaleTypes
{
    public class CreateSaleTypeCommandHandlerTests
    {
        private readonly DbContextOptions<ApplicationDbContext> _dbOptions;
        private readonly IMapper _mapper;

        public CreateSaleTypeCommandHandlerTests()
        {
            _dbOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase($"TestDb_CreateSaleType_{Guid.NewGuid()}")
                .Options;

            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<SaleTypeMappingProfile>();
            });

            _mapper = config.CreateMapper();
        }

        [Fact]
        public async Task Handle_Should_Return_SaleTypeDto_When_SaleType_Is_Created()
        {
            // Arrange
            using var context = new ApplicationDbContext(_dbOptions);
            var loggerMoq = new Mock<ILogger<GenericRepository<Core.Domain.Entities.SaleType>>>();
            var repository = new SaleTypeRepository(context, loggerMoq.Object);
            var handler = new CreateSaleTypeCommandHandler(repository, _mapper);

            var command = new CreateSaleTypeCommand
            {
                Name = "Venta",
                Description = "Transferencia definitiva de la propiedad"
            };

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result!.Id.Should().BeGreaterThan(0);
            result.Name.Should().Be(command.Name);
            result.Description.Should().Be(command.Description);

            var created = await context.SaleTypes.FindAsync(result.Id);
            created.Should().NotBeNull();
            created!.Name.Should().Be(command.Name);
        }

        [Fact]
        public async Task Handle_Should_Persist_SaleType_In_Database()
        {
            // Arrange
            using var context = new ApplicationDbContext(_dbOptions);
            var loggerMoq = new Mock<ILogger<GenericRepository<Core.Domain.Entities.SaleType>>>();
            var repository = new SaleTypeRepository(context, loggerMoq.Object);
            var handler = new CreateSaleTypeCommandHandler(repository, _mapper);

            var command = new CreateSaleTypeCommand
            {
                Name = "Alquiler",
                Description = "Arrendamiento temporal de la propiedad"
            };

            // Act
            await handler.Handle(command, CancellationToken.None);

            // Assert
            var count = await context.SaleTypes.CountAsync();
            count.Should().Be(1);
        }
    }
}
