using ITBees.Models.EmailAccounts;

namespace ITBees.Mailing.Interfaces
{
    public interface IEmailSendingService
    {
        void SendEmail(EmailAccount senderEmailAccount, string recipient, string subject, string bodyPlainText, string bodyHtml);
        void SendEmail(EmailAccount senderEmailAccount, string[] recipients, string subject, string bodyPlainText, string bodyHtml);
        void SendEmail(EmailAccount senderEmailAccount, string recipient, string subject, string bodyPlainText, string bodyHtml, byte[] document, string documentName, string replyToAddresses);

        void SendEmail(EmailAccount senderEmailAccount, string[] recipients, string subject, string bodyPlainText, string bodyHtml,
            byte[] document, string documentName, string[] replyToAddresses);
    }
}