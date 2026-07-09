using AutoMapper;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using RealEstateApp.Core.Application.Features.PropertyTypes.Queries.GetById;
using RealEstateApp.Core.Application.Mappings.EntitiesAndDtos;
using RealEstateApp.Core.Domain.Entities;
using RealEstateApp.Infrastructure.Persistence.Contexts;
using RealEstateApp.Infrastructure.Persistence.Repositories;

namespace RealEstateApp.Unit.Tests.Features.PropertyTypes
{
    public class GetPropertyTypeByIdQueryHandlerTests
    {
        private readonly DbContextOptions<ApplicationDbContext> _dbOptions;
        private readonly IMapper _mapper;

        public GetPropertyTypeByIdQueryHandlerTests()
        {
            _dbOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase($"TestDb_GetPropertyTypeById_{Guid.NewGuid()}")
                .Options;

            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<PropertyTypeMappingProfile>();
            });

            _mapper = config.CreateMapper();
        }

        [Fact]
        public async Task Handle_Should_Return_PropertyTypeDto_When_PropertyType_Exists()
        {
            // Arrange
            using var context = new ApplicationDbContext(_dbOptions);
            var loggerMoq = new Mock<ILogger<GenericRepository<PropertyType>>>();

            var propertyType = new PropertyType { Id = 1, Name = "Villa", Description = "Propiedad de lujo con jardín" };
            context.PropertyTypes.Add(propertyType);
            await context.SaveChangesAsync();

            var repository = new PropertyTypeRepository(context, loggerMoq.Object);
            var handler = new GetPropertyTypeByIdQueryHandler(repository, _mapper);

            // Act
            var result = await handler.Handle(new GetPropertyTypeByIdQuery { Id = 1 }, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result!.Id.Should().Be(1);
            result.Name.Should().Be("Villa");
            result.Description.Should().Be("Propiedad de lujo con jardín");
        }

        [Fact]
        public async Task Handle_Should_Return_Null_When_PropertyType_Does_Not_Exist()
        {
            // Arrange
            using var context = new ApplicationDbContext(_dbOptions);
            var loggerMoq = new Mock<ILogger<GenericRepository<PropertyType>>>();
            var repository = new PropertyTypeRepository(context, loggerMoq.Object);
            var handler = new GetPropertyTypeByIdQueryHandler(repository, _mapper);

            // Act
            var result = await handler.Handle(new GetPropertyTypeByIdQuery { Id = 999 }, CancellationToken.None);

            // Assert
            result.Should().BeNull();
        }
    }
}