// MIT License Copyright 2014 (c) David Melendez. All rights reserved. See License.txt in the project root for license information.

using ElCamino.AspNet.Identity.Dynamo.Model;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElCamino.AspNet.Identity.Dynamo.Configuration
{
    public class IdentityConfigurationSection : ConfigurationSection
    {
        public static string Name
        {
            get
            {
                return "elcaminoIdentityDynamoDBConfiguration";
            }
        }

        public static IdentityConfiguration GetCurrent()
        {           
            IdentityConfigurationSection section = ConfigurationManager.GetSection(Name) as IdentityConfigurationSection;
            //Add this code when appSettings configuration are phased out.
            //if (section == null)
            //    throw new ConfigurationErrorsException(string.Format("Configuration Section Not Found: {0}", Name));

            if (section != null)
            {
                IdentityConfiguration ic = new IdentityConfiguration();
                ic.ServiceURL = section.ServiceURL;
                ic.AuthenticationRegion = section.AuthenticationRegion;
                if (section.BufferSize.HasValue)
                {
                    ic.BufferSize = section.BufferSize.Value;
                }
                if (section.ConnectionLimit.HasValue)
                {
                    ic.ConnectionLimit = section.ConnectionLimit.Value;
                }
                ic.LogMetrics = section.LogMetrics;
                ic.LogResponse = section.LogResponse;
                ic.TablePrefix = section.TablePrefix;
                return ic;
            }
            return null;
        }

        [ConfigurationProperty("tablePrefix", DefaultValue = "", IsRequired = false)]
        public string TablePrefix
        {
            get
            {
                return (string)this["tablePrefix"];
            }
            set
            {
                this["tablePrefix"] = value;
            }
        }

        [ConfigurationProperty("serviceURL", DefaultValue = "", IsRequired = false)]
        public string ServiceURL
        {
            get
            {
                return (string)this["serviceURL"];
            }
            set
            {
                this["serviceURL"] = value;
            }
        }

        [ConfigurationProperty("authenticationRegion", DefaultValue = "", IsRequired = false)]
        public string AuthenticationRegion
        {
            get
            {
                return (string)this["authenticationRegion"];
            }
            set
            {
                this["authenticationRegion"] = value;
            }
        }

        [ConfigurationProperty("bufferSize", DefaultValue = null, IsRequired = false)]
        public int? BufferSize
        {
            get
            {
                return (int?)this["bufferSize"];
            }
            set
            {
                this["bufferSize"] = value;
            }
        }

        [ConfigurationProperty("connectionLimit", DefaultValue = null, IsRequired = false)]
        public int? ConnectionLimit
        {
            get
            {
                return (int?)this["connectionLimit"];
            }
            set
            {
                this["connectionLimit"] = value;
            }
        }

        [ConfigurationProperty("logMetrics", DefaultValue = false, IsRequired = false)]
        public bool LogMetrics
        {
            get
            {
                return (bool)this["logMetrics"];
            }
            set
            {
                this["logMetrics"] = value;
            }
        }

        [ConfigurationProperty("logResponse", DefaultValue = false, IsRequired = false)]
        public bool LogResponse
        {
            get
            {
                return (bool)this["logResponse"];
            }
            set
            {
                this["logResponse"] = value;
            }
        }

    }
}
