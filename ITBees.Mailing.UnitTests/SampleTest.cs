using System;
using NUnit.Framework;

namespace ITBees.Mailing.UnitTests
{

    public class SampleTest
    {
        [Test]
        public void SampleTestShouldReturnTrue()
        {
            Assert.True(new Mailing() != null);
        }
    }
}
