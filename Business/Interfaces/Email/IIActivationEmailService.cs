using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.Interfaces.Email
{
    public interface IIActivationEmailService
    {
        Task SendEmailAsync(string toEmail, string fullName, string activationCode);
        Task SendEmailForgetPassAsync(string toEmail, string token, string resetUrl, string fullName);

    }
}
