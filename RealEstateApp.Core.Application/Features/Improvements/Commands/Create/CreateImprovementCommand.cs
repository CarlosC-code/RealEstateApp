using AutoMapper;
using MediatR;
using RealEstateApp.Core.Application.Dtos.Improvement;
using RealEstateApp.Core.Domain.Entities;
using RealEstateApp.Core.Domain.Interfaces;

namespace RealEstateApp.Core.Application.Features.Improvements.Commands.Create
{
    public class CreateImprovementCommand : IRequest<ImprovementDto?>
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
    }

    public class CreateImprovementCommandHandler : IRequestHandler<CreateImprovementCommand, ImprovementDto?>
    {
        private readonly IImprovementRepository _improvementRepository;
        private readonly IMapper _mapper;

        public CreateImprovementCommandHandler(IImprovementRepository improvementRepository, IMapper mapper)
        {
            _improvementRepository = improvementRepository;
            _mapper = mapper;
        }

        public async Task<ImprovementDto?> Handle(CreateImprovementCommand command, CancellationToken cancellationToken)
        {
            var entity = new Improvement
            {
                Name = command.Name,
                Description = command.Description
            };
            var result = await _improvementRepository.AddAsync(entity);
            return _mapper.Map<ImprovementDto>(result);
        }
    }
}