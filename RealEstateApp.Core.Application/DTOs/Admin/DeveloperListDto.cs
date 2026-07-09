namespace RealEstateApp.Core.Application.Dtos.Admin
{
    public class DeveloperListDto
    {
        public string? Id { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? UserName { get; set; }
        public string? Email { get; set; }
        public string? IdCard { get; set; }
        public bool IsActive { get; set; }
    }
}