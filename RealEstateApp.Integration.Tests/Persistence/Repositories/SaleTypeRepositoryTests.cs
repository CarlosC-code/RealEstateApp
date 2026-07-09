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
    public class SaleTypeRepositoryTests
    {
        private readonly DbContextOptions<ApplicationDbContext> _dbOptions;

        public SaleTypeRepositoryTests()
        {
            _dbOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: $"TestDb_SaleType_{Guid.NewGuid()}")
                .Options;
        }

        private Mock<ILogger<GenericRepository<SaleType>>> CreateLoggerMock()
        {
            return new Mock<ILogger<GenericRepository<SaleType>>>();
        }

        [Fact]
        public async Task AddAsync_Should_Add_SaleType_To_Database()
        {
            // Arrange
            using var context = new ApplicationDbContext(_dbOptions);
            var loggerMock = CreateLoggerMock();
            var repo = new SaleTypeRepository(context, loggerMock.Object);

            var saleType = new SaleType
            {
                Id = 0,
                Name = "Venta",
                Description = "Venta directa de la propiedad"
            };

            // Act
            var result = await repo.AddAsync(saleType);

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
            var repo = new SaleTypeRepository(context, loggerMock.Object);

            // Act
            Func<Task> act = async () => await repo.AddAsync(null!);

            // Assert
            await act.Should().ThrowAsync<Exception>();
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_SaleType_When_Exists()
        {
            // Arrange
            using var context = new ApplicationDbContext(_dbOptions);
            var loggerMock = CreateLoggerMock();

            var saleType = new SaleType
            {
                Id = 0,
                Name = "Alquiler",
                Description = "Arrendamiento mensual"
            };
            context.SaleTypes.Add(saleType);
            await context.SaveChangesAsync();

            var repo = new SaleTypeRepository(context, loggerMock.Object);

            // Act
            var result = await repo.GetByIdAsync(saleType.Id);

            // Assert
            result.Should().NotBeNull();
            result!.Name.Should().Be("Alquiler");
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_When_NotExists()
        {
            // Arrange
            using var context = new ApplicationDbContext(_dbOptions);
            var loggerMock = CreateLoggerMock();
            var repo = new SaleTypeRepository(context, loggerMock.Object);

            // Act
            var result = await repo.GetByIdAsync(999);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task UpdateAsync_Should_Update_SaleType()
        {
            // Arrange
            using var context = new ApplicationDbContext(_dbOptions);
            var loggerMock = CreateLoggerMock();

            var saleType = new SaleType
            {
                Id = 0,
                Name = "Alquiler Amueblado",
                Description = "Descripción original"
            };
            context.SaleTypes.Add(saleType);
            await context.SaveChangesAsync();

            var repo = new SaleTypeRepository(context, loggerMock.Object);
            saleType.Description = "Propiedad totalmente amueblada en alquiler";

            // Act
            var updated = await repo.UpdateAsync(saleType.Id, saleType);

            // Assert
            updated.Should().NotBeNull();
            updated!.Description.Should().Be("Propiedad totalmente amueblada en alquiler");
        }

        [Fact]
        public async Task UpdateAsync_Should_Return_Null_When_NotExists()
        {
            // Arrange
            using var context = new ApplicationDbContext(_dbOptions);
            var loggerMock = CreateLoggerMock();
            var repo = new SaleTypeRepository(context, loggerMock.Object);

            var fake = new SaleType
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
        public async Task DeleteAsync_Should_Remove_SaleType()
        {
            // Arrange
            using var context = new ApplicationDbContext(_dbOptions);
            var loggerMock = CreateLoggerMock();

            var saleType = new SaleType
            {
                Id = 0,
                Name = "Venta con Financiamiento",
                Description = "Venta con plan de pago"
            };
            context.SaleTypes.Add(saleType);
            await context.SaveChangesAsync();

            var repo = new SaleTypeRepository(context, loggerMock.Object);

            // Act
            await repo.DeleteAsync(saleType.Id);
            var result = await repo.GetByIdAsync(saleType.Id);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task DeleteAsync_Should_Not_Throw_When_Id_NotExists()
        {
            // Arrange
            using var context = new ApplicationDbContext(_dbOptions);
            var loggerMock = CreateLoggerMock();
            var repo = new SaleTypeRepository(context, loggerMock.Object);

            // Act
            Func<Task> act = async () => await repo.DeleteAsync(999);

            // Assert
            await act.Should().NotThrowAsync();
        }

        [Fact]
        public async Task GetAllAsync_Should_Return_All_SaleTypes()
        {
            // Arrange
            using var context = new ApplicationDbContext(_dbOptions);
            var loggerMock = CreateLoggerMock();

            context.SaleTypes.AddRange(
                new SaleType { Id = 0, Name = "Venta", Description = "Descripción A" },
                new SaleType { Id = 0, Name = "Alquiler", Description = "Descripción B" }
            );
            await context.SaveChangesAsync();

            var repo = new SaleTypeRepository(context, loggerMock.Object);

            // Act
            var result = await repo.GetAllAsync();

            // Assert
            result.Should().HaveCount(2);
        }

        [Fact]
        public async Task GetAllWithIncludeAsync_Should_Include_Properties()
        {
            // Arrange
            using var context = new ApplicationDbContext(_dbOptions);
            var loggerMock = CreateLoggerMock();

            var saleType = new SaleType
            {
                Id = 0,
                Name = "Permuta",
                Description = "Intercambio de propiedades"
            };
            context.SaleTypes.Add(saleType);
            await context.SaveChangesAsync();

            var repo = new SaleTypeRepository(context, loggerMock.Object);

            // Act
            var result = await repo.GetAllWithIncludeAsync(["Properties"]);

            // Assert
            result.Should().NotBeEmpty();
            result[0].Name.Should().Be("Permuta");
        }

        [Fact]
        public async Task GetAllQuery_Should_Return_Queryable_SaleTypes()
        {
            // Arrange
            using var context = new ApplicationDbContext(_dbOptions);
            var loggerMock = CreateLoggerMock();

            context.SaleTypes.Add(new SaleType
            {
                Id = 0,
                Name = "Alquiler con Opción a Compra",
                Description = "Renta con derecho a adquirir"
            });
            await context.SaveChangesAsync();

            var repo = new SaleTypeRepository(context, loggerMock.Object);

            // Act
            var query = repo.GetAllQuery();
            var result = await query.ToListAsync();

            // Assert
            result.Should().NotBeEmpty();
        }
    }
}