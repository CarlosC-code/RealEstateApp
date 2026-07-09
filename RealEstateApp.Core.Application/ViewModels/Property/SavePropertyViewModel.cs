using System.ComponentModel.DataAnnotations;

namespace RealEstateApp.Core.Application.ViewModels.Property
{
    public class SavePropertyViewModel : BaseViewModel
    {
       

        [Required(ErrorMessage = "El tipo de propiedad es requerido")]
        public int PropertyTypeId { get; set; }

        [Required(ErrorMessage = "El tipo de venta es requerido")]
        public int SaleTypeId { get; set; }

        [Required(ErrorMessage = "El precio es requerido")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "El tamaño es requerido")]
        public double LandSize { get; set; }

        [Required(ErrorMessage = "La cantidad de habitaciones es requerida")]
        public int Bedrooms { get; set; }

        [Required(ErrorMessage = "La cantidad de baños es requerida")]
        public int Bathrooms { get; set; }

        [Required(ErrorMessage = "La descripción es requerida")]
        public string? Description { get; set; }

        public string? AgentId { get; set; }
        public List<int>? ImprovementIds { get; set; }
        public List<string>? ExistingImages { get; set; }
    }
}