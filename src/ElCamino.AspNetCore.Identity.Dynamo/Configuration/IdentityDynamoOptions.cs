using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElCamino.AspNet.Identity.Dynamo.Configuration
{
    /// <summary>
    /// The class that will be used to configure this library using appsettings.json
    /// </summary>
    public class IdentityDynamoOptions
    {
        public string TablePrefix { get; set; }
        public string ServiceUrl { get; set; }
        public string AuthenticationRegion { get; set; }
        public int BufferSize { get; set; }
        public int ConnectionLimit { get; set; }
        public bool? LogMetrics { get; set; }
        public bool? LogResponse { get; set; }
    }
}
