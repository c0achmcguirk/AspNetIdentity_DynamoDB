// MIT License Copyright 2014 (c) David Melendez. All rights reserved. See License.txt in the project root for license information.
using Amazon.DynamoDBv2.DataModel;
using ElCamino.AspNet.Identity.Dynamo.Helpers;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElCamino.AspNet.Identity.Dynamo.Model
{
    [DynamoDBTable(Constants.TableNames.UsersTable)]
    public class IdentityUserRole : IdentityUserRole<string>, IGenerateKeys
    {
        public IdentityUserRole() { }

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
            return KeyHelper.GenerateRowKeyIdentityUserRole(RoleName);
        }

    }


    public class IdentityUserRole<TKey> : IUserKeys<TKey>
    {

        [DynamoDBRangeKey]
        public TKey Id
        {
            get
            {
                return RoleId;
            }
            set
            {
                RoleId = value;
            }
        }

        [DynamoDBIgnore]
        public virtual TKey RoleId { get; set; }

        public string RoleName { get; set; }

        [DynamoDBHashKey]
        public virtual TKey UserId { get; set; }

        [DynamoDBGlobalSecondaryIndexHashKey(Constants.SecondaryIndexNames.UserNameIndex)]
        public string UserName { get; set; }

        [DynamoDBGlobalSecondaryIndexHashKey(Constants.SecondaryIndexNames.UserEmailIndex)]
        public string Email { get; set; }
    }

}
