using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RealEstateApp.Core.Domain.Entities;
using RealEstateApp.Core.Domain.Interfaces;
using RealEstateApp.Infrastructure.Persistence.Contexts;

namespace RealEstateApp.Infrastructure.Persistence.Repositories
{
    public class OfferRepository : GenericRepository<Offer>, IOfferRepository
    {
        private readonly ApplicationDbContext _context;

        public OfferRepository(ApplicationDbContext context, ILogger<GenericRepository<Offer>> logger)
            : base(context, logger)
        {
            _context = context;
        }

        public async Task<List<Offer>> GetByPropertyIdAsync(int propertyId)
        {
            return await _context.Offers
                .Where(o => o.PropertyId == propertyId)
                .ToListAsync();
        }

        public async Task<List<Offer>> GetByClientIdAsync(string clientId)
        {
            return await _context.Offers
                .Where(o => o.ClientId == clientId)
                .ToListAsync();
        }
    }
}