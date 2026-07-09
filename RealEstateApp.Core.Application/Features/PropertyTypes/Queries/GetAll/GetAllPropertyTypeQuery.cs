using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using RealEstateApp.Core.Application.Dtos.PropertyType;
using RealEstateApp.Core.Domain.Interfaces;

namespace RealEstateApp.Core.Application.Features.PropertyTypes.Queries.GetAll
{
    public class GetAllPropertyTypeQuery : IRequest<IList<PropertyTypeDto>>
    {
    }

    public class GetAllPropertyTypeQueryHandler : IRequestHandler<GetAllPropertyTypeQuery, IList<PropertyTypeDto>>
    {
        private readonly IPropertyTypeRepository _propertyTypeRepository;
        private readonly IMapper _mapper;

        public GetAllPropertyTypeQueryHandler(IPropertyTypeRepository propertyTypeRepository, IMapper mapper)
        {
            _propertyTypeRepository = propertyTypeRepository;
            _mapper = mapper;
        }

        public async Task<IList<PropertyTypeDto>> Handle(GetAllPropertyTypeQuery query, CancellationToken cancellationToken)
        {
            var listEntitiesQuery = _propertyTypeRepository.GetAllQuery();
            return await listEntitiesQuery
                .ProjectTo<PropertyTypeDto>(_mapper.ConfigurationProvider)
                .ToListAsync(cancellationToken);
        }
    }
}