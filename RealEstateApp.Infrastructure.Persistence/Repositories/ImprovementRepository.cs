using Microsoft.Extensions.Logging;
using RealEstateApp.Core.Domain.Entities;
using RealEstateApp.Core.Domain.Interfaces;
using RealEstateApp.Infrastructure.Persistence.Contexts;

namespace RealEstateApp.Infrastructure.Persistence.Repositories
{
    public class ImprovementRepository : GenericRepository<Improvement>, IImprovementRepository
    {
        public ImprovementRepository(ApplicationDbContext context,
            ILogger<GenericRepository<Improvement>> logger) : base(context, logger)
        {
        }
    }
}