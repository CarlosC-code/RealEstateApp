using FluentValidation;

namespace RealEstateApp.Core.Application.Features.SaleTypes.Queries.GetById
{
    public class GetSaleTypeByIdQueryValidator : AbstractValidator<GetSaleTypeByIdQuery>
    {
        public GetSaleTypeByIdQueryValidator()
        {
            RuleFor(x => x.Id)
                .GreaterThan(0).WithMessage("El Id es requerido.");
        }
    }
}