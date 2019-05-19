using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;

namespace PFC.WebApp.Services.Providers
{
    // This class is used by the application to send email for account confirmation and password reset.
    // For more details see https://go.microsoft.com/fwlink/?LinkID=532713
    public class EmailSender : IEmailSender
    {
        private readonly string host;
        private readonly int port;
        private readonly string user;
        private readonly string password;

        public EmailSender(string host, int port, string user, string password)
        {
            this.host = host;
            this.port = port;
            this.user = user;
            this.password = password;
        }

        public Task SendEmailAsync(string email, string subject, string message)
        {
            var smtpClient = new SmtpClient(host, port);

            smtpClient.Credentials = new System.Net.NetworkCredential(user, password);
            smtpClient.EnableSsl = true;

            return smtpClient.SendMailAsync(new MailMessage(user, email, subject, message)
            {
                IsBodyHtml = true
            })
            .ContinueWith(task =>
            {
                smtpClient.Dispose();
            });
        }
    }
}
