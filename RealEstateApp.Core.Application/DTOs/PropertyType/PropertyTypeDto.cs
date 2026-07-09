namespace RealEstateApp.Core.Application.Dtos.PropertyType
{
    public class PropertyTypeDto
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public int PropertyCount { get; set; }
    }
}