using AutoMapper;
using MediatR;
using RealEstateApp.Core.Application.Dtos.PropertyType;
using RealEstateApp.Core.Domain.Interfaces;

namespace RealEstateApp.Core.Application.Features.PropertyTypes.Queries.GetById
{
    public class GetPropertyTypeByIdQuery : IRequest<PropertyTypeDto?>
    {
        public int Id { get; set; }
    }

    public class GetPropertyTypeByIdQueryHandler : IRequestHandler<GetPropertyTypeByIdQuery, PropertyTypeDto?>
    {
        private readonly IPropertyTypeRepository _propertyTypeRepository;
        private readonly IMapper _mapper;

        public GetPropertyTypeByIdQueryHandler(IPropertyTypeRepository propertyTypeRepository, IMapper mapper)
        {
            _propertyTypeRepository = propertyTypeRepository;
            _mapper = mapper;
        }

        public async Task<PropertyTypeDto?> Handle(GetPropertyTypeByIdQuery query, CancellationToken cancellationToken)
        {
            var entity = await _propertyTypeRepository.GetByIdAsync(query.Id);
            if (entity == null) return null;
            return _mapper.Map<PropertyTypeDto>(entity);
        }
    }
}