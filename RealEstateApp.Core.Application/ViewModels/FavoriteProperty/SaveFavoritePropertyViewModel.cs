namespace RealEstateApp.Core.Application.ViewModels.FavoriteProperty
{
    public class SaveFavoritePropertyViewModel : BaseViewModel
    {
        public int PropertyId { get; set; }
        public string? ClientId { get; set; }
    }
}