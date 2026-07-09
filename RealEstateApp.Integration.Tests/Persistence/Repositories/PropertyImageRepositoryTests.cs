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
    public class PropertyImageRepositoryTests
    {
        private readonly DbContextOptions<ApplicationDbContext> _dbOptions;

        public PropertyImageRepositoryTests()
        {
            _dbOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: $"TestDb_PropertyImage_{Guid.NewGuid()}")
                .Options;
        }

        private Mock<ILogger<GenericRepository<PropertyImage>>> CreateLoggerMock()
        {
            return new Mock<ILogger<GenericRepository<PropertyImage>>>();
        }

        private async Task<Property> SeedPropertyAsync(ApplicationDbContext context)
        {
            var pt = new PropertyType { Id = 0, Name = "Casa", Description = "Vivienda" };
            var st = new SaleType { Id = 0, Name = "Venta", Description = "Venta directa" };
            context.PropertyTypes.Add(pt);
            context.SaleTypes.Add(st);
            await context.SaveChangesAsync();

            var property = new Property
            {
                Id = 0,
                Code = $"I{Guid.NewGuid().ToString()[..5].ToUpper()}",
                AgentId = "agent-seed",
                PropertyTypeId = pt.Id,
                SaleTypeId = st.Id,
                Price = 2000000,
                LandSize = 120,
                Bedrooms = 3,
                Bathrooms = 2,
                Description = "Propiedad para imágenes",
                Status = PropertyStatus.Available
            };
            context.Properties.Add(property);
            await context.SaveChangesAsync();
            return property;
        }

        [Fact]
        public async Task AddAsync_Should_Add_PropertyImage_To_Database()
        {
            // Arrange
            using var context = new ApplicationDbContext(_dbOptions);
            var loggerMock = CreateLoggerMock();
            var property = await SeedPropertyAsync(context);
            var repo = new PropertyImageRepository(context, loggerMock.Object);

            var image = new PropertyImage
            {
                Id = 0,
                PropertyId = property.Id,
                ImageUrl = "/images/propiedad1.jpg"
            };

            // Act
            var result = await repo.AddAsync(image);

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
            var repo = new PropertyImageRepository(context, loggerMock.Object);

            // Act
            Func<Task> act = async () => await repo.AddAsync(null!);

            // Assert
            await act.Should().ThrowAsync<Exception>();
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_PropertyImage_When_Exists()
        {
            // Arrange
            using var context = new ApplicationDbContext(_dbOptions);
            var loggerMock = CreateLoggerMock();
            var property = await SeedPropertyAsync(context);

            var image = new PropertyImage
            {
                Id = 0,
                PropertyId = property.Id,
                ImageUrl = "/images/sala.jpg"
            };
            context.PropertyImages.Add(image);
            await context.SaveChangesAsync();

            var repo = new PropertyImageRepository(context, loggerMock.Object);

            // Act
            var result = await repo.GetByIdAsync(image.Id);

            // Assert
            result.Should().NotBeNull();
            result!.ImageUrl.Should().Be("/images/sala.jpg");
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_When_NotExists()
        {
            // Arrange
            using var context = new ApplicationDbContext(_dbOptions);
            var loggerMock = CreateLoggerMock();
            var repo = new PropertyImageRepository(context, loggerMock.Object);

            // Act
            var result = await repo.GetByIdAsync(999);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task DeleteAsync_Should_Remove_PropertyImage()
        {
            // Arrange
            using var context = new ApplicationDbContext(_dbOptions);
            var loggerMock = CreateLoggerMock();
            var property = await SeedPropertyAsync(context);

            var image = new PropertyImage
            {
                Id = 0,
                PropertyId = property.Id,
                ImageUrl = "/images/fachada.jpg"
            };
            context.PropertyImages.Add(image);
            await context.SaveChangesAsync();

            var repo = new PropertyImageRepository(context, loggerMock.Object);

            // Act
            await repo.DeleteAsync(image.Id);
            var result = await repo.GetByIdAsync(image.Id);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task DeleteAsync_Should_Not_Throw_When_Id_NotExists()
        {
            // Arrange
            using var context = new ApplicationDbContext(_dbOptions);
            var loggerMock = CreateLoggerMock();
            var repo = new PropertyImageRepository(context, loggerMock.Object);

            // Act
            Func<Task> act = async () => await repo.DeleteAsync(999);

            // Assert
            await act.Should().NotThrowAsync();
        }

        [Fact]
        public async Task GetByPropertyIdAsync_Should_Return_Images_Of_Property()
        {
            // Arrange
            using var context = new ApplicationDbContext(_dbOptions);
            var loggerMock = CreateLoggerMock();
            var property = await SeedPropertyAsync(context);

            context.PropertyImages.AddRange(
                new PropertyImage { Id = 0, PropertyId = property.Id, ImageUrl = "/images/img1.jpg" },
                new PropertyImage { Id = 0, PropertyId = property.Id, ImageUrl = "/images/img2.jpg" },
                new PropertyImage { Id = 0, PropertyId = property.Id, ImageUrl = "/images/img3.jpg" },
                new PropertyImage { Id = 0, PropertyId = 9999, ImageUrl = "/images/otro.jpg" }
            );
            await context.SaveChangesAsync();

            var repo = new PropertyImageRepository(context, loggerMock.Object);

            // Act
            var result = await repo.GetByPropertyIdAsync(property.Id);

            // Assert
            result.Should().HaveCount(3);
            result.Should().AllSatisfy(i => i.PropertyId.Should().Be(property.Id));
        }

        [Fact]
        public async Task GetAllAsync_Should_Return_All_PropertyImages()
        {
            // Arrange
            using var context = new ApplicationDbContext(_dbOptions);
            var loggerMock = CreateLoggerMock();
            var property = await SeedPropertyAsync(context);

            context.PropertyImages.AddRange(
                new PropertyImage { Id = 0, PropertyId = property.Id, ImageUrl = "/images/a.jpg" },
                new PropertyImage { Id = 0, PropertyId = property.Id, ImageUrl = "/images/b.jpg" }
            );
            await context.SaveChangesAsync();

            var repo = new PropertyImageRepository(context, loggerMock.Object);

            // Act
            var result = await repo.GetAllAsync();

            // Assert
            result.Should().HaveCount(2);
        }
    }

    public class PropertyImprovementRepositoryTests
    {
        private readonly DbContextOptions<ApplicationDbContext> _dbOptions;

        public PropertyImprovementRepositoryTests()
        {
            _dbOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: $"TestDb_PropertyImprovement_{Guid.NewGuid()}")
                .Options;
        }

        private Mock<ILogger<GenericRepository<PropertyImprovement>>> CreateLoggerMock()
        {
            return new Mock<ILogger<GenericRepository<PropertyImprovement>>>();
        }

        private async Task<(Property property, Improvement improvement)> SeedDependenciesAsync(ApplicationDbContext context)
        {
            var pt = new PropertyType { Id = 0, Name = "Casa", Description = "Vivienda" };
            var st = new SaleType { Id = 0, Name = "Venta", Description = "Venta directa" };
            context.PropertyTypes.Add(pt);
            context.SaleTypes.Add(st);
            await context.SaveChangesAsync();

            var property = new Property
            {
                Id = 0,
                Code = $"PI{Guid.NewGuid().ToString()[..4].ToUpper()}",
                AgentId = "agent-seed",
                PropertyTypeId = pt.Id,
                SaleTypeId = st.Id,
                Price = 3000000,
                LandSize = 150,
                Bedrooms = 3,
                Bathrooms = 2,
                Description = "Propiedad con mejoras",
                Status = PropertyStatus.Available
            };
            var improvement = new Improvement
            {
                Id = 0,
                Name = "Piscina",
                Description = "Piscina climatizada"
            };

            context.Properties.Add(property);
            context.Improvements.Add(improvement);
            await context.SaveChangesAsync();

            return (property, improvement);
        }

        [Fact]
        public async Task AddAsync_Should_Add_PropertyImprovement_To_Database()
        {
            // Arrange
            using var context = new ApplicationDbContext(_dbOptions);
            var loggerMock = CreateLoggerMock();
            var (property, improvement) = await SeedDependenciesAsync(context);

            var repo = new PropertyImprovementRepository(context, loggerMock.Object);

            var propImprovement = new PropertyImprovement
            {
                Id = 0,
                PropertyId = property.Id,
                ImprovementId = improvement.Id
            };

            // Act
            var result = await repo.AddAsync(propImprovement);

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
            var repo = new PropertyImprovementRepository(context, loggerMock.Object);

            // Act
            Func<Task> act = async () => await repo.AddAsync(null!);

            // Assert
            await act.Should().ThrowAsync<Exception>();
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_PropertyImprovement_When_Exists()
        {
            // Arrange
            using var context = new ApplicationDbContext(_dbOptions);
            var loggerMock = CreateLoggerMock();
            var (property, improvement) = await SeedDependenciesAsync(context);

            var propImprovement = new PropertyImprovement
            {
                Id = 0,
                PropertyId = property.Id,
                ImprovementId = improvement.Id
            };
            context.PropertyImprovements.Add(propImprovement);
            await context.SaveChangesAsync();

            var repo = new PropertyImprovementRepository(context, loggerMock.Object);

            // Act
            var result = await repo.GetByIdAsync(propImprovement.Id);

            // Assert
            result.Should().NotBeNull();
            result!.ImprovementId.Should().Be(improvement.Id);
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_When_NotExists()
        {
            // Arrange
            using var context = new ApplicationDbContext(_dbOptions);
            var loggerMock = CreateLoggerMock();
            var repo = new PropertyImprovementRepository(context, loggerMock.Object);

            // Act
            var result = await repo.GetByIdAsync(999);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task DeleteAsync_Should_Remove_PropertyImprovement()
        {
            // Arrange
            using var context = new ApplicationDbContext(_dbOptions);
            var loggerMock = CreateLoggerMock();
            var (property, improvement) = await SeedDependenciesAsync(context);

            var propImprovement = new PropertyImprovement
            {
                Id = 0,
                PropertyId = property.Id,
                ImprovementId = improvement.Id
            };
            context.PropertyImprovements.Add(propImprovement);
            await context.SaveChangesAsync();

            var repo = new PropertyImprovementRepository(context, loggerMock.Object);

            // Act
            await repo.DeleteAsync(propImprovement.Id);
            var result = await repo.GetByIdAsync(propImprovement.Id);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetByPropertyIdAsync_Should_Return_Improvements_Of_Property()
        {
            // Arrange
            using var context = new ApplicationDbContext(_dbOptions);
            var loggerMock = CreateLoggerMock();
            var (property, improvement) = await SeedDependenciesAsync(context);

            var improvement2 = new Improvement { Id = 0, Name = "Jardín", Description = "Jardín privado" };
            context.Improvements.Add(improvement2);
            await context.SaveChangesAsync();

            context.PropertyImprovements.AddRange(
                new PropertyImprovement { Id = 0, PropertyId = property.Id, ImprovementId = improvement.Id },
                new PropertyImprovement { Id = 0, PropertyId = property.Id, ImprovementId = improvement2.Id },
                new PropertyImprovement { Id = 0, PropertyId = 9999, ImprovementId = improvement.Id }
            );
            await context.SaveChangesAsync();

            var repo = new PropertyImprovementRepository(context, loggerMock.Object);

            // Act
            var result = await repo.GetByPropertyIdAsync(property.Id);

            // Assert
            result.Should().HaveCount(2);
            result.Should().AllSatisfy(pi => pi.PropertyId.Should().Be(property.Id));
        }

        [Fact]
        public async Task GetAllAsync_Should_Return_All_PropertyImprovements()
        {
            // Arrange
            using var context = new ApplicationDbContext(_dbOptions);
            var loggerMock = CreateLoggerMock();
            var (property, improvement) = await SeedDependenciesAsync(context);

            context.PropertyImprovements.AddRange(
                new PropertyImprovement { Id = 0, PropertyId = property.Id, ImprovementId = improvement.Id },
                new PropertyImprovement { Id = 0, PropertyId = property.Id, ImprovementId = improvement.Id }
            );
            await context.SaveChangesAsync();

            var repo = new PropertyImprovementRepository(context, loggerMock.Object);

            // Act
            var result = await repo.GetAllAsync();

            // Assert
            result.Should().HaveCount(2);
        }
    }
}