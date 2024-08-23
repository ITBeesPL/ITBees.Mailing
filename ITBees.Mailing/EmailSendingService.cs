using ITBees.Mailing.Interfaces;
using ITBees.Models.EmailAccounts;
using Microsoft.Extensions.Logging;
using MimeKit;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ITBees.Models.EmailMessages;
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

        /// <summary>
        /// Send email without attachment to specified recipient
        /// </summary>
        /// <param name="emailAccount"></param>
        /// <param name="emailMessage"></param>
        public void SendEmail(EmailAccount emailAccount, EmailMessage emailMessage)
        {
            this.SendEmail(emailAccount,new []{emailMessage.Recipients}, emailMessage.Subject, emailMessage.BodyText, emailMessage.BodyHtml, emailMessage.EmailAttachments, new []{emailMessage.ReplyToEmail});
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
            SendEmail(senderEmailAccount, recipients, subject, bodyPlainText, bodyHtml, null, null);
        }

        public void SendEmail(EmailAccount senderEmailAccount, string recipient, string subject, string bodyPlainText, string bodyHtml,
            string replyToAddresses)
        {
            this.SendEmail(senderEmailAccount, recipient, subject, bodyPlainText, bodyHtml, replyToAddresses);
        }

        public void SendEmail(EmailAccount senderEmailAccount, string recipient, string subject, string bodyPlainText,
            string bodyHtml,
            byte[] document, string documentName, string replyToAddresses)
        {
            SendEmail(senderEmailAccount, new[] {recipient}, subject, bodyPlainText, bodyHtml, null,
                new[] {replyToAddresses});
        }

        public virtual void SendEmail(EmailAccount senderEmailAccount, string[] recipients, string subject,
            string bodyPlainText,
            string bodyHtml, List<EmailAttachment> attachments, string[] replyToAddresses)
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

                message.Body = (attachments != null && attachments.Any())
                    ? CreateMimeMessageWithAttachment(attachments, body, message)
                    : body;

                SendMessage(message, senderEmailAccount);
            }
            catch (Exception e)
            {
                _logger.LogError($"Error thrown while running ProductionEmailSendingService. Message: {e.Message}");
                throw;
            }
        }

        private Multipart CreateMimeMessageWithAttachment(List<EmailAttachment> emailAttachments, MimeEntity body,
            MimeMessage message)
        {
            var bodyParts = new Multipart();
            foreach (var emailAttachment in emailAttachments)
            {
                var stream = new MemoryStream(emailAttachment.File);

                var attachment = new MimePart("application", "octet-stream")
                {
                    Content = new MimeContent(stream),
                    ContentDisposition = new ContentDisposition(ContentDisposition.Attachment),
                    ContentTransferEncoding = ContentEncoding.Base64,
                    FileName = emailAttachment.FileName
                };
                bodyParts.Add(body);
                bodyParts.Add(attachment);
            }
            
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