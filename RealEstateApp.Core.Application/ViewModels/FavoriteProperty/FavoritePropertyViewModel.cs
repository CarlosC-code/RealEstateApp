namespace RealEstateApp.Core.Application.ViewModels.FavoriteProperty
{
    public class FavoritePropertyViewModel : BaseViewModel
    {
        
        public int PropertyId { get; set; }
        public string? ClientId { get; set; }
        public string? PropertyCode { get; set; }
        public string? PropertyTypeName { get; set; }
        public decimal Price { get; set; }
    }
}