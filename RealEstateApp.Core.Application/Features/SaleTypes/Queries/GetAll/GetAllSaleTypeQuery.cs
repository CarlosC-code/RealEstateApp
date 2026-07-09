using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using RealEstateApp.Core.Application.Dtos.SaleType;
using RealEstateApp.Core.Domain.Interfaces;

namespace RealEstateApp.Core.Application.Features.SaleTypes.Queries.GetAll
{
    public class GetAllSaleTypeQuery : IRequest<IList<SaleTypeDto>>
    {
    }

    public class GetAllSaleTypeQueryHandler : IRequestHandler<GetAllSaleTypeQuery, IList<SaleTypeDto>>
    {
        private readonly ISaleTypeRepository _saleTypeRepository;
        private readonly IMapper _mapper;

        public GetAllSaleTypeQueryHandler(ISaleTypeRepository saleTypeRepository, IMapper mapper)
        {
            _saleTypeRepository = saleTypeRepository;
            _mapper = mapper;
        }

        public async Task<IList<SaleTypeDto>> Handle(GetAllSaleTypeQuery query, CancellationToken cancellationToken)
        {
            var listEntitiesQuery = _saleTypeRepository.GetAllQuery();
            return await listEntitiesQuery
                .ProjectTo<SaleTypeDto>(_mapper.ConfigurationProvider)
                .ToListAsync(cancellationToken);
        }
    }
}