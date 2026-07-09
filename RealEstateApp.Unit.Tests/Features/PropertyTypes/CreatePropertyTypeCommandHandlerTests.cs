using AutoMapper;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using RealEstateApp.Core.Application.Features.PropertyTypes.Commands.Create;
using RealEstateApp.Core.Application.Mappings.EntitiesAndDtos;
using RealEstateApp.Infrastructure.Persistence.Contexts;
using RealEstateApp.Infrastructure.Persistence.Repositories;

namespace RealEstateApp.Unit.Tests.Features.PropertyTypes
{
    public class CreatePropertyTypeCommandHandlerTests
    {
        private readonly DbContextOptions<ApplicationDbContext> _dbOptions;
        private readonly IMapper _mapper;

        public CreatePropertyTypeCommandHandlerTests()
        {
            _dbOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase($"TestDb_CreatePropertyType_{Guid.NewGuid()}")
                .Options;

            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<PropertyTypeMappingProfile>();
            });

            _mapper = config.CreateMapper();
        }

        [Fact]
        public async Task Handle_Should_Return_PropertyTypeDto_When_PropertyType_Is_Created()
        {
            // Arrange
            using var context = new ApplicationDbContext(_dbOptions);
            var loggerMoq = new Mock<ILogger<GenericRepository<Core.Domain.Entities.PropertyType>>>();
            var repository = new PropertyTypeRepository(context, loggerMoq.Object);
            var handler = new CreatePropertyTypeCommandHandler(repository, _mapper);

            var command = new CreatePropertyTypeCommand
            {
                Name = "Apartamento",
                Description = "Unidad residencial en edificio"
            };

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result!.Id.Should().BeGreaterThan(0);
            result.Name.Should().Be(command.Name);
            result.Description.Should().Be(command.Description);

            var created = await context.PropertyTypes.FindAsync(result.Id);
            created.Should().NotBeNull();
            created!.Name.Should().Be(command.Name);
        }

        [Fact]
        public async Task Handle_Should_Persist_PropertyType_In_Database()
        {
            // Arrange
            using var context = new ApplicationDbContext(_dbOptions);
            var loggerMoq = new Mock<ILogger<GenericRepository<Core.Domain.Entities.PropertyType>>>();
            var repository = new PropertyTypeRepository(context, loggerMoq.Object);
            var handler = new CreatePropertyTypeCommandHandler(repository, _mapper);

            var command = new CreatePropertyTypeCommand
            {
                Name = "Casa",
                Description = "Vivienda unifamiliar independiente"
            };

            // Act
            await handler.Handle(command, CancellationToken.None);

            // Assert
            var count = await context.PropertyTypes.CountAsync();
            count.Should().Be(1);
        }
    }
}