using AutoMapper;
using Microsoft.Extensions.Logging;
using RealEstateApp.Core.Application.Dtos.FavoriteProperty;
using RealEstateApp.Core.Domain.Entities;
using RealEstateApp.Core.Domain.Interfaces;
using RealEstateApp.Core.Application.Interfaces;

namespace RealEstateApp.Core.Application.Services
{
    public class FavoritePropertyService : GenericService<FavoriteProperty, FavoritePropertyDto>, IFavoritePropertyService
    {
        private readonly IFavoritePropertyRepository _favoritePropertyRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<FavoritePropertyService> _logger;

        public FavoritePropertyService(IFavoritePropertyRepository favoritePropertyRepository, IMapper mapper,
            ILoggerFactory loggerFactory)
            : base(favoritePropertyRepository, mapper, loggerFactory.CreateLogger<FavoritePropertyService>())
        {
            _favoritePropertyRepository = favoritePropertyRepository;
            _mapper = mapper;
            _logger = loggerFactory.CreateLogger<FavoritePropertyService>();
        }

        public async Task<List<FavoritePropertyDto>> GetByClientIdAsync(string clientId)
        {
            try
            {
                _logger.LogInformation("Getting favorite properties for client ID: {ClientId}", clientId);
                var list = await _favoritePropertyRepository.GetByClientIdAsync(clientId);
                return list.Select(x => new FavoritePropertyDto
                {
                    Id = x.Id,
                    PropertyId = x.PropertyId,
                    ClientId = x.ClientId,
                    PropertyCode = x.Property?.Code,
                    PropertyTypeName = x.Property?.PropertyType?.Name,
                    SaleTypeName = x.Property?.SaleType?.Name,
                    Price = x.Property?.Price ?? 0,
                    Bedrooms = x.Property?.Bedrooms ?? 0,
                    Bathrooms = x.Property?.Bathrooms ?? 0,
                    LandSize = x.Property?.LandSize ?? 0,
                    Images = x.Property?.Images?.Select(i => i.ImageUrl!).ToList()
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting favorite properties for client ID: {ClientId}", clientId);
                return [];
            }
        }

        public override async Task<FavoritePropertyDto?> AddAsync(FavoritePropertyDto dto)
        {
            try
            {
                _logger.LogInformation("Adding property ID: {PropertyId} to favorites for client ID: {ClientId}",
                    dto.PropertyId, dto.ClientId);

                // Logica de negocio: verificar que no este ya en favoritos
                var favorites = await _favoritePropertyRepository.GetByClientIdAsync(dto.ClientId!);
                bool alreadyFavorite = favorites.Any(f => f.PropertyId == dto.PropertyId);
                if (alreadyFavorite)
                {
                    _logger.LogWarning("Property ID: {PropertyId} is already in favorites for client ID: {ClientId}",
                        dto.PropertyId, dto.ClientId);
                    return null;
                }

                var entity = new FavoriteProperty
                {
                    PropertyId = dto.PropertyId,
                    ClientId = dto.ClientId
                };

                var result = await _favoritePropertyRepository.AddAsync(entity);
                return _mapper.Map<FavoritePropertyDto>(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding property to favorites");
                return null;
            }
        }
    }
}