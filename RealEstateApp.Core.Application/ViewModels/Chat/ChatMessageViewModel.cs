namespace RealEstateApp.Core.Application.ViewModels.Chat
{
    public class ChatMessageViewModel : BaseViewModel
    {
        
        public int PropertyId { get; set; }
        public string? SenderId { get; set; }
        public string? ReceiverId { get; set; }
        public string? Message { get; set; }
        public DateTime SentAt { get; set; }
    }
}