using RealEstateApp.Core.Application.Dtos.Email;
using RealEstateApp.Core.Domain.Settings;
using Microsoft.Extensions.Options;
using MimeKit;
using Microsoft.Extensions.Logging;
using RealEstateApp.Core.Application.Interfaces;
using MailKit.Security;
using MailKit.Net.Smtp;

namespace RealEstateApp.Infrastructure.Shared.Services
{
    public class EmailService : IEmailService
    {
        private readonly MailSettings _mailSettings;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IOptions<MailSettings> mailSettings, ILogger<EmailService> logger)
        {
            _mailSettings = mailSettings.Value;
            _logger = logger;
        }

        public async Task SendAsync(EmailRequestDto emailRequestDto)
        {
            try
            {
                MimeMessage email = new()
                {
                    Sender = MailboxAddress.Parse(_mailSettings.EmailFrom ?? ""),
                    Subject = emailRequestDto.Subject ?? ""
                };

                if (!string.IsNullOrEmpty(emailRequestDto.To))
                    email.To.Add(MailboxAddress.Parse(emailRequestDto.To));

                if (emailRequestDto.ToRange != null)
                {
                    foreach (var toItem in emailRequestDto.ToRange)
                    {
                        if (!string.IsNullOrEmpty(toItem))
                            email.To.Add(MailboxAddress.Parse(toItem));
                    }
                }

                BodyBuilder builder = new()
                {
                    HtmlBody = emailRequestDto.HtmlBody
                };
                email.Body = builder.ToMessageBody();

                using SmtpClient smtpClient = new();
                smtpClient.ServerCertificateValidationCallback = (s, c, h, e) => true;
                await smtpClient.ConnectAsync(_mailSettings.SmtpHost, _mailSettings.SmtpPort,
                    SecureSocketOptions.StartTls);
                await smtpClient.AuthenticateAsync(_mailSettings.SmtpUser, _mailSettings.SmtpPass);
                await smtpClient.SendAsync(email);
                await smtpClient.DisconnectAsync(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An exception occured {Exception}.", ex);
            }
        }
    }
}