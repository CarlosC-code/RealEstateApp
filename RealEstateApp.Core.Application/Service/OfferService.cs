using AutoMapper;
using Microsoft.Extensions.Logging;
using RealEstateApp.Core.Application.Dtos.Offer;
using RealEstateApp.Core.Domain.Entities;
using RealEstateApp.Core.Domain.Enums;
using RealEstateApp.Core.Domain.Interfaces;
using RealEstateApp.Core.Application.Interfaces;

namespace RealEstateApp.Core.Application.Services
{
    public class OfferService : GenericService<Offer, OfferDto>, IOfferService
    {
        private readonly IOfferRepository _offerRepository;
        private readonly IPropertyRepository _propertyRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<OfferService> _logger;

        public OfferService(IOfferRepository offerRepository, IPropertyRepository propertyRepository,
            IMapper mapper, ILoggerFactory loggerFactory)
            : base(offerRepository, mapper, loggerFactory.CreateLogger<OfferService>())
        {
            _offerRepository = offerRepository;
            _propertyRepository = propertyRepository;
            _mapper = mapper;
            _logger = loggerFactory.CreateLogger<OfferService>();
        }

        public async Task<List<OfferDto>> GetByPropertyIdAsync(int propertyId)
        {
            try
            {
                var list = await _offerRepository.GetByPropertyIdAsync(propertyId);
                return _mapper.Map<List<OfferDto>>(list);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting offers by property ID: {PropertyId}", propertyId);
                return [];
            }
        }

        public async Task<List<OfferDto>> GetByClientIdAsync(string clientId)
        {
            try
            {
                var list = await _offerRepository.GetByClientIdAsync(clientId);
                return _mapper.Map<List<OfferDto>>(list);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting offers by client ID: {ClientId}", clientId);
                return [];
            }
        }

        public override async Task<OfferDto?> AddAsync(OfferDto dto)
        {
            try
            {
                _logger.LogInformation("Creating new offer for property ID: {PropertyId}", dto.PropertyId);

                // Logica de negocio: verificar que no exista una oferta aprobada para esta propiedad
                var existingOffers = await _offerRepository.GetByPropertyIdAsync(dto.PropertyId);
                bool hasAcceptedOffer = existingOffers.Any(o => o.Status == OfferStatus.Accepted);
                if (hasAcceptedOffer)
                {
                    _logger.LogWarning("Property ID: {PropertyId} already has an accepted offer", dto.PropertyId);
                    return null;
                }

                // Logica de negocio: verificar que el cliente no tenga una oferta pendiente
                bool hasPendingOffer = existingOffers.Any(o => o.ClientId == dto.ClientId
                    && o.Status == OfferStatus.Pending);
                if (hasPendingOffer)
                {
                    _logger.LogWarning("Client ID: {ClientId} already has a pending offer for property ID: {PropertyId}",
                        dto.ClientId, dto.PropertyId);
                    return null;
                }

                // Crear la oferta en estado pendiente
                var entity = new Offer
                {
                    PropertyId = dto.PropertyId,
                    ClientId = dto.ClientId,
                    Amount = dto.Amount,
                    OfferDate = DateTime.Now,
                    Status = OfferStatus.Pending
                };

                var result = await _offerRepository.AddAsync(entity);
                return _mapper.Map<OfferDto>(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating offer for property ID: {PropertyId}", dto.PropertyId);
                return null;
            }
        }

        public async Task AcceptAsync(int offerId)
        {
            try
            {
                _logger.LogInformation("Accepting offer with ID: {OfferId}", offerId);
                var offer = await _offerRepository.GetByIdAsync(offerId);
                if (offer == null)
                {
                    _logger.LogWarning("Offer with ID: {OfferId} not found", offerId);
                    return;
                }

                // Logica de negocio: aceptar la oferta
                offer.Status = OfferStatus.Accepted;
                await _offerRepository.UpdateAsync(offerId, offer);

                // Logica de negocio: rechazar todas las demas ofertas pendientes de esa propiedad
                var allOffers = await _offerRepository.GetByPropertyIdAsync(offer.PropertyId);
                foreach (var o in allOffers.Where(o => o.Id != offerId && o.Status == OfferStatus.Pending))
                {
                    _logger.LogInformation("Rejecting offer with ID: {OfferId} due to another offer being accepted", o.Id);
                    o.Status = OfferStatus.Rejected;
                    await _offerRepository.UpdateAsync(o.Id, o);
                }

                // Logica de negocio: poner la propiedad en estado vendida
                var property = await _propertyRepository.GetByIdAsync(offer.PropertyId);
                if (property != null)
                {
                    _logger.LogInformation("Setting property ID: {PropertyId} to Sold status", offer.PropertyId);
                    property.Status = PropertyStatus.Sold;
                    await _propertyRepository.UpdateAsync(offer.PropertyId, property);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error accepting offer with ID: {OfferId}", offerId);
            }
        }

        public async Task RejectAsync(int offerId)
        {
            try
            {
                _logger.LogInformation("Rejecting offer with ID: {OfferId}", offerId);
                var offer = await _offerRepository.GetByIdAsync(offerId);
                if (offer == null)
                {
                    _logger.LogWarning("Offer with ID: {OfferId} not found", offerId);
                    return;
                }

                // Logica de negocio: solo se pueden rechazar ofertas pendientes
                if (offer.Status != OfferStatus.Pending)
                {
                    _logger.LogWarning("Offer with ID: {OfferId} is not in pending status", offerId);
                    return;
                }

                offer.Status = OfferStatus.Rejected;
                await _offerRepository.UpdateAsync(offerId, offer);
                _logger.LogInformation("Offer with ID: {OfferId} rejected successfully", offerId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error rejecting offer with ID: {OfferId}", offerId);
            }
        }
    }
}