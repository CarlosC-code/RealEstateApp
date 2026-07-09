using AutoMapper;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using RealEstateApp.Core.Application.Features.Improvements.Queries.GetById;
using RealEstateApp.Core.Application.Mappings.EntitiesAndDtos;
using RealEstateApp.Core.Domain.Entities;
using RealEstateApp.Infrastructure.Persistence.Contexts;
using RealEstateApp.Infrastructure.Persistence.Repositories;

namespace RealEstateApp.Unit.Tests.Features.Improvements
{
    public class GetImprovementByIdQueryHandlerTests
    {
        private readonly DbContextOptions<ApplicationDbContext> _dbOptions;
        private readonly IMapper _mapper;

        public GetImprovementByIdQueryHandlerTests()
        {
            _dbOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase($"TestDb_GetImprovementById_{Guid.NewGuid()}")
                .Options;

            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<ImprovementMappingProfile>();
            });

            _mapper = config.CreateMapper();
        }

        [Fact]
        public async Task Handle_Should_Return_ImprovementDto_When_Improvement_Exists()
        {
            // Arrange
            using var context = new ApplicationDbContext(_dbOptions);
            var loggerMoq = new Mock<ILogger<GenericRepository<Improvement>>>();

            var improvement = new Improvement { Id = 1, Name = "Terraza", Description = "Terraza techada" };
            context.Improvements.Add(improvement);
            await context.SaveChangesAsync();

            var repository = new ImprovementRepository(context, loggerMoq.Object);
            var handler = new GetImprovementByIdQueryHandler(repository, _mapper);

            // Act
            var result = await handler.Handle(new GetImprovementByIdQuery { Id = 1 }, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result!.Id.Should().Be(1);
            result.Name.Should().Be("Terraza");
            result.Description.Should().Be("Terraza techada");
        }

        [Fact]
        public async Task Handle_Should_Return_Null_When_Improvement_Does_Not_Exist()
        {
            // Arrange
            using var context = new ApplicationDbContext(_dbOptions);
            var loggerMoq = new Mock<ILogger<GenericRepository<Improvement>>>();
            var repository = new ImprovementRepository(context, loggerMoq.Object);
            var handler = new GetImprovementByIdQueryHandler(repository, _mapper);

            // Act
            var result = await handler.Handle(new GetImprovementByIdQuery { Id = 999 }, CancellationToken.None);

            // Assert
            result.Should().BeNull();
        }
    }
}