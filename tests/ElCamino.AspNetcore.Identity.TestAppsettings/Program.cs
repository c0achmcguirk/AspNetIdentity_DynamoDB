using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace ElCamino.AspNet.Identity.TestAppsettings
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = new ConfigurationBuilder();
            Console.WriteLine("Initial Config Sources: " + builder.Sources.Count());

            builder.AddInMemoryCollection(new Dictionary<string, string>
            {
                { "username", "Guest" }
            });

            Console.WriteLine("Added Memory Source. Sources: " + builder.Sources.Count());

            builder.SetBasePath(Directory.GetCurrentDirectory());
            builder.AddJsonFile("appsettings.json");
            Console.WriteLine("Added Command-line Source. Sources: " + builder.Sources.Count());
            var config = builder.Build();

            var username = config["username"];
            var prefix = config["ElCaminoIdentityDynamoDBConfiguration:TablePrefix"];
            var serviceUrl = config["ElCaminoIdentityDynamoDBConfiguration:ServiceUrl"];
            var authenticationRegion = config["ElCaminoIdentityDynamoDBConfiguration:AuthenticationRegion"];
            var bufferSize = config["ElCaminoIdentityDynamoDBConfiguration:BufferSize"];
            var connectionLimit = config["ElCaminoIdentityDynamoDBConfiguration:ConnectionLimit"];
            var logMetrics = config["ElCaminoIdentityDynamoDBConfiguration:ConnectionLimit"];
            var logResponse = config["ElCaminoIdentityDynamoDBConfiguration:LogResponse"];

            var section = config.GetSection("ElCaminoIdentityDynamoDBConfiguration");
            var aa = section["ConnectionLimit"];

            Console.WriteLine($"Table Prefix: {prefix}");
            Console.WriteLine($"Hello, {username}!");
        }
    }
}
