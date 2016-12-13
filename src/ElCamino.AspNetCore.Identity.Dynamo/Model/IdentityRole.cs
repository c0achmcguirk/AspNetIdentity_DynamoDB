// MIT License Copyright 2014 (c) David Melendez. All rights reserved. See License.txt in the project root for license information.
using Amazon.DynamoDBv2.DataModel;
using ElCamino.AspNetCore.Identity.Dynamo.Helpers;
using System.Collections.Generic;

namespace ElCamino.AspNetCore.Identity.Dynamo.Model
{

    [DynamoDBTable(Constants.TableNames.RolesTable)]    
    public class IdentityRole : IdentityRole<string, IdentityUserRole>, IGenerateKeys
    {
        public IdentityRole() : base() { }

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
            return KeyHelper.GenerateRowKeyIdentityRole(Name);
        }

        public IdentityRole(string roleName)
            : this()
        {
            base.Name = roleName;
        }

       
    }

    public class IdentityRole<TKey, TUserRole>
#if net45
        : IRole<TKey> where TUserRole : IdentityUserRole<TKey>
#endif
    {

        public IdentityRole() : base()
        {
            this.Users = new List<TUserRole>();
        }

        [DynamoDBHashKey]
        public virtual TKey Id { get; set; }

        public string Name { get; set; }

#if !net45
        public string NormalizedName { get; set; }
#endif

        [DynamoDBIgnore]
        public ICollection<TUserRole> Users { get; private set; }
    }
}
