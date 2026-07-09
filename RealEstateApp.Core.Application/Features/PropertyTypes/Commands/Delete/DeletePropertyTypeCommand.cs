using MediatR;
using RealEstateApp.Core.Domain.Interfaces;

namespace RealEstateApp.Core.Application.Features.PropertyTypes.Commands.Delete
{
    public class DeletePropertyTypeCommand : IRequest<Unit>
    {
        public int Id { get; set; }
    }

    public class DeletePropertyTypeCommandHandler : IRequestHandler<DeletePropertyTypeCommand, Unit>
    {
        private readonly IPropertyTypeRepository _propertyTypeRepository;

        public DeletePropertyTypeCommandHandler(IPropertyTypeRepository propertyTypeRepository)
        {
            _propertyTypeRepository = propertyTypeRepository;
        }

        public async Task<Unit> Handle(DeletePropertyTypeCommand command, CancellationToken cancellationToken)
        {
            await _propertyTypeRepository.DeleteAsync(command.Id);
            return Unit.Value;
        }
    }
}