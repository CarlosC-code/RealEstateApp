namespace RealEstateApp.Core.Application.Dtos.Chat
{
    public class ChatMessageDto
    {
        public int Id { get; set; }
        public int PropertyId { get; set; }
        public string? SenderId { get; set; }
        public string? SenderName { get; set; }
        public string? ReceiverId { get; set; }
        public string? Message { get; set; }
        public DateTime SentAt { get; set; }
        public bool IsFromClient { get; set; }
    }
}