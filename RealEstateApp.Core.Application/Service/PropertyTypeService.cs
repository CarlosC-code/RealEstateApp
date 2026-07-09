using AutoMapper;
using Microsoft.Extensions.Logging;
using RealEstateApp.Core.Application.Dtos.PropertyType;
using RealEstateApp.Core.Application.Interfaces;
using RealEstateApp.Core.Domain.Entities;
using RealEstateApp.Core.Domain.Interfaces;


namespace RealEstateApp.Core.Application.Services
{
    public class PropertyTypeService : GenericService<PropertyType, PropertyTypeDto>, IPropertyTypeService
    {
        private readonly IPropertyTypeRepository _repository;
        private readonly IPropertyRepository _propertyRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<PropertyTypeService> _logger;

        public PropertyTypeService(IPropertyTypeRepository repository, IPropertyRepository propertyRepository,
            IMapper mapper, ILoggerFactory loggerFactory)
            : base(repository, mapper, loggerFactory.CreateLogger<PropertyTypeService>())
        {
            _repository = repository;
            _propertyRepository = propertyRepository;
            _mapper = mapper;
            _logger = loggerFactory.CreateLogger<PropertyTypeService>();
        }

        public override async Task<List<PropertyTypeDto>> GetAll()
        {
            try
            {
                // Incluir cantidad de propiedades por tipo
                var list = await _repository.GetAllWithIncludeAsync(["Properties"]);
                return list.Select(x => new PropertyTypeDto
                {
                    Id = x.Id,
                    Name = x.Name,
                    Description = x.Description,
                    PropertyCount = x.Properties?.Count ?? 0
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all property types");
                return [];
            }
        }

        public override async Task<bool> DeleteAsync(int id)
        {
            try
            {
                _logger.LogInformation("Deleting property type with ID: {Id} and all its properties", id);

                // Logica de negocio: eliminar todas las propiedades asociadas a este tipo
                var properties = await _propertyRepository.GetAllAsync();
                var propertiesOfType = properties.Where(p => p.PropertyTypeId == id).ToList();

                foreach (var property in propertiesOfType)
                {
                    _logger.LogInformation("Deleting property with ID: {PropertyId} associated to type {TypeId}",
                        property.Id, id);
                    await _propertyRepository.DeleteAsync(property.Id);
                }

                await _repository.DeleteAsync(id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting property type with ID: {Id}", id);
                return false;
            }
        }
    }
}