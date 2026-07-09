using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using RealEstateApp.Core.Domain.Entities;
using RealEstateApp.Core.Domain.Enums;
using RealEstateApp.Infrastructure.Persistence.Contexts;
using RealEstateApp.Infrastructure.Persistence.Repositories;

namespace RealEstateApp.Integration.Tests.Persistence.Repositories
{
    public class FavoritePropertyRepositoryTests
    {
        private readonly DbContextOptions<ApplicationDbContext> _dbOptions;

        public FavoritePropertyRepositoryTests()
        {
            _dbOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: $"TestDb_FavoriteProperty_{Guid.NewGuid()}")
                .Options;
        }

        private Mock<ILogger<GenericRepository<FavoriteProperty>>> CreateLoggerMock()
        {
            return new Mock<ILogger<GenericRepository<FavoriteProperty>>>();
        }

        private async Task<Property> SeedPropertyAsync(ApplicationDbContext context, string code)
        {
            var pt = new PropertyType { Id = 0, Name = "Casa", Description = "Vivienda" };
            var st = new SaleType { Id = 0, Name = "Venta", Description = "Venta directa" };
            context.PropertyTypes.Add(pt);
            context.SaleTypes.Add(st);
            await context.SaveChangesAsync();

            var property = new Property
            {
                Id = 0,
                Code = code,
                AgentId = "agent-seed",
                PropertyTypeId = pt.Id,
                SaleTypeId = st.Id,
                Price = 2000000,
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

        [Fact]
        public async Task AddAsync_Should_Add_FavoriteProperty_To_Database()
        {
            // Arrange
            using var context = new ApplicationDbContext(_dbOptions);
            var loggerMock = CreateLoggerMock();
            var property = await SeedPropertyAsync(context, "FAV001");

            var repo = new FavoritePropertyRepository(context, loggerMock.Object);

            var favorite = new FavoriteProperty
            {
                Id = 0,
                PropertyId = property.Id,
                ClientId = "client-1"
            };

            // Act
            var result = await repo.AddAsync(favorite);

            // Assert
            result.Should().NotBeNull();
            result!.Id.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task AddAsync_Should_Throw_When_Null()
        {
            // Arrange
            using var context = new ApplicationDbContext(_dbOptions);
            var loggerMock = CreateLoggerMock();
            var repo = new FavoritePropertyRepository(context, loggerMock.Object);

            // Act
            Func<Task> act = async () => await repo.AddAsync(null!);

            // Assert
            await act.Should().ThrowAsync<Exception>();
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_FavoriteProperty_When_Exists()
        {
            // Arrange
            using var context = new ApplicationDbContext(_dbOptions);
            var loggerMock = CreateLoggerMock();
            var property = await SeedPropertyAsync(context, "FAV002");

            var favorite = new FavoriteProperty
            {
                Id = 0,
                PropertyId = property.Id,
                ClientId = "client-2"
            };
            context.FavoriteProperties.Add(favorite);
            await context.SaveChangesAsync();

            var repo = new FavoritePropertyRepository(context, loggerMock.Object);

            // Act
            var result = await repo.GetByIdAsync(favorite.Id);

            // Assert
            result.Should().NotBeNull();
            result!.ClientId.Should().Be("client-2");
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_When_NotExists()
        {
            // Arrange
            using var context = new ApplicationDbContext(_dbOptions);
            var loggerMock = CreateLoggerMock();
            var repo = new FavoritePropertyRepository(context, loggerMock.Object);

            // Act
            var result = await repo.GetByIdAsync(999);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task DeleteAsync_Should_Remove_FavoriteProperty()
        {
            // Arrange
            using var context = new ApplicationDbContext(_dbOptions);
            var loggerMock = CreateLoggerMock();
            var property = await SeedPropertyAsync(context, "FAV003");

            var favorite = new FavoriteProperty
            {
                Id = 0,
                PropertyId = property.Id,
                ClientId = "client-3"
            };
            context.FavoriteProperties.Add(favorite);
            await context.SaveChangesAsync();

            var repo = new FavoritePropertyRepository(context, loggerMock.Object);

            // Act
            await repo.DeleteAsync(favorite.Id);
            var result = await repo.GetByIdAsync(favorite.Id);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task DeleteAsync_Should_Not_Throw_When_Id_NotExists()
        {
            // Arrange
            using var context = new ApplicationDbContext(_dbOptions);
            var loggerMock = CreateLoggerMock();
            var repo = new FavoritePropertyRepository(context, loggerMock.Object);

            // Act
            Func<Task> act = async () => await repo.DeleteAsync(999);

            // Assert
            await act.Should().NotThrowAsync();
        }

        [Fact]
        public async Task GetByClientIdAsync_Should_Return_Favorites_Of_Client()
        {
            // Arrange
            using var context = new ApplicationDbContext(_dbOptions);
            var loggerMock = CreateLoggerMock();
            var property1 = await SeedPropertyAsync(context, "FAV004");
            var property2 = await SeedPropertyAsync(context, "FAV005");
            var property3 = await SeedPropertyAsync(context, "FAV006");

            var clientId = "client-favoritos";

            context.FavoriteProperties.AddRange(
                new FavoriteProperty { Id = 0, PropertyId = property1.Id, ClientId = clientId },
                new FavoriteProperty { Id = 0, PropertyId = property2.Id, ClientId = clientId },
                new FavoriteProperty { Id = 0, PropertyId = property3.Id, ClientId = "otro-cliente" }
            );
            await context.SaveChangesAsync();

            var repo = new FavoritePropertyRepository(context, loggerMock.Object);

            // Act
            var result = await repo.GetByClientIdAsync(clientId);

            // Assert
            result.Should().HaveCount(2);
            result.Should().AllSatisfy(f => f.ClientId.Should().Be(clientId));
        }

        [Fact]
        public async Task GetByClientIdAsync_Should_Include_Property_Navigation()
        {
            // Arrange
            using var context = new ApplicationDbContext(_dbOptions);
            var loggerMock = CreateLoggerMock();
            var property = await SeedPropertyAsync(context, "FAV007");

            var clientId = "client-include";

            context.FavoriteProperties.Add(new FavoriteProperty
            {
                Id = 0,
                PropertyId = property.Id,
                ClientId = clientId
            });
            await context.SaveChangesAsync();

            var repo = new FavoritePropertyRepository(context, loggerMock.Object);

            // Act
            var result = await repo.GetByClientIdAsync(clientId);

            // Assert
            result.Should().HaveCount(1);
            result[0].Property.Should().NotBeNull();
            result[0].Property!.Code.Should().Be("FAV007");
        }

        [Fact]
        public async Task GetByClientIdAsync_Should_Return_Empty_When_No_Favorites()
        {
            // Arrange
            using var context = new ApplicationDbContext(_dbOptions);
            var loggerMock = CreateLoggerMock();
            var repo = new FavoritePropertyRepository(context, loggerMock.Object);

            // Act
            var result = await repo.GetByClientIdAsync("client-sin-favoritos");

            // Assert
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetAllAsync_Should_Return_All_FavoriteProperties()
        {
            // Arrange
            using var context = new ApplicationDbContext(_dbOptions);
            var loggerMock = CreateLoggerMock();
            var property = await SeedPropertyAsync(context, "FAV008");

            context.FavoriteProperties.AddRange(
                new FavoriteProperty { Id = 0, PropertyId = property.Id, ClientId = "c1" },
                new FavoriteProperty { Id = 0, PropertyId = property.Id, ClientId = "c2" }
            );
            await context.SaveChangesAsync();

            var repo = new FavoritePropertyRepository(context, loggerMock.Object);

            // Act
            var result = await repo.GetAllAsync();

            // Assert
            result.Should().HaveCount(2);
        }
    }
}
