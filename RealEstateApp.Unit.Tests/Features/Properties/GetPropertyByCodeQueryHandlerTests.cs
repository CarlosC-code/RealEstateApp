using AutoMapper;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using RealEstateApp.Core.Application.Features.Properties.Queries.GetByCode;
using RealEstateApp.Core.Application.Mappings.EntitiesAndDtos;
using RealEstateApp.Core.Domain.Entities;
using RealEstateApp.Core.Domain.Enums;
using RealEstateApp.Infrastructure.Persistence.Contexts;
using RealEstateApp.Infrastructure.Persistence.Repositories;

namespace RealEstateApp.Unit.Tests.Features.Properties
{
    public class GetPropertyByCodeQueryHandlerTests
    {
        private readonly DbContextOptions<ApplicationDbContext> _dbOptions;
        private readonly IMapper _mapper;

        public GetPropertyByCodeQueryHandlerTests()
        {
            _dbOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase($"TestDb_GetPropertyByCode_{Guid.NewGuid()}")
                .Options;

            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<PropertyMappingProfile>();
                cfg.AddProfile<PropertyTypeMappingProfile>();
                cfg.AddProfile<SaleTypeMappingProfile>();
            });

            _mapper = config.CreateMapper();
        }

        [Fact]
        public async Task Handle_Should_Return_PropertyDto_When_Code_Exists()
        {
            // Arrange
            using var context = new ApplicationDbContext(_dbOptions);
            var loggerMoq = new Mock<ILogger<GenericRepository<Property>>>();

            var propertyType = new PropertyType { Id = 1, Name = "Apartamento", Description = "Unidad en edificio" };
            var saleType = new SaleType { Id = 1, Name = "Venta", Description = "Compra definitiva" };
            context.PropertyTypes.Add(propertyType);
            context.SaleTypes.Add(saleType);

            var property = new Property
            {
                Id = 1,
                Code = "ABC123",
                AgentId = "agent-1",
                PropertyTypeId = 1,
                SaleTypeId = 1,
                Price = 7500000,
                LandSize = 150,
                Bedrooms = 3,
                Bathrooms = 2,
                Description = "Apartamento bien ubicado",
                Status = PropertyStatus.Available
            };
            context.Properties.Add(property);
            await context.SaveChangesAsync();

            var repository = new PropertyRepository(context, loggerMoq.Object);
            var handler = new GetPropertyByCodeQueryHandler(repository, _mapper);

            // Act
            var result = await handler.Handle(new GetPropertyByCodeQuery { Code = "ABC123" }, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result!.Code.Should().Be("ABC123");
            result.Bedrooms.Should().Be(3);
            result.PropertyTypeName.Should().Be("Apartamento");
        }

        [Fact]
        public async Task Handle_Should_Return_Null_When_Code_Does_Not_Exist()
        {
            // Arrange
            using var context = new ApplicationDbContext(_dbOptions);
            var loggerMoq = new Mock<ILogger<GenericRepository<Property>>>();
            var repository = new PropertyRepository(context, loggerMoq.Object);
            var handler = new GetPropertyByCodeQueryHandler(repository, _mapper);

            // Act
            var result = await handler.Handle(new GetPropertyByCodeQuery { Code = "NOEXISTE" }, CancellationToken.None);

            // Assert
            result.Should().BeNull();
        }
    }
}