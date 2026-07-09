using MediatR;
using RealEstateApp.Core.Domain.Interfaces;

namespace RealEstateApp.Core.Application.Features.Improvements.Commands.Delete
{
    public class DeleteImprovementCommand : IRequest<Unit>
    {
        public int Id { get; set; }
    }

    public class DeleteImprovementCommandHandler : IRequestHandler<DeleteImprovementCommand, Unit>
    {
        private readonly IImprovementRepository _improvementRepository;

        public DeleteImprovementCommandHandler(IImprovementRepository improvementRepository)
        {
            _improvementRepository = improvementRepository;
        }

        public async Task<Unit> Handle(DeleteImprovementCommand command, CancellationToken cancellationToken)
        {
            await _improvementRepository.DeleteAsync(command.Id);
            return Unit.Value;
        }
    }
}