using ElCamino.AspNetCore.Identity.Dynamo;
using NUnit.Framework;

namespace ElCamino.AspNetCore.Identity.UnitTests
{
    [TestFixture]
    public class IdentityCloudContextTests
    {
        [Test]
        public void Ctor_Always_Creates()
        {
            var sut = MakeSut();
            Assert.IsNotNull(sut);
        }

        private IdentityCloudContext MakeSut()
        {
            return new IdentityCloudContext();
        }
    }
}
