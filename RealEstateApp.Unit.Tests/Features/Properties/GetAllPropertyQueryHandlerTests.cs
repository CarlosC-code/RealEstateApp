using AutoMapper;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using RealEstateApp.Core.Application.Features.Properties.Queries.GetAll;
using RealEstateApp.Core.Application.Mappings.EntitiesAndDtos;
using RealEstateApp.Core.Domain.Entities;
using RealEstateApp.Core.Domain.Enums;
using RealEstateApp.Infrastructure.Persistence.Contexts;
using RealEstateApp.Infrastructure.Persistence.Repositories;

namespace RealEstateApp.Unit.Tests.Features.Properties
{
    public class GetAllPropertyQueryHandlerTests
    {
        private readonly DbContextOptions<ApplicationDbContext> _dbOptions;
        private readonly IMapper _mapper;

        public GetAllPropertyQueryHandlerTests()
        {
            _dbOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase($"TestDb_GetAllProperty_{Guid.NewGuid()}")
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
        public async Task Handle_Should_Return_All_Properties_With_Includes()
        {
            // Arrange
            using var context = new ApplicationDbContext(_dbOptions);
            var loggerMoq = new Mock<ILogger<GenericRepository<Property>>>();

            var propertyType = new PropertyType { Id = 1, Name = "Apartamento", Description = "Unidad en edificio" };
            var saleType = new SaleType { Id = 1, Name = "Venta", Description = "Compra definitiva" };

            context.PropertyTypes.Add(propertyType);
            context.SaleTypes.Add(saleType);

            context.Properties.AddRange(
                new Property
                {
                    Id = 1,
                    Code = "100001",
                    AgentId = "agent-1",
                    PropertyTypeId = 1,
                    SaleTypeId = 1,
                    Price = 5000000,
                    LandSize = 120,
                    Bedrooms = 3,
                    Bathrooms = 2,
                    Description = "Apartamento céntrico",
                    Status = PropertyStatus.Available
                },
                new Property
                {
                    Id = 2,
                    Code = "100002",
                    AgentId = "agent-1",
                    PropertyTypeId = 1,
                    SaleTypeId = 1,
                    Price = 8000000,
                    LandSize = 200,
                    Bedrooms = 4,
                    Bathrooms = 3,
                    Description = "Apartamento de lujo",
                    Status = PropertyStatus.Available
                }
            );

            await context.SaveChangesAsync();

            var repository = new PropertyRepository(context, loggerMoq.Object);
            var handler = new GetAllPropertyQueryHandler(repository, _mapper);

            // Act
            var result = await handler.Handle(new GetAllPropertyQuery(), CancellationToken.None);

            // Assert
            result.Should().HaveCount(2);
            result.All(p => p.PropertyTypeName != null).Should().BeTrue();
            result.All(p => p.SaleTypeName != null).Should().BeTrue();
            result.First(p => p.Code == "100001").Bedrooms.Should().Be(3);
        }

        [Fact]
        public async Task Handle_Should_Return_Empty_List_When_No_Properties_Exist()
        {
            // Arrange
            using var context = new ApplicationDbContext(_dbOptions);
            var loggerMoq = new Mock<ILogger<GenericRepository<Property>>>();
            var repository = new PropertyRepository(context, loggerMoq.Object);
            var handler = new GetAllPropertyQueryHandler(repository, _mapper);

            // Act
            var result = await handler.Handle(new GetAllPropertyQuery(), CancellationToken.None);

            // Assert
            result.Should().BeEmpty();
        }
    }
}