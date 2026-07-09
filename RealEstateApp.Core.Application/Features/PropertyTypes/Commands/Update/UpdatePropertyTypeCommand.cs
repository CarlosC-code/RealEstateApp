using AutoMapper;
using MediatR;
using RealEstateApp.Core.Application.Dtos.PropertyType;
using RealEstateApp.Core.Domain.Interfaces;

namespace RealEstateApp.Core.Application.Features.PropertyTypes.Commands.Update
{
    public class UpdatePropertyTypeCommand : IRequest<PropertyTypeDto?>
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
    }

    public class UpdatePropertyTypeCommandHandler : IRequestHandler<UpdatePropertyTypeCommand, PropertyTypeDto?>
    {
        private readonly IPropertyTypeRepository _propertyTypeRepository;
        private readonly IMapper _mapper;

        public UpdatePropertyTypeCommandHandler(IPropertyTypeRepository propertyTypeRepository, IMapper mapper)
        {
            _propertyTypeRepository = propertyTypeRepository;
            _mapper = mapper;
        }

        public async Task<PropertyTypeDto?> Handle(UpdatePropertyTypeCommand command, CancellationToken cancellationToken)
        {
            var entity = await _propertyTypeRepository.GetByIdAsync(command.Id);
            if (entity == null) return null;

            entity.Name = command.Name;
            entity.Description = command.Description;

            var result = await _propertyTypeRepository.UpdateAsync(command.Id, entity);
            return _mapper.Map<PropertyTypeDto>(result);
        }
    }
}