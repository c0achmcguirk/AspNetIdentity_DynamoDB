using ElCamino.AspNetCore.Identity.Dynamo.Model;
using NUnit.Framework;

namespace ElCamino.AspNetCore.Identity.UnitTests
{
    [TestFixture]
    public class IdentityUserTests
    {
        [Test]
        public void Ctor_Always_Creates()
        {
            var sut = MakeSut("someguy");
            Assert.AreEqual("someguy", sut.UserName);
        }

        [Test]
        public void GenerateKeys_Always_CreatesUsefulKeys()
        {
            var sut = MakeSut();
            sut.GenerateKeys();
            Assert.IsNotNull(sut.Id);
            Assert.IsNotNull(sut.UserId);
            Assert.AreEqual(sut.Id, sut.UserId);
        }

        public IdentityUser MakeSut(string userName = "someuser@email.com")
        {
            return new IdentityUser(userName);
        }
    }
}
