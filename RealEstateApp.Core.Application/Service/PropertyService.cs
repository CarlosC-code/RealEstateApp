using AutoMapper;
using Microsoft.Extensions.Logging;
using RealEstateApp.Core.Application.Dtos.Property;
using RealEstateApp.Core.Domain.Entities;
using RealEstateApp.Core.Domain.Enums;
using RealEstateApp.Core.Domain.Interfaces;
using RealEstateApp.Core.Application.Interfaces;

namespace RealEstateApp.Core.Application.Services
{
    public class PropertyService : GenericService<Property, PropertyDto>, IPropertyService
    {
        private readonly IPropertyRepository _propertyRepository;
        private readonly IPropertyTypeRepository _propertyTypeRepository;
        private readonly ISaleTypeRepository _saleTypeRepository;
        private readonly IImprovementRepository _improvementRepository;
        private readonly IPropertyImageRepository _propertyImageRepository;
        private readonly IPropertyImprovementRepository _propertyImprovementRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<PropertyService> _logger;

        public PropertyService(IPropertyRepository propertyRepository,
            IPropertyTypeRepository propertyTypeRepository,
            ISaleTypeRepository saleTypeRepository,
            IImprovementRepository improvementRepository,
            IPropertyImageRepository propertyImageRepository,
            IPropertyImprovementRepository propertyImprovementRepository,
            IMapper mapper, ILoggerFactory loggerFactory)
            : base(propertyRepository, mapper, loggerFactory.CreateLogger<PropertyService>())
        {
            _propertyRepository = propertyRepository;
            _propertyTypeRepository = propertyTypeRepository;
            _saleTypeRepository = saleTypeRepository;
            _improvementRepository = improvementRepository;
            _propertyImageRepository = propertyImageRepository;
            _propertyImprovementRepository = propertyImprovementRepository;
            _mapper = mapper;
            _logger = loggerFactory.CreateLogger<PropertyService>();
        }

        public override async Task<List<PropertyDto>> GetAll()
        {
            try
            {
                var list = await _propertyRepository.GetAllWithIncludeAsync(
                    ["PropertyType", "SaleType", "Images", "PropertyImprovements"]);
                return list.Select(MapToDto).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all properties");
                return [];
            }
        }

        public override async Task<PropertyDto?> GetById(int id)
        {
            try
            {
                var property = await _propertyRepository.GetByIdWithIncludeAsync(id);
                if (property == null) return null;
                return MapToDto(property);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting property by ID: {Id}", id);
                return null;
            }
        }

        public async Task<PropertyDto?> GetByCodeAsync(string code)
        {
            try
            {
                var property = await _propertyRepository.GetByCodeAsync(code);
                if (property == null) return null;
                return MapToDto(property);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting property by code: {Code}", code);
                return null;
            }
        }

        public async Task<List<PropertyDto>> GetByAgentIdAsync(string agentId)
        {
            try
            {
                var list = await _propertyRepository.GetByAgentIdAsync(agentId);
                return list.Select(MapToDto).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting properties by agent ID: {AgentId}", agentId);
                return [];
            }
        }

        public async Task<List<PropertyDto>> GetAllWithFiltersAsync(int? propertyTypeId,
            decimal? minPrice, decimal? maxPrice, int? bedrooms, int? bathrooms)
        {
            try
            {
                var list = await _propertyRepository.GetAllWithFiltersAsync(
                    propertyTypeId, minPrice, maxPrice, bedrooms, bathrooms);
                return list.Select(MapToDto).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting properties with filters");
                return [];
            }
        }

