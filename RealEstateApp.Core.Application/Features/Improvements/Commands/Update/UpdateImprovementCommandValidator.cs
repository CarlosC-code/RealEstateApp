using FluentValidation;

namespace RealEstateApp.Core.Application.Features.Improvements.Commands.Update
{
    public class UpdateImprovementCommandValidator : AbstractValidator<UpdateImprovementCommand>
    {
        public UpdateImprovementCommandValidator()
        {
            RuleFor(x => x.Id)
                .GreaterThan(0).WithMessage("El Id es requerido.");

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("El nombre es requerido.");

            RuleFor(x => x.Description)
                .NotEmpty().WithMessage("La descripción es requerida.");
        }
    }
}