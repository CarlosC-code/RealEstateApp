using MediatR;
using RealEstateApp.Core.Domain.Interfaces;

namespace RealEstateApp.Core.Application.Features.SaleTypes.Commands.Delete
{
    public class DeleteSaleTypeCommand : IRequest<Unit>
    {
        public int Id { get; set; }
    }

    public class DeleteSaleTypeCommandHandler : IRequestHandler<DeleteSaleTypeCommand, Unit>
    {
        private readonly ISaleTypeRepository _saleTypeRepository;

        public DeleteSaleTypeCommandHandler(ISaleTypeRepository saleTypeRepository)
        {
            _saleTypeRepository = saleTypeRepository;
        }

        public async Task<Unit> Handle(DeleteSaleTypeCommand command, CancellationToken cancellationToken)
        {
            await _saleTypeRepository.DeleteAsync(command.Id);
            return Unit.Value;
        }
    }
}