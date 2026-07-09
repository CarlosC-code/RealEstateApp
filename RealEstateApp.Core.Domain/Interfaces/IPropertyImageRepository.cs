using RealEstateApp.Core.Application.Interfaces.Repositories;
using RealEstateApp.Core.Domain.Entities;

namespace RealEstateApp.Core.Domain.Interfaces
{
    public interface IPropertyImageRepository : IGenericRepository<PropertyImage>
    {
        Task<List<PropertyImage>> GetByPropertyIdAsync(int propertyId);
    }
}