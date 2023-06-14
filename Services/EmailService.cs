using MimeKit;
using MailKit.Net.Smtp;
using System.Threading.Tasks;
using System;
using BookHaven.Models;
using Microsoft.Extensions.Configuration;

namespace BookHaven.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;

        public EmailService(IConfiguration config)
        {
            _config = config;
        }

        public async Task SendEmailAsync(String email, String subject, String message, Base64File[] attachments = null)
        {
            var officialEmail = _config.GetValue<string>("OfficialEmail:Email");
            var password = _config.GetValue<string>("OfficialEmail:Password");
            var emailMessage = new MimeMessage();

            emailMessage.From.Add(new MailboxAddress("BookHaven Administration", officialEmail));
            emailMessage.To.Add(new MailboxAddress("", email));
            emailMessage.Subject = subject;
            if (attachments == null)
            {
                emailMessage.Body = new TextPart(MimeKit.Text.TextFormat.Html)
                {
                    Text = message
                };
            }
            else
            {
                var builder = new BodyBuilder();
                builder.HtmlBody = message;
                foreach (var attachment in attachments)
                {
                    builder.Attachments.Add(attachment.FileName, Convert.FromBase64String(attachment.Data));
                }
                emailMessage.Body = builder.ToMessageBody();
            }

            using (var client = new SmtpClient())
            {
                await client.ConnectAsync("smtp.gmail.com", 465, true);
                await client.AuthenticateAsync(officialEmail, password);
                await client.SendAsync(emailMessage);
                await client.DisconnectAsync(true);
            }
        }
    }
}