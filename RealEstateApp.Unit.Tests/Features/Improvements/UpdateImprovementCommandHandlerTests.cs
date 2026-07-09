using AutoMapper;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using RealEstateApp.Core.Application.Features.Improvements.Commands.Update;
using RealEstateApp.Core.Application.Mappings.EntitiesAndDtos;
using RealEstateApp.Core.Domain.Entities;
using RealEstateApp.Infrastructure.Persistence.Contexts;
using RealEstateApp.Infrastructure.Persistence.Repositories;

namespace RealEstateApp.Unit.Tests.Features.Improvements
{
    public class UpdateImprovementCommandHandlerTests
    {
        private readonly DbContextOptions<ApplicationDbContext> _dbOptions;
        private readonly IMapper _mapper;

        public UpdateImprovementCommandHandlerTests()
        {
            _dbOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase($"TestDb_UpdateImprovement_{Guid.NewGuid()}")
                .Options;

            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<ImprovementMappingProfile>();
            });

            _mapper = config.CreateMapper();
        }

        [Fact]
        public async Task Handle_Should_Return_Updated_ImprovementDto_When_Improvement_Exists()
        {
            // Arrange
            using var context = new ApplicationDbContext(_dbOptions);
            var loggerMoq = new Mock<ILogger<GenericRepository<Improvement>>>();

            var improvement = new Improvement { Id = 1, Name = "Piscina", Description = "Descripción original" };
            context.Improvements.Add(improvement);
            await context.SaveChangesAsync();

            var repository = new ImprovementRepository(context, loggerMoq.Object);
            var handler = new UpdateImprovementCommandHandler(repository, _mapper);

            var command = new UpdateImprovementCommand
            {
                Id = 1,
                Name = "Piscina Renovada",
                Description = "Descripción actualizada"
            };

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result!.Name.Should().Be(command.Name);
            result.Description.Should().Be(command.Description);

            var updated = await context.Improvements.FindAsync(1);
            updated!.Name.Should().Be(command.Name);
        }

        [Fact]
        public async Task Handle_Should_Return_Null_When_Improvement_Does_Not_Exist()
        {
            // Arrange
            using var context = new ApplicationDbContext(_dbOptions);
            var loggerMoq = new Mock<ILogger<GenericRepository<Improvement>>>();
            var repository = new ImprovementRepository(context, loggerMoq.Object);
            var handler = new UpdateImprovementCommandHandler(repository, _mapper);

            var command = new UpdateImprovementCommand
            {
                Id = 999,
                Name = "No existe",
                Description = "Descripción"
            };

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().BeNull();
        }
    }
}