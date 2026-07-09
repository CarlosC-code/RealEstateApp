using MediatR;
using RealEstateApp.Core.Application.Dtos.Account;
using RealEstateApp.Core.Application.Interfaces;

namespace RealEstateApp.Core.Application.Features.Account.Commands.Login
{
    public class LoginCommand : IRequest<AuthenticationResponse>
    {
        public string? UsernameOrEmail { get; set; }
        public string? Password { get; set; }
    }

    public class LoginCommandHandler : IRequestHandler<LoginCommand, AuthenticationResponse>
    {
        private readonly IAccountService _accountService;

        public LoginCommandHandler(IAccountService accountService)
        {
            _accountService = accountService;
        }

        public async Task<AuthenticationResponse> Handle(LoginCommand command, CancellationToken cancellationToken)
        {
            return await _accountService.AuthenticateForApiAsync(new AuthenticationRequest
            {
                UsernameOrEmail = command.UsernameOrEmail,
                Password = command.Password
            });
        }
    }
}