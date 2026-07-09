using FluentValidation;

namespace RealEstateApp.Core.Application.Features.Improvements.Commands.Create
{
    public class CreateImprovementCommandValidator : AbstractValidator<CreateImprovementCommand>
    {
        public CreateImprovementCommandValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("El nombre es requerido.");

            RuleFor(x => x.Description)
                .NotEmpty().WithMessage("La descripción es requerida.");
        }
    }
}