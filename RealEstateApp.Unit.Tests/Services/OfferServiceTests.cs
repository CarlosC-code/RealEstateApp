using AutoMapper;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using RealEstateApp.Core.Application.Dtos.Offer;
using RealEstateApp.Core.Application.Mappings.EntitiesAndDtos;
using RealEstateApp.Core.Application.Services;
using RealEstateApp.Core.Domain.Entities;
using RealEstateApp.Core.Domain.Enums;
using RealEstateApp.Infrastructure.Persistence.Contexts;
using RealEstateApp.Infrastructure.Persistence.Repositories;

namespace RealEstateApp.Unit.Tests.Services
{
    public class OfferServiceTests
    {
        private readonly DbContextOptions<ApplicationDbContext> _dbOptions;
        private readonly IMapper _mapper;

        public OfferServiceTests()
        {
            _dbOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase($"TestDb_OfferService_{Guid.NewGuid()}")
                .Options;

            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<OfferMappingProfile>();
                cfg.AddProfile<PropertyMappingProfile>();
            });

            _mapper = config.CreateMapper();
        }

        private async Task<Property> SeedPropertyAsync(ApplicationDbContext context)
        {
            var pt = new PropertyType { Id = 0, Name = "Casa", Description = "Vivienda" };
            var st = new SaleType { Id = 0, Name = "Venta", Description = "Directa" };
            context.PropertyTypes.Add(pt);
            context.SaleTypes.Add(st);
            await context.SaveChangesAsync();

            var property = new Property
            {
                Id = 0,
                Code = $"P{Guid.NewGuid().ToString()[..5].ToUpper()}",
                AgentId = "agent-1",
                PropertyTypeId = pt.Id,
                SaleTypeId = st.Id,
                Price = 5000000,
                LandSize = 200,
                Bedrooms = 3,
                Bathrooms = 2,
                Description = "Propiedad semilla",
                Status = PropertyStatus.Available
            };
            context.Properties.Add(property);
            await context.SaveChangesAsync();
            return property;
        }

        private OfferService CreateService()
        {
            var factoryMoq = new Mock<ILoggerFactory>();
            factoryMoq.Setup(x => x.CreateLogger(It.IsAny<string>()))
                .Returns(new NullLogger<OfferService>());

            var context = new ApplicationDbContext(_dbOptions);
            var offerRepo = new OfferRepository(context, new NullLogger<GenericRepository<Offer>>());
            var propertyRepo = new PropertyRepository(context, new NullLogger<GenericRepository<Property>>());
            return new OfferService(offerRepo, propertyRepo, _mapper, factoryMoq.Object);
        }

        [Fact]
        public async Task AddAsync_Should_Create_Offer_With_Pending_Status()
        {
            // Arrange
            using var context = new ApplicationDbContext(_dbOptions);
            var property = await SeedPropertyAsync(context);
            var service = CreateService();

            var dto = new OfferDto
            {
                Id = 0,
                PropertyId = property.Id,
                ClientId = "client-1",
                Amount = 4500000
            };

            // Act
            var result = await service.AddAsync(dto);

            // Assert
            result.Should().NotBeNull();
            result!.Amount.Should().Be(4500000);
            result.Status.Should().Be(OfferStatus.Pending.ToString());
        }

        [Fact]
        public async Task AddAsync_Should_Return_Null_When_Property_Has_Accepted_Offer()
        {
            // Arrange
            using var context = new ApplicationDbContext(_dbOptions);
            var property = await SeedPropertyAsync(context);

            context.Offers.Add(new Offer
            {
                Id = 0,
                PropertyId = property.Id,
                ClientId = "client-accepted",
                Amount = 5000000,
                OfferDate = DateTime.UtcNow,
                Status = OfferStatus.Accepted
            });
            await context.SaveChangesAsync();

            var service = CreateService();

            var dto = new OfferDto
            {
                Id = 0,
                PropertyId = property.Id,
                ClientId = "client-new",
                Amount = 4800000
            };

            // Act
            var result = await service.AddAsync(dto);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task AddAsync_Should_Return_Null_When_Client_Has_Pending_Offer()
        {
            // Arrange
            using var context = new ApplicationDbContext(_dbOptions);
            var property = await SeedPropertyAsync(context);

            context.Offers.Add(new Offer
            {
                Id = 0,
                PropertyId = property.Id,
                ClientId = "client-pending",
                Amount = 4000000,
                OfferDate = DateTime.UtcNow,
                Status = OfferStatus.Pending
            });
            await context.SaveChangesAsync();

            var service = CreateService();

            var dto = new OfferDto
            {
                Id = 0,
                PropertyId = property.Id,
                ClientId = "client-pending",
                Amount = 4200000
            };

            // Act
            var result = await service.AddAsync(dto);

            // Assert
            result.Should().BeNull();
        }

     

      

        [Fact]
        public async Task AcceptAsync_Should_Do_Nothing_When_Offer_NotExists()
        {
            // Arrange
            var service = CreateService();

            // Act
            Func<Task> act = async () => await service.AcceptAsync(999);

            // Assert
            await act.Should().NotThrowAsync();
        }

        [Fact]
        public async Task RejectAsync_Should_Not_Reject_Already_Accepted_Offer()
        {
            // Arrange
            using var context = new ApplicationDbContext(_dbOptions);
            var property = await SeedPropertyAsync(context);

            var offer = new Offer { Id = 0, PropertyId = property.Id, ClientId = "client-1", Amount = 5000000, OfferDate = DateTime.UtcNow, Status = OfferStatus.Accepted };
            context.Offers.Add(offer);
            await context.SaveChangesAsync();

            var service = CreateService();

            // Act
            await service.RejectAsync(offer.Id);

            // Assert
            // Estado debe seguir siendo Accepted, no cambiar a Rejected
            var unchanged = await context.Offers.FindAsync(offer.Id);
            unchanged!.Status.Should().Be(OfferStatus.Accepted);
        }

        [Fact]
        public async Task GetByPropertyIdAsync_Should_Return_Offers_Of_Property()
        {
            // Arrange
            using var context = new ApplicationDbContext(_dbOptions);
            var property = await SeedPropertyAsync(context);

            context.Offers.AddRange(
                new Offer { Id = 0, PropertyId = property.Id, ClientId = "c1", Amount = 3000000, OfferDate = DateTime.UtcNow, Status = OfferStatus.Pending },
                new Offer { Id = 0, PropertyId = property.Id, ClientId = "c2", Amount = 3200000, OfferDate = DateTime.UtcNow, Status = OfferStatus.Pending }
            );
            await context.SaveChangesAsync();

            var service = CreateService();

            // Act
            var result = await service.GetByPropertyIdAsync(property.Id);

            // Assert
            result.Should().HaveCount(2);
            result.Should().AllSatisfy(o => o.PropertyId.Should().Be(property.Id));
        }

        [Fact]
        public async Task GetByClientIdAsync_Should_Return_Offers_Of_Client()
        {
            // Arrange
            using var context = new ApplicationDbContext(_dbOptions);
            var property = await SeedPropertyAsync(context);

            var clientId = "client-especifico";
            context.Offers.AddRange(
                new Offer { Id = 0, PropertyId = property.Id, ClientId = clientId, Amount = 4000000, OfferDate = DateTime.UtcNow, Status = OfferStatus.Pending },
                new Offer { Id = 0, PropertyId = property.Id, ClientId = "otro-cliente", Amount = 3800000, OfferDate = DateTime.UtcNow, Status = OfferStatus.Pending }
            );
            await context.SaveChangesAsync();

            var service = CreateService();

            // Act
            var result = await service.GetByClientIdAsync(clientId);

            // Assert
            result.Should().HaveCount(1);
            result[0].ClientId.Should().Be(clientId);
        }

        [Fact]
        public async Task GetAll_Should_Return_All_Offers()
        {
            // Arrange
            using var context = new ApplicationDbContext(_dbOptions);
            var property = await SeedPropertyAsync(context);

            context.Offers.AddRange(
                new Offer { Id = 0, PropertyId = property.Id, ClientId = "c1", Amount = 1000000, OfferDate = DateTime.UtcNow, Status = OfferStatus.Pending },
                new Offer { Id = 0, PropertyId = property.Id, ClientId = "c2", Amount = 1100000, OfferDate = DateTime.UtcNow, Status = OfferStatus.Rejected },
                new Offer { Id = 0, PropertyId = property.Id, ClientId = "c3", Amount = 1200000, OfferDate = DateTime.UtcNow, Status = OfferStatus.Accepted }
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