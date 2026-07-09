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
    public class ImprovementRepositoryTests
    {
        private readonly DbContextOptions<ApplicationDbContext> _dbOptions;

        public ImprovementRepositoryTests()
        {
            _dbOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: $"TestDb_Improvement_{Guid.NewGuid()}")
                .Options;
        }

        private Mock<ILogger<GenericRepository<Improvement>>> CreateLoggerMock()
        {
            var mock = new Mock<ILogger<GenericRepository<Improvement>>>();
            return mock;
        }

        [Fact]
        public async Task AddAsync_Should_Add_Improvement_To_Database()
        {
            // Arrange
            using var context = new ApplicationDbContext(_dbOptions);
            var loggerMock = CreateLoggerMock();
            var repo = new ImprovementRepository(context, loggerMock.Object);

            var improvement = new Improvement
            {
                Id = 0,
                Name = "Balcón",
                Description = "Balcón amplio con vista al mar"
            };

            // Act
            var result = await repo.AddAsync(improvement);

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
            var repo = new ImprovementRepository(context, loggerMock.Object);

            // Act
            Func<Task> act = async () => await repo.AddAsync(null!);

            // Assert
            await act.Should().ThrowAsync<Exception>();
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Improvement_When_Exists()
        {
            // Arrange
            using var context = new ApplicationDbContext(_dbOptions);
            var loggerMock = CreateLoggerMock();

            var improvement = new Improvement
            {
                Id = 0,
                Name = "Piscina",
                Description = "Piscina climatizada"
            };
            context.Improvements.Add(improvement);
            await context.SaveChangesAsync();

            var repo = new ImprovementRepository(context, loggerMock.Object);

            // Act
            var result = await repo.GetByIdAsync(improvement.Id);

            // Assert
            result.Should().NotBeNull();
            result!.Name.Should().Be("Piscina");
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_When_NotExists()
        {
            // Arrange
            using var context = new ApplicationDbContext(_dbOptions);
            var loggerMock = CreateLoggerMock();
            var repo = new ImprovementRepository(context, loggerMock.Object);

            // Act
            var result = await repo.GetByIdAsync(999);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task UpdateAsync_Should_Update_Improvement()
        {
            // Arrange
            using var context = new ApplicationDbContext(_dbOptions);
            var loggerMock = CreateLoggerMock();

            var improvement = new Improvement
            {
                Id = 0,
                Name = "Gimnasio",
                Description = "Gimnasio básico"
            };
            context.Improvements.Add(improvement);
            await context.SaveChangesAsync();

            var repo = new ImprovementRepository(context, loggerMock.Object);
            improvement.Description = "Gimnasio completamente equipado";

            // Act
            var updated = await repo.UpdateAsync(improvement.Id, improvement);

            // Assert
            updated.Should().NotBeNull();
            updated!.Description.Should().Be("Gimnasio completamente equipado");
        }

        [Fact]
        public async Task UpdateAsync_Should_Return_Null_When_NotExists()
        {
            // Arrange
            using var context = new ApplicationDbContext(_dbOptions);
            var loggerMock = CreateLoggerMock();
            var repo = new ImprovementRepository(context, loggerMock.Object);

            var fake = new Improvement
            {
                Id = 999,
                Name = "Fake",
                Description = "Fake description"
            };

            // Act
            var result = await repo.UpdateAsync(fake.Id, fake);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task DeleteAsync_Should_Remove_Improvement()
        {
            // Arrange
            using var context = new ApplicationDbContext(_dbOptions);
            var loggerMock = CreateLoggerMock();

            var improvement = new Improvement
            {
                Id = 0,
                Name = "Jacuzzi",
                Description = "Jacuzzi exterior"
            };
            context.Improvements.Add(improvement);
            await context.SaveChangesAsync();

            var repo = new ImprovementRepository(context, loggerMock.Object);

            // Act
            await repo.DeleteAsync(improvement.Id);
            var result = await repo.GetByIdAsync(improvement.Id);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task DeleteAsync_Should_Not_Throw_When_Id_NotExists()
        {
            // Arrange
            using var context = new ApplicationDbContext(_dbOptions);
            var loggerMock = CreateLoggerMock();
            var repo = new ImprovementRepository(context, loggerMock.Object);

            // Act
            Func<Task> act = async () => await repo.DeleteAsync(999);

            // Assert
            await act.Should().NotThrowAsync();
        }

        [Fact]
        public async Task GetAllAsync_Should_Return_All_Improvements()
        {
            // Arrange
            using var context = new ApplicationDbContext(_dbOptions);
            var loggerMock = CreateLoggerMock();

            context.Improvements.AddRange(
                new Improvement { Id = 0, Name = "Parqueo", Description = "Dos espacios de parqueo" },
                new Improvement { Id = 0, Name = "Terraza", Description = "Terraza techada" }
            );
            await context.SaveChangesAsync();

            var repo = new ImprovementRepository(context, loggerMock.Object);

            // Act
            var result = await repo.GetAllAsync();

            // Assert
            result.Should().HaveCount(2);
        }

        [Fact]
        public async Task GetAllWithIncludeAsync_Should_Return_Improvements_With_PropertyImprovements()
        {
            // Arrange
            using var context = new ApplicationDbContext(_dbOptions);
            var loggerMock = CreateLoggerMock();

            var improvement = new Improvement
            {
                Id = 0,
                Name = "Ascensor",
                Description = "Ascensor moderno"
            };
            context.Improvements.Add(improvement);
            await context.SaveChangesAsync();

            var repo = new ImprovementRepository(context, loggerMock.Object);

            // Act
            var result = await repo.GetAllWithIncludeAsync(["PropertyImprovements"]);

            // Assert
            result.Should().NotBeEmpty();
            result[0].Name.Should().Be("Ascensor");
        }

        [Fact]
        public async Task GetAllQuery_Should_Return_Queryable_Improvements()
        {
            // Arrange
            using var context = new ApplicationDbContext(_dbOptions);
            var loggerMock = CreateLoggerMock();

            context.Improvements.Add(new Improvement
            {
                Id = 0,
                Name = "A/C",
                Description = "Aire acondicionado central"
            });
            await context.SaveChangesAsync();

            var repo = new ImprovementRepository(context, loggerMock.Object);

            // Act
            var query = repo.GetAllQuery();
            var result = await query.ToListAsync();

            // Assert
            result.Should().NotBeEmpty();
        }
    }
}