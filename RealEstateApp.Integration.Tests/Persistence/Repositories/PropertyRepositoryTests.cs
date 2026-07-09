using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Newtonsoft.Json.Linq;
using RealEstateApp.Core.Domain.Entities;
using RealEstateApp.Core.Domain.Enums;
using RealEstateApp.Infrastructure.Persistence.Contexts;
using RealEstateApp.Infrastructure.Persistence.Repositories;

namespace RealEstateApp.Integration.Tests.Persistence.Repositories
{
    public class PropertyRepositoryTests
    {
        private readonly DbContextOptions<ApplicationDbContext> _dbOptions;

        public PropertyRepositoryTests()
        {
            _dbOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: $"TestDb_Property_{Guid.NewGuid()}")
                .Options;
        }

        private Mock<ILogger<GenericRepository<Property>>> CreateLoggerMock()
        {
            return new Mock<ILogger<GenericRepository<Property>>>();
        }

        private async Task<(PropertyType pt, SaleType st)> SeedDependenciesAsync(ApplicationDbContext context)
        {
            var pt = new PropertyType { Id = 0, Name = "Casa", Description = "Vivienda" };
            var st = new SaleType { Id = 0, Name = "Venta", Description = "Venta directa" };
            context.PropertyTypes.Add(pt);
            context.SaleTypes.Add(st);
            await context.SaveChangesAsync();
            return (pt, st);
        }

