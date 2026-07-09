using AutoMapper;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using RealEstateApp.Core.Application.Dtos.Property;
using RealEstateApp.Core.Application.Mappings.EntitiesAndDtos;
using RealEstateApp.Core.Application.Services;
using RealEstateApp.Core.Domain.Entities;
using RealEstateApp.Core.Domain.Enums;
using RealEstateApp.Infrastructure.Persistence.Contexts;
using RealEstateApp.Infrastructure.Persistence.Repositories;

namespace RealEstateApp.Unit.Tests.Services
{
    public class PropertyServiceTests
    {
        private readonly DbContextOptions<ApplicationDbContext> _dbOptions;
        private readonly IMapper _mapper;

        public PropertyServiceTests()
        {
            _dbOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase($"TestDb_PropertyService_{Guid.NewGuid()}")
                .Options;

            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<PropertyMappingProfile>();
                cfg.AddProfile<PropertyTypeMappingProfile>();
                cfg.AddProfile<SaleTypeMappingProfile>();
                cfg.AddProfile<ImprovementMappingProfile>();
            });

            _mapper = config.CreateMapper();
        }

        private async Task<(PropertyType pt, SaleType st, Improvement improvement)> SeedCatalogAsync(ApplicationDbContext context)
        {
            var pt = new PropertyType { Id = 0, Name = "Casa", Description = "Vivienda" };
            var st = new SaleType { Id = 0, Name = "Venta", Description = "Directa" };
            var improvement = new Improvement { Id = 0, Name = "Piscina", Description = "Climatizada" };
            context.PropertyTypes.Add(pt);
            context.SaleTypes.Add(st);
            context.Improvements.Add(improvement);
            await context.SaveChangesAsync();
            return (pt, st, improvement);
        }

        private PropertyService CreateService()
        {
            var factoryMoq = new Mock<ILoggerFactory>();
            factoryMoq.Setup(x => x.CreateLogger(It.IsAny<string>()))
                .Returns(new NullLogger<PropertyService>());

            var context = new ApplicationDbContext(_dbOptions);
            var propertyRepo = new PropertyRepository(context, new NullLogger<GenericRepository<Property>>());
            var propertyTypeRepo = new PropertyTypeRepository(context, new NullLogger<GenericRepository<PropertyType>>());
            var saleTypeRepo = new SaleTypeRepository(context, new NullLogger<GenericRepository<SaleType>>());
            var improvementRepo = new ImprovementRepository(context, new NullLogger<GenericRepository<Improvement>>());
            var propertyImageRepo = new PropertyImageRepository(context, new NullLogger<GenericRepository<PropertyImage>>());
            var propertyImprovementRepo = new PropertyImprovementRepository(context, new NullLogger<GenericRepository<PropertyImprovement>>());

            return new PropertyService(propertyRepo, propertyTypeRepo, saleTypeRepo, improvementRepo,
                propertyImageRepo, propertyImprovementRepo, _mapper, factoryMoq.Object);
        }

        [Fact]
        public async Task AddAsync_Should_Create_Property_With_Unique_Code()
        {
            // Arrange
            using var context = new ApplicationDbContext(_dbOptions);
            var (pt, st, improvement) = await SeedCatalogAsync(context);

            var service = CreateService();

            var dto = new PropertyDto
            {
                Id = 0,
                AgentId = "agent-1",
                PropertyTypeId = pt.Id,
                SaleTypeId = st.Id,
                Price = 5000000,
                LandSize = 200,
                Bedrooms = 3,
                Bathrooms = 2,
                Description = "Casa moderna",
                ImprovementIds = [improvement.Id]
            };

            // Act
            var result = await service.AddAsync(dto);

            // Assert
            result.Should().NotBeNull();
            result!.Code.Should().NotBeNullOrEmpty();
            result.Code!.Length.Should().Be(6);
            result.Status.Should().Be(PropertyStatus.Available.ToString());
        }

        [Fact]
        public async Task AddAsync_Should_Return_Null_When_No_PropertyTypes_Exist()
        {
            // Arrange
            var service = CreateService();

            var dto = new PropertyDto
            {
                Id = 0,
                AgentId = "agent-1",
                PropertyTypeId = 1,
                SaleTypeId = 1,
                Price = 3000000,
                LandSize = 100,
                Bedrooms = 2,
                Bathrooms = 1,
                Description = "Sin tipos"
            };

            // Act
            var result = await service.AddAsync(dto);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task AddAsync_Should_Return_Null_When_No_SaleTypes_Exist()
        {
            // Arrange
            using var context = new ApplicationDbContext(_dbOptions);
            context.PropertyTypes.Add(new PropertyType { Id = 0, Name = "Casa", Description = "Vivienda" });
            await context.SaveChangesAsync();

            var service = CreateService();

            var dto = new PropertyDto
            {
                Id = 0,
                AgentId = "agent-1",
                PropertyTypeId = 1,
                SaleTypeId = 1,
                Price = 3000000,
                LandSize = 100,
                Bedrooms = 2,
                Bathrooms = 1,
                Description = "Sin tipo de venta"
            };

            // Act
            var result = await service.AddAsync(dto);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task AddAsync_Should_Return_Null_When_No_Improvements_Exist()
        {
            // Arrange
            using var context = new ApplicationDbContext(_dbOptions);
            context.PropertyTypes.Add(new PropertyType { Id = 0, Name = "Casa", Description = "Vivienda" });
            context.SaleTypes.Add(new SaleType { Id = 0, Name = "Venta", Description = "Directa" });
            await context.SaveChangesAsync();

            var service = CreateService();

            var dto = new PropertyDto
            {
                Id = 0,
                AgentId = "agent-1",
                PropertyTypeId = 1,
                SaleTypeId = 1,
                Price = 3000000,
                LandSize = 100,
                Bedrooms = 2,
                Bathrooms = 1,
                Description = "Sin mejoras"
            };

            // Act
            var result = await service.AddAsync(dto);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task AddAsync_Should_Save_Images_And_Improvements()
        {
            // Arrange
            using var context = new ApplicationDbContext(_dbOptions);
            var (pt, st, improvement) = await SeedCatalogAsync(context);

            var service = CreateService();

            var dto = new PropertyDto
            {
                Id = 0,
                AgentId = "agent-1",
                PropertyTypeId = pt.Id,
                SaleTypeId = st.Id,
                Price = 4000000,
                LandSize = 150,
                Bedrooms = 3,
                Bathrooms = 2,
                Description = "Con imágenes y mejoras",
                Images = ["/img1.jpg", "/img2.jpg"],
                ImprovementIds = [improvement.Id]
            };

            // Act
            var result = await service.AddAsync(dto);

            // Assert
            result.Should().NotBeNull();

            var images = context.PropertyImages.Where(i => i.PropertyId == result!.Id).ToList();
            images.Should().HaveCount(2);

            var improvements = context.PropertyImprovements.Where(pi => pi.PropertyId == result!.Id).ToList();
            improvements.Should().HaveCount(1);
        }

        [Fact]
        public async Task GetById_Should_Return_Property_With_Includes()
        {
            // Arrange
            using var context = new ApplicationDbContext(_dbOptions);
            var (pt, st, _) = await SeedCatalogAsync(context);

            var property = new Property
            {
                Id = 0,
                Code = "GBI001",
                AgentId = "agent-1",
                PropertyTypeId = pt.Id,
                SaleTypeId = st.Id,
                Price = 3000000,
                LandSize = 120,
                Bedrooms = 2,
                Bathrooms = 1,
                Description = "Con includes",
                Status = PropertyStatus.Available
            };
            context.Properties.Add(property);
            await context.SaveChangesAsync();

            var service = CreateService();

            // Act
            var result = await service.GetById(property.Id);

            // Assert
            result.Should().NotBeNull();
            result!.PropertyTypeName.Should().Be("Casa");
            result.SaleTypeName.Should().Be("Venta");
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
        public async Task GetByCodeAsync_Should_Return_Property_When_Code_Exists()
        {
            // Arrange
            using var context = new ApplicationDbContext(_dbOptions);
            var (pt, st, _) = await SeedCatalogAsync(context);

            var property = new Property
            {
                Id = 0,
                Code = "COD123",
                AgentId = "agent-1",
                PropertyTypeId = pt.Id,
                SaleTypeId = st.Id,
                Price = 5000000,
                LandSize = 200,
                Bedrooms = 4,
                Bathrooms = 3,
                Description = "Código único",
                Status = PropertyStatus.Available
            };
            context.Properties.Add(property);
            await context.SaveChangesAsync();

            var service = CreateService();

            // Act
            var result = await service.GetByCodeAsync("COD123");

            // Assert
            result.Should().NotBeNull();
            result!.Code.Should().Be("COD123");
        }

        [Fact]
        public async Task GetByCodeAsync_Should_Return_Null_When_Code_NotExists()
        {
            // Arrange
            var service = CreateService();

            // Act
            var result = await service.GetByCodeAsync("ZZZZZZ");

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetByAgentIdAsync_Should_Return_Properties_Of_Agent()
        {
            // Arrange
            using var context = new ApplicationDbContext(_dbOptions);
            var (pt, st, _) = await SeedCatalogAsync(context);

            var agentId = "agent-filter";
            context.Properties.AddRange(
                new Property { Id = 0, Code = "A00001", AgentId = agentId, PropertyTypeId = pt.Id, SaleTypeId = st.Id, Price = 1000000, LandSize = 80, Bedrooms = 2, Bathrooms = 1, Description = "D1", Status = PropertyStatus.Available },
                new Property { Id = 0, Code = "A00002", AgentId = agentId, PropertyTypeId = pt.Id, SaleTypeId = st.Id, Price = 2000000, LandSize = 100, Bedrooms = 3, Bathrooms = 2, Description = "D2", Status = PropertyStatus.Available },
                new Property { Id = 0, Code = "B00001", AgentId = "otro-agente", PropertyTypeId = pt.Id, SaleTypeId = st.Id, Price = 3000000, LandSize = 150, Bedrooms = 4, Bathrooms = 3, Description = "D3", Status = PropertyStatus.Available }
            );
            await context.SaveChangesAsync();

            var service = CreateService();

            // Act
            var result = await service.GetByAgentIdAsync(agentId);

            // Assert
            result.Should().HaveCount(2);
            result.Should().AllSatisfy(p => p.AgentId.Should().Be(agentId));
        }

        [Fact]
        public async Task UpdateAsync_Should_Update_Property_Fields()
        {
            // Arrange
            using var context = new ApplicationDbContext(_dbOptions);
            var (pt, st, _) = await SeedCatalogAsync(context);

            var property = new Property
            {
                Id = 0,
                Code = "UPD001",
                AgentId = "agent-1",
                PropertyTypeId = pt.Id,
                SaleTypeId = st.Id,
                Price = 2000000,
                LandSize = 100,
                Bedrooms = 2,
                Bathrooms = 1,
                Description = "Original",
                Status = PropertyStatus.Available
            };
            context.Properties.Add(property);
            await context.SaveChangesAsync();

            var service = CreateService();

            var dto = new PropertyDto
            {
                Id = property.Id,
                AgentId = "agent-1",
                PropertyTypeId = pt.Id,
                SaleTypeId = st.Id,
                Price = 2500000,
                LandSize = 120,
                Bedrooms = 3,
                Bathrooms = 2,
                Description = "Actualizada"
            };

            // Act
            var result = await service.UpdateAsync(dto, property.Id);

            // Assert
            result.Should().NotBeNull();
            result!.Price.Should().Be(2500000);
            result.Description.Should().Be("Actualizada");
        }

        [Fact]
        public async Task UpdateAsync_Should_Return_Null_When_Property_NotExists()
        {
            // Arrange
            var service = CreateService();

            var dto = new PropertyDto
            {
                Id = 999,
                AgentId = "agent-1",
                PropertyTypeId = 1,
                SaleTypeId = 1,
                Price = 1000000,
                LandSize = 50,
                Bedrooms = 1,
                Bathrooms = 1,
                Description = "No existe"
            };

            // Act
            var result = await service.UpdateAsync(dto, 999);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task DeleteAsync_Should_Return_True_When_Property_Is_Available()
        {
            // Arrange
            using var context = new ApplicationDbContext(_dbOptions);
            var (pt, st, _) = await SeedCatalogAsync(context);

            var property = new Property
            {
                Id = 0,
                Code = "DEL001",
                AgentId = "agent-1",
                PropertyTypeId = pt.Id,
                SaleTypeId = st.Id,
                Price = 1000000,
                LandSize = 70,
                Bedrooms = 1,
                Bathrooms = 1,
                Description = "A eliminar",
                Status = PropertyStatus.Available
            };
            context.Properties.Add(property);
            await context.SaveChangesAsync();

            var service = CreateService();

            // Act
            var result = await service.DeleteAsync(property.Id);

            // Assert
            result.Should().BeTrue();
            var deleted = await service.GetById(property.Id);
            deleted.Should().BeNull();
        }

        [Fact]
        public async Task DeleteAsync_Should_Return_False_When_Property_Is_Sold()
        {
            // Arrange
            using var context = new ApplicationDbContext(_dbOptions);
            var (pt, st, _) = await SeedCatalogAsync(context);

            var property = new Property
            {
                Id = 0,
                Code = "SLD001",
                AgentId = "agent-1",
                PropertyTypeId = pt.Id,
                SaleTypeId = st.Id,
                Price = 5000000,
                LandSize = 250,
                Bedrooms = 4,
                Bathrooms = 3,
                Description = "Ya vendida",
                Status = PropertyStatus.Sold
            };
            context.Properties.Add(property);
            await context.SaveChangesAsync();

            var service = CreateService();

            // Act
            var result = await service.DeleteAsync(property.Id);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task DeleteAsync_Should_Return_False_When_Property_NotExists()
        {
            // Arrange
            var service = CreateService();

            // Act
            var result = await service.DeleteAsync(999);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task GetAllWithFiltersAsync_Should_Return_Only_Available_Properties()
        {
            // Arrange
            using var context = new ApplicationDbContext(_dbOptions);
            var (pt, st, _) = await SeedCatalogAsync(context);

            context.Properties.AddRange(
                new Property { Id = 0, Code = "AV0001", AgentId = "a1", PropertyTypeId = pt.Id, SaleTypeId = st.Id, Price = 2000000, LandSize = 100, Bedrooms = 3, Bathrooms = 2, Description = "D1", Status = PropertyStatus.Available },
                new Property { Id = 0, Code = "SL0001", AgentId = "a1", PropertyTypeId = pt.Id, SaleTypeId = st.Id, Price = 2000000, LandSize = 100, Bedrooms = 3, Bathrooms = 2, Description = "D2", Status = PropertyStatus.Sold }
            );
            await context.SaveChangesAsync();

            var service = CreateService();

            // Act
            var result = await service.GetAllWithFiltersAsync(null, null, null, null, null);

            // Assert
            result.Should().HaveCount(1);
            result[0].Status.Should().Be(PropertyStatus.Available.ToString());
        }

        [Fact]
        public async Task GetAllWithFiltersAsync_Should_Filter_By_Price_Range()
        {
            // Arrange
            using var context = new ApplicationDbContext(_dbOptions);
            var (pt, st, _) = await SeedCatalogAsync(context);

            context.Properties.AddRange(
                new Property { Id = 0, Code = "P10001", AgentId = "a1", PropertyTypeId = pt.Id, SaleTypeId = st.Id, Price = 500000, LandSize = 50, Bedrooms = 1, Bathrooms = 1, Description = "Econ", Status = PropertyStatus.Available },
                new Property { Id = 0, Code = "P10002", AgentId = "a1", PropertyTypeId = pt.Id, SaleTypeId = st.Id, Price = 1500000, LandSize = 100, Bedrooms = 2, Bathrooms = 1, Description = "Med", Status = PropertyStatus.Available },
                new Property { Id = 0, Code = "P10003", AgentId = "a1", PropertyTypeId = pt.Id, SaleTypeId = st.Id, Price = 8000000, LandSize = 500, Bedrooms = 6, Bathrooms = 5, Description = "Luj", Status = PropertyStatus.Available }
            );
            await context.SaveChangesAsync();

            var service = CreateService();

            // Act
            var result = await service.GetAllWithFiltersAsync(null, 1000000, 2000000, null, null);

            // Assert
            result.Should().HaveCount(1);
            result[0].Price.Should().Be(1500000);
        }

        [Fact]
        public async Task GetAll_Should_Return_All_Properties_With_Includes()
        {
            // Arrange
            using var context = new ApplicationDbContext(_dbOptions);
            var (pt, st, _) = await SeedCatalogAsync(context);

            context.Properties.AddRange(
                new Property { Id = 0, Code = "GA0001", AgentId = "a1", PropertyTypeId = pt.Id, SaleTypeId = st.Id, Price = 1000000, LandSize = 80, Bedrooms = 2, Bathrooms = 1, Description = "D1", Status = PropertyStatus.Available },
                new Property { Id = 0, Code = "GA0002", AgentId = "a2", PropertyTypeId = pt.Id, SaleTypeId = st.Id, Price = 2000000, LandSize = 120, Bedrooms = 3, Bathrooms = 2, Description = "D2", Status = PropertyStatus.Available }
            );
            await context.SaveChangesAsync();

            var service = CreateService();

            // Act
            var result = await service.GetAll();

            // Assert
            result.Should().HaveCount(2);
            result.Should().AllSatisfy(p => p.PropertyTypeName.Should().Be("Casa"));
        }
    }
}