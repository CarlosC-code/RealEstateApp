using System.ComponentModel.DataAnnotations;

namespace RealEstateApp.Core.Application.ViewModels.Improvement
{
    public class SaveImprovementViewModel : BaseViewModel
    {
        

        [Required(ErrorMessage = "El nombre es requerido")]
        public string? Name { get; set; }

        [Required(ErrorMessage = "La descripción es requerida")]
        public string? Description { get; set; }
    }
}