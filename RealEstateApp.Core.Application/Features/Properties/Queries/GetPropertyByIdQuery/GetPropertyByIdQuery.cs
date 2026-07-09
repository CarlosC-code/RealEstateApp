using AutoMapper;
using MediatR;
using RealEstateApp.Core.Application.Dtos.Property;
using RealEstateApp.Core.Domain.Interfaces;

namespace RealEstateApp.Core.Application.Features.Properties.Queries.GetById
{
    public class GetPropertyByIdQuery : IRequest<PropertyDto?>
    {
        public int Id { get; set; }
    }

    public class GetPropertyByIdQueryHandler : IRequestHandler<GetPropertyByIdQuery, PropertyDto?>
    {
        private readonly IPropertyRepository _propertyRepository;
        private readonly IMapper _mapper;

        public GetPropertyByIdQueryHandler(IPropertyRepository propertyRepository, IMapper mapper)
        {
            _propertyRepository = propertyRepository;
            _mapper = mapper;
        }

        public async Task<PropertyDto?> Handle(GetPropertyByIdQuery query, CancellationToken cancellationToken)
        {
            var property = await _propertyRepository.GetByIdWithIncludeAsync(query.Id);
            if (property == null) return null;
            return _mapper.Map<PropertyDto>(property);
        }
    }
}