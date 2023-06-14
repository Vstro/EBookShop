using System;
using System.Threading.Tasks;
using BookHaven.Models;

namespace BookHaven.Services
{
    public interface IEmailService
    {
        Task SendEmailAsync(String email, String subject, String message, Base64File[] attachments = null);
    }
}
