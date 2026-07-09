using AutoMapper;
using MediatR;
using RealEstateApp.Core.Application.Dtos.Improvement;
using RealEstateApp.Core.Domain.Interfaces;

namespace RealEstateApp.Core.Application.Features.Improvements.Queries.GetById
{
    public class GetImprovementByIdQuery : IRequest<ImprovementDto?>
    {
        public int Id { get; set; }
    }

    public class GetImprovementByIdQueryHandler : IRequestHandler<GetImprovementByIdQuery, ImprovementDto?>
    {
        private readonly IImprovementRepository _improvementRepository;
        private readonly IMapper _mapper;

        public GetImprovementByIdQueryHandler(IImprovementRepository improvementRepository, IMapper mapper)
        {
            _improvementRepository = improvementRepository;
            _mapper = mapper;
        }

        public async Task<ImprovementDto?> Handle(GetImprovementByIdQuery query, CancellationToken cancellationToken)
        {
            var entity = await _improvementRepository.GetByIdAsync(query.Id);
            if (entity == null) return null;
            return _mapper.Map<ImprovementDto>(entity);
        }
    }
}