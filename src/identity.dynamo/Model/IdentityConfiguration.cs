// MIT License Copyright 2014 (c) David Melendez. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Amazon.DynamoDBv2;

namespace ElCamino.AspNet.Identity.Dynamo.Model
{
    [JsonObject("identityConfiguration")]
    public class IdentityConfiguration : AmazonDynamoDBConfig
    {
        public IdentityConfiguration()  { }

        public IdentityConfiguration(AmazonDynamoDBConfig config) 
        {
            var aConfigProperties = typeof(AmazonDynamoDBConfig).GetProperties();

            foreach (var aMember in aConfigProperties)
            {
                var iConfigProperty = typeof(IdentityConfiguration).GetProperty(aMember.Name);
                if (iConfigProperty.CanWrite)
                {
                    var objvalue = aMember.GetValue(config);
                    if (objvalue != null)
                    {
                        iConfigProperty.SetValue(this, objvalue);
                    }
                }
            }
        }

        [JsonProperty("tablePrefix")]
        public string TablePrefix { get; set; }

    }
}
