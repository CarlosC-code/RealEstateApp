using RealEstateApp.Core.Application.Dtos.Email;

namespace RealEstateApp.Core.Application.Interfaces
{
    public interface IEmailService
    {
        Task SendAsync(EmailRequestDto emailRequestDto);
    }
}