using AutoMapper;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using RealEstateApp.Core.Application.Dtos.Improvement;
using RealEstateApp.Core.Application.Mappings.EntitiesAndDtos;
using RealEstateApp.Core.Application.Services;
using RealEstateApp.Core.Domain.Entities;
using RealEstateApp.Infrastructure.Persistence.Contexts;
using RealEstateApp.Infrastructure.Persistence.Repositories;

namespace RealEstateApp.Unit.Tests.Services
{
    public class ImprovementServiceTests
    {
        private readonly DbContextOptions<ApplicationDbContext> _dbOptions;
        private readonly IMapper _mapper;

        public ImprovementServiceTests()
        {
            _dbOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase($"TestDb_ImprovementService_{Guid.NewGuid()}")
                .Options;

            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<ImprovementMappingProfile>();
            });

            _mapper = config.CreateMapper();
        }

        private ImprovementService CreateService()
        {
            var factoryMoq = new Mock<ILoggerFactory>();
            factoryMoq.Setup(x => x.CreateLogger(It.IsAny<string>()))
                .Returns(new NullLogger<ImprovementService>());

            var context = new ApplicationDbContext(_dbOptions);
            var repo = new ImprovementRepository(context, new NullLogger<GenericRepository<Improvement>>());
            return new ImprovementService(repo, _mapper, factoryMoq.Object);
        }

        [Fact]
        public async Task AddAsync_Should_Add_Improvement()
        {
            // Arrange
            var service = CreateService();
            var dto = new ImprovementDto
            {
                Id = 0,
                Name = "Piscina",
                Description = "Piscina climatizada"
            };

            // Act
            var result = await service.AddAsync(dto);

            // Assert
            result.Should().NotBeNull();
            result!.Name.Should().Be("Piscina");
            result.Id.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task GetById_Should_Return_Improvement_When_Exists()
        {
            // Arrange
            using var context = new ApplicationDbContext(_dbOptions);
            var improvement = new Improvement { Id = 0, Name = "Jacuzzi", Description = "Jacuzzi exterior" };
            context.Improvements.Add(improvement);
            await context.SaveChangesAsync();

            var service = CreateService();

            // Act
            var result = await service.GetById(improvement.Id);

            // Assert
            result.Should().NotBeNull();
            result!.Name.Should().Be("Jacuzzi");
        }

        [Fact]
        public async Task GetById_Should_Return_Null_When_NotFound()
        {
            // Arrange
            var service = CreateService();

            // Act
            var result = await service.GetById(999);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task UpdateAsync_Should_Update_Improvement()
        {
            // Arrange
            using var context = new ApplicationDbContext(_dbOptions);
            var improvement = new Improvement { Id = 0, Name = "Gimnasio", Description = "Básico" };
            context.Improvements.Add(improvement);
            await context.SaveChangesAsync();

            var service = CreateService();
            var dto = new ImprovementDto
            {
                Id = improvement.Id,
                Name = "Gimnasio",
                Description = "Completamente equipado"
            };

            // Act
            var result = await service.UpdateAsync(dto, improvement.Id);

            // Assert
            result.Should().NotBeNull();
            result!.Description.Should().Be("Completamente equipado");
        }

        [Fact]
        public async Task UpdateAsync_Should_Return_Null_When_NotExists()
        {
            // Arrange
            var service = CreateService();
            var dto = new ImprovementDto { Id = 999, Name = "Fake", Description = "Fake" };

            // Act
            var result = await service.UpdateAsync(dto, 999);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task DeleteAsync_Should_Return_True_When_Improvement_Exists()
        {
            // Arrange
            using var context = new ApplicationDbContext(_dbOptions);
            var improvement = new Improvement { Id = 0, Name = "Balcón", Description = "Vista al mar" };
            context.Improvements.Add(improvement);
            await context.SaveChangesAsync();

            var service = CreateService();

            // Act
            var result = await service.DeleteAsync(improvement.Id);

            // Assert
            result.Should().BeTrue();
            var deleted = await service.GetById(improvement.Id);
            deleted.Should().BeNull();
        }

        [Fact]
        public async Task DeleteAsync_Should_Return_False_When_Improvement_NotExists()
        {
            // Arrange
            var service = CreateService();

            // Act
            var result = await service.DeleteAsync(999);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task GetAll_Should_Return_All_Improvements()
        {
            // Arrange
            using var context = new ApplicationDbContext(_dbOptions);
            context.Improvements.AddRange(
                new Improvement { Id = 0, Name = "Parqueo", Description = "Dos espacios" },
                new Improvement { Id = 0, Name = "Terraza", Description = "Techada" }
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