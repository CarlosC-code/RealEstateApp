using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RealEstateApp.Core.Domain.Entities;
using RealEstateApp.Core.Domain.Interfaces;
using RealEstateApp.Infrastructure.Persistence.Contexts;

namespace RealEstateApp.Infrastructure.Persistence.Repositories
{
    public class PropertyImageRepository : GenericRepository<PropertyImage>, IPropertyImageRepository
    {
        private readonly ApplicationDbContext _context;

        public PropertyImageRepository(ApplicationDbContext context,
            ILogger<GenericRepository<PropertyImage>> logger) : base(context, logger)
        {
            _context = context;
        }

        public async Task<List<PropertyImage>> GetByPropertyIdAsync(int propertyId)
        {
            return await _context.PropertyImages
                .Where(p => p.PropertyId == propertyId)
                .ToListAsync();
        }
    }
}