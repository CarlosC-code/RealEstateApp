namespace RealEstateApp.Core.Application.Dtos.Email
{
    public class EmailRequestDto
    {
        public string? To { get; set; }
        public List<string>? ToRange { get; set; }
        public string? Subject { get; set; }
        public string? HtmlBody { get; set; }
    }
}