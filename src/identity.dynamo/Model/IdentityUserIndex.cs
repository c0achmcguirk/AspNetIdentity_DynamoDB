// MIT License Copyright 2014 (c) David Melendez. All rights reserved. See License.txt in the project root for license information.
using Amazon.DynamoDBv2.DataModel;
using ElCamino.AspNet.Identity.Dynamo.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElCamino.AspNet.Identity.Dynamo.Model
{
    public class IdentityUserIndex : IdentityUserIndex<string>
    {
        public IdentityUserIndex() { }
    }

    [DynamoDBTable(Constants.TableNames.IndexTable)]
    public class IdentityUserIndex<TKey> 
    {
        [DynamoDBHashKey]
        public TKey Id { get; set; }

        public virtual TKey UserId { get; set; }

    }

}
