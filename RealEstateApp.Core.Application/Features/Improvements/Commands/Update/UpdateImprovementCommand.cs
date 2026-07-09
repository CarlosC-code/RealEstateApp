using AutoMapper;
using MediatR;
using RealEstateApp.Core.Application.Dtos.Improvement;
using RealEstateApp.Core.Domain.Interfaces;

namespace RealEstateApp.Core.Application.Features.Improvements.Commands.Update
{
    public class UpdateImprovementCommand : IRequest<ImprovementDto?>
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
    }

    public class UpdateImprovementCommandHandler : IRequestHandler<UpdateImprovementCommand, ImprovementDto?>
    {
        private readonly IImprovementRepository _improvementRepository;
        private readonly IMapper _mapper;

        public UpdateImprovementCommandHandler(IImprovementRepository improvementRepository, IMapper mapper)
        {
            _improvementRepository = improvementRepository;
            _mapper = mapper;
        }

        public async Task<ImprovementDto?> Handle(UpdateImprovementCommand command, CancellationToken cancellationToken)
        {
            var entity = await _improvementRepository.GetByIdAsync(command.Id);
            if (entity == null) return null;

            entity.Name = command.Name;
            entity.Description = command.Description;

            var result = await _improvementRepository.UpdateAsync(command.Id, entity);
            return _mapper.Map<ImprovementDto>(result);
        }
    }
}