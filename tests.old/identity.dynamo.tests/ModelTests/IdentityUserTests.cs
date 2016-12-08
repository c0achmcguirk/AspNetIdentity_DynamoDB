// MIT License Copyright 2014 (c) David Melendez. All rights reserved. See License.txt in the project root for license information.
using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.AspNet.Identity;
using ElCamino.AspNet.Identity.Dynamo.Helpers;
using ElCamino.AspNet.Identity.Dynamo.Model;

namespace ElCamino.AspNet.Identity.Dynamo.Tests.ModelTests
{
    [TestClass]
    public class IdentityUserTests
    {
        [TestMethod]
        [TestCategory("Identity.Dynamo.Model")]
        public void IdentityUserCtors()
        {

            Assert.IsNotNull(new IdentityUser(Guid.NewGuid().ToString()), "Identity User is null.");

        }
    }
}
