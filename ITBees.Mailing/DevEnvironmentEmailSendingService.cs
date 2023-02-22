using System.Collections.Generic;
using ITBees.Interfaces.Platforms;
using ITBees.Models.EmailAccounts;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Logging;

namespace ITBees.Mailing
{
    public class DevEnvironmentEmailSendingService : EmailSendingService
    {
        private readonly ILogger<EmailSendingService> _logger;
        private readonly IPlatformSettingsService _platformSettingsService;

        public DevEnvironmentEmailSendingService(ILogger<EmailSendingService> logger,
            IPlatformSettingsService platformSettingsService, ISmtpClient smtpClient = null) : base(logger, smtpClient)
        {
            _logger = logger;
            _platformSettingsService = platformSettingsService;
        }

        public override void SendEmail(EmailAccount senderEmailAccount, string[] recipients, string subject, string bodyPlainText,
            string bodyHtml, byte[] document, string documentName, string[] replyToAddresses)
        {
            List<string> platformDebugEmails = _platformSettingsService.GetPlatformDebugEmails();
            
            var debugRecipients = new List<string>();
            foreach (var recipient in recipients)
            {
                if (platformDebugEmails.Contains(recipient) || recipient.Contains("+test"))
                {
                    debugRecipients.Add(recipient);
                }
                else
                {
                    _logger.LogInformation($"Debug send email (only console) to {recipient} subject {subject} \r\n{bodyPlainText}");
                }
            }

            if (debugRecipients.Count > 0)
                base.SendEmail(senderEmailAccount, debugRecipients.ToArray(), subject, bodyPlainText, bodyHtml, document, documentName, replyToAddresses);
        }
    }
}