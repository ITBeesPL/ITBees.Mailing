using System.Collections.Generic;
using System.Net.Mail;
using ITBees.Mailing.UnitTests.InMemory;
using ITBees.Models.EmailAccounts;
using ITBees.Models.Platforms;
using MailKit;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using Moq;
using NUnit.Framework;

namespace ITBees.Mailing.UnitTests
{
    public class DevEnvironmentEmailSendingServiceTests
    {
        private EmailAccount _senderEmailAccount;

        [SetUp]
        public void Setup()
        {
            _senderEmailAccount = new EmailAccount() {Email = "testplatform@plasdfafas.com", EmailFromTitle = "Test email from"};
        }

        [Test]
        public void DevEnvironmentEmailSendingService_ShouldSendEmailToAddresWhenHasPlusSign()
        {
            List<string> debugEmails = new List<string>() { "jakub+test@fasdkfad.com", "jakub2+test@fasdkfad.com" };
            var platformSettings = new Mock<IPlatformSettingsService>();
            platformSettings.Setup(x => x.GetPlatformDebugEmails()).Returns(debugEmails);
            var smtpClient = new Mock<ISmtpClient>();
            smtpClient.Setup(c => c.AuthenticationMechanisms).Returns(new HashSet<string>());
            var service = new DevEnvironmentEmailSendingService(new InMemmoryLogger<DevEnvironmentEmailSendingService>(), platformSettings.Object, smtpClient.Object);

            service.SendEmail(_senderEmailAccount, "jakub+test@gfadsfaxc.com", "test@t.com", "", "");
            
            smtpClient.Verify(x => x.Send(It.IsAny<MimeMessage>(),default,null), Times.Once);
        }

        [Test]
        public void DevEnvironmentEmailSendingService_ShouldNotSendEmailToAddresWhenHasNoPlusSign()
        {
            List<string> debugEmails = new List<string>();
            var platformSettings = new Mock<IPlatformSettingsService>();
            platformSettings.Setup(x => x.GetPlatformDebugEmails()).Returns(debugEmails);
            var smtpClient = new Mock<ISmtpClient>();
            smtpClient.Setup(c => c.AuthenticationMechanisms).Returns(new HashSet<string>());
            var service = new DevEnvironmentEmailSendingService(new InMemmoryLogger<DevEnvironmentEmailSendingService>(), platformSettings.Object, smtpClient.Object);

            service.SendEmail(_senderEmailAccount, "jakubtest@gfadsfaxc.com", "test@t.com", "", "");

            smtpClient.Verify(x => x.Send(It.IsAny<MimeMessage>(), default, null), Times.Never);
        }
    }
}