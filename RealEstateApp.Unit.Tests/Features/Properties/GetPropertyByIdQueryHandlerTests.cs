using AutoMapper;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using RealEstateApp.Core.Application.Features.Properties.Queries.GetById;
using RealEstateApp.Core.Application.Mappings.EntitiesAndDtos;
using RealEstateApp.Core.Domain.Entities;
using RealEstateApp.Core.Domain.Enums;
using RealEstateApp.Infrastructure.Persistence.Contexts;
using RealEstateApp.Infrastructure.Persistence.Repositories;

namespace RealEstateApp.Unit.Tests.Features.Properties
{
    public class GetPropertyByIdQueryHandlerTests
    {
        private readonly DbContextOptions<ApplicationDbContext> _dbOptions;
        private readonly IMapper _mapper;

        public GetPropertyByIdQueryHandlerTests()
        {
            _dbOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase($"TestDb_GetPropertyById_{Guid.NewGuid()}")
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
        public async Task Handle_Should_Return_PropertyDto_With_Includes_When_Property_Exists()
        {
            // Arrange
            using var context = new ApplicationDbContext(_dbOptions);
            var loggerMoq = new Mock<ILogger<GenericRepository<Property>>>();

            var propertyType = new PropertyType { Id = 1, Name = "Casa", Description = "Vivienda independiente" };
            var saleType = new SaleType { Id = 1, Name = "Alquiler", Description = "Arrendamiento mensual" };
            context.PropertyTypes.Add(propertyType);
            context.SaleTypes.Add(saleType);

            var improvement = new Improvement { Id = 1, Name = "Piscina", Description = "Piscina temperada" };
            context.Improvements.Add(improvement);

            var property = new Property
            {
                Id = 1,
                Code = "100001",
                AgentId = "agent-1",
                PropertyTypeId = 1,
                SaleTypeId = 1,
                Price = 12000000,
                LandSize = 300,
                Bedrooms = 4,
                Bathrooms = 3,
                Description = "Casa con piscina en zona residencial",
                Status = PropertyStatus.Available,
                PropertyImprovements = new List<PropertyImprovement>
                {
                    new PropertyImprovement { ImprovementId = 1, Improvement = improvement }
                }
            };
            context.Properties.Add(property);
            await context.SaveChangesAsync();

            var repository = new PropertyRepository(context, loggerMoq.Object);
            var handler = new GetPropertyByIdQueryHandler(repository, _mapper);

            // Act
            var result = await handler.Handle(new GetPropertyByIdQuery { Id = 1 }, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result!.Id.Should().Be(1);
            result.Code.Should().Be("100001");
            result.Bedrooms.Should().Be(4);
            result.PropertyTypeName.Should().Be("Casa");
            result.SaleTypeName.Should().Be("Alquiler");
            result.Improvements.Should().Contain("Piscina");
        }

        [Fact]
        public async Task Handle_Should_Return_Null_When_Property_Does_Not_Exist()
        {
            // Arrange
            using var context = new ApplicationDbContext(_dbOptions);
            var loggerMoq = new Mock<ILogger<GenericRepository<Property>>>();
            var repository = new PropertyRepository(context, loggerMoq.Object);
            var handler = new GetPropertyByIdQueryHandler(repository, _mapper);

            // Act
            var result = await handler.Handle(new GetPropertyByIdQuery { Id = 999 }, CancellationToken.None);

            // Assert
            result.Should().BeNull();
        }
    }
}
