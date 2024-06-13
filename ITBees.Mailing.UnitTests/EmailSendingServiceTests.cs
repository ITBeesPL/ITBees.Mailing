using ITBees.Mailing.UnitTests.InMemory;
using NUnit.Framework;

namespace ITBees.Mailing.UnitTests
{

    public class EmailSendingServiceTests
    {
        [Test]
        public void ServiceCreation_shouldWork()
        {
            Assert.That(new EmailSendingService(new InMemmoryLogger<EmailSendingService>()) != null);
        }
    }
}
