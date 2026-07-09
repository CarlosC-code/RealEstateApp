using FluentValidation;

namespace RealEstateApp.Core.Application.Features.PropertyTypes.Queries.GetById
{
    public class GetPropertyTypeByIdQueryValidator : AbstractValidator<GetPropertyTypeByIdQuery>
    {
        public GetPropertyTypeByIdQueryValidator()
        {
            RuleFor(x => x.Id)
                .GreaterThan(0).WithMessage("El Id es requerido.");
        }
    }
}