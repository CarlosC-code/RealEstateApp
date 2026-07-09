using RealEstateApp.Core.Application.Interfaces.Repositories;
using RealEstateApp.Core.Domain.Entities;

namespace RealEstateApp.Core.Domain.Interfaces
{
    public interface IFavoritePropertyRepository : IGenericRepository<FavoriteProperty>
    {
        Task<List<FavoriteProperty>> GetByClientIdAsync(string clientId);
    }
}