using RealEstateApp.Core.Domain.Common;
using RealEstateApp.Core.Domain.Enums;

namespace RealEstateApp.Core.Domain.Entities
{
    public class Offer : BaseEntity
    {
        public int PropertyId { get; set; }
        public string? ClientId { get; set; }
        public decimal Amount { get; set; }
        public DateTime OfferDate { get; set; }
        public OfferStatus Status { get; set; }
        public Property? Property { get; set; }
    }
}