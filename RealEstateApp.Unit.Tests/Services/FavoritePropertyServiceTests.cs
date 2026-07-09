using AutoMapper;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using RealEstateApp.Core.Application.Dtos.FavoriteProperty;
using RealEstateApp.Core.Application.Mappings.EntitiesAndDtos;
using RealEstateApp.Core.Application.Services;
using RealEstateApp.Core.Domain.Entities;
using RealEstateApp.Core.Domain.Enums;
using RealEstateApp.Infrastructure.Persistence.Contexts;
using RealEstateApp.Infrastructure.Persistence.Repositories;

namespace RealEstateApp.Unit.Tests.Services
{
    public class FavoritePropertyServiceTests
    {
        private readonly DbContextOptions<ApplicationDbContext> _dbOptions;
        private readonly IMapper _mapper;

        public FavoritePropertyServiceTests()
        {
            _dbOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase($"TestDb_FavoritePropertyService_{Guid.NewGuid()}")
                .Options;

            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<FavoritePropertyMappingProfile>();
                cfg.AddProfile<PropertyMappingProfile>();
            });

            _mapper = config.CreateMapper();
        }

        private async Task<Property> SeedPropertyAsync(ApplicationDbContext context, string code)
        {
            var pt = new PropertyType { Id = 0, Name = "Casa", Description = "Vivienda" };
            var st = new SaleType { Id = 0, Name = "Venta", Description = "Directa" };
            context.PropertyTypes.Add(pt);
            context.SaleTypes.Add(st);
            await context.SaveChangesAsync();

            var property = new Property
            {
                Id = 0,
                Code = code,
                AgentId = "agent-1",
                PropertyTypeId = pt.Id,
                SaleTypeId = st.Id,
                Price = 3000000,
                LandSize = 100,
                Bedrooms = 2,
                Bathrooms = 1,
                Description = "Propiedad semilla",
                Status = PropertyStatus.Available
            };
            context.Properties.Add(property);
            await context.SaveChangesAsync();
            return property;
        }

        private FavoritePropertyService CreateService()
        {
            var factoryMoq = new Mock<ILoggerFactory>();
            factoryMoq.Setup(x => x.CreateLogger(It.IsAny<string>()))
                .Returns(new NullLogger<FavoritePropertyService>());

            var context = new ApplicationDbContext(_dbOptions);
            var repo = new FavoritePropertyRepository(context, new NullLogger<GenericRepository<FavoriteProperty>>());
            return new FavoritePropertyService(repo, _mapper, factoryMoq.Object);
        }

        [Fact]
        public async Task AddAsync_Should_Add_Property_To_Favorites()
        {
            // Arrange
            using var context = new ApplicationDbContext(_dbOptions);
            var property = await SeedPropertyAsync(context, "FAV001");
            var service = CreateService();

            var dto = new FavoritePropertyDto
            {
                Id = 0,
                PropertyId = property.Id,
                ClientId = "client-1"
            };

            // Act
            var result = await service.AddAsync(dto);

            // Assert
            result.Should().NotBeNull();
            result!.PropertyId.Should().Be(property.Id);
            result.ClientId.Should().Be("client-1");
        }

        [Fact]
        public async Task AddAsync_Should_Return_Null_When_Property_Already_In_Favorites()
        {
            // Arrange
            using var context = new ApplicationDbContext(_dbOptions);
            var property = await SeedPropertyAsync(context, "FAV002");

            context.FavoriteProperties.Add(new FavoriteProperty
            {
                Id = 0,
                PropertyId = property.Id,
                ClientId = "client-dup"
            });
            await context.SaveChangesAsync();

            var service = CreateService();

            var dto = new FavoritePropertyDto
            {
                Id = 0,
                PropertyId = property.Id,
                ClientId = "client-dup"
            };

            // Act
            var result = await service.AddAsync(dto);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetByClientIdAsync_Should_Return_Favorites_Of_Client()
        {
            // Arrange
            using var context = new ApplicationDbContext(_dbOptions);
            var property1 = await SeedPropertyAsync(context, "FAV003");
            var property2 = await SeedPropertyAsync(context, "FAV004");
            var property3 = await SeedPropertyAsync(context, "FAV005");

            var clientId = "client-fav";

            context.FavoriteProperties.AddRange(
                new FavoriteProperty { Id = 0, PropertyId = property1.Id, ClientId = clientId },
                new FavoriteProperty { Id = 0, PropertyId = property2.Id, ClientId = clientId },
                new FavoriteProperty { Id = 0, PropertyId = property3.Id, ClientId = "otro-cliente" }
            );
            await context.SaveChangesAsync();

            var service = CreateService();

            // Act
            var result = await service.GetByClientIdAsync(clientId);

            // Assert
            result.Should().HaveCount(2);
            result.Should().AllSatisfy(f => f.ClientId.Should().Be(clientId));
        }

        [Fact]
        public async Task GetByClientIdAsync_Should_Return_Property_Info()
        {
            // Arrange
            using var context = new ApplicationDbContext(_dbOptions);
            var property = await SeedPropertyAsync(context, "FAV006");

            var clientId = "client-info";
            context.FavoriteProperties.Add(new FavoriteProperty
            {
                Id = 0,
                PropertyId = property.Id,
                ClientId = clientId
            });
            await context.SaveChangesAsync();

            var service = CreateService();

            // Act
            var result = await service.GetByClientIdAsync(clientId);

            // Assert
            result.Should().HaveCount(1);
            result[0].PropertyCode.Should().Be("FAV006");
            result[0].Price.Should().Be(3000000);
        }

        [Fact]
        public async Task GetByClientIdAsync_Should_Return_Empty_When_No_Favorites()
        {
            // Arrange
            var service = CreateService();

            // Act
            var result = await service.GetByClientIdAsync("client-sin-favoritos");

            // Assert
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task DeleteAsync_Should_Remove_Favorite()
        {
            // Arrange
            using var context = new ApplicationDbContext(_dbOptions);
            var property = await SeedPropertyAsync(context, "FAV007");

            var favorite = new FavoriteProperty
            {
                Id = 0,
                PropertyId = property.Id,
                ClientId = "client-del"
            };
            context.FavoriteProperties.Add(favorite);
            await context.SaveChangesAsync();

            var service = CreateService();

            // Act
            var result = await service.DeleteAsync(favorite.Id);

            // Assert
            result.Should().BeTrue();
            var deleted = await service.GetById(favorite.Id);
            deleted.Should().BeNull();
        }

        [Fact]
        public async Task GetAll_Should_Return_All_FavoriteProperties()
        {
            // Arrange
            using var context = new ApplicationDbContext(_dbOptions);
            var property = await SeedPropertyAsync(context, "FAV008");

            context.FavoriteProperties.AddRange(
                new FavoriteProperty { Id = 0, PropertyId = property.Id, ClientId = "c1" },
                new FavoriteProperty { Id = 0, PropertyId = property.Id, ClientId = "c2" }
            );
            await context.SaveChangesAsync();

            var service = CreateService();

            // Act
            var result = await service.GetAll();

            // Assert
            result.Should().HaveCount(2);
        }
    }
}