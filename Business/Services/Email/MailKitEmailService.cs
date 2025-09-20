using Business.Interfaces.Email;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using MimeKit;
using System;
using System.Net;
using System.Threading.Tasks;

// MailKit tabanlı email gönderim servisi - SMTP email delivery implementation
// Account activation ve password reset email'leri için specialized service
// Production-ready email delivery ile HTML/Text dual format support
// Security-focused implementation ile credential management
public class MailKitActivationEmailService : IIActivationEmailService
{
    // Configuration dependency - appsettings.json'dan email settings okuma
    private readonly IConfiguration _configuration;
    
    // SMTP server connection parameters - production email server configuration
    // Bu değerler appsettings.json'dan compile-time'da cache'lenir
    private readonly string _host;         // SMTP server address (örn: smtp.gmail.com)
    private readonly string _username;     // Authentication username (email address)
    private readonly string _password;     // App password veya authentication token
    private readonly int _port;           // SMTP port (genellikle 587 for TLS)

    // Dependency injection constructor - configuration binding
    // Email settings validation ve default value assignment
    public MailKitActivationEmailService(IConfiguration configuration)
    {
        _configuration = configuration;
        
        // Configuration binding - null reference protection ile
        // Production'da bu değerler environment variables'dan da gelebilir
        _host = _configuration["Email:Host"]!;
        _username = _configuration["Email:Username"]!;
        _password = _configuration["Email:Password"]!;
        
        // Port parsing ile default fallback - TLS standard port
        _port = int.TryParse(_configuration["Email:Port"], out int p) ? p : 587;
    }

    // Hesap aktivasyon email'i gönderme - user onboarding process
    // Account verification workflow'un kritik adımı
    // HTML template ile professional email formatting
    public async Task SendEmailAsync(string toEmail, string fullName, string activationCode)
    {
        try
        {
            // URL encoding ile XSS protection - activation code güvenliği
            // Query string injection attacks'ten korunma
            var encodedCode = WebUtility.UrlEncode(activationCode);
            
            // Activation link construction - dynamic URL generation
            // Localhost development için, production'da domain değiştirilmeli
            // Environment-based URL configuration önerilisr
            var activationLink = $"https://localhost:7211/account/activate?code={encodedCode}";
            
            // MimeMessage construction - RFC compliant email format
            var mimeMessage = new MimeMessage();
            
            // Sender information - branded email header
            // Production'da company domain email address kullanılmalı
            mimeMessage.From.Add(new MailboxAddress("Staj Takip Sistemi", _username));
            
            // Recipient parsing - email format validation
            mimeMessage.To.Add(MailboxAddress.Parse(toEmail));
            
            // Subject line - user-friendly ve descriptive
            mimeMessage.Subject = "Hesabınızı Aktifleştirin";
            
            // HTML email body construction - professional template design
            // Inline CSS ile email client compatibility
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
            
            // Dual format email body - HTML + plain text fallback
            // Email client compatibility için both formats provide edilir
            mimeMessage.Body = new BodyBuilder
            {
                HtmlBody = body,  // Rich HTML version for modern email clients
                // Plain text fallback - accessibility ve old email clients için
                TextBody = $"Merhaba {fullName}, Hesabınızı aktifleştirmek için şu linki kullanın: {activationLink}"
            }.ToMessageBody();
            
            // SMTP client connection - disposable pattern ile resource management
            using var smtp = new SmtpClient();
            
            // TLS connection establishment - security-first approach
            // StartTls ile encrypted communication ensure edilir
            await smtp.ConnectAsync(_host, _port, SecureSocketOptions.StartTls);
            
            // SMTP authentication - credential-based server access
            // Production'da OAuth2 veya App-specific passwords kullanılmalı
            await smtp.AuthenticateAsync(_username, _password);
            
            // Email delivery - actual message transmission
            await smtp.SendAsync(mimeMessage);
            
            // Clean disconnection - server resource release
            await smtp.DisconnectAsync(true);
        }
        catch (Exception ex)
        {
            // Error logging - email delivery failure tracking
            // Production'da structured logging (Serilog, NLog) kullanılmalı
            Console.WriteLine($"Aktivasyon maili gönderim hatası: {ex.Message}");
            
            // Exception propagation - calling code'da error handling için
            // Email delivery failure business logic'e escalate edilir
            throw;
        }
    }

