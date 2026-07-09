using RealEstateApp.Core.Application.Dtos.Offer;

namespace RealEstateApp.Core.Application.Interfaces
{
    public interface IOfferService : IGenericService<OfferDto>
    {
        Task<List<OfferDto>> GetByPropertyIdAsync(int propertyId);
        Task<List<OfferDto>> GetByClientIdAsync(string clientId);
        Task AcceptAsync(int offerId);
        Task RejectAsync(int offerId);
    }
}