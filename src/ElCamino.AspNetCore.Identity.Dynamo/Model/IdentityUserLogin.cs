// MIT License Copyright 2014 (c) David Melendez. All rights reserved. See License.txt in the project root for license information.
using Amazon.DynamoDBv2.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ElCamino.AspNetCore.Identity.Dynamo.Helpers;

#if net45
namespace ElCamino.AspNet.Identity.Dynamo.Model
#else
namespace ElCamino.AspNetCore.Identity.Dynamo.Model
#endif
{
    [DynamoDBTable(Constants.TableNames.UsersTable)]
    public class IdentityUserLogin : IdentityUserLogin<string>, IGenerateKeys
    {
        public IdentityUserLogin() { }


        /// <summary>
        /// Generates Row and Id keys.
        /// Partition key is equal to the UserId
        /// </summary>
        public void GenerateKeys()
        {
            Id = PeekRowKey();
        }

        /// <summary>
        /// Generates the RowKey without setting it on the object.
        /// </summary>
        /// <returns></returns>
        public string PeekRowKey()
        {
            string strTemp = string.Format("{0}_{1}", KeyHelper.EscapeKey(LoginProvider), KeyHelper.EscapeKey(ProviderKey));
            return string.Format(Constants.RowKeyConstants.FormatterIdentityUserLogin, strTemp);
        }

    }

    public class IdentityUserLogin<TKey> : IUserKeys<TKey>
    {
        public virtual string LoginProvider { get; set; }

        public virtual string ProviderKey { get; set; }

        [DynamoDBRangeKey]
        public TKey Id { get; set; }

        [DynamoDBHashKey]
        public virtual TKey UserId { get; set; }

        [DynamoDBGlobalSecondaryIndexHashKey(Constants.SecondaryIndexNames.UserNameIndex)]
        public string UserName { get; set; }

        [DynamoDBGlobalSecondaryIndexHashKey(Constants.SecondaryIndexNames.UserEmailIndex)]
        public string Email { get; set; }

    }

}
