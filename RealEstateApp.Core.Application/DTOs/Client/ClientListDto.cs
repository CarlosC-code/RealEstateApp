namespace RealEstateApp.Core.Application.Dtos.Admin
{
    public class ClientListDto
    {
        public string? Id { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? FullName => $"{FirstName} {LastName}";
        public string? Email { get; set; }
        public string? UserName { get; set; }
        public bool IsActive { get; set; }
    }
}