using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ElCamino.AspNet.Identity.Dynamo.Configuration;
using NUnit.Framework;

namespace ElCamino.AspNetCore.Identity.UnitTests.Configuration
{
    [TestFixture]
    public class IdentityDynamoOptionsTests
    {
        [Test]
        public void Ctor_Always_Creates()
        {
            var sut = MakeSut();
            Assert.IsNotNull(sut);
            Assert.IsNull(sut.ServiceUrl);
        }

        private IdentityDynamoOptions MakeSut()
        {
            return new IdentityDynamoOptions(); 
        }
    }
}
