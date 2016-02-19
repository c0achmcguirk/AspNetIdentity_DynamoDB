// MIT License Copyright 2014 (c) David Melendez. All rights reserved. See License.txt in the project root for license information.
using Amazon.DynamoDBv2.DataModel;
using ElCamino.AspNet.Identity.Dynamo.Helpers;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;

namespace ElCamino.AspNet.Identity.Dynamo.Model
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

    public class IdentityRole<TKey, TUserRole> :
         IRole<TKey> where TUserRole : IdentityUserRole<TKey>
    {

        public IdentityRole() : base()
        {
            this.Users = new List<TUserRole>();
        }

        [DynamoDBHashKey]
        public virtual TKey Id { get; set; }

        public string Name { get; set; }
     
        [DynamoDBIgnore]
        public ICollection<TUserRole> Users { get; private set; }
    }
}
