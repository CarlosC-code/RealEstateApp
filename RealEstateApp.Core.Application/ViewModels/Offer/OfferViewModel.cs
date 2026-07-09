namespace RealEstateApp.Core.Application.ViewModels.Offer
{
    public class OfferViewModel : BaseViewModel
    {
        
        public int PropertyId { get; set; }
        public string? ClientId { get; set; }
        public decimal Amount { get; set; }
        public DateTime OfferDate { get; set; }
        public string? Status { get; set; }
    }
}