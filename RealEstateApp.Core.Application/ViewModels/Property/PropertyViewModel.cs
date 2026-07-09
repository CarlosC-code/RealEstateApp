namespace RealEstateApp.Core.Application.ViewModels.Property
{
    public class PropertyViewModel : BaseViewModel
    {
        
        public string? Code { get; set; }
        public string? PropertyTypeName { get; set; }
        public string? SaleTypeName { get; set; }
        public decimal Price { get; set; }
        public double LandSize { get; set; }
        public int Bedrooms { get; set; }
        public int Bathrooms { get; set; }
        public string? Description { get; set; }
        public string? Status { get; set; }
        public string? AgentId { get; set; }
        public string? AgentName { get; set; }
        public List<string>? Images { get; set; }
        public List<string>? Improvements { get; set; }
    }
}