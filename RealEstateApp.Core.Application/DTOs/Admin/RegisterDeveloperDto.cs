using System.ComponentModel.DataAnnotations;

namespace RealEstateApp.Core.Application.Dtos.Admin
{
    public class RegisterDeveloperDto
    {
        public string? Id { get; set; }

        [Required(ErrorMessage = "El nombre es requerido")]
        public string? FirstName { get; set; }

        [Required(ErrorMessage = "El apellido es requerido")]
        public string? LastName { get; set; }

        [Required(ErrorMessage = "La cédula es requerida")]
        public string? IdCard { get; set; }

        [Required(ErrorMessage = "El correo es requerido")]
        public string? Email { get; set; }

        [Required(ErrorMessage = "El usuario es requerido")]
        public string? UserName { get; set; }

        [Required(ErrorMessage = "La contraseña es requerida")]
        public string? Password { get; set; }

        [Required(ErrorMessage = "Confirmar contraseña es requerido")]
        [Compare("Password", ErrorMessage = "Las contraseñas no coinciden")]
        public string? ConfirmPassword { get; set; }
    }
}