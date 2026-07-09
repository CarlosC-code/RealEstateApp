using RealEstateApp.Core.Application.Interfaces.Repositories;
using RealEstateApp.Core.Domain.Entities;

namespace RealEstateApp.Core.Domain.Interfaces
{
    public interface IPropertyRepository : IGenericRepository<Property>
    {
        Task<Property?> GetByCodeAsync(string code);
        Task<List<Property>> GetAllWithFiltersAsync(int? propertyTypeId, decimal? minPrice,
            decimal? maxPrice, int? bedrooms, int? bathrooms);
        Task<List<Property>> GetByAgentIdAsync(string agentId);
        Task<Property?> GetByIdWithIncludeAsync(int id);
    }
}