namespace RealEstateApp.Core.Application.ViewModels.PropertyType
{
    public class PropertyTypeViewModel : BaseViewModel
    {
       
        public string? Name { get; set; }
        public string? Description { get; set; }
        public int PropertyCount { get; set; }
    }
}