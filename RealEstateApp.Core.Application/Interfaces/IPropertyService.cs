using RealEstateApp.Core.Application.Dtos.Property;

namespace RealEstateApp.Core.Application.Interfaces
{
    public interface IPropertyService : IGenericService<PropertyDto>
    {
        Task<PropertyDto?> GetByCodeAsync(string code);
        Task<List<PropertyDto>> GetByAgentIdAsync(string agentId);
        Task<List<PropertyDto>> GetAllWithFiltersAsync(int? propertyTypeId, decimal? minPrice,
            decimal? maxPrice, int? bedrooms, int? bathrooms);
        Task DeleteImageAsync(int propertyId, string imageUrl);
    }
}