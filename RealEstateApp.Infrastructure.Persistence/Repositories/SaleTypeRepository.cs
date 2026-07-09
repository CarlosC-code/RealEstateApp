using Microsoft.Extensions.Logging;
using RealEstateApp.Core.Domain.Entities;
using RealEstateApp.Core.Domain.Interfaces;
using RealEstateApp.Infrastructure.Persistence.Contexts;

namespace RealEstateApp.Infrastructure.Persistence.Repositories
{
    public class SaleTypeRepository : GenericRepository<SaleType>, ISaleTypeRepository
    {
        public SaleTypeRepository(ApplicationDbContext context,
            ILogger<GenericRepository<SaleType>> logger) : base(context, logger)
        {
        }
    }
}