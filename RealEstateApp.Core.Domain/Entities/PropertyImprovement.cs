using RealEstateApp.Core.Domain.Common;

namespace RealEstateApp.Core.Domain.Entities
{
    public class PropertyImprovement : BaseEntity
    {
        public int PropertyId { get; set; }
        public int ImprovementId { get; set; }
        public Property? Property { get; set; }
        public Improvement? Improvement { get; set; }
    }
}