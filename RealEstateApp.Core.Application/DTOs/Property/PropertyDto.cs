namespace RealEstateApp.Core.Application.Dtos.Property
{
    public class PropertyDto
    {
        public int Id { get; set; }
        public string? Code { get; set; }
        public int PropertyTypeId { get; set; }
        public string? PropertyTypeName { get; set; }
        public int SaleTypeId { get; set; }
        public string? SaleTypeName { get; set; }
        public decimal Price { get; set; }
        public double LandSize { get; set; }
        public int Bedrooms { get; set; }
        public int Bathrooms { get; set; }
        public string? Description { get; set; }
        public string? AgentId { get; set; }
        public string? Status { get; set; }
        public List<string>? Images { get; set; }
        public List<string>? Improvements { get; set; }
        public List<int>? ImprovementIds { get; set; }

        public string? AgentName { get; set; }
    }
}