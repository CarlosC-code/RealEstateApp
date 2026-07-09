using AutoMapper;
using MediatR;
using RealEstateApp.Core.Application.Dtos.SaleType;
using RealEstateApp.Core.Domain.Interfaces;

namespace RealEstateApp.Core.Application.Features.SaleTypes.Commands.Update
{
    public class UpdateSaleTypeCommand : IRequest<SaleTypeDto?>
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
    }

    public class UpdateSaleTypeCommandHandler : IRequestHandler<UpdateSaleTypeCommand, SaleTypeDto?>
    {
        private readonly ISaleTypeRepository _saleTypeRepository;
        private readonly IMapper _mapper;

        public UpdateSaleTypeCommandHandler(ISaleTypeRepository saleTypeRepository, IMapper mapper)
        {
            _saleTypeRepository = saleTypeRepository;
            _mapper = mapper;
        }

        public async Task<SaleTypeDto?> Handle(UpdateSaleTypeCommand command, CancellationToken cancellationToken)
        {
            var entity = await _saleTypeRepository.GetByIdAsync(command.Id);
            if (entity == null) return null;

            entity.Name = command.Name;
            entity.Description = command.Description;

            var result = await _saleTypeRepository.UpdateAsync(command.Id, entity);
            return _mapper.Map<SaleTypeDto>(result);
        }
    }
}