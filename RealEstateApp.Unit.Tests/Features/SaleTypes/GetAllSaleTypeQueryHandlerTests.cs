using AutoMapper;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using RealEstateApp.Core.Application.Features.SaleTypes.Queries.GetAll;
using RealEstateApp.Core.Application.Mappings.EntitiesAndDtos;
using RealEstateApp.Core.Domain.Entities;
using RealEstateApp.Infrastructure.Persistence.Contexts;
using RealEstateApp.Infrastructure.Persistence.Repositories;

namespace RealEstateApp.Unit.Tests.Features.SaleTypes
{
    public class GetAllSaleTypeQueryHandlerTests
    {
        private readonly DbContextOptions<ApplicationDbContext> _dbOptions;
        private readonly IMapper _mapper;

        public GetAllSaleTypeQueryHandlerTests()
        {
            _dbOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase($"TestDb_GetAllSaleType_{Guid.NewGuid()}")
                .Options;

            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<SaleTypeMappingProfile>();
            });

            _mapper = config.CreateMapper();
        }

        [Fact]
        public async Task Handle_Should_Return_All_SaleTypes()
        {
            // Arrange
            using var context = new ApplicationDbContext(_dbOptions);
            var loggerMoq = new Mock<ILogger<GenericRepository<SaleType>>>();

            context.SaleTypes.AddRange(
                new SaleType { Id = 1, Name = "Venta", Description = "Compra definitiva" },
                new SaleType { Id = 2, Name = "Alquiler", Description = "Arrendamiento mensual" },
                new SaleType { Id = 3, Name = "Alquiler Amueblado", Description = "Arrendamiento con mobiliario" }
            );
            await context.SaveChangesAsync();

            var repository = new SaleTypeRepository(context, loggerMoq.Object);
            var handler = new GetAllSaleTypeQueryHandler(repository, _mapper);

            // Act
            var result = await handler.Handle(new GetAllSaleTypeQuery(), CancellationToken.None);

            // Assert
            result.Should().HaveCount(3);
            result.Select(st => st.Name).Should().Contain(new[] { "Venta", "Alquiler", "Alquiler Amueblado" });
        }

        [Fact]
        public async Task Handle_Should_Return_Empty_List_When_No_SaleTypes_Exist()
        {
            // Arrange
            using var context = new ApplicationDbContext(_dbOptions);
            var loggerMoq = new Mock<ILogger<GenericRepository<SaleType>>>();
            var repository = new SaleTypeRepository(context, loggerMoq.Object);
            var handler = new GetAllSaleTypeQueryHandler(repository, _mapper);

            // Act
            var result = await handler.Handle(new GetAllSaleTypeQuery(), CancellationToken.None);

            // Assert
            result.Should().BeEmpty();
        }
    }
}