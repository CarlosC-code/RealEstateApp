using MediatR;
using RealEstateApp.Core.Application.Dtos.Property;
using RealEstateApp.Core.Application.Interfaces;

namespace RealEstateApp.Core.Application.Features.Agents.Queries.GetAgentProperties
{
    public class GetAgentPropertiesQuery : IRequest<IList<PropertyDto>>
    {
        public string AgentId { get; set; } = string.Empty;
    }

    public class GetAgentPropertiesQueryHandler : IRequestHandler<GetAgentPropertiesQuery, IList<PropertyDto>>
    {
        private readonly IAgentService _agentService;

        public GetAgentPropertiesQueryHandler(IAgentService agentService)
        {
            _agentService = agentService;
        }

        public async Task<IList<PropertyDto>> Handle(GetAgentPropertiesQuery query, CancellationToken cancellationToken)
        {
            return await _agentService.GetAgentPropertiesAsync(query.AgentId);
        }
    }
}