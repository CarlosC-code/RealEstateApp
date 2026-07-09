using RealEstateApp.Core.Domain.Common;
using RealEstateApp.Core.Domain.Enums;

namespace RealEstateApp.Core.Domain.Entities
{
    public class Property : BaseEntity
    {
        public string? Code { get; set; }
        public string? AgentId { get; set; }
        public int PropertyTypeId { get; set; }
        public int SaleTypeId { get; set; }
        public decimal Price { get; set; }
        public double LandSize { get; set; }
        public int Bedrooms { get; set; }
        public int Bathrooms { get; set; }
        public string? Description { get; set; }
        public PropertyStatus Status { get; set; }
        public PropertyType? PropertyType { get; set; }
        public SaleType? SaleType { get; set; }
        public ICollection<PropertyImage>? Images { get; set; }
        public ICollection<PropertyImprovement>? PropertyImprovements { get; set; }
        public ICollection<Offer>? Offers { get; set; }
        public ICollection<ChatMessage>? ChatMessages { get; set; }
        public ICollection<FavoriteProperty>? FavoriteProperties { get; set; }
    }
}