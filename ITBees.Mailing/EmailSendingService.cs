using ITBees.Mailing.Interfaces;
using ITBees.Models.EmailAccounts;
using Microsoft.Extensions.Logging;
using MimeKit;
using System;
using System.IO;
using MailKit.Net.Smtp;

namespace ITBees.Mailing
{
    public class EmailSendingService : IEmailSendingService
    {
        private readonly ILogger<EmailSendingService> _logger;
        private readonly ISmtpClient _smtpClient;
        private EmailAccount _lastUsedSenderEmailAccount;

        public EmailSendingService(ILogger<EmailSendingService> logger)
        {
            _logger = logger;
            _smtpClient = new SmtpClient();
        }

        public EmailSendingService(ILogger<EmailSendingService> logger, ISmtpClient smtpClient)
        {
            _logger = logger;
            _smtpClient = smtpClient;
        }

        public void SendEmail(EmailAccount senderEmailAccount, string recipient, string subject, string bodyPlainText,
            string bodyHtml)
        {
            SendEmail(senderEmailAccount, new[] {recipient}, subject, bodyPlainText, bodyHtml);
        }

        public void SendEmail(EmailAccount senderEmailAccount, string[] recipients, string subject,
            string bodyPlainText,
            string bodyHtml)
        {
            SendEmail(senderEmailAccount, recipients, subject, bodyPlainText, bodyHtml, null, null, null);
        }

        public void SendEmail(EmailAccount senderEmailAccount, string recipient, string subject, string bodyPlainText,
            string bodyHtml,
            byte[] document, string documentName, string replyToAddresses)
        {
            SendEmail(senderEmailAccount, new[] {recipient}, subject, bodyPlainText, bodyHtml, document, documentName,
                new[] {replyToAddresses});
        }

        public virtual void SendEmail(EmailAccount senderEmailAccount, string[] recipients, string subject,
            string bodyPlainText,
            string bodyHtml, byte[] document, string documentName, string[] replyToAddresses)
        {
            try
            {
                var message = new MimeMessage();

                var senderName = senderEmailAccount.EmailFromTitle;
                var address = senderEmailAccount.Email;

                foreach (var recipient in recipients)
                {
                    message.To.Add(new MailboxAddress(recipient, recipient));
                }

                message.From.Add(new MailboxAddress(senderName, address));

                message.Subject = subject;

                if (replyToAddresses != null)
                {
                    foreach (var replyTo in replyToAddresses)
                    {
                        if(replyTo !=null) 
                            message.ReplyTo.Add(new MailboxAddress(replyTo, replyTo));
                    }
                }

                var body = new TextPart();
                if (string.IsNullOrEmpty(bodyHtml))
                {
                    body = new TextPart("plain")
                    {
                        Text = bodyPlainText
                    };
                }
                else
                {
                    body = new TextPart("html")
                    {
                        Text = bodyHtml,
                    };
                }

                message.Body = string.IsNullOrEmpty(documentName)
                    ? body
                    : CreateMimeMessageWithAttachment(document, documentName, body, message);

                SendMessage(message, senderEmailAccount);
            }
            catch (Exception e)
            {
                _logger.LogError($"Error thrown while running ProductionEmailSendingService. Message: {e.Message}");
                throw;
            }
        }

        private Multipart CreateMimeMessageWithAttachment(byte[] document, string documentName, MimeEntity body,
            MimeMessage message)
        {
            var bodyParts = new Multipart();

            var stream = new MemoryStream(document);

            var attachment = new MimePart("application", "octet-stream")
            {
                Content = new MimeContent(stream),
                ContentDisposition = new ContentDisposition(ContentDisposition.Attachment),
                ContentTransferEncoding = ContentEncoding.Base64,
                FileName = documentName
            };

            bodyParts.Add(body);
            bodyParts.Add(attachment);
            return bodyParts;
        }

        private void SendMessage(MimeMessage message, EmailAccount senderEmailAccount)
        {
            if (_lastUsedSenderEmailAccount !=null && _lastUsedSenderEmailAccount != senderEmailAccount)
            {
                _smtpClient.Disconnect(true);
            }

            if (_smtpClient.IsConnected == false)
            {
                var smtpHost = senderEmailAccount.SmtpServer;
                var smtpPort = Convert.ToInt32(senderEmailAccount.SmtpPort);
                var useSsl = senderEmailAccount.UseSSL;
                _smtpClient.Connect(smtpHost, smtpPort, useSsl);
                _smtpClient.AuthenticationMechanisms.Remove("XOAUTH2");
                _lastUsedSenderEmailAccount = senderEmailAccount;
            }

            if (_smtpClient.IsAuthenticated == false)
            {
                var userName = senderEmailAccount.Email;
                var password = senderEmailAccount.Pass;
                _smtpClient.Authenticate(userName, password);
            }

            _smtpClient.Send(message);
        }
    }
}