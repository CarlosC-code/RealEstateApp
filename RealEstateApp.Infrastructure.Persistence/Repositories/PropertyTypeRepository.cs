using Microsoft.Extensions.Logging;
using RealEstateApp.Core.Domain.Entities;
using RealEstateApp.Core.Domain.Interfaces;
using RealEstateApp.Infrastructure.Persistence.Contexts;

namespace RealEstateApp.Infrastructure.Persistence.Repositories
{
    public class PropertyTypeRepository : GenericRepository<PropertyType>, IPropertyTypeRepository
    {
        public PropertyTypeRepository(ApplicationDbContext context,
            ILogger<GenericRepository<PropertyType>> logger) : base(context, logger)
        {
        }
    }
}