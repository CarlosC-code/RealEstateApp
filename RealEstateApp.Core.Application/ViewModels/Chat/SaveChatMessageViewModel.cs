using System.ComponentModel.DataAnnotations;

namespace RealEstateApp.Core.Application.ViewModels.Chat
{
    public class SaveChatMessageViewModel : BaseViewModel
    {
        public int PropertyId { get; set; }
        public string? SenderId { get; set; }
        public string? ReceiverId { get; set; }

        [Required(ErrorMessage = "El mensaje es requerido")]
        public string? Message { get; set; }
    }
}