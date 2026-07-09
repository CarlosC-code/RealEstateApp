using AutoMapper;
using MediatR;
using RealEstateApp.Core.Application.Dtos.Property;
using RealEstateApp.Core.Domain.Interfaces;

namespace RealEstateApp.Core.Application.Features.Properties.Queries.GetByCode
{
    public class GetPropertyByCodeQuery : IRequest<PropertyDto?>
    {
        public string? Code { get; set; }
    }

    public class GetPropertyByCodeQueryHandler : IRequestHandler<GetPropertyByCodeQuery, PropertyDto?>
    {
        private readonly IPropertyRepository _propertyRepository;
        private readonly IMapper _mapper;

        public GetPropertyByCodeQueryHandler(IPropertyRepository propertyRepository, IMapper mapper)
        {
            _propertyRepository = propertyRepository;
            _mapper = mapper;
        }

        public async Task<PropertyDto?> Handle(GetPropertyByCodeQuery query, CancellationToken cancellationToken)
        {
            var property = await _propertyRepository.GetByCodeAsync(query.Code!);
            if (property == null) return null;
            return _mapper.Map<PropertyDto>(property);
        }
    }
}