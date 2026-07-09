using RealEstateApp.Core.Domain.Common;

namespace RealEstateApp.Core.Domain.Entities
{
    public class Improvement : BaseEntity
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public ICollection<PropertyImprovement>? PropertyImprovements { get; set; }
    }
}