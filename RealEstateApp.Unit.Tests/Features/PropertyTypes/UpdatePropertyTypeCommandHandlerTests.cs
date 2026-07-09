using AutoMapper;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using RealEstateApp.Core.Application.Features.PropertyTypes.Commands.Update;
using RealEstateApp.Core.Application.Mappings.EntitiesAndDtos;
using RealEstateApp.Core.Domain.Entities;
using RealEstateApp.Infrastructure.Persistence.Contexts;
using RealEstateApp.Infrastructure.Persistence.Repositories;

namespace RealEstateApp.Unit.Tests.Features.PropertyTypes
{
    public class UpdatePropertyTypeCommandHandlerTests
    {
        private readonly DbContextOptions<ApplicationDbContext> _dbOptions;
        private readonly IMapper _mapper;

        public UpdatePropertyTypeCommandHandlerTests()
        {
            _dbOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase($"TestDb_UpdatePropertyType_{Guid.NewGuid()}")
                .Options;

            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<PropertyTypeMappingProfile>();
            });

            _mapper = config.CreateMapper();
        }

        [Fact]
        public async Task Handle_Should_Return_Updated_PropertyTypeDto_When_PropertyType_Exists()
        {
            // Arrange
            using var context = new ApplicationDbContext(_dbOptions);
            var loggerMoq = new Mock<ILogger<GenericRepository<PropertyType>>>();

            var propertyType = new PropertyType { Id = 1, Name = "Apartamento", Description = "Descripción original" };
            context.PropertyTypes.Add(propertyType);
            await context.SaveChangesAsync();

            var repository = new PropertyTypeRepository(context, loggerMoq.Object);
            var handler = new UpdatePropertyTypeCommandHandler(repository, _mapper);

            var command = new UpdatePropertyTypeCommand
            {
                Id = 1,
                Name = "Apartamento Actualizado",
                Description = "Descripción actualizada"
            };

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result!.Name.Should().Be(command.Name);
            result.Description.Should().Be(command.Description);

            var updated = await context.PropertyTypes.FindAsync(1);
            updated!.Name.Should().Be(command.Name);
        }

        [Fact]
        public async Task Handle_Should_Return_Null_When_PropertyType_Does_Not_Exist()
        {
            // Arrange
            using var context = new ApplicationDbContext(_dbOptions);
            var loggerMoq = new Mock<ILogger<GenericRepository<PropertyType>>>();
            var repository = new PropertyTypeRepository(context, loggerMoq.Object);
            var handler = new UpdatePropertyTypeCommandHandler(repository, _mapper);

            var command = new UpdatePropertyTypeCommand
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