using System.Collections.Generic;
using System.IO;
using System.Threading;
using ITBees.Mailing.UnitTests.InMemory;
using ITBees.Models.EmailAccounts;
using ITBees.Models.EmailMessages;
using MailKit;
using MailKit.Net.Smtp;
using MimeKit;
using Moq;
using NUnit.Framework;

namespace ITBees.Mailing.UnitTests
{

    public class EmailSendingServiceTests
    {
        private const string BodyHtml = "<p>Test body</p>";

        [Test]
        public void ServiceCreation_shouldWork()
        {
            Assert.That(new EmailSendingService(new InMemmoryLogger<EmailSendingService>()) != null);
        }

        [Test]
        public void SendEmail_ShouldAddBodyOnceFollowedByAttachmentsInOrderWhenHasTwoAttachments()
        {
            var invoice = new EmailAttachment() { File = new byte[] { 1, 2, 3 }, FileName = "invoice.pdf" };
            var qrCode = new EmailAttachment() { File = new byte[] { 4, 5, 6, 7 }, FileName = "qr-code.png" };

            var sentMessage = SendEmailAndCaptureSentMessage(new List<EmailAttachment>() { invoice, qrCode });

            var multipart = GetMultipartMixedBody(sentMessage);
            Assert.That(multipart.Count, Is.EqualTo(3));
            AssertIsBody(multipart[0]);
            AssertIsAttachment(multipart[1], invoice);
            AssertIsAttachment(multipart[2], qrCode);
        }

        [Test]
        public void SendEmail_ShouldAddBodyFollowedByAttachmentWhenHasSingleAttachment()
        {
            var invoice = new EmailAttachment() { File = new byte[] { 1, 2, 3 }, FileName = "invoice.pdf" };

            var sentMessage = SendEmailAndCaptureSentMessage(new List<EmailAttachment>() { invoice });

            var multipart = GetMultipartMixedBody(sentMessage);
            Assert.That(multipart.Count, Is.EqualTo(2));
            AssertIsBody(multipart[0]);
            AssertIsAttachment(multipart[1], invoice);
        }

        [Test]
        public void SendEmail_ShouldSendBodyWithoutMultipartWhenAttachmentsAreNull()
        {
            var sentMessage = SendEmailAndCaptureSentMessage(null);

            AssertIsBody(sentMessage.Body);
        }

        [Test]
        public void SendEmail_ShouldSendBodyWithoutMultipartWhenAttachmentsListIsEmpty()
        {
            var sentMessage = SendEmailAndCaptureSentMessage(new List<EmailAttachment>());

            AssertIsBody(sentMessage.Body);
        }

        private static MimeMessage SendEmailAndCaptureSentMessage(List<EmailAttachment> attachments)
        {
            MimeMessage sentMessage = null;
            var smtpClient = new Mock<ISmtpClient>();
            smtpClient.Setup(c => c.AuthenticationMechanisms).Returns(new HashSet<string>());
            smtpClient.Setup(c => c.Send(It.IsAny<MimeMessage>(), It.IsAny<CancellationToken>(), It.IsAny<ITransferProgress>()))
                .Callback<MimeMessage, CancellationToken, ITransferProgress>((message, _, _) => sentMessage = message);
            var senderEmailAccount = new EmailAccount() { Email = "sender@example.com", EmailFromTitle = "Test email from" };
            var service = new EmailSendingService(new InMemmoryLogger<EmailSendingService>(), smtpClient.Object);

            service.SendEmail(senderEmailAccount, new[] { "recipient@example.com" }, "Test subject", "Test body", BodyHtml, attachments, null);

            smtpClient.Verify(x => x.Send(It.IsAny<MimeMessage>(), default, null), Times.Once);
            return sentMessage;
        }

        private static Multipart GetMultipartMixedBody(MimeMessage message)
        {
            var multipart = message.Body as Multipart;
            Assert.That(multipart, Is.Not.Null);
            Assert.That(multipart.ContentType.MimeType, Is.EqualTo("multipart/mixed"));
            return multipart;
        }

        private static void AssertIsBody(MimeEntity part)
        {
            var body = part as TextPart;
            Assert.That(body, Is.Not.Null);
            Assert.That(body.IsAttachment, Is.False);
            Assert.That(body.ContentType.MimeType, Is.EqualTo("text/html"));
            Assert.That(body.Text, Is.EqualTo(BodyHtml));
        }

        private static void AssertIsAttachment(MimeEntity part, EmailAttachment expectedAttachment)
        {
            var attachment = part as MimePart;
            Assert.That(attachment, Is.Not.Null);
            Assert.That(attachment.IsAttachment, Is.True);
            Assert.That(attachment.FileName, Is.EqualTo(expectedAttachment.FileName));
            using (var content = new MemoryStream())
            {
                attachment.Content.DecodeTo(content);
                Assert.That(content.ToArray(), Is.EqualTo(expectedAttachment.File));
            }
        }
    }
}
