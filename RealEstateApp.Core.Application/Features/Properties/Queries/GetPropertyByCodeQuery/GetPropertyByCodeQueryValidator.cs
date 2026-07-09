using FluentValidation;

namespace RealEstateApp.Core.Application.Features.Properties.Queries.GetByCode
{
    public class GetPropertyByCodeQueryValidator : AbstractValidator<GetPropertyByCodeQuery>
    {
        public GetPropertyByCodeQueryValidator()
        {
            RuleFor(x => x.Code)
                .NotEmpty().WithMessage("El código es requerido.");
        }
    }
}