namespace RealEstateApp.Core.Application.Dtos.Agent
{
    public class AgentDto
    {
        public string? Id { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? FullName => $"{FirstName} {LastName}";
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? PhotoUrl { get; set; }
        public bool IsActive { get; set; }
        public int PropertyCount { get; set; }
    }
}