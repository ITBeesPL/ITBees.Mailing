using ITBees.Models.EmailAccounts;
using ITBees.Models.EmailMessages;

namespace ITBees.Mailing.Interfaces
{
    public interface IEmailSendingService
    {
        void SendEmail(EmailAccount emailAccount, EmailMessage emailMessage);
        void SendEmail(EmailAccount senderEmailAccount, string recipient, string subject, string bodyPlainText, string bodyHtml);
        void SendEmail(EmailAccount senderEmailAccount, string[] recipients, string subject, string bodyPlainText, string bodyHtml);
        void SendEmail(EmailAccount senderEmailAccount, string recipient, string subject, string bodyPlainText, string bodyHtml, byte[] document, string documentName, string replyToAddresses);

        void SendEmail(EmailAccount senderEmailAccount, string[] recipients, string subject, string bodyPlainText, string bodyHtml,
            byte[] document, string documentName, string[] replyToAddresses);
    }
}