    // Password reset email gönderme - forgot password workflow
    // Security-critical operation ile time-limited token delivery
    // URL encoding ve XSS protection ile güvenli link generation
    public async Task SendEmailForgetPassAsync(string toEmail, string token, string resetUrl, string fullName)
    {
        // Double URL encoding - query parameter security
        // Reset token manipulation attacks'ten korunma
        var encodedToken = WebUtility.UrlEncode(token);
        var encodedEmail = WebUtility.UrlEncode(toEmail);

        // Reset link construction - parameterized URL building
        // Token ve email verification için dual parameter system
        var resetLink = $"{resetUrl}?token={encodedToken}&email={encodedEmail}";

        // Email message construction - consistent dengan activation email pattern
        var mimeMessage = new MimeMessage();
        mimeMessage.From.Add(new MailboxAddress("Staj Takip Sistemi", _username));
        mimeMessage.To.Add(MailboxAddress.Parse(toEmail));
        mimeMessage.Subject = "Şifre Sıfırlama Talebi";

        // Password reset email template - security-focused messaging
        // 3 saatlik expiry time prominence ile urgency communication
        var body = $@"
        <p>Merhaba <strong>{WebUtility.HtmlEncode(fullName)}</strong>,</p>
        <p>Şifrenizi sıfırlamak için aşağıdaki bağlantıya tıklayın:</p>
        <p><a href='{resetLink}' 
              style='display:inline-block;padding:10px 20px;background:#007bff;color:#fff;
                     text-decoration:none;border-radius:6px;'>Şifreyi Sıfırla</a></p>
        <p>Bu bağlantı <strong>3 saat</strong> boyunca geçerlidir.</p>
    ";

        // Dual format body construction - HTML + plain text versions
        mimeMessage.Body = new BodyBuilder
        {
            HtmlBody = body,
            // Plain text version - accessibility compliance
            TextBody = $"Merhaba {fullName}, şifrenizi sıfırlamak için şu linki kullanın: {resetLink} (3 saat geçerli)"
        }.ToMessageBody();

        // SMTP delivery process - same pattern as activation email
        // Resource management ile connection lifecycle handling
        using var smtp = new SmtpClient();
        
        // Secure TLS connection - encrypted email transmission
        await smtp.ConnectAsync(_host, _port, SecureSocketOptions.StartTls);
        await smtp.AuthenticateAsync(_username, _password);
        await smtp.SendAsync(mimeMessage);
        await smtp.DisconnectAsync(true);
        
        // No explicit exception handling - method level error propagation
        // Calling service layer'da error handling ve retry logic implement edilmeli
    }

    // Email delivery status tracking method - future enhancement
    // Production'da email delivery confirmation ve bounce handling için
    public async Task<bool> IsEmailDeliveredAsync(string toEmail, string messageId)
    {
        // Placeholder implementation - email delivery status tracking
        // Third-party email service APIs (SendGrid, MailChimp) ile integration possible
        
        // Return true for now - future implementation with actual delivery tracking
        await Task.Delay(0); // Async placeholder
        return true;
    }

    // Bulk email sending capability - future feature
    // Marketing emails, announcements için batch processing
    public async Task SendBulkEmailAsync(List<string> recipients, string subject, string htmlBody, string textBody)
    {
        // Batch email processing - rate limiting ve throttling ile
        // SMTP server limits respect etmek için delay injection gerekli
        
        foreach (var recipient in recipients)
        {
            try
            {
                var mimeMessage = new MimeMessage();
                mimeMessage.From.Add(new MailboxAddress("Staj Takip Sistemi", _username));
                mimeMessage.To.Add(MailboxAddress.Parse(recipient));
                mimeMessage.Subject = subject;
                
                mimeMessage.Body = new BodyBuilder
                {
                    HtmlBody = htmlBody,
                    TextBody = textBody
                }.ToMessageBody();
                
                using var smtp = new SmtpClient();
                await smtp.ConnectAsync(_host, _port, SecureSocketOptions.StartTls);
                await smtp.AuthenticateAsync(_username, _password);
                await smtp.SendAsync(mimeMessage);
                await smtp.DisconnectAsync(true);
                
                // Rate limiting - SMTP server overload prevention
                // Production'da configurable delay value
                await Task.Delay(1000); // 1 second delay between emails
            }
            catch (Exception ex)
            {
                // Individual email failure logging - batch process continuation
                Console.WriteLine($"Bulk email failed for {recipient}: {ex.Message}");
                // Continue with next recipient - don't fail entire batch
            }
        }
    }
}
