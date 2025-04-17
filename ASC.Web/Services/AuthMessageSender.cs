using ASC.Solution.Configuration;
using Microsoft.Extensions.Options;
using MimeKit;
using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace ASC.Web.Services
{
    public class AuthMessageSender : IEmailSender, ISmsSender
    {
        private IOptions<ApplicationSettings> _settings;
        public AuthMessageSender(IOptions<ApplicationSettings> settings)
        {
            _settings = settings;
        }
        public Task SendSmsAsync(string number, string message)
        {
            return Task.FromResult(0);
        }

        public async Task SendEmailAsync(string email, string subject, string message)
        {
            var emailMessage = new MimeMessage();
            emailMessage.From.Add(new MailboxAddress("admin", _settings.Value.SMTPAccount));
            emailMessage.To.Add(new MailboxAddress("user", email));
            emailMessage.Subject = subject;
            emailMessage.Body = new TextPart("plain") { Text = message };

            using (var client = new SmtpClient())
            {
                client.ServerCertificateValidationCallback = (s, c, h, e) => true;
                await client.ConnectAsync(_settings.Value.SMTPServer, _settings.Value.SMTPPort, false);
                await client.AuthenticateAsync(_settings.Value.SMTPAccount, _settings.Value.SMTPPassword);
                await client.SendAsync(emailMessage);
                await client.DisconnectAsync(true);
            }
        }

    }
}