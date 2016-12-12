using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ElCamino.AspNetCore.Identity.Dynamo.Helpers;
using Microsoft.AspNetCore.Identity;
using NUnit.Framework;

namespace ElCamino.AspNetCore.Identity.UnitTests.Helpers
{
    [TestFixture]
    public class KeyHelperTests
    {
        [TestCase("aaa%aaa", "AAA_25AAA")]
        [TestCase("bbb:bbb", "BBB_3ABBB")]
        [TestCase("ccc/ccc", "CCC_2FCCC")]
        [TestCase("ddd?ddd", "DDD_3FDDD")]
        [TestCase("eee=eee", "EEE_3DEEE")]
        [TestCase("fff!fff", "FFF!FFF")]
        [TestCase("!@#$%^&*(){}()|:", "!_40_23_24_25_5E_26*()_7B_7D()_7C_3A")]
        public void Escape_WithIllegalCharacter_ReplacesWithUnderscore(string input, string expected)
        {
            var ILLEGAL_CHAR = "%";
            var result = KeyHelper.EscapeKey(input);
            StringAssert.DoesNotContain(ILLEGAL_CHAR, result);
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void Escape_Null_Null()
        {
            var result = KeyHelper.EscapeKey(null);
            Assert.IsNull(result);
        }

        [Test]
        public void GenerateRowKeyProviderKey_LoginProviderAndProviderKeyEntered_PutsBoth()
        {
            var loginProvider = "SomeLoginProvider";
            var providerKey = "SomeProviderKey";
            var displayName = "NoMatter";
            var info = new UserLoginInfo(loginProvider, providerKey, displayName);
            var result = KeyHelper.GenerateRowKeyProviderKey(info);
            Assert.AreEqual("L_SOMELOGINPROVIDER_SOMEPROVIDERKEY", result);
        }


        [Test]
        public void GenerateRowKeyUserLoginInfo_ULIContainsLoginProviderAndProviderKey_PutsBoth()
        {
            var loginProvider = "SomeLoginProvider";
            var providerKey = "SomeProviderKey";
            var displayName = "NoMatter";
            var info = new UserLoginInfo(loginProvider, providerKey, displayName);
            var result = info.GenerateRowKeyUserLoginInfo();
            Assert.AreEqual("L_SOMELOGINPROVIDER_SOMEPROVIDERKEY", result);
        }


        [Test]
        public void GenerateRowKeyIdentitytUserRole_RoleName_ContainsRoleName()
        {
            var roleName = "SomeRoleName";
            var result = KeyHelper.GenerateRowKeyIdentityUserRole(roleName);
            Assert.AreEqual("R_SOMEROLENAME", result);
        }

        [Test]
        public void GenerateRowKeyIdentityRole_RoleName_ReturnsRoleName()
        {
            var roleName = "SomeRoleName";
            var result = KeyHelper.GenerateRowKeyIdentityRole(roleName);
            Assert.AreEqual("SOMEROLENAME", result);
        }

        [Test]
        public void GenerateRowKeyIdentityUserClaim_ClaimTypeAClaimValueB_AunderscoreB()
        {
            var claimType = "SomeClaim";
            var claimValue = "SomeValue";
            var result = KeyHelper.GenerateRowKeyIdentityUserClaim(claimType, claimValue);
            Assert.AreEqual("C_SOMECLAIM_SOMEVALUE", result);
        }
    }
}
