using MediatR;
using RealEstateApp.Core.Application.Dtos.Agent;
using RealEstateApp.Core.Application.Interfaces;

namespace RealEstateApp.Core.Application.Features.Agents.Queries.GetAll
{
    public class GetAllAgentQuery : IRequest<IList<AgentDto>>
    {
    }

    public class GetAllAgentQueryHandler : IRequestHandler<GetAllAgentQuery, IList<AgentDto>>
    {
        private readonly IAgentService _agentService;

        public GetAllAgentQueryHandler(IAgentService agentService)
        {
            _agentService = agentService;
        }

        public async Task<IList<AgentDto>> Handle(GetAllAgentQuery query, CancellationToken cancellationToken)
        {
            return await _agentService.GetAllAsync();
        }
    }
}