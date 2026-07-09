using AutoMapper;
using Microsoft.Extensions.Logging;
using RealEstateApp.Core.Application.Dtos.Chat;
using RealEstateApp.Core.Domain.Entities;
using RealEstateApp.Core.Domain.Interfaces;
using RealEstateApp.Core.Application.Interfaces;

namespace RealEstateApp.Core.Application.Services
{
    public class ChatMessageService : GenericService<ChatMessage, ChatMessageDto>, IChatMessageService
    {
        private readonly IChatMessageRepository _chatMessageRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<ChatMessageService> _logger;

        public ChatMessageService(IChatMessageRepository chatMessageRepository, IMapper mapper,
            ILoggerFactory loggerFactory)
            : base(chatMessageRepository, mapper, loggerFactory.CreateLogger<ChatMessageService>())
        {
            _chatMessageRepository = chatMessageRepository;
            _mapper = mapper;
            _logger = loggerFactory.CreateLogger<ChatMessageService>();
        }

        public async Task<List<ChatMessageDto>> GetByPropertyAndUsersAsync(int propertyId,
            string senderId, string receiverId)
        {
            try
            {
                _logger.LogInformation("Getting chat messages for property ID: {PropertyId}", propertyId);
                var list = await _chatMessageRepository.GetByPropertyAndUsersAsync(propertyId, senderId, receiverId);
                return list.Select(m => new ChatMessageDto
                {
                    Id = m.Id,
                    PropertyId = m.PropertyId,
                    SenderId = m.SenderId,
                    ReceiverId = m.ReceiverId,
                    Message = m.Message,
                    SentAt = m.SentAt,
                    IsFromClient = m.SenderId == senderId
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting chat messages for property ID: {PropertyId}", propertyId);
                return [];
            }
        }

        public async Task<List<ChatMessageDto>> GetConversationsByAgentAsync(string agentId)
        {
            try
            {
                var messages = await _chatMessageRepository.GetByAgentIdAsync(agentId);
                return messages.Select(m => new ChatMessageDto
                {
                    Id = m.Id,
                    PropertyId = m.PropertyId,
                    SenderId = m.SenderId,
                    ReceiverId = m.ReceiverId,
                    Message = m.Message,
                    SentAt = m.SentAt,
                    IsFromClient = m.SenderId != agentId
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting conversations for agent: {AgentId}", agentId);
                return [];
            }
        }

        public async Task<ChatMessageDto?> SendMessageAsync(int propertyId, string senderId,
            string receiverId, string message)
        {
            try
            {
                _logger.LogInformation("Sending message from {SenderId} to {ReceiverId}", senderId, receiverId);

                if (senderId == receiverId)
                {
                    _logger.LogWarning("Sender and receiver cannot be the same user");
                    return null;
                }

                if (string.IsNullOrWhiteSpace(message))
                {
                    _logger.LogWarning("Message cannot be empty");
                    return null;
                }

                var entity = new ChatMessage
                {
                    PropertyId = propertyId,
                    SenderId = senderId,
                    ReceiverId = receiverId,
                    Message = message,
                    SentAt = DateTime.Now
                };

                var result = await _chatMessageRepository.AddAsync(entity);
                if (result == null) return null;

                return new ChatMessageDto
                {
                    Id = result.Id,
                    PropertyId = result.PropertyId,
                    SenderId = result.SenderId,
                    ReceiverId = result.ReceiverId,
                    Message = result.Message,
                    SentAt = result.SentAt,
                    IsFromClient = true
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending message");
                return null;
            }
        }

        public override async Task<ChatMessageDto?> AddAsync(ChatMessageDto dto)
        {
            return await SendMessageAsync(dto.PropertyId, dto.SenderId!, dto.ReceiverId!, dto.Message!);
        }
    }
}