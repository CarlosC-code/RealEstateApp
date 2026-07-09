using AutoMapper;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using RealEstateApp.Core.Application.Dtos.Chat;
using RealEstateApp.Core.Application.Mappings.EntitiesAndDtos;
using RealEstateApp.Core.Application.Services;
using RealEstateApp.Core.Domain.Entities;
using RealEstateApp.Core.Domain.Enums;
using RealEstateApp.Infrastructure.Persistence.Contexts;
using RealEstateApp.Infrastructure.Persistence.Repositories;

namespace RealEstateApp.Unit.Tests.Services
{
    public class ChatMessageServiceTests
    {
        private readonly DbContextOptions<ApplicationDbContext> _dbOptions;
        private readonly IMapper _mapper;

        public ChatMessageServiceTests()
        {
            _dbOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase($"TestDb_ChatMessageService_{Guid.NewGuid()}")
                .Options;

            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<ChatMessageMappingProfile>();
            });

            _mapper = config.CreateMapper();
        }

        private async Task<Property> SeedPropertyAsync(ApplicationDbContext context)
        {
            var pt = new PropertyType { Id = 0, Name = "Casa", Description = "Vivienda" };
            var st = new SaleType { Id = 0, Name = "Venta", Description = "Directa" };
            context.PropertyTypes.Add(pt);
            context.SaleTypes.Add(st);
            await context.SaveChangesAsync();

            var property = new Property
            {
                Id = 0,
                Code = $"M{Guid.NewGuid().ToString()[..5].ToUpper()}",
                AgentId = "agent-1",
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

        private ChatMessageService CreateService()
        {
            var factoryMoq = new Mock<ILoggerFactory>();
            factoryMoq.Setup(x => x.CreateLogger(It.IsAny<string>()))
                .Returns(new NullLogger<ChatMessageService>());

            var context = new ApplicationDbContext(_dbOptions);
            var repo = new ChatMessageRepository(context, new NullLogger<GenericRepository<ChatMessage>>());
            return new ChatMessageService(repo, _mapper, factoryMoq.Object);
        }

        [Fact]
        public async Task SendMessageAsync_Should_Send_Message_Successfully()
        {
            // Arrange
            using var context = new ApplicationDbContext(_dbOptions);
            var property = await SeedPropertyAsync(context);
            var service = CreateService();

            // Act
            var result = await service.SendMessageAsync(property.Id, "client-1", "agent-1", "Hola, estoy interesado");

            // Assert
            result.Should().NotBeNull();
            result!.Message.Should().Be("Hola, estoy interesado");
            result.SenderId.Should().Be("client-1");
            result.ReceiverId.Should().Be("agent-1");
            result.PropertyId.Should().Be(property.Id);
        }

        [Fact]
        public async Task SendMessageAsync_Should_Return_Null_When_Sender_Equals_Receiver()
        {
            // Arrange
            using var context = new ApplicationDbContext(_dbOptions);
            var property = await SeedPropertyAsync(context);
            var service = CreateService();

            // Act
            var result = await service.SendMessageAsync(property.Id, "user-1", "user-1", "Mensaje inválido");

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task SendMessageAsync_Should_Return_Null_When_Message_Is_Empty()
        {
            // Arrange
            using var context = new ApplicationDbContext(_dbOptions);
            var property = await SeedPropertyAsync(context);
            var service = CreateService();

            // Act
            var result = await service.SendMessageAsync(property.Id, "client-1", "agent-1", "   ");

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task AddAsync_Should_Use_SendMessageAsync_Logic()
        {
            // Arrange
            using var context = new ApplicationDbContext(_dbOptions);
            var property = await SeedPropertyAsync(context);
            var service = CreateService();

            var dto = new ChatMessageDto
            {
                PropertyId = property.Id,
                SenderId = "client-1",
                ReceiverId = "agent-1",
                Message = "Mensaje via AddAsync"
            };

            // Act
            var result = await service.AddAsync(dto);

            // Assert
            result.Should().NotBeNull();
            result!.Message.Should().Be("Mensaje via AddAsync");
        }

        [Fact]
        public async Task GetByPropertyAndUsersAsync_Should_Return_Conversation()
        {
            // Arrange
            using var context = new ApplicationDbContext(_dbOptions);
            var property = await SeedPropertyAsync(context);

            var clientId = "client-conv";
            var agentId = "agent-conv";

            context.ChatMessages.AddRange(
                new ChatMessage { Id = 0, PropertyId = property.Id, SenderId = clientId, ReceiverId = agentId, Message = "Hola agente", SentAt = DateTime.UtcNow.AddMinutes(-5) },
                new ChatMessage { Id = 0, PropertyId = property.Id, SenderId = agentId, ReceiverId = clientId, Message = "Hola cliente, ¿en qué te ayudo?", SentAt = DateTime.UtcNow.AddMinutes(-3) },
                new ChatMessage { Id = 0, PropertyId = property.Id, SenderId = "otro-cliente", ReceiverId = agentId, Message = "Soy otro cliente", SentAt = DateTime.UtcNow }
            );
            await context.SaveChangesAsync();

            var service = CreateService();

            // Act
            var result = await service.GetByPropertyAndUsersAsync(property.Id, clientId, agentId);

            // Assert
            result.Should().HaveCount(2);
            result.Should().BeInAscendingOrder(m => m.SentAt);
        }

        [Fact]
        public async Task GetByPropertyAndUsersAsync_Should_Set_IsFromClient_Correctly()
        {
            // Arrange
            using var context = new ApplicationDbContext(_dbOptions);
            var property = await SeedPropertyAsync(context);

            var clientId = "client-flag";
            var agentId = "agent-flag";

            context.ChatMessages.AddRange(
                new ChatMessage { Id = 0, PropertyId = property.Id, SenderId = clientId, ReceiverId = agentId, Message = "Del cliente", SentAt = DateTime.UtcNow.AddMinutes(-2) },
                new ChatMessage { Id = 0, PropertyId = property.Id, SenderId = agentId, ReceiverId = clientId, Message = "Del agente", SentAt = DateTime.UtcNow }
            );
            await context.SaveChangesAsync();

            var service = CreateService();

            // Act
            var result = await service.GetByPropertyAndUsersAsync(property.Id, clientId, agentId);

            // Assert
            result.Should().HaveCount(2);
            result.First(m => m.SenderId == clientId).IsFromClient.Should().BeTrue();
            result.First(m => m.SenderId == agentId).IsFromClient.Should().BeFalse();
        }

        [Fact]
        public async Task GetConversationsByAgentAsync_Should_Return_All_Agent_Messages()
        {
            // Arrange
            using var context = new ApplicationDbContext(_dbOptions);
            var property = await SeedPropertyAsync(context);

            var agentId = "agent-filter";

            context.ChatMessages.AddRange(
                new ChatMessage { Id = 0, PropertyId = property.Id, SenderId = "client-1", ReceiverId = agentId, Message = "M1", SentAt = DateTime.UtcNow.AddMinutes(-10) },
                new ChatMessage { Id = 0, PropertyId = property.Id, SenderId = agentId, ReceiverId = "client-1", Message = "R1", SentAt = DateTime.UtcNow.AddMinutes(-8) },
                new ChatMessage { Id = 0, PropertyId = property.Id, SenderId = "client-2", ReceiverId = "otro-agente", Message = "Otro", SentAt = DateTime.UtcNow }
            );
            await context.SaveChangesAsync();

            var service = CreateService();

            // Act
            var result = await service.GetConversationsByAgentAsync(agentId);

            // Assert
            result.Should().HaveCount(2);
            result.Should().AllSatisfy(m =>
                (m.SenderId == agentId || m.ReceiverId == agentId).Should().BeTrue()
            );
        }

        [Fact]
        public async Task GetConversationsByAgentAsync_Should_Set_IsFromClient_Correctly()
        {
            // Arrange
            using var context = new ApplicationDbContext(_dbOptions);
            var property = await SeedPropertyAsync(context);

            var agentId = "agent-isFromClient";

            context.ChatMessages.AddRange(
                new ChatMessage { Id = 0, PropertyId = property.Id, SenderId = "client-x", ReceiverId = agentId, Message = "Del cliente", SentAt = DateTime.UtcNow.AddMinutes(-2) },
                new ChatMessage { Id = 0, PropertyId = property.Id, SenderId = agentId, ReceiverId = "client-x", Message = "Del agente", SentAt = DateTime.UtcNow }
            );
            await context.SaveChangesAsync();

            var service = CreateService();

            // Act
            var result = await service.GetConversationsByAgentAsync(agentId);

            // Assert
            result.First(m => m.SenderId != agentId).IsFromClient.Should().BeTrue();
            result.First(m => m.SenderId == agentId).IsFromClient.Should().BeFalse();
        }

        [Fact]
        public async Task GetByPropertyAndUsersAsync_Should_Return_Empty_When_No_Messages()
        {
            // Arrange
            using var context = new ApplicationDbContext(_dbOptions);
            var property = await SeedPropertyAsync(context);
            var service = CreateService();

            // Act
            var result = await service.GetByPropertyAndUsersAsync(property.Id, "client-x", "agent-x");

            // Assert
            result.Should().BeEmpty();
        }
    }
}
