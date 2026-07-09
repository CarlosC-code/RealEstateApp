namespace RealEstateApp.Core.Application.Dtos.FavoriteProperty
{
    public class FavoritePropertyDto
    {
        public int Id { get; set; }
        public int PropertyId { get; set; }
        public string? ClientId { get; set; }
        public string? PropertyCode { get; set; }
        public string? PropertyTypeName { get; set; }
        public string? SaleTypeName { get; set; }
        public decimal Price { get; set; }
        public int Bedrooms { get; set; }
        public int Bathrooms { get; set; }
        public double LandSize { get; set; }
        public List<string>? Images { get; set; }
    }
}