        [Fact]
        public async Task AddAsync_Should_Add_Property_To_Database()
        {
            // Arrange
            using var context = new ApplicationDbContext(_dbOptions);
            var loggerMock = CreateLoggerMock();
            var (pt, st) = await SeedDependenciesAsync(context);

            var repo = new PropertyRepository(context, loggerMock.Object);

            var property = new Property
            {
                Id = 0,
                Code = "ABC123",
                AgentId = "agent-1",
                PropertyTypeId = pt.Id,
                SaleTypeId = st.Id,
                Price = 5000000,
                LandSize = 200,
                Bedrooms = 3,
                Bathrooms = 2,
                Description = "Hermosa casa familiar",
                Status = PropertyStatus.Available
            };

            // Act
            var result = await repo.AddAsync(property);

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
            var repo = new PropertyRepository(context, loggerMock.Object);

            // Act
            Func<Task> act = async () => await repo.AddAsync(null!);

            // Assert
            await act.Should().ThrowAsync<Exception>();
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Property_When_Exists()
        {
            // Arrange
            using var context = new ApplicationDbContext(_dbOptions);
            var loggerMock = CreateLoggerMock();
            var (pt, st) = await SeedDependenciesAsync(context);

            var property = new Property
            {
                Id = 0,
                Code = "XYZ789",
                AgentId = "agent-2",
                PropertyTypeId = pt.Id,
                SaleTypeId = st.Id,
                Price = 3000000,
                LandSize = 150,
                Bedrooms = 2,
                Bathrooms = 1,
                Description = "Apartamento céntrico",
                Status = PropertyStatus.Available
            };
            context.Properties.Add(property);
            await context.SaveChangesAsync();

            var repo = new PropertyRepository(context, loggerMock.Object);

            // Act
            var result = await repo.GetByIdAsync(property.Id);

            // Assert
            result.Should().NotBeNull();
            result!.Code.Should().Be("XYZ789");
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_When_NotExists()
        {
            // Arrange
            using var context = new ApplicationDbContext(_dbOptions);
            var loggerMock = CreateLoggerMock();
            var repo = new PropertyRepository(context, loggerMock.Object);

            // Act
            var result = await repo.GetByIdAsync(999);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task UpdateAsync_Should_Update_Property()
        {
            // Arrange
            using var context = new ApplicationDbContext(_dbOptions);
            var loggerMock = CreateLoggerMock();
            var (pt, st) = await SeedDependenciesAsync(context);

            var property = new Property
            {
                Id = 0,
                Code = "UPD001",
                AgentId = "agent-3",
                PropertyTypeId = pt.Id,
                SaleTypeId = st.Id,
                Price = 2000000,
                LandSize = 100,
                Bedrooms = 1,
                Bathrooms = 1,
                Description = "Descripción original",
                Status = PropertyStatus.Available
            };
            context.Properties.Add(property);
            await context.SaveChangesAsync();

            var repo = new PropertyRepository(context, loggerMock.Object);
            property.Price = 2500000;

            // Act
            var updated = await repo.UpdateAsync(property.Id, property);

            // Assert
            updated.Should().NotBeNull();
            updated!.Price.Should().Be(2500000);
        }

        [Fact]
        public async Task UpdateAsync_Should_Return_Null_When_NotExists()
        {
            // Arrange
            using var context = new ApplicationDbContext(_dbOptions);
            var loggerMock = CreateLoggerMock();
            var repo = new PropertyRepository(context, loggerMock.Object);

            var fake = new Property
            {
                Id = 999,
                Code = "FAKE00",
                AgentId = "agent-x",
                PropertyTypeId = 1,
                SaleTypeId = 1,
                Price = 999,
                LandSize = 0,
                Bedrooms = 0,
                Bathrooms = 0,
                Description = "Fake",
                Status = PropertyStatus.Available
            };

            // Act
            var result = await repo.UpdateAsync(fake.Id, fake);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task DeleteAsync_Should_Remove_Property()
        {
            // Arrange
            using var context = new ApplicationDbContext(_dbOptions);
            var loggerMock = CreateLoggerMock();
            var (pt, st) = await SeedDependenciesAsync(context);

            var property = new Property
            {
                Id = 0,
                Code = "DEL001",
                AgentId = "agent-4",
                PropertyTypeId = pt.Id,
                SaleTypeId = st.Id,
                Price = 1000000,
                LandSize = 80,
                Bedrooms = 1,
                Bathrooms = 1,
                Description = "Propiedad a eliminar",
                Status = PropertyStatus.Available
            };
            context.Properties.Add(property);
            await context.SaveChangesAsync();

            var repo = new PropertyRepository(context, loggerMock.Object);

            // Act
            await repo.DeleteAsync(property.Id);
            var result = await repo.GetByIdAsync(property.Id);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task DeleteAsync_Should_Not_Throw_When_Id_NotExists()
        {
            // Arrange
            using var context = new ApplicationDbContext(_dbOptions);
            var loggerMock = CreateLoggerMock();
            var repo = new PropertyRepository(context, loggerMock.Object);

            // Act
            Func<Task> act = async () => await repo.DeleteAsync(999);

            // Assert
            await act.Should().NotThrowAsync();
        }

        [Fact]
        public async Task GetAllAsync_Should_Return_All_Properties()
        {
            // Arrange
            using var context = new ApplicationDbContext(_dbOptions);
            var loggerMock = CreateLoggerMock();
            var (pt, st) = await SeedDependenciesAsync(context);

            context.Properties.AddRange(
                new Property
                {
                    Id = 0,
                    Code = "P00001",
                    AgentId = "agent-1",
                    PropertyTypeId = pt.Id,
                    SaleTypeId = st.Id,
                    Price = 1000000,
                    LandSize = 90,
                    Bedrooms = 2,
                    Bathrooms = 1,
                    Description = "Propiedad A",
                    Status = PropertyStatus.Available
                },
                new Property
                {
                    Id = 0,
                    Code = "P00002",
                    AgentId = "agent-2",
                    PropertyTypeId = pt.Id,
                    SaleTypeId = st.Id,
                    Price = 2000000,
                    LandSize = 120,
                    Bedrooms = 3,
                    Bathrooms = 2,
                    Description = "Propiedad B",
                    Status = PropertyStatus.Available
                }
            );
            await context.SaveChangesAsync();

            var repo = new PropertyRepository(context, loggerMock.Object);

            // Act
            var result = await repo.GetAllAsync();

            // Assert
            result.Should().HaveCount(2);
        }

        [Fact]
        public async Task GetByCodeAsync_Should_Return_Property_When_Code_Exists()
        {
            // Arrange
            using var context = new ApplicationDbContext(_dbOptions);
            var loggerMock = CreateLoggerMock();
            var (pt, st) = await SeedDependenciesAsync(context);

            var property = new Property
            {
                Id = 0,
                Code = "COD001",
                AgentId = "agent-5",
                PropertyTypeId = pt.Id,
                SaleTypeId = st.Id,
                Price = 4500000,
                LandSize = 200,
                Bedrooms = 4,
                Bathrooms = 3,
                Description = "Propiedad con código único",
                Status = PropertyStatus.Available
            };
            context.Properties.Add(property);
            await context.SaveChangesAsync();

            var repo = new PropertyRepository(context, loggerMock.Object);

            // Act
            var result = await repo.GetByCodeAsync("COD001");

            // Assert
            result.Should().NotBeNull();
            result!.Code.Should().Be("COD001");
        }

        [Fact]
        public async Task GetByCodeAsync_Should_Return_Null_When_Code_NotExists()
        {
            // Arrange
            using var context = new ApplicationDbContext(_dbOptions);
            var loggerMock = CreateLoggerMock();
            var repo = new PropertyRepository(context, loggerMock.Object);

            // Act
            var result = await repo.GetByCodeAsync("ZZZZZZ");

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetByAgentIdAsync_Should_Return_Properties_Of_Agent()
        {
            // Arrange
            using var context = new ApplicationDbContext(_dbOptions);
            var loggerMock = CreateLoggerMock();
            var (pt, st) = await SeedDependenciesAsync(context);

            var agentId = "agent-filter";
            context.Properties.AddRange(
                new Property
                {
                    Id = 0,
                    Code = "A00001",
                    AgentId = agentId,
                    PropertyTypeId = pt.Id,
                    SaleTypeId = st.Id,
                    Price = 1000000,
                    LandSize = 80,
                    Bedrooms = 2,
                    Bathrooms = 1,
                    Description = "Propiedad del agente 1",
                    Status = PropertyStatus.Available
                },
                new Property
                {
                    Id = 0,
                    Code = "A00002",
                    AgentId = agentId,
                    PropertyTypeId = pt.Id,
                    SaleTypeId = st.Id,
                    Price = 2000000,
                    LandSize = 100,
                    Bedrooms = 3,
                    Bathrooms = 2,
                    Description = "Propiedad del agente 2",
                    Status = PropertyStatus.Available
                },
                new Property
                {
                    Id = 0,
                    Code = "B00001",
                    AgentId = "otro-agente",
                    PropertyTypeId = pt.Id,
                    SaleTypeId = st.Id,
                    Price = 3000000,
                    LandSize = 150,
                    Bedrooms = 4,
                    Bathrooms = 3,
                    Description = "Propiedad de otro agente",
                    Status = PropertyStatus.Available
                }
            );
            await context.SaveChangesAsync();

            var repo = new PropertyRepository(context, loggerMock.Object);

            // Act
            var result = await repo.GetByAgentIdAsync(agentId);

            // Assert
            result.Should().HaveCount(2);
            result.Should().AllSatisfy(p => p.AgentId.Should().Be(agentId));
        }

        [Fact]
        public async Task GetByIdWithIncludeAsync_Should_Include_PropertyType_And_SaleType()
        {
            // Arrange
            using var context = new ApplicationDbContext(_dbOptions);
            var loggerMock = CreateLoggerMock();
            var (pt, st) = await SeedDependenciesAsync(context);

            var property = new Property
            {
                Id = 0,
                Code = "INC001",
                AgentId = "agent-6",
                PropertyTypeId = pt.Id,
                SaleTypeId = st.Id,
                Price = 6000000,
                LandSize = 300,
                Bedrooms = 5,
                Bathrooms = 4,
                Description = "Propiedad con includes",
                Status = PropertyStatus.Available
            };
            context.Properties.Add(property);
            await context.SaveChangesAsync();

            var repo = new PropertyRepository(context, loggerMock.Object);

            // Act
            var result = await repo.GetByIdWithIncludeAsync(property.Id);

            // Assert
            result.Should().NotBeNull();
            result!.PropertyType.Should().NotBeNull();
            result.SaleType.Should().NotBeNull();
            result.PropertyType!.Name.Should().Be("Casa");
        }

        [Fact]
        public async Task GetAllWithFiltersAsync_Should_Filter_By_PropertyType()
        {
            // Arrange
            using var context = new ApplicationDbContext(_dbOptions);
            var loggerMock = CreateLoggerMock();
            var (pt, st) = await SeedDependenciesAsync(context);

            var pt2 = new PropertyType { Id = 0, Name = "Apartamento", Description = "Apt" };
            context.PropertyTypes.Add(pt2);
            await context.SaveChangesAsync();

            context.Properties.AddRange(
                new Property
                {
                    Id = 0,
                    Code = "F00001",
                    AgentId = "agent-7",
                    PropertyTypeId = pt.Id,
                    SaleTypeId = st.Id,
                    Price = 1000000,
                    LandSize = 90,
                    Bedrooms = 3,
                    Bathrooms = 2,
                    Description = "Casa filtro",
                    Status = PropertyStatus.Available
                },
                new Property
                {
                    Id = 0,
                    Code = "F00002",
                    AgentId = "agent-7",
                    PropertyTypeId = pt2.Id,
                    SaleTypeId = st.Id,
                    Price = 800000,
                    LandSize = 70,
                    Bedrooms = 2,
                    Bathrooms = 1,
                    Description = "Apartamento filtro",
                    Status = PropertyStatus.Available
                }
            );
            await context.SaveChangesAsync();

            var repo = new PropertyRepository(context, loggerMock.Object);

            // Act
            var result = await repo.GetAllWithFiltersAsync(pt.Id, null, null, null, null);

            // Assert
            result.Should().HaveCount(1);
            result[0].PropertyTypeId.Should().Be(pt.Id);
        }

        [Fact]
        public async Task GetAllWithFiltersAsync_Should_Filter_By_Price_Range()
        {
            // Arrange
            using var context = new ApplicationDbContext(_dbOptions);
            var loggerMock = CreateLoggerMock();
            var (pt, st) = await SeedDependenciesAsync(context);

            context.Properties.AddRange(
                new Property
                {
                    Id = 0,
                    Code = "P10001",
                    AgentId = "agent-8",
                    PropertyTypeId = pt.Id,
                    SaleTypeId = st.Id,
                    Price = 500000,
                    LandSize = 60,
                    Bedrooms = 1,
                    Bathrooms = 1,
                    Description = "Económica",
                    Status = PropertyStatus.Available
                },
                new Property
                {
                    Id = 0,
                    Code = "P10002",
                    AgentId = "agent-8",
                    PropertyTypeId = pt.Id,
                    SaleTypeId = st.Id,
                    Price = 1500000,
                    LandSize = 100,
                    Bedrooms = 3,
                    Bathrooms = 2,
                    Description = "Media",
                    Status = PropertyStatus.Available
                },
                new Property
                {
                    Id = 0,
                    Code = "P10003",
                    AgentId = "agent-8",
                    PropertyTypeId = pt.Id,
                    SaleTypeId = st.Id,
                    Price = 5000000,
                    LandSize = 300,
                    Bedrooms = 5,
                    Bathrooms = 4,
                    Description = "Lujosa",
                    Status = PropertyStatus.Available
                }
            );
            await context.SaveChangesAsync();

            var repo = new PropertyRepository(context, loggerMock.Object);

            // Act
            var result = await repo.GetAllWithFiltersAsync(null, 1000000, 2000000, null, null);

            // Assert
            result.Should().HaveCount(1);
            result[0].Price.Should().Be(1500000);
        }

        [Fact]
        public async Task GetAllWithFiltersAsync_Should_Only_Return_Available_Properties()
        {
            // Arrange
            using var context = new ApplicationDbContext(_dbOptions);
            var loggerMock = CreateLoggerMock();
            var (pt, st) = await SeedDependenciesAsync(context);

            context.Properties.AddRange(
                new Property
                {
                    Id = 0,
                    Code = "AV0001",
                    AgentId = "agent-9",
                    PropertyTypeId = pt.Id,
                    SaleTypeId = st.Id,
                    Price = 2000000,
                    LandSize = 100,
                    Bedrooms = 3,
                    Bathrooms = 2,
                    Description = "Disponible",
                    Status = PropertyStatus.Available
                },
                new Property
                {
                    Id = 0,
                    Code = "SL0001",
                    AgentId = "agent-9",
                    PropertyTypeId = pt.Id,
                    SaleTypeId = st.Id,
                    Price = 2000000,
                    LandSize = 100,
                    Bedrooms = 3,
                    Bathrooms = 2,
                    Description = "Vendida",
                    Status = PropertyStatus.Sold
                }
            );
            await context.SaveChangesAsync();

            var repo = new PropertyRepository(context, loggerMock.Object);

            // Act
            var result = await repo.GetAllWithFiltersAsync(null, null, null, null, null);

            // Assert
            result.Should().HaveCount(1);
            result[0].Status.Should().Be(PropertyStatus.Available);
        }

        [Fact]
        public async Task GetAllQueryWithInclude_Should_Include_PropertyType()
        {
            // Arrange
            using var context = new ApplicationDbContext(_dbOptions);
            var loggerMock = CreateLoggerMock();
            var (pt, st) = await SeedDependenciesAsync(context);

            context.Properties.Add(new Property
            {
                Id = 0,
                Code = "QI0001",
                AgentId = "agent-10",
                PropertyTypeId = pt.Id,
                SaleTypeId = st.Id,
                Price = 3000000,
                LandSize = 150,
                Bedrooms = 3,
                Bathrooms = 2,
                Description = "Query include test",
                Status = PropertyStatus.Available
            });
            await context.SaveChangesAsync();

            var repo = new PropertyRepository(context, loggerMock.Object);

            // Act
            var query = repo.GetAllQueryWithInclude(["PropertyType", "SaleType"]);
            var result = await query.ToListAsync();

            // Assert
            result.Should().NotBeEmpty();
            result[0].PropertyType.Should().NotBeNull();
            result[0].SaleType.Should().NotBeNull();
        }
    }
}