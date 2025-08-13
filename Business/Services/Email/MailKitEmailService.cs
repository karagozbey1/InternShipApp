
using Business.Interfaces.Email;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using MimeKit;
using System.Threading.Tasks;

namespace Business.Services.Email
{
    public class MailKitActivationEmailService : IIActivationEmailService
    {
        private readonly IConfiguration _configuration;

        public MailKitActivationEmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendEmailAsync(string toEmail, string fullName, string activationCode)
        {
            var host = _configuration["Email:Host"];
            var username = _configuration["Email:Username"];
            var password = _configuration["Email:Password"];
            var portSetting = _configuration["Email:Port"];
            int port = 587;
            if (!string.IsNullOrEmpty(portSetting) && int.TryParse(portSetting, out var p))
                port = p;

            var activationLink = $"https://localhost:7211/account/activate?code={activationCode}";

            var mimeMessage = new MimeMessage();
            mimeMessage.From.Add(MailboxAddress.Parse(username));
            mimeMessage.To.Add(MailboxAddress.Parse(toEmail));
            mimeMessage.Subject = "Hesabınızı Aktifleştirin";

            var message = $@"
                Merhaba {fullName},<br/><br/>
                Hesabınızı aktifleştirmek için aşağıdaki bağlantıya tıklayın:<br/>
                <a href='{activationLink}'>Hesabı Aktifleştir</a><br/><br/>
                Bu bağlantı 24 saat boyunca geçerlidir.
            ";

            var bodyBuilder = new BodyBuilder { HtmlBody = message };
            mimeMessage.Body = bodyBuilder.ToMessageBody();

            using var smtpClient = new SmtpClient();

            try
            {
                await smtpClient.ConnectAsync(host, port, SecureSocketOptions.StartTls);
                await smtpClient.AuthenticateAsync(username, password);
                await smtpClient.SendAsync(mimeMessage);
                await smtpClient.DisconnectAsync(true);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("E-posta gönderilirken hata oluştu: " + ex.Message, ex);
            }
        }
    }
}
