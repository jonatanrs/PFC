using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PFC.WebApp.Services.Providers
{
    public class ConsoleEmailSender : IEmailSender
    {
        public Task SendEmailAsync(string email, string subject, string message)
        {
            Console.WriteLine($"Email: {email}");
            Console.WriteLine($"Subject: {subject}");
            Console.WriteLine($"Message: {message}");

            return Task.CompletedTask;
        }
    }
}
