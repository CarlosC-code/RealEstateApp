namespace RealEstateApp.Core.Application.ViewModels.Property
{
    public class FilterPropertyViewModel : BaseViewModel
    {
        public int? PropertyTypeId { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public int? Bedrooms { get; set; }
        public int? Bathrooms { get; set; }
        public string? Code { get; set; }
    }
}