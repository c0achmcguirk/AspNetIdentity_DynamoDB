// MIT License Copyright 2014 (c) David Melendez. All rights reserved. See License.txt in the project root for license information.
using System;

namespace ElCamino.AspNet.Identity.Dynamo.Tests.HelperTests
{
    [TestClass]
    public class KeyHelperTests
    {
        [TestMethod]
        [TestCategory("Identity.Dynamo.Helper.KeyHelper")]
        public void Escape()
        {
            string url = @"https://www.msn.com/hello/index.aspx?parm1=2341341%2345234598!@#$%^&*()&param3=(^%$#$%^&HJKKK}P{}|:\";
            string escaped = Helpers.KeyHelper.EscapeKey(url);
            Assert.IsFalse(escaped.Contains(Helpers.KeyHelper.ReplaceIllegalChar), "Contains illegal char for row or partition key");

            Assert.IsNull(Helpers.KeyHelper.EscapeKey(string.Empty), "Escape didn't return null with an empty string.");
        }
    }
}
