using FluentValidation;

namespace RealEstateApp.Core.Application.Features.Agents.Queries.GetAgentProperties
{
    public class GetAgentPropertiesQueryValidator : AbstractValidator<GetAgentPropertiesQuery>
    {
        public GetAgentPropertiesQueryValidator()
        {
            RuleFor(x => x.AgentId)
                .NotEmpty().WithMessage("El Id del agente es requerido.");
        }
    }
}