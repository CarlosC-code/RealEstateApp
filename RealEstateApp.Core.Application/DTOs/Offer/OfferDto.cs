namespace RealEstateApp.Core.Application.Dtos.Offer
{
    public class OfferDto
    {
        public int Id { get; set; }
        public int PropertyId { get; set; }
        public string? ClientId { get; set; }
        public decimal Amount { get; set; }
        public DateTime OfferDate { get; set; }
        public string? Status { get; set; }
    }
}