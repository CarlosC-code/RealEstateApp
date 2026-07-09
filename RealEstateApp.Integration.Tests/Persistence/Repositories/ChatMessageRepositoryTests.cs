using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using RealEstateApp.Core.Domain.Entities;
using RealEstateApp.Core.Domain.Enums;
using RealEstateApp.Infrastructure.Persistence.Contexts;
using RealEstateApp.Infrastructure.Persistence.Repositories;

namespace RealEstateApp.Integration.Tests.Persistence.Repositories
{
    public class ChatMessageRepositoryTests
    {
        private readonly DbContextOptions<ApplicationDbContext> _dbOptions;

        public ChatMessageRepositoryTests()
        {
            _dbOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: $"TestDb_ChatMessage_{Guid.NewGuid()}")
                .Options;
        }

        private Mock<ILogger<GenericRepository<ChatMessage>>> CreateLoggerMock()
        {
            return new Mock<ILogger<GenericRepository<ChatMessage>>>();
        }

        private async Task<Property> SeedPropertyAsync(ApplicationDbContext context)
        {
            var pt = new PropertyType { Id = 0, Name = "Casa", Description = "Vivienda" };
            var st = new SaleType { Id = 0, Name = "Venta", Description = "Venta directa" };
            context.PropertyTypes.Add(pt);
            context.SaleTypes.Add(st);
            await context.SaveChangesAsync();

            var property = new Property
            {
                Id = 0,
                Code = $"C{Guid.NewGuid().ToString()[..5].ToUpper()}",
                AgentId = "agent-seed",
                PropertyTypeId = pt.Id,
                SaleTypeId = st.Id,
                Price = 3000000,
                LandSize = 150,
                Bedrooms = 3,
                Bathrooms = 2,
                Description = "Propiedad semilla",
                Status = PropertyStatus.Available
            };
            context.Properties.Add(property);
            await context.SaveChangesAsync();
            return property;
        }

