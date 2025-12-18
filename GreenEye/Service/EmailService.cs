using GreenEye.Service.IService;
using System.Net.Mail;
using System.Net;

namespace GreenEye.Service
{
    public class EmailService(IConfiguration _configuration) : IEmailService
    {
        public async Task SendEmailAsync(string to, string subject, string body)
        {
            var port = int.Parse(_configuration["SMTP:Port"]!);
            string host = _configuration["SMTP:Host"]!;
            var email = _configuration["SMTP:Email"];
            var password = _configuration["SMTP:Password"];

            using var smtp = new SmtpClient(host)
            {
                Port = port,
                Credentials = new NetworkCredential(email, password),
                EnableSsl = true
            };

            var mail = new MailMessage
            {
                From = new MailAddress(email!),
                Subject = subject,
                Body = body,
                IsBodyHtml = true,
            };
            mail.To.Add(to);

            await smtp.SendMailAsync(mail);
        }

    }
}
