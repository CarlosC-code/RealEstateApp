using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RealEstateApp.Core.Domain.Entities;
using RealEstateApp.Core.Domain.Interfaces;
using RealEstateApp.Infrastructure.Persistence.Contexts;

namespace RealEstateApp.Infrastructure.Persistence.Repositories
{
    public class FavoritePropertyRepository : GenericRepository<FavoriteProperty>, IFavoritePropertyRepository
    {
        private readonly ApplicationDbContext _context;

        public FavoritePropertyRepository(ApplicationDbContext context, ILogger<GenericRepository<FavoriteProperty>> logger)
            : base(context, logger)
        {
            _context = context;
        }

        public async Task<List<FavoriteProperty>> GetByClientIdAsync(string clientId)
        {
            return await _context.FavoriteProperties
                .Include(f => f.Property)
                    .ThenInclude(p => p!.Images)
                .Include(f => f.Property)
                    .ThenInclude(p => p!.PropertyType)
                .Include(f => f.Property)
                    .ThenInclude(p => p!.SaleType)
                .Where(f => f.ClientId == clientId)
                .ToListAsync();
        }
    }
}