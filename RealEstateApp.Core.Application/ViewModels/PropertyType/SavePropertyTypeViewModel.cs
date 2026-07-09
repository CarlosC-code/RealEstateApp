using System.ComponentModel.DataAnnotations;

namespace RealEstateApp.Core.Application.ViewModels.PropertyType
{
    public class SavePropertyTypeViewModel : BaseViewModel
    {
        

        [Required(ErrorMessage = "El nombre es requerido")]
        public string? Name { get; set; }

        [Required(ErrorMessage = "La descripción es requerida")]
        public string? Description { get; set; }
    }
}