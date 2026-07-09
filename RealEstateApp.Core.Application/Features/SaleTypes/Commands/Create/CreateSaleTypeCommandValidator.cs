using FluentValidation;

namespace RealEstateApp.Core.Application.Features.SaleTypes.Commands.Create
{
    public class CreateSaleTypeCommandValidator : AbstractValidator<CreateSaleTypeCommand>
    {
        public CreateSaleTypeCommandValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("El nombre es requerido.");

            RuleFor(x => x.Description)
                .NotEmpty().WithMessage("La descripción es requerida.");
        }
    }
}