using FluentValidation;

namespace RealEstateApp.Core.Application.Features.Properties.Queries.GetById
{
    public class GetPropertyByIdQueryValidator : AbstractValidator<GetPropertyByIdQuery>
    {
        public GetPropertyByIdQueryValidator()
        {
            RuleFor(x => x.Id)
                .GreaterThan(0).WithMessage("El Id es requerido.");
        }
    }
}