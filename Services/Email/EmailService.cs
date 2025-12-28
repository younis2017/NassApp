using Microsoft.Extensions.Configuration;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace Nass.Services.Email
{
    public class EmailService : IEmailService<EmailService>
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendAsync(string to, string subject, string body)
        {
            var smtp = _configuration.GetSection("Smtp");

            using var client = new SmtpClient(smtp["Host"], int.Parse(smtp["Port"]))
            {
                Credentials = new NetworkCredential(
                    smtp["Username"],
                    smtp["Password"]
                ),
                EnableSsl = true
            };

            var mail = new MailMessage
            {
                From = new MailAddress(smtp["From"]),
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };

            mail.To.Add(to);

            await client.SendMailAsync(mail);
        }
        public async Task SendAgency(string to, string subject, string body, string bcc)
        {
            var smtp = _configuration.GetSection("Smtp");

            using var client = new SmtpClient(smtp["Host"], int.Parse(smtp["Port"]))
            {
                Credentials = new NetworkCredential(
                    smtp["Username"],
                    smtp["Password"]
                ),
                EnableSsl = true
            };

            var mail = new MailMessage
            {
                From = new MailAddress(smtp["From"]),
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };

            // TO
            mail.To.Add(to);

            // BCC (optional & safe)
            if (!string.IsNullOrWhiteSpace(bcc))
            {
                mail.Bcc.Add(bcc);
            }

            await client.SendMailAsync(mail);
        }

    }
}
