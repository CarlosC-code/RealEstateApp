using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using RealEstateApp.Core.Application.Dtos.Improvement;
using RealEstateApp.Core.Domain.Interfaces;

namespace RealEstateApp.Core.Application.Features.Improvements.Queries.GetAll
{
    public class GetAllImprovementQuery : IRequest<IList<ImprovementDto>>
    {
    }

    public class GetAllImprovementQueryHandler : IRequestHandler<GetAllImprovementQuery, IList<ImprovementDto>>
    {
        private readonly IImprovementRepository _improvementRepository;
        private readonly IMapper _mapper;

        public GetAllImprovementQueryHandler(IImprovementRepository improvementRepository, IMapper mapper)
        {
            _improvementRepository = improvementRepository;
            _mapper = mapper;
        }

        public async Task<IList<ImprovementDto>> Handle(GetAllImprovementQuery query, CancellationToken cancellationToken)
        {
            var listEntitiesQuery = _improvementRepository.GetAllQuery();
            return await listEntitiesQuery
                .ProjectTo<ImprovementDto>(_mapper.ConfigurationProvider)
                .ToListAsync(cancellationToken);
        }
    }
}