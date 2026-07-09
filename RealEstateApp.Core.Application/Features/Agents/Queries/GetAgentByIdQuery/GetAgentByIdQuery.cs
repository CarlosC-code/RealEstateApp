using MediatR;
using RealEstateApp.Core.Application.Dtos.Agent;
using RealEstateApp.Core.Application.Interfaces;

namespace RealEstateApp.Core.Application.Features.Agents.Queries.GetById
{
    public class GetAgentByIdQuery : IRequest<AgentDto?>
    {
        public string Id { get; set; } = string.Empty;
    }

    public class GetAgentByIdQueryHandler : IRequestHandler<GetAgentByIdQuery, AgentDto?>
    {
        private readonly IAgentService _agentService;

        public GetAgentByIdQueryHandler(IAgentService agentService)
        {
            _agentService = agentService;
        }

        public async Task<AgentDto?> Handle(GetAgentByIdQuery query, CancellationToken cancellationToken)
        {
            return await _agentService.GetByIdAsync(query.Id);
        }
    }
}