        [Fact]
        public async Task AddAsync_Should_Add_ChatMessage_To_Database()
        {
            // Arrange
            using var context = new ApplicationDbContext(_dbOptions);
            var loggerMock = CreateLoggerMock();
            var property = await SeedPropertyAsync(context);

            var repo = new ChatMessageRepository(context, loggerMock.Object);

            var message = new ChatMessage
            {
                Id = 0,
                PropertyId = property.Id,
                SenderId = "client-1",
                ReceiverId = "agent-1",
                Message = "Hola, estoy interesado en la propiedad",
                SentAt = DateTime.UtcNow
            };

            // Act
            var result = await repo.AddAsync(message);

            // Assert
            result.Should().NotBeNull();
            result!.Id.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task AddAsync_Should_Throw_When_Null()
        {
            // Arrange
            using var context = new ApplicationDbContext(_dbOptions);
            var loggerMock = CreateLoggerMock();
            var repo = new ChatMessageRepository(context, loggerMock.Object);

            // Act
            Func<Task> act = async () => await repo.AddAsync(null!);

            // Assert
            await act.Should().ThrowAsync<Exception>();
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_ChatMessage_When_Exists()
        {
            // Arrange
            using var context = new ApplicationDbContext(_dbOptions);
            var loggerMock = CreateLoggerMock();
            var property = await SeedPropertyAsync(context);

            var message = new ChatMessage
            {
                Id = 0,
                PropertyId = property.Id,
                SenderId = "client-2",
                ReceiverId = "agent-2",
                Message = "¿Cuándo puedo visitar?",
                SentAt = DateTime.UtcNow
            };
            context.ChatMessages.Add(message);
            await context.SaveChangesAsync();

            var repo = new ChatMessageRepository(context, loggerMock.Object);

            // Act
            var result = await repo.GetByIdAsync(message.Id);

            // Assert
            result.Should().NotBeNull();
            result!.Message.Should().Be("¿Cuándo puedo visitar?");
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_When_NotExists()
        {
            // Arrange
            using var context = new ApplicationDbContext(_dbOptions);
            var loggerMock = CreateLoggerMock();
            var repo = new ChatMessageRepository(context, loggerMock.Object);

            // Act
            var result = await repo.GetByIdAsync(999);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task DeleteAsync_Should_Remove_ChatMessage()
        {
            // Arrange
            using var context = new ApplicationDbContext(_dbOptions);
            var loggerMock = CreateLoggerMock();
            var property = await SeedPropertyAsync(context);

            var message = new ChatMessage
            {
                Id = 0,
                PropertyId = property.Id,
                SenderId = "client-3",
                ReceiverId = "agent-3",
                Message = "Mensaje a eliminar",
                SentAt = DateTime.UtcNow
            };
            context.ChatMessages.Add(message);
            await context.SaveChangesAsync();

            var repo = new ChatMessageRepository(context, loggerMock.Object);

            // Act
            await repo.DeleteAsync(message.Id);
            var result = await repo.GetByIdAsync(message.Id);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task DeleteAsync_Should_Not_Throw_When_Id_NotExists()
        {
            // Arrange
            using var context = new ApplicationDbContext(_dbOptions);
            var loggerMock = CreateLoggerMock();
            var repo = new ChatMessageRepository(context, loggerMock.Object);

            // Act
            Func<Task> act = async () => await repo.DeleteAsync(999);

            // Assert
            await act.Should().NotThrowAsync();
        }

        [Fact]
        public async Task GetByPropertyAndUsersAsync_Should_Return_Conversation()
        {
            // Arrange
            using var context = new ApplicationDbContext(_dbOptions);
            var loggerMock = CreateLoggerMock();
            var property = await SeedPropertyAsync(context);

            var clientId = "client-conv";
            var agentId = "agent-conv";

            context.ChatMessages.AddRange(
                new ChatMessage { Id = 0, PropertyId = property.Id, SenderId = clientId, ReceiverId = agentId, Message = "Hola agente", SentAt = DateTime.UtcNow.AddMinutes(-5) },
                new ChatMessage { Id = 0, PropertyId = property.Id, SenderId = agentId, ReceiverId = clientId, Message = "Hola cliente", SentAt = DateTime.UtcNow.AddMinutes(-3) },
                new ChatMessage { Id = 0, PropertyId = property.Id, SenderId = "otro-cliente", ReceiverId = agentId, Message = "Otro mensaje", SentAt = DateTime.UtcNow }
            );
            await context.SaveChangesAsync();

            var repo = new ChatMessageRepository(context, loggerMock.Object);

            // Act
            var result = await repo.GetByPropertyAndUsersAsync(property.Id, clientId, agentId);

            // Assert
            result.Should().HaveCount(2);
            result.Should().BeInAscendingOrder(m => m.SentAt);
        }

        [Fact]
        public async Task GetByPropertyAndUsersAsync_Should_Return_Empty_When_No_Messages()
        {
            // Arrange
            using var context = new ApplicationDbContext(_dbOptions);
            var loggerMock = CreateLoggerMock();
            var property = await SeedPropertyAsync(context);

            var repo = new ChatMessageRepository(context, loggerMock.Object);

            // Act
            var result = await repo.GetByPropertyAndUsersAsync(property.Id, "client-x", "agent-x");

            // Assert
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetByAgentIdAsync_Should_Return_Messages_Related_To_Agent()
        {
            // Arrange
            using var context = new ApplicationDbContext(_dbOptions);
            var loggerMock = CreateLoggerMock();
            var property = await SeedPropertyAsync(context);

            var agentId = "agent-filter";

            context.ChatMessages.AddRange(
                new ChatMessage { Id = 0, PropertyId = property.Id, SenderId = "client-1", ReceiverId = agentId, Message = "Mensaje 1", SentAt = DateTime.UtcNow.AddMinutes(-10) },
                new ChatMessage { Id = 0, PropertyId = property.Id, SenderId = agentId, ReceiverId = "client-1", Message = "Respuesta 1", SentAt = DateTime.UtcNow.AddMinutes(-8) },
                new ChatMessage { Id = 0, PropertyId = property.Id, SenderId = "client-2", ReceiverId = "otro-agente", Message = "Otro agente", SentAt = DateTime.UtcNow }
            );
            await context.SaveChangesAsync();

            var repo = new ChatMessageRepository(context, loggerMock.Object);

            // Act
            var result = await repo.GetByAgentIdAsync(agentId);

            // Assert
            result.Should().HaveCount(2);
            result.Should().AllSatisfy(m =>
                (m.SenderId == agentId || m.ReceiverId == agentId).Should().BeTrue()
            );
        }

        [Fact]
        public async Task GetAllAsync_Should_Return_All_ChatMessages()
        {
            // Arrange
            using var context = new ApplicationDbContext(_dbOptions);
            var loggerMock = CreateLoggerMock();
            var property = await SeedPropertyAsync(context);

            context.ChatMessages.AddRange(
                new ChatMessage { Id = 0, PropertyId = property.Id, SenderId = "c1", ReceiverId = "a1", Message = "M1", SentAt = DateTime.UtcNow },
                new ChatMessage { Id = 0, PropertyId = property.Id, SenderId = "a1", ReceiverId = "c1", Message = "M2", SentAt = DateTime.UtcNow },
                new ChatMessage { Id = 0, PropertyId = property.Id, SenderId = "c2", ReceiverId = "a1", Message = "M3", SentAt = DateTime.UtcNow }
            );
            await context.SaveChangesAsync();

            var repo = new ChatMessageRepository(context, loggerMock.Object);

            // Act
            var result = await repo.GetAllAsync();

            // Assert
            result.Should().HaveCount(3);
        }
    }
}