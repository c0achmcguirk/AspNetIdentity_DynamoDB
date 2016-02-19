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
    public class IdentityUser : IdentityUser<string, IdentityUserLogin, IdentityUserRole, IdentityUserClaim>
        , IUser
        , IUser<string>
        , IGenerateKeys
    {
        public IdentityUser() { }

        public IdentityUser(string userName)
            : this()
        {
            this.UserName = userName;
        }

        /// <summary>
        /// Generates Row, Partition and Id keys.
        /// All are the same in this case
        /// </summary>
        public void GenerateKeys()
        {
            Id = PeekRowKey();
            UserId = Id;
        }

        /// <summary>
        /// Generates the RowKey without setting it on the object.
        /// In this case, just returns a new guid
        /// </summary>
        /// <returns></returns>
        public string PeekRowKey()
        {
            return Guid.NewGuid().ToString();
        }

        [DynamoDBGlobalSecondaryIndexHashKey(Constants.SecondaryIndexNames.UserNameIndex)]
        public override string UserName
        {
            get
            {
                return base.UserName;
            }
            set
            {
                if (!string.IsNullOrWhiteSpace(value))
                {
                    base.UserName = value.Trim();
                }
            }
        }
    }

    public class IdentityUser<TKey, TLogin, TRole, TClaim> :
        IUser<TKey>
        where TLogin : IdentityUserLogin<TKey>
        where TRole : IdentityUserRole<TKey>
        where TClaim : IdentityUserClaim<TKey>,
        IUserKeys<TKey>
    {
        public IdentityUser()
        {
            this.Claims = new List<TClaim>(100);
            this.Roles = new List<TRole>(100);
            this.Logins = new List<TLogin>(10);
        }

        public virtual int AccessFailedCount { get; set; }

        [DynamoDBIgnore]
        public ICollection<TClaim> Claims { get; private set; }

        [DynamoDBGlobalSecondaryIndexHashKey(Constants.SecondaryIndexNames.UserEmailIndex)]
        public virtual string Email { get; set; }

        public virtual bool EmailConfirmed { get; set; }

        [DynamoDBRangeKey]
        public virtual TKey Id { get; set; }

        [DynamoDBHashKey]
        public TKey UserId { get; set; }

        public virtual bool LockoutEnabled { get; set; }

        public virtual DateTime? LockoutEndDateUtc { get; set; }

        [DynamoDBIgnore]
        public ICollection<TLogin> Logins { get; private set; }

        public virtual string PasswordHash { get; set; }

        public virtual string PhoneNumber { get; set; }

        public virtual bool PhoneNumberConfirmed { get; set; }

        [DynamoDBIgnore]
        public ICollection<TRole> Roles { get; private set; }

        public virtual string SecurityStamp { get; set; }

        public virtual bool TwoFactorEnabled { get; set; }

        public virtual string UserName { get; set; }
    }

}
