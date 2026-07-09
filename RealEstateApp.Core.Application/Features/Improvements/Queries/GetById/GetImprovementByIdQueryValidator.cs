using FluentValidation;

namespace RealEstateApp.Core.Application.Features.Improvements.Queries.GetById
{
    public class GetImprovementByIdQueryValidator : AbstractValidator<GetImprovementByIdQuery>
    {
        public GetImprovementByIdQueryValidator()
        {
            RuleFor(x => x.Id)
                .GreaterThan(0).WithMessage("El Id es requerido.");
        }
    }
}