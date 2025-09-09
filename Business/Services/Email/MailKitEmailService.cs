using Business.Interfaces.Email;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using MimeKit;
using System;
using System.Net;
using System.Threading.Tasks;

public class MailKitActivationEmailService : IIActivationEmailService
{
    private readonly IConfiguration _configuration;
    private readonly string _host;
    private readonly string _username;
    private readonly string _password;
    private readonly int _port;

    public MailKitActivationEmailService(IConfiguration configuration)
    {
        _configuration = configuration;
        _host = _configuration["Email:Host"]!;
        _username = _configuration["Email:Username"]!;
        _password = _configuration["Email:Password"]!;
        _port = int.TryParse(_configuration["Email:Port"], out int p) ? p : 587;
    }

    public async Task SendEmailAsync(string toEmail, string fullName, string activationCode)
    {
        try
        {
            var encodedCode = WebUtility.UrlEncode(activationCode);
            var activationLink = $"https://localhost:7211/account/activate?code={encodedCode}";
            var mimeMessage = new MimeMessage();
            mimeMessage.From.Add(new MailboxAddress("Staj Takip Sistemi", _username));
            mimeMessage.To.Add(MailboxAddress.Parse(toEmail));
            mimeMessage.Subject = "Hesabınızı Aktifleştirin";
            var body = $@"
            <p>Merhaba <strong>{WebUtility.HtmlEncode(fullName)}</strong>,</p>
            <p>Hesabınızı aktifleştirmek için aşağıdaki bağlantıya tıklayın:</p>
            <p><a href='{activationLink}' 
                  style='display:inline-block;padding:10px 20px;background:#4CAF50;color:#fff;
                         text-decoration:none;border-radius:6px;'>Hesabı Aktifleştir</a></p>
            <p>Bu bağlantı 24 saat boyunca geçerlidir.</p>
            <hr/>
            <small>Bu maili siz göndermediyseniz lütfen dikkate almayınız.</small>
        ";
            mimeMessage.Body = new BodyBuilder
            {
                HtmlBody = body,
                TextBody = $"Merhaba {fullName}, Hesabınızı aktifleştirmek için şu linki kullanın: {activationLink}"
            }.ToMessageBody();
            using var smtp = new SmtpClient();
            await smtp.ConnectAsync(_host, _port, SecureSocketOptions.StartTls);
            await smtp.AuthenticateAsync(_username, _password);
            await smtp.SendAsync(mimeMessage);
            await smtp.DisconnectAsync(true);
        }
        catch (Exception ex)
        {
            // Hata loglama
            Console.WriteLine($"Aktivasyon maili gönderim hatası: {ex.Message}");
            throw;
        }
    }

    public async Task SendEmailForgetPassAsync(string toEmail, string token, string resetUrl, string fullName)
    {
        var encodedToken = WebUtility.UrlEncode(token);
        var encodedEmail = WebUtility.UrlEncode(toEmail); 

        var resetLink = $"{resetUrl}?token={encodedToken}&email={encodedEmail}";

        var mimeMessage = new MimeMessage();
        mimeMessage.From.Add(new MailboxAddress("Staj Takip Sistemi", _username));
        mimeMessage.To.Add(MailboxAddress.Parse(toEmail));
        mimeMessage.Subject = "Şifre Sıfırlama Talebi";

        var body = $@"
        <p>Merhaba <strong>{WebUtility.HtmlEncode(fullName)}</strong>,</p>
        <p>Şifrenizi sıfırlamak için aşağıdaki bağlantıya tıklayın:</p>
        <p><a href='{resetLink}' 
              style='display:inline-block;padding:10px 20px;background:#007bff;color:#fff;
                     text-decoration:none;border-radius:6px;'>Şifreyi Sıfırla</a></p>
        <p>Bu bağlantı <strong>3 saat</strong> boyunca geçerlidir.</p>
    ";

        mimeMessage.Body = new BodyBuilder
        {
            HtmlBody = body,
            TextBody = $"Merhaba {fullName}, şifrenizi sıfırlamak için şu linki kullanın: {resetLink} (3 saat geçerli)"
        }.ToMessageBody();

        using var smtp = new SmtpClient();
        await smtp.ConnectAsync(_host, _port, SecureSocketOptions.StartTls);
        await smtp.AuthenticateAsync(_username, _password);
        await smtp.SendAsync(mimeMessage);
        await smtp.DisconnectAsync(true);
    }

}
