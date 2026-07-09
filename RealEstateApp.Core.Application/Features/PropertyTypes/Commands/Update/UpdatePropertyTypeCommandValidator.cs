using FluentValidation;

namespace RealEstateApp.Core.Application.Features.PropertyTypes.Commands.Update
{
    public class UpdatePropertyTypeCommandValidator : AbstractValidator<UpdatePropertyTypeCommand>
    {
        public UpdatePropertyTypeCommandValidator()
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