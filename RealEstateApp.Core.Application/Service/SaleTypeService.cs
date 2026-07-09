using AutoMapper;
using Microsoft.Extensions.Logging;
using RealEstateApp.Core.Application.Dtos.SaleType;
using RealEstateApp.Core.Domain.Entities;
using RealEstateApp.Core.Domain.Interfaces;
using RealEstateApp.Core.Application.Interfaces;

namespace RealEstateApp.Core.Application.Services
{
    public class SaleTypeService : GenericService<SaleType, SaleTypeDto>, ISaleTypeService
    {
        private readonly ISaleTypeRepository _repository;
        private readonly IPropertyRepository _propertyRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<SaleTypeService> _logger;

        public SaleTypeService(ISaleTypeRepository repository, IPropertyRepository propertyRepository,
            IMapper mapper, ILoggerFactory loggerFactory)
            : base(repository, mapper, loggerFactory.CreateLogger<SaleTypeService>())
        {
            _repository = repository;
            _propertyRepository = propertyRepository;
            _mapper = mapper;
            _logger = loggerFactory.CreateLogger<SaleTypeService>();
        }

        public override async Task<List<SaleTypeDto>> GetAll()
        {
            try
            {
                // Incluir cantidad de propiedades por tipo de venta
                var list = await _repository.GetAllWithIncludeAsync(["Properties"]);
                return list.Select(x => new SaleTypeDto
                {
                    Id = x.Id,
                    Name = x.Name,
                    Description = x.Description,
                    PropertyCount = x.Properties?.Count ?? 0
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all sale types");
                return [];
            }
        }

        public override async Task<bool> DeleteAsync(int id)
        {
            try
            {
                _logger.LogInformation("Deleting sale type with ID: {Id} and all its properties", id);

                // Logica de negocio: eliminar todas las propiedades asociadas a este tipo de venta
                var properties = await _propertyRepository.GetAllAsync();
                var propertiesOfType = properties.Where(p => p.SaleTypeId == id).ToList();

                foreach (var property in propertiesOfType)
                {
                    _logger.LogInformation("Deleting property with ID: {PropertyId} associated to sale type {TypeId}",
                        property.Id, id);
                    await _propertyRepository.DeleteAsync(property.Id);
                }

                await _repository.DeleteAsync(id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting sale type with ID: {Id}", id);
                return false;
            }
        }
    }
}