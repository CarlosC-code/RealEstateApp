using RealEstateApp.Core.Application.Interfaces.Repositories;
using RealEstateApp.Core.Domain.Entities;

namespace RealEstateApp.Core.Domain.Interfaces
{
    public interface IOfferRepository : IGenericRepository<Offer>
    {
        Task<List<Offer>> GetByPropertyIdAsync(int propertyId);
        Task<List<Offer>> GetByClientIdAsync(string clientId);
    }
}