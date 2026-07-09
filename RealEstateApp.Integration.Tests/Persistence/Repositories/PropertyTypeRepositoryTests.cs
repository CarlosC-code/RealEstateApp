using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using RealEstateApp.Core.Domain.Entities;
using RealEstateApp.Infrastructure.Persistence.Contexts;
using RealEstateApp.Infrastructure.Persistence.Repositories;

namespace RealEstateApp.Integration.Tests.Persistence.Repositories
{
    public class PropertyTypeRepositoryTests
    {
        private readonly DbContextOptions<ApplicationDbContext> _dbOptions;

        public PropertyTypeRepositoryTests()
        {
            _dbOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: $"TestDb_PropertyType_{Guid.NewGuid()}")
                .Options;
        }

        private Mock<ILogger<GenericRepository<PropertyType>>> CreateLoggerMock()
        {
            return new Mock<ILogger<GenericRepository<PropertyType>>>();
        }

        [Fact]
        public async Task AddAsync_Should_Add_PropertyType_To_Database()
        {
            // Arrange
            using var context = new ApplicationDbContext(_dbOptions);
            var loggerMock = CreateLoggerMock();
            var repo = new PropertyTypeRepository(context, loggerMock.Object);

            var propertyType = new PropertyType
            {
                Id = 0,
                Name = "Apartamento",
                Description = "Unidad de vivienda en edificio"
            };

            // Act
            var result = await repo.AddAsync(propertyType);

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
            var repo = new PropertyTypeRepository(context, loggerMock.Object);

            // Act
            Func<Task> act = async () => await repo.AddAsync(null!);

            // Assert
            await act.Should().ThrowAsync<Exception>();
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_PropertyType_When_Exists()
        {
            // Arrange
            using var context = new ApplicationDbContext(_dbOptions);
            var loggerMock = CreateLoggerMock();

            var propertyType = new PropertyType
            {
                Id = 0,
                Name = "Casa",
                Description = "Vivienda unifamiliar"
            };
            context.PropertyTypes.Add(propertyType);
            await context.SaveChangesAsync();

            var repo = new PropertyTypeRepository(context, loggerMock.Object);

            // Act
            var result = await repo.GetByIdAsync(propertyType.Id);

            // Assert
            result.Should().NotBeNull();
            result!.Name.Should().Be("Casa");
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_When_NotExists()
        {
            // Arrange
            using var context = new ApplicationDbContext(_dbOptions);
            var loggerMock = CreateLoggerMock();
            var repo = new PropertyTypeRepository(context, loggerMock.Object);

            // Act
            var result = await repo.GetByIdAsync(999);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task UpdateAsync_Should_Update_PropertyType()
        {
            // Arrange
            using var context = new ApplicationDbContext(_dbOptions);
            var loggerMock = CreateLoggerMock();

            var propertyType = new PropertyType
            {
                Id = 0,
                Name = "Villa",
                Description = "Descripción original"
            };
            context.PropertyTypes.Add(propertyType);
            await context.SaveChangesAsync();

            var repo = new PropertyTypeRepository(context, loggerMock.Object);
            propertyType.Description = "Residencia de lujo con jardín privado";

            // Act
            var updated = await repo.UpdateAsync(propertyType.Id, propertyType);

            // Assert
            updated.Should().NotBeNull();
            updated!.Description.Should().Be("Residencia de lujo con jardín privado");
        }

        [Fact]
        public async Task UpdateAsync_Should_Return_Null_When_NotExists()
        {
            // Arrange
            using var context = new ApplicationDbContext(_dbOptions);
            var loggerMock = CreateLoggerMock();
            var repo = new PropertyTypeRepository(context, loggerMock.Object);

            var fake = new PropertyType
            {
                Id = 999,
                Name = "Fake",
                Description = "Fake"
            };

            // Act
            var result = await repo.UpdateAsync(fake.Id, fake);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task DeleteAsync_Should_Remove_PropertyType()
        {
            // Arrange
            using var context = new ApplicationDbContext(_dbOptions);
            var loggerMock = CreateLoggerMock();

            var propertyType = new PropertyType
            {
                Id = 0,
                Name = "Penthouse",
                Description = "Unidad en último piso"
            };
            context.PropertyTypes.Add(propertyType);
            await context.SaveChangesAsync();

            var repo = new PropertyTypeRepository(context, loggerMock.Object);

            // Act
            await repo.DeleteAsync(propertyType.Id);
            var result = await repo.GetByIdAsync(propertyType.Id);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task DeleteAsync_Should_Not_Throw_When_Id_NotExists()
        {
            // Arrange
            using var context = new ApplicationDbContext(_dbOptions);
            var loggerMock = CreateLoggerMock();
            var repo = new PropertyTypeRepository(context, loggerMock.Object);

            // Act
            Func<Task> act = async () => await repo.DeleteAsync(999);

            // Assert
            await act.Should().NotThrowAsync();
        }

        [Fact]
        public async Task GetAllAsync_Should_Return_All_PropertyTypes()
        {
            // Arrange
            using var context = new ApplicationDbContext(_dbOptions);
            var loggerMock = CreateLoggerMock();

            context.PropertyTypes.AddRange(
                new PropertyType { Id = 0, Name = "Apartamento", Description = "Descripción A" },
                new PropertyType { Id = 0, Name = "Casa", Description = "Descripción B" },
                new PropertyType { Id = 0, Name = "Local", Description = "Descripción C" }
            );
            await context.SaveChangesAsync();

            var repo = new PropertyTypeRepository(context, loggerMock.Object);

            // Act
            var result = await repo.GetAllAsync();

            // Assert
            result.Should().HaveCount(3);
        }

        [Fact]
        public async Task GetAllWithIncludeAsync_Should_Include_Properties()
        {
            // Arrange
            using var context = new ApplicationDbContext(_dbOptions);
            var loggerMock = CreateLoggerMock();

            var propertyType = new PropertyType
            {
                Id = 0,
                Name = "Estudio",
                Description = "Apartamento de un ambiente"
            };
            context.PropertyTypes.Add(propertyType);
            await context.SaveChangesAsync();

            var repo = new PropertyTypeRepository(context, loggerMock.Object);

            // Act
            var result = await repo.GetAllWithIncludeAsync(["Properties"]);

            // Assert
            result.Should().NotBeEmpty();
            result[0].Name.Should().Be("Estudio");
        }

        [Fact]
        public async Task GetAllQuery_Should_Return_Queryable_PropertyTypes()
        {
            // Arrange
            using var context = new ApplicationDbContext(_dbOptions);
            var loggerMock = CreateLoggerMock();

            context.PropertyTypes.Add(new PropertyType
            {
                Id = 0,
                Name = "Townhouse",
                Description = "Vivienda adosada"
            });
            await context.SaveChangesAsync();

            var repo = new PropertyTypeRepository(context, loggerMock.Object);

            // Act
            var query = repo.GetAllQuery();
            var result = await query.ToListAsync();

            // Assert
            result.Should().NotBeEmpty();
        }
    }
}