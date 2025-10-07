using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace Ade_Farming.Services
{
    public class EmailSender : IEmailSender
    {
        private readonly string _smtpHost = "smtp.gmail.com";
        private readonly int _smtpPort = 587;
        private readonly string _smtpUser; // your Gmail
        private readonly string _smtpPass; // Gmail App Password
        private readonly string _fromName;

        public EmailSender(IConfiguration configuration)
        {
            _smtpUser = configuration["EmailSettings:Email"];
            _smtpPass = configuration["EmailSettings:AppPassword"];
            _fromName = configuration["EmailSettings:FromName"]; // optional

            if (string.IsNullOrEmpty(_smtpUser) || string.IsNullOrEmpty(_smtpPass))
                throw new ArgumentNullException("SMTP Email or Password is not configured.");
        }


        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            var mail = new MailMessage
            {
                From = new MailAddress(_smtpUser, _fromName),
                Subject = subject,
                Body = htmlMessage,
                IsBodyHtml = true
            };

            mail.To.Add(email);

            using var smtp = new SmtpClient(_smtpHost, _smtpPort)
            {
                Credentials = new NetworkCredential(_smtpUser, _smtpPass),
                EnableSsl = true
            };

            await smtp.SendMailAsync(mail);
        }
    }
}
