using MediatR;
using RealEstateApp.Core.Application.Dtos.Account;
using RealEstateApp.Core.Application.Interfaces;

namespace RealEstateApp.Core.Application.Features.Account.Commands.RegisterDeveloper
{
    public class RegisterDeveloperCommand : IRequest<RegisterResponse>
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Email { get; set; }
        public string? UserName { get; set; }
        public string? Phone { get; set; }
        public string? Password { get; set; }
        public string? ConfirmPassword { get; set; }
    }

    public class RegisterDeveloperCommandHandler : IRequestHandler<RegisterDeveloperCommand, RegisterResponse>
    {
        private readonly IAccountService _accountService;

        public RegisterDeveloperCommandHandler(IAccountService accountService)
        {
            _accountService = accountService;
        }

        public async Task<RegisterResponse> Handle(RegisterDeveloperCommand command, CancellationToken cancellationToken)
        {
            return await _accountService.RegisterDeveloperAsync(new RegisterRequest
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