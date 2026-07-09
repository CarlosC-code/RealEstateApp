using AutoMapper;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using RealEstateApp.Core.Application.Features.PropertyTypes.Queries.GetAll;
using RealEstateApp.Core.Application.Mappings.EntitiesAndDtos;
using RealEstateApp.Core.Domain.Entities;
using RealEstateApp.Infrastructure.Persistence.Contexts;
using RealEstateApp.Infrastructure.Persistence.Repositories;

namespace RealEstateApp.Unit.Tests.Features.PropertyTypes
{
    public class GetAllPropertyTypeQueryHandlerTests
    {
        private readonly DbContextOptions<ApplicationDbContext> _dbOptions;
        private readonly IMapper _mapper;

        public GetAllPropertyTypeQueryHandlerTests()
        {
            _dbOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase($"TestDb_GetAllPropertyType_{Guid.NewGuid()}")
                .Options;

            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<PropertyTypeMappingProfile>();
            });

            _mapper = config.CreateMapper();
        }

        [Fact]
        public async Task Handle_Should_Return_All_PropertyTypes()
        {
            // Arrange
            using var context = new ApplicationDbContext(_dbOptions);
            var loggerMoq = new Mock<ILogger<GenericRepository<PropertyType>>>();

            context.PropertyTypes.AddRange(
                new PropertyType { Id = 1, Name = "Casa", Description = "Vivienda independiente" },
                new PropertyType { Id = 2, Name = "Apartamento", Description = "Unidad en edificio" },
                new PropertyType { Id = 3, Name = "Local Comercial", Description = "Espacio para negocio" }
            );
            await context.SaveChangesAsync();

            var repository = new PropertyTypeRepository(context, loggerMoq.Object);
            var handler = new GetAllPropertyTypeQueryHandler(repository, _mapper);

            // Act
            var result = await handler.Handle(new GetAllPropertyTypeQuery(), CancellationToken.None);

            // Assert
            result.Should().HaveCount(3);
            result.Select(pt => pt.Name).Should().Contain(new[] { "Casa", "Apartamento", "Local Comercial" });
        }

        [Fact]
        public async Task Handle_Should_Return_Empty_List_When_No_PropertyTypes_Exist()
        {
            // Arrange
            using var context = new ApplicationDbContext(_dbOptions);
            var loggerMoq = new Mock<ILogger<GenericRepository<PropertyType>>>();
            var repository = new PropertyTypeRepository(context, loggerMoq.Object);
            var handler = new GetAllPropertyTypeQueryHandler(repository, _mapper);

            // Act
            var result = await handler.Handle(new GetAllPropertyTypeQuery(), CancellationToken.None);

            // Assert
            result.Should().BeEmpty();
        }
    }
}