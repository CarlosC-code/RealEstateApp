using AutoMapper;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using RealEstateApp.Core.Application.Features.Improvements.Queries.GetAll;
using RealEstateApp.Core.Application.Mappings.EntitiesAndDtos;
using RealEstateApp.Core.Domain.Entities;
using RealEstateApp.Infrastructure.Persistence.Contexts;
using RealEstateApp.Infrastructure.Persistence.Repositories;

namespace RealEstateApp.Unit.Tests.Features.Improvements
{
    public class GetAllImprovementQueryHandlerTests
    {
        private readonly DbContextOptions<ApplicationDbContext> _dbOptions;
        private readonly IMapper _mapper;

        public GetAllImprovementQueryHandlerTests()
        {
            _dbOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase($"TestDb_GetAllImprovement_{Guid.NewGuid()}")
                .Options;

            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<ImprovementMappingProfile>();
            });

            _mapper = config.CreateMapper();
        }

        [Fact]
        public async Task Handle_Should_Return_All_Improvements()
        {
            // Arrange
            using var context = new ApplicationDbContext(_dbOptions);
            var loggerMoq = new Mock<ILogger<GenericRepository<Improvement>>>();

            context.Improvements.AddRange(
                new Improvement { Id = 1, Name = "Piscina", Description = "Piscina climatizada" },
                new Improvement { Id = 2, Name = "Gimnasio", Description = "Gimnasio con equipos" },
                new Improvement { Id = 3, Name = "Balcón", Description = "Balcón con vista al mar" }
            );
            await context.SaveChangesAsync();

            var repository = new ImprovementRepository(context, loggerMoq.Object);
            var handler = new GetAllImprovementQueryHandler(repository, _mapper);

            // Act
            var result = await handler.Handle(new GetAllImprovementQuery(), CancellationToken.None);

            // Assert
            result.Should().HaveCount(3);
            result.Select(i => i.Name).Should().Contain(new[] { "Piscina", "Gimnasio", "Balcón" });
        }

        [Fact]
        public async Task Handle_Should_Return_Empty_List_When_No_Improvements_Exist()
        {
            // Arrange
            using var context = new ApplicationDbContext(_dbOptions);
            var loggerMoq = new Mock<ILogger<GenericRepository<Improvement>>>();
            var repository = new ImprovementRepository(context, loggerMoq.Object);
            var handler = new GetAllImprovementQueryHandler(repository, _mapper);

            // Act
            var result = await handler.Handle(new GetAllImprovementQuery(), CancellationToken.None);

            // Assert
            result.Should().BeEmpty();
        }
    }
}