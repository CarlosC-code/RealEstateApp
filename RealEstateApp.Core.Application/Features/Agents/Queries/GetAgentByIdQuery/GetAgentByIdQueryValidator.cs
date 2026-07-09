using FluentValidation;

namespace RealEstateApp.Core.Application.Features.Agents.Queries.GetById
{
    public class GetAgentByIdQueryValidator : AbstractValidator<GetAgentByIdQuery>
    {
        public GetAgentByIdQueryValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("El Id del agente es requerido.");
        }
    }
}