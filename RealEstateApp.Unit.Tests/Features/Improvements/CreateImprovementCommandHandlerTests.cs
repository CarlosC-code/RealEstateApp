using AutoMapper;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using RealEstateApp.Core.Application.Features.Improvements.Commands.Create;
using RealEstateApp.Core.Application.Mappings.EntitiesAndDtos;
using RealEstateApp.Infrastructure.Persistence.Contexts;
using RealEstateApp.Infrastructure.Persistence.Repositories;

namespace RealEstateApp.Unit.Tests.Features.Improvements
{
    public class CreateImprovementCommandHandlerTests
    {
        private readonly DbContextOptions<ApplicationDbContext> _dbOptions;
        private readonly IMapper _mapper;

        public CreateImprovementCommandHandlerTests()
        {
            _dbOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase($"TestDb_CreateImprovement_{Guid.NewGuid()}")
                .Options;

            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<ImprovementMappingProfile>();
            });

            _mapper = config.CreateMapper();
        }

        [Fact]
        public async Task Handle_Should_Return_ImprovementDto_When_Improvement_Is_Created()
        {
            // Arrange
            using var context = new ApplicationDbContext(_dbOptions);
            var loggerMoq = new Mock<ILogger<GenericRepository<Core.Domain.Entities.Improvement>>>();
            var repository = new ImprovementRepository(context, loggerMoq.Object);
            var handler = new CreateImprovementCommandHandler(repository, _mapper);

            var command = new CreateImprovementCommand
            {
                Name = "Piscina",
                Description = "Piscina temperada de uso exclusivo"
            };

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result!.Id.Should().BeGreaterThan(0);
            result.Name.Should().Be(command.Name);
            result.Description.Should().Be(command.Description);

            var created = await context.Improvements.FindAsync(result.Id);
            created.Should().NotBeNull();
            created!.Name.Should().Be(command.Name);
        }

        [Fact]
        public async Task Handle_Should_Persist_Improvement_In_Database()
        {
            // Arrange
            using var context = new ApplicationDbContext(_dbOptions);
            var loggerMoq = new Mock<ILogger<GenericRepository<Core.Domain.Entities.Improvement>>>();
            var repository = new ImprovementRepository(context, loggerMoq.Object);
            var handler = new CreateImprovementCommandHandler(repository, _mapper);

            var command = new CreateImprovementCommand
            {
                Name = "Balcón",
                Description = "Balcón con vista al mar"
            };

            // Act
            await handler.Handle(command, CancellationToken.None);

            // Assert
            var count = await context.Improvements.CountAsync();
            count.Should().Be(1);
        }
    }
}