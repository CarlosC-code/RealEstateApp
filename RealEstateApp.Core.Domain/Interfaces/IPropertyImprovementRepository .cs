using RealEstateApp.Core.Application.Interfaces.Repositories;
using RealEstateApp.Core.Domain.Entities;

namespace RealEstateApp.Core.Domain.Interfaces
{
    public interface IPropertyImprovementRepository : IGenericRepository<PropertyImprovement>
    {
        Task<List<PropertyImprovement>> GetByPropertyIdAsync(int propertyId);
    }
}