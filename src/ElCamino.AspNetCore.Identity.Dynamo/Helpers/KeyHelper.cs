// MIT License Copyright 2014 (c) David Melendez. All rights reserved. See License.txt in the project root for license information.
using Microsoft.AspNetCore.Identity;

namespace ElCamino.AspNetCore.Identity.Dynamo.Helpers
{
    /// <summary>
    /// Provides helper methods for generating keys in the identity tables.
    /// </summary>
    public static class KeyHelper
    {
        private const string REPLACE_ILLEGAL_CHAR = "%";
        public const string NEW_CHAR_FOR_ILLEGAL_CHAR = "_";

        /// <summary>
        /// Escapes the key using System.Uri.EscapeDataString
        /// </summary>
        /// <param name="keyUnsafe"></param>
        /// <returns>A string that has been escaped and illegalr characters replaced.</returns>
        public static string EscapeKey(string keyUnsafe)
        {
            if (!string.IsNullOrWhiteSpace(keyUnsafe))
            {
                return System.Uri.EscapeDataString(keyUnsafe).Replace(REPLACE_ILLEGAL_CHAR, NEW_CHAR_FOR_ILLEGAL_CHAR).ToUpper();
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
