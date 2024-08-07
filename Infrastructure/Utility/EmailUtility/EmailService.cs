using Microsoft.Extensions.Options;
using MimeKit;
using MailKit.Net.Smtp;
using MailKit.Security;

namespace Infrastructure.Utility.EmailUtility
{
    public class EmailService : IEmailService
    {
        private readonly EmailSettings _emailsettings;

        public EmailService( IOptions<EmailSettings> options)
        {
            this._emailsettings=options.Value;
        }

        public async Task SendEmail(MailRequest mailRequest)
        {
            var email = new MimeMessage
            {
                Sender = MailboxAddress.Parse(_emailsettings.SmtpUser),
                Subject = mailRequest.EmailSubject
            };

            email.To.Add(MailboxAddress.Parse(mailRequest.Email));

            var builder = new BodyBuilder { HtmlBody = mailRequest.EmailBody };
            email.Body = builder.ToMessageBody();

            using var smtpClient = new SmtpClient();
            try
            {
                await smtpClient.ConnectAsync(_emailsettings.SmtpServer, _emailsettings.SmtpPort, SecureSocketOptions.StartTls);
                await smtpClient.AuthenticateAsync(_emailsettings.SmtpUser, _emailsettings.SmtpPassword);
                await smtpClient.SendAsync(email);
            }
            catch (Exception ex)
            {
                // Handle exceptions related to SMTP connection or authentication
                throw new ApplicationException($"An error occurred while sending the email: {ex.Message}", ex);
            }
            finally
            {
                await smtpClient.DisconnectAsync(true);
            }
        }

    }
}
