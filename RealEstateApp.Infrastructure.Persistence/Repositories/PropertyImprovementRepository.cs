using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RealEstateApp.Core.Domain.Entities;
using RealEstateApp.Core.Domain.Interfaces;
using RealEstateApp.Infrastructure.Persistence.Contexts;

namespace RealEstateApp.Infrastructure.Persistence.Repositories
{
    public class PropertyImprovementRepository : GenericRepository<PropertyImprovement>, IPropertyImprovementRepository
    {
        private readonly ApplicationDbContext _context;

        public PropertyImprovementRepository(ApplicationDbContext context,
            ILogger<GenericRepository<PropertyImprovement>> logger) : base(context, logger)
        {
            _context = context;
        }

        public async Task<List<PropertyImprovement>> GetByPropertyIdAsync(int propertyId)
        {
            return await _context.PropertyImprovements
                .Where(p => p.PropertyId == propertyId)
                .ToListAsync();
        }
    }
}