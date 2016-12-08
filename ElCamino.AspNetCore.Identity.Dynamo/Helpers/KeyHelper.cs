// MIT License Copyright 2014 (c) David Melendez. All rights reserved. See License.txt in the project root for license information.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

#if net45
namespace ElCamino.AspNet.Identity.Dynamo.Helpers
#else
namespace ElCamino.AspNetCore.Identity.Dynamo.Helpers
#endif
{
    public static class KeyHelper
    {
        public const string ReplaceIllegalChar = "%";
        public const string NewCharForIllegalChar = "_";
        public static string EscapeKey(string keyUnsafe)
        {
            if (!string.IsNullOrWhiteSpace(keyUnsafe))
            {
                return System.Uri.EscapeDataString(keyUnsafe).Replace(ReplaceIllegalChar, NewCharForIllegalChar).ToUpper();
            }
            return null;
        }

        public static string GenerateRowKeyUserLoginInfo(this UserLoginInfo info)
        {
            return GenerateRowKeyProviderKey(info);
        }

        public static string GenerateRowKeyProviderKey(UserLoginInfo info)
        {
            string strTemp = string.Format("{0}_{1}", KeyHelper.EscapeKey(info.LoginProvider),
                KeyHelper.EscapeKey(info.ProviderKey));
            return string.Format(Constants.RowKeyConstants.FormatterIdentityUserLogin, strTemp);
        }

        public static string GenerateRowKeyIdentityUserRole(string plainRoleName)
        {
            return string.Format(Constants.RowKeyConstants.FormatterIdentityUserRole,
                KeyHelper.EscapeKey(plainRoleName));
        }

        public static string GenerateRowKeyIdentityRole(string plainRoleName)
        {
            return KeyHelper.EscapeKey(plainRoleName);
        }

        public static string GenerateRowKeyIdentityUserClaim(string claimType, string claimValue)
        {
            string strTemp = string.Format("{0}_{1}", KeyHelper.EscapeKey(claimType), KeyHelper.EscapeKey(claimValue));
            return string.Format(Constants.RowKeyConstants.FormatterIdentityUserClaim, strTemp);
        }
    }
}