        public override async Task<PropertyDto?> AddAsync(PropertyDto dto)
        {
            try
            {
                _logger.LogInformation("Creating new property for agent ID: {AgentId}", dto.AgentId);

                var propertyTypes = await _propertyTypeRepository.GetAllAsync();
                if (!propertyTypes.Any())
                {
                    _logger.LogWarning("No property types found, cannot create property");
                    return null;
                }

                var saleTypes = await _saleTypeRepository.GetAllAsync();
                if (!saleTypes.Any())
                {
                    _logger.LogWarning("No sale types found, cannot create property");
                    return null;
                }

                var improvements = await _improvementRepository.GetAllAsync();
                if (!improvements.Any())
                {
                    _logger.LogWarning("No improvements found, cannot create property");
                    return null;
                }

                string code = await GenerateUniqueCodeAsync();

                var entity = new Property
                {
                    Code = code,
                    AgentId = dto.AgentId,
                    PropertyTypeId = dto.PropertyTypeId,
                    SaleTypeId = dto.SaleTypeId,
                    Price = dto.Price,
                    LandSize = dto.LandSize,
                    Bedrooms = dto.Bedrooms,
                    Bathrooms = dto.Bathrooms,
                    Description = dto.Description,
                    Status = PropertyStatus.Available
                };

                var result = await _propertyRepository.AddAsync(entity);
                if (result == null) return null;

                if (dto.Images != null && dto.Images.Any())
                {
                    foreach (var imageUrl in dto.Images)
                    {
                        _logger.LogInformation("Saving image: {ImageUrl}", imageUrl);
                        await _propertyImageRepository.AddAsync(new PropertyImage
                        {
                            PropertyId = result.Id,
                            ImageUrl = imageUrl
                        });
                    }
                }

                if (dto.ImprovementIds != null && dto.ImprovementIds.Any())
                {
                    foreach (var improvementId in dto.ImprovementIds)
                    {
                        _logger.LogInformation("Saving improvement: {ImprovementId}", improvementId);
                        await _propertyImprovementRepository.AddAsync(new PropertyImprovement
                        {
                            PropertyId = result.Id,
                            ImprovementId = improvementId
                        });
                    }
                }

                _logger.LogInformation("Property created successfully with code: {Code}", code);
                return MapToDto(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating property");
                return null;
            }
        }

        public override async Task<PropertyDto?> UpdateAsync(PropertyDto dto, int id)
        {
            try
            {
                _logger.LogInformation("Updating property with ID: {Id}", id);

                var entity = await _propertyRepository.GetByIdAsync(id);
                if (entity == null)
                {
                    _logger.LogWarning("Property with ID: {Id} not found", id);
                    return null;
                }

                entity.PropertyTypeId = dto.PropertyTypeId;
                entity.SaleTypeId = dto.SaleTypeId;
                entity.Price = dto.Price;
                entity.LandSize = dto.LandSize;
                entity.Bedrooms = dto.Bedrooms;
                entity.Bathrooms = dto.Bathrooms;
                entity.Description = dto.Description;

                await _propertyRepository.UpdateAsync(id, entity);

                var existingImprovements = await _propertyImprovementRepository.GetByPropertyIdAsync(id);
                foreach (var improvement in existingImprovements)
                {
                    await _propertyImprovementRepository.DeleteAsync(improvement.Id);
                }

                if (dto.ImprovementIds != null && dto.ImprovementIds.Any())
                {
                    foreach (var improvementId in dto.ImprovementIds)
                    {
                        await _propertyImprovementRepository.AddAsync(new PropertyImprovement
                        {
                            PropertyId = id,
                            ImprovementId = improvementId
                        });
                    }
                }

                if (dto.Images != null && dto.Images.Any())
                {
                    foreach (var imageUrl in dto.Images)
                    {
                        await _propertyImageRepository.AddAsync(new PropertyImage
                        {
                            PropertyId = id,
                            ImageUrl = imageUrl
                        });
                    }
                }

                _logger.LogInformation("Property with ID: {Id} updated successfully", id);
                return await GetById(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating property with ID: {Id}", id);
                return null;
            }
        }

        public override async Task<bool> DeleteAsync(int id)
        {
            try
            {
                _logger.LogInformation("Deleting property with ID: {Id}", id);

                var property = await _propertyRepository.GetByIdAsync(id);
                if (property == null)
                {
                    _logger.LogWarning("Property with ID: {Id} not found", id);
                    return false;
                }

                if (property.Status == PropertyStatus.Sold)
                {
                    _logger.LogWarning("Cannot delete a sold property with ID: {Id}", id);
                    return false;
                }

                await _propertyRepository.DeleteAsync(id);
                _logger.LogInformation("Property with ID: {Id} deleted successfully", id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting property with ID: {Id}", id);
                return false;
            }
        }

        public async Task DeleteImageAsync(int propertyId, string imageUrl)
        {
            if (string.IsNullOrEmpty(imageUrl)) return;
            try
            {
                imageUrl = Uri.UnescapeDataString(imageUrl);
                if (!imageUrl.StartsWith("/"))
                    imageUrl = "/" + imageUrl;

                _logger.LogInformation("Deleting image: {ImageUrl} for property ID: {PropertyId}",
                    imageUrl, propertyId);

                var images = await _propertyImageRepository.GetByPropertyIdAsync(propertyId);
                var image = images.FirstOrDefault(i =>
                    i.ImageUrl != null && i.ImageUrl == imageUrl);

                if (image != null)
                {
                    await _propertyImageRepository.DeleteAsync(image.Id);
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(),
                        "wwwroot", imageUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                        _logger.LogInformation("Image file deleted: {FilePath}", filePath);
                    }
                }
                else
                {
                    _logger.LogWarning("Image not found for property ID: {PropertyId}, URL: {ImageUrl}",
                        propertyId, imageUrl);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting image for property ID: {PropertyId}", propertyId);
            }
        }

        #region Private methods

        private async Task<string> GenerateUniqueCodeAsync()
        {
            var random = new Random();
            string code;
            bool exists;

            do
            {
                code = random.Next(100000, 999999).ToString();
                var property = await _propertyRepository.GetByCodeAsync(code);
                exists = property != null;
            } while (exists);

            return code;
        }

        private PropertyDto MapToDto(Property property)
        {
            return new PropertyDto
            {
                Id = property.Id,
                Code = property.Code,
                PropertyTypeId = property.PropertyTypeId,
                PropertyTypeName = property.PropertyType?.Name,
                SaleTypeId = property.SaleTypeId,
                SaleTypeName = property.SaleType?.Name,
                Price = property.Price,
                LandSize = property.LandSize,
                Bedrooms = property.Bedrooms,
                Bathrooms = property.Bathrooms,
                Description = property.Description,
                AgentId = property.AgentId,
                Status = property.Status.ToString(),
                Images = property.Images?.Select(i => i.ImageUrl!).ToList(),
                Improvements = property.PropertyImprovements?
                    .Select(pi => pi.Improvement?.Name!)
                    .ToList(),
                ImprovementIds = property.PropertyImprovements?
                    .Select(pi => pi.ImprovementId)
                    .ToList()
            };
        }

        #endregion
    }
}