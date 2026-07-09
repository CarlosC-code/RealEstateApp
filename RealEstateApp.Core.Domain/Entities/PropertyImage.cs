using RealEstateApp.Core.Domain.Common;

namespace RealEstateApp.Core.Domain.Entities
{
    public class PropertyImage : BaseEntity
    {
        public int PropertyId { get; set; }
        public string? ImageUrl { get; set; }
        public Property? Property { get; set; }
    }
}