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
    public class OfferRepositoryTests
    {
        private readonly DbContextOptions<ApplicationDbContext> _dbOptions;

        public OfferRepositoryTests()
        {
            _dbOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: $"TestDb_Offer_{Guid.NewGuid()}")
                .Options;
        }

        private Mock<ILogger<GenericRepository<Offer>>> CreateLoggerMock()
        {
            return new Mock<ILogger<GenericRepository<Offer>>>();
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
                Code = $"P{Guid.NewGuid().ToString()[..5].ToUpper()}",
                AgentId = "agent-seed",
                PropertyTypeId = pt.Id,
                SaleTypeId = st.Id,
                Price = 3000000,
                LandSize = 150,
                Bedrooms = 3,
                Bathrooms = 2,
                Description = "Propiedad semilla",
                Status = PropertyStatus.Available
            };
            context.Properties.Add(property);
            await context.SaveChangesAsync();
            return property;
        }

        [Fact]
        public async Task AddAsync_Should_Add_Offer_To_Database()
        {
            // Arrange
            using var context = new ApplicationDbContext(_dbOptions);
            var loggerMock = CreateLoggerMock();
            var property = await SeedPropertyAsync(context);

            var repo = new OfferRepository(context, loggerMock.Object);

            var offer = new Offer
            {
                Id = 0,
                PropertyId = property.Id,
                ClientId = "client-1",
                Amount = 2800000,
                OfferDate = DateTime.UtcNow,
                Status = OfferStatus.Pending
            };

            // Act
            var result = await repo.AddAsync(offer);

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
            var repo = new OfferRepository(context, loggerMock.Object);

            // Act
            Func<Task> act = async () => await repo.AddAsync(null!);

            // Assert
            await act.Should().ThrowAsync<Exception>();
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Offer_When_Exists()
        {
            // Arrange
            using var context = new ApplicationDbContext(_dbOptions);
            var loggerMock = CreateLoggerMock();
            var property = await SeedPropertyAsync(context);

            var offer = new Offer
            {
                Id = 0,
                PropertyId = property.Id,
                ClientId = "client-2",
                Amount = 2500000,
                OfferDate = DateTime.UtcNow,
                Status = OfferStatus.Pending
            };
            context.Offers.Add(offer);
            await context.SaveChangesAsync();

            var repo = new OfferRepository(context, loggerMock.Object);

            // Act
            var result = await repo.GetByIdAsync(offer.Id);

            // Assert
            result.Should().NotBeNull();
            result!.Amount.Should().Be(2500000);
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_When_NotExists()
        {
            // Arrange
            using var context = new ApplicationDbContext(_dbOptions);
            var loggerMock = CreateLoggerMock();
            var repo = new OfferRepository(context, loggerMock.Object);

            // Act
            var result = await repo.GetByIdAsync(999);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task UpdateAsync_Should_Update_Offer_Status()
        {
            // Arrange
            using var context = new ApplicationDbContext(_dbOptions);
            var loggerMock = CreateLoggerMock();
            var property = await SeedPropertyAsync(context);

            var offer = new Offer
            {
                Id = 0,
                PropertyId = property.Id,
                ClientId = "client-3",
                Amount = 3000000,
                OfferDate = DateTime.UtcNow,
                Status = OfferStatus.Pending
            };
            context.Offers.Add(offer);
            await context.SaveChangesAsync();

            var repo = new OfferRepository(context, loggerMock.Object);
            offer.Status = OfferStatus.Accepted;

            // Act
            var updated = await repo.UpdateAsync(offer.Id, offer);

            // Assert
            updated.Should().NotBeNull();
            updated!.Status.Should().Be(OfferStatus.Accepted);
        }

        [Fact]
        public async Task UpdateAsync_Should_Return_Null_When_NotExists()
        {
            // Arrange
            using var context = new ApplicationDbContext(_dbOptions);
            var loggerMock = CreateLoggerMock();
            var repo = new OfferRepository(context, loggerMock.Object);

            var fake = new Offer
            {
                Id = 999,
                PropertyId = 1,
                ClientId = "fake-client",
                Amount = 999,
                OfferDate = DateTime.UtcNow,
                Status = OfferStatus.Pending
            };

            // Act
            var result = await repo.UpdateAsync(fake.Id, fake);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task DeleteAsync_Should_Remove_Offer()
        {
            // Arrange
            using var context = new ApplicationDbContext(_dbOptions);
            var loggerMock = CreateLoggerMock();
            var property = await SeedPropertyAsync(context);

            var offer = new Offer
            {
                Id = 0,
                PropertyId = property.Id,
                ClientId = "client-4",
                Amount = 1500000,
                OfferDate = DateTime.UtcNow,
                Status = OfferStatus.Pending
            };
            context.Offers.Add(offer);
            await context.SaveChangesAsync();

            var repo = new OfferRepository(context, loggerMock.Object);

            // Act
            await repo.DeleteAsync(offer.Id);
            var result = await repo.GetByIdAsync(offer.Id);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task DeleteAsync_Should_Not_Throw_When_Id_NotExists()
        {
            // Arrange
            using var context = new ApplicationDbContext(_dbOptions);
            var loggerMock = CreateLoggerMock();
            var repo = new OfferRepository(context, loggerMock.Object);

            // Act
            Func<Task> act = async () => await repo.DeleteAsync(999);

            // Assert
            await act.Should().NotThrowAsync();
        }

        [Fact]
        public async Task GetByPropertyIdAsync_Should_Return_Offers_Of_Property()
        {
            // Arrange
            using var context = new ApplicationDbContext(_dbOptions);
            var loggerMock = CreateLoggerMock();
            var property = await SeedPropertyAsync(context);

            context.Offers.AddRange(
                new Offer { Id = 0, PropertyId = property.Id, ClientId = "client-a", Amount = 2000000, OfferDate = DateTime.UtcNow, Status = OfferStatus.Pending },
                new Offer { Id = 0, PropertyId = property.Id, ClientId = "client-b", Amount = 2100000, OfferDate = DateTime.UtcNow, Status = OfferStatus.Pending },
                new Offer { Id = 0, PropertyId = 9999, ClientId = "client-c", Amount = 1500000, OfferDate = DateTime.UtcNow, Status = OfferStatus.Pending }
            );
            await context.SaveChangesAsync();

            var repo = new OfferRepository(context, loggerMock.Object);

            // Act
            var result = await repo.GetByPropertyIdAsync(property.Id);

            // Assert
            result.Should().HaveCount(2);
            result.Should().AllSatisfy(o => o.PropertyId.Should().Be(property.Id));
        }

        [Fact]
        public async Task GetByClientIdAsync_Should_Return_Offers_Of_Client()
        {
            // Arrange
            using var context = new ApplicationDbContext(_dbOptions);
            var loggerMock = CreateLoggerMock();
            var property = await SeedPropertyAsync(context);

            var clientId = "client-especifico";
            context.Offers.AddRange(
                new Offer { Id = 0, PropertyId = property.Id, ClientId = clientId, Amount = 2000000, OfferDate = DateTime.UtcNow, Status = OfferStatus.Pending },
                new Offer { Id = 0, PropertyId = property.Id, ClientId = clientId, Amount = 2200000, OfferDate = DateTime.UtcNow, Status = OfferStatus.Rejected },
                new Offer { Id = 0, PropertyId = property.Id, ClientId = "otro-cliente", Amount = 1900000, OfferDate = DateTime.UtcNow, Status = OfferStatus.Pending }
            );
            await context.SaveChangesAsync();

            var repo = new OfferRepository(context, loggerMock.Object);

            // Act
            var result = await repo.GetByClientIdAsync(clientId);

            // Assert
            result.Should().HaveCount(2);
            result.Should().AllSatisfy(o => o.ClientId.Should().Be(clientId));
        }

        [Fact]
        public async Task GetAllAsync_Should_Return_All_Offers()
        {
            // Arrange
            using var context = new ApplicationDbContext(_dbOptions);
            var loggerMock = CreateLoggerMock();
            var property = await SeedPropertyAsync(context);

            context.Offers.AddRange(
                new Offer { Id = 0, PropertyId = property.Id, ClientId = "c1", Amount = 1000000, OfferDate = DateTime.UtcNow, Status = OfferStatus.Pending },
                new Offer { Id = 0, PropertyId = property.Id, ClientId = "c2", Amount = 1100000, OfferDate = DateTime.UtcNow, Status = OfferStatus.Accepted }
            );
            await context.SaveChangesAsync();

            var repo = new OfferRepository(context, loggerMock.Object);

            // Act
            var result = await repo.GetAllAsync();

            // Assert
            result.Should().HaveCount(2);
        }
    }
}