using System.ComponentModel.DataAnnotations;

namespace RealEstateApp.Core.Application.ViewModels.Offer
{
    public class SaveOfferViewModel : BaseViewModel
    {
        public int PropertyId { get; set; }
        public string? ClientId { get; set; }

        [Required(ErrorMessage = "El monto es requerido")]
        public decimal Amount { get; set; }
    }
}