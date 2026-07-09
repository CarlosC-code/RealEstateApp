using AutoMapper;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using RealEstateApp.Core.Application.Features.SaleTypes.Queries.GetById;
using RealEstateApp.Core.Application.Mappings.EntitiesAndDtos;
using RealEstateApp.Core.Domain.Entities;
using RealEstateApp.Infrastructure.Persistence.Contexts;
using RealEstateApp.Infrastructure.Persistence.Repositories;

namespace RealEstateApp.Unit.Tests.Features.SaleTypes
{
    public class GetSaleTypeByIdQueryHandlerTests
    {
        private readonly DbContextOptions<ApplicationDbContext> _dbOptions;
        private readonly IMapper _mapper;

        public GetSaleTypeByIdQueryHandlerTests()
        {
            _dbOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase($"TestDb_GetSaleTypeById_{Guid.NewGuid()}")
                .Options;

            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<SaleTypeMappingProfile>();
            });

            _mapper = config.CreateMapper();
        }

        [Fact]
        public async Task Handle_Should_Return_SaleTypeDto_When_SaleType_Exists()
        {
            // Arrange
            using var context = new ApplicationDbContext(_dbOptions);
            var loggerMoq = new Mock<ILogger<GenericRepository<SaleType>>>();

            var saleType = new SaleType { Id = 1, Name = "Venta", Description = "Transferencia definitiva" };
            context.SaleTypes.Add(saleType);
            await context.SaveChangesAsync();

            var repository = new SaleTypeRepository(context, loggerMoq.Object);
            var handler = new GetSaleTypeByIdQueryHandler(repository, _mapper);

            // Act
            var result = await handler.Handle(new GetSaleTypeByIdQuery { Id = 1 }, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result!.Id.Should().Be(1);
            result.Name.Should().Be("Venta");
            result.Description.Should().Be("Transferencia definitiva");
        }

        [Fact]
        public async Task Handle_Should_Return_Null_When_SaleType_Does_Not_Exist()
        {
            // Arrange
            using var context = new ApplicationDbContext(_dbOptions);
            var loggerMoq = new Mock<ILogger<GenericRepository<SaleType>>>();
            var repository = new SaleTypeRepository(context, loggerMoq.Object);
            var handler = new GetSaleTypeByIdQueryHandler(repository, _mapper);

            // Act
            var result = await handler.Handle(new GetSaleTypeByIdQuery { Id = 999 }, CancellationToken.None);

            // Assert
            result.Should().BeNull();
        }
    }
}