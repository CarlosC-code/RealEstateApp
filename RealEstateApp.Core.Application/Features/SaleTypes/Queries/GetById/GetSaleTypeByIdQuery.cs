using AutoMapper;
using MediatR;
using RealEstateApp.Core.Application.Dtos.SaleType;
using RealEstateApp.Core.Domain.Interfaces;

namespace RealEstateApp.Core.Application.Features.SaleTypes.Queries.GetById
{
    public class GetSaleTypeByIdQuery : IRequest<SaleTypeDto?>
    {
        public int Id { get; set; }
    }

    public class GetSaleTypeByIdQueryHandler : IRequestHandler<GetSaleTypeByIdQuery, SaleTypeDto?>
    {
        private readonly ISaleTypeRepository _saleTypeRepository;
        private readonly IMapper _mapper;

        public GetSaleTypeByIdQueryHandler(ISaleTypeRepository saleTypeRepository, IMapper mapper)
        {
            _saleTypeRepository = saleTypeRepository;
            _mapper = mapper;
        }

        public async Task<SaleTypeDto?> Handle(GetSaleTypeByIdQuery query, CancellationToken cancellationToken)
        {
            var entity = await _saleTypeRepository.GetByIdAsync(query.Id);
            if (entity == null) return null;
            return _mapper.Map<SaleTypeDto>(entity);
        }
    }
}