using System.ComponentModel.DataAnnotations;

namespace RealEstateApp.Core.Application.Dtos.Account
{
    public class AuthenticationRequest
    {
        [Required(ErrorMessage = "El usuario o correo es requerido")]
        public string? UsernameOrEmail { get; set; }

        [Required(ErrorMessage = "La contraseña es requerida")]
        public string? Password { get; set; }
    }
}