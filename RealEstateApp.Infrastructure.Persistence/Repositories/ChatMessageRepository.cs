using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RealEstateApp.Core.Domain.Entities;
using RealEstateApp.Core.Domain.Interfaces;
using RealEstateApp.Infrastructure.Persistence.Contexts;

namespace RealEstateApp.Infrastructure.Persistence.Repositories
{
    public class ChatMessageRepository : GenericRepository<ChatMessage>, IChatMessageRepository
    {
        private readonly ApplicationDbContext _context;

        public ChatMessageRepository(ApplicationDbContext context, ILogger<GenericRepository<ChatMessage>> logger)
            : base(context, logger)
        {
            _context = context;
        }

        public async Task<List<ChatMessage>> GetByPropertyAndUsersAsync(int propertyId,
            string senderId, string receiverId)
        {
            return await _context.ChatMessages
                .Where(c => c.PropertyId == propertyId &&
                    ((c.SenderId == senderId && c.ReceiverId == receiverId) ||
                     (c.SenderId == receiverId && c.ReceiverId == senderId)))
                .OrderBy(c => c.SentAt)
                .ToListAsync();
        }

        public async Task<List<ChatMessage>> GetByAgentIdAsync(string agentId)
        {
            return await _context.ChatMessages
                .Where(c => c.SenderId == agentId || c.ReceiverId == agentId)
                .OrderBy(c => c.PropertyId)
                .ThenBy(c => c.SentAt)
                .ToListAsync();
        }
    }
}