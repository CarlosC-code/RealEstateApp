using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using RealEstateApp.Core.Application.Dtos.Property;
using RealEstateApp.Core.Domain.Interfaces;

namespace RealEstateApp.Core.Application.Features.Properties.Queries.GetAll
{
    public class GetAllPropertyQuery : IRequest<IList<PropertyDto>>
    {
    }

    public class GetAllPropertyQueryHandler : IRequestHandler<GetAllPropertyQuery, IList<PropertyDto>>
    {
        private readonly IPropertyRepository _propertyRepository;
        private readonly IMapper _mapper;

        public GetAllPropertyQueryHandler(IPropertyRepository propertyRepository, IMapper mapper)
        {
            _propertyRepository = propertyRepository;
            _mapper = mapper;
        }

        public async Task<IList<PropertyDto>> Handle(GetAllPropertyQuery query, CancellationToken cancellationToken)
        {
            var listEntitiesQuery = _propertyRepository.GetAllQueryWithInclude(
                ["PropertyType", "SaleType", "Images", "PropertyImprovements"]);
            var listEntityDtos = await listEntitiesQuery
                .ProjectTo<PropertyDto>(_mapper.ConfigurationProvider)
                .ToListAsync(cancellationToken);
            return listEntityDtos;
        }
    }
}