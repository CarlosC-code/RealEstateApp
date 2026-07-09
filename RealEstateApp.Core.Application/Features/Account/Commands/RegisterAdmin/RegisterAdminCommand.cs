using MediatR;
using RealEstateApp.Core.Application.Dtos.Account;
using RealEstateApp.Core.Application.Interfaces;

namespace RealEstateApp.Core.Application.Features.Account.Commands.RegisterAdmin
{
    public class RegisterAdminCommand : IRequest<RegisterResponse>
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Email { get; set; }
        public string? UserName { get; set; }
        public string? Phone { get; set; }
        public string? Password { get; set; }
        public string? ConfirmPassword { get; set; }
    }

    public class RegisterAdminCommandHandler : IRequestHandler<RegisterAdminCommand, RegisterResponse>
    {
        private readonly IAccountService _accountService;

        public RegisterAdminCommandHandler(IAccountService accountService)
        {
            _accountService = accountService;
        }

        public async Task<RegisterResponse> Handle(RegisterAdminCommand command, CancellationToken cancellationToken)
        {
            return await _accountService.RegisterAdminAsync(new RegisterRequest
            {
                FirstName = command.FirstName,
                LastName = command.LastName,
                Email = command.Email,
                UserName = command.UserName,
                Phone = command.Phone,
                Password = command.Password,
                ConfirmPassword = command.ConfirmPassword
            });
        }
    }
}