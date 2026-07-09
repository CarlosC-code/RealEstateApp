using AutoMapper;
using RealEstateApp.Core.Application.Interfaces;
using RealEstateApp.Core.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using RealEstateApp.Core.Application.Interfaces.Repositories;


namespace RealEstateApp.Core.Application.Services
{
    public class GenericService<Entity, DtoModel> : IGenericService<DtoModel>
        where Entity : class
        where DtoModel : class
    {
        private readonly IGenericRepository<Entity> _repository;
        private readonly IMapper _mapper;
        private readonly ILogger<GenericService<Entity, DtoModel>> _logger;

        public GenericService(IGenericRepository<Entity> repository, IMapper mapper,
            ILogger<GenericService<Entity, DtoModel>> logger)
        {
            _repository = repository;
            _mapper = mapper;
            _logger = logger;
        }

        public virtual async Task<DtoModel?> AddAsync(DtoModel dto)
        {
            try
            {
                _logger.LogInformation("Adding new entity of type {EntityType}", typeof(Entity).Name);
                Entity entity = _mapper.Map<Entity>(dto);
                Entity? returnEntity = await _repository.AddAsync(entity);
                if (returnEntity == null)
                {
                    _logger.LogWarning("Failed to add entity of type {EntityType}", typeof(Entity).Name);
                    return null;
                }
                return _mapper.Map<DtoModel>(returnEntity);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public virtual async Task<DtoModel?> UpdateAsync(DtoModel dto, int id)
        {
            try
            {
                Entity entity = _mapper.Map<Entity>(dto);
                Entity? returnEntity = await _repository.UpdateAsync(id, entity);
                if (returnEntity == null) return null;
                return _mapper.Map<DtoModel>(returnEntity);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public virtual async Task<bool> DeleteAsync(int id)
        {
            try
            {
                await _repository.DeleteAsync(id);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public virtual async Task<DtoModel?> GetById(int id)
        {
            try
            {
                var entity = await _repository.GetByIdAsync(id);
                if (entity == null) return null;
                return _mapper.Map<DtoModel>(entity);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public virtual async Task<List<DtoModel>> GetAll()
        {
            try
            {
                var listEntities = await _repository.GetAllAsync();
                return _mapper.Map<List<DtoModel>>(listEntities);
            }
            catch (Exception)
            {
                return [];
            }
        }
    }
}