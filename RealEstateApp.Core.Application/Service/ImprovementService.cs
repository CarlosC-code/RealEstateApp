using AutoMapper;
using Microsoft.Extensions.Logging;
using RealEstateApp.Core.Application.Dtos.Improvement;
using RealEstateApp.Core.Domain.Entities;
using RealEstateApp.Core.Domain.Interfaces;
using RealEstateApp.Core.Application.Interfaces;

namespace RealEstateApp.Core.Application.Services
{
    public class ImprovementService : GenericService<Improvement, ImprovementDto>, IImprovementService
    {
        private readonly IImprovementRepository _repository;
        private readonly IMapper _mapper;
        private readonly ILogger<ImprovementService> _logger;

        public ImprovementService(IImprovementRepository repository, IMapper mapper, ILoggerFactory loggerFactory)
            : base(repository, mapper, loggerFactory.CreateLogger<ImprovementService>())
        {
            _repository = repository;
            _mapper = mapper;
            _logger = loggerFactory.CreateLogger<ImprovementService>();
        }

        public override async Task<bool> DeleteAsync(int id)
        {
            try
            {
                _logger.LogInformation("Deleting improvement with ID: {Id}", id);

                // Logica de negocio: verificar que la mejora existe antes de eliminar
                var improvement = await _repository.GetByIdAsync(id);
                if (improvement == null)
                {
                    _logger.LogWarning("Improvement with ID: {Id} not found", id);
                    return false;
                }

                await _repository.DeleteAsync(id);
                _logger.LogInformation("Improvement with ID: {Id} deleted successfully", id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting improvement with ID: {Id}", id);
                return false;
            }
        }
    }
}