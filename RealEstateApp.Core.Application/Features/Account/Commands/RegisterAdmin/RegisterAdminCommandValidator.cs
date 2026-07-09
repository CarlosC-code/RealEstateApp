using FluentValidation;

namespace RealEstateApp.Core.Application.Features.Account.Commands.RegisterAdmin
{
    public class RegisterAdminCommandValidator : AbstractValidator<RegisterAdminCommand>
    {
        public RegisterAdminCommandValidator()
        {
            RuleFor(x => x.FirstName)
                .NotEmpty().WithMessage("El nombre es requerido.");

            RuleFor(x => x.LastName)
                .NotEmpty().WithMessage("El apellido es requerido.");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("El correo es requerido.")
                .EmailAddress().WithMessage("El correo no es válido.");

            RuleFor(x => x.UserName)
                .NotEmpty().WithMessage("El usuario es requerido.");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("La contraseña es requerida.")
                .MinimumLength(8).WithMessage("La contraseña debe tener al menos 8 caracteres.");

            RuleFor(x => x.ConfirmPassword)
                .NotEmpty().WithMessage("La confirmación de contraseña es requerida.")
                .Equal(x => x.Password).WithMessage("Las contraseñas no coinciden.");
        }
    }
}