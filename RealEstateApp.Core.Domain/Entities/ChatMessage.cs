using RealEstateApp.Core.Domain.Common;

namespace RealEstateApp.Core.Domain.Entities
{
    public class ChatMessage : BaseEntity
    {
        public int PropertyId { get; set; }
        public string? SenderId { get; set; }
        public string? ReceiverId { get; set; }
        public string? Message { get; set; }
        public DateTime SentAt { get; set; }
        public Property? Property { get; set; }
    }
}