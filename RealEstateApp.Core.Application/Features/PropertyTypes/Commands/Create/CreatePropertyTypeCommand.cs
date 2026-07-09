using AutoMapper;
using MediatR;
using RealEstateApp.Core.Application.Dtos.PropertyType;
using RealEstateApp.Core.Domain.Entities;
using RealEstateApp.Core.Domain.Interfaces;

namespace RealEstateApp.Core.Application.Features.PropertyTypes.Commands.Create
{
    public class CreatePropertyTypeCommand : IRequest<PropertyTypeDto?>
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
    }

    public class CreatePropertyTypeCommandHandler : IRequestHandler<CreatePropertyTypeCommand, PropertyTypeDto?>
    {
        private readonly IPropertyTypeRepository _propertyTypeRepository;
        private readonly IMapper _mapper;

        public CreatePropertyTypeCommandHandler(IPropertyTypeRepository propertyTypeRepository, IMapper mapper)
        {
            _propertyTypeRepository = propertyTypeRepository;
            _mapper = mapper;
        }

        public async Task<PropertyTypeDto?> Handle(CreatePropertyTypeCommand command, CancellationToken cancellationToken)
        {
            var entity = new PropertyType
            {
                Name = command.Name,
                Description = command.Description
            };
            var result = await _propertyTypeRepository.AddAsync(entity);
            return _mapper.Map<PropertyTypeDto>(result);
        }
    }
}