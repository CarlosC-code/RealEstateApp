using RealEstateApp.Core.Application.Dtos.FavoriteProperty;

namespace RealEstateApp.Core.Application.Interfaces
{
    public interface IFavoritePropertyService : IGenericService<FavoritePropertyDto>
    {
        Task<List<FavoritePropertyDto>> GetByClientIdAsync(string clientId);
    }
}