using MediatR;
using RealEstateApp.Core.Application.Interfaces;

namespace RealEstateApp.Core.Application.Features.Agents.Commands.ChangeAgentStatus
{
    public class ChangeAgentStatusCommand : IRequest<Unit>
    {
        public string AgentId { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }

    public class ChangeAgentStatusCommandHandler : IRequestHandler<ChangeAgentStatusCommand, Unit>
    {
        private readonly IAgentService _agentService;

        public ChangeAgentStatusCommandHandler(IAgentService agentService)
        {
            _agentService = agentService;
        }

        public async Task<Unit> Handle(ChangeAgentStatusCommand command, CancellationToken cancellationToken)
        {
            await _agentService.ChangeStatusAsync(command.AgentId, command.IsActive);
            return Unit.Value;
        }
    }
}