using AutoMapper;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using RealEstateApp.Core.Application.Dtos.SaleType;
using RealEstateApp.Core.Application.Mappings.EntitiesAndDtos;
using RealEstateApp.Core.Application.Services;
using RealEstateApp.Core.Domain.Entities;
using RealEstateApp.Core.Domain.Enums;
using RealEstateApp.Infrastructure.Persistence.Contexts;
using RealEstateApp.Infrastructure.Persistence.Repositories;

namespace RealEstateApp.Unit.Tests.Services
{
    public class SaleTypeServiceTests
    {
        private readonly DbContextOptions<ApplicationDbContext> _dbOptions;
        private readonly IMapper _mapper;

        public SaleTypeServiceTests()
        {
            _dbOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase($"TestDb_SaleTypeService_{Guid.NewGuid()}")
                .Options;

            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<SaleTypeMappingProfile>();
                cfg.AddProfile<PropertyMappingProfile>();
            });

            _mapper = config.CreateMapper();
        }

        private SaleTypeService CreateService()
        {
            var factoryMoq = new Mock<ILoggerFactory>();
            factoryMoq.Setup(x => x.CreateLogger(It.IsAny<string>()))
                .Returns(new NullLogger<SaleTypeService>());

            var context = new ApplicationDbContext(_dbOptions);
            var repo = new SaleTypeRepository(context, new NullLogger<GenericRepository<SaleType>>());
            var propertyRepo = new PropertyRepository(context, new NullLogger<GenericRepository<Property>>());
            return new SaleTypeService(repo, propertyRepo, _mapper, factoryMoq.Object);
        }

        [Fact]
        public async Task AddAsync_Should_Add_SaleType()
        {
            // Arrange
            var service = CreateService();
            var dto = new SaleTypeDto
            {
                Id = 0,
                Name = "Venta",
                Description = "Venta directa"
            };

            // Act
            var result = await service.AddAsync(dto);

            // Assert
            result.Should().NotBeNull();
            result!.Name.Should().Be("Venta");
            result.Id.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task GetById_Should_Return_SaleType_When_Exists()
        {
            // Arrange
            using var context = new ApplicationDbContext(_dbOptions);
            var saleType = new SaleType { Id = 0, Name = "Alquiler", Description = "Mensual" };
            context.SaleTypes.Add(saleType);
            await context.SaveChangesAsync();

            var service = CreateService();

            // Act
            var result = await service.GetById(saleType.Id);

            // Assert
            result.Should().NotBeNull();
            result!.Name.Should().Be("Alquiler");
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
        public async Task UpdateAsync_Should_Update_SaleType()
        {
            // Arrange
            using var context = new ApplicationDbContext(_dbOptions);
            var saleType = new SaleType { Id = 0, Name = "Alquiler Amueblado", Description = "Original" };
            context.SaleTypes.Add(saleType);
            await context.SaveChangesAsync();

            var service = CreateService();
            var dto = new SaleTypeDto
            {
                Id = saleType.Id,
                Name = "Alquiler Amueblado",
                Description = "Propiedad completamente amueblada"
            };

            // Act
            var result = await service.UpdateAsync(dto, saleType.Id);

            // Assert
            result.Should().NotBeNull();
            result!.Description.Should().Be("Propiedad completamente amueblada");
        }

        [Fact]
        public async Task UpdateAsync_Should_Return_Null_When_NotExists()
        {
            // Arrange
            var service = CreateService();
            var dto = new SaleTypeDto { Id = 999, Name = "Fake", Description = "Fake" };

            // Act
            var result = await service.UpdateAsync(dto, 999);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetAll_Should_Return_PropertyCount_Per_SaleType()
        {
            // Arrange
            using var context = new ApplicationDbContext(_dbOptions);
            var pt = new PropertyType { Id = 0, Name = "Casa", Description = "Vivienda" };
            var st = new SaleType { Id = 0, Name = "Permuta", Description = "Intercambio" };
            context.PropertyTypes.Add(pt);
            context.SaleTypes.Add(st);
            await context.SaveChangesAsync();

            context.Properties.AddRange(
                new Property { Id = 0, Code = "P00001", AgentId = "a1", PropertyTypeId = pt.Id, SaleTypeId = st.Id, Price = 3000000, LandSize = 100, Bedrooms = 3, Bathrooms = 2, Description = "D1", Status = PropertyStatus.Available },
                new Property { Id = 0, Code = "P00002", AgentId = "a1", PropertyTypeId = pt.Id, SaleTypeId = st.Id, Price = 4000000, LandSize = 150, Bedrooms = 4, Bathrooms = 3, Description = "D2", Status = PropertyStatus.Available },
                new Property { Id = 0, Code = "P00003", AgentId = "a1", PropertyTypeId = pt.Id, SaleTypeId = st.Id, Price = 5000000, LandSize = 200, Bedrooms = 5, Bathrooms = 4, Description = "D3", Status = PropertyStatus.Available }
            );
            await context.SaveChangesAsync();

            var service = CreateService();

            // Act
            var result = await service.GetAll();

            // Assert
            result.Should().NotBeEmpty();
            result.First(x => x.Id == st.Id).PropertyCount.Should().Be(3);
        }

        [Fact]
        public async Task DeleteAsync_Should_Delete_SaleType_And_Its_Properties()
        {
            // Arrange
            using var context = new ApplicationDbContext(_dbOptions);
            var pt = new PropertyType { Id = 0, Name = "Apartamento", Description = "Apt" };
            var st = new SaleType { Id = 0, Name = "Venta con Financiamiento", Description = "Con plan de pago" };
            context.PropertyTypes.Add(pt);
            context.SaleTypes.Add(st);
            await context.SaveChangesAsync();

            context.Properties.AddRange(
                new Property { Id = 0, Code = "V00001", AgentId = "a1", PropertyTypeId = pt.Id, SaleTypeId = st.Id, Price = 2000000, LandSize = 80, Bedrooms = 2, Bathrooms = 1, Description = "D1", Status = PropertyStatus.Available },
                new Property { Id = 0, Code = "V00002", AgentId = "a1", PropertyTypeId = pt.Id, SaleTypeId = st.Id, Price = 2500000, LandSize = 100, Bedrooms = 3, Bathrooms = 2, Description = "D2", Status = PropertyStatus.Available }
            );
            await context.SaveChangesAsync();

            var service = CreateService();

            // Act
            var result = await service.DeleteAsync(st.Id);

            // Assert
            result.Should().BeTrue();

            // Verificar que el tipo de venta fue eliminado
            var deleted = await service.GetById(st.Id);
            deleted.Should().BeNull();

            // Verificar que las propiedades asociadas también fueron eliminadas
            var remaining = context.Properties.Where(p => p.SaleTypeId == st.Id).ToList();
            remaining.Should().BeEmpty();
        }

        [Fact]
        public async Task GetAll_Should_Return_All_SaleTypes()
        {
            // Arrange
            using var context = new ApplicationDbContext(_dbOptions);
            context.SaleTypes.AddRange(
                new SaleType { Id = 0, Name = "Venta", Description = "Directa" },
                new SaleType { Id = 0, Name = "Alquiler", Description = "Mensual" },
                new SaleType { Id = 0, Name = "Permuta", Description = "Intercambio" }
            );
            await context.SaveChangesAsync();

            var service = CreateService();

            // Act
            var result = await service.GetAll();

            // Assert
            result.Should().HaveCount(3);
        }
    }
}