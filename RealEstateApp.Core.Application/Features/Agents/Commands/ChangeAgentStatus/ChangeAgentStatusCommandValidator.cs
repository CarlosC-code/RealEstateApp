using FluentValidation;
using RealEstateApp.Core.Application.Features.Agents.Commands.ChangeAgentStatus;

namespace RealEstateApp.Core.Application.Features.Agents.Commands.ChangeAgentStatus
{
    public class ChangeAgentStatusCommandValidator : AbstractValidator<ChangeAgentStatusCommand>
    {
        public ChangeAgentStatusCommandValidator()
        {
            RuleFor(x => x.AgentId)
                .NotEmpty().WithMessage("El Id del agente es requerido.");
        }
    }
}