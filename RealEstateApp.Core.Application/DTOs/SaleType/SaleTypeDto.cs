namespace RealEstateApp.Core.Application.Dtos.SaleType
{
    public class SaleTypeDto
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public int PropertyCount { get; set; }
    }
}