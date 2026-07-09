namespace RealEstateApp.Core.Application.ViewModels.SaleType
{
    public class SaleTypeViewModel : BaseViewModel
    {
      
        public string? Name { get; set; }
        public string? Description { get; set; }
        public int PropertyCount { get; set; }
    }
}