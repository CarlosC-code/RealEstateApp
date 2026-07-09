using AutoMapper;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using RealEstateApp.Core.Application.Dtos.PropertyType;
using RealEstateApp.Core.Application.Mappings.EntitiesAndDtos;
using RealEstateApp.Core.Application.Services;
using RealEstateApp.Core.Domain.Entities;
using RealEstateApp.Core.Domain.Enums;
using RealEstateApp.Infrastructure.Persistence.Contexts;
using RealEstateApp.Infrastructure.Persistence.Repositories;

namespace RealEstateApp.Unit.Tests.Services
{
    public class PropertyTypeServiceTests
    {
        private readonly DbContextOptions<ApplicationDbContext> _dbOptions;
        private readonly IMapper _mapper;

        public PropertyTypeServiceTests()
        {
            _dbOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase($"TestDb_PropertyTypeService_{Guid.NewGuid()}")
                .Options;

            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<PropertyTypeMappingProfile>();
                cfg.AddProfile<PropertyMappingProfile>();
            });

            _mapper = config.CreateMapper();
        }

        private PropertyTypeService CreateService()
        {
            var factoryMoq = new Mock<ILoggerFactory>();
            factoryMoq.Setup(x => x.CreateLogger(It.IsAny<string>()))
                .Returns(new NullLogger<PropertyTypeService>());

            var context = new ApplicationDbContext(_dbOptions);
            var repo = new PropertyTypeRepository(context, new NullLogger<GenericRepository<PropertyType>>());
            var propertyRepo = new PropertyRepository(context, new NullLogger<GenericRepository<Property>>());
            return new PropertyTypeService(repo, propertyRepo, _mapper, factoryMoq.Object);
        }

        [Fact]
        public async Task AddAsync_Should_Add_PropertyType()
        {
            // Arrange
            var service = CreateService();
            var dto = new PropertyTypeDto
            {
                Id = 0,
                Name = "Apartamento",
                Description = "Unidad en edificio"
            };

            // Act
            var result = await service.AddAsync(dto);

            // Assert
            result.Should().NotBeNull();
            result!.Name.Should().Be("Apartamento");
            result.Id.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task GetById_Should_Return_PropertyType_When_Exists()
        {
            // Arrange
            using var context = new ApplicationDbContext(_dbOptions);
            var propertyType = new PropertyType { Id = 0, Name = "Casa", Description = "Vivienda" };
            context.PropertyTypes.Add(propertyType);
            await context.SaveChangesAsync();

            var service = CreateService();

            // Act
            var result = await service.GetById(propertyType.Id);

            // Assert
            result.Should().NotBeNull();
            result!.Name.Should().Be("Casa");
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
        public async Task UpdateAsync_Should_Update_PropertyType()
        {
            // Arrange
            using var context = new ApplicationDbContext(_dbOptions);
            var propertyType = new PropertyType { Id = 0, Name = "Villa", Description = "Original" };
            context.PropertyTypes.Add(propertyType);
            await context.SaveChangesAsync();

            var service = CreateService();
            var dto = new PropertyTypeDto
            {
                Id = propertyType.Id,
                Name = "Villa",
                Description = "Residencia de lujo"
            };

            // Act
            var result = await service.UpdateAsync(dto, propertyType.Id);

            // Assert
            result.Should().NotBeNull();
            result!.Description.Should().Be("Residencia de lujo");
        }

        [Fact]
        public async Task UpdateAsync_Should_Return_Null_When_NotExists()
        {
            // Arrange
            var service = CreateService();
            var dto = new PropertyTypeDto { Id = 999, Name = "Fake", Description = "Fake" };

            // Act
            var result = await service.UpdateAsync(dto, 999);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetAll_Should_Return_PropertyCount_Per_Type()
        {
            // Arrange
            using var context = new ApplicationDbContext(_dbOptions);
            var pt = new PropertyType { Id = 0, Name = "Penthouse", Description = "Último piso" };
            var st = new SaleType { Id = 0, Name = "Venta", Description = "Venta directa" };
            context.PropertyTypes.Add(pt);
            context.SaleTypes.Add(st);
            await context.SaveChangesAsync();

            context.Properties.AddRange(
                new Property { Id = 0, Code = "P00001", AgentId = "a1", PropertyTypeId = pt.Id, SaleTypeId = st.Id, Price = 5000000, LandSize = 200, Bedrooms = 3, Bathrooms = 2, Description = "Desc", Status = PropertyStatus.Available },
                new Property { Id = 0, Code = "P00002", AgentId = "a1", PropertyTypeId = pt.Id, SaleTypeId = st.Id, Price = 6000000, LandSize = 250, Bedrooms = 4, Bathrooms = 3, Description = "Desc", Status = PropertyStatus.Available }
            );
            await context.SaveChangesAsync();

            var service = CreateService();

            // Act
            var result = await service.GetAll();

            // Assert
            result.Should().NotBeEmpty();
            result.First(x => x.Id == pt.Id).PropertyCount.Should().Be(2);
        }

        [Fact]
        public async Task DeleteAsync_Should_Delete_PropertyType_And_Its_Properties()
        {
            // Arrange
            using var context = new ApplicationDbContext(_dbOptions);
            var pt = new PropertyType { Id = 0, Name = "Local", Description = "Comercial" };
            var st = new SaleType { Id = 0, Name = "Alquiler", Description = "Mensual" };
            context.PropertyTypes.Add(pt);
            context.SaleTypes.Add(st);
            await context.SaveChangesAsync();

            context.Properties.Add(new Property
            {
                Id = 0,
                Code = "L00001",
                AgentId = "a1",
                PropertyTypeId = pt.Id,
                SaleTypeId = st.Id,
                Price = 1000000,
                LandSize = 80,
                Bedrooms = 0,
                Bathrooms = 1,
                Description = "Local comercial",
                Status = PropertyStatus.Available
            });
            await context.SaveChangesAsync();

            var service = CreateService();

            // Act
            var result = await service.DeleteAsync(pt.Id);

            // Assert
            result.Should().BeTrue();

            // Verificar que el tipo de propiedad fue eliminado
            var deleted = await service.GetById(pt.Id);
            deleted.Should().BeNull();

            // Verificar que las propiedades asociadas también fueron eliminadas
            var remainingProperties = context.Properties.Where(p => p.PropertyTypeId == pt.Id).ToList();
            remainingProperties.Should().BeEmpty();
        }

        [Fact]
        public async Task GetAll_Should_Return_All_PropertyTypes()
        {
            // Arrange
            using var context = new ApplicationDbContext(_dbOptions);
            context.PropertyTypes.AddRange(
                new PropertyType { Id = 0, Name = "Tipo A", Description = "Desc A" },
                new PropertyType { Id = 0, Name = "Tipo B", Description = "Desc B" }
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