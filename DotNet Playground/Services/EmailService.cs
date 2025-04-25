using MailKit.Net.Smtp;
using MimeKit;

namespace DotNet_Playground.Services
{
    public class EmailService
    {
        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            var email = new MimeMessage();
            email.From.Add(MailboxAddress.Parse("del.fake01@gmail.com")); // sender email
            email.To.Add(MailboxAddress.Parse(toEmail));
            email.Subject = subject;

            var builder = new BodyBuilder { HtmlBody = body };
            email.Body = builder.ToMessageBody();

            using var smtp = new SmtpClient();
            await smtp.ConnectAsync("smtp.gmail.com", 587, MailKit.Security.SecureSocketOptions.StartTls);
            await smtp.AuthenticateAsync("del.fake01@gmail.com", "vjinbqqwrpuhaqpo");
            await smtp.SendAsync(email);
            await smtp.DisconnectAsync(true);
        }
    }
}
