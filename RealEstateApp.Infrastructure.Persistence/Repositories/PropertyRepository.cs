using Microsoft.EntityFrameworkCore;
using RealEstateApp.Core.Domain.Entities;
using RealEstateApp.Core.Domain.Enums;
using RealEstateApp.Core.Domain.Interfaces;
using RealEstateApp.Infrastructure.Persistence.Contexts;
using Microsoft.Extensions.Logging;

namespace RealEstateApp.Infrastructure.Persistence.Repositories
{
    public class PropertyRepository : GenericRepository<Property>, IPropertyRepository
    {
        private readonly ApplicationDbContext _context;

        public PropertyRepository(ApplicationDbContext context, ILogger<GenericRepository<Property>> logger)
            : base(context, logger)
        {
            _context = context;
        }

        public async Task<Property?> GetByCodeAsync(string code)
        {
            return await _context.Properties
                .Include(p => p.PropertyType)
                .Include(p => p.SaleType)
                .Include(p => p.Images)
                .Include(p => p.PropertyImprovements!)
                .ThenInclude(pi => pi.Improvement)
                .FirstOrDefaultAsync(p => p.Code == code);
        }

        public async Task<List<Property>> GetAllWithFiltersAsync(int? propertyTypeId,
            decimal? minPrice, decimal? maxPrice, int? bedrooms, int? bathrooms)
        {
            var query = _context.Properties
                .Include(p => p.PropertyType)
                .Include(p => p.SaleType)
                .Include(p => p.Images)
                .Where(p => p.Status == PropertyStatus.Available)
                .AsQueryable();

            if (propertyTypeId.HasValue)
                query = query.Where(p => p.PropertyTypeId == propertyTypeId);

            if (minPrice.HasValue)
                query = query.Where(p => p.Price >= minPrice);

            if (maxPrice.HasValue)
                query = query.Where(p => p.Price <= maxPrice);

            if (bedrooms.HasValue)
                query = query.Where(p => p.Bedrooms == bedrooms);

            if (bathrooms.HasValue)
                query = query.Where(p => p.Bathrooms == bathrooms);

            return await query.OrderByDescending(p => p.Id).ToListAsync();
        }

        public async Task<Property?> GetByIdWithIncludeAsync(int id)
        {
            return await _context.Properties
                .Include(p => p.PropertyType)
                .Include(p => p.SaleType)
                .Include(p => p.Images)
                .Include(p => p.PropertyImprovements!)
                    .ThenInclude(pi => pi.Improvement)
                .FirstOrDefaultAsync(p => p.Id == id);
        }
        public async Task<List<Property>> GetByAgentIdAsync(string agentId)
        {
            return await _context.Properties
                .Include(p => p.PropertyType)
                .Include(p => p.SaleType)
                .Include(p => p.Images)
                .Where(p => p.AgentId == agentId)
                .ToListAsync();
        }
    }
}