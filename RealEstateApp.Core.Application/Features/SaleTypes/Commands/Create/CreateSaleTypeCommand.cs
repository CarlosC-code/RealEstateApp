using AutoMapper;
using MediatR;
using RealEstateApp.Core.Application.Dtos.SaleType;
using RealEstateApp.Core.Domain.Entities;
using RealEstateApp.Core.Domain.Interfaces;

namespace RealEstateApp.Core.Application.Features.SaleTypes.Commands.Create
{
    public class CreateSaleTypeCommand : IRequest<SaleTypeDto?>
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
    }

    public class CreateSaleTypeCommandHandler : IRequestHandler<CreateSaleTypeCommand, SaleTypeDto?>
    {
        private readonly ISaleTypeRepository _saleTypeRepository;
        private readonly IMapper _mapper;

        public CreateSaleTypeCommandHandler(ISaleTypeRepository saleTypeRepository, IMapper mapper)
        {
            _saleTypeRepository = saleTypeRepository;
            _mapper = mapper;
        }

        public async Task<SaleTypeDto?> Handle(CreateSaleTypeCommand command, CancellationToken cancellationToken)
        {
            var entity = new SaleType
            {
                Name = command.Name,
                Description = command.Description
            };
            var result = await _saleTypeRepository.AddAsync(entity);
            return _mapper.Map<SaleTypeDto>(result);
        }
    